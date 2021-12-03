using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpContentDropManager : MpSingleton<MpContentDropManager> {
        #region Private Variables

        private List<MpIContentDropTarget> _dropTargets {
            get {
                List<MpIContentDropTarget> dtl = new List<MpIContentDropTarget>();

                var clvl = Application.Current.MainWindow.GetVisualDescendents<MpContentListView>();
                dtl.AddRange(clvl.Select(x => x.ContentListDropBehavior).Where(x => x.IsEnabled).ToList());

                var rtbvl = Application.Current.MainWindow.GetVisualDescendents<MpRtbView>();
                dtl.AddRange(rtbvl.Select(x => x.RtbViewDropBehavior).Where(x => x.IsEnabled).ToList());

                dtl.Add(Application.Current.MainWindow.GetVisualDescendent<MpClipTrayView>().ClipTrayDropBehavior);

                dtl.Add(Application.Current.MainWindow.GetVisualDescendent<MpExternalDropView>().ExternalDropBehavior);

                return dtl;
            }
        }

        private DispatcherTimer _autoScrollTimer;

        #endregion

        #region Properties

        #region State

        public int DropPriority { get; private set; } = -1;

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

        public MpIContentDropTarget Select(object dragData) {
            foreach (var dt in _dropTargets.Where(x=>x.IsEnabled)) {
                if (!dt.IsDragDataValid(dragData)) {
                    continue;
                }
                dt.DropIdx = dt.GetDropTargetRectIdx();
                if (dt.DropIdx >= 0) {
                    DropPriority = dt.DropPriority;
                    return dt;
                }
            }
            DropPriority = -1;
            return null;
        }

        public void StartDrag() {
            IsDragAndDrop = true;
            _autoScrollTimer.Start();
        }

        public void StopDrag() {
            IsDragAndDrop = false;
            if(DropPriority == int.MaxValue) {

            }
            DropPriority = -1;
            _autoScrollTimer.Stop(); 
            _dropTargets.ForEach(x => x.Reset());
        }
        #endregion

        #region Private Methods

        private void _autoScrollTimer_Tick(object sender, EventArgs e) {
            _dropTargets.ForEach(x => x.UpdateAdorner());
            _dropTargets.ForEach(x => x.AutoScrollByMouse());
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
