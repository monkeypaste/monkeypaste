using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using MonkeyPaste.Common.Avalonia;
using System;
using PropertyChanged;
using Avalonia.Metadata;
using WebKit;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvGtkWebViewHost2 : NativeControlHost {
        
    }
    [DoNotNotify]
    public class MpAvGtkWebViewHost : NativeControlHost {
        private IPlatformHandle _webViewHandle;

        private readonly Window _hostedWindow = CreateHostedWindow();
        private WebView _webview;

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

        private IPlatformHandle CreateLinux() {
            var webViewHandle = Glib.RunOnGlibThreadAsync(() => {
                _webview = new WebView() {
                    // Vexpand = true,                    
                    // Hexpand = true,
                    // HeightRequest = 300,
                    // WidthRequest = 300                              
                };  
                    
                GtkApi.gtk_widget_realize(_webview.Handle);

                var xid = GtkApi.gdk_x11_window_get_xid(GtkApi.gtk_widget_get_window(_webview.Handle));
                GtkApi.gtk_window_present(_webview.Handle);

                _webview.LoadUri("https://www.google.com");
                return new MpAvGtkWidgetHandle(_webview.Handle, xid);
            }).Result;

            return webViewHandle;            
        }

        void DestroyLinux(IPlatformHandle handle) {
            ((MpAvGtkWidgetHandle)handle).Dispose();
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            if (OperatingSystem.IsLinux()) {
                var handle = CreateLinux();
                return handle;
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
    }

    [DoNotNotify]
    class MpAvGtkWidgetHandle : IPlatformHandle, IDisposable {
        //private WebView _webview;
        private IntPtr _widget;
        
        public IntPtr Handle { get; }
        public string HandleDescriptor => "XID";
        
        public MpAvGtkWidgetHandle(IntPtr widget, IntPtr xid) {
            _widget = widget;
            Handle = xid;
        }

        public void Destroy() {
            Glib.RunOnGlibThreadAsync(() =>
            {
                GtkApi.gtk_widget_destroy(_widget);
                return 0;
            }).Wait();
        }

        public void Dispose() {
            Destroy();
        }
    }
}
