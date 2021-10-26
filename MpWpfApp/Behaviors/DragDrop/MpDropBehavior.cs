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

namespace MpWpfApp {
    public class MpDropBehavior : Behavior<ListBox> {
        private bool isTrayDrop {
            get {
                return containerType == MpCopyItemType.None;
            }
        }
        private MpDropLineAdorner lineAdorner;
        private AdornerLayer adornerLayer;
        public int dropIdx = -1;

        private List<MpClipTileViewModel> dragTiles;
        private MpCopyItemType containerType;

        public MpDropBehavior() { }

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            InitAdorner();
            if (AssociatedObject.DataContext != null) {

                MpMainWindowViewModel mwvm = null;
                if (AssociatedObject.DataContext is MpClipTrayViewModel) {
                    containerType = MpCopyItemType.None;
                    mwvm = MpMainWindowViewModel.Instance;
                } else if (AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
                    containerType = ctvm.ItemType;
                    mwvm = MpMainWindowViewModel.Instance;
                }

                mwvm.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
            }
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            Reset();
        }

        private void InitAdorner() {
            lineAdorner = new MpDropLineAdorner(AssociatedObject);
            this.adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            adornerLayer.Add(lineAdorner);

            adornerLayer.Update();
        }

        public bool StartDrop(List<MpClipTileViewModel> dctvml, int overIdx) {            
            if (!IsDragDataValid(dctvml, overIdx)) {
                CancelDrop();
                return false;
            }

            if (dropIdx == overIdx) {
                return dropIdx <= 0;
            }

            dragTiles = dctvml;
            dropIdx = overIdx;
            if(!isTrayDrop) {
                (AssociatedObject.DataContext as MpClipTileViewModel).DropIdx = dropIdx;
            }

            UpdateDropLineAdorner();
            
            return true;
        }

        private bool IsDragDataValid(List<MpClipTileViewModel> dcil, int overIdx) {
            if (dcil == null || dcil.Count == 0) {
                return false;
            }
            //just ensure they are all the same content type
            bool areDragItemsSameType = dcil.All(x => x.ItemType == dcil[0].ItemType);
            if (!areDragItemsSameType) {
                return false;
            }
            if(!isTrayDrop) {
                bool areDragItemsSameTypeAsDropContainer = dcil.All(x => x.ItemType == containerType);
                if (!areDragItemsSameTypeAsDropContainer) {
                    return false;
                }
            }
            return true;
        }

        private void UpdateDropLineAdorner() {
            Rect overRect;
            bool isTail = false;
            if (dropIdx < AssociatedObject.Items.Count) {
                overRect = AssociatedObject.GetListBoxItemRect(dropIdx);
            } else {
                overRect = AssociatedObject.GetListBoxItemRect(AssociatedObject.Items.Count - 1);
                isTail = true;
            }

            if (isTrayDrop) {
                if (isTail) {
                    lineAdorner.Points[0] = overRect.TopRight;
                    lineAdorner.Points[1] = overRect.BottomRight;
                } else {
                    lineAdorner.Points[0] = overRect.TopLeft;
                    lineAdorner.Points[1] = overRect.BottomLeft;
                }
            } else {
                if (isTail) {
                    lineAdorner.Points[0] = overRect.BottomLeft;
                    lineAdorner.Points[1] = overRect.BottomRight;
                } else {
                    lineAdorner.Points[0] = overRect.TopLeft;
                    lineAdorner.Points[1] = overRect.TopRight;
                }
            }

            lineAdorner.IsShowing = true;
            adornerLayer.Update();
        }

        public void CancelDrop() {
            Reset();
        }

        public void Drop(bool isCopy = false) {
            MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                if (this.dragTiles == null || this.dragTiles.Count == 0) {
                    Reset();
                    return;
                }

                MpConsole.WriteLine("DropIdx: " + dropIdx);
                var dragModels = MpClipTrayViewModel.Instance.SelectedModels;
                int tileCount = MpClipTrayViewModel.Instance.Items.Count;
                if (isTrayDrop) {
                    if(dropIdx > MpClipTrayViewModel.Instance.VisibleItems.Count) {
                        dropIdx = MpClipTrayViewModel.Instance.VisibleItems.Count;

                        MpConsole.WriteLine("Dropping at visible tail, reset tray dropIdx: " + dropIdx);
                    }
                    bool isTileResort = dragTiles.All(x => x.SelectedItems.Count == x.ItemViewModels.Count);
                    if (isTileResort) {
                        //For full tile's moved on tray reverse order and use standard move
                        dragTiles.Reverse();
                        foreach (var dragTile in dragTiles) {
                            int oldIdx = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
                            if (oldIdx < dropIdx) {
                                dropIdx--;
                                MpConsole.WriteLine("Decrementing tray dropIdx: " + dropIdx);
                            }
                            MpClipTrayViewModel.Instance.Items.Move(oldIdx, dropIdx);
                        }
                    } else {
                        //some collection of items dropped onto tray
                        //recycle tail item to use for dropped content
                        var dropTile = MpClipTrayViewModel.Instance.TailItem; //MpClipTrayViewModel.Instance.CreateClipTileViewModel(null);
                        if (dragTiles.Any(x => x == dropTile)) {
                            // special case where items are dragged from last item
                            // create a new tile at end of list

                            dropTile = await MpClipTrayViewModel.Instance.CreateClipTileViewModel(null);
                            MpClipTrayViewModel.Instance.Items.Add(dropTile);
                        }
                        foreach (var dragTile in dragTiles) {
                            //remove drag content from their tiles
                            foreach (var dci in dragModels) {
                                var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
                                if (dcivm != null) {
                                    dragTile.ItemViewModels.Remove(dcivm);
                                }
                            }
                        }
                        //update drag items into new composite 
                        //selected models is ordered by select time ascending
                        //dragModels.Reverse();
                        for (int i = 0; i < dragModels.Count; i++) {
                            dragModels[i].CompositeSortOrderIdx = i;
                            if (i == 0) {
                                dragModels[i].CompositeParentCopyItemId = 0;
                            } else {
                                dragModels[i].CompositeParentCopyItemId = dragModels[0].Id;
                            }
                            await dragModels[i].WriteToDatabaseAsync();
                        }
                        foreach (var dragTile in dragTiles) {
                            //clean up tiles removed items and recycle if empty
                            if (dragTile.Count == 0) {
                                int dragIdxToRemove = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
                                if(dragIdxToRemove < dropIdx) {
                                    dropIdx--;
                                    MpConsole.WriteLine("Decrementing tray dropIdx: " + dropIdx);
                                }
                                //await dragTile.InitializeAsync(null);
                                //MpClipTrayViewModel.Instance.Items.Move(dragIdxToRemove, MpClipTrayViewModel.Instance.Items.Count - 1);
                                MpClipTrayViewModel.Instance.Items.RemoveAt(dragIdxToRemove);
                            } else {
                                await dragTile.UpdateSortOrderAsync();
                                await dragTile.InitializeAsync(dragTile.HeadItem.CopyItem);
                            }
                        }
                        await dropTile.InitializeAsync(dragModels[0]);
                        
                        int oldDropIdx = MpClipTrayViewModel.Instance.Items.IndexOf(dropTile);
                        MpClipTrayViewModel.Instance.Items.Move(oldDropIdx, dropIdx);

                        dropTile.RequestUiUpdate();
                    }
                    MpClipTileSortViewModel.Instance.SetToManualSort();
                } else {
                    MpClipTileViewModel dropTile = AssociatedObject.DataContext as MpClipTileViewModel;
                    bool isContentResort = dragTiles.Count == 1 && dragTiles[0] == dropTile;
                    if (isContentResort) {
                        //For items moved within a tile reverse order and move
                        var dragCivml = dragTiles[0].SelectedItems;
                        dragCivml.Reverse();
                        foreach (var dragCivm in dragCivml) {
                            int oldIdx = dragTiles[0].ItemViewModels.IndexOf(dragCivm);
                            if (oldIdx < dropIdx) {
                                dropIdx--;
                                MpConsole.WriteLine("Decrementing tile dropIdx: " + dropIdx);
                            }
                            dragTiles[0].ItemViewModels.Move(oldIdx, dropIdx);
                        }
                    } else {
                        foreach (var dragTile in dragTiles) {
                            //remove all items from their drag tiles
                            foreach (var dci in dragModels) {
                                //check if drag item is in this drag tile
                                var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
                                if (dcivm != null) {
                                    //drag item is in drag tile
                                    if (dragTile == dropTile) {
                                        // drag item is IN the drop tile
                                        int dcivmIdx = dragTile.ItemViewModels.IndexOf(dcivm);
                                        if (dcivmIdx < dropIdx) {
                                            //drag item idx is lower than drop idx so adjust
                                            dropIdx--;
                                            MpConsole.WriteLine("Decrementing tile dropIdx: " + dropIdx);
                                        }
                                    }
                                    dragTile.ItemViewModels.Remove(dcivm);
                                }
                            }
                        }
                        //now all drag items are removed from their tiles and dropIdx should be correct
                        //range insert drag items into drop and reorder based on drop idx
                        dragModels.Reverse();
                        var dropModels = dropTile.ItemViewModels.Select(x => x.CopyItem).ToList();
                        dropModels.InsertRange(dropIdx, dragModels);
                        for (int i = 0; i < dropModels.Count; i++) {
                            dropModels[i].CompositeSortOrderIdx = i;
                            if (i == 0) {
                                dropModels[i].CompositeParentCopyItemId = 0;
                            } else {
                                dropModels[i].CompositeParentCopyItemId = dropModels[0].Id;
                            }
                            await dropModels[i].WriteToDatabaseAsync();
                        }
                        //clean up all tiles with content dragged
                        //if tile has no items recycle it
                        //if it still has items sync sort order from its visuals
                        foreach (var dragTile in dragTiles) {
                            if (dragTile == dropTile) {
                                continue;
                            }
                            if (dragTile.Count == 0) {
                                //recycle empty tile
                                int dragIdxToRemove = MpClipTrayViewModel.Instance.Items.IndexOf(dragTile);
                                MpClipTrayViewModel.Instance.Items.RemoveAt(dragIdxToRemove);
                            } else {
                                await dragTile.UpdateSortOrderAsync();
                                await dragTile.InitializeAsync(dragTile.HeadItem.CopyItem);
                            }
                        }
                        await dropTile.InitializeAsync(dropModels[0]);
                    }
                }
                Reset();
            });
        }

        private void Reset() {
            dropIdx = -1;
            dragTiles = null;
            lineAdorner.IsShowing = false;
            adornerLayer.Update();
            if(!isTrayDrop) {
                (AssociatedObject.DataContext as MpClipTileViewModel).DropIdx = -1;
            }
            //AssociatedObject.GetScrollViewer()?.ScrollToHome();
        }

        public void AutoScrollByMouse() {
            //return;
            //during drop autoscroll listbox to beginning or end of list
            //if more items are there depending on which half of the visible list
            //the mouse is in
            ListBox ctr_lb;
            if(isTrayDrop) {
                ctr_lb = AssociatedObject;
            } else {
                ctr_lb = AssociatedObject.GetVisualAncestor<MpClipTrayView>().ClipTray;
            }
            var ctr_sv = ctr_lb.GetScrollViewer() as AnimatedScrollViewer;
            
            var ctr_mp = Mouse.GetPosition(ctr_lb);
            Rect ctr_rect = ctr_lb.GetListBoxRect();
            double ctr_midX = ctr_rect.Left + (ctr_rect.Width / 2);
            double minScrollDist = 30;
            double maxAutoScrollOffset = 50;
            double exp = 1.5;

            //
            double leftDiff = ctr_mp.X - ctr_rect.Left;
            double rightDiff = ctr_rect.Right - ctr_mp.X;
            if (leftDiff < minScrollDist) {
                double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(ctr_midX - ctr_mp.X, exp));
                ctr_sv.TargetHorizontalOffset = ctr_sv.HorizontalOffset - autoScrollOffset;
            } else if (rightDiff < minScrollDist) {
                double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(ctr_mp.X-ctr_midX, exp));
                ctr_sv.TargetHorizontalOffset = ctr_sv.HorizontalOffset + autoScrollOffset;
            }

            if (!isTrayDrop) {
                var content_rect = AssociatedObject.GetListBoxRect();
                double content_midY = content_rect.Top + (content_rect.Height / 2);
                var content_sv = AssociatedObject.GetScrollViewer() as AnimatedScrollViewer;
                var content_mp = Mouse.GetPosition(AssociatedObject);

                double topDiff = content_mp.Y - content_rect.Top;
                double bottomDiff = content_rect.Bottom - content_mp.Y;
                if (topDiff < minScrollDist) {
                    double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(content_midY - content_mp.Y, exp));
                    content_sv.TargetVerticalOffset = content_sv.VerticalOffset - autoScrollOffset;
                } else if (bottomDiff < minScrollDist) {
                    double autoScrollOffset = Math.Min(maxAutoScrollOffset, Math.Pow(content_mp.Y-content_midY, exp));
                    content_sv.TargetVerticalOffset = content_sv.VerticalOffset + maxAutoScrollOffset;
                }
            }
            
        }
    }

}
