using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.WebKit;
using PropertyChanged;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Controls.Platform;
using Avalonia.Metadata;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvMacWebView : NativeControlHost {
        /*
         var window = new NSWindow(new CGRect(0, 0, 100, 100), NSWindowStyle.Borderless, NSBackingStore.Buffered, false);
            var webview = new WebView();
            webview.Frame = new CGRect(0, 0, 100, 100);
            window.ContentView = webview;
            webview.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl("https://google.com")));
        */
        public MpAvMacWebView() {
        }
    }
    [DoNotNotify]
    public class EmbedSample : NativeControlHost {
        IPlatformHandle CreateOSX(IPlatformHandle parent) {
            // Note: We are using MonoMac for example purposes
            // It shouldn't be used in production apps
            MpAvMacHelpers.EnsureInitialized();

            var webView = new WebView();

            Dispatcher.UIThread.Post(() =>
            {
                webView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl("https://www.google.com/")));
            });

            return new MacOSViewHandle(webView);
        }

        void DestroyOSX(IPlatformHandle handle) {
            ((MacOSViewHandle)handle).Dispose();
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return CreateOSX(parent);
            return base.CreateNativeControlCore(parent);
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                DestroyOSX(control);
            else
                base.DestroyNativeControlCore(control);
        }
    }

    [DoNotNotify]
    class MacOSViewHandle : IPlatformHandle, IDisposable {
        private NSView _view;

        public MacOSViewHandle(NSView view) {
            _view = view;
        }

        public IntPtr Handle => _view?.Handle ?? IntPtr.Zero;
        public string HandleDescriptor => "NSView";

        public void Dispose() {
            _view.Dispose();
            _view = null;
        }
    }

    [DoNotNotify]
    public class ExtendedNativeControlHost : NativeControlHost {
        private readonly Window _hostedWindow = CreateHostedWindow();

        public static readonly StyledProperty<IControl> ChildProperty =
            AvaloniaProperty.Register<ExtendedNativeControlHost, IControl>(nameof(Child));

        [Content]
        public IControl Child {
            get => GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        static ExtendedNativeControlHost() {
            ChildProperty.Changed.AddClassHandler<ExtendedNativeControlHost>((host, e) => host.ChildChanged(e));
        }

        public ExtendedNativeControlHost() {
            InitWindow(_hostedWindow);
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            if (_hostedWindow.PlatformImpl.Handle is IMacOSTopLevelPlatformHandle macOSTopLevelPlatformHandle) {
                return new PlatformHandle(macOSTopLevelPlatformHandle.NSView, nameof(macOSTopLevelPlatformHandle.NSView));
            } 


            return _hostedWindow.PlatformImpl.Handle;
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            if (control is INativeControlHostDestroyableControlHandle)
                base.DestroyNativeControlCore(control);
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
}

