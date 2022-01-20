using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpRtbViewDropBehavior : MpDropBehaviorBase<MpRtbView> {
        
        public override bool IsEnabled { get; set; } = true;

        public override MpDropType DropType => MpDropType.Content;

        public override UIElement RelativeToElement => AssociatedObject.Rtb;

        public override FrameworkElement AdornedElement => AssociatedObject.Rtb;

        public override Orientation AdornerOrientation => Orientation.Vertical;

        public override MpCursorType MoveCursor => MpCursorType.ContentMove;
        public override MpCursorType CopyCursor => MpCursorType.ContentCopy;

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
                    if (AssociatedObject.BindingContext.Parent.IsExpanded) {
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
            var homeAndEndRects = new List<Rect>() {
                AssociatedObject.Rtb.Document.ContentStart.GetCharacterRect(LogicalDirection.Forward),
                AssociatedObject.Rtb.Document.ContentEnd.GetCharacterRect(LogicalDirection.Backward)
            };
            return homeAndEndRects;
        }

        public override int GetDropTargetRectIdx() {
            var rtb = AssociatedObject.Rtb;
            var ctr_mp = Mouse.GetPosition(rtb.Document);
            Rect rtb_rect = new Rect(0, 0, rtb.ActualWidth, rtb.ActualHeight);
            if (!rtb_rect.Contains(ctr_mp)) {
                return -1;
            }

            double homeDist = GetDropTargetRects()[0].Location.Distance(ctr_mp);
            double endDist = GetDropTargetRects()[1].Location.Distance(ctr_mp);
            if (homeDist <= endDist) {
                return 0;
            } else {
                return 1;
            }
        }

        public override bool IsDragDataValid(bool isCopy,object dragData) {
            if(!base.IsDragDataValid(isCopy,dragData)) {
                return false;
            }
            if(AssociatedObject != null && dragData is List<MpCopyItem> ddl) {
                if(!isCopy && 
                   ddl.Any(x=>x.Id ==AssociatedObject.BindingContext.CopyItemId)) {
                    return false;
                }

                if(ddl[0].ItemType != MpCopyItemType.Text) {
                    return false;
                }
            }
            return true;
        }

        public override async Task Drop(bool isCopy, object dragData) {
            await base.Drop(isCopy, dragData);

            List<MpCopyItem> dragModels = isCopy ? await GetDragDataCopy(dragData) : dragData as List<MpCopyItem>;

            List<MpClipTileViewModel> dragTiles = new List<MpClipTileViewModel>();
            
            if(dragModels.Count == 1 && dragModels[0].Id == AssociatedObject.BindingContext.CopyItem.Id) {
                return;
            }
            

            if (!isCopy) {
                for (int i = 0; i < dragModels.Count; i++) {
                    if (dragModels[i].CompositeParentCopyItemId == 0 &&
                        !AssociatedObject.BindingContext.Parent.ItemViewModels.Any(x => x.CopyItemId == dragModels[i].Id)) {
                        //if drag item is head of ANOTHER tile swap or remove from main query ref w/ first child
                        await MpDataModelProvider.Instance.RemoveQueryItem(dragModels[i].Id);
                    }
                    var dcivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(dragModels[i].Id);
                    if (dcivm != null) {
                        //will be null if virtualized
                        if (!dragTiles.Contains(dcivm.Parent) && dcivm.Parent != AssociatedObject.DataContext) {
                            dragTiles.Add(dcivm.Parent);
                        }
                    }
                }
            }
            //now all drag items are removed from their tiles and dropIdx should be correct
            //reverse drag items and merge incrementally
            dragModels.Reverse();

            foreach (var dragModel in dragModels.ToList()) {
                await MergeContentItem(dragModel, isCopy);
            }

            //clean up all tiles with content dragged
            //if tile has no items recycle it
            //if it still has items sync sort order from its visuals
            if (!AssociatedObject.BindingContext.Parent.IsExpanded && !isCopy) {
                bool needsRequery = false;
                foreach (var dctvm in dragTiles) {
                    if (dctvm.HeadItem == null ||
                        dragModels.Any(x => x.Id == dctvm.HeadItem.CopyItemId)) {
                        if (dctvm.ItemViewModels.Count <= 1) {
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
                    MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged(false);
                    //MpClipTrayViewModel.Instance.RequeryCommand.Execute(MpClipTrayViewModel.Instance.HeadQueryIdx);
                }
            }
        }


        public async Task MergeContentItem(MpCopyItem mci, bool isDuplicating) {
            if(AssociatedObject == null) {
                return;
            }
            bool isHomeMerge = DropIdx == 0;

            AssociatedObject.BindingContext.IsBusy = true;

            await AssociatedObject.ClearHyperlinks();

            // merge content
            if (isHomeMerge) {
                AssociatedObject.BindingContext.CopyItem.ItemData = MpHelpers.CombineRichText(AssociatedObject.Rtb.Document.ToRichText(), mci.ItemData);
            } else {
                AssociatedObject.BindingContext.CopyItem.ItemData = MpHelpers.CombineRichText(mci.ItemData, AssociatedObject.Rtb.Document.ToRichText());
            }

            // merge templates
            var citl = await MpDataModelProvider.Instance.GetTemplatesAsync(AssociatedObject.BindingContext.CopyItemId);
            var mcitl = await MpDataModelProvider.Instance.GetTemplatesAsync(mci.Id);
            foreach (MpCopyItemTemplate mcit in mcitl) {
                if (citl.Any(x => x.TemplateName == mcit.TemplateName)) {
                    //if merged item has template w/ same name just ignore it since it will already be parsed
                    continue;
                }
                mcit.CopyItemId = AssociatedObject.BindingContext.CopyItemId;
                await mcit.WriteToDatabaseAsync();
            }

            // merge tags
            var tl = await MpDataModelProvider.Instance.GetCopyItemTagsForCopyItemAsync(AssociatedObject.BindingContext.CopyItemId);
            var mtl = await MpDataModelProvider.Instance.GetCopyItemTagsForCopyItemAsync(mci.Id);
            foreach (MpCopyItemTag mt in mtl) {
                if (tl.Any(x => x.TagId == mt.TagId)) {
                    //if merged item has tags w/ same name just ignore it 
                    continue;
                }
                mt.CopyItemId = AssociatedObject.BindingContext.CopyItemId;
                await mt.WriteToDatabaseAsync();
            }

            if (!isDuplicating) {
                await mci.DeleteFromDatabaseAsync();
            }

            // write and restore item
            await AssociatedObject.BindingContext.CopyItem.WriteToDatabaseAsync();

            AssociatedObject.BindingContext.OnPropertyChanged(nameof(AssociatedObject.BindingContext.CopyItemData));

            await AssociatedObject.CreateHyperlinksAsync(AssociatedObject.CTS.Token);
        }

        public override void AutoScrollByMouse() {
            if (DropIdx == 0) {
                AssociatedObject.BindingContext.ScrollToHomeCommand.Execute(null);
            } else if(DropIdx == 1) {
                AssociatedObject.BindingContext.ScrollToEndCommand.Execute(null);
            }
        }

        public override async Task StartDrop() {
            await Task.Delay(1);
        }
    }

}
