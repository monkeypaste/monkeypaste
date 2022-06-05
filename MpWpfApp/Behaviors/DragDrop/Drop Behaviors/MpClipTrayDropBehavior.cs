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
    public class MpClipTrayDropBehavior : MpDropBehaviorBase<MpClipTrayView> {
        #region Private Variables               
        private double _autoScrollMinScrollDist = 30;

        private double _autoScrollAccumulator = 5;
        private double _baseAutoScrollVelocity = 25;
        private double _autoScrollVelocity;
        #endregion

        public override Orientation AdornerOrientation => Orientation.Vertical;
        public override FrameworkElement AdornedElement => AssociatedObject.ClipTray;

        public override bool IsDropEnabled { get; set; } = false;

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
            var mp = Application.Current.MainWindow.TranslatePoint(gmp, AssociatedObject.ClipTray);
            //MpConsole.WriteLine("Global mp in tray coords: " + mp);
            var tray_rect = new Rect(0, 0, AssociatedObject.ClipTray.ActualWidth, AssociatedObject.ClipTray.ActualHeight);
            if(!tray_rect.Contains(mp)) {
                return -1;
            }
            Rect targetRect = DropRects.FirstOrDefault(x => x.Contains(mp));
            if (targetRect == null || targetRect.IsEmpty) {
                return -1;
            }
            return DropRects.IndexOf(targetRect);
        }

        public override void AutoScrollByMouse() {
            var gmp = MpShortcutCollectionViewModel.Instance.GlobalMouseLocation;
            var ctr_sv_mp = Application.Current.MainWindow.TranslatePoint(gmp, AssociatedObject.PagingScrollViewer);
            Rect ctr_sv_rect = new Rect(0, 0, AssociatedObject.PagingScrollViewer.ActualWidth, AssociatedObject.PagingScrollViewer.ActualHeight);
            if(!ctr_sv_rect.Contains(ctr_sv_mp)) {
                return;
            }

            double origScrollOfset = MpClipTrayViewModel.Instance.ScrollOffset;

            if (Math.Abs(ctr_sv_rect.Right - ctr_sv_mp.X) <= _autoScrollMinScrollDist) {
                MpClipTrayViewModel.Instance.ScrollOffset += _autoScrollVelocity;
            } else if (Math.Abs(ctr_sv_rect.Left - ctr_sv_mp.X) <= _autoScrollMinScrollDist) {
                MpClipTrayViewModel.Instance.ScrollOffset -= _autoScrollVelocity;
            }

            if(origScrollOfset != MpClipTrayViewModel.Instance.ScrollOffset) {
                _autoScrollVelocity += _autoScrollAccumulator;
            }
        }

        public override List<Rect> GetDropTargetRects() {
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

                    targetRect.Width = 10;// targetRect.Left + tileMargin;
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
        public override MpShape[] GetDropTargetAdornerShape() {
            var drl = GetDropTargetRects();
            if(DropIdx < 0 || DropIdx >= drl.Count) {
                return null;
            }
            var dr = drl[DropIdx];
            double x = dr.Left + (dr.Width / 2) + 2;
            if(DropIdx == 0) {
                x = 3;
            }
            //MpConsole.WriteLine("Tray DropLine X: " + x);
            return new MpLine(x, dr.Top, x, dr.Bottom).ToArray<MpShape>();
        }

        public override async Task StartDrop() { 
            await Task.Delay(1);
        }

        public override async Task Drop(bool isCopy, object dragData) {
            return;
            if(AssociatedObject == null) {
                return;
            }
            // BUG storing dropIdx because somehow it gets lost after calling base
            int dropIdx = DropIdx;

            await base.Drop(isCopy, dragData);

            MpClipTileSortViewModel.Instance.SetToManualSort();

            List<MpCopyItem> dragModels;
            
            if(dragData is MpPortableDataObject mpdo) {
                var ci = await MpCopyItemBuilder.CreateFromDataObject(mpdo);
                dragModels = new List<MpCopyItem>() { ci };
            } else {
                dragModels = isCopy ? await GetDragDataCopy(dragData) : dragData as List<MpCopyItem>;
            }
            

            int queryDropIdx = MpClipTrayViewModel.Instance.HeadQueryIdx + dropIdx;

            MpDataModelProvider.InsertQueryItem(dragModels[0].Id, queryDropIdx);

            MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);

            while (MpClipTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            var civm = MpClipTrayViewModel.Instance.GetClipTileViewModelById(dragModels[0].Id);
            if(civm != null) {
                civm.IsSelected = true;
            }
        }

        public override void Reset() {
            base.Reset();

            _autoScrollVelocity = _baseAutoScrollVelocity;
        }
    }

}
