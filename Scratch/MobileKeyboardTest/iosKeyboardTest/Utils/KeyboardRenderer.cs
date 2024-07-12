using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System;
using System.Linq;
using Size = Avalonia.Size;

namespace iosKeyboardTest {
    public static class KeyboardRenderer {
        public static Control KeyboardView { get; set; }
        public static byte[] GetKeyboardImageBytes(double screenScale) {
            if (KeyboardView is not { } rs ||
                rs.GetVisualDescendants().OfType<TextBlock>() is not { } tbl) {
                return null;
            }
            foreach(var tb in tbl) {
                tb.InvalidateVisual();
            }

            return RenderHelpers.RenderToByteArray(rs, screenScale);
        }
        public static void SetError(string msg) {
            if(KeyboardView is { } kbv &&
                kbv.DataContext is KeyboardViewModel kbvm) {
                kbvm.SetError(msg);
            }
        }
        public static void Init(IKeyboardInputConnection conn, Size scaledDesiredSize, double scale, out Size unscaledActualSize) {
            //if(!Dispatcher.UIThread.CheckAccess()) {
            //    Size tempActualSize = default;
            //    Dispatcher.UIThread.Post(() => {
            //        Init(conn, scaledDesiredSize, scale, out tempActualSize);
            //    });
            //    unscaledActualSize = tempActualSize;
            //    return;
            //}
            //var kbv = KeyboardViewModel.CreateKeyboardView(conn, scaledDesiredSize, scale, out unscaledActualSize);
            var kbv = KeyboardBuilder.Build(conn, scaledDesiredSize, scale, out unscaledActualSize);

            if(OperatingSystem.IsWindows()) {
                //var hidden_window = new Window() {
                //    SizeToContent = SizeToContent.WidthAndHeight,
                //    ShowInTaskbar = false,
                //    WindowState = WindowState.Minimized,
                //    SystemDecorations = SystemDecorations.None,
                //    Content = kbv
                //};
                //hidden_window.Show();
            } else {
                if(Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime lt &&
                    lt.MainView is MainView mv && 
                    mv.ContainerCanvas is { } canvas) {
                    if(canvas.Children.OfType<KeyboardView>() is { } kvl) {
                        // remove any existing keyboards
                        foreach(var kv in kvl) {
                            canvas.Children.Remove(kv);
                        }
                    }
                    canvas.Children.Add(kbv);
                    // position keyboard sufficiently outside of screen
                    Canvas.SetTop(kbv, Math.Max(mv.Width, mv.Height) + 1_000);
                }
            }

            if (conn is not IHeadlessRender hrd) {
                return;
            }
            hrd.OnPointerChanged += (s, e) => {
                if(kbv.DataContext is not KeyboardViewModel kbvm) {
                    return;
                }
                kbvm.SetPointerLocation(e);
            };
        }
    }
}
