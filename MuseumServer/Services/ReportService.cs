using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using MuseumServer.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using DocModel = MuseumServer.Models.Document;
// Псевдонимы для разрешения конфликта имён
using PdfDocument = QuestPDF.Fluent.Document;

namespace MuseumServer.Services
{
    public class ReportService
    {
        private readonly MuseumContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportService(MuseumContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ==================== COUNT METHODS ====================

        public async Task<int> GetExhibitsCountAsync()
            => await _context.Exhibits.CountAsync();

        public async Task<int> GetDocumentsCountAsync()
            => await _context.Documents.CountAsync();

        public async Task<int> GetImagesCountAsync()
            => await _context.MediaFiles.Where(m => m.MediaType == "image").CountAsync();

        public async Task<int> GetVideosCountAsync()
            => await _context.MediaFiles.Where(m => m.MediaType == "video").CountAsync();

        public async Task<int> GetLogsCountAsync()
            => await _context.Logs.CountAsync();

        // ==================== EXHIBITS ====================

        public async Task<byte[]> GenerateExhibitsReportAsync(ExhibitsReportRequest request)
        {
            var query = _context.Exhibits
                .Include(e => e.Department)
                .OrderBy(e => e.ExhibitId)
                .AsQueryable();

            if (request.Limit is > 0)
                query = query.Take(request.Limit.Value);

            var exhibits = await query.ToListAsync();

            byte[]? LoadThumbnail(string? imagePath)
            {
                if (!request.IncludeImages || string.IsNullOrEmpty(imagePath))
                    return null;

                var thumbPath = Path.Combine(_env.WebRootPath, "exhibits", "thumbnails", imagePath);
                if (File.Exists(thumbPath))
                    return File.ReadAllBytes(thumbPath);

                var originalPath = Path.Combine(_env.WebRootPath, "exhibits", "images", imagePath);
                return File.Exists(originalPath) ? File.ReadAllBytes(originalPath) : null;
            }

            var pdf = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Отчёт по экспонатам").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().Text($"Всего в отчёте: {exhibits.Count}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        if (request.Compact)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(35);
                                    if (request.IncludeImages)
                                        columns.ConstantColumn(50);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("ID").Bold();
                                    if (request.IncludeImages)
                                        header.Cell().Text("");
                                    header.Cell().Text("Название").Bold();
                                    header.Cell().Text("Отдел").Bold();
                                    header.Cell().Text("Материалы").Bold();
                                    header.Cell().Text("Хранение").Bold();
                                });

                                foreach (var exhibit in exhibits)
                                {
                                    table.Cell().Text(exhibit.ExhibitId.ToString());

                                    if (request.IncludeImages)
                                    {
                                        var bytes = LoadThumbnail(exhibit.ImagePath);
                                        if (bytes != null)
                                            table.Cell().Height(40).Width(40).Image(bytes).FitArea();
                                        else
                                            table.Cell().Text("—");
                                    }

                                    table.Cell().Text(exhibit.Name);
                                    table.Cell().Text(exhibit.Department?.Name ?? "Без отдела");
                                    table.Cell().Text(exhibit.Materials ?? "—");
                                    table.Cell().Text(exhibit.IsPermanent ? "Постоянное" : "Временное");
                                }
                            });
                        }
                        else
                        {
                            foreach (var exhibit in exhibits)
                            {
                                col.Item().PaddingBottom(10)
                                    .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                    .Row(row =>
                                    {
                                        if (request.IncludeImages)
                                        {
                                            var bytes = LoadThumbnail(exhibit.ImagePath);
                                            if (bytes != null)
                                                row.ConstantItem(70).Height(70).Image(bytes).FitArea();
                                            else
                                                row.ConstantItem(70);
                                        }

                                        row.RelativeItem()
                                            .PaddingLeft(request.IncludeImages ? 10 : 0)
                                            .Column(info =>
                                            {
                                                info.Item().Text($"№{exhibit.ExhibitId} — {exhibit.Name}")
                                                    .Bold().FontSize(13);
                                                info.Item().Text($"Отдел: {exhibit.Department?.Name ?? "Без отдела"}");
                                                info.Item().Text($"Материалы: {exhibit.Materials ?? "—"}");
                                                info.Item().Text($"Тип хранения: {(exhibit.IsPermanent ? "Постоянное" : "Временное")}");
                                            });
                                    });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        // ==================== DOCUMENTS ====================

        public async Task<byte[]> GenerateDocumentsReportAsync(DocumentsReportRequest request)
        {
            var query = _context.Documents
                .Include(d => d.Department)
                .OrderBy(d => d.DocumentId)
                .AsQueryable();

            if (request.Limit is > 0)
                query = query.Take(request.Limit.Value);

            var documents = await query.ToListAsync();

            var pdf = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Отчёт по документам (статьям)").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().Text($"Всего в отчёте: {documents.Count}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        if (request.Compact)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("ID").Bold();
                                    header.Cell().Text("Название").Bold();
                                    header.Cell().Text("Отдел").Bold();
                                    header.Cell().Text("Тип файла").Bold();
                                });

                                foreach (var doc in documents)
                                {
                                    table.Cell().Text(doc.DocumentId.ToString());
                                    table.Cell().Text(doc.Title);
                                    table.Cell().Text(doc.Department?.Name ?? "Без отдела");
                                    table.Cell().Text(doc.FileType);
                                }
                            });
                        }
                        else
                        {
                            foreach (var doc in documents)
                            {
                                col.Item().PaddingBottom(8)
                                    .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                    .Column(info =>
                                    {
                                        info.Item().Text($"№{doc.DocumentId} — {doc.Title}")
                                            .Bold().FontSize(13);
                                        info.Item().Text($"Отдел: {doc.Department?.Name ?? "Без отдела"}");
                                        info.Item().Text($"Тип файла: {doc.FileType}");
                                        info.Item().Text($"Путь: {doc.FilePath}")
                                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                                    });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        // ==================== IMAGES ====================

        public async Task<byte[]> GenerateImagesReportAsync(ImagesReportRequest request)
        {
            var query = _context.MediaFiles
                .Where(m => m.MediaType == "image")
                .Include(m => m.Department)
                .OrderBy(m => m.MediaFileId)
                .AsQueryable();

            if (request.Limit is > 0)
                query = query.Take(request.Limit.Value);

            var images = await query.ToListAsync();

            byte[]? LoadImageBytes(string? filePath)
            {
                if (!request.IncludeImages || string.IsNullOrEmpty(filePath))
                    return null;

                var fullPath = Path.Combine(_env.WebRootPath, filePath);
                return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
            }

            var pdf = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Отчёт по изображениям").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().Text($"Всего в отчёте: {images.Count}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        if (request.Compact)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    if (request.IncludeImages)
                                        columns.ConstantColumn(50);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("ID").Bold();
                                    if (request.IncludeImages)
                                        header.Cell().Text("");
                                    header.Cell().Text("Название").Bold();
                                    header.Cell().Text("Отдел").Bold();
                                });

                                foreach (var img in images)
                                {
                                    table.Cell().Text(img.MediaFileId.ToString());

                                    if (request.IncludeImages)
                                    {
                                        var bytes = LoadImageBytes(img.FilePath);
                                        if (bytes != null)
                                            table.Cell().Height(40).Width(40).Image(bytes).FitArea();
                                        else
                                            table.Cell().Text("—");
                                    }

                                    table.Cell().Text(img.Title);
                                    table.Cell().Text(img.Department?.Name ?? "Без отдела");
                                }
                            });
                        }
                        else
                        {
                            foreach (var img in images)
                            {
                                col.Item().PaddingBottom(10)
                                    .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                    .Row(row =>
                                    {
                                        if (request.IncludeImages)
                                        {
                                            var bytes = LoadImageBytes(img.FilePath);
                                            if (bytes != null)
                                                row.ConstantItem(70).Height(70).Image(bytes).FitArea();
                                            else
                                                row.ConstantItem(70);
                                        }

                                        row.RelativeItem()
                                            .PaddingLeft(request.IncludeImages ? 10 : 0)
                                            .Column(info =>
                                            {
                                                info.Item().Text($"№{img.MediaFileId} — {img.Title}")
                                                    .Bold().FontSize(13);
                                                info.Item().Text($"Отдел: {img.Department?.Name ?? "Без отдела"}");
                                                if (!string.IsNullOrEmpty(img.Description))
                                                    info.Item().Text($"Описание: {img.Description}");
                                                info.Item().Text($"Путь: {img.FilePath}")
                                                    .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                                            });
                                    });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        // ==================== VIDEOS ====================

        public async Task<byte[]> GenerateVideosReportAsync(VideosReportRequest request)
        {
            var query = _context.MediaFiles
                .Where(m => m.MediaType == "video")
                .Include(m => m.Department)
                .OrderBy(m => m.MediaFileId)
                .AsQueryable();

            if (request.Limit is > 0)
                query = query.Take(request.Limit.Value);

            var videos = await query.ToListAsync();

            var pdf = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Отчёт по видео").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().Text($"Всего в отчёте: {videos.Count}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        if (request.Compact)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("ID").Bold();
                                    header.Cell().Text("Название").Bold();
                                    header.Cell().Text("Отдел").Bold();
                                });

                                foreach (var video in videos)
                                {
                                    table.Cell().Text(video.MediaFileId.ToString());
                                    table.Cell().Text(video.Title);
                                    table.Cell().Text(video.Department?.Name ?? "Без отдела");
                                }
                            });
                        }
                        else
                        {
                            foreach (var video in videos)
                            {
                                col.Item().PaddingBottom(8)
                                    .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                    .Column(info =>
                                    {
                                        info.Item().Text($"№{video.MediaFileId} — {video.Title}")
                                            .Bold().FontSize(13);
                                        info.Item().Text($"Отдел: {video.Department?.Name ?? "Без отдела"}");
                                        if (!string.IsNullOrEmpty(video.Description))
                                            info.Item().Text($"Описание: {video.Description}");
                                        info.Item().Text($"Путь: {video.FilePath}")
                                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                                    });
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        // ==================== MUSEUM STATISTICS ====================

        public async Task<byte[]> GenerateMuseumStatisticsAsync(MuseumStatisticsRequest request)
        {
            var departmentsCount = await _context.Departments.CountAsync();
            var exhibitsCount = await _context.Exhibits.CountAsync();
            var documentsCount = await _context.Documents.CountAsync();
            var mediaFilesCount = await _context.MediaFiles.CountAsync();

            var exhibitBreakdown = await _context.Departments
                .Select(d => new
                {
                    d.Name,
                    Count = _context.Exhibits.Count(e => e.DepartmentId == d.DepartmentId)
                })
                .ToListAsync();

            var pdf = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Статистика музея").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        col.Item().PaddingBottom(5).Text("Общая сводка").Bold().FontSize(14);

                        if (request.IncludeDepartments || request.IncludeExhibits ||
                            request.IncludeDocuments || request.IncludeMediaFiles)
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Показатель").Bold();
                                    header.Cell().Text("Количество").Bold();
                                });

                                if (request.IncludeDepartments)
                                {
                                    table.Cell().Text("Отделов");
                                    table.Cell().Text(departmentsCount.ToString());
                                }

                                if (request.IncludeExhibits)
                                {
                                    table.Cell().Text("Экспонатов");
                                    table.Cell().Text(exhibitsCount.ToString());
                                }

                                if (request.IncludeDocuments)
                                {
                                    table.Cell().Text("Документов (статей)");
                                    table.Cell().Text(documentsCount.ToString());
                                }

                                if (request.IncludeMediaFiles)
                                {
                                    table.Cell().Text("Медиафайлов");
                                    table.Cell().Text(mediaFilesCount.ToString());
                                }
                            });
                        }

                        if (request.IncludeExhibitBreakdown)
                        {
                            col.Item().PaddingTop(15).PaddingBottom(5)
                                .Text("Разбивка экспонатов по отделам")
                                .Bold().FontSize(14);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Отдел").Bold();
                                    header.Cell().Text("Количество экспонатов").Bold();
                                });

                                foreach (var item in exhibitBreakdown)
                                {
                                    table.Cell().Text(item.Name);
                                    table.Cell().Text(item.Count.ToString());
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        // ==================== LOGS ====================

        public async Task<byte[]> GenerateLogsReportAsync(LogsReportRequest request)
        {
            var query = _context.Logs
                .OrderByDescending(l => l.Timestamp)
                .AsQueryable();

            if (request.Limit is > 0)
                query = query.Take(request.Limit.Value);

            var logs = await query.ToListAsync();

            var pdf = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Журнал действий").FontSize(18).Bold();
                        col.Item().Text($"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().Text($"Всего в отчёте: {logs.Count}")
                            .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(110);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Дата").Bold();
                                header.Cell().Text("Роль").Bold();
                                header.Cell().Text("Действие").Bold();
                                header.Cell().Text("Объект").Bold();
                            });

                            foreach (var log in logs)
                            {
                                table.Cell().Text(log.Timestamp.ToString("dd.MM.yyyy HH:mm"));
                                table.Cell().Text(log.UserType);
                                table.Cell().Text(log.Action);
                                table.Cell().Text($"{log.EntityType} — {log.EntityName}");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" из ");
                        x.TotalPages();
                    });
                });
            });

            return pdf.GeneratePdf();
        }

        // ==================== COMBINED REPORT ====================

        public async Task<byte[]> GenerateCombinedReportAsync(CombinedReportRequest request)
        {
            var now = DateTime.Now;

            // Явно типизированные списки с псевдонимом — устраняет CS0173
            List<DocModel> documents;
            if (request.Documents != null)
            {
                documents = await _context.Documents
                    .Include(d => d.Department)
                    .OrderBy(d => d.DocumentId)
                    .Take(request.Documents.Limit ?? int.MaxValue)
                    .ToListAsync();
            }
            else
            {
                documents = new List<DocModel>();
            }

            var exhibits = request.Exhibits != null
                ? await _context.Exhibits
                    .Include(e => e.Department)
                    .OrderBy(e => e.ExhibitId)
                    .Take(request.Exhibits.Limit ?? int.MaxValue)
                    .ToListAsync()
                : new List<Exhibit>();

            var images = request.Images != null
                ? await _context.MediaFiles
                    .Where(m => m.MediaType == "image")
                    .Include(m => m.Department)
                    .OrderBy(m => m.MediaFileId)
                    .Take(request.Images.Limit ?? int.MaxValue)
                    .ToListAsync()
                : new List<MediaFile>();

            var videos = request.Videos != null
                ? await _context.MediaFiles
                    .Where(m => m.MediaType == "video")
                    .Include(m => m.Department)
                    .OrderBy(m => m.MediaFileId)
                    .Take(request.Videos.Limit ?? int.MaxValue)
                    .ToListAsync()
                : new List<MediaFile>();

            var logs = request.Logs != null
                ? await _context.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(request.Logs.Limit ?? int.MaxValue)
                    .ToListAsync()
                : new List<LogEntry>();

            var departmentsCount = await _context.Departments.CountAsync();
            var exhibitsCount = await _context.Exhibits.CountAsync();
            var documentsCount = await _context.Documents.CountAsync();
            var mediaFilesCount = await _context.MediaFiles.CountAsync();

            var exhibitBreakdown = await _context.Departments
                .Select(d => new
                {
                    d.Name,
                    Count = _context.Exhibits.Count(e => e.DepartmentId == d.DepartmentId)
                })
                .ToListAsync();

            byte[]? LoadExhibitThumbnail(string? imagePath)
            {
                if (request.Exhibits?.IncludeImages != true || string.IsNullOrEmpty(imagePath))
                    return null;

                var thumbPath = Path.Combine(_env.WebRootPath, "exhibits", "thumbnails", imagePath);
                if (File.Exists(thumbPath))
                    return File.ReadAllBytes(thumbPath);

                var originalPath = Path.Combine(_env.WebRootPath, "exhibits", "images", imagePath);
                return File.Exists(originalPath) ? File.ReadAllBytes(originalPath) : null;
            }

            byte[]? LoadMediaImage(string? filePath)
            {
                if (request.Images?.IncludeImages != true || string.IsNullOrEmpty(filePath))
                    return null;

                var fullPath = Path.Combine(_env.WebRootPath, filePath);
                return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
            }

            // Переменная pdfBuilder вместо document — устраняет CS0104
            var pdfBuilder = PdfDocument.Create(container =>
            {
                // Титульная страница
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Content().AlignCenter().AlignMiddle().Column(col =>
                    {
                        col.Item().Text("Сводный отчёт").FontSize(28).Bold();
                        col.Item().PaddingTop(10)
                            .Text("Музейная информационная система")
                            .FontSize(14).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                        col.Item().PaddingTop(30)
                            .Text($"Дата формирования: {now:dd.MM.yyyy HH:mm}")
                            .FontSize(12);
                    });
                });

                // Глава 1. Экспонаты
                if (request.Exhibits != null && exhibits.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Глава 1. Экспонаты").FontSize(18).Bold();
                            col.Item().Text($"Всего: {exhibits.Count}")
                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            if (request.Exhibits.Compact)
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(35);
                                        if (request.Exhibits.IncludeImages)
                                            columns.ConstantColumn(50);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("ID").Bold();
                                        if (request.Exhibits.IncludeImages)
                                            header.Cell().Text("");
                                        header.Cell().Text("Название").Bold();
                                        header.Cell().Text("Отдел").Bold();
                                        header.Cell().Text("Материалы").Bold();
                                        header.Cell().Text("Хранение").Bold();
                                    });

                                    foreach (var exhibit in exhibits)
                                    {
                                        table.Cell().Text(exhibit.ExhibitId.ToString());

                                        if (request.Exhibits.IncludeImages)
                                        {
                                            var bytes = LoadExhibitThumbnail(exhibit.ImagePath);
                                            if (bytes != null)
                                                table.Cell().Height(40).Width(40).Image(bytes).FitArea();
                                            else
                                                table.Cell().Text("—");
                                        }

                                        table.Cell().Text(exhibit.Name);
                                        table.Cell().Text(exhibit.Department?.Name ?? "Без отдела");
                                        table.Cell().Text(exhibit.Materials ?? "—");
                                        table.Cell().Text(exhibit.IsPermanent ? "Постоянное" : "Временное");
                                    }
                                });
                            }
                            else
                            {
                                foreach (var exhibit in exhibits)
                                {
                                    col.Item().PaddingBottom(10)
                                        .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                        .Row(row =>
                                        {
                                            if (request.Exhibits.IncludeImages)
                                            {
                                                var bytes = LoadExhibitThumbnail(exhibit.ImagePath);
                                                if (bytes != null)
                                                    row.ConstantItem(70).Height(70).Image(bytes).FitArea();
                                                else
                                                    row.ConstantItem(70);
                                            }

                                            row.RelativeItem()
                                                .PaddingLeft(request.Exhibits.IncludeImages ? 10 : 0)
                                                .Column(info =>
                                                {
                                                    info.Item().Text($"№{exhibit.ExhibitId} — {exhibit.Name}")
                                                        .Bold().FontSize(13);
                                                    info.Item().Text($"Отдел: {exhibit.Department?.Name ?? "Без отдела"}");
                                                    info.Item().Text($"Материалы: {exhibit.Materials ?? "—"}");
                                                    info.Item().Text($"Тип хранения: {(exhibit.IsPermanent ? "Постоянное" : "Временное")}");
                                                });
                                        });
                                }
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }

                // Глава 2. Документы
                if (request.Documents != null && documents.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Глава 2. Документы (статьи)").FontSize(18).Bold();
                            col.Item().Text($"Всего: {documents.Count}")
                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            if (request.Documents.Compact)
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("ID").Bold();
                                        header.Cell().Text("Название").Bold();
                                        header.Cell().Text("Отдел").Bold();
                                        header.Cell().Text("Тип файла").Bold();
                                    });

                                    foreach (var doc in documents)
                                    {
                                        table.Cell().Text(doc.DocumentId.ToString());
                                        table.Cell().Text(doc.Title);
                                        table.Cell().Text(doc.Department?.Name ?? "Без отдела");
                                        table.Cell().Text(doc.FileType);
                                    }
                                });
                            }
                            else
                            {
                                foreach (var doc in documents)
                                {
                                    col.Item().PaddingBottom(8)
                                        .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                        .Column(info =>
                                        {
                                            info.Item().Text($"№{doc.DocumentId} — {doc.Title}")
                                                .Bold().FontSize(13);
                                            info.Item().Text($"Отдел: {doc.Department?.Name ?? "Без отдела"}");
                                            info.Item().Text($"Тип файла: {doc.FileType}");
                                            info.Item().Text($"Путь: {doc.FilePath}")
                                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                                        });
                                }
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }

                // Глава 3. Изображения
                if (request.Images != null && images.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Глава 3. Изображения").FontSize(18).Bold();
                            col.Item().Text($"Всего: {images.Count}")
                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            if (request.Images.Compact)
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        if (request.Images.IncludeImages)
                                            columns.ConstantColumn(50);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("ID").Bold();
                                        if (request.Images.IncludeImages)
                                            header.Cell().Text("");
                                        header.Cell().Text("Название").Bold();
                                        header.Cell().Text("Отдел").Bold();
                                    });

                                    foreach (var img in images)
                                    {
                                        table.Cell().Text(img.MediaFileId.ToString());

                                        if (request.Images.IncludeImages)
                                        {
                                            var bytes = LoadMediaImage(img.FilePath);
                                            if (bytes != null)
                                                table.Cell().Height(40).Width(40).Image(bytes).FitArea();
                                            else
                                                table.Cell().Text("—");
                                        }

                                        table.Cell().Text(img.Title);
                                        table.Cell().Text(img.Department?.Name ?? "Без отдела");
                                    }
                                });
                            }
                            else
                            {
                                foreach (var img in images)
                                {
                                    col.Item().PaddingBottom(10)
                                        .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                        .Row(row =>
                                        {
                                            if (request.Images.IncludeImages)
                                            {
                                                var bytes = LoadMediaImage(img.FilePath);
                                                if (bytes != null)
                                                    row.ConstantItem(70).Height(70).Image(bytes).FitArea();
                                                else
                                                    row.ConstantItem(70);
                                            }

                                            row.RelativeItem()
                                                .PaddingLeft(request.Images.IncludeImages ? 10 : 0)
                                                .Column(info =>
                                                {
                                                    info.Item().Text($"№{img.MediaFileId} — {img.Title}")
                                                        .Bold().FontSize(13);
                                                    info.Item().Text($"Отдел: {img.Department?.Name ?? "Без отдела"}");
                                                    if (!string.IsNullOrEmpty(img.Description))
                                                        info.Item().Text($"Описание: {img.Description}");
                                                    info.Item().Text($"Путь: {img.FilePath}")
                                                        .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                                                });
                                        });
                                }
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }

                // Глава 4. Видео
                if (request.Videos != null && videos.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Глава 4. Видео").FontSize(18).Bold();
                            col.Item().Text($"Всего: {videos.Count}")
                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            if (request.Videos.Compact)
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.ConstantColumn(40);
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(3);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("ID").Bold();
                                        header.Cell().Text("Название").Bold();
                                        header.Cell().Text("Отдел").Bold();
                                    });

                                    foreach (var video in videos)
                                    {
                                        table.Cell().Text(video.MediaFileId.ToString());
                                        table.Cell().Text(video.Title);
                                        table.Cell().Text(video.Department?.Name ?? "Без отдела");
                                    }
                                });
                            }
                            else
                            {
                                foreach (var video in videos)
                                {
                                    col.Item().PaddingBottom(8)
                                        .Border(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(10)
                                        .Column(info =>
                                        {
                                            info.Item().Text($"№{video.MediaFileId} — {video.Title}")
                                                .Bold().FontSize(13);
                                            info.Item().Text($"Отдел: {video.Department?.Name ?? "Без отдела"}");
                                            if (!string.IsNullOrEmpty(video.Description))
                                                info.Item().Text($"Описание: {video.Description}");
                                            info.Item().Text($"Путь: {video.FilePath}")
                                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                                        });
                                }
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }

                // Глава 5. Статистика
                if (request.Statistics != null)
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Глава 5. Статистика музея").FontSize(18).Bold();
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            col.Item().PaddingBottom(5).Text("Общая сводка").Bold().FontSize(14);

                            if (request.Statistics.IncludeDepartments || request.Statistics.IncludeExhibits ||
                                request.Statistics.IncludeDocuments || request.Statistics.IncludeMediaFiles)
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Показатель").Bold();
                                        header.Cell().Text("Количество").Bold();
                                    });

                                    if (request.Statistics.IncludeDepartments)
                                    {
                                        table.Cell().Text("Отделов");
                                        table.Cell().Text(departmentsCount.ToString());
                                    }

                                    if (request.Statistics.IncludeExhibits)
                                    {
                                        table.Cell().Text("Экспонатов");
                                        table.Cell().Text(exhibitsCount.ToString());
                                    }

                                    if (request.Statistics.IncludeDocuments)
                                    {
                                        table.Cell().Text("Документов (статей)");
                                        table.Cell().Text(documentsCount.ToString());
                                    }

                                    if (request.Statistics.IncludeMediaFiles)
                                    {
                                        table.Cell().Text("Медиафайлов");
                                        table.Cell().Text(mediaFilesCount.ToString());
                                    }
                                });
                            }

                            if (request.Statistics.IncludeExhibitBreakdown)
                            {
                                col.Item().PaddingTop(15).PaddingBottom(5)
                                    .Text("Разбивка экспонатов по отделам")
                                    .Bold().FontSize(14);

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Отдел").Bold();
                                        header.Cell().Text("Количество экспонатов").Bold();
                                    });

                                    foreach (var item in exhibitBreakdown)
                                    {
                                        table.Cell().Text(item.Name);
                                        table.Cell().Text(item.Count.ToString());
                                    }
                                });
                            }
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }

                // Глава 6. Журнал действий
                if (request.Logs != null && logs.Count > 0)
                {
                    container.Page(page =>
                    {
                        page.Size(QuestPDF.Helpers.PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(11));

                        page.Header().Column(col =>
                        {
                            col.Item().Text("Глава 6. Журнал действий").FontSize(18).Bold();
                            col.Item().Text($"Всего: {logs.Count}")
                                .FontSize(9).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(QuestPDF.Helpers.Colors.Grey.Lighten1);
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(110);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(100);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Дата").Bold();
                                    header.Cell().Text("Роль").Bold();
                                    header.Cell().Text("Действие").Bold();
                                    header.Cell().Text("Объект").Bold();
                                });

                                foreach (var log in logs)
                                {
                                    table.Cell().Text(log.Timestamp.ToString("dd.MM.yyyy HH:mm"));
                                    table.Cell().Text(log.UserType);
                                    table.Cell().Text(log.Action);
                                    table.Cell().Text($"{log.EntityType} — {log.EntityName}");
                                }
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber();
                            x.Span(" из ");
                            x.TotalPages();
                        });
                    });
                }
            });

            return pdfBuilder.GeneratePdf();
        }
    }
}