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
using Xamarin.Forms.Internals;

namespace MpWpfApp {
    public class MpContentListDropBehavior : MpDropBehaviorBase<MpContentListView> {
        #region Private Variables

        private double _minScrollDist = 10;
        private double _autoScrollVelocity = 10;

        #endregion

        public static double TargetMargin = 3;

        public override bool IsDropEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Tile;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

        public override FrameworkElement AdornedElement => AssociatedObject.ContentListBox;
        public override Orientation AdornerOrientation => Orientation.Horizontal;

        public override UIElement RelativeToElement => AssociatedObject.ContentListBox;

        public override void OnLoaded() {
            base.OnLoaded();

            _dataContext = AssociatedObject.DataContext;

            MpMessenger.Register<MpMessageType>(
                AssociatedObject.DataContext, 
                ReceivedAssociateObjectViewModelMessage,
                AssociatedObject.DataContext);
        }

        public override void OnUnloaded() {
            base.OnUnloaded();

            MpMessenger.Unregister<MpMessageType>(
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
                case MpMessageType.ResizingMainWindowComplete:
                    if (IsDropEnabled) {
                        RefreshDropRects();
                    }
                    IsDropEnabled = true;
                    UpdateAdorner();
                    break;
            }
        }

        public override List<Rect> GetDropTargetRects() {
            if(AssociatedObject == null ||
               AssociatedObject.ContentListBox.Items.Count == 0) {
                return new List<Rect>() { new Rect() };
            }

            switch(AssociatedObject.BindingContext.ItemType) {
                case MpCopyItemType.Text:
                    return GetRtbDropTargetRects();
                case MpCopyItemType.FileList:
                    return GetFileListDropTargetRects();
                //case MpCopyItemType.Image:
                //    return GetImageDropTargetRects();
                default:
                    return new List<Rect>();
            }
            
        }

        private List<Rect> GetRtbDropTargetRects() {
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
                double h;

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
            if (lastItemIdx < rtbvl.Count) {
                Rect lastItemRect = targetRects[lastItemIdx];
                double listRectBottom = AssociatedObject.ActualHeight;
                //if (lastItemRect.Bottom <= listRectBottom) 
                {
                    //due to async loading sometimes the rtb view's are not always loaded in time (probably for larger or heavy tokened items)
                    //or this item is a different content type so don't expect item list to be populated
                    double tailX = lastItemRect.Location.X;
                    double tailY = rtbvl.Count != itemRects.Count ?
                                         listRectBottom :
                                         rtbvl[lastItemIdx].Rtb.TranslatePoint(
                                             rtbvl[lastItemIdx].RtbViewDropBehavior.DropRects[1].BottomRight, 
                                             RelativeToElement)
                                                .Y;
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

        private List<Rect> GetFileListDropTargetRects() {
            double itemMargin = TargetMargin;
            List<Rect> targetRects = new List<Rect>();
            var flivl = AssociatedObject.GetVisualDescendents<MpFileListItemView>().ToList();
            if (flivl.Count == 0) {
                return new List<Rect>() { new Rect() };
            }

            var itemRects = AssociatedObject.ContentListBox.GetListBoxItemRects(RelativeToElement);

            for (int i = 0; i < itemRects.Count; i++) {
                double x = itemRects[i].Location.X;
                double y = itemRects[i].Location.Y - itemMargin;
                double w = itemRects[i].Width;
                double h = itemMargin * 2;

                if (i == 0) {
                    //for head drop make target from top of content all the way up to top of actual tile
                    y = -MpMeasurements.Instance.ClipTileTitleHeight;
                    h = MpMeasurements.Instance.ClipTileTitleHeight + itemMargin;
                } 
                targetRects.Add(new Rect(x, y, w, h));
            }

            int lastItemIdx = targetRects.Count - 1;
            if (lastItemIdx < flivl.Count) {
                Rect lastItemRect = targetRects[lastItemIdx];
                double listRectBottom = AssociatedObject.ActualHeight;

                double tailX = lastItemRect.Location.X;
                double tailY = lastItemRect.Location.Y - itemMargin;
                double tailW = lastItemRect.Width;
                double tailH = Math.Max(lastItemRect.Bottom, listRectBottom);

                targetRects.Add(new Rect(tailX, tailY, tailW, tailH));
            }

            return targetRects;
        }

        private List<Rect> GetImageDropTargetRects() {
            return new List<Rect>();
        }

        public override bool IsDragDataValid(bool isCopy, object dragData) {
            if(!base.IsDragDataValid(isCopy, dragData)) {
                return false;
            }
            return AssociatedObject.BindingContext.ItemType == (dragData as List<MpCopyItem>)[0].ItemType;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await base.Drop(isCopy, dragData);
            int origDropIdx = DropIdx;
            int tileIdx = MpClipTrayViewModel.Instance.Items.IndexOf(AssociatedObject.BindingContext);
                        
            List<MpCopyItem> dragModels = isCopy ? await GetDragDataCopy(dragData) : dragData as List<MpCopyItem>;
            List<MpCopyItem> dropModels = AssociatedObject.BindingContext.ItemViewModels.Select(x => x.CopyItem).OrderBy(x => x.CompositeSortOrderIdx).ToList();

            if(!isCopy) {
                bool isResort = dragModels.All(x => dropModels.Any(y => y.Id == x.Id));
                if (isResort) {
                    DropIdx = Math.Min(AssociatedObject.BindingContext.ItemViewModels.Count - 1, DropIdx);
                    //dragModels.Reverse();
                    foreach (var dragModel in dragModels) {
                        int dragIdx = AssociatedObject.BindingContext.ItemViewModels.IndexOf(AssociatedObject.BindingContext.ItemViewModels.FirstOrDefault(x => x.CopyItemId == dragModel.Id));
                        AssociatedObject.BindingContext.ItemViewModels.Move(dragIdx, DropIdx);
                    }
                    await AssociatedObject.BindingContext.UpdateSortOrderAsync();
                    return;
                }
                List<MpClipTileViewModel> dragTiles = new List<MpClipTileViewModel>();
                foreach (var dragModel in dragModels) {
                    var ctvm = MpClipTrayViewModel.Instance.GetClipTileViewModelById(dragModel.Id);
                    if (ctvm != null &&
                       !dragTiles.Contains(ctvm) &&
                       ctvm != AssociatedObject.BindingContext) {
                        dragTiles.Add(ctvm);
                        bool willBeRemoved = true;
                        foreach (var civm in ctvm.ItemViewModels) {
                            if (dragModels.All(x => x.Id != civm.CopyItemId)) {
                                willBeRemoved = false;
                                break;
                            }
                        }
                        if (willBeRemoved) {
                            int dragTileIdx = MpClipTrayViewModel.Instance.Items.IndexOf(ctvm);
                            if (dragTileIdx < tileIdx) {
                                tileIdx--;
                            }
                        }
                    }
                }
            }

            List<MpCopyItem> dropModelsToRemove = dropModels.Where(x => dragModels.Any(y => y.Id == x.Id)).ToList();
            foreach(var dropModelToRemove in dropModelsToRemove) {
                if(dropModelToRemove.CompositeSortOrderIdx < DropIdx && !isCopy) {
                    DropIdx--;
                }
                dropModels.Remove(dropModelToRemove);
            }
            
            dropModels.InsertRange(DropIdx, dragModels.OrderBy(x=>x.CompositeSortOrderIdx));

            dropModels = await Detach(dropModels, true);

            //if (AssociatedObject.BindingContext.IsExpanded) {
            //    await AssociatedObject.BindingContext.InitializeAsync(
            //        dropModels[0],
            //        AssociatedObject.BindingContext.QueryOffsetIdx);
            //} else {
            //    int queryDropIdx = MpClipTrayViewModel.Instance.HeadQueryIdx + tileIdx;
            //    MpDataModelProvider.InsertQueryItem(dropModels[0].Id, queryDropIdx);
            //    MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
            //}
            int queryDropIdx = MpClipTrayViewModel.Instance.HeadQueryIdx + tileIdx;
            MpDataModelProvider.InsertQueryItem(dropModels[0].Id, queryDropIdx);
            MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);

        }

        public override void AutoScrollByMouse() {
            var lb = AssociatedObject.ContentListBox;
            var ctr_mp = Mouse.GetPosition(RelativeToElement);
            Rect clb_rect = new Rect(0,0,lb.ActualWidth,lb.ActualHeight);
            if (!clb_rect.Contains(ctr_mp)) {
                return;
            }
            var sv = lb.GetVisualDescendent<ScrollViewer>();
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
        }
    }
}
