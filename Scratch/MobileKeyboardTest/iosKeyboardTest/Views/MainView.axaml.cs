using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
            kbv.DataContext is not KeyboardViewModel kbmvm) {
            return;
        }

        kbmvm.SetDesiredSize(KeyboardViewModel.GetTotalSizeByScreenSize(this.Bounds.Size));
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
            var kbv = KeyboardViewModel.CreateKeyboardView(_conn, KeyboardViewModel.GetTotalSizeByScreenSize(this.Bounds.Size), scale, out _);

            var hidden_window = new Window() {
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInTaskbar = false,
                WindowState = WindowState.Minimized,
                SystemDecorations = SystemDecorations.None,
                Content = kbv
            };
            hidden_window.Show();

            var HeadlessKeyboardImage = new Image();

            var bg_border = new Viewbox() {
                Width = kbv.BindingContext.TotalWidth,
                Height = kbv.BindingContext.TotalHeight,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Child = new Border {
                    Background = Brushes.MidnightBlue,
                    Child = HeadlessKeyboardImage,
                }
            };

            OuterPanel.Children.Add(bg_border);
            Grid.SetRow(bg_border, 3);

            if (_conn is IKeyboardInputConnection_desktop icd) {
                icd.SetKeyboardInputSource(this.TestTextBox);                
            }
            if(_conn is IHeadLessRender_desktop hrd) {
                hrd.SetRenderSource(kbv);
                hrd.SetPointerInputSource(bg_border);
                scale = 1;
                var render_timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(1000d / 120d),
                    IsEnabled = true
                };

                render_timer.Tick += (s, e) => {
                    scale = 2.25;
                    HeadlessKeyboardImage.Source = hrd.RenderToBitmap(scale);
                };

                HeadlessKeyboardImage.Source = hrd.RenderToBitmap(scale);

                hrd.OnPointerChanged += (s, e) => {
                    kbv.BindingContext.SetPointerLocation(e);
                    //HeadlessKeyboardImage.Source = hrd.RenderToBitmap(scale);
                    //RenderHelpers.RenderToFile(kbv, @"C:\Users\tkefauver\Desktop\test2.png");
                    if(e == null) {
                        
                    }
                };

                
            }
        }
        TestTextBox.Focus();
    }
    
}