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
            //drop is move by default
            if (dragItemList == null || dragItemList.Count == 0) {
                Reset();
                return;
            }
            var affectedClipTiles = new List<MpClipTileViewModel>();
            foreach (var dci in dragItemList) {
                //add all tiles involed in the drag drop
                var dctvm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(dci.Id);
                if (dctvm != null && !affectedClipTiles.Contains(dctvm.Parent)) {
                    affectedClipTiles.Add(dctvm.Parent);
                }
            }
            // TODO check for Ctrl down if so clone dragItemList
            int dropParentId = dragItemList[0].Id;
            var cil = new List<MpCopyItem>();
            if (isHorizontal) {
                cil = dragItemList;
            } else {
                var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
                //ensure drop tile is not in affected list
                if (affectedClipTiles.Contains(ctvm)) {
                    affectedClipTiles.Remove(ctvm);
                }

                if (dropIdx > 0) {
                    dropParentId = ctvm.HeadItem.CopyItem.Id;
                }
                cil = ctvm.ItemViewModels.Select(x => x.CopyItem).ToList();
                cil = cil.OrderBy(x => x.CompositeSortOrderIdx).ToList();
                dragItemList.Reverse();
                foreach (var dci in dragItemList) {
                    if (cil.Any(x => x.Id == dci.Id)) {
                        //drag item is part of drop list
                        cil.RemoveAt(cil.IndexOf(dci));
                        dropIdx--;
                    }
                    cil.Insert(dropIdx, dci);
                }
            }
            var curTagVm = MpTagTrayViewModel.Instance.SelectedTagTile;
            
            //re-root all affected tiles
            for (int i = 0; i < affectedClipTiles.Count; i++) {
                var ndatcil = affectedClipTiles[i].ItemViewModels.Select(x => x.CopyItem).ToList();
                foreach (var di in dragItemList) {
                    //for non drop affected tiles remove any dropped items
                    if (ndatcil.Contains(di)) {
                        ndatcil.Remove(di);
                    }
                }
                if (ndatcil.Count == 0) {
                    continue;
                }
                //make the lowest sort item the root
                ndatcil = ndatcil.OrderBy(x => x.CompositeSortOrderIdx).ToList();

                //same loop as for drop tile but from sub non-drop list
                for (int j = 0; j < ndatcil.Count; j++) {
                    MpCopyItem ci = ndatcil[j];
                    if (j == 0) {
                        ci.CompositeParentCopyItemId = 0;
                    } else {
                        ci.CompositeParentCopyItemId = ndatcil[0].Id;
                    }
                    ci.CompositeSortOrderIdx = j;
                    ci.WriteToDatabase();
                }
            }
            //loop through drop items and re-root/sort
            for (int i = 0; i < cil.Count; i++) {
                MpCopyItem ci = cil[i];
                if (ci.Id == dropParentId) {
                    ci.CompositeParentCopyItemId = 0;
                } else {
                    ci.CompositeParentCopyItemId = cil[0].Id;
                }
                ci.CompositeSortOrderIdx = i;
                ci.WriteToDatabase();
            }
                        

            //clear selection ensure its not reset after refresh 
            MpClipTrayViewModel.Instance.ClearClipSelection();
            MpClipTrayViewModel.Instance.IgnoreSelectionReset = true;

            //recreate tiles with new structure
            MpClipTrayViewModel.Instance.RefreshClips();

            //find new or moved dropped tile
            MpClipTileViewModel dropTile = MpClipTrayViewModel.Instance.GetContentItemViewModelById(dropParentId).Parent;
            int dropTileIdx = MpClipTrayViewModel.Instance.ClipTileViewModels.IndexOf(dropTile);

            if(curTagVm.IsSudoTag) {
                MpClipTileSortViewModel.Instance.SetToManualSort();
            }
            MpClipTrayViewModel.Instance.ClipTileViewModels.Move(dropTileIdx, dropIdx);
            if(!curTagVm.IsSudoTag) {
                //resersdf
                MpClipTrayViewModel.Instance.UpdateSortOrder();
            }
            //do reverse to preserve selection order
            dragItemList.Reverse();
            foreach (var di in dragItemList) {
                var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(di.Id);
                if (civm != null) {
                    civm.Parent.IsSelected = true;
                    civm.IsSelected = true;
                    if (dropTile == null) {
                        //reference moved or new tile
                        dropTile = civm.Parent;
                    }
                }
            }

            //for sudo tag drops refreshclips will change the sort order
            //so move the drop tile back where user wanted it
            MpClipTrayViewModel.Instance.IgnoreSelectionReset = false;




            Reset();
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
            lineAdorner.IsShowing = false;
            adornerLayer.Update();
        }
    }

}
