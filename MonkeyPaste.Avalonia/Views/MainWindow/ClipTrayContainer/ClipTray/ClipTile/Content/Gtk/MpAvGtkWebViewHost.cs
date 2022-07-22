using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using MonkeyPaste.Common.Avalonia;
using SharpWebview;
using SharpWebview.Content;
using System;
using PropertyChanged;
using Avalonia.Metadata;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvGtkWebViewHost : NativeControlHost {
        private readonly Window _hostedWindow = CreateHostedWindow();
        private Webview _webview;

        public static readonly StyledProperty<IControl> ChildProperty =
            AvaloniaProperty.Register<MpAvGtkWebViewHost, IControl>(nameof(Child));

        [Content]
        public IControl Child {
            get => GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        static MpAvGtkWebViewHost() {
            ChildProperty.Changed.AddClassHandler<MpAvGtkWebViewHost>((host, e) => host.ChildChanged(e));
        }

        public MpAvGtkWebViewHost() {
            InitWindow(_hostedWindow);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            if (OperatingSystem.IsLinux()) {
                return CreateLinux();
            }
            return null;
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            if (OperatingSystem.IsLinux()) {
                DestroyLinux(control);
            } else {
                base.DestroyNativeControlCore(control);
            }
        }


        private void ChildChanged(AvaloniaPropertyChangedEventArgs e) {
            if (e.OldValue is Control oldChild) {
                ((ISetLogicalParent)oldChild).SetParent(null);
                LogicalChildren.Clear();
                _hostedWindow.Content = null;
            }

            if (e.NewValue is Control newChild) {
                ((ISetLogicalParent)newChild).SetParent(this);
                _hostedWindow.Content = newChild;
                LogicalChildren.Add(newChild);
            }
        }

        private void InitWindow(Window window) {
            _hostedWindow[!DataContextProperty] = this[!DataContextProperty];

            window.RaiseEvent(new RoutedEventArgs(Window.WindowOpenedEvent));
            window.BeginInit();
            window.EndInit();
            window.IsVisible = true;
            window.LayoutManager.ExecuteInitialLayoutPass();
            window.Renderer.Start();

            window.AttachedToVisualTree += (s,e) =>{
                _webview
                        // Set the title of the application window
                        .SetTitle("The Hitchhicker")
                        // Set the start size of the window                
                        .SetSize(1024, 768, WebviewHint.None)
                        // Set the minimum size of the window
                        .SetSize(800, 600, WebviewHint.Min)
                        // This script gets executed after navigating to the url
                        //.InitScript("window.x = 42;")
                        // Bind a c# function to the webview - Accessible with the name "evalTest"
                        .Bind("evalTest", (id, req) => {
                        // Executes the javascript on the webview
                        _webview.Evaluate("console.log('The anwser is ' + window.x);");
                        // And returns a successful promise result to the javascript function, which executed the 'evalTest'
                        _webview.Return(id, RPCResult.Success, "{ result: 'We always knew it!' }");
                        })
                        // Navigate to this url on start
                        .Navigate(new UrlContent("https://en.wikipedia.org/wiki/The_Hitchhiker%27s_Guide_to_the_Galaxy_(novel)"))
                        // Run the webview loop
                        .Run();
            };
        }

        private static Window CreateHostedWindow() {
            return new Window() {
                SystemDecorations = SystemDecorations.None,
                // Not setting this property to 'true' causes one more transparent window (this window)
                // to appear in window switcher when you press Alt + Tab in Windows.
                ShowInTaskbar = true,
                // Do not activate window automatically, it will be activated manually on click or on focus.
                ShowActivated = false,
                // Window shouldn't be resizable it should automatically fill given area.
                CanResize = false
            };
        }



        private IPlatformHandle CreateLinux() {
            var webViewHandle = Glib.RunOnGlibThreadAsync(() =>
            {
                _webview = new SharpWebview.Webview(true, true);                
                return new MpAvWebViewHandle(_webview);
            }).Result;
            return webViewHandle;            
        }

        void DestroyLinux(IPlatformHandle handle) {
            ((MpAvWebViewHandle)handle).Dispose();
        }

    }
}
