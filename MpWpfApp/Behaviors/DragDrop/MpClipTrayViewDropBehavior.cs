using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTrayViewDropBehavior : Behavior<MpClipTrayView> {
        private MpLineAdorner lineAdorner;

        private List<object> dragObjList;
        private int dropIdx = -1;
        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            lineAdorner = new MpLineAdorner(AssociatedObject.ClipTray);
            AssociatedObject.ClipTrayAdornerLayer.Add(lineAdorner);
            AssociatedObject.ClipTrayAdornerLayer.Update();
        }

        public void DragOver(List<object> vml, int overIdx) {
            var ctrvm = AssociatedObject.DataContext as MpClipTrayViewModel;
            if(vml.All(x=>x is MpClipTileViewModel) && 
                MpTagTrayViewModel.Instance.SelectedTagTile.IsSudoTag) {
                // don't allow reordering of tiles in recent or all tags
                // only allow composite items to be dragged to create new tiles
                Reset();
                return;
            }
            if (dropIdx == overIdx) {
                return;
            }

            dragObjList = vml;
            dropIdx = overIdx;

            lineAdorner.IsShowing = true;
            lineAdorner.Points = AssociatedObject.ClipTray.GetAdornerPoints(overIdx, true);
            AssociatedObject.ClipTrayAdornerLayer.Update();
        }

        public void Drop() {
            if(dragObjList == null || dragObjList.Count == 0) {
                //nothing to drop so ignore
            } else {
                //reverse drag list so first item is at insert idx
                dragObjList.Reverse();
                var ctrvm = AssociatedObject.DataContext as MpClipTrayViewModel;

                var cid = new Dictionary<int,List<MpCopyItem>>();
                foreach(var obj in dragObjList) {
                    if(obj is MpClipTileViewModel ctvm) {
                        //for clip tile just move them
                        var cit = MpCopyItemTag.GetCopyItemTagByCopyItemId(
                            MpTagTrayViewModel.Instance.SelectedTagTile.Tag.Id,
                            ctvm.HeadItem.CopyItem.Id);
                        cit.CopyItemSortIdx = dropIdx;
                        cit.WriteToDatabase();
                    } else if(obj is MpContentItemViewModel civm) {
                        int cciid = civm.CopyItem.CompositeParentCopyItemId;
                        //for composite items lump them into kvp list if from same composite item
                        if (!cid.ContainsKey(cciid)) {
                            cid.Add(cciid, new List<MpCopyItem> { civm.CopyItem });
                        } else {
                            cid[cciid].Add(civm.CopyItem);
                        }
                    }
                }

                //loop through composite item lumps and make new composite items
                foreach(var kvp in cid) {
                    for (int i = 0; i < kvp.Value.Count; i++) {
                        MpCopyItem ci = kvp.Value[i];
                        if(i == 0) {
                            ci.CompositeParentCopyItemId = 0;
                        } else {
                            ci.CompositeParentCopyItemId = kvp.Value[0].Id;
                        }
                        ci.CompositeSortOrderIdx = i;
                        ci.WriteToDatabase();
                    }
                }

                ctrvm.RefreshClips();
            }
            Reset();
        }

        public void CancelDrop() {
            Reset();
        }

        private void Reset() {
            dragObjList = null;
            lineAdorner.IsShowing = false;
            AssociatedObject.ClipTrayAdornerLayer.Update();
        }
    }
}
