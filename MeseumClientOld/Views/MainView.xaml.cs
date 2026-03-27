using MeseumClient.Services;
using MeseumClient.ViewModels;
using System.Windows.Controls;

namespace MeseumClient.Views
{
    public partial class MainView : UserControl
    {
        public MainView(SessionService sessionService)
        {
            InitializeComponent();
            DataContext = new MainViewModel(sessionService);
        }
    }
}