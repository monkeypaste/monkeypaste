using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;
using System.Windows.Threading;
using System.Diagnostics;

namespace MpWpfApp {
    public enum MpDropType {
        None,
        Content,
        Tile,
        Tray,
        External
    }

    public abstract class MpDropBehaviorBase<T> : Behavior<T>, MpIContentDropTarget where T : FrameworkElement {
        #region Private Variables
        
        private AdornerLayer adornerLayer;

        private bool _hasRegistered = false;

        #endregion

        #region Properties
                
        public MpDropLineAdorner DropLineAdorner { get; set; }

        public int DropIdx { get; set; } = -1;

        public object DataContext => AssociatedObject?.DataContext;

        public abstract bool IsEnabled { get; set; }
        public abstract MpDropType DropType { get; }

        public abstract UIElement RelativeToElement { get; }

        public List<Rect> DropRects => GetDropTargetRects();

        public abstract FrameworkElement AdornedElement { get; }
        public abstract Orientation AdornerOrientation { get; }
        #endregion


        public MpDropBehaviorBase() { }

        protected override void OnAttached() {
            base.OnAttached();

            MpMainWindowViewModel.Instance.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;

            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;

            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
            //MpContentDropManager.Instance.Register(this);
        }

        private void AssociatedObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(AssociatedObject?.DataContext != null) {
                Attach(AssociatedObject);
            }
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            Detach();
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            if(AssociatedObject != null) {
                AssociatedObject.Loaded -= AssociatedObject_Loaded;
                AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            }
            OnUnloaded();
            //MpContentDropManager.Instance.Unregister(this);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {            
            OnLoaded();
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            if(DropType == MpDropType.External) {
                return;
            }
            Reset();
        }

        protected virtual void ReceivedClipTrayViewModelMessage(MpMessageType msg) { }
        protected virtual void ReceivedMainWindowViewModelMessage(MpMessageType msg) { }

        public virtual void OnLoaded() {
            MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
            MpMessenger.Instance.Register<MpMessageType>(MpMainWindowViewModel.Instance, ReceivedMainWindowViewModelMessage);

            InitAdorner();
        }

        public virtual void OnUnloaded() {
            MpMessenger.Instance.Unregister<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
            MpMessenger.Instance.Unregister<MpMessageType>(MpMainWindowViewModel.Instance, ReceivedMainWindowViewModelMessage);

        }
        
        public abstract MpCursorType MoveCursor { get; }
        public abstract MpCursorType CopyCursor { get; }
        public abstract List<Rect> GetDropTargetRects();
        public abstract Task Drop(bool isCopy, object dragData);
        public abstract void AutoScrollByMouse();
                

        public void CancelDrop() {
            Reset();
        }

        public void Reset() {
            DropIdx = -1;
            UpdateAdorner();

            //MpRtbView.ClearCaretAdorner();

            //if (!isTrayDrop) {
            //    if (AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
            //        ctvm.DropIdx = -1;
            //    } else if (AssociatedObject.DataContext is MpContentItemViewModel civm) {
            //        civm.Parent.DropIdx = -1;
            //    }
            //}
            //AssociatedObject.GetScrollViewer()?.ScrollToHome();
        }

        public virtual bool IsDragDataValid(object dragData) {
            if (dragData == null) {
                return false;
            }
            if (dragData is List<MpCopyItem> dcil) {
                if (dcil.Count == 0) {
                    return false;
                }
                return dcil.All(x => x.ItemType == dcil[0].ItemType);
            } else if (dragData is List<MpContentItemViewModel> dcivml) {
                if (dcivml.Count == 0) {
                    return false;
                }
                return dcivml.All(x => x.CopyItemType == dcivml[0].CopyItemType);
            }
            return false;
        }

        public abstract Task StartDrop();

        public virtual int GetDropTargetRectIdx() {
            Point trayMp = Mouse.GetPosition(RelativeToElement);

            Rect targetRect = DropRects.FirstOrDefault(x => x.Contains(trayMp));
            if (targetRect == null || targetRect.IsEmpty) {
                return -1;
            }
            return DropRects.IndexOf(targetRect);
        }

        public void ContinueDragOverTarget() {
            DropIdx = GetDropTargetRectIdx();
            if(DropIdx >= 0) {
                MpConsole.WriteLine("DropIdx: " + DropIdx);
            }
            UpdateAdorner();
        }

        public void InitAdorner() {
            DropLineAdorner = new MpDropLineAdorner(AdornedElement,this);
            adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            adornerLayer.Add(DropLineAdorner);

            if(GetType() != typeof(MpExternalDropBehavior)) {
                //EnableDebugMode();
            }

            RefreshDropRects();
        }

        public void RefreshDropRects() {
            UpdateAdorner();
        }

        public void UpdateAdorner() {
            if(adornerLayer == null) {
                InitAdorner();
            }
            adornerLayer?.Update();
        }

        public void EnableDebugMode() {
            DropLineAdorner.IsDebugMode = true;
        }



        //public void Drop(bool isCopy = false) {
        //    MpHelpers.Instance.RunOnMainThreadAsync((Func<Task>)(async () => {
        //        if (this.dragTiles == null || this.dragTiles.Count == 0) {
        //            Reset();
        //            return;
        //        }

        //        MpConsole.WriteLine("DropIdx: " + DropIdx);
        //        MpClipTileViewModel dropTile = null;
        //        var dragModels = MpClipTrayViewModel.Instance.SelectedModels;
        //        int tileCount = MpClipTrayViewModel.Instance.Items.Count;
        //        if (isTrayDrop) {
        //            if (DropIdx > MpClipTrayViewModel.Instance.Items.Count) {
        //                DropIdx = MpClipTrayViewModel.Instance.Items.Count;

        //                MpConsole.WriteLine("Dropping at visible tail, reset tray dropIdx: " + DropIdx);
        //            }
        //            bool isTileResort = dragTiles.All(x => x.SelectedItems.Count == x.ItemViewModels.Count);
        //            if (isTileResort) {
        //                //For full tile's moved on tray reverse order and use standard move
        //                dragTiles.Reverse();
        //                foreach (var dragTile in dragTiles) {
        //                    int oldIdx = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
        //                    if (oldIdx < DropIdx) {
        //                        DropIdx--;
        //                        MpConsole.WriteLine("Decrementing tray dropIdx: " + DropIdx);
        //                    }
        //                    //to prevent bounds exceptions
        //                    DropIdx = Math.Min(Math.Max(0, DropIdx), (int)(MpClipTrayViewModel.Instance.Items.Count - 1));
        //                    oldIdx = Math.Min(Math.Max(0, oldIdx), (int)(MpClipTrayViewModel.Instance.Items.Count - 1));
        //                    MpClipTrayViewModel.Instance.Items.Move(oldIdx, DropIdx);
        //                }
        //            } else {
        //                //some collection of items dropped onto tray
        //                //recycle tail item to use for dropped content
        //                dropTile = MpClipTrayViewModel.Instance.TailItem; //MpClipTrayViewModel.Instance.CreateClipTileViewModel(null);
        //                if (dragTiles.Any(x => x == dropTile)) {
        //                    // special case where items are dragged from last item
        //                    // create a new tile at end of list

        //                    dropTile = await MpClipTrayViewModel.Instance.CreateClipTileViewModel(null);
        //                    MpClipTrayViewModel.Instance.Items.Add(dropTile);
        //                }
        //                foreach (var dragTile in dragTiles) {
        //                    //remove drag content from their tiles
        //                    foreach (var dci in dragModels) {
        //                        var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
        //                        if (dcivm != null) {
        //                            dragTile.ItemViewModels.Remove(dcivm);
        //                        }
        //                    }
        //                }
        //                //update drag items into new composite 
        //                //selected models is ordered by select time ascending
        //                //dragModels.Reverse();
        //                for (int i = 0; i < dragModels.Count; i++) {
        //                    dragModels[i].CompositeSortOrderIdx = i;
        //                    if (i == 0) {
        //                        dragModels[i].CompositeParentCopyItemId = 0;
        //                    } else {
        //                        dragModels[i].CompositeParentCopyItemId = dragModels[0].Id;
        //                    }
        //                    await dragModels[i].WriteToDatabaseAsync();
        //                }
        //                foreach (var dragTile in dragTiles) {
        //                    //clean up tiles removed items and recycle if empty
        //                    if (dragTile.Count == 0) {
        //                        int dragIdxToRemove = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
        //                        if (dragIdxToRemove < DropIdx) {
        //                            DropIdx--;
        //                            MpConsole.WriteLine("Decrementing tray dropIdx: " + DropIdx);
        //                        }
        //                        //await dragTile.InitializeAsync(null);
        //                        //MpClipTrayViewModel.Instance.Items.Move(dragIdxToRemove, MpClipTrayViewModel.Instance.Items.Count - 1);
        //                        MpClipTrayViewModel.Instance.Items.RemoveAt(dragIdxToRemove);
        //                    } else {
        //                        await dragTile.UpdateSortOrderAsync();
        //                        await dragTile.InitializeAsync(dragTile.HeadItem.CopyItem);
        //                    }
        //                }
        //                await dropTile.InitializeAsync(dragModels[0]);
        //                int oldDropIdx = MpClipTrayViewModel.Instance.Items.IndexOf(dropTile);
        //                //to prevent bounds exceptions
        //                DropIdx = Math.Min(Math.Max(0, DropIdx), (int)(MpClipTrayViewModel.Instance.Items.Count - 1));
        //                oldDropIdx = Math.Min(Math.Max(0, oldDropIdx), (int)(MpClipTrayViewModel.Instance.Items.Count - 1));
        //                MpClipTrayViewModel.Instance.Items.Move(oldDropIdx, DropIdx);

        //                dropTile.RequestUiUpdate();
        //                MpClipTrayViewModel.Instance.ClearClipSelection();
        //                dropTile.SubSelectAll();
        //                dropTile.ItemViewModels.ForEach(x => x.IsItemDragging = false);
        //            }
        //            MpClipTileSortViewModel.Instance.SetToManualSort();
        //        } else {
        //            if (AssociatedObject.DataContext is MpClipTileViewModel) {
        //                dropTile = AssociatedObject.DataContext as MpClipTileViewModel;
        //                bool isContentResort = dragTiles.Count == 1 && dragTiles[0] == dropTile;
        //                if (isContentResort) {
        //                    //For items moved within a tile reverse order and move
        //                    var dragCivml = dragTiles[0].SelectedItems;
        //                    dragCivml.Reverse();
        //                    foreach (var dragCivm in dragCivml) {
        //                        int oldIdx = dragTiles[0].ItemViewModels.IndexOf(dragCivm);
        //                        if (oldIdx < DropIdx) {
        //                            DropIdx--;
        //                            MpConsole.WriteLine("Decrementing tile dropIdx: " + DropIdx);
        //                        }
        //                        //to prevent bounds exceptions
        //                        DropIdx = Math.Min(Math.Max(0, DropIdx), dragTiles[0].ItemViewModels.Count - 1);
        //                        oldIdx = Math.Min(Math.Max(0, oldIdx), dragTiles[0].ItemViewModels.Count - 1);
        //                        dragTiles[0].ItemViewModels.Move(oldIdx, DropIdx);
        //                    }
        //                } else {
        //                    foreach (var dragTile in dragTiles) {
        //                        //remove all items from their drag tiles
        //                        foreach (var dci in dragModels) {
        //                            //check if drag item is in this drag tile
        //                            var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
        //                            if (dcivm != null) {
        //                                //drag item is in drag tile
        //                                if (dragTile == dropTile) {
        //                                    // drag item is IN the drop tile
        //                                    int dcivmIdx = dragTile.ItemViewModels.IndexOf(dcivm);
        //                                    if (dcivmIdx < DropIdx) {
        //                                        //drag item idx is lower than drop idx so adjust
        //                                        DropIdx--;
        //                                        MpConsole.WriteLine("Decrementing tile dropIdx: " + DropIdx);
        //                                    }
        //                                }
        //                                dragTile.ItemViewModels.Remove(dcivm);
        //                            }
        //                        }
        //                    }
        //                    //now all drag items are removed from their tiles and dropIdx should be correct
        //                    //range insert drag items into drop and reorder based on drop idx
        //                    dragModels.Reverse();
        //                    var dropModels = dropTile.ItemViewModels.Select(x => x.CopyItem).ToList();

        //                    //to prevent bounds exceptions
        //                    DropIdx = Math.Min(Math.Max(0, DropIdx), dropModels.Count - 1);
        //                    dropModels.InsertRange(DropIdx, dragModels);
        //                    for (int i = 0; i < dropModels.Count; i++) {
        //                        dropModels[i].CompositeSortOrderIdx = i;
        //                        if (i == 0) {
        //                            dropModels[i].CompositeParentCopyItemId = 0;
        //                        } else {
        //                            dropModels[i].CompositeParentCopyItemId = dropModels[0].Id;
        //                        }
        //                        await dropModels[i].WriteToDatabaseAsync();
        //                    }
        //                    //clean up all tiles with content dragged
        //                    //if tile has no items recycle it
        //                    //if it still has items sync sort order from its visuals
        //                    foreach (var dragTile in dragTiles) {
        //                        if (dragTile == dropTile) {
        //                            continue;
        //                        }
        //                        if (dragTile.Count == 0) {
        //                            //recycle empty tile
        //                            int dragIdxToRemove = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
        //                            MpClipTrayViewModel.Instance.Items.RemoveAt(dragIdxToRemove);
        //                        } else {
        //                            await dragTile.UpdateSortOrderAsync();
        //                            await dragTile.InitializeAsync(dragTile.HeadItem.CopyItem);
        //                        }
        //                    }
        //                    await dropTile.InitializeAsync(dropModels[0]);
        //                }
        //            } else {
        //                // drop is content merge
        //                dropTile = (AssociatedObject.DataContext as MpContentItemViewModel).Parent;
        //                foreach (var dragTile in dragTiles) {
        //                    //remove all items from their drag tiles
        //                    foreach (var dci in dragModels) {
        //                        //check if drag item is in this drag tile
        //                        var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
        //                        if (dcivm != null) {
        //                            //drag item is in drag tile
        //                            if (dragTile == dropTile) {
        //                                // drag item is IN the drop tile
        //                                int dcivmIdx = dragTile.ItemViewModels.IndexOf(dcivm);
        //                                if (dcivmIdx < DropIdx) {
        //                                    //drag item idx is lower than drop idx so adjust
        //                                    DropIdx--;
        //                                    MpConsole.WriteLine("Decrementing tile dropIdx: " + DropIdx);
        //                                }
        //                            }
        //                            dragTile.ItemViewModels.Remove(dcivm);
        //                        }
        //                    }
        //                }
        //                //now all drag items are removed from their tiles and dropIdx should be correct
        //                //reverse drag items and merge incrementally
        //                var dropItemView = AssociatedObject.GetVisualAncestor<MpRtbView>();
        //                dragModels.Reverse();

        //                foreach (var dragModel in dragModels) {
        //                    await dropItemView.MergeContentItem(dragModel, isCopy);
        //                }
        //                //clean up all tiles with content dragged
        //                //if tile has no items recycle it
        //                //if it still has items sync sort order from its visuals
        //                foreach (var dragTile in dragTiles) {
        //                    if (dragTile == dropTile) {
        //                        continue;
        //                    }
        //                    if (dragTile.Count == 0) {
        //                        //recycle empty tile
        //                        int dragIdxToRemove = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
        //                        MpClipTrayViewModel.Instance.Items.RemoveAt(dragIdxToRemove);
        //                    } else {
        //                        await dragTile.UpdateSortOrderAsync();
        //                        await dragTile.InitializeAsync(dragTile.HeadItem.CopyItem);
        //                    }
        //                }
        //                //await dropTile.InitializeAsync(dropTile.HeadItem.CopyItem);
        //            }
        //        }

        //        while (MpClipTrayViewModel.Instance.IsAnyBusy) {
        //            await Task.Delay(100);
        //        }

        //        Reset();

        //        if (!isTrayDrop) {
        //            MpClipTrayViewModel.Instance.ClearClipSelection();
        //            if (AssociatedObject.DataContext is MpClipTileViewModel) {
        //                MpClipTileViewModel selectedTile = null;
        //                foreach (var dragModel in dragModels) {
        //                    var dcivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(dragModel.Id);
        //                    if (selectedTile == null) {
        //                        selectedTile = dcivm.Parent;
        //                        selectedTile.AllowMultiSelect = true;
        //                    }
        //                    dcivm.IsSelected = true;
        //                    dcivm.IsItemDragging = false;
        //                }
        //                selectedTile.AllowMultiSelect = false;
        //            } else {
        //                // var lb = AssociatedObject.FindParentOfType<ListBox>();
        //                //var lbi = AssociatedObject.FindParentOfType<ListBoxItem>();
        //                AssociatedObject.FindParentOfType<MpRtbView>().BindingContext.IsSelected = true;
        //                //lb.ScrollIntoView(lbi);
        //            }
        //        }


        //        MpExtensions.ForEach<MpClipTileViewModel>(MpClipTrayViewModel.Instance.Items, (Action<MpClipTileViewModel>)(x =>
        //             x.ItemViewModels.ForEach((Action<MpContentItemViewModel>)(y => y.IsItemDragging = false))));
        //    }));
        //}

        //private void UpdateDropLineAdorner() {
        //    Rect overRect;
        //    bool isTail = false;
        //    ListBox lb = AssociatedObject as ListBox;
        //    if (lb == null) {
        //        // content merge adorner handled in drag
        //        lineAdorner.IsShowing = false;
        //        adornerLayer.Update();
        //        return;
        //    }
        //    if (DropIdx < lb.Items.Count) {
        //        overRect = lb.GetListBoxItemRect(DropIdx);
        //    } else {
        //        overRect = lb.GetListBoxItemRect(lb.Items.Count - 1);
        //        isTail = true;
        //    }

        //    if (isTrayDrop) {
        //        if (isTail) {
        //            lineAdorner.Points[0] = overRect.TopRight;
        //            lineAdorner.Points[1] = overRect.BottomRight;
        //        } else {
        //            lineAdorner.Points[0] = overRect.TopLeft;
        //            lineAdorner.Points[1] = overRect.BottomLeft;
        //        }
        //    } else {
        //        if (isTail) {
        //            lineAdorner.Points[0] = overRect.BottomLeft;
        //            lineAdorner.Points[1] = overRect.BottomRight;
        //        } else {
        //            lineAdorner.Points[0] = overRect.TopLeft;
        //            lineAdorner.Points[1] = overRect.TopRight;
        //        }
        //    }

        //    lineAdorner.IsShowing = true;
        //    adornerLayer.Update();
        //}

        //public void AutoScrollByMouse() {
        //    //during drop autoscroll listbox to beginning or end of list
        //    //if more items are there depending on which half of the visible list
        //    //the mouse is in

        //    double minScrollDist = 30;
        //    double maxAutoScrollOffset = 50;
        //    double exp = 1.5;

        //    if (isTrayDrop) {
        //        var ctr_lb = AssociatedObject as ListBox;
        //        var ctr_sv = ctr_lb.GetScrollViewer();

        //        var ctr_mp = Mouse.GetPosition(ctr_lb);
        //        Rect ctr_rect = ctr_lb.GetListBoxRect();
        //        double ctr_midX = ctr_rect.Left + (ctr_rect.Width / 2);

        //        double leftDiff = ctr_mp.X - ctr_rect.Left;
        //        double rightDiff = ctr_rect.Right - ctr_mp.X;
        //        double targetOffsetX = 0;
        //        if (leftDiff < minScrollDist) {
        //            double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(ctr_midX - ctr_mp.X, exp));
        //            targetOffsetX = ctr_sv.HorizontalOffset - autoScrollOffset;
        //        } else if (rightDiff < minScrollDist) {
        //            double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(ctr_mp.X - ctr_midX, exp));
        //            targetOffsetX = ctr_sv.HorizontalOffset + autoScrollOffset;
        //        }
        //        ctr_sv.ScrollToHorizontalOffset(targetOffsetX);
        //        MpConsole.WriteLine($"Tray auto-scroll: {targetOffsetX}");
        //    } else {
        //        if(AssociatedObject.DataContext is MpClipTileViewModel) {
        //            var lb = AssociatedObject as ListBox;
        //            var content_rect = lb.GetListBoxRect();
        //            double content_midY = content_rect.Top + (content_rect.Height / 2);
        //            var content_sv = lb.GetScrollViewer();
        //            var content_mp = Mouse.GetPosition(AssociatedObject);

        //            double topDiff = content_mp.Y - content_rect.Top;
        //            double bottomDiff = content_rect.Bottom - content_mp.Y;
        //            double targetOffsetY = 0;
        //            if (topDiff < minScrollDist) {
        //                double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(content_midY - content_mp.Y, exp));
        //                targetOffsetY = content_sv.VerticalOffset - autoScrollOffset;
        //            } else if (bottomDiff < minScrollDist) {
        //                double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(content_mp.Y - content_midY, exp));
        //                targetOffsetY = content_sv.VerticalOffset + maxAutoScrollOffset;
        //            }
        //            content_sv.ScrollToVerticalOffset(targetOffsetY);
        //        } else {
        //            AssociatedObject.GetVisualAncestor<MpContentListView>().DropBehavior.AutoScrollByMouse();
        //            var rtbv = AssociatedObject.GetVisualAncestor<MpRtbView>();
        //            var lbi = AssociatedObject.GetVisualAncestor<ListBoxItem>();
        //            var lbir = lbi.GetRect();
        //            var item_mp = Mouse.GetPosition(lbi);
        //            if(item_mp.Y < lbir.Height / 2) {
        //                rtbv.ScrollToHome();
        //            } else {
        //                rtbv.ScrollToEnd();
        //            }
        //        }
        //    }

        //}
    }

}
