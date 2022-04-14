using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MonkeyPaste;

namespace MpWpfApp {
    public static class MpDragDropManager {
        #region private static  Variables

        private const double MINIMUM_DRAG_DISTANCE = 10;

        private static Point _mouseStartPosition;

        private static MpIContentDropTarget _curDropTarget;

        private static List<MpIContentDropTarget> _dropTargets {
            get {
                List<MpIContentDropTarget> dtl = new List<MpIContentDropTarget>();

                //var clvl = Application.Current.MainWindow.GetVisualDescendents<MpContentListView>();
                //dtl.AddRange(clvl.Select(x => x.ContentListDropBehavior).Where(x => x.IsDropEnabled).ToList());

                //var rtbvl = Application.Current.MainWindow.GetVisualDescendents<MpRtbView>();
                //dtl.AddRange(rtbvl.Select(x => x.RtbViewDropBehavior).Where(x => x.IsDropEnabled).ToList());

                dtl.Add(Application.Current.MainWindow.GetVisualDescendent<MpClipTrayView>().ClipTrayDropBehavior);

                //var adivl = Application.Current.MainWindow.GetVisualDescendents<MpActionDesignerItemView>();
                //dtl.AddRange(adivl.Select(x => x.ActionDesignerItemDropBehavior).Where(x => x.IsDropEnabled).ToList());

                dtl.Add((Application.Current.MainWindow as MpMainWindow).ExternalDropBehavior);

                //dtl.Add((Application.Current.MainWindow as MpMainWindow).MainWindowResizeBehvior);

                return dtl;
            }
        }

        private static DispatcherTimer _autoScrollTimer;

        #endregion

        #region Properties

        public static bool IsDropValid => _curDropTarget != null;

        public static bool IsDragCopy {
            get {
                if (MpShortcutCollectionViewModel.Instance == null) {
                    return false;
                }
                return MpShortcutCollectionViewModel.Instance.IsCtrlDown;
            }
        }

        public static MpDropType DropType {
            get {
                if(_curDropTarget == null) {
                    return MpDropType.None;
                }
                return _curDropTarget.DropType;
            }
        }

        public static bool IsDragAndDrop { get; private set; }

        public static bool IsCheckingForDrag { get; private set; } = false;

        #endregion

        #region Events

        #endregion

        #region Init
        public static void Init() {
            _autoScrollTimer = new DispatcherTimer();
            _autoScrollTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _autoScrollTimer.Tick += _autoScrollTimer_Tick;

            MpMainWindowViewModel.Instance.OnMainWindowHidden += Instance_OnMainWindowHide;

            MpMessenger.Register<MpMessageType>(
                MpClipTrayViewModel.Instance, 
                ReceivedClipTrayViewModelMessage);
        }

        #endregion

        #region public static Methods

        public static void StartDragCheck(Point mainWindowMouseDownPosition) {
            if(MpMoveBehavior.IsAnyMoving) {
                return;
            }

            IsCheckingForDrag = true;

            _mouseStartPosition = mainWindowMouseDownPosition;

            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseMove += GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseUp += GlobalHook_MouseUp;
        }

        #endregion

        #region private static  Methods

        private static MpIContentDropTarget SelectDropTarget(object dragData) {
            MpIContentDropTarget selectedTarget = null;
            foreach (var dt in _dropTargets.Where(x => x.IsDropEnabled)) {
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

        private static void Application_KeyDown(object sender, KeyEventArgs e) {
            if(!IsDragAndDrop) {
                return;
            }
            if(e.Key == Key.Escape) {
                Reset();
            }
        }

        private static  async void GlobalHook_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) {
                return;
            }

            if (IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            if (IsDragAndDrop) {
                await PerformDrop(MpClipTrayViewModel.Instance.PersistentSelectedModels);
                MpMessenger.Send(MpMessageType.ItemDragEnd);
            }
        }

        private static void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (!IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            if(Mouse.LeftButton == MouseButtonState.Released) {
                // this is a sanity check since global event handlers are needed and 
                // probably something isn't releasing mouse capture
                Reset();
                return;
            }
            Vector diff = Mouse.GetPosition(Application.Current.MainWindow) - _mouseStartPosition;

            if (diff.Length >= MINIMUM_DRAG_DISTANCE || IsDragAndDrop) {
                if (!IsDragAndDrop) {
                    IsDragAndDrop = true;
                    _autoScrollTimer.Start();
                    Keyboard.AddKeyDownHandler(Application.Current.MainWindow, Application_KeyDown);

                    MpMessenger.Send(MpMessageType.ItemDragBegin);
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

        private static async Task PerformDrop(object dragData) {
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
        private static void Reset() {
            IsCheckingForDrag = IsDragAndDrop = false;

            _curDropTarget = null;

            _autoScrollTimer.Stop();
            _dropTargets.ForEach(x => x.Reset());

            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseMove -= GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalHook.MouseUp -= GlobalHook_MouseUp;

            Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, Application_KeyDown);

            UpdateCursor();
        }

        private static void UpdateCursor() {
            MpCursor.UnsetCursor(nameof(MpDragDropManager));

            MpCursorType currentCursor = MpCursorType.Default;

            if (!IsDragAndDrop) {
                return;
            } else if (!IsDropValid) {
                currentCursor = MpCursorType.Invalid;
            } else if (IsDragCopy) {
                currentCursor = _curDropTarget.CopyCursor;
            } else if (IsDragAndDrop) {
                currentCursor = _curDropTarget.MoveCursor;
            } else {
                return;
            }

            MpCursor.SetCursor(nameof(MpDragDropManager), currentCursor);
        }

        private static void Instance_OnMainWindowHide(object sender, EventArgs e) {
            Reset();
        }

        private static void _autoScrollTimer_Tick(object sender, EventArgs e) {
            
            _dropTargets.ForEach(x => x.UpdateAdorner());
            _dropTargets.ForEach(x => x.AutoScrollByMouse());

            UpdateCursor();
        }

        private static void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.JumpToIdxCompleted:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.TrayScrollChanged:
                    if(IsCheckingForDrag || IsDragAndDrop) {
                        _dropTargets.ForEach(x => x.UpdateAdorner());
                    }
                    
                    break;
            }
        }

        #endregion
    }

}
