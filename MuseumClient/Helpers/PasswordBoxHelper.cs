using System.Windows;
using System.Windows.Controls;

namespace MuseumClient.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPassword =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject obj)
            => (string)obj.GetValue(BoundPassword);

        public static void SetBoundPassword(DependencyObject obj, string value)
            => obj.SetValue(BoundPassword, value);

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox pb)
            {
                // отписываемся чтобы избежать рекурсии
                pb.PasswordChanged -= PasswordChanged;
                if (pb.Password != (string)e.NewValue)
                    pb.Password = (string)e.NewValue;
                pb.PasswordChanged += PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                SetBoundPassword(pb, pb.Password);
            }
        }
    }
}