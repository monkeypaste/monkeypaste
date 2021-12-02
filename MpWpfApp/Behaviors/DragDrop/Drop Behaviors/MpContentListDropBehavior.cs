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
using System.Windows.Media;

namespace MpWpfApp {
    public class MpContentListDropBehavior : MpDropBehaviorBase<MpContentListView> {
        #region Private Variables

        private double _minScrollDist = 10;
        private double _autoScrollVelocity = 10;

        private object _dataContext;

        #endregion

        public static double TargetMargin = 3;

        public override bool IsEnabled { get; set; } = true;

        public override int DropPriority => 2;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        public override FrameworkElement AdornedElement => AssociatedObject.ContentListBox;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        public override UIElement RelativeToElement => AssociatedObject.ContentListBox;//GetVisualDescendent<ScrollViewer>();

        public override void OnLoaded() {
            base.OnLoaded();

            _dataContext = AssociatedObject.DataContext;

            MpMessenger.Instance.Register<MpMessageType>(
                AssociatedObject.DataContext, 
                ReceivedAssociateObjectViewModelMessage,
                AssociatedObject.DataContext);
        }

        public override void OnUnloaded() {
            base.OnUnloaded();

            MpMessenger.Instance.Unregister<MpMessageType>(
                _dataContext,
                ReceivedAssociateObjectViewModelMessage,
                _dataContext);
        }

        private void ReceivedAssociateObjectViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentListItemsChanged:
                case MpMessageType.ContentListScrollChanged:
                    RefreshDropRects();
                    break;
            }
        }

        protected override void ReceivedMainWindowViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ExpandComplete:
                    if (AssociatedObject.BindingContext.IsExpanded) {
                        IsEnabled = true;
                        RefreshDropRects();
                    } else {
                        IsEnabled = false;
                    }
                    UpdateAdorner();
                    break;
                case MpMessageType.UnexpandComplete:
                    if (IsEnabled) {
                        RefreshDropRects();
                    }
                    IsEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            if(AssociatedObject == null ||
               AssociatedObject.ContentListBox.Items.Count == 0) {
                return new List<Rect>() { new Rect() };
            }
            double itemMargin = TargetMargin;
            List<Rect> targetRects = new List<Rect>();

            var rtbvl = AssociatedObject.GetVisualDescendents<MpRtbView>().ToList();
            if (rtbvl.Count == 0) {
                return new List<Rect>() { new Rect() };
            }
            var itemRects = AssociatedObject.ContentListBox.GetListBoxItemRects(RelativeToElement);

            for (int i = 0; i < itemRects.Count; i++) {
                double x = itemRects[i].Location.X;
                double y = itemRects[i].Location.Y - itemMargin;
                double w = itemRects[i].Width;
                double h = itemRects[i].Height;

                if (i == 0) {
                    //for head drop make target from top of content all the way up to top of actual tile
                    y = -MpMeasurements.Instance.ClipTileTitleHeight;
                    h = MpMeasurements.Instance.ClipTileTitleHeight + itemMargin;
                } else {
                    //for subsequent targets find prev end of content Y, if its within lbi height increase target rect
                    y = rtbvl.Count != itemRects.Count ?
                            y :
                            rtbvl[i - 1].Rtb.TranslatePoint(rtbvl[i - 1].RtbViewDropBehavior.DropRects[1].BottomRight, RelativeToElement).Y;

                    double prevItemBottom = itemRects[i - 1].Bottom;
                    if (y > prevItemBottom) {
                        y = prevItemBottom - itemMargin;
                    }
                    h = Math.Abs(itemRects[i].Top + itemMargin - y);
                }
                targetRects.Add(new Rect(x, y, w, h));
            }

            int lastItemIdx = targetRects.Count - 1;
            if(lastItemIdx < rtbvl.Count) {
                Rect lastItemRect = targetRects[lastItemIdx];
                double listRectBottom = AssociatedObject.ActualHeight;
                if (lastItemRect.Bottom <= listRectBottom) {
                    //due to async loading sometimes the rtb view's are not always loaded in time (probably for larger or heavy tokened items)
                    //or this item is a different content type so don't expect item list to be populated
                    double tailX = lastItemRect.Location.X;
                    double tailY = rtbvl.Count != itemRects.Count ?
                                         listRectBottom : 
                                         rtbvl[lastItemIdx].Rtb.TranslatePoint(rtbvl[lastItemIdx].RtbViewDropBehavior.DropRects[1].BottomRight, RelativeToElement).Y;
                    double tailW = lastItemRect.Width;
                    double tailH;

                    if (tailY > listRectBottom) {
                        tailY = listRectBottom - itemMargin;
                    }
                    tailH = Math.Abs(listRectBottom - tailY);

                    targetRects.Add(new Rect(tailX, tailY, tailW, tailH));
                }
            }

            return targetRects;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            if (!IsDragDataValid(dragData)) {
                MpConsole.WriteTraceLine("Invalid drop data: " + dragData?.ToString());
                return;
            }
            List<MpClipTileViewModel> dragTiles = new List<MpClipTileViewModel>();
            List<MpCopyItem> dragModels = dragData as List<MpCopyItem>;
            if (isCopy) {
                var clones = await Task.WhenAll(dragModels.Select(x => x.Clone(true)).ToArray());
                dragModels = clones.Cast<MpCopyItem>().ToList();
            }
            var dropModels = AssociatedObject.BindingContext.ItemViewModels.Select(x => x.CopyItem).OrderBy(x => x.CompositeSortOrderIdx).ToList();

            int actualDropIdx = DropIdx;
            if(!isCopy) {
                for (int i = 0; i < dragModels.Count; i++) {
                    if (dragModels[i].CompositeParentCopyItemId == 0 &&
                        !AssociatedObject.BindingContext.ItemViewModels.Any(x => x.CopyItemId == dragModels[i].Id)) {
                        //if drag item is head of ANOTHER tile swap or remove from main query ref w/ first child
                        await MpDataModelProvider.Instance.UpdateQuery(dragModels[i].Id, -1);
                    }
                    if (AssociatedObject.BindingContext.ItemViewModels.Any(x => x.CopyItemId == dragModels[i].Id)) {
                        //if drag item is part of this tile
                        if (actualDropIdx > dragModels[i].CompositeSortOrderIdx) {
                            actualDropIdx--;
                        }
                        dropModels.Remove(dragModels[i]);
                    } else {
                        var dcivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(dragModels[i].Id);
                        if (dcivm != null) {
                            //will be null if virtualized
                            if (!dragTiles.Contains(dcivm.Parent) && dcivm.Parent != AssociatedObject.DataContext) {
                                dragTiles.Add(dcivm.Parent);
                            }
                        }
                    }
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

            await AssociatedObject.BindingContext.InitializeAsync(dropModels[0],AssociatedObject.BindingContext.QueryOffsetIdx);

            if(!AssociatedObject.BindingContext.IsExpanded && !isCopy) {
                bool needsRequery = false;
                foreach (var dctvm in dragTiles) {
                    if (dropModels.Any(x => x.Id == dctvm.HeadItem.CopyItemId)) {
                        if (dctvm.ItemViewModels.Count == 1) {
                            //this tile needs to be removed and instead of shifting all the query idx's 
                            //just requery at current offset
                            needsRequery = true;
                            break;
                        }
                        await dctvm.InitializeAsync(dctvm.ItemViewModels[1].CopyItem, dctvm.QueryOffsetIdx);
                    } else {
                        await dctvm.InitializeAsync(dctvm.ItemViewModels[0].CopyItem, dctvm.QueryOffsetIdx);
                    }
                }

                if (needsRequery) {
                    MpClipTrayViewModel.Instance.RequeryCommand.Execute(MpClipTrayViewModel.Instance.HeadQueryIdx);
                }
            }

        }

        public override void AutoScrollByMouse() {
            var lb = AssociatedObject.ContentListBox;
            var ctr_mp = Mouse.GetPosition(RelativeToElement);
            Rect clb_rect = new Rect(0,0,lb.ActualWidth,lb.ActualHeight);
            if (!clb_rect.Contains(ctr_mp)) {
                //MpConsole.WriteLine($"Mouse point ({ctr_mp.X},{ctr_mp.Y}) not in rect ({clb_rect})");
                return;
            }
            var sv = lb.GetScrollViewer();
            double vertOffset = sv.VerticalOffset;
            if (Math.Abs(clb_rect.Top - ctr_mp.Y) <= _minScrollDist) {
                vertOffset -= _autoScrollVelocity;
            } else if (Math.Abs(clb_rect.Bottom - ctr_mp.Y) <= _minScrollDist) {
                vertOffset += _autoScrollVelocity;
            }
            sv.ScrollToVerticalOffset(vertOffset);
        }

        public override async Task StartDrop() { 
            await Task.Delay(1);
            MpConsole.WriteLine("New DropTarget, dropIdx: " + DropIdx);
        }
    }
}
