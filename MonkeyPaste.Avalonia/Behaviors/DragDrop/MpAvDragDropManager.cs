using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;
using Avalonia.LogicalTree;

namespace MonkeyPaste.Avalonia {

    public static class MpAvDragDropManager {
        #region private static  Variables

        private const double MINIMUM_DRAG_DISTANCE = 10;

        private static MpPoint _mouseDragCheckStartPosition;

        private static DispatcherTimer _update_timer;

        #endregion

        #region Properties

        public static List<MpAvIContentDropTargetAsync> DropTargets {
            get {
                List<MpAvIContentDropTargetAsync> dtl = new List<MpAvIContentDropTargetAsync>();

                if (MpAvMainWindow.Instance == null) {
                    return dtl;
                }

                //var rtbvl = MpAvMainWindow.Instance.GetVisualDescendants<MpAvClipTileContentView>();
                //dtl.AddRange(rtbvl.Select(x => x.ContentViewDropBehavior).Where(x => x.IsDropEnabled).ToList());

                //var ptrv = MpAvMainWindow.Instance.GetVisualDescendant<MpAvPinTrayView>();
                //if (ptrv != null && ptrv.PinTrayDropBehavior != null) {
                //    dtl.Add(ptrv.PinTrayDropBehavior);
                //}

                if(MpAvPinTrayDropBehavior.Instance != null) {
                    dtl.Add(MpAvPinTrayDropBehavior.Instance);
                }

                //var adivl = MpAvMainWindow.Instance.GetVisualDescendants<MpActionDesignerItemView>();
                //dtl.AddRange(adivl.Select(x => x.ActionDesignerItemDropBehavior).Where(x => x.IsDropEnabled).ToList());

                //var edb = MpAvMainWindow.Instance.ExternalDropBehavior;
                //if (edb != null && edb.IsDropEnabled) {
                //    dtl.Add(MpAvMainWindow.Instance.ExternalDropBehavior);
                //}

                return dtl;
            }
        }

        public static bool IsDraggingFromExternal { get; set; } = false;

        public static object DragData { get; private set; }

        public static bool IsDropValid => CurDropTarget != null;

        public static bool IsDragCopy {
            get {
                if (MpAvShortcutCollectionViewModel.Instance == null) {
                    return false;
                }
                return MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown;
            }
        }

        public static MpDropType DropType {
            get {
                if (CurDropTarget == null) {
                    return MpDropType.None;
                }
                return CurDropTarget.DropType;
            }
        }

        public static MpAvIContentDropTargetAsync CurDropTarget { get; private set; }

        public static bool IsDragAndDrop { get; private set; }

        public static bool IsPerformingDrop { get; private set; }

        public static bool IsCheckingForDrag { get; private set; } = false;


        #endregion

        #region Events

        #endregion

        #region Public Methods

        public static void Init() {
            _update_timer = new DispatcherTimer(DispatcherPriority.Render);
            _update_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _update_timer.Tick += update_timer_Tick;

            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowHide;

            MpMessenger.Register<MpMessageType>(
                MpAvClipTrayViewModel.Instance,
                ReceivedClipTrayViewModelMessage);
        }

        public static void StartDragCheck(object dragData) {
            //if(MpMoveBehavior.IsAnyMoving || 
            //   MpResizeBehavior.IsAnyResizing ||
            //   IsCheckingForDrag) {
            //    return;
            //}

            DragData = dragData;
            IsCheckingForDrag = true;

            _mouseDragCheckStartPosition = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            _update_timer.Start();

            //MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove += DragCheck_OnGlobalMouseMove;

            //MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased += DragCheck_OnGlobalMouseReleased;
        }


        public static void SetDragData(object data) {
            DragData = data;
        }

        public static bool PrepareDropDataFromExternalSource(IDataObject extDragData) {
            //bool isValid = MpWpfDataObjectHelper.Instance.IsContentDropDragDataValid((DataObject)extDragData);
            //if (isValid) {
            //    // TODO add other format checks

            //    return true;
            //}
            //return false;
            DragData = MpPlatformWrapper.Services.DataObjectHelper.ConvertToSupportedPortableFormats(extDragData);
            //if(DragData != null) {
            //    MpConsole.WriteLine((DragData as MpPortableDataObject).ToJson().ToPrettyPrintJson());
            //}
            return DragData != null;
        }

        #endregion

        #region private static  Methods

        //private static async Task<MpAvIContentDropTargetAsync> SelectDropTargetAsync(object dragData) {
        //    MpAvIContentDropTargetAsync selectedTarget = null;

        //    foreach (var dt in DropTargets.Where(x => x.IsDropEnabled).OrderByDescending(x => (int)x.DropType)) {
        //        bool isDropValid = await dt.IsDragDataValidAsync(MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown, dragData);
        //        if (!isDropValid) {
        //            continue;
        //        }

        //        dt.DropIdx = await dt.GetDropTargetRectIdxAsync();
        //        if (dt.DropIdx >= 0) {
        //            if (selectedTarget != null) {
        //                selectedTarget.DropIdx = -1;
        //            }
        //            selectedTarget = dt;
        //        }
        //    }
        //    return selectedTarget;
        //}
        private static MpAvIContentDropTargetAsync SelectDropTarget(object dragData) {
            MpAvIContentDropTargetAsync selectedTarget = null;

            foreach (var dt in DropTargets.Where(x => x.IsDropEnabled).OrderByDescending(x => (int)x.DropType)) {
                bool isDropValid =  dt.IsDragDataValid(MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown, dragData);
                if (!isDropValid) {
                    continue;
                }

                dt.DropIdx = dt.GetDropTargetRectIdx();
                if (dt.DropIdx >= 0) {
                    if (selectedTarget != null) {
                        selectedTarget.DropIdx = -1;
                    }
                    selectedTarget = dt;
                }
            }
            return selectedTarget;
        }

        private static void Instance_OnGlobalEscapePressed(object sender, EventArgs e) {
            Reset(true);
        }

        private static void DragCheck_OnGlobalMouseReleased(object sender, bool isLeftButton) {
            if (isLeftButton) {
                if (IsDragAndDrop) {
                    PerformDrop(DragData).FireAndForgetSafeAsync(MpAvMainWindowViewModel.Instance);
                } else if (IsCheckingForDrag) {
                    Reset();
                }
            }
        }

        private static void DragCheck_OnGlobalMouseMove(object sender, MpPoint gmp) {
            
        }

        private static async Task PerformDrop(object dragData) {
            if (IsPerformingDrop) {
                return;
            }

            if (CurDropTarget != null) {
                IsPerformingDrop = true;

                await CurDropTarget?.DropAsync(
                        MpAvShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                        dragData);

                IsPerformingDrop = false;
            }

            Reset();
        }
        private static void Reset(bool isUserCancel = false) {

            //if(!isUserCancel && !IsDragAndDrop) {
            //    // not sure if this is right but need to be careful about order of events/state changes
            //    // so when reset comes from escape key this shouldn't be reachable even if IsDragDrop==false
            //    return;
            //}

            IsCheckingForDrag = false;
            IsDragAndDrop = false;
            IsDraggingFromExternal = false;
            MpMessenger.SendGlobal(MpMessageType.ItemDragEnd);

            DropTargets.Where(x=>x.DropAdorner != null)
                        .Select(x => x.DropAdorner)
                         .Cast<MpAvContentDragDropAdorner>()
                         .ForEach(x => x.StopRenderTimer());

            _mouseDragCheckStartPosition = null;
            CurDropTarget = null;

            DragData = null;
            DropTargets.ForEach(x => x.Reset());

            MpAvShortcutCollectionViewModel.Instance.OnGlobalEscKeyPressed -= Instance_OnGlobalEscapePressed;

            //MpAvMainWindow.Instance.PointerMoved -= Instance_PointerMoved;
            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseMove -= DragCheck_OnGlobalMouseMove;

            MpAvShortcutCollectionViewModel.Instance.OnGlobalMouseReleased -= DragCheck_OnGlobalMouseReleased;

            UpdateCursor();
            //
        }

        private static void UpdateCursor() {
            //MpPlatformWrapper.Services.Cursor.UnsetCursor(nameof(MpDragDropManager));

            MpCursorType dropCursor = MpCursorType.Default;

            if (!IsDragAndDrop) {
                MpPlatformWrapper.Services.Cursor.UnsetCursor(App.Desktop.MainWindow);
                return;
            } else if (!IsDropValid) {
                dropCursor = MpCursorType.Invalid;
            } else if (IsDragCopy) {
                dropCursor = CurDropTarget.CopyCursor;
            } else if (IsDragAndDrop) {
                dropCursor = CurDropTarget.MoveCursor;
            } else {
                MpPlatformWrapper.Services.Cursor.UnsetCursor(App.Desktop.MainWindow);
                return;
            }

            if (MpPlatformWrapper.Services.Cursor.CurrentCursor != dropCursor) {
                if(CurDropTarget != null && CurDropTarget.RelativeToElement != null) {
                    MpPlatformWrapper.Services.Cursor.SetCursor(CurDropTarget.RelativeToElement, dropCursor);
                }else {
                    MpPlatformWrapper.Services.Cursor.SetCursor(App.Desktop.MainWindow, dropCursor);
                }
               
            }
        }

        private static void Instance_OnMainWindowHide(object sender, EventArgs e) {
            if (!IsDragAndDrop) {
                Reset();
            }
        }

        private static void update_timer_Tick(object sender, EventArgs e) {
            //MpConsole.WriteLine("tick " + DateTime.Now);
            //DropTargets.ForEach(x => x.UpdateAdorner());
            //DropTargets.ForEach(x => x.AutoScrollByMouse());
            //Task.WhenAll(DropTargets.Select(x => x.UpdateRectsAsync()))
            //    .FireAndForgetSafeAsync(MpAvMainWindowViewModel.Instance);

            //MpConsole.WriteLine("mouse move: " + gmp);
            UpdateCursor();

            if (IsPerformingDrop) {
                // mouse is released below note is old from mouse move event

                // NOTE added this state to try to fix DropIdx from clearing during drop                
                return;
            }


            if (IsDraggingFromExternal && !MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                IsDraggingFromExternal = false;
            }
            // NOTE is not on main thread from external drag
            if (!IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            bool isLeftMouseButtonDown = MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown;
            if (!isLeftMouseButtonDown || _mouseDragCheckStartPosition == null) {
                if (IsDragAndDrop) {
                    PerformDrop(DragData).FireAndForgetSafeAsync(null);
                    return;
                } //else if (IsCheckingForDrag) {
                 //   Reset();
               // }

                // this is a sanity check since global event handlers are needed and 
                // probably something isn't releasing mouse capture
                Reset();
                return;
            }
            //MpConsole.WriteLine("In DragDrop mouse move " + MpShortcutCollectionViewModel.Instance.GlobalMouseLocation);
            MpPoint mp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            MpPoint diff = mp - _mouseDragCheckStartPosition;

            if (diff.Length >= MINIMUM_DRAG_DISTANCE || IsDragAndDrop) {
                if (!IsDragAndDrop) {
                    IsDragAndDrop = true;

                    DropTargets
                        .Where(x=>x.DropAdorner != null)
                         .Select(x => x.DropAdorner)
                         .Cast<MpAvContentDragDropAdorner>()
                         .ForEach(x => x.StartRenderTimer());

                    MpAvShortcutCollectionViewModel.Instance.OnGlobalEscKeyPressed += Instance_OnGlobalEscapePressed;
                    MpMessenger.SendGlobal(MpMessageType.ItemDragBegin);
                }

                var dropTarget = SelectDropTarget(DragData);

                if (dropTarget != CurDropTarget) {
                    CurDropTarget?.CancelDrop();
                    CurDropTarget = dropTarget;
                    CurDropTarget?.StartDropAsync();
                }

                CurDropTarget?.ContinueDragOverTargetAsync();
            }
            UpdateCursor();
        }

        private static void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.JumpToIdxCompleted:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.TrayScrollChanged:
                    if (IsCheckingForDrag || IsDragAndDrop) {
                        DropTargets.ForEach(x => x.UpdateAdorner());
                    }

                    break;
            }
        }

        #endregion
    }

}
