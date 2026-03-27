using System.Windows.Controls;
using MeseumClient.ViewModels;

namespace MeseumClient.Views
{
    public partial class AboutMuseumView : UserControl
    {
        public AboutMuseumView(string token)
        {
            InitializeComponent();
            DataContext = new AboutMuseumViewModel(token);
        }
    }
}