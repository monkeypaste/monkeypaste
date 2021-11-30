using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentDropManager : MpSingleton<MpContentDropManager> {
        #region Private Variables

        private List<MpIContentDropTarget> _dropTargets = new List<MpIContentDropTarget>();

        private DispatcherTimer _autoScrollTimer;

        private MouseEventArgs _curCapturedMouseEvent;

        #endregion

        #region Properties

        #region State

        public MpIContentDropTarget CurrentDropTarget { get; private set; }

        public bool IsDragAndDrop { get; private set; }

        #endregion

        #endregion

        #region Init

        public MpContentDropManager() {
            _autoScrollTimer = new DispatcherTimer();
            _autoScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _autoScrollTimer.Tick += _autoScrollTimer_Tick;

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
            _curCapturedMouseEvent = e;

            MpIContentDropTarget selectedTarget = null;

            foreach (var dt in _dropTargets.Where(x=>x.IsEnabled)) {
                if (!dt.IsDragDataValid(dragData)) {
                    continue;
                }
                dt.DropIdx = dt.GetDropTargetRectIdx(_curCapturedMouseEvent);
                if (dt.DropIdx >= 0) {
                    if (selectedTarget == null) {
                        selectedTarget = dt;
                    } else {//if (dt != selectedTarget && 
                              // dt.DropPriority > selectedTarget.DropPriority) {
                        selectedTarget.DropIdx = -1;
                        selectedTarget = dt;
                    }
                }
            }
            CurrentDropTarget = selectedTarget;

            return CurrentDropTarget;
        }

        public void StartDrag() {
            IsDragAndDrop = true;
            _autoScrollTimer.Start();
        }

        public void StopDrag() {
            IsDragAndDrop = false;
            _autoScrollTimer.Stop(); 
            _dropTargets.ForEach(x => x.Reset());
        }
        #endregion

        #region Private Methods

        private void _autoScrollTimer_Tick(object sender, EventArgs e) {
            _dropTargets.ForEach(x => x.UpdateAdorner());
            _dropTargets.ForEach(x => x.AutoScrollByMouse(_curCapturedMouseEvent));
        }

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.JumpToIdxCompleted:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.TrayScrollChanged:
                    _dropTargets.ForEach(x => x.UpdateAdorner());
                    break;
            }
        }


        #endregion
    }

}
