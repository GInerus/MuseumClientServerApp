using System.ComponentModel;
using System.Runtime.CompilerServices;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _welcomeMessage;
    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set { _welcomeMessage = value; OnPropertyChanged(); }
    }

    public MainViewModel(string userType)
    {
        WelcomeMessage = $"Вы успешно вошли как {userType}!";
    }

    protected void OnPropertyChanged([CallerMemberName] string? propName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}