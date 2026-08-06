using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MuseumClient.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly ContentHubViewModel _hub;

        // "Menu" | "Exhibits" | "Documents" | "Images" | "Videos" | "Statistics" | "Logs" | "Combined"
        private string _selectedSection = "Menu";
        public string SelectedSection
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                OnPropertyChanged(nameof(SelectedSection));
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private string _busyText = "";
        public string BusyText
        {
            get => _busyText;
            set
            {
                _busyText = value;
                OnPropertyChanged(nameof(BusyText));
            }
        }

        // ==================== ЭКСПОНАТЫ ====================
        private int _exhibitsTotalCount;
        public int ExhibitsTotalCount
        {
            get => _exhibitsTotalCount;
            set { _exhibitsTotalCount = value; OnPropertyChanged(nameof(ExhibitsTotalCount)); }
        }

        private string _exhibitsLimitText = "";
        public string ExhibitsLimitText
        {
            get => _exhibitsLimitText;
            set { _exhibitsLimitText = value; OnPropertyChanged(nameof(ExhibitsLimitText)); }
        }

        private bool _exhibitsCompact = true;
        public bool ExhibitsCompact
        {
            get => _exhibitsCompact;
            set { _exhibitsCompact = value; OnPropertyChanged(nameof(ExhibitsCompact)); }
        }

        private bool _exhibitsIncludeImages;
        public bool ExhibitsIncludeImages
        {
            get => _exhibitsIncludeImages;
            set { _exhibitsIncludeImages = value; OnPropertyChanged(nameof(ExhibitsIncludeImages)); }
        }

        // ==================== ДОКУМЕНТЫ ====================
        private int _documentsTotalCount;
        public int DocumentsTotalCount
        {
            get => _documentsTotalCount;
            set { _documentsTotalCount = value; OnPropertyChanged(nameof(DocumentsTotalCount)); }
        }

        private string _documentsLimitText = "";
        public string DocumentsLimitText
        {
            get => _documentsLimitText;
            set { _documentsLimitText = value; OnPropertyChanged(nameof(DocumentsLimitText)); }
        }

        private bool _documentsCompact = true;
        public bool DocumentsCompact
        {
            get => _documentsCompact;
            set { _documentsCompact = value; OnPropertyChanged(nameof(DocumentsCompact)); }
        }

        // ==================== ИЗОБРАЖЕНИЯ ====================
        private int _imagesTotalCount;
        public int ImagesTotalCount
        {
            get => _imagesTotalCount;
            set { _imagesTotalCount = value; OnPropertyChanged(nameof(ImagesTotalCount)); }
        }

        private string _imagesLimitText = "";
        public string ImagesLimitText
        {
            get => _imagesLimitText;
            set { _imagesLimitText = value; OnPropertyChanged(nameof(ImagesLimitText)); }
        }

        private bool _imagesCompact = true;
        public bool ImagesCompact
        {
            get => _imagesCompact;
            set { _imagesCompact = value; OnPropertyChanged(nameof(ImagesCompact)); }
        }

        private bool _imagesIncludeImages = true;
        public bool ImagesIncludeImages
        {
            get => _imagesIncludeImages;
            set { _imagesIncludeImages = value; OnPropertyChanged(nameof(ImagesIncludeImages)); }
        }

        // ==================== ВИДЕО ====================
        private int _videosTotalCount;
        public int VideosTotalCount
        {
            get => _videosTotalCount;
            set { _videosTotalCount = value; OnPropertyChanged(nameof(VideosTotalCount)); }
        }

        private string _videosLimitText = "";
        public string VideosLimitText
        {
            get => _videosLimitText;
            set { _videosLimitText = value; OnPropertyChanged(nameof(VideosLimitText)); }
        }

        private bool _videosCompact = true;
        public bool VideosCompact
        {
            get => _videosCompact;
            set { _videosCompact = value; OnPropertyChanged(nameof(VideosCompact)); }
        }

        // ==================== СТАТИСТИКА ====================
        private bool _statIncludeDepartments = true;
        public bool StatIncludeDepartments
        {
            get => _statIncludeDepartments;
            set { _statIncludeDepartments = value; OnPropertyChanged(nameof(StatIncludeDepartments)); }
        }

        private bool _statIncludeExhibits = true;
        public bool StatIncludeExhibits
        {
            get => _statIncludeExhibits;
            set { _statIncludeExhibits = value; OnPropertyChanged(nameof(StatIncludeExhibits)); }
        }

        private bool _statIncludeDocuments = true;
        public bool StatIncludeDocuments
        {
            get => _statIncludeDocuments;
            set { _statIncludeDocuments = value; OnPropertyChanged(nameof(StatIncludeDocuments)); }
        }

        private bool _statIncludeMediaFiles = true;
        public bool StatIncludeMediaFiles
        {
            get => _statIncludeMediaFiles;
            set { _statIncludeMediaFiles = value; OnPropertyChanged(nameof(StatIncludeMediaFiles)); }
        }

        private bool _statIncludeExhibitBreakdown = true;
        public bool StatIncludeExhibitBreakdown
        {
            get => _statIncludeExhibitBreakdown;
            set { _statIncludeExhibitBreakdown = value; OnPropertyChanged(nameof(StatIncludeExhibitBreakdown)); }
        }

        // ==================== ЖУРНАЛ ====================
        private int _logsTotalCount;
        public int LogsTotalCount
        {
            get => _logsTotalCount;
            set { _logsTotalCount = value; OnPropertyChanged(nameof(LogsTotalCount)); }
        }

        private string _logsLimitText = "";
        public string LogsLimitText
        {
            get => _logsLimitText;
            set { _logsLimitText = value; OnPropertyChanged(nameof(LogsLimitText)); }
        }

        // ==================== СВОДНЫЙ ====================
        private bool _combinedIncludeExhibits = true;
        public bool CombinedIncludeExhibits
        {
            get => _combinedIncludeExhibits;
            set { _combinedIncludeExhibits = value; OnPropertyChanged(nameof(CombinedIncludeExhibits)); }
        }

        private bool _combinedIncludeDocuments = true;
        public bool CombinedIncludeDocuments
        {
            get => _combinedIncludeDocuments;
            set { _combinedIncludeDocuments = value; OnPropertyChanged(nameof(CombinedIncludeDocuments)); }
        }

        private bool _combinedIncludeImagesSection = true;
        public bool CombinedIncludeImagesSection
        {
            get => _combinedIncludeImagesSection;
            set { _combinedIncludeImagesSection = value; OnPropertyChanged(nameof(CombinedIncludeImagesSection)); }
        }

        private bool _combinedIncludeVideos = true;
        public bool CombinedIncludeVideos
        {
            get => _combinedIncludeVideos;
            set { _combinedIncludeVideos = value; OnPropertyChanged(nameof(CombinedIncludeVideos)); }
        }

        private bool _combinedIncludeStatistics = true;
        public bool CombinedIncludeStatistics
        {
            get => _combinedIncludeStatistics;
            set { _combinedIncludeStatistics = value; OnPropertyChanged(nameof(CombinedIncludeStatistics)); }
        }

        private bool _combinedIncludeLogs = true;
        public bool CombinedIncludeLogs
        {
            get => _combinedIncludeLogs;
            set { _combinedIncludeLogs = value; OnPropertyChanged(nameof(CombinedIncludeLogs)); }
        }

        private string _combinedLimitText = "";
        public string CombinedLimitText
        {
            get => _combinedLimitText;
            set { _combinedLimitText = value; OnPropertyChanged(nameof(CombinedLimitText)); }
        }

        private bool _combinedCompact = true;
        public bool CombinedCompact
        {
            get => _combinedCompact;
            set { _combinedCompact = value; OnPropertyChanged(nameof(CombinedCompact)); }
        }

        private bool _combinedIncludeThumbnails = true;
        public bool CombinedIncludeThumbnails
        {
            get => _combinedIncludeThumbnails;
            set { _combinedIncludeThumbnails = value; OnPropertyChanged(nameof(CombinedIncludeThumbnails)); }
        }

        private bool _combinedIncludeImages = true;
        public bool CombinedIncludeImages
        {
            get => _combinedIncludeImages;
            set { _combinedIncludeImages = value; OnPropertyChanged(nameof(CombinedIncludeImages)); }
        }

        // ==================== КОМАНДЫ ====================
        public RelayCommand SelectSectionCommand { get; }
        public RelayCommand BackToMenuCommand { get; }

        public RelayCommand GenerateExhibitsReportCommand { get; }
        public RelayCommand GenerateDocumentsReportCommand { get; }
        public RelayCommand GenerateImagesReportCommand { get; }
        public RelayCommand GenerateVideosReportCommand { get; }
        public RelayCommand GenerateStatisticsReportCommand { get; }
        public RelayCommand GenerateLogsReportCommand { get; }
        public RelayCommand GenerateCombinedReportCommand { get; }

        public ReportsViewModel(ContentHubViewModel hub)
        {
            _hub = hub;

            _apiService = new ApiService(
                new ConfigService().Server,
                AuthService.Instance());

            SelectSectionCommand = new RelayCommand(async param =>
            {
                if (param is not string section)
                    return;

                SelectedSection = section;

                switch (section)
                {
                    case "Exhibits": await LoadExhibitsCountAsync(); break;
                    case "Documents": await LoadDocumentsCountAsync(); break;
                    case "Images": await LoadImagesCountAsync(); break;
                    case "Videos": await LoadVideosCountAsync(); break;
                    case "Logs": await LoadLogsCountAsync(); break;
                }
            });

            BackToMenuCommand = new RelayCommand(_ =>
            {
                SelectedSection = "Menu";
                return Task.CompletedTask;
            });

            GenerateExhibitsReportCommand = new RelayCommand(async _ => await GenerateExhibitsReportAsync());
            GenerateDocumentsReportCommand = new RelayCommand(async _ => await GenerateDocumentsReportAsync());
            GenerateImagesReportCommand = new RelayCommand(async _ => await GenerateImagesReportAsync());
            GenerateVideosReportCommand = new RelayCommand(async _ => await GenerateVideosReportAsync());
            GenerateStatisticsReportCommand = new RelayCommand(async _ => await GenerateStatisticsReportAsync());
            GenerateLogsReportCommand = new RelayCommand(async _ => await GenerateLogsReportAsync());
            GenerateCombinedReportCommand = new RelayCommand(async _ => await GenerateCombinedReportAsync());
        }

        // ==================== ЗАГРУЗКА COUNT ====================

        private async Task LoadExhibitsCountAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CountResponse>("Exhibit/count");
                ExhibitsTotalCount = response?.Count ?? 0;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка получения количества экспонатов:\n{ex.Message}");
            }
        }

        private async Task LoadDocumentsCountAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CountResponse>("document/count");
                DocumentsTotalCount = response?.Count ?? 0;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка получения количества документов:\n{ex.Message}");
            }
        }

        private async Task LoadImagesCountAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CountResponse>("Report/images/count");
                ImagesTotalCount = response?.Count ?? 0;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка получения количества изображений:\n{ex.Message}");
            }
        }

        private async Task LoadVideosCountAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CountResponse>("Report/videos/count");
                VideosTotalCount = response?.Count ?? 0;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка получения количества видео:\n{ex.Message}");
            }
        }

        private async Task LoadLogsCountAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CountResponse>("Report/logs/count");
                LogsTotalCount = response?.Count ?? 0;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка получения количества логов:\n{ex.Message}");
            }
        }

        // ==================== ГЕНЕРАЦИЯ ОТЧЁТОВ ====================

        private async Task GenerateExhibitsReportAsync()
        {
            if (IsBusy) return;

            var limit = ParseLimit(ExhibitsLimitText);
            if (limit == -1)
            {
                InfoService.Show("Введите корректное количество (целое число > 0) или оставьте пустым.");
                return;
            }

            await GenerateReportAsync("Report/exhibits", new
            {
                limit = limit == 0 ? (int?)null : limit,
                compact = ExhibitsCompact,
                includeImages = ExhibitsIncludeImages
            }, "Отчёт по экспонатам");
        }

        private async Task GenerateDocumentsReportAsync()
        {
            if (IsBusy) return;

            var limit = ParseLimit(DocumentsLimitText);
            if (limit == -1)
            {
                InfoService.Show("Введите корректное количество (целое число > 0) или оставьте пустым.");
                return;
            }

            await GenerateReportAsync("Report/documents", new
            {
                limit = limit == 0 ? (int?)null : limit,
                compact = DocumentsCompact
            }, "Отчёт по документам");
        }

        private async Task GenerateImagesReportAsync()
        {
            if (IsBusy) return;

            var limit = ParseLimit(ImagesLimitText);
            if (limit == -1)
            {
                InfoService.Show("Введите корректное количество (целое число > 0) или оставьте пустым.");
                return;
            }

            await GenerateReportAsync("Report/images", new
            {
                limit = limit == 0 ? (int?)null : limit,
                compact = ImagesCompact,
                includeImages = ImagesIncludeImages
            }, "Отчёт по изображениям");
        }

        private async Task GenerateVideosReportAsync()
        {
            if (IsBusy) return;

            var limit = ParseLimit(VideosLimitText);
            if (limit == -1)
            {
                InfoService.Show("Введите корректное количество (целое число > 0) или оставьте пустым.");
                return;
            }

            await GenerateReportAsync("Report/videos", new
            {
                limit = limit == 0 ? (int?)null : limit,
                compact = VideosCompact
            }, "Отчёт по видео");
        }

        private async Task GenerateStatisticsReportAsync()
        {
            if (IsBusy) return;

            await GenerateReportAsync("Report/statistics", new
            {
                includeDepartments = StatIncludeDepartments,
                includeExhibits = StatIncludeExhibits,
                includeDocuments = StatIncludeDocuments,
                includeMediaFiles = StatIncludeMediaFiles,
                includeExhibitBreakdown = StatIncludeExhibitBreakdown
            }, "Статистика музея");
        }

        private async Task GenerateLogsReportAsync()
        {
            if (IsBusy) return;

            var limit = ParseLimit(LogsLimitText);
            if (limit == -1)
            {
                InfoService.Show("Введите корректное количество (целое число > 0) или оставьте пустым.");
                return;
            }

            await GenerateReportAsync("Report/logs", new
            {
                limit = limit == 0 ? (int?)null : limit
            }, "Журнал действий");
        }

        private async Task GenerateCombinedReportAsync()
        {
            if (IsBusy) return;

            var limit = ParseLimit(CombinedLimitText);
            if (limit == -1)
            {
                InfoService.Show("Введите корректное количество (целое число > 0) или оставьте пустым.");
                return;
            }

            int? nullableLimit = limit == 0 ? null : limit;

            await GenerateReportAsync("Report/combined", new
            {
                exhibits = CombinedIncludeExhibits ? new
                {
                    limit = nullableLimit,
                    compact = CombinedCompact,
                    includeImages = CombinedIncludeImages
                } : null,
                documents = CombinedIncludeDocuments ? new
                {
                    limit = nullableLimit,
                    compact = CombinedCompact
                } : null,
                images = CombinedIncludeImagesSection ? new
                {
                    limit = nullableLimit,
                    compact = CombinedCompact,
                    includeImages = CombinedIncludeThumbnails
                } : null,
                videos = CombinedIncludeVideos ? new
                {
                    limit = nullableLimit,
                    compact = CombinedCompact
                } : null,
                statistics = CombinedIncludeStatistics ? new
                {
                    includeDepartments = StatIncludeDepartments,
                    includeExhibits = StatIncludeExhibits,
                    includeDocuments = StatIncludeDocuments,
                    includeMediaFiles = StatIncludeMediaFiles,
                    includeExhibitBreakdown = StatIncludeExhibitBreakdown
                } : null,
                logs = CombinedIncludeLogs ? new
                {
                    limit = nullableLimit
                } : null
            }, "Сводный отчёт");
        }

        // ==================== УТИЛИТЫ ====================

        private async Task GenerateReportAsync(string endpoint, object payload, string title)
        {
            try
            {
                IsBusy = true;
                BusyText = $"Формирование: {title}...";

                var bytes = await _apiService.PostBytesAsync(endpoint, payload);

                _hub.ShowReportPdf(bytes, title);
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка формирования отчёта:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 0 = пусто (все записи), >0 = лимит, -1 = ошибка парсинга
        /// </summary>
        private static int ParseLimit(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            if (!int.TryParse(text.Trim(), out var value) || value <= 0)
                return -1;

            return value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}