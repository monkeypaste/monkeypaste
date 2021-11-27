using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpClipTrayDropBehavior : Behavior<MpClipTrayView>, MpIContentDropTarget {
        #region Private Variables

        private MpDropLineAdorner lineAdorner;
        private AdornerLayer adornerLayer;

        private DateTime _lastAutoScrollDateTime = DateTime.MinValue;

        private const int _MIN_AUTOSCROLL_DURATION_IN_MS = 1000;
        #endregion

        #region Initialization

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            
            TargetId = MpContentDropManager.Instance.Register(this);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            InitAdorner();
            MpMainWindowViewModel.Instance.OnMainWindowHide += Instance_OnMainWindowHide;            
        }

        private void Instance_OnMainWindowHide(object sender, EventArgs e) {
            Reset();
        }

        #endregion

        #region MpIContentDropTarget Implementation

        #region Properties

        public int DropPriority => 1;

        public int TargetId { get; set; }

        #endregion

        #region Adorner

        public void InitAdorner() {
            lineAdorner = new MpDropLineAdorner(AssociatedObject.ClipTray);
            adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject.ClipTray);
            adornerLayer.Add(lineAdorner);

            //EnableDebugMode();

            UpdateAdorner();
        }

        public void UpdateAdorner() {
            if(lineAdorner.IsDebugMode) {
                lineAdorner.IsShowing = true;
                lineAdorner.DebugRects = GetDropTargetRects();
            }
            adornerLayer.Update();
        }

        public void EnableDebugMode() {
            lineAdorner.IsDebugMode = true;
            lineAdorner.DebugRects = GetDropTargetRects();
        }

        #endregion

        public void AutoScrollByMouse(MouseEventArgs e) {
            TimeSpan timeSinceLastScroll = DateTime.Now - _lastAutoScrollDateTime;
            if (timeSinceLastScroll.TotalMilliseconds < _MIN_AUTOSCROLL_DURATION_IN_MS) {
                return;
            }
            double minScrollDist = 30;

            var ctr_sv = AssociatedObject.ClipTray.GetScrollViewer();
            var ctr_lb = AssociatedObject.ClipTray;
            var ctr_mp = e.GetPosition(ctr_sv);
            Rect ctr_lb_rect = ctr_lb.Bounds();
            Rect ctr_sv_rect = ctr_sv.Bounds();
            if(Math.Abs(ctr_sv_rect.Left - ctr_mp.X) <= minScrollDist) {
                _lastAutoScrollDateTime = DateTime.Now;
                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.KeyboardPrev);
            } else if (Math.Abs(ctr_sv_rect.Right - ctr_mp.X) <= minScrollDist) {
                _lastAutoScrollDateTime = DateTime.Now;
                MpMessenger.Instance.Send<MpMessageType>(MpMessageType.KeyboardNext);
            }
        }

        public bool IsDragDataValid(object dragData) {
            if(dragData == null) {
                return false;
            }
            if(dragData is List<MpContentItemViewModel> dcivml) {
                if(dcivml.Count == 0) {
                    return false;
                }
                return dcivml.All(x => x.CopyItemType == dcivml[0].CopyItemType);
            }
            return false;
        }

        #endregion

        public void CancelDrop() {
            lineAdorner.IsShowing = false;
            lineAdorner.DropIdx = -1;
            UpdateAdorner();
            return;
        }

        public int GetDropTargetRectIdx(MouseEventArgs e) {
            Point trayMp = e.GetPosition(AssociatedObject.ClipTray);

            var dropRects = GetDropTargetRects();
            Rect targetRect = dropRects.FirstOrDefault(x => x.Contains(trayMp));
            if(targetRect == null || targetRect.IsEmpty) {
                return -1;
            }
            return dropRects.IndexOf(targetRect);
        }

        public void ContinueDragOverTarget(MouseEventArgs e) {
            lineAdorner.DropIdx = GetDropTargetRectIdx(e);
            UpdateAdorner();
        }

        public List<Rect> GetDropTargetRects() {
            double tileMargin = MpMeasurements.Instance.ClipTileMargin + MpMeasurements.Instance.ClipTileBorderThickness;

            List<Rect> targetRects = new List<Rect>();

            var tileRects = AssociatedObject.ClipTray.GetListBoxItemRects();
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

                if(i == tileRects.Count - 1) {
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

        public Task Drop(object dragData) {
            throw new NotImplementedException();
        }

        public void Reset() {
            
        }
    }

}
