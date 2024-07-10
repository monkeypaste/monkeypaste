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
        this.GetObservable(BoundsProperty).Subscribe(value => OnBoundsChanged());
        this.EffectiveViewportChanged += (s, e) => OnBoundsChanged();
    }
    void OnBoundsChanged() {
        if (this.GetVisualDescendants().OfType<KeyboardView>().FirstOrDefault() is not { } kbv || 
            kbv.DataContext is not KeyboardMainViewModel kbmvm) {
            return;
        }
        double w = this.Bounds.Width;
        double h = this.Bounds.Height * KeyboardMainViewModel.TOTAL_KEYBOARD_SCREEN_HEIGHT_RATIO;
        kbmvm.ForceSize(new Size(w, h));
        kbv.Width = kbmvm.TotalWidth;
        kbv.Height = kbmvm.TotalHeight;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        if(MainViewModel.IsMockKeyboardVisible) {
            double scale = 1;
            if (OperatingSystem.IsWindows() &&
            TopLevel.GetTopLevel(this) is Window w) {
                scale = w.DesktopScaling;

            }
            var kbv = KeyboardMainViewModel.CreateKeyboardView(_conn, this.Bounds.Size, scale, out _);
            kbv.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
            OuterPanel.Children.Add(kbv);
            Grid.SetRow(kbv, 3);

            if (_conn is IKeyboardInputConnection_desktop icd) {
                icd.SetInputSource(this.TestTextBox);
            }
        }
        TestTextBox.Focus();
    }
    
}