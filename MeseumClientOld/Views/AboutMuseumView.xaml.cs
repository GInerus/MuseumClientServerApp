using MeseumClient.ViewModels;
using System.Windows.Controls;

namespace MeseumClient.Views
{
    public partial class AboutMuseumView : UserControl
    {
        public AboutMuseumViewModel ViewModel { get; }

        public AboutMuseumView(string token = "")
        {
            InitializeComponent();
            ViewModel = new AboutMuseumViewModel(token);
            DataContext = ViewModel;
        }
    }
}