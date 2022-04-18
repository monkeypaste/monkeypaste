using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MonkeyPaste;
using MonkeyPaste.Plugin;

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

                var rtbvl = Application.Current.MainWindow.GetVisualDescendents<MpContentView>();
                dtl.AddRange(rtbvl.Select(x => x.ContentViewDropBehavior).Where(x => x.IsDropEnabled).ToList());

                dtl.Add(Application.Current.MainWindow.GetVisualDescendent<MpClipTrayView>().ClipTrayDropBehavior);

                //var adivl = Application.Current.MainWindow.GetVisualDescendents<MpActionDesignerItemView>();
                //dtl.AddRange(adivl.Select(x => x.ActionDesignerItemDropBehavior).Where(x => x.IsDropEnabled).ToList());

                dtl.Add((Application.Current.MainWindow as MpMainWindow).ExternalDropBehavior);

                return dtl;
            }
        }

        private static DispatcherTimer _timer;

        #endregion

        #region Properties
        public static object DragData { get; private set; }

        public static bool IsDropValid => _curDropTarget != null;

        public static bool IsDragCopy {
            get {
                if (MpShortcutCollectionViewModel.Instance == null) {
                    return false;
                }
                return MpShortcutCollectionViewModel.Instance.GlobalIsCtrlDown;
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
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += _timer_Tick;

            MpMainWindowViewModel.Instance.OnMainWindowHidden += Instance_OnMainWindowHide;

            MpMessenger.Register<MpMessageType>(
                MpClipTrayViewModel.Instance, 
                ReceivedClipTrayViewModelMessage);
        }

        #endregion

        #region public static Methods

        public static void StartDragCheck(object dragData) {
            if(MpMoveBehavior.IsAnyMoving) {
                return;
            }

            DragData = dragData;
            IsCheckingForDrag = true;

            _mouseStartPosition = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;

            MpShortcutCollectionViewModel.Instance.GlobalMouseMove += GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonUp += GlobalHook_MouseUp;
        }

        public static bool PrepareDropDataFromExternalSource(IDataObject extDragData) {
            //bool isValid = MpWpfDataObjectHelper.Instance.IsContentDropDragDataValid((DataObject)extDragData);
            //if (isValid) {
            //    // TODO add other format checks

            //    return true;
            //}
            //return false;
            DragData = MpWpfDataObjectHelper.Instance.ConvertToSupportedPortableFormats(extDragData);
            if(DragData != null) {
                MpConsole.WriteLine((DragData as MpDataObject).ToJson().ToPrettyPrintJson());
            }
            return DragData != null;
        }

        #endregion

        #region private static  Methods

        private static MpIContentDropTarget SelectDropTarget(object dragData) {
            MpIContentDropTarget selectedTarget = null;
            foreach (var dt in _dropTargets.Where(x => x.IsDropEnabled)) {
                if (!dt.IsDragDataValid(MpShortcutCollectionViewModel.Instance.GlobalIsCtrlDown, dragData)) {
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

        private static void GlobalEscKey_Pressed(object sender, EventArgs e) {
            if(!IsDragAndDrop) {
                return;
            }
            Reset();
        }

        private static  async void GlobalHook_MouseUp(object sender, EventArgs e) {
            if (IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            if (IsDragAndDrop) {
                await PerformDrop(DragData);
            }
        }

        private static void GlobalHook_MouseMove(object sender, Point mp) {
            MpHelpers.RunOnMainThread(() => {
                // NOTE is not on main thread from external drag
                if (!IsCheckingForDrag && !IsDragAndDrop) {
                    Reset();
                    return;
                }
                if (!MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                    // this is a sanity check since global event handlers are needed and 
                    // probably something isn't releasing mouse capture
                    Reset();
                    return;
                }
                MpConsole.WriteLine("In DragDrop mouse move " + MpShortcutCollectionViewModel.Instance.GlobalMouseLocation);

                Vector diff = mp - _mouseStartPosition;

                if (diff.Length >= MINIMUM_DRAG_DISTANCE || IsDragAndDrop) {
                    if (!IsDragAndDrop) {
                        IsDragAndDrop = true;
                        _timer.Start();
                        MpShortcutCollectionViewModel.Instance.GlobalEscKeyPressed += GlobalEscKey_Pressed;
                        MpMessenger.Send(MpMessageType.ItemDragBegin);
                    }

                    var dropTarget = SelectDropTarget(DragData);

                    if (dropTarget != _curDropTarget) {
                        _curDropTarget?.CancelDrop();
                        _curDropTarget = dropTarget;
                        _curDropTarget?.StartDrop();
                    }
                    _curDropTarget?.ContinueDragOverTarget();
                }
            });
        }

        private static async Task PerformDrop(object dragData) {            
            await MpHelpers.RunOnMainThreadAsync(async() => {
                if (_curDropTarget != null) {
                    await _curDropTarget?.Drop(
                            MpShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
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

                MpMessenger.Send(MpMessageType.ItemDragEnd);

                Reset();
            });

        }
        private static void Reset() {
            IsCheckingForDrag = IsDragAndDrop = false;

            _curDropTarget = null;
            DragData = null;
            _timer.Stop();
            _dropTargets.ForEach(x => x.Reset());

            MpShortcutCollectionViewModel.Instance.GlobalMouseMove -= GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonUp -= GlobalHook_MouseUp;

            MpShortcutCollectionViewModel.Instance.GlobalEscKeyPressed -= GlobalEscKey_Pressed;
            //Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, GlobalEscKey_Pressed);

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

        private static void _timer_Tick(object sender, EventArgs e) {            
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
