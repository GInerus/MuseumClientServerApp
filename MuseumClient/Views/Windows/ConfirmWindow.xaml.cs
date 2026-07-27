using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MuseumClient.Views.Windows
{
    public partial class ConfirmWindow : Window
    {
        public string TitleText { get; }

        public string YesButtonText { get; }

        public string NoButtonText { get; }

        public Brush YesButtonBrush { get; }

        public bool Result { get; private set; }

        public string Message { get; }


        public ConfirmWindow(
            string title,
            string message,
            string yesButtonText = "Да",
            string noButtonText = "Нет",
            Brush? yesButtonBrush = null)
        {
            InitializeComponent();

            TitleText = title;
            Message = message;
            YesButtonText = yesButtonText;
            NoButtonText = noButtonText;
            YesButtonBrush = yesButtonBrush ?? Brushes.DodgerBlue;

            DataContext = this;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}