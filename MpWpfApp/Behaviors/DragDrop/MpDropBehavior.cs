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
            lineAdorner = new MpDropLineAdorner(AssociatedObject);
            this.adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            adornerLayer.Add(lineAdorner);

            adornerLayer.Update();

            MpMainWindowViewModel mwvm = null;
            if (AssociatedObject.DataContext is MpClipTrayViewModel) {
                containerType = MpCopyItemType.None;
                mwvm = (AssociatedObject.DataContext as MpClipTrayViewModel).MainWindowViewModel;
            } else if (AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
                containerType = ctvm.ItemType;
                mwvm = (AssociatedObject.DataContext as MpClipTileViewModel).MainWindowViewModel;
            }

            mwvm.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            Reset();
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

            AutoScrollByMouse();
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
            if (!isTrayDrop) {
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
                var dragModels = MpClipTrayViewModel.Instance.SelectedModels;
                int tileCount = MpClipTrayViewModel.Instance.ClipTileViewModels.Count;
                if (isTrayDrop) {
                    bool isTileResort = dragTiles.All(x => x.SelectedItems.Count == x.ItemViewModels.Count);
                    if (isTileResort) {
                        //For full tile's moved on tray reverse order and use standard move
                        dragTiles.Reverse();
                        foreach (var dragTile in dragTiles) {
                            int oldIdx = MpClipTrayViewModel.Instance.ClipTileViewModels.IndexOf(dragTile);
                            if (oldIdx < dropIdx) {
                                dropIdx--;
                            }
                            MpClipTrayViewModel.Instance.ClipTileViewModels.Move(oldIdx, dropIdx);
                        }
                    } else {
                        //partial tile drop onto tray, create new tile, remove from source
                        //update drag models, remove empty tiles updating dropIdx then init new tile
                        var dropTile = MpClipTrayViewModel.Instance.CreateClipTileViewModel(null);
                        foreach (var dragTile in dragTiles) {
                            foreach (var dci in dragModels) {
                                var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
                                if (dcivm != null) {
                                    dragTile.ItemViewModels.Remove(dcivm);
                                }
                            }
                        }
                        dragModels.Reverse();
                        for (int i = 0; i < dragModels.Count; i++) {
                            dragModels[i].CompositeSortOrderIdx = i;
                            if (i == 0) {
                                dragModels[i].CompositeParentCopyItemId = 0;
                            } else {
                                dragModels[i].CompositeParentCopyItemId = dragModels[0].Id;
                            }
                            dragModels[i].WriteToDatabase();
                        }
                        foreach (var dragTile in dragTiles) {
                            if (dragTile.Count == 0) {
                                int dragIdxToRemove = MpClipTrayViewModel.Instance.ClipTileViewModels.IndexOf(dragTile);
                                if(dragIdxToRemove < dropIdx) {
                                    dropIdx--;
                                }
                                MpClipTrayViewModel.Instance.ClipTileViewModels.Remove(dragTile);
                            } else {
                                dragTile.UpdateSortOrder();
                            }
                        }

                        await MpHelpers.Instance.RunOnMainThreadAsync(
                                    () => MpClipTrayViewModel.Instance.ClipTileViewModels.Insert(dropIdx, dropTile));
                        await dropTile.Initialize(dragModels[0]);

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
                            }
                            dragTiles[0].ItemViewModels.Move(oldIdx, dropIdx);
                        }
                    } else {
                        foreach (var dragTile in dragTiles) {
                            foreach (var dci in dragModels) {
                                var dcivm = dragTile.GetContentItemByCopyItemId(dci.Id);
                                if (dcivm != null) {
                                    if (dragTile == dropTile) {
                                        int dcivmIdx = dragTile.ItemViewModels.IndexOf(dcivm);
                                        if (dcivmIdx < dropIdx) {
                                            dropIdx--;
                                        }
                                    }
                                    dragTile.ItemViewModels.Remove(dcivm);
                                }
                            }
                        }
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
                            dropModels[i].WriteToDatabase();
                        }
                        foreach (var dragTile in dragTiles) {
                            if (dragTile == dropTile) {
                                continue;
                            }
                            if (dragTile.Count == 0) {
                                MpClipTrayViewModel.Instance.ClipTileViewModels.Remove(dragTile);
                            } else {
                                dragTile.UpdateSortOrder();
                            }
                        }
                        await dropTile.Initialize(dropModels[0]);

                        var cilv = AssociatedObject.GetVisualAncestor<MpContentListView>();
                        cilv.UpdateAdorner();
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
                var clv = AssociatedObject.GetVisualAncestor<MpContentListView>();
                clv.SeperatorAdornerLayer.Update();
            }
            AssociatedObject.GetScrollViewer().ScrollToHome();
        }

        public void AutoScrollByMouse() {
            //during drop autoscroll listbox to beginning or end of list
            //if more items are there depending on which half of the visible list
            //the mouse is in
            var sv = AssociatedObject.GetScrollViewer();
            
            var mp = MpHelpers.Instance.GetMousePosition(AssociatedObject);
            Rect listBoxRect = AssociatedObject.GetListBoxRect();
            double minScrollDist = 20;
            double autoScrollOffset = 15;
            
            if(isTrayDrop) {
                MpConsole.WriteLine($"Tray AutoScroll sv width: {sv.ScrollableWidth} lb width: {AssociatedObject.Width}");
                if(sv.ScrollableWidth <= AssociatedObject.Width) {

                }
                double leftDiff = MpHelpers.Instance.DistanceBetweenValues(mp.X, 0);
                double rightDiff = MpHelpers.Instance.DistanceBetweenValues(mp.X, listBoxRect.Right);
                if (leftDiff < minScrollDist) {
                    autoScrollOffset += Math.Pow(leftDiff, 2);
                    sv.ScrollToHorizontalOffset(sv.HorizontalOffset - autoScrollOffset);
                } else if (rightDiff < minScrollDist) {
                    autoScrollOffset += Math.Pow(rightDiff, 2);
                    sv.ScrollToHorizontalOffset(sv.HorizontalOffset + autoScrollOffset);
                }
            } else {
                MpConsole.WriteLine($"Content AutoScroll sv height: {sv.ScrollableHeight} lb height: {AssociatedObject.Height}");
                if (sv.ScrollableWidth <= AssociatedObject.Width) {

                }
                double topDiff = MpHelpers.Instance.DistanceBetweenValues(mp.Y, 0);
                double bottomDiff = MpHelpers.Instance.DistanceBetweenValues(mp.Y, listBoxRect.Bottom);
                if (topDiff < minScrollDist) {
                    autoScrollOffset += Math.Pow(topDiff, 2);
                    sv.ScrollToVerticalOffset(sv.VerticalOffset - autoScrollOffset);
                } else if (bottomDiff < minScrollDist) {
                    autoScrollOffset += Math.Pow(bottomDiff, 2);
                    sv.ScrollToVerticalOffset(sv.VerticalOffset + autoScrollOffset);
                }
            }
            
        }
    }

}
