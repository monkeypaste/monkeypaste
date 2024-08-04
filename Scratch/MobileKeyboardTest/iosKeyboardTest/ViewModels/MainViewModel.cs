using System;

namespace iosKeyboardTest;

public class MainViewModel : ViewModelBase {
    public static bool IsMockKeyboardVisible =>
         OperatingSystem.IsWindows();
    public static string ErrorText { get; set; } //= Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string Test => "Yoooo dude";
#pragma warning disable CA1822 // Mark members as static
    public static string Greeting { get; set; } = 
        "1 Welcome to Avalonia!"+Environment.NewLine+
        "2 Welcome to Avalonia!"+Environment.NewLine+
        "3 Welcome to Avalonia!"+Environment.NewLine+
        "4 Welcome to Avalonia!"+Environment.NewLine+
        "5 Welcome to Avalonia!"+Environment.NewLine+
        "6 Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
}
