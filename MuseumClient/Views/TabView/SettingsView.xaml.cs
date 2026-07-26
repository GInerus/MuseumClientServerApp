using MuseumClient.ViewModels;
using System.Windows.Controls;

namespace MuseumClient.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (e.OldValue is SettingsViewModel oldVm)
                    oldVm.PasswordChangeSucceeded -= ClearPasswordBoxes;

                if (e.NewValue is SettingsViewModel newVm)
                    newVm.PasswordChangeSucceeded += ClearPasswordBoxes;
            };
        }

        private void ClearPasswordBoxes()
        {
            OldPasswordBox.Clear();
            NewPasswordBox.Clear();
            ConfirmPasswordBox.Clear();
        }

        private void OldPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
                vm.OldPassword = OldPasswordBox.Password;
        }

        private void NewPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
                vm.NewPassword = NewPasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
                vm.ConfirmNewPassword = ConfirmPasswordBox.Password;
        }
    }
}