namespace iosKeyboardTest;

public class MainViewModel : ViewModelBase
{
    public static string ErrorText { get; set; } = "NO ERRORS";
    public static string Test => "Yoooo dude";
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
}
