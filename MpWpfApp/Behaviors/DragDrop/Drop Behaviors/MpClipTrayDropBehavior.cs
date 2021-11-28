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

        private MpDropLineAdorner lineAdorner;
        private AdornerLayer adornerLayer;

        private DateTime _lastAutoScrollDateTime = DateTime.MinValue;

        private const int _MIN_AUTOSCROLL_DURATION_IN_MS = 1000;
        #endregion

        protected override FrameworkElement AdornedElement => AssociatedObject.ClipTray;

        public override int DropPriority => 1;

        public override void AutoScrollByMouse() {
            //TimeSpan timeSinceLastScroll = DateTime.Now - _lastAutoScrollDateTime;
            //if (timeSinceLastScroll.TotalMilliseconds < _MIN_AUTOSCROLL_DURATION_IN_MS) {
            //    return;
            //}

            //while(MpClipTrayViewModel.Instance.IsBusy) { await Task.Delay(10); }

            MpConsole.WriteLine("Autoscroll CALLED");
            double minScrollDist = 30;

            var ctr_mp = Mouse.GetPosition(AssociatedObject);
            Rect ctr_sv_rect = AssociatedObject.GetVisualDescendent<ScrollViewer>().Bounds();
            double right = AssociatedObject.GetVisualDescendent<ScrollViewer>().ActualWidth;
            if (Math.Abs(right - ctr_mp.X) <= minScrollDist) {
                _lastAutoScrollDateTime = DateTime.Now;
                MpConsole.WriteLine("Autoscroll RIGHT");
                MpClipTrayViewModel.Instance.ScrollOffset += (MpMeasurements.Instance.ClipTileMinSize/2);
            } else if (ctr_mp.X <= minScrollDist) {
                _lastAutoScrollDateTime = DateTime.Now;
                MpConsole.WriteLine("Autoscroll LEFT");
                MpClipTrayViewModel.Instance.ScrollOffset -= (MpMeasurements.Instance.ClipTileMinSize / 2);
            } else {
                Debugger.Break();
            }
        }

        public override List<Rect> GetDropTargetRects() {
            double tileMargin = MpMeasurements.Instance.ClipTileMargin + MpMeasurements.Instance.ClipTileBorderThickness;

            List<Rect> targetRects = new List<Rect>();

            var tileRects = AssociatedObject.ClipTray.GetListBoxItemRects(AssociatedObject.ClipTray.GetVisualDescendent<ScrollViewer>());
            for (int i = 0; i < tileRects.Count; i++) {                
                Rect targetRect = tileRects[i];
                if(i == 0) {
                    targetRect.Location = new Point(0, 0);

                    targetRect.Width = targetRect.Left + tileMargin;
                } else {
                    targetRect.Location = new Point(
                        targetRect.Location.X - tileMargin,
                        targetRect.Location.Y);

                    targetRect.Width = tileMargin * 2;
                }

                targetRects.Add(targetRect);

                if(i == tileRects.Count - 1 &&
                   MpClipTrayViewModel.Instance.TailQueryIdx == MpClipTrayViewModel.Instance.TotalItemsInQuery - 1) {
                    Rect tailRect = tileRects[i];

                    tailRect.Location = new Point(
                        tailRect.Right - tileMargin,
                        targetRect.Location.Y);

                    Rect trayRect = AssociatedObject.ClipTray.GetListBoxRect();
                    if(trayRect.Right > tailRect.Left) {
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

        public override Task Drop(object dragData) {
            throw new NotImplementedException();
        }
    }

}
