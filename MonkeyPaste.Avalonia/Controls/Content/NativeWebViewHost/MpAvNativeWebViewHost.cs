using Android.Content;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIWebViewInterop {
        void SendMessage(MpAvIPlatformHandleHost nwvh, string msg);

        void ReceiveMessage(string bindingName, string msg);

        void Bind(MpIWebViewBindable handler);

    }
    public interface MpIOffscreenRenderSourceHost {
        MpIOffscreenRenderSource RenderSource { get; }
    }
    public interface MpIOffscreenRenderSource {
        byte[] Buffer { get; }
    }
    public interface MpIWebViewBindable {
        event EventHandler<string> OnNavigateRequest;
        void OnNavigated(string url);
    }
    public interface MpIAsyncJsEvalTracker {
        int PendingEvals { get; set; }
    }
    public interface MpAvIPlatformHandleHost {
        IPlatformHandle PlatformHandle { get; }
    }
    public interface MpIHaveLog {
        void AppendLine(string line);
        string LogText { get; }
    }
    public interface MpIWebViewNavigator {
        void Navigate(string url);
    }

    public interface MpIEmbedHost { }
    public interface MpIWebViewHost : MpIEmbedHost {
        string HostGuid { get; }
        void Render();
        MpAvIWebViewBindingResponseHandler BindingHandler { get; }
    }
    public interface MpIResizableControl {
        void SetBounds(MpRect bounds);
    }
    [DoNotNotify]
    public abstract class MpAvNativeWebViewHost :
        NativeControlHost,
        MpIWebViewHost,
        MpIWebViewBindable,
        MpIAsyncJsEvalTracker,
        MpAvIPlatformHandleHost,
        MpIHasDevTools {
        #region Private Variable
        private string _webViewGuid;
        private double _scale = 0;
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static MpAvINativeControlBuilder Implementation { get; set; }
        public static MpAvIWebViewInterop Interop =>
            Implementation as MpAvIWebViewInterop;

        static MpAvNativeWebViewHost() {

        }
        #endregion

        #region Interfaces
        #region MpIWebViewHost Implementation

        public string HostGuid {
            get {
                if (string.IsNullOrEmpty(_webViewGuid)) {
                    _webViewGuid = System.Guid.NewGuid().ToString();
                }
                return _webViewGuid;
            }
        }
        void MpIWebViewHost.Render() {
            this.Redraw();
        }


        public abstract MpAvIWebViewBindingResponseHandler BindingHandler { get; }
        #endregion

        #region MpIAsyncJsEvalTracker Implementation

        public int PendingEvals { get; set; }

        #endregion

        #region MpIWebViewBindable Implementation
        public event EventHandler<string> OnNavigateRequest;
        public virtual void OnNavigated(string url) {
            this.Redraw();
        }

        #endregion

        #region MpIHasDevTools Implementation
        public void ShowDevTools() {
            if (PlatformHandle is not MpIOffscreenRenderSourceHost osrsh ||
                    osrsh.RenderSource is not MpIHaveLog hl) {
                return;
            }
            Mp.Services.NotificationBuilder.ShowMessageAsync(
                title: DataContext == null ? "NULL" : DataContext.ToString(),
                body: hl.LogText,
                maxShowTimeMs: -1).FireAndForgetSafeAsync();

        }
        #endregion

        #endregion

        #region Properties

        #region State
        public IPlatformHandle PlatformHandle { get; private set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvNativeWebViewHost() : base() {
            this.GetObservable(BoundsProperty).Subscribe(value => OnBoundsChanged());
            this.EffectiveViewportChanged += (s, e) => OnBoundsChanged();
        }
        #endregion

        #region Public Methods
        public void Navigate(string url) {
            OnNavigateRequest?.Invoke(this, url);
        }

        public override void Render(global::Avalonia.Media.DrawingContext context) {
            if (!MpAvMainWindowViewModel.Instance.IsMainWindowActive) {
                return;
            }
            if (PlatformHandle is MpIOffscreenRenderSourceHost osrsh &&
                    osrsh.RenderSource is MpIOffscreenRenderSource osrs &&
                    osrs.Buffer != null &&
                    osrs.Buffer.Length > 0) {
                if (_scale == 0) {
                    _scale = MpDeviceWrapper.Instance.ScreenInfoCollection.Screens.FirstOrDefault().Scaling;
                }
                var bmp = osrs.Buffer.ToAvBitmap();
                //var test1 = bmp.PixelSize;
                //var test2 = bmp.Size;
                //var test3 = this.Bounds.Size;
                //bmp = bmp.Resize(this.Bounds.Size.ToPortableSize());
                //var test4 = bmp.Size;
                //var test5 = bmp.PixelSize;
                //var source_rect = new MpRect(bmp.Size.ToPortableSize());//.ToPortablePoint() * _scale).ToPortableSize());
                //var dest_rect = this.Bounds;
                //context.DrawImage(bmp, source_rect.ToAvRect(), dest_rect);
                context.DrawImage(bmp, new Rect(bmp.Size), this.Bounds);
            } else {
                //base.Render(context);
            }
        }
        #endregion

        #region Protected Methods
        protected int PendingEvalCount() {
            return PendingEvals;
        }

        protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent) {
            if (Implementation == null) {
                PlatformHandle = base.CreateNativeControlCore(parent);
            } else {
                PlatformHandle =
                    Implementation.Build(
                        parent,
                        () => base.CreateNativeControlCore(parent),
                        this);

            }
            if (Interop != null &&
                PlatformHandle != null) {
                Interop.Bind(this);
                Navigate(Mp.Services.PlatformInfo.EditorPath.ToFileSystemUriFromPath());
            }

            return PlatformHandle;
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            base.DestroyNativeControlCore(control);
        }

        //protected override void OnPointerPressed(PointerPressedEventArgs e) {
        //    Dispatcher.UIThread.Post(() => {
        //        base.OnPointerPressed(e);
        //        if (DataContext is MpAvClipTileViewModel ctvm) {
        //            MpAvClipTrayViewModel.Instance.SelectClipTileCommand.Execute(ctvm);
        //        }
        //    });
        //}


        #endregion

        #region Private Methods
        private void OnBoundsChanged() {
            if (PlatformHandle is not MpIResizableControl rc) {
                return;
            }
            rc.SetBounds(Bounds.ToPortableRect());
        }
        #endregion

        #region Commands
        #endregion
    }
}
