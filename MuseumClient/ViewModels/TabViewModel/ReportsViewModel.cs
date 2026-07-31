using MuseumClient.Commands;
using MuseumClient.Models;
using MuseumClient.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace MuseumClient.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly ContentHubViewModel _hub;

        // "Menu" | "Exhibits" (позже: "Documents" | "Images" | "Videos" | "Statistics" | "Logs" | "All")
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

        // ===== Отчёт по экспонатам =====

        private int _exhibitsTotalCount;
        public int ExhibitsTotalCount
        {
            get => _exhibitsTotalCount;
            set
            {
                _exhibitsTotalCount = value;
                OnPropertyChanged(nameof(ExhibitsTotalCount));
            }
        }

        // Пусто = вывести все экспонаты
        private string _exhibitsLimitText = "";
        public string ExhibitsLimitText
        {
            get => _exhibitsLimitText;
            set
            {
                _exhibitsLimitText = value;
                OnPropertyChanged(nameof(ExhibitsLimitText));
            }
        }

        private bool _exhibitsCompact = true;
        public bool ExhibitsCompact
        {
            get => _exhibitsCompact;
            set
            {
                _exhibitsCompact = value;
                OnPropertyChanged(nameof(ExhibitsCompact));
            }
        }

        private bool _exhibitsIncludeImages;
        public bool ExhibitsIncludeImages
        {
            get => _exhibitsIncludeImages;
            set
            {
                _exhibitsIncludeImages = value;
                OnPropertyChanged(nameof(ExhibitsIncludeImages));
            }
        }

        public RelayCommand SelectSectionCommand { get; }
        public RelayCommand BackToMenuCommand { get; }
        public RelayCommand GenerateExhibitsReportCommand { get; }

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

                if (section == "Exhibits")
                {
                    await LoadExhibitsCountAsync();
                }
            });

            BackToMenuCommand = new RelayCommand(_ =>
            {
                SelectedSection = "Menu";
                return Task.CompletedTask;
            });

            GenerateExhibitsReportCommand = new RelayCommand(async _ => await GenerateExhibitsReportAsync());
        }

        private async Task LoadExhibitsCountAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<CountResponse>("Report/exhibits/count");
                ExhibitsTotalCount = response?.Count ?? 0;
            }
            catch (Exception ex)
            {
                InfoService.Show($"Ошибка получения количества экспонатов:\n{ex.Message}");
            }
        }

        private async Task GenerateExhibitsReportAsync()
        {
            if (IsBusy)
                return;

            int? limit = null;

            if (!string.IsNullOrWhiteSpace(ExhibitsLimitText))
            {
                if (!int.TryParse(ExhibitsLimitText.Trim(), out var parsed) || parsed <= 0)
                {
                    InfoService.Show("Введите корректное количество экспонатов (целое число больше 0) или оставьте поле пустым, чтобы вывести все.");
                    return;
                }

                limit = parsed;
            }

            try
            {
                IsBusy = true;
                BusyText = "Формирование отчёта по экспонатам...";

                var bytes = await _apiService.PostBytesAsync("Report/exhibits", new
                {
                    limit,
                    compact = ExhibitsCompact,
                    includeImages = ExhibitsIncludeImages
                });

                var path = Path.Combine(
                    Path.GetTempPath(),
                    $"report_exhibits_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                await File.WriteAllBytesAsync(path, bytes);

                _hub.ShowReportPdf(path, "Отчёт по экспонатам");
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}