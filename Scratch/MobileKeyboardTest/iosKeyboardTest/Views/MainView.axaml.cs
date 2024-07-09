using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Linq;

namespace iosKeyboardTest;

public partial class MainView : UserControl
{
    static IKeyboardInputConnection _conn;
    public static void ForceInputConn(IKeyboardInputConnection conn) {
        _conn = conn;
    }

    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        double scale = 1;
        if(OperatingSystem.IsWindows() &&
            TopLevel.GetTopLevel(this) is Window w) {
            scale = w.DesktopScaling;
            
        }
        var kbv = KeyboardMainViewModel.CreateKeyboardView(_conn, this.Bounds.Size, scale, out _);
        OuterPanel.Children.Add(kbv);

        if (_conn is IKeyboardInputConnection_desktop icd) {
            icd.SetInputSource(this.TestTextBox);
        }
        TestTextBox.Focus();
    }
    
}