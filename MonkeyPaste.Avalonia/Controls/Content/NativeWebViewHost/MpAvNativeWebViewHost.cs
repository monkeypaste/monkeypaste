using Avalonia.Controls;
using Avalonia.Platform;
using Gdk;
using MonkeyPaste.Common;
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
                        MpAvCefNetApplication.GetEditorPath().ToFileSystemUriFromPath());

            }
            if (Interop != null &&
                PlatformHandle != null) {
                Interop.Bind(this);
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
