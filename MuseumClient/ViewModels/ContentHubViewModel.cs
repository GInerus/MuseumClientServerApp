using System.ComponentModel;
using System.Windows;
using MuseumClient.Commands;

namespace MuseumClient.ViewModels
{
    public class ContentHubViewModel : INotifyPropertyChanged
    {
        private object _currentTabView;
        public object CurrentTabView
        {
            get => _currentTabView;
            set
            {
                _currentTabView = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTabView)));
            }
        }

        // Внутренние ViewModels
        public AboutMuseumViewModel AboutMuseumVM { get; }
        //public DepartmentViewModel DepartmentVM { get; }
        //public DocumentViewModel DocumentVM { get; }
        private readonly MainViewModel _mainVM;

        // Команды для кнопок
        public RelayCommand ShowAboutMuseumCommand { get; }
        public RelayCommand ShowArticlesCommand { get; }
        public RelayCommand ShowExhibitsCommand { get; }
        public RelayCommand ShowDepartmentsCommand { get; }
        public RelayCommand ShowIllustrationsCommand { get; }
        public RelayCommand ShowVideosCommand { get; }
        public RelayCommand ExitCommand { get; }


        public ContentHubViewModel(MainViewModel mainVM)
        {
            _mainVM = mainVM;

            // Инициализация внутренних ViewModel
            AboutMuseumVM = new AboutMuseumViewModel();
            //DepartmentVM = new DepartmentViewModel();
            //DocumentVM = new DocumentViewModel();

            // Команды
            ShowAboutMuseumCommand = new RelayCommand(async _ =>
            {
                CurrentTabView = AboutMuseumVM;
                await AboutMuseumVM.LoadDepartmentCountAsync(); // обновляем данные
            });

            ShowArticlesCommand = new RelayCommand(async _ =>
            {
                MessageBox.Show("Статьи");
                //CurrentTabView = DepartmentVM;
                //await Task.CompletedTask;
            });

            ShowExhibitsCommand = new RelayCommand(async _ =>
            {
                MessageBox.Show("Экспонаты");
                //CurrentTabView = DepartmentVM;
                //await Task.CompletedTask;
            });

            ShowDepartmentsCommand = new RelayCommand(async _ =>
            {
                MessageBox.Show("Отделы");
                //CurrentTabView = DepartmentVM;
                //await Task.CompletedTask;
            });

            ShowIllustrationsCommand = new RelayCommand(async _ =>
            {
                MessageBox.Show("Иллюстрации");
                //CurrentTabView = DepartmentVM;
                //await Task.CompletedTask;
            });

            ShowVideosCommand = new RelayCommand(async _ =>
            {
                MessageBox.Show("Видео");
                //CurrentTabView = DepartmentVM;
                //await Task.CompletedTask;
            });

            ExitCommand = new RelayCommand(async _ =>
            {
                _mainVM.ShowLoginView();
            });

            // Стартовая вкладка
            CurrentTabView = AboutMuseumVM;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}