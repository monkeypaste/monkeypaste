using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentDropManager : MpSingleton<MpContentDropManager> {
        #region Private Variables

        private List<MpIContentDropTarget> _dropTargets = new List<MpIContentDropTarget>();

        #endregion

        #region Init

        public MpContentDropManager() {
            MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
        }

        #endregion

        #region Public Methods

        public int Register(MpIContentDropTarget dropBehavior) {
            _dropTargets.Add(dropBehavior);
            return _dropTargets.Count;
        }

        public void Unregister(MpIContentDropTarget dropBehavior) {
            var dropBehaviorToRemove = _dropTargets.FirstOrDefault(x => x.TargetId == dropBehavior.TargetId);
            if (dropBehaviorToRemove != null) {
                _dropTargets.Remove(dropBehaviorToRemove);
            } else {
                MpConsole.WriteLine("Warning! Cannot identify dropBehavior to remove so ignoring");
                return;
            }
        }

        public MpIContentDropTarget Select(object dragData, MouseEventArgs e) {
            MpIContentDropTarget selectedTarget = null;

            foreach (var dt in _dropTargets) {
                if (!dt.IsDragDataValid(dragData)) {
                    continue;
                }
                if (dt.GetDropTargetRectIdx(e) >= 0) {
                    if (selectedTarget == null) {
                        selectedTarget = dt;
                    } else if (dt.DropPriority > selectedTarget.DropPriority) {
                        selectedTarget = dt;
                    }
                }
            }

            return selectedTarget;
        }

        #endregion

        #region Private Methods

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    _dropTargets.ForEach(x => x.UpdateAdorner());
                    break;
                case MpMessageType.ScrollChanged:
                    _dropTargets.ForEach(x => x.UpdateAdorner());
                    break;
            }
        }


        #endregion
    }

}
