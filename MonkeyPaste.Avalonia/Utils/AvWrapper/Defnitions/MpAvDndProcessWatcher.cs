using Avalonia.Threading;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvDndProcessWatcher :
        MpIDragProcessWatcher,
        MpIDropProcessWatcher {
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
            Mp.Services.DropProcessWatcher.StartWatcher();
        }

        #endregion

        #region Interfaces

        #region MpIDragProcessWatcher Implementation
        public MpPortableProcessInfo DragProcess { get; private set; }

        void MpIDragProcessWatcher.StartWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragBegin += Sccvm_OnGlobalDragBegin;
        }

        void MpIDragProcessWatcher.StopWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragBegin -= Sccvm_OnGlobalDragBegin;
        }

        void MpIDragProcessWatcher.Reset() {
            DragProcess = null;
        }
        #endregion

        #region MpIDropProcessWatcher Implementation
        public MpPortableProcessInfo DropProcess { get; private set; }

        void MpIDropProcessWatcher.StartWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragEnd += Sccvm_OnGlobalDragEnd;
        }

        void MpIDropProcessWatcher.StopWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragEnd -= Sccvm_OnGlobalDragEnd;
        }

        void MpIDropProcessWatcher.Reset() {
            DropProcess = null;
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
        private void Sccvm_OnGlobalDragEnd(object sender, System.EventArgs e) {
            Dispatcher.UIThread.Post(() => {
                DropProcess = Mp.Services.ProcessWatcher.LastProcessInfo;
            }, _watcherPriority);
        }
        #endregion

        #region Commands
        #endregion
    }
}

