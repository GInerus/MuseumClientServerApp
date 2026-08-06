using MuseumClient.Commands;
using MuseumClient.Services;
using MuseumClient.ViewModels.Details;
using System.ComponentModel;
using System.Windows;

namespace MuseumClient.ViewModels
{
    public class ContentHubViewModel : INotifyPropertyChanged
    {
        private object _currentTabView;

        public string CurrentUserType =>
            AuthService.Instance().CurrentUserType?.ToUpper() ?? "UNKNOWN"; // Действующая тип юсера (роль)
        public string CurrentUserTypeDisplay =>
            AuthService.Instance().CurrentUserType == "admin"
                ? "Администратор"
                : "Гость";

        public object CurrentTabView
        {
            get => _currentTabView;
            set
            {

                if (_currentTabView is IDisposable disposable)
                {
                    disposable.Dispose();
                }


                _currentTabView = value;


                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(CurrentTabView)));

                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(nameof(CurrentUserType)));
            }
        }

        private string _selectedMenu = "About";

        public string SelectedMenu
        {
            get => _selectedMenu;
            set
            {
                _selectedMenu = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedMenu)));
            }
        }

        // Внутренние ViewModels
        public AboutMuseumViewModel AboutMuseumVM { get; }
        public ArticlesViewModel ArticlesVM { get; }
        public ExhibitsViewModel ExhibitsVM { get; }
        public DepartmentsViewModel DepartmentsVM { get; }
        public MediaImagesViewModel MediaImagesVM { get; }
        public MediaVideosViewModel MediaVideosVM { get; }
        public SettingsViewModel SettingsVM { get; }
        public ReportsViewModel ReportsVM { get; }

        private readonly MainViewModel _mainVM;

        // Команды для кнопок
        public RelayCommand ShowAboutMuseumCommand { get; }
        public RelayCommand ShowArticlesCommand { get; }
        public RelayCommand ShowExhibitsCommand { get; }
        public RelayCommand ShowDepartmentsCommand { get; }
        public RelayCommand ShowIllustrationsCommand { get; }
        public RelayCommand ShowVideosCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }



        public ContentHubViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // Инициализация внутренних ViewModel
            AboutMuseumVM = new AboutMuseumViewModel();
            ArticlesVM = new ArticlesViewModel(this);
            ExhibitsVM = new ExhibitsViewModel(this);
            DepartmentsVM = new DepartmentsViewModel(this);
            MediaImagesVM = new MediaImagesViewModel(this);
            MediaVideosVM = new MediaVideosViewModel(this);
            ReportsVM = new ReportsViewModel(this);
            SettingsVM = new SettingsViewModel(this);

            // Команды
            ShowAboutMuseumCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "About";
                CurrentTabView = AboutMuseumVM;
                await AboutMuseumVM.LoadDepartmentCountAsync();
            });

            ShowArticlesCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "Articles";
                CurrentTabView = ArticlesVM;
            });

            ShowExhibitsCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "Exhibits";
                CurrentTabView = ExhibitsVM;
            });

            ShowDepartmentsCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "Departments";
                CurrentTabView = DepartmentsVM;
            });

            ShowIllustrationsCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "Illustrations";
                CurrentTabView = MediaImagesVM;
            });

            ShowVideosCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "Videos";
                CurrentTabView = MediaVideosVM;
            });

            ExitCommand = new RelayCommand(async _ =>
            {
                AuthService.Instance().Logout();

                _mainVM.ShowLoginView();
            });

            ShowSettingsCommand = new RelayCommand(async _ =>
            {
                SelectedMenu = "Settings";
                CurrentTabView = SettingsVM;

                await SettingsVM.LoadServerVersionAsync();
            });

            // Стартовая вкладка
            CurrentTabView = AboutMuseumVM;

            AuthService.Instance().AuthChanged += OnAuthChanged;
        }

        public void ShowIllustration(int id)
        {
            CurrentTabView = new IllustrationViewModel(id);
        }

        public void ShowDocument(int id, string fileType)
        {
            CurrentTabView = new DocumentViewerViewModel(id, fileType);
        }

        public void ShowExhibit(int id)
        {
            CurrentTabView = new ExhibitViewerViewModel(id);
        }

        public void ShowExhibits()
        {
            CurrentTabView = ExhibitsVM;
        }

        public void ShowCreateExhibit()
        {
            CurrentTabView = new AddExhibitViewModel(this);
        }

        public void ShowVideo(int id)
        {
            CurrentTabView = new VideoViewerViewModel(id);
        }

        public void ShowDepartmentCatalog(int departmentId, string departmentName)
        {
            var vm = new DepartmentCatalogViewModel(this, departmentId, departmentName);
            CurrentTabView = vm;
            _ = vm.LoadAllAsync();
        }

        public void ShowArticles()
        {
            CurrentTabView = ArticlesVM;
        }

        public void ShowCreateArticle()
        {
            CurrentTabView = new AddArticleViewModel(this);
        }

        public void ShowDepartments()
        {
            CurrentTabView = DepartmentsVM;
        }

        public void ShowCreateDepartment()
        {
            CurrentTabView = new AddDepartmentViewModel(this);
        }

        public void ShowIllustrations()
        {
            CurrentTabView = MediaImagesVM;
        }

        public void ShowCreateImage()
        {
            CurrentTabView = new AddImageViewModel(this);
        }

        public void ShowEditExhibit(int id)
        {
            CurrentTabView = new AddExhibitViewModel(this, id);
        }

        public void ShowEditDepartment(int id)
        {
            CurrentTabView = new AddDepartmentViewModel(this, id);
        }

        public void ShowVideos()
        {
            CurrentTabView = MediaVideosVM;
        }

        public void ShowCreateVideo()
        {
            CurrentTabView = new AddVideoViewModel(this);
        }

        public void ShowGuide()
        {
            CurrentTabView = DocumentViewerViewModel.CreateForGuide();
        }

        public void ShowReportPdf(byte[] pdfBytes, string title)
        {
            CurrentTabView = DocumentViewerViewModel.CreateForReport(pdfBytes, title);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnAuthChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUserType)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUserTypeDisplay)));
        }
    }
}