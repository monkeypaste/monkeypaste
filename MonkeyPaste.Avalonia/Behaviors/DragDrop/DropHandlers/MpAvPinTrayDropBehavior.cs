using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Layout;
using MonkeyPaste.Common.Avalonia;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using MonoMac.CoreText;


namespace MonkeyPaste.Avalonia {
    public class MpAvPinTrayDropBehavior : MpAvDropBehaviorBase<MpAvPinTrayView> {
        #region Private Variables               
        private double _autoScrollMinScrollDist = 15.0d;

        private double _autoScrollAccumulator = 5.0d;
        private double _baseAutoScrollVelocity = 5.0d;
        private double _autoScrollVelocity;

        private double _dragOverPadding = 30;

        #endregion

        #region Statics
        private static MpAvPinTrayDropBehavior _instance;
        public static MpAvPinTrayDropBehavior Instance => _instance;
        #endregion

        #region Properties

        #region MpAvIContentDropTarget Override Implementation

        public override Orientation AdornerOrientation => Orientation.Vertical;
        public override Control AdornedElement => AssociatedObject == null ? null : AssociatedObject.PinTrayListBox;

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.PinTray;

        public override Control RelativeToElement => AssociatedObject == null ? null : AssociatedObject.PinTrayListBox.GetVisualDescendant<ScrollViewer>();

        public override MpCursorType MoveCursor => MpCursorType.TileMove;
        public override MpCursorType CopyCursor => MpCursorType.TileCopy;

        #endregion

        #endregion

        #region Constructors

        public MpAvPinTrayDropBehavior() : base() {
            if(_instance == null) {
                _instance = this;
            } else {
                Debugger.Break();
            }
        }
        #endregion

        #region Public Methods

        public override void OnLoaded() {
            //IsDebugEnabled = true;
            base.OnLoaded();
        }

        protected override void ReceivedGlobalMessage(MpMessageType msg) {
            base.ReceivedGlobalMessage(msg);
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                    RefreshDropRects();
                    break;
            }
        }

        public override void AutoScrollByMouse() {
            var pt_lb = AssociatedObject.PinTrayListBox;
            var sv = pt_lb.GetVisualDescendant<ScrollViewer>();
            if(sv == null) {
                // BUG this happens intermittently, maybe when dragging during immediately
                // after startup
                return;
            }
            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;            
            var pin_tray_lb_mp = VisualExtensions.PointToClient(pt_lb,gmp.ToAvPixelPoint()).ToPortablePoint();

            MpRect pin_tray_lb_rect = new MpRect(0, 0, pt_lb.Bounds.Width, pt_lb.Bounds.Height);

            if (sv.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) {
                //pin_tray_lb_rect.Height -= sv.GetScrollBar(Orientation.Horizontal).Height;
                pin_tray_lb_rect.Bottom -= sv.GetScrollBar(Orientation.Horizontal).Height;
            }
            if (sv.VerticalScrollBarVisibility == ScrollBarVisibility.Visible) {
                //pin_tray_lb_rect.Width -= sv.GetScrollBar(Orientation.Vertical).Width;
                pin_tray_lb_rect.Right -= sv.GetScrollBar(Orientation.Vertical).Width;
            }

            if (!pin_tray_lb_rect.Contains(pin_tray_lb_mp)) {
                return;
            }

            double ldist = Math.Abs(pin_tray_lb_mp.X - pin_tray_lb_rect.Left);
            double rdist = Math.Abs(pin_tray_lb_mp.X - pin_tray_lb_rect.Right);
            double tdist = Math.Abs(pin_tray_lb_mp.Y - pin_tray_lb_rect.Top);
            double bdist = Math.Abs(pin_tray_lb_mp.Y - pin_tray_lb_rect.Bottom);

            MpPoint pinTrayScrollOffsetDelta = new MpPoint();
            if (ldist <= _autoScrollMinScrollDist) {
                pinTrayScrollOffsetDelta.X = -_autoScrollVelocity;
            } else if (rdist <= _autoScrollMinScrollDist) {
                pinTrayScrollOffsetDelta.X = _autoScrollVelocity;
            }

            if (tdist <= _autoScrollMinScrollDist) {
                pinTrayScrollOffsetDelta.Y = -_autoScrollVelocity;
            } else if (bdist <= _autoScrollMinScrollDist) {
                pinTrayScrollOffsetDelta.Y = _autoScrollVelocity;
            }

            if (pinTrayScrollOffsetDelta.X != 0 || pinTrayScrollOffsetDelta.Y != 0) {
                _autoScrollVelocity += _autoScrollAccumulator;                
                sv.ScrollByPointDelta(pinTrayScrollOffsetDelta);
            }
        }

        public override async Task<bool> IsDragDataValidAsync(bool isCopy, object dragData) {
            bool isValidBaseResult = await base.IsDragDataValidAsync(isCopy, dragData);
            if (!isValidBaseResult) {
                return false;
            }
            if(dragData is MpAvClipTileViewModel ctvm) {
                bool isAllSelected = await MpAvTextBoxSelectionExtension.IsAllSelectedAsync(ctvm);
                if (ctvm.IsPinned && isAllSelected) {
                    return false;
                }
            }
            return true;
        }

        public override async Task<int> GetDropTargetRectIdxAsync() {
            await Task.Delay(1);
            var ctrvm = MpAvClipTrayViewModel.Instance;
            var gmp = MpAvShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = VisualExtensions.PointToClient(AssociatedObject.PinTrayListBox, gmp.ToAvPixelPoint()).ToPortablePoint();
            var hover_ctvm = ctrvm.PinnedItems.FirstOrDefault(x => x.IsHovering);
            return ctrvm.PinnedItems.IndexOf(hover_ctvm);

            //var drl = await GetDropTargetRectsAsync();
            //ctrvm.IsDragOverPinTray = drl[0].Contains(mp) && !isOverPinnedItem;
            //return ctrvm.IsDragOverPinTray ? 0 : -1;
        }
        public override async Task<List<MpRect>> GetDropTargetRectsAsync() {
            // NOTE only parent rect is used for selecting target actual drop idx
            // is based off of hover item idx and drop behavior priority (so content view drop takes priority)

            await Task.Delay(1);

            var rl = new List<MpRect>();
            if(AssociatedObject == null) {
                return rl;
            }

            rl = new List<MpRect>() {
                new MpRect(0, 0, AssociatedObject.PinTrayListBox.Bounds.Width, AssociatedObject.PinTrayListBox.Bounds.Height) {
                    StrokeOctColor = MpSystemColors.blue1,
                    FillOctColor = MpSystemColors.Red
                }
            };
            return rl;
        }
        public override async Task<MpShape[]> GetDropTargetAdornerShapeAsync() {
            await Task.Delay(1);
            var hover_Ctvm = MpAvClipTrayViewModel.Instance.PinnedItems.FirstOrDefault(x => x.IsHovering);
            if(hover_Ctvm != null) {
                return new MpShape[] {
                    new MpRect(10,10,20,500) {
                        FillOctColor = MpSystemColors.White
                    }
                };
            }
            return new MpShape[] {};
        }

        public override async Task StartDropAsync() { 
            await Task.Delay(1);
            AssociatedObject.PinTrayListBox.Width += _dragOverPadding;

            if(AssociatedObject.DataContext is MpAvClipTrayViewModel ctrvm) {
                ctrvm.IsDragOverPinTray = true;
            }
        }

        public override void CancelDrop() {
            base.CancelDrop();
            AssociatedObject.PinTrayListBox.Width -= _dragOverPadding;            
        }

        public override async Task DropAsync(bool isCopy, object dragData) {
            var ctrvm = MpAvClipTrayViewModel.Instance;
            // BUG storing dropIdx because somehow it gets lost after calling base

            int dropIdx = ctrvm.PinnedItems.Count;

            await base.DropAsync(isCopy, dragData);

            MpAvClipTileViewModel drag_ctvm = null;
            MpAvClipTileViewModel drop_ctvm = null;

            if(dragData is MpPortableDataObject mpdo) {
                // from external source
                var dragModel = await MpAvCopyItemBuilder.CreateFromDataObject(mpdo);

                drop_ctvm = await ctrvm.CreateClipTileViewModel(dragModel);                
            } else if(dragData is MpAvClipTileViewModel) {
                // either a partial pinned or tray selection or tray item

                drag_ctvm = dragData as MpAvClipTileViewModel;
                bool isTotalContentDrop = await MpAvTextBoxSelectionExtension.IsAllSelectedAsync(drag_ctvm);
                if (isTotalContentDrop) {
                    drop_ctvm = drag_ctvm;
                    drag_ctvm = null;
                } else {
                    string drop_data = drag_ctvm.SelectedRichText; 

                    var dragModel = await drag_ctvm.CopyItem.Clone(false) as MpCopyItem;
                    dragModel.ItemData = drop_data;
                    await dragModel.WriteToDatabaseAsync();

                    drop_ctvm = await ctrvm.CreateClipTileViewModel(dragModel);
                }       
            }

            ctrvm.PinTileCommand.Execute(drop_ctvm);

            while (ctrvm.IsAnyBusy) {
                await Task.Delay(100);
            }

            if(drag_ctvm != null && !isCopy) {
                await MpAvCefNetWebViewExtension.FinishContentCutAsync(drag_ctvm);
            }
            
                        
            // NOTE need to re-assign drop_ctvm since toggle may create a new tile if it was on tray
            drop_ctvm = ctrvm.PinnedItems.FirstOrDefault(x => x.CopyItemId == drop_ctvm.CopyItemId);

            if(drop_ctvm == null) {
                //Debugger.Break();
                return;
            }

            drop_ctvm.IsSelected = true;
        }

        public override void Reset() {
            base.Reset();

            _autoScrollVelocity = _baseAutoScrollVelocity;

            if (AssociatedObject != null &&
                AssociatedObject.DataContext is MpAvClipTrayViewModel ctrvm) {
                ctrvm.IsDragOverPinTray = false;
            }
        }

        #endregion

        //sync

        //public override void StartDrop() {
        //    AssociatedObject.PinTrayListBox.Width += _dragOverPadding;

        //    if (AssociatedObject.DataContext is MpAvClipTrayViewModel ctrvm) {
        //        ctrvm.IsDragOverPinTray = true;
        //    }
        //}

        //public override bool IsDragDataValid(bool isCopy, object dragData) {
        //    bool isValidBaseResult = base.IsDragDataValid(isCopy, dragData);
        //    if (!isValidBaseResult) {
        //        return false;
        //    }
        //    if (dragData is MpAvClipTileViewModel ctvm) {
        //        bool isAllSelected = await MpAvTextBoxSelectionExtension.IsAllSelectedAsync(ctvm);
        //        if (ctvm.IsPinned && isAllSelected) {
        //            return false;
        //        }
        //    }
        //    return true;
        //}
    }

}
