using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using MuseumServer.Models;

namespace MuseumServer.Services
{
    public enum BackupKind
    {
        Full,
        Differential
    }

    public class BackupService
    {
        private readonly IDbContextFactory<MuseumContext> _dbFactory;
        private readonly IWebHostEnvironment _env;
        private readonly LoggingService _logging;
        private readonly string _databaseName;
        private readonly string _masterConnectionString;
        private readonly string _backupsRoot;
        private readonly string _databaseBackupsRoot;
        private readonly string _mediaBackupsRoot;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private static readonly Regex DatabaseFileRegex =
            new(@"^backup_(\d{8}_\d{6})_(full|diff)\.bak$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MediaFileRegex =
            new(@"^backup_(\d{8}_\d{6})_media\.zip$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public BackupService(
            IDbContextFactory<MuseumContext> dbFactory,
            IConfiguration configuration,
            IWebHostEnvironment env,
            LoggingService logging)
        {
            _dbFactory = dbFactory;
            _env = env;
            _logging = logging;

            var cs = configuration.GetConnectionString("MuseumDb");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("Connection string 'MuseumDb' is missing.");

            var builder = new SqlConnectionStringBuilder(cs);
            _databaseName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "MuseumDB" : builder.InitialCatalog;

            builder.InitialCatalog = "master";
            _masterConnectionString = builder.ConnectionString;

            _backupsRoot = Path.Combine(_env.ContentRootPath, "Backups");
            _databaseBackupsRoot = Path.Combine(_backupsRoot, "Database");
            _mediaBackupsRoot = Path.Combine(_backupsRoot, "Media");

            Directory.CreateDirectory(_databaseBackupsRoot);
            Directory.CreateDirectory(_mediaBackupsRoot);
        }

        public async Task<BackupSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            using var db = _dbFactory.CreateDbContext();

            var info = await db.MuseumInfo.FirstOrDefaultAsync(cancellationToken);
            if (info == null)
            {
                return new BackupSettingsDto();
            }

            return new BackupSettingsDto
            {
                BackupFullDayOfWeek = info.BackupFullDayOfWeek,
                BackupFullTime = info.BackupFullTime,
                BackupDifferentialTime = info.BackupDifferentialTime,
                BackupRetentionDays = info.BackupRetentionDays
            };
        }

        public async Task<BackupSettingsDto> UpdateSettingsAsync(
            UpdateBackupSettingsRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.BackupRetentionDays < 1)
                request.BackupRetentionDays = 1;

            using var db = _dbFactory.CreateDbContext();

            var info = await db.MuseumInfo.FirstOrDefaultAsync(cancellationToken);
            if (info == null)
                throw new InvalidOperationException("MuseumInfo row was not found.");

            info.BackupFullDayOfWeek = request.BackupFullDayOfWeek;
            info.BackupFullTime = request.BackupFullTime;
            info.BackupDifferentialTime = request.BackupDifferentialTime;
            info.BackupRetentionDays = request.BackupRetentionDays;

            await db.SaveChangesAsync(cancellationToken);

            return new BackupSettingsDto
            {
                BackupFullDayOfWeek = info.BackupFullDayOfWeek,
                BackupFullTime = info.BackupFullTime,
                BackupDifferentialTime = info.BackupDifferentialTime,
                BackupRetentionDays = info.BackupRetentionDays
            };
        }

        public async Task<List<BackupPointDto>> GetBackupsAsync(CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, BackupPointDto>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in Directory.EnumerateFiles(_databaseBackupsRoot, "*.bak"))
            {
                if (!TryParseDatabaseFile(file, out var baseName, out var kind, out var createdAt))
                    continue;

                var fi = new FileInfo(file);

                result[baseName] = new BackupPointDto
                {
                    BaseName = baseName,
                    CreatedAt = createdAt,
                    BackupType = kind == BackupKind.Full ? "Full" : "Differential",
                    DatabaseFileName = Path.GetFileName(file),
                    HasDatabaseBackup = true,
                    DatabaseSizeBytes = fi.Length
                };
            }

            foreach (var point in result.Values)
            {
                var mediaPath = await ResolveMediaArchiveForRestoreAsync(point.BaseName, point.CreatedAt, cancellationToken);
                if (!string.IsNullOrWhiteSpace(mediaPath) && File.Exists(mediaPath))
                {
                    var fi = new FileInfo(mediaPath);
                    point.MediaFileName = Path.GetFileName(mediaPath);
                    point.MediaSizeBytes = fi.Length;
                    point.HasMediaArchive = true;
                }
            }

            return result.Values
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public async Task<BackupOperationResultDto> CreateFullBackupAsync(
            string userType,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var now = DateTime.Now;
                var baseName = BuildBaseName(now);
                var dbPath = Path.Combine(_databaseBackupsRoot, $"{baseName}_full.bak");
                var mediaPath = Path.Combine(_mediaBackupsRoot, $"{baseName}_media.zip");

                await ExecuteBackupAsync(
                    $"BACKUP DATABASE [{_databaseName}] TO DISK = N'{EscapeSqlLiteral(dbPath)}' WITH INIT, STATS = 10;",
                    cancellationToken);

                var warning = default(string);
                var mediaCreated = false;

                try
                {
                    await CreateMediaArchiveAsync(mediaPath, cancellationToken);
                    mediaCreated = true;
                }
                catch (Exception ex)
                {
                    warning = "MEDIA_ARCHIVE_FAILED";
                    await _logging.LogAsync(userType, "BackupMediaArchiveFailed", "Backup", ex.Message);
                }

                await CleanupOldBackupsAsync(cancellationToken);

                await _logging.LogAsync(userType, "CreateFullBackup", "Backup", baseName);

                return new BackupOperationResultDto
                {
                    BaseName = baseName,
                    MediaArchiveCreated = mediaCreated,
                    Warning = warning
                };
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<BackupOperationResultDto> CreateDifferentialBackupAsync(
            string userType,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                if (!await HasAnyFullBackupAsync(cancellationToken))
                    throw new InvalidOperationException("FULL_BACKUP_REQUIRED");

                var now = DateTime.Now;
                var baseName = BuildBaseName(now);
                var dbPath = Path.Combine(_databaseBackupsRoot, $"{baseName}_diff.bak");

                await ExecuteBackupAsync(
                    $"BACKUP DATABASE [{_databaseName}] TO DISK = N'{EscapeSqlLiteral(dbPath)}' WITH DIFFERENTIAL, INIT, STATS = 10;",
                    cancellationToken);

                await CleanupOldBackupsAsync(cancellationToken);

                await _logging.LogAsync(userType, "CreateDifferentialBackup", "Backup", baseName);

                return new BackupOperationResultDto
                {
                    BaseName = baseName,
                    MediaArchiveCreated = false
                };
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task RestoreBackupAsync(
            string userType,
            string baseName,
            bool restoreMedia,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var point = await GetBackupPointAsync(baseName, cancellationToken);
                if (point == null)
                    throw new FileNotFoundException("BACKUP_NOT_FOUND");

                var dbPath = GetDatabaseFilePath(baseName, point.BackupType);
                if (!File.Exists(dbPath))
                    throw new FileNotFoundException("DATABASE_BACKUP_NOT_FOUND");

                string? fullDbPathForChain = null;

                if (point.BackupType.Equals("Differential", StringComparison.OrdinalIgnoreCase))
                {
                    var fullBaseName = FindFullBackupBaseNameBefore(point.CreatedAt);
                    if (fullBaseName == null)
                        throw new FileNotFoundException("BASE_FULL_BACKUP_NOT_FOUND");

                    fullDbPathForChain = Path.Combine(_databaseBackupsRoot, $"{fullBaseName}_full.bak");
                    if (!File.Exists(fullDbPathForChain))
                        throw new FileNotFoundException("BASE_FULL_BACKUP_NOT_FOUND");
                }

                await using (var connection = new SqlConnection(_masterConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    await ExecuteNonQueryAsync(connection,
                        $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;",
                        cancellationToken);

                    try
                    {
                        if (fullDbPathForChain != null)
                        {
                            // Шаг 1: базовый полный бэкап, без восстановления
                            await ExecuteNonQueryAsync(connection,
                                $"RESTORE DATABASE [{_databaseName}] FROM DISK = N'{EscapeSqlLiteral(fullDbPathForChain)}' WITH REPLACE, NORECOVERY, STATS = 10;",
                                cancellationToken);

                            // Шаг 2: разностный поверх него, уже с восстановлением
                            await ExecuteNonQueryAsync(connection,
                                $"RESTORE DATABASE [{_databaseName}] FROM DISK = N'{EscapeSqlLiteral(dbPath)}' WITH RECOVERY, STATS = 10;",
                                cancellationToken);
                        }
                        else
                        {
                            await ExecuteNonQueryAsync(connection,
                                $"RESTORE DATABASE [{_databaseName}] FROM DISK = N'{EscapeSqlLiteral(dbPath)}' WITH REPLACE, RECOVERY, STATS = 10;",
                                cancellationToken);
                        }
                    }
                    finally
                    {
                        await ExecuteNonQueryAsync(connection,
                            $"ALTER DATABASE [{_databaseName}] SET MULTI_USER;",
                            CancellationToken.None);
                    }
                }

                if (restoreMedia)
                {
                    var mediaPath = await ResolveMediaArchiveForRestoreAsync(baseName, point.CreatedAt, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(mediaPath) && File.Exists(mediaPath))
                    {
                        await RestoreMediaArchiveAsync(mediaPath, cancellationToken);
                    }
                }

                await _logging.LogAsync(userType, "RestoreBackup", "Backup", baseName);
            }
            finally
            {
                _gate.Release();
            }
        }

        // Ищет ближайший ПОЛНЫЙ бэкап, созданный не позже targetTime
        private string? FindFullBackupBaseNameBefore(DateTime targetTime)
        {
            string? candidate = null;
            var candidateTime = DateTime.MinValue;

            foreach (var file in Directory.EnumerateFiles(_databaseBackupsRoot, "*_full.bak"))
            {
                if (!TryParseDatabaseFile(file, out var fileBaseName, out var kind, out var fileTime))
                    continue;

                if (kind != BackupKind.Full)
                    continue;

                if (fileTime <= targetTime && fileTime >= candidateTime)
                {
                    candidate = fileBaseName;
                    candidateTime = fileTime;
                }
            }

            return candidate;
        }


        public async Task DeleteBackupAsync(
            string userType,
            string baseName,
            CancellationToken cancellationToken = default)
        {
            await _gate.WaitAsync(cancellationToken);
            try
            {
                var point = await GetBackupPointAsync(baseName, cancellationToken);
                if (point == null)
                    return;

                var dbPath = GetDatabaseFilePath(baseName, point.BackupType);
                if (File.Exists(dbPath))
                    File.Delete(dbPath);

                var mediaPath = Path.Combine(_mediaBackupsRoot, $"{baseName}_media.zip");
                if (File.Exists(mediaPath))
                    File.Delete(mediaPath);

                await _logging.LogAsync(userType, "DeleteBackup", "Backup", baseName);
            }
            finally
            {
                _gate.Release();
            }
        }

        public string? GetDatabaseBackupPath(string baseName)
        {
            if (!TryValidateBaseName(baseName))
                return null;

            var fullPath = Path.Combine(_databaseBackupsRoot, $"{baseName}_full.bak");
            if (File.Exists(fullPath))
                return fullPath;

            var diffPath = Path.Combine(_databaseBackupsRoot, $"{baseName}_diff.bak");
            if (File.Exists(diffPath))
                return diffPath;

            return null;
        }

        public string? GetMediaArchivePath(string baseName)
        {
            if (!TryValidateBaseName(baseName))
                return null;

            var path = Path.Combine(_mediaBackupsRoot, $"{baseName}_media.zip");
            return File.Exists(path) ? path : null;
        }

        public async Task RunScheduledTickAsync(CancellationToken cancellationToken = default)
        {
            var settings = await GetSettingsAsync(cancellationToken);
            var now = DateTime.Now;

            var isFullDay = (int)now.DayOfWeek == settings.BackupFullDayOfWeek;
            var fullScheduledAt = now.Date + settings.BackupFullTime;
            var diffScheduledAt = now.Date + settings.BackupDifferentialTime;

            if (isFullDay && now >= fullScheduledAt)
            {
                if (!TodayFullBackupExists(now))
                {
                    await CreateFullBackupAsync("system", cancellationToken);
                }

                return;
            }

            if (now >= diffScheduledAt)
            {
                if (!await HasAnyFullBackupAsync(cancellationToken))
                {
                    if (!TodayFullBackupExists(now))
                    {
                        await CreateFullBackupAsync("system", cancellationToken);
                    }

                    return;
                }

                if (!TodayDifferentialBackupExists(now))
                {
                    await CreateDifferentialBackupAsync("system", cancellationToken);
                }
            }
        }

        private bool TodayFullBackupExists(DateTime now)
            => Directory.EnumerateFiles(_databaseBackupsRoot, $"backup_{now:yyyyMMdd}_*_full.bak").Any();

        private bool TodayDifferentialBackupExists(DateTime now)
            => Directory.EnumerateFiles(_databaseBackupsRoot, $"backup_{now:yyyyMMdd}_*_diff.bak").Any();
        private async Task<bool> HasAnyFullBackupAsync(CancellationToken cancellationToken)
        {
            await foreach (var _ in GetFullBackupFilesAsync(cancellationToken))
            {
                return true;
            }

            return false;
        }

        private async IAsyncEnumerable<string> GetFullBackupFilesAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var file in Directory.EnumerateFiles(_databaseBackupsRoot, "*_full.bak"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return file;
                await Task.Yield();
            }
        }

        private async Task CleanupOldBackupsAsync(CancellationToken cancellationToken)
        {
            var settings = await GetSettingsAsync(cancellationToken);
            var cutoff = DateTime.Now.AddDays(-settings.BackupRetentionDays);

            var allDbFiles = Directory.EnumerateFiles(_databaseBackupsRoot, "*.bak").ToList();

            foreach (var file in allDbFiles)
            {
                if (!TryParseDatabaseFile(file, out var baseName, out var kind, out var createdAt))
                    continue;

                if (createdAt >= cutoff)
                    continue;

                if (kind == BackupKind.Full)
                {
                    var hasDependentDiff = allDbFiles.Any(f =>
                    {
                        if (!TryParseDatabaseFile(f, out _, out var otherKind, out var otherCreatedAt))
                            return false;

                        return otherKind == BackupKind.Differential
                            && otherCreatedAt > createdAt
                            && otherCreatedAt >= cutoff;
                    });

                    if (hasDependentDiff)
                        continue; // ещё нужен зависимым разностным
                }

                var mediaPath = Path.Combine(_mediaBackupsRoot, $"{baseName}_media.zip");

                try
                {
                    if (File.Exists(file))
                        File.Delete(file);

                    if (File.Exists(mediaPath))
                        File.Delete(mediaPath);
                }
                catch
                {
                }
            }
        }

        private async Task<BackupPointDto?> GetBackupPointAsync(string baseName, CancellationToken cancellationToken)
        {
            if (!TryValidateBaseName(baseName))
                return null;

            var fullPath = Path.Combine(_databaseBackupsRoot, $"{baseName}_full.bak");
            if (File.Exists(fullPath))
            {
                var fi = new FileInfo(fullPath);
                return new BackupPointDto
                {
                    BaseName = baseName,
                    CreatedAt = GetTimestampFromBaseName(baseName),
                    BackupType = "Full",
                    DatabaseFileName = Path.GetFileName(fullPath),
                    HasDatabaseBackup = true,
                    DatabaseSizeBytes = fi.Length
                };
            }

            var diffPath = Path.Combine(_databaseBackupsRoot, $"{baseName}_diff.bak");
            if (File.Exists(diffPath))
            {
                var fi = new FileInfo(diffPath);
                return new BackupPointDto
                {
                    BaseName = baseName,
                    CreatedAt = GetTimestampFromBaseName(baseName),
                    BackupType = "Differential",
                    DatabaseFileName = Path.GetFileName(diffPath),
                    HasDatabaseBackup = true,
                    DatabaseSizeBytes = fi.Length
                };
            }

            return null;
        }

        private async Task<string?> ResolveMediaArchiveForRestoreAsync(
            string baseName,
            DateTime pointCreatedAt,
            CancellationToken cancellationToken)
        {
            var exactPath = Path.Combine(_mediaBackupsRoot, $"{baseName}_media.zip");
            if (File.Exists(exactPath))
                return exactPath;

            string? candidate = null;
            DateTime candidateTime = DateTime.MinValue;

            foreach (var file in Directory.EnumerateFiles(_mediaBackupsRoot, "*.zip"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TryParseMediaFile(file, out var fileBaseName, out var fileTime))
                    continue;

                if (fileTime <= pointCreatedAt && fileTime >= candidateTime)
                {
                    candidate = file;
                    candidateTime = fileTime;
                }
            }

            return candidate;
        }

        private async Task CreateMediaArchiveAsync(string zipPath, CancellationToken cancellationToken)
        {
            var wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");

            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
                using var emptyArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
                return;
            }

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

            foreach (var file in Directory.EnumerateFiles(wwwrootPath, "*", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(wwwrootPath, file);
                var entry = archive.CreateEntry(relativePath, CompressionLevel.Optimal);

                await using var entryStream = entry.Open();
                await using var sourceStream = File.OpenRead(file);
                await sourceStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        private async Task RestoreMediaArchiveAsync(string zipPath, CancellationToken cancellationToken)
        {
            var wwwrootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
            Directory.CreateDirectory(wwwrootPath);

            ClearDirectory(wwwrootPath);

            ZipFile.ExtractToDirectory(zipPath, wwwrootPath, overwriteFiles: true);
            await Task.CompletedTask;
        }

        private async Task ExecuteBackupAsync(string sql, CancellationToken cancellationToken)
        {
            await using var connection = new SqlConnection(_masterConnectionString);
            await connection.OpenAsync(cancellationToken);
            await ExecuteNonQueryAsync(connection, sql, cancellationToken);
        }

        private static async Task ExecuteNonQueryAsync(SqlConnection connection, string sql, CancellationToken cancellationToken)
        {
            await using var command = connection.CreateCommand();
            command.CommandTimeout = 0;
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private static void ClearDirectory(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
            {
                Directory.Delete(directory, true);
            }

            foreach (var file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }
        }

        private static string BuildBaseName(DateTime now)
            => $"backup_{now:yyyyMMdd_HHmmss}";

        private string GetDatabaseFilePath(string baseName, string backupType)
        {
            return backupType.Equals("Full", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(_databaseBackupsRoot, $"{baseName}_full.bak")
                : Path.Combine(_databaseBackupsRoot, $"{baseName}_diff.bak");
        }

        private static bool TryValidateBaseName(string baseName)
        {
            return Regex.IsMatch(baseName, @"^backup_\d{8}_\d{6}$", RegexOptions.IgnoreCase);
        }

        private static bool TryParseDatabaseFile(string filePath, out string baseName, out BackupKind kind, out DateTime createdAt)
        {
            baseName = string.Empty;
            kind = BackupKind.Full;
            createdAt = default;

            var fileName = Path.GetFileName(filePath);
            var match = DatabaseFileRegex.Match(fileName);
            if (!match.Success)
                return false;

            var timestamp = match.Groups[1].Value;
            baseName = $"backup_{timestamp}";
            kind = match.Groups[2].Value.Equals("full", StringComparison.OrdinalIgnoreCase)
                ? BackupKind.Full
                : BackupKind.Differential;

            createdAt = DateTime.ParseExact(timestamp, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            return true;
        }

        private static bool TryParseMediaFile(string filePath, out string baseName, out DateTime createdAt)
        {
            baseName = string.Empty;
            createdAt = default;

            var fileName = Path.GetFileName(filePath);
            var match = MediaFileRegex.Match(fileName);
            if (!match.Success)
                return false;

            var timestamp = match.Groups[1].Value;
            baseName = $"backup_{timestamp}";
            createdAt = DateTime.ParseExact(timestamp, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            return true;
        }

        private static string GetBaseNameFromDatabaseFileName(string fileName)
        {
            var match = DatabaseFileRegex.Match(fileName);
            if (!match.Success)
                return string.Empty;

            return $"backup_{match.Groups[1].Value}";
        }

        private static DateTime GetTimestampFromBaseName(string baseName)
        {
            var timestamp = baseName.Replace("backup_", "");
            return DateTime.ParseExact(timestamp, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        }

        private static string EscapeSqlLiteral(string value)
            => value.Replace("'", "''");
    }
}