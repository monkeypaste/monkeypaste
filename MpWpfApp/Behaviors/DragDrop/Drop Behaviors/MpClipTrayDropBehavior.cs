using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using static SkiaSharp.SKImageFilter;

namespace MpWpfApp {
    public class MpClipTrayDropBehavior : MpDropBehaviorBase<MpClipTrayView> {
        #region Private Variables               
        private double _minScrollDist = 30;

        private double _autoScrollAccumulator = 5;
        private double _baseAutoScrollVelocity = 25;
        private double _autoScrollVelocity;
        #endregion

        public override Orientation AdornerOrientation => Orientation.Vertical;
        public override FrameworkElement AdornedElement => AssociatedObject.ClipTray;

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Tray;

        public override UIElement RelativeToElement => AssociatedObject.ClipTray.GetVisualDescendent<ScrollViewer>();

        public override MpCursorType MoveCursor => MpCursorType.TileMove;
        public override MpCursorType CopyCursor => MpCursorType.TileCopy;

        public override void OnLoaded() {
            base.OnLoaded();
        }
        protected override void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            base.ReceivedClipTrayViewModelMessage(msg);
            switch (msg) {
                case MpMessageType.TrayScrollChanged:
                    RefreshDropRects();
                    break;
            }
        }

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingMainWindowComplete:
                    RefreshDropRects();
                    break;
            }
        }

        public override int GetDropTargetRectIdx() {
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, RelativeToElement);
            Point trayMp = Mouse.GetPosition(RelativeToElement);

            Rect targetRect = DropRects.FirstOrDefault(x => x.Contains(trayMp));
            if (targetRect == null || targetRect.IsEmpty) {
                return -1;
            }
            return DropRects.IndexOf(targetRect);
        }

        public override void AutoScrollByMouse() {
            var ctr_mp = Mouse.GetPosition(AssociatedObject);
            Rect ctr_sv_rect = new Rect(0, 0, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            if(!ctr_sv_rect.Contains(ctr_mp)) {
                return;
            }

            double origScrollOfset = MpClipTrayViewModel.Instance.ScrollOffset;

            if (Math.Abs(ctr_sv_rect.Right - ctr_mp.X) <= _minScrollDist) {
                MpClipTrayViewModel.Instance.ScrollOffset += _autoScrollVelocity;
            } else if (Math.Abs(ctr_sv_rect.Left - ctr_mp.X) <= _minScrollDist) {
                MpClipTrayViewModel.Instance.ScrollOffset -= _autoScrollVelocity;
            }

            if(origScrollOfset != MpClipTrayViewModel.Instance.ScrollOffset) {
                _autoScrollVelocity += _autoScrollAccumulator;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            double margin = MpMeasurements.Instance.ClipTileMargin;
            double borderThickness = MpMeasurements.Instance.ClipTileBorderThickness;
            double tileMargin = Math.Floor(MpMeasurements.Instance.ClipTileMargin * 1) - MpMeasurements.Instance.ClipTileBorderThickness;

            double offset = 5;
            double width = 15;
            List<Rect> targetRects = new List<Rect>();

            var tileRects = AssociatedObject.ClipTray.GetListBoxItemRects(RelativeToElement);
            for (int i = 0; i < tileRects.Count; i++) {
                // NOTE drop rect is space preceding each tile after previous tile
                Rect targetRect = tileRects[i];
                if (i == 0) {
                    targetRect.Location = new Point(0, 0);

                    targetRect.Width = targetRect.Left + tileMargin;
                } 
                //else if(i < tileRects.Count - 1) {
                //    double curMidX = targetRect.Location.X + (targetRect.Width / 2);
                //    double nextMidX = tileRects[i + 1].Location.X + (tileRects[i + 1].Width / 2);
                //    targetRect.Location = new Point(
                //        targetRect.Location.X - tileMargin,
                //        targetRect.Location.Y);

                //    targetRect.Width = tileMargin * 2;
                //} 
                else {
                    targetRect.Location = new Point(
                        targetRect.Location.X - offset,
                        targetRect.Location.Y);

                    targetRect.Width = width;
                }

                targetRects.Add(targetRect);

                if (i == tileRects.Count - 1 &&
                   MpClipTrayViewModel.Instance.TailQueryIdx == MpClipTrayViewModel.Instance.TotalTilesInQuery - 1) {
                    Rect tailRect = tileRects[i];

                    tailRect.Location = new Point(
                        tailRect.Right - tileMargin,
                        targetRect.Location.Y);

                    Rect trayRect = AssociatedObject.ClipTray.GetListBoxRect();
                    if (trayRect.Right > tailRect.Left) {
                        //when last tile is within viewport
                        tailRect.Width = trayRect.Right - tailRect.Left;
                    } else {
                        tailRect.Width = tailRect.Left + tileMargin;
                    }

                    targetRects.Add(tailRect);
                }
            }
            return targetRects;
        }
        public override MpShape GetDropTargetAdornerShape() {
            var drl = GetDropTargetRects();
            if(DropIdx < 0 || DropIdx >= drl.Count) {
                return null;
            }
            var dr = drl[DropIdx];
            double x = dr.Left + (dr.Width / 2) + 2;
            return new MpLine(x, dr.Top, x, dr.Bottom);
        }

        public override async Task StartDrop() { 
            await Task.Delay(1);
            _autoScrollVelocity = _baseAutoScrollVelocity;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await base.Drop(isCopy, dragData);

            MpClipTileSortViewModel.Instance.SetToManualSort();            

            List<MpCopyItem> dragModels = isCopy ? await GetDragDataCopy(dragData) : dragData as List<MpCopyItem>;
            
            if(!isCopy) {
                dragModels = await Detach(dragModels);
            }

            int queryDropIdx = MpClipTrayViewModel.Instance.HeadQueryIdx + DropIdx;

            MpDataModelProvider.InsertQueryItem(dragModels[0].Id, queryDropIdx);

            MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
        }
    }

}
