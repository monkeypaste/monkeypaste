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

        public override int GetDropTargetRectIdx() {
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, AssociatedObject.PinTrayListBox);
            //MpConsole.WriteLine("Global mp in tray coords: " + mp);
            var tray_rect = new Rect(0, 0, AssociatedObject.PinTrayListBox.ActualWidth, AssociatedObject.PinTrayListBox.ActualHeight);
            if (!tray_rect.Contains(mp)) {
                return -1;
            }
            Rect targetRect = DropRects.FirstOrDefault(x => x.Contains(mp));
            if (targetRect == null || targetRect.IsEmpty) {
                return -1;
            }
            return DropRects.IndexOf(targetRect);
        }
        public override List<Rect> GetDropTargetRects() {
            double tileMargin = Math.Floor(MpMeasurements.Instance.ClipTileMargin * 1) - MpMeasurements.Instance.ClipTileBorderThickness;

            double offset = 5;
            double width = 15;
            List<Rect> targetRects = new List<Rect>();

            var pinTrayRect = new Rect(0, 0, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            var tileRects = AssociatedObject.PinTrayListBox.GetListBoxItemRects(RelativeToElement);
            bool isEmpty = tileRects.Count == 0;
            if(isEmpty) {
                var emptyRect = new Rect(0, 0, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
                tileRects.Add(emptyRect);
            }
            for (int i = 0; i < tileRects.Count; i++) {
                // NOTE drop rect is space preceding each tile after previous tile
                Rect targetRect = tileRects[i];
                if (i == 0) {
                    if(isEmpty) {
                        // when empty keep rect size of pin tray
                    } else {
                        targetRect.Location = new Point(0, 0);
                        targetRect.Width = 10;
                    }
                } else {
                    targetRect.Location = new Point(
                        targetRect.Location.X - offset,
                        targetRect.Location.Y);

                    targetRect.Width = width;
                }

                targetRects.Add(targetRect);

                bool isRowTail = i == tileRects.Count - 1 || (i < tileRects.Count - 1 && tileRects[i].Y < tileRects[i + 1].Y);

                if (isRowTail) {
                    Rect rowTailTargetRect = new Rect();

                    rowTailTargetRect.Location = new Point(
                        tileRects[i].Right - tileMargin,
                        targetRect.Location.Y);
                    rowTailTargetRect.Size = new Size(
                        Math.Max(10,pinTrayRect.Right - rowTailTargetRect.X),
                        tileRects[i].Height);

                    //Rect trayRect = AssociatedObject.PinTrayListBox.GetListBoxRect();
                    //if (trayRect.Right > rowTailTargetRect.Left) {
                    //    //when last tile is within viewport
                    //    rowTailTargetRect.Width = trayRect.Right - rowTailTargetRect.Left;
                    //} else {
                    //    rowTailTargetRect.Width = rowTailTargetRect.Left + tileMargin;
                    //}

                    targetRects.Add(rowTailTargetRect);
                }
            }
            return targetRects;
        }
        public override MpShape[] GetDropTargetAdornerShape() {
            var drl = GetDropTargetRects();
            if(DropIdx < 0 || DropIdx >= drl.Count) {
                return null;
            }
            var dr = drl[DropIdx];
            double x = dr.Left + (dr.Width / 2);
            if(DropIdx == 0) {
                x = 3;
                if(MpClipTrayViewModel.Instance.PinnedItems.Count == 0) {
                    x = AssociatedObject.ActualWidth / 2;
                }
            } else if(DropIdx == drl.Count -1 ||
                      (DropIdx < drl.Count - 1 && drl[DropIdx].Y != drl[DropIdx+1].Y)) {
                // when dropping at the end of a row adjust drop x because the rect is HUUGE
                // adjust so its spaced based on previous
                x = dr.Left + drl[DropIdx - 1].Width / 2;
            }
            //MpConsole.WriteLine("Tray DropLine X: " + x);
            return new MpLine(x, dr.Top, x, dr.Bottom).ToArray<MpShape>();
        }

        public override async Task StartDrop() { 
            await Task.Delay(1);
            AssociatedObject.PinTrayListBox.Width += _dragOverPadding;
        }

        public override void CancelDrop() {
            base.CancelDrop();
            AssociatedObject.PinTrayListBox.Width -= _dragOverPadding;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            var ctrvm = MpClipTrayViewModel.Instance;
            // BUG storing dropIdx because somehow it gets lost after calling base

            int dropIdx = DropIdx;

            await base.Drop(isCopy, dragData);

            MpClipTileViewModel drop_ctvm = null;
            
            if(dragData is MpPortableDataObject mpdo) {
                // from external source
                var dragModel = await MpCopyItemBuilder.CreateFromDataObject(mpdo);

                drop_ctvm = await ctrvm.CreateClipTileViewModel(dragModel);                
            } else if(dragData is MpClipTileViewModel drag_ctvm) {
                bool isPartialDrop = MpTextSelectionRangeExtension.IsAllSelected(drag_ctvm);
                if(isPartialDrop) {
                    string dragDataStr = MpContentDocumentRtfExtension.ExchangeDragDataWithDropTarget(drag_ctvm, isCopy, false);

                    var dragModel = await drag_ctvm.CopyItem.Clone(false) as MpCopyItem;
                    dragModel.ItemData = dragDataStr;
                    await dragModel.WriteToDatabaseAsync();

                    drop_ctvm = await ctrvm.CreateClipTileViewModel(dragModel);
                } else {
                    if(drag_ctvm.IsPinned) {
                        // when pinned item is moved must adjust drop if its after current idx
                        int prevIdx = ctrvm.PinnedItems.IndexOf(drag_ctvm);
                        if(prevIdx < dropIdx) {
                            //dropIdx--;
                        }
                    }
                    drop_ctvm = drag_ctvm;
                }        
            }
            
            if(!drop_ctvm.IsPinned) {
                // only toggle isPinned if drag item is not already in pin tray
                ctrvm.ToggleTileIsPinnedCommand.Execute(drop_ctvm);
            }

            while (ctrvm.IsAnyBusy) {
                await Task.Delay(100);
            }
                        
            // NOTE since end of row tiles have 2 rects dropIdx must be adjusted to actual insert idx
            var tileRects = AssociatedObject.PinTrayListBox.GetListBoxItemRects(RelativeToElement);
            for (int i = 0; i < tileRects.Count - 1; i++) {
                if (i == dropIdx) {
                    break;
                }
                if(tileRects[i].Y < tileRects[i+1].Y) {
                    //end of row tile so adjust dropIdx since it has 2 rects
                    dropIdx--;
                }
                if (i == dropIdx) {
                    // double check 
                    break;
                }
            }
            // NOTE need to re-assign drop_ctvm since toggle may create a new tile if it was on tray
            drop_ctvm = ctrvm.PinnedItems.FirstOrDefault(x => x.CopyItemId == drop_ctvm.CopyItemId);

            int curIdx = ctrvm.PinnedItems.IndexOf(drop_ctvm);
            if(curIdx >= 0 && curIdx < ctrvm.PinnedItems.Count &&
               dropIdx >= 0 && dropIdx < ctrvm.PinnedItems.Count) {
                ctrvm.PinnedItems.Move(curIdx, dropIdx);
            }

            while (ctrvm.IsAnyBusy) {
                await Task.Delay(100);
            }

            drop_ctvm.IsSelected = true;
        }

        public override void Reset() {
            base.Reset();

            _autoScrollVelocity = _baseAutoScrollVelocity;
        }
    }

}
