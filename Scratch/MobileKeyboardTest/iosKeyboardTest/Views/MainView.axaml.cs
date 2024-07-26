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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace iosKeyboardTest;

public partial class MainView : UserControl
{
    public static bool show_windowless_kb = false;
    static IKeyboardInputConnection _conn;
    public static void ForceInputConn(IKeyboardInputConnection conn) {
        _conn = conn;
    }
    public Canvas OuterCanvas =>
        ContainerCanvas;

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

        kbmvm.SetDesiredSize(KeyboardViewModel.GetTotalSizeByScreenSize(this.Bounds.Size, kbmvm.KeyboardFlags.HasFlag(KeyboardFlags.Portrait)));
        kbv.Width = kbmvm.TotalWidth;
        kbv.Height = kbmvm.TotalHeight;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        KeyboardViewModel kbvm = null;

        OrientationButton.Click += (s, e) => {
            if(TopLevel.GetTopLevel(this) is not Window w) {
                return;
            }
            if (w.Width > w.Height) {
                kbvm.KeyboardFlags &= ~KeyboardFlags.Landscape;
                kbvm.KeyboardFlags |= KeyboardFlags.Portrait;
            } else {
                kbvm.KeyboardFlags &= ~KeyboardFlags.Portrait;
                kbvm.KeyboardFlags |= KeyboardFlags.Landscape;
            }
            double temp = w.Width;
            w.Width = w.Height;
            w.Height = temp;
            w.InvalidateArrange();
            w.InvalidateMeasure();
            w.InvalidateVisual();
            OnBoundsChanged();
            kbvm.Init(kbvm.KeyboardFlags);
        };

        TestButton.Click += (s, e) => {
            TestTextBox.Text = "Welcome to Avalonia!" + Environment.NewLine + "Welcome to Avalonia!" + Environment.NewLine + "Welcome to Avalonia!" + Environment.NewLine + "Welcome to Avalonia!" + Environment.NewLine + "Welcome to Avalonia!" + Environment.NewLine + "Welcome to Avalonia!";
            //KeyboardPalette.PrintPalette();

            if (kbvm != null) {
                kbvm.UpdateKeyboardState();
                if(kbvm.IsNumPadLayout) {
                    kbvm.KeyboardFlags &= ~KeyboardFlags.Numbers;
                    kbvm.KeyboardFlags |= KeyboardFlags.FreeText;
                } else {
                    kbvm.KeyboardFlags &= ~KeyboardFlags.FreeText;
                    kbvm.KeyboardFlags |= KeyboardFlags.Numbers;
                }

                kbvm.Init(kbvm.KeyboardFlags);
            }
            Touches.Clear();
            if(!show_windowless_kb) {
                return;
            }

            var rect = new Rect(0, 0, 1000, 300);         
            var test = KeyboardBuilder.Build(null, new Size(1000, 300), 2.25, out _);
            test.Measure(rect.Size);
            test.Arrange(rect);
            test.UpdateLayout();
            test.InvalidateVisual();
            RenderHelpers.RenderToFile(test, @"C:\Users\tkefauver\Desktop\test1.png");
        };

        if(MainViewModel.IsMockKeyboardVisible) {
            double scale = 1;
            if (OperatingSystem.IsWindows() &&
            TopLevel.GetTopLevel(this) is Window w) {
                scale = w.DesktopScaling;
            }
            Control ctrl_to_add = null;
            Control kbv = null;
            //show_windowless_kb = false;
            if(show_windowless_kb) {
                kbv = KeyboardBuilder.Build(_conn, KeyboardViewModel.GetTotalSizeByScreenSize(this.Bounds.Size, kbvm.KeyboardFlags.HasFlag(KeyboardFlags.Portrait)), scale, out _);
                kbvm = kbv.DataContext as KeyboardViewModel;
                //if(_conn is IKeyboardInputConnection_desktop) {
                //    var hidden_window = new Window() {
                //        SizeToContent = SizeToContent.WidthAndHeight,
                //        ShowInTaskbar = false,
                //        WindowState = WindowState.Minimized,
                //        SystemDecorations = SystemDecorations.None,
                //        Content = kbv
                //    };
                //    hidden_window.Show();
                //} else {

                //}

                var HeadlessKeyboardImage = new Image();

                var bg_border = new Viewbox() {
                    Width = kbvm.TotalWidth,
                    Height = kbvm.TotalHeight,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                    Child = new Border {
                        Background = Brushes.MidnightBlue,
                        Child = HeadlessKeyboardImage,
                    }
                };
                ctrl_to_add = bg_border;
            } else {
                kbv = KeyboardFactory.CreateKeyboardView(_conn, KeyboardViewModel.GetTotalSizeByScreenSize(this.Bounds.Size, true), scale, out _);
                kbvm = kbv.DataContext as KeyboardViewModel;
                ctrl_to_add = kbv;
            }

            ctrl_to_add.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

            OuterPanel.Children.Add(ctrl_to_add);
            Grid.SetRow(ctrl_to_add, 3);

            if (_conn is IKeyboardInputConnection_desktop icd) {
                icd.SetKeyboardInputSource(this.TestTextBox);                
            }

            if(_conn is IHeadLessRender_desktop hrd && show_windowless_kb) {
                hrd.SetRenderSource(kbv);
                hrd.SetPointerInputSource(ctrl_to_add);
                //scale = 1;
                var render_timer = new DispatcherTimer() {
                    Interval = TimeSpan.FromMilliseconds(1000d / 120d),
                    IsEnabled = true
                };

                void RenderKeyboard() {
                    if(!show_windowless_kb) {
                        return;
                    }
                    if (KeyboardRenderer.GetKeyboardImageBytes(scale) is not { } bytes) {
                        return;
                    }
                    if (ctrl_to_add.GetVisualDescendants().OfType<Image>().FirstOrDefault() is not { } img) {
                        return;
                    }
                    img.Source = RenderHelpers.RenderToBitmap(bytes);
                }

                render_timer.Tick += (s, e) => {
                    RenderKeyboard();
                };
                RenderKeyboard();

                hrd.OnPointerChanged += (s, e) => {
                    kbvm.SetPointerLocation(e);
                    //kbv.BindingContext.Test1Command.Execute(null);
                    //HeadlessKeyboardImage.Source = hrd.RenderToBitmap(scale);
                    //RenderHelpers.RenderToFile(kbv, @"C:\Users\tkefauver\Desktop\test2.png");
                    if(e == null) {
                        
                    }
                };

                
            }
        }
        if(!OperatingSystem.IsAndroid()) {
            TestTextBox.Focus();
        }
        
    }
    
}