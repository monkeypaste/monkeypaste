﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {

    public enum MpDropType {
        None,
        Content,
        PinTray,
        External,
        Action
    }

    public interface MpIContentDropTarget {
        object DataContext { get; }
        bool IsDropEnabled { get; set; }
        bool IsDebugEnabled { get; set; }
        int DropIdx { get; set; }
        MpDropType DropType { get; }

        MpCursorType MoveCursor { get; }
        MpCursorType CopyCursor { get; }

        void AutoScrollByMouse();

        bool IsDragDataValid(bool isCopy, object dragData);

        Task StartDrop();
        Task Drop(bool isCopy, object dragData);
        void CancelDrop();

        Control RelativeToElement { get; }
        List<Rect> DropRects { get; }
        List<Rect> GetDropTargetRects();
        int GetDropTargetRectIdx();
        MpShape[] GetDropTargetAdornerShape();
        void ContinueDragOverTarget();

       //MpContentAdorner DropLineAdorner { get; set; }
        Orientation AdornerOrientation { get; }
        void InitAdorner();
        void UpdateAdorner();
        void Reset();
    }

    public static class MpAvDragDropManager {
        #region private static  Variables

        private const double MINIMUM_DRAG_DISTANCE = 10;

        private static MpPoint _mouseDragCheckStartPosition;

        private static DispatcherTimer _timer;

        #endregion

        #region Properties

        public static List<MpIContentDropTarget> DropTargets {
            get {
                //List<MpIContentDropTarget> dtl = new List<MpIContentDropTarget>();

                //var rtbvl = Application.Current.MainWindow.GetVisualDescendents<MpRtbContentView>();
                //dtl.AddRange(rtbvl.Select(x => x.ContentViewDropBehavior).Where(x => x.IsDropEnabled).ToList());

                //var ctrv = Application.Current.MainWindow.GetVisualDescendent<MpClipTrayView>();
                //if (ctrv != null) {
                //    dtl.Add(ctrv.ClipTrayDropBehavior);
                //}

                //var ptrv = Application.Current.MainWindow.GetVisualDescendent<MpPinTrayView>();
                //if (ptrv != null) {
                //    dtl.Add(ptrv.PinTrayDropBehavior);
                //}


                //var adivl = Application.Current.MainWindow.GetVisualDescendents<MpActionDesignerItemView>();
                //dtl.AddRange(adivl.Select(x => x.ActionDesignerItemDropBehavior).Where(x => x.IsDropEnabled).ToList());

                //var mwv = Application.Current.MainWindow as MpMainWindow;
                //if (mwv != null) {
                //    dtl.Add(mwv.ExternalDropBehavior);
                //}


                //return dtl;
                return null;

            }
        }

        public static bool IsDraggingFromExternal { get; set; } = false;

        public static object DragData { get; private set; }

        public static bool IsDropValid => CurDropTarget != null;

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
                if(CurDropTarget == null) {
                    return MpDropType.None;
                }
                return CurDropTarget.DropType;
            }
        }

        public static MpIContentDropTarget CurDropTarget { get; private set; }

        public static bool IsDragAndDrop { get; private set; }        

        public static bool IsPerformingDrop { get; private set; }

        public static bool IsCheckingForDrag { get; private set; } = false;


        #endregion

        #region Events

        #endregion

        #region Init
        public static void Init() {
            _timer = new DispatcherTimer(DispatcherPriority.Render);
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += _timer_Tick;

            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += Instance_OnMainWindowHide;

            MpMessenger.Register<MpMessageType>(
                MpAvClipTrayViewModel.Instance, 
                ReceivedClipTrayViewModelMessage);
        }

        #endregion

        #region public static Methods


        public static void StartDragCheck(object dragData) {
            //if(MpMoveBehavior.IsAnyMoving || 
            //   MpResizeBehavior.IsAnyResizing ||
            //   IsCheckingForDrag) {
            //    return;
            //}
            
            DragData = dragData;
            IsCheckingForDrag = true;

            _mouseDragCheckStartPosition = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;

            MpShortcutCollectionViewModel.Instance.GlobalMouseMove += GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonUp += GlobalHook_MouseUp;
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

        private static MpIContentDropTarget SelectDropTarget(object dragData) {
            MpIContentDropTarget selectedTarget = null;
            foreach (var dt in DropTargets.Where(x => x.IsDropEnabled).OrderByDescending(x=>(int)x.DropType)) {
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
            if (IsDragAndDrop) {
                await PerformDrop(DragData);
            } else if (IsCheckingForDrag) {
                Reset();
            }
        }

        private static void GlobalHook_MouseMove(object sender, Point mp) {
            if(IsPerformingDrop) {
                // NOTE added this state to try to fix DropIdx from clearing during drop                
                return;
            }
            if(IsDraggingFromExternal && !MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                IsDraggingFromExternal = false;
            }
            // NOTE is not on main thread from external drag
            if (!IsCheckingForDrag && !IsDragAndDrop) {
                Reset();
                return;
            }
            bool isLeftMouseButtonDown = MpShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown;
            if (!isLeftMouseButtonDown || _mouseDragCheckStartPosition == null) {
                // this is a sanity check since global event handlers are needed and 
                // probably something isn't releasing mouse capture
                Reset();
                return;
            }
            //MpConsole.WriteLine("In DragDrop mouse move " + MpShortcutCollectionViewModel.Instance.GlobalMouseLocation);

            MpPoint diff = mp.ToPortablePoint() - _mouseDragCheckStartPosition;

            if (diff.Length >= MINIMUM_DRAG_DISTANCE || IsDragAndDrop) {
                if (!IsDragAndDrop) {
                    IsDragAndDrop = true;
                    _timer.Start();
                    MpShortcutCollectionViewModel.Instance.GlobalEscKeyPressed += GlobalEscKey_Pressed;
                    MpMessenger.SendGlobal(MpMessageType.ItemDragBegin);
                }

                var dropTarget = SelectDropTarget(DragData);

                if (dropTarget != CurDropTarget) {
                    CurDropTarget?.CancelDrop();
                    CurDropTarget = dropTarget;
                    CurDropTarget?.StartDrop();
                }

                CurDropTarget?.ContinueDragOverTarget();

            }
        }

        private static async Task PerformDrop(object dragData) {
            if(IsPerformingDrop) {
                return;
            }

            if (CurDropTarget != null) {
                IsPerformingDrop = true;

                await CurDropTarget?.Drop(
                        MpShortcutCollectionViewModel.Instance.GlobalIsCtrlDown,
                        dragData);

                IsPerformingDrop = false;
            }                        

            Reset(); 
        }
        private static void Reset() {
            IsCheckingForDrag = IsDragAndDrop = IsDraggingFromExternal = false;
            MpMessenger.SendGlobal(MpMessageType.ItemDragEnd);

            _mouseDragCheckStartPosition = null;
            CurDropTarget = null;
            
            DragData = null;
            _timer.Stop();
            DropTargets.ForEach(x => x.Reset());

            MpShortcutCollectionViewModel.Instance.GlobalMouseMove -= GlobalHook_MouseMove;
            MpShortcutCollectionViewModel.Instance.GlobalMouseLeftButtonUp -= GlobalHook_MouseUp;
            MpShortcutCollectionViewModel.Instance.GlobalEscKeyPressed -= GlobalEscKey_Pressed;

            UpdateCursor();
            //
        }

        private static void UpdateCursor() {
            //MpCursor.UnsetCursor(nameof(MpDragDropManager));

            MpCursorType dropCursor = MpCursorType.Default;

            if (!IsDragAndDrop) {
                MpCursor.UnsetCursor(nameof(MpAvDragDropManager));
                return;
            } else if (!IsDropValid) {
                dropCursor = MpCursorType.Invalid;
            } else if (IsDragCopy) {
                dropCursor = CurDropTarget.CopyCursor;
            } else if (IsDragAndDrop) {
                dropCursor = CurDropTarget.MoveCursor;
            } else {
                MpCursor.UnsetCursor(nameof(MpAvDragDropManager));
                return;
            }

            if(MpCursor.CurrentCursor != dropCursor) {
                MpCursor.SetCursor(nameof(MpAvDragDropManager), dropCursor);
            }
        }

        private static void Instance_OnMainWindowHide(object sender, EventArgs e) {
            if (!IsDragAndDrop) {
                Reset();
            }            
        }

        private static void _timer_Tick(object sender, EventArgs e) {            
            DropTargets.ForEach(x => x.UpdateAdorner());
            DropTargets.ForEach(x => x.AutoScrollByMouse());

            UpdateCursor();
        }

        private static void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.JumpToIdxCompleted:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.TrayScrollChanged:
                    if(IsCheckingForDrag || IsDragAndDrop) {
                        DropTargets.ForEach(x => x.UpdateAdorner());
                    }
                    
                    break;
            }
        }

        #endregion
    }

}