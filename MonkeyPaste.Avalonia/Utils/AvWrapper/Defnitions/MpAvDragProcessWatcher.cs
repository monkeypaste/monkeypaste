using Avalonia.Threading;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvDragProcessWatcher :
        MpIDragProcessWatcher {
        #region Private Variable

        private DispatcherPriority _watcherPriority = DispatcherPriority.Normal;
        #endregion

        #region Constants

        public const double MIN_DRAG_DIST = 10;

        #endregion

        #region Statics
        public static void Init() {
            // called in bootstrapper after shortcut collection created
            Mp.Services.DragProcessWatcher.StartWatcher();
        }

        #endregion

        #region Interfaces

        #region MpIDragProcessWatcher Implementation
        public MpPortableProcessInfo DragProcess { get; private set; }

        public void StartWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragBegin += Sccvm_OnGlobalDragBegin;
        }

        public void StopWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragBegin -= Sccvm_OnGlobalDragBegin;
        }

        public void Reset() {
            DragProcess = null;
        }
        #endregion

        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void Sccvm_OnGlobalDragBegin(object sender, System.EventArgs e) {
            Dispatcher.UIThread.Post(() => {
                DragProcess = Mp.Services.ProcessWatcher.LastProcessInfo;
            }, _watcherPriority);
        }

        #endregion

        #region Commands
        #endregion
    }
}

