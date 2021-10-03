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
        private bool isHorizontal;
        private MpDropLineAdorner lineAdorner;
        private AdornerLayer adornerLayer;
        public int dropIdx = -1;

        private List<MpCopyItem> dragItemList;
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
                isHorizontal = true;
                containerType = MpCopyItemType.None;
                mwvm = (AssociatedObject.DataContext as MpClipTrayViewModel).MainWindowViewModel;
            } else if (AssociatedObject.DataContext is MpClipTileViewModel ctvm) {
                isHorizontal = false;
                containerType = ctvm.ItemType;
                mwvm = (AssociatedObject.DataContext as MpClipTileViewModel).MainWindowViewModel;
            }

            mwvm.OnMainWindowHide += MainWindowViewModel_OnMainWindowHide;
        }

        private void MainWindowViewModel_OnMainWindowHide(object sender, EventArgs e) {
            Reset();
        }

        public bool StartDrop(List<MpCopyItem> dcil, int overIdx) {
            if (!IsDragDataValid(dcil, overIdx)) {
                CancelDrop();
                return false;
            }

            if (dropIdx == overIdx) {
                return dropIdx <= 0;
            }

            dragItemList = dcil;
            dropIdx = overIdx;

            Rect overRect;
            bool isTail = false;
            if (overIdx < AssociatedObject.Items.Count) {
                overRect = AssociatedObject.GetListBoxItemRect(overIdx);
            } else {
                overRect = AssociatedObject.GetListBoxItemRect(AssociatedObject.Items.Count - 1);
                isTail = true;
            }
            if (isHorizontal) {
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
            return true;
        }

        public void CancelDrop() {
            Reset();
        }

        public void Drop(bool isCopy = false) {
            if (dragItemList == null || dragItemList.Count == 0) {
                Reset();
                return;
            }
            int tileCount = MpClipTrayViewModel.Instance.ClipTileViewModels.Count;
            MpClipTileViewModel dropTile = null;
            var dragTiles = new List<MpClipTileViewModel>();
            foreach (var di in dragItemList) {
                var dcivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(di.Id);
                if (dcivm != null && dcivm.Parent != null && !dragTiles.Contains(dcivm.Parent) && dcivm.Parent != dropTile) {
                    dragTiles.Add(dcivm.Parent);
                }
            }

            if (isHorizontal) {
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

                }
                //dropIdx = dropIdx < 0 ? 0 : dropIdx >= tileCount ? tileCount - 1 : dropIdx;
                //dropTile = MpClipTrayViewModel.Instance.ClipTileViewModels[tileCount - 1];
                MpClipTileSortViewModel.Instance.SetToManualSort();
            } else {
                bool isContentResort = dragTiles.Count == 1;
                if (isContentResort) {
                    //For items moved within a tile reverse order and move
                    var dragCivml = dragTiles[0].SelectedItems;
                    dragCivml.Reverse();
                    foreach (var dragCivm in dragCivml) {
                        int oldIdx = dragTiles[0].ItemViewModels.IndexOf(dragCivm);
                        if(oldIdx < dropIdx) {
                            dropIdx--;
                        }
                        dragTiles[0].ItemViewModels.Move(oldIdx, dropIdx);
                    }

                }
                dropTile = AssociatedObject.DataContext as MpClipTileViewModel;
            }
            Reset();
            return;
            MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                foreach (var ctvm in dragTiles) {
                    await ctvm.RemoveRange(dragItemList);
                }
                await dropTile.InsertRange(dropIdx, dragItemList);


                if (isHorizontal) {

                    await MpClipTrayViewModel.Instance.RefreshTiles();
                    MpClipTrayViewModel.Instance.ClipTileViewModels.Move(tileCount - 1, dropIdx);
                }

                //dropTile.IsSelected = true;
                Reset();
            });
        }

        private bool IsDragDataValid(List<MpCopyItem> dcil, int overIdx) {
            if (dcil == null || dcil.Count == 0) {
                return false;
            }
            if (containerType == MpCopyItemType.None) {
                //just ensure they are all the same content type
                return dcil.All(x => x.ItemType == dcil[0].ItemType);

                if (!MpTagTrayViewModel.Instance.SelectedTagTile.IsSudoTag) {
                    return true;
                }
                //only allow tray dropping onto sudo tag's if items are partial selections of their container
                foreach (var dci in dcil) {
                    if (dci.CompositeParentCopyItemId == 0) {
                        var ccil = MpCopyItem.GetCompositeChildren(dci);
                        if (ccil.Count == 0) {
                            return false;
                        }
                        foreach (var dcici in ccil) {
                            if (!dcil.Contains(dcici)) {
                                continue;
                            }
                            return false;
                        }
                    } else {
                        var dcipci = dci.GetCompositeParent();
                        var dcipcil = MpCopyItem.GetCompositeChildren(dcipci);
                        dcipcil.Add(dcipci);

                        foreach (var dcici in dcipcil) {
                            if (!dcil.Contains(dcici)) {
                                continue;
                            }
                            return false;
                        }
                    }

                }
            } else if (dcil.Any(x => x.ItemType != containerType)) {
                return false;
            }
            return true;
        }


        private void Reset() {
            dropIdx = -1;
            dragItemList = null;
            adornerLayer.Update();
            lineAdorner.IsShowing = false;
            adornerLayer.Update();
        }
    }

}
