using Avalonia.Threading;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvDropProcessWatcher :
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
            Mp.Services.DropProcessWatcher.StartWatcher();
        }

        #endregion

        #region Interfaces

        #region MpIDropProcessWatcher Implementation
        public MpPortableProcessInfo DropProcess { get; private set; }

        public void StartWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragEnd += Sccvm_OnGlobalDragEnd;
        }

        public void StopWatcher() {
            var sccvm = MpAvShortcutCollectionViewModel.Instance;
            if (sccvm == null) {
                return;
            }
            sccvm.OnGlobalDragEnd -= Sccvm_OnGlobalDragEnd;
        }

        public void Reset() {
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

