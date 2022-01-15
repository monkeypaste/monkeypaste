using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpDragDropManager : MpSingleton<MpDragDropManager> {
        #region Private Variables

        private const double MINIMUM_DRAG_DISTANCE = 10;

        private Point _mouseStartPosition;

        private MpIContentDropTarget _curDropTarget;

        private List<MpIContentDropTarget> _dropTargets {
            get {
                List<MpIContentDropTarget> dtl = new List<MpIContentDropTarget>();

                var clvl = Application.Current.MainWindow.GetVisualDescendents<MpContentListView>();
                dtl.AddRange(clvl.Select(x => x.ContentListDropBehavior).Where(x => x.IsEnabled).ToList());

                var rtbvl = Application.Current.MainWindow.GetVisualDescendents<MpRtbView>();
                dtl.AddRange(rtbvl.Select(x => x.RtbViewDropBehavior).Where(x => x.IsEnabled).ToList());

                dtl.Add(Application.Current.MainWindow.GetVisualDescendent<MpClipTrayView>().ClipTrayDropBehavior);

                dtl.Add((Application.Current.MainWindow as MpMainWindow).ExternalDropBehavior);

                return dtl;
            }
        }

        private DispatcherTimer _autoScrollTimer;

        #endregion

        #region Properties

        public bool IsDropValid => _curDropTarget != null;

        public bool IsDragCopy {
            get {
                if (MpShortcutCollectionViewModel.Instance == null) {
                    return false;
                }
                return MpShortcutCollectionViewModel.Instance.IsCtrlDown;
            }
        }

        public MpDropType DropType {
            get {
                if(_curDropTarget == null) {
                    return MpDropType.None;
                }
                return _curDropTarget.DropType;
            }
        }

        public bool IsDragAndDrop { get; private set; }

        public bool IsCheckingForDrag { get; private set; } = false;

        #endregion

        #region Events


        #endregion

        #region Init

        private MpDragDropManager() : base() {
            _autoScrollTimer = new DispatcherTimer();
            _autoScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _autoScrollTimer.Tick += _autoScrollTimer_Tick;

            MpMainWindowViewModel.Instance.OnMainWindowHide += Instance_OnMainWindowHide;

            MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
        }

        #endregion

        #region Public Methods

        public void StartDragCheck(Point mainWindowMouseDownPosition) {
            IsCheckingForDrag = true;

            _mouseStartPosition = mainWindowMouseDownPosition;

            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseMove += GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseUp += GlobalHook_MouseUp;
        }

        #endregion

        #region Private Methods

        private MpIContentDropTarget SelectDropTarget(object dragData) {
            MpIContentDropTarget selectedTarget = null;
            foreach (var dt in _dropTargets.Where(x => x.IsEnabled)) {
                if (!dt.IsDragDataValid(MpShortcutCollectionViewModel.Instance.IsCtrlDown, dragData)) {
                    continue;
                }

                dt.DropIdx = dt.GetDropTargetRectIdx();
                if (dt.DropIdx >= 0) {
                    if(selectedTarget != null) {
                        selectedTarget.DropIdx = -1;
                    }
                    selectedTarget = dt;
                }
            }
            return selectedTarget;
        }

        private  void Application_KeyDown(object sender, KeyEventArgs e) {
            if(!IsDragAndDrop) {
                return;
            }
            if(e.Key == Key.Escape) {
                Reset();
            }
        }
        private async void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) {
                return;
            }

            if (IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            if (IsDragAndDrop) {
                await PerformDrop(MpClipTrayViewModel.Instance.PersistentSelectedModels);
                MpMessenger.Instance.Send(MpMessageType.ItemDragEnd);
            }
        }

        private void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            Vector diff = Mouse.GetPosition(Application.Current.MainWindow) - _mouseStartPosition;

            if (diff.Length >= MINIMUM_DRAG_DISTANCE || IsDragAndDrop) {
                if (!IsDragAndDrop) {
                    IsDragAndDrop = true;
                    _autoScrollTimer.Start();
                    Keyboard.AddKeyDownHandler(Application.Current.MainWindow, Application_KeyDown);

                    MpMessenger.Instance.Send(MpMessageType.ItemDragBegin);
                }

                var dropTarget = SelectDropTarget(MpClipTrayViewModel.Instance.PersistentSelectedModels);

                if (dropTarget != _curDropTarget) {
                    _curDropTarget?.CancelDrop();
                    _curDropTarget = dropTarget;
                    _curDropTarget?.StartDrop();
                }
                _curDropTarget?.ContinueDragOverTarget();
            }
        }

        private async Task PerformDrop(object dragData) {
            if (_curDropTarget != null) {
                await _curDropTarget?.Drop(
                        MpShortcutCollectionViewModel.Instance.IsCtrlDown,
                        dragData);

                bool wasExternalDrop = _curDropTarget is MpExternalDropBehavior;

                if (wasExternalDrop) {
                    Application.Current.MainWindow.Activate();
                    Application.Current.MainWindow.Focus();
                    Application.Current.MainWindow.Topmost = true;

                    MpMainWindowViewModel.Instance.HideWindowCommand.Execute(null);
                    Application.Current.MainWindow.Top = 0;
                }
            }

            Reset();
        }
        private void Reset() {
            IsCheckingForDrag = IsDragAndDrop = false;

            _curDropTarget = null;

            _autoScrollTimer.Stop();
            _dropTargets.ForEach(x => x.Reset());

            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseMove -= GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseUp -= GlobalHook_MouseUp;

            Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, Application_KeyDown);

            UpdateCursor();
        }

        private void UpdateCursor() {
            MpCursorType currentCursor = MpCursorType.Default;

            if (!IsDragAndDrop) {
                currentCursor = MpCursorType.Default;
            } else if (!IsDropValid) {
                currentCursor = MpCursorType.Invalid;
            } else if (IsDragCopy) {
                currentCursor = _curDropTarget.CopyCursor;
            } else if (IsDragAndDrop) {
                currentCursor = _curDropTarget.MoveCursor;
            }

            MpCursorViewModel.Instance.CurrentCursor = currentCursor;
        }

        private void Instance_OnMainWindowHide(object sender, EventArgs e) {
            Reset();
        }

        private void _autoScrollTimer_Tick(object sender, EventArgs e) {
            
            _dropTargets.ForEach(x => x.UpdateAdorner());
            _dropTargets.ForEach(x => x.AutoScrollByMouse());

            UpdateCursor();
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
