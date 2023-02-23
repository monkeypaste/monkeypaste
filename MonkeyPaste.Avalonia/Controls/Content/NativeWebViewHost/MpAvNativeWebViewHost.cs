using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Gdk;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIWebViewInterop {
        void SendMessage(MpAvIPlatformHandleHost nwvh, string msg);
        Task<string> SendMessageAsync(MpAvIPlatformHandleHost nwvh, string msg);

        void ReceiveMessage(string bindingName, string msg);

        void Bind(MpIWebViewBindable handler);

    }
    public interface MpIOffscreenRenderSourceHost {
        MpIOffscreenRenderSource RenderSource { get; }
    }
    public interface MpIOffscreenRenderSource {
        byte[] Buffer { get; }
        event EventHandler BufferChanged;
    }
    public interface MpIWebViewBindable {
        event EventHandler<string> OnNavigateRequest;
        void OnNavigated(string url);
        void OnRenderBufferChanged();

    }
    public interface MpIAsyncJsEvalTracker {
        int PendingEvals { get; set; }
    }
    public interface MpAvIPlatformHandleHost {
        IPlatformHandle PlatformHandle { get; }
    }
    public interface MpIWebViewNavigator {
        void Navigate(string url);
    }

    [DoNotNotify]
    public class MpAvNativeWebViewHost :
        NativeControlHost,
        MpIWebViewBindable,
        MpIAsyncJsEvalTracker,
        MpAvIPlatformHandleHost,
        MpIHasDevTools {


        #region Private Variable
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

        #region MpIAsyncJsEvalTracker Implementation

        public int PendingEvals { get; set; }

        #endregion

        #region MpIWebViewEventHandler Implementation
        public event EventHandler<string> OnNavigateRequest;
        public virtual void OnNavigated(string url) {
            Dispatcher.UIThread.Post(this.InvalidateVisual);
        }

        public void OnRenderBufferChanged() {
            Dispatcher.UIThread.Post(this.InvalidateVisual);

        }
        #endregion

        #region MpIHasDevTools Implementation
        public void ShowDevTools() { }
        #endregion

        #endregion

        #region Properties

        #region State
        public IPlatformHandle PlatformHandle { get; private set; }

        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public void Navigate(string url) {
            OnNavigateRequest?.Invoke(this, url);
        }

        public override void Render(global::Avalonia.Media.DrawingContext context) {
            //base.Render(context);
            if (PlatformHandle is MpIOffscreenRenderSourceHost osrsh &&
                    osrsh.RenderSource is MpIOffscreenRenderSource osrs &&
                    osrs.Buffer != null &&
                    osrs.Buffer.Length > 0) {

                context.DrawImage(osrs.Buffer.ToAvBitmap(), this.Bounds);
            } else {
                base.Render(context);
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
                        MpAvClipTrayViewModel.EditorUri);

            }
            if (Interop != null &&
                PlatformHandle != null) {
                Interop.Bind(this);
                Navigate(MpAvClipTrayViewModel.EditorUri);
            }

            return PlatformHandle;
        }

        protected override void DestroyNativeControlCore(IPlatformHandle control) {
            base.DestroyNativeControlCore(control);
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
