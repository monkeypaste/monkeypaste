using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using System;

namespace iosKeyboardTest;

public partial class MainView : UserControl
{
    public static bool IsMainViewLoaded { get; private set; }
    public static event EventHandler OnMainViewLoaded;
    public MainView()
    {
        InitializeComponent();
    }
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        //var kbv = KeyboardViewModel.CreateKeyboardView(null, this.Bounds.Size);
        //kbv.Loaded += (s, e) => {
        //    IsMainViewLoaded = true;
        //    OnMainViewLoaded?.Invoke(this, EventArgs.Empty);
        //};
        //OuterPanel.Children.Add(kbv);

    }
}