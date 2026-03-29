using System.ComponentModel;
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

        // Команды для кнопок
        public RelayCommand ShowAboutMuseumCommand { get; }
        //public RelayCommand ShowDepartmentsCommand { get; }
        //public RelayCommand ShowDocumentsCommand { get; }

        public ContentHubViewModel()
        {
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

            //ShowDepartmentsCommand = new RelayCommand(async _ =>
            //{
            //    CurrentTabView = DepartmentVM;
            //    await Task.CompletedTask;
            //});

            //ShowDocumentsCommand = new RelayCommand(async _ =>
            //{
            //    CurrentTabView = DocumentVM;
            //    await Task.CompletedTask;
            //});

            // Стартовая вкладка
            CurrentTabView = AboutMuseumVM;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}