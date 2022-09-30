using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpPinTrayDropBehavior : MpDropBehaviorBase<MpPinTrayView> {
        #region Private Variables               
        private double _autoScrollMinScrollDist = 15.0d;

        private double _autoScrollAccumulator = 5.0d;
        private double _baseAutoScrollVelocity = 5.0d;
        private double _autoScrollVelocity;

        private double _dragOverPadding = 30;

        #endregion

        public override Orientation AdornerOrientation => Orientation.Vertical;
        public override FrameworkElement AdornedElement => AssociatedObject.PinTrayListBox;

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.PinTray;

        public override UIElement RelativeToElement => AssociatedObject.PinTrayListBox.GetVisualDescendent<ScrollViewer>();

        public override MpCursorType MoveCursor => MpCursorType.TileMove;
        public override MpCursorType CopyCursor => MpCursorType.TileCopy;



        public override void OnLoaded() {
            //IsDebugEnabled = true;
            base.OnLoaded();
        }

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                    RefreshDropRects();
                    break;
            }
        }

        public override void AutoScrollByMouse() {
            var pt_lb = AssociatedObject.PinTrayListBox;
            var sv = pt_lb.GetVisualDescendent<ScrollViewer>();
            if(sv == null) {
                // BUG this happens intermittently, maybe when dragging during immediately
                // after startup
                return;
            }
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;            
            var pin_tray_lb_mp = Application.Current.MainWindow.TranslatePoint(gmp, pt_lb);

            Rect pin_tray_lb_rect = new Rect(0, 0, pt_lb.ActualWidth, pt_lb.ActualHeight);

            if (sv.HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) {
                pin_tray_lb_rect.Height -= sv.GetScrollBar(Orientation.Horizontal).Height;
            }
            if (sv.VerticalScrollBarVisibility == ScrollBarVisibility.Visible) {
                pin_tray_lb_rect.Width -= sv.GetScrollBar(Orientation.Vertical).Width;
            }

            if (!pin_tray_lb_rect.Contains(pin_tray_lb_mp)) {
                return;
            }

            double ldist = Math.Abs(pin_tray_lb_mp.X - pin_tray_lb_rect.Left);
            double rdist = Math.Abs(pin_tray_lb_mp.X - pin_tray_lb_rect.Right);
            double tdist = Math.Abs(pin_tray_lb_mp.Y - pin_tray_lb_rect.Top);
            double bdist = Math.Abs(pin_tray_lb_mp.Y - pin_tray_lb_rect.Bottom);

            Point pinTrayScrollOffsetDelta = new Point();
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

        public override bool IsDragDataValid(bool isCopy, object dragData) {
            if(!base.IsDragDataValid(isCopy, dragData)) {
                return false;
            }
            if(dragData is MpClipTileViewModel ctvm) {
                if(ctvm.IsPinned && MpTextBoxSelectionRangeExtension.IsAllSelected(ctvm)) {
                    return false;
                }
            }
            return true;
        }

        public override int GetDropTargetRectIdx() {
            var ctrvm = MpClipTrayViewModel.Instance;
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, AssociatedObject.PinTrayListBox);
            bool isOverPinnedItem = ctrvm.PinnedItems.Any(x => x.IsHovering);
            ctrvm.IsDragOverPinTray = DropRects[0].Contains(mp) && !isOverPinnedItem;
            return ctrvm.IsDragOverPinTray ? 0 : -1;
        }
        public override List<Rect> GetDropTargetRects() {
            var rl = new List<Rect>();
            if(AssociatedObject == null) {
                return rl;
            }

            rl = new List<Rect>() {
                new Rect(0, 0, AssociatedObject.PinTrayListBox.ActualWidth, AssociatedObject.PinTrayListBox.ActualHeight)
            };
            return rl;
        }
        public override MpShape[] GetDropTargetAdornerShape() {
            return null;
        }

        public override async Task StartDrop() { 
            await Task.Delay(1);
            AssociatedObject.PinTrayListBox.Width += _dragOverPadding;

            if(AssociatedObject.DataContext is MpClipTrayViewModel ctrvm) {
                ctrvm.IsDragOverPinTray = true;
            }
        }

        public override void CancelDrop() {
            base.CancelDrop();
            AssociatedObject.PinTrayListBox.Width -= _dragOverPadding;            
        }

        public override async Task Drop(bool isCopy, object dragData) {
            var ctrvm = MpClipTrayViewModel.Instance;
            // BUG storing dropIdx because somehow it gets lost after calling base

            int dropIdx = ctrvm.PinnedItems.Count;

            await base.Drop(isCopy, dragData);

            MpClipTileViewModel drag_ctvm = null;
            MpClipTileViewModel drop_ctvm = null;

            if(dragData is MpPortableDataObject mpdo) {
                // from external source
                var dragModel = await MpWpfCopyItemBuilder.CreateFromDataObjectAsync(mpdo);

                drop_ctvm = await ctrvm.CreateClipTileViewModel(dragModel);                
            } else if(dragData is MpClipTileViewModel) {
                // either a partial pinned or tray selection or tray item

                drag_ctvm = dragData as MpClipTileViewModel;
                bool isPartialDrop = !MpTextBoxSelectionRangeExtension.IsAllSelected(drag_ctvm);
                if(isPartialDrop) {
                    string drop_data = drag_ctvm.SelectedRichText; 

                    var dragModel = await drag_ctvm.CopyItem.Clone(false) as MpCopyItem;
                    dragModel.ItemData = drop_data;
                    await dragModel.WriteToDatabaseAsync();

                    drop_ctvm = await ctrvm.CreateClipTileViewModel(dragModel);
                } else {
                    drop_ctvm = drag_ctvm;
                    drag_ctvm = null;
                }        
            }

            ctrvm.PinTileCommand.Execute(drop_ctvm);

            while (ctrvm.IsAnyBusy) {
                await Task.Delay(100);
            }

            if(drag_ctvm != null && !isCopy) {
                await MpContentDocumentRtfExtension.FinishContentCut(drag_ctvm);
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

            if (AssociatedObject.DataContext is MpClipTrayViewModel ctrvm) {
                ctrvm.IsDragOverPinTray = false;
            }
        }
    }

}
