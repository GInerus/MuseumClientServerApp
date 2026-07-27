using MuseumClient.Views.Windows;
using System.Windows;
using System.Windows.Media;

namespace MuseumClient.Services
{
    public static class ConfirmService
    {
        // Универсальное подтверждение
        public static bool Show(string message)
        {
            var window = new ConfirmWindow(
                "Подтверждение",
                message,
                "Да",
                "Нет",
                Brushes.DodgerBlue);

            window.Owner = Application.Current.MainWindow;

            window.ShowDialog();

            return window.Result;
        }

        // Старый метод удаления оставляем как есть,
        // чтобы ничего в проекте не переписывать.
        public static bool ConfirmDelete(string itemName)
        {
            var window = new ConfirmWindow(
                "Подтверждение удаления",
                $"Вы действительно хотите удалить {itemName}?",
                "Удалить",
                "Отмена",
                Brushes.IndianRed);

            window.Owner = Application.Current.MainWindow;

            window.ShowDialog();

            return window.Result;
        }
    }
}