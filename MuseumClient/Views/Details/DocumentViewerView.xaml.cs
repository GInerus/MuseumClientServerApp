using System.Windows;
using System.Windows.Controls;

namespace MuseumClient.Views.Details
{
    public partial class DocumentViewerView : UserControl
    {
        public DocumentViewerView()
        {
            InitializeComponent();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PdfView.CoreWebView2?.ShowPrintUI();
            }
            catch
            {
                MessageBox.Show(
                    "Не удалось открыть диалог печати. Попробуйте ещё раз после полной загрузки документа.",
                    "Печать",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}