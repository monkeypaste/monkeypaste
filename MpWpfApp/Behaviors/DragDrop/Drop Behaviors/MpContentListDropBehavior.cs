using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpContentListDropBehavior : MpDropBehaviorBase<MpContentListView> {
        #region Private Variables

        private double _minScrollDist = 10;
        private double _autoScrollVelocity = 10;

        #endregion

        public override bool IsEnabled { get; set; } = true;

        public override int DropPriority => 2;

        protected override FrameworkElement AdornedElement => AssociatedObject.ContentListBox;

        public override Orientation AdornerOrientation => Orientation.Horizontal;

        protected override void OnLoaded() {
            //DropRects.Clear();
        }

        protected override void ReceivedAssociateObjectViewModelMessage(MpMessageType msg) {
            //switch(msg) {
            //    case MpMessageType.ContentListScrollChanged:
            //        _dropRects = GetDropTargetRects();
            //        UpdateAdorner();
            //        break;
            //}
        }

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExpandComplete:
                    if (AssociatedObject.BindingContext.IsExpanded) {
                        IsEnabled = true;
                        _dropRects = GetDropTargetRects();
                    } else {
                        IsEnabled = false;
                        //_dropRects.Clear();
                    }
                    UpdateAdorner();
                    break;
                case MpMessageType.UnexpandComplete:
                    if (IsEnabled) {
                        //the item that was expanded

                        _dropRects = GetDropTargetRects();
                    }
                    IsEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            double itemMargin = 3;

            List<Rect> targetRects = new List<Rect>();

            var rtbvl = AssociatedObject.GetVisualDescendents<MpRtbView>().ToList();
            var tileRects = AssociatedObject.ContentListBox.GetListBoxItemRects();
            for (int i = 0; i < tileRects.Count; i++) {
                Rect targetRect = tileRects[i];
                targetRect.Location = new Point(
                        targetRect.Location.X,
                        targetRect.Location.Y - itemMargin);

                if (i == 0) {
                    targetRect.Location = new Point(targetRect.Location.X, -MpMeasurements.Instance.ClipTileTitleHeight);
                    targetRect.Height = MpMeasurements.Instance.ClipTileTitleHeight + itemMargin;
                } else {
                    int refIdx = Math.Max(0, i - 1);
                    double adjustedY = double.MaxValue;
                    if (i < rtbvl.Count) {
                        adjustedY = rtbvl[refIdx].Rtb.TranslatePoint(rtbvl[refIdx].EndCaretLine[1], AssociatedObject.ContentListBox).Y;
                    }

                    Rect prevItemRect = AssociatedObject.ContentListBox.GetListBoxItemRect(refIdx);
                    if (adjustedY > prevItemRect.Bottom) {
                        adjustedY = prevItemRect.Bottom - itemMargin;
                    }
                    targetRect.Location = new Point(
                        targetRect.Location.X,
                        adjustedY);
                    targetRect.Height = Math.Abs(tileRects[i].Top + itemMargin - adjustedY );

                    //targetRect.Height = itemMargin * 2;
                }
                targetRects.Add(targetRect);

                if (i == tileRects.Count - 1) {
                    Rect listRect = new Rect(new Point(), new Size(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight));
                    if (targetRect.Bottom <= listRect.Bottom) {
                        Rect tailRect = targetRect;
                        //due to async loading sometimes the rtb view's are not always loaded in time (probably for larger or heavy tokened items)
                        //or this item is a different content type so don't expect item list to be populated
                        double tailY = double.MaxValue;
                        if (i < rtbvl.Count) {
                            tailY = rtbvl[i].Rtb.TranslatePoint(rtbvl[i].EndCaretLine[1], AssociatedObject.ContentListBox).Y;
                        } else {
                            //Debugger.Break();
                        }

                        if (tailY > listRect.Bottom) {
                            tailY = listRect.Bottom - itemMargin;
                        }
                        tailRect.Location = new Point(
                            tailRect.Location.X,
                            tailY);
                        tailRect.Height = Math.Abs(listRect.Bottom - tailY);

                        targetRects.Add(tailRect);
                    }
                } 
            }

            return targetRects;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            if (dragData == null || dragData.GetType() != typeof(List<MpCopyItem>)) {
                MpConsole.WriteTraceLine("Invalid drop data: " + dragData?.ToString());
                return;
            }

            List<MpCopyItem> dragModels = dragData as List<MpCopyItem>;
            var dropModels = AssociatedObject.BindingContext.ItemViewModels.Select(x => x.CopyItem).OrderBy(x => x.CompositeSortOrderIdx).ToList();

            int actualDropIdx = DropIdx;
            for (int i = 0; i < dragModels.Count; i++) {
                if (dragModels[i].CompositeParentCopyItemId == 0 &&
                    i > 0 &&
                    !AssociatedObject.BindingContext.ItemViewModels.Any(x=>x.CopyItemId == dragModels[i].Id)) {
                    await MpDataModelProvider.Instance.UpdateQuery(dragModels[i].Id, -1);
                }
                if(AssociatedObject.BindingContext.ItemViewModels.Any(x => x.CopyItemId == dragModels[i].Id)) {
                    if(actualDropIdx > dragModels[i].CompositeSortOrderIdx) {
                        actualDropIdx--;
                    }                    
                    dropModels.Remove(dragModels[i]);
                }
            }
            dragModels.Reverse();
            dropModels.InsertRange(actualDropIdx, dragModels);
            for (int i = 0; i < dropModels.Count; i++) {
                dropModels[i].CompositeSortOrderIdx = i;
                if (i == 0) {
                    dropModels[i].CompositeParentCopyItemId = 0;
                } else {
                    dropModels[i].CompositeParentCopyItemId = dropModels[0].Id;
                }
                await dropModels[i].WriteToDatabaseAsync();
            }

            await AssociatedObject.BindingContext.InitializeAsync(dropModels[0]);
        }

        public override void AutoScrollByMouse(MouseEventArgs e) {
            return;

            var lb = AssociatedObject.ContentListBox;
            var ctr_mp = e.GetPosition(lb);
            Rect clb_rect = lb.Bounds();
            if (!clb_rect.Contains(ctr_mp)) {
                MpConsole.WriteLine($"Mouse point ({ctr_mp.X},{ctr_mp.Y}) not in rect ({clb_rect})");
                return;
            }
            var sv = lb.GetScrollViewer();
            double vertOffset = sv.VerticalOffset;
            if (Math.Abs(clb_rect.Top - ctr_mp.X) <= _minScrollDist) {
                vertOffset += _autoScrollVelocity;
            } else if (Math.Abs(clb_rect.Left - ctr_mp.X) <= _minScrollDist) {
                vertOffset -= _autoScrollVelocity;
            }
            sv.ScrollToVerticalOffset(vertOffset);
        }

        public override async Task StartDrop() { await Task.Delay(1); }
    }
}
