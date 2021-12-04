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

namespace MpWpfApp {
    public class MpClipTrayDropBehavior : MpDropBehaviorBase<MpClipTrayView> {
        #region Private Variables

        private double _minScrollDist = 30;
        private double _autoScrollVelocity = 25;

        #endregion

        public override Orientation AdornerOrientation => Orientation.Vertical;
        public override FrameworkElement AdornedElement => AssociatedObject.ClipTray;

        public override bool IsEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Tray;

        public override UIElement RelativeToElement => AssociatedObject.ClipTray.GetVisualDescendent<ScrollViewer>();

        public override MpCursorType MoveCursor => MpCursorType.TileMove;
        public override MpCursorType CopyCursor => MpCursorType.TileCopy;

        public override void OnLoaded() {
            base.OnLoaded();
        }
        protected override void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayScrollChanged:
                    RefreshDropRects();
                    break;
            }
        }

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExpandComplete:
                    IsEnabled = false;
                    UpdateAdorner();
                    break;
                case MpMessageType.UnexpandComplete:
                    IsEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override void AutoScrollByMouse() {
            var ctr_mp = Mouse.GetPosition(RelativeToElement);
            Rect ctr_sv_rect = new Rect(0, 0, RelativeToElement.RenderSize.Width, RelativeToElement.RenderSize.Height);
            if(!ctr_sv_rect.Contains(ctr_mp)) {
                return;
            }

            if (Math.Abs(ctr_sv_rect.Right - ctr_mp.X) <= _minScrollDist) {
                MpClipTrayViewModel.Instance.ScrollOffset += _autoScrollVelocity;
            } else if (Math.Abs(ctr_sv_rect.Left - ctr_mp.X) <= _minScrollDist) {
                MpClipTrayViewModel.Instance.ScrollOffset -= _autoScrollVelocity;
            } 
        }

        public override List<Rect> GetDropTargetRects() {
            double tileMargin = MpMeasurements.Instance.ClipTileMargin + MpMeasurements.Instance.ClipTileBorderThickness;

            List<Rect> targetRects = new List<Rect>();

            var tileRects = AssociatedObject.ClipTray.GetListBoxItemRects(RelativeToElement);
            for (int i = 0; i < tileRects.Count; i++) {
                Rect targetRect = tileRects[i];
                if (i == 0) {
                    targetRect.Location = new Point(0, 0);

                    targetRect.Width = targetRect.Left + tileMargin;
                } else {
                    targetRect.Location = new Point(
                        targetRect.Location.X - tileMargin,
                        targetRect.Location.Y);

                    targetRect.Width = tileMargin * 2;
                }

                targetRects.Add(targetRect);

                if (i == tileRects.Count - 1 &&
                   MpClipTrayViewModel.Instance.TailQueryIdx == MpClipTrayViewModel.Instance.TotalItemsInQuery - 1) {
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

        public override async Task StartDrop() { await Task.Delay(1); }

        public override async Task Drop(bool isCopy, object dragData) {
            if(dragData == null || dragData.GetType() != typeof(List<MpCopyItem>)) {
                MpConsole.WriteTraceLine("Invalid drop data: " + dragData?.ToString());
                return;
            }
            MpClipTileSortViewModel.Instance.SetToManualSort();            

            List<MpCopyItem> dragModels = dragData as List<MpCopyItem>;
            if(isCopy) {
                var clones = await Task.WhenAll(dragModels.Select(x => x.Clone(true)).ToArray());
                dragModels = clones.Cast<MpCopyItem>().ToList();
            }
            for (int i = 0; i < dragModels.Count; i++) {
                if(dragModels[i].CompositeParentCopyItemId == 0 && 
                   i > 0 && !isCopy) {
                    //if dropping a former composite parent into non-parent idx
                    await MpDataModelProvider.Instance.RemoveQueryItem(dragModels[i].Id);
                }
                dragModels[i].CompositeSortOrderIdx = i;
                if (i == 0) {
                    dragModels[i].CompositeParentCopyItemId = 0;
                } else {
                    dragModels[i].CompositeParentCopyItemId = dragModels[0].Id;
                }
                await dragModels[i].WriteToDatabaseAsync();
            }

            int queryDropIdx = MpClipTrayViewModel.Instance.HeadQueryIdx + DropIdx;
            MpDataModelProvider.Instance.MoveOrInsertQueryItem(dragModels[0].Id, queryDropIdx);

            MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged(false);
        }
    }

}
