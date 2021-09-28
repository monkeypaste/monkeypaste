using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTrayViewDropBehavior : Behavior<MpClipTrayView> {
        private MpLineAdorner lineAdorner;

        private List<MpClipTileViewModel> dragObjList;
        private int dropIdx = -1;

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            lineAdorner = new MpLineAdorner(AssociatedObject.ClipTray);
            AssociatedObject.ClipTrayAdornerLayer.Add(lineAdorner);
            AssociatedObject.ClipTrayAdornerLayer.Update();

            var logTimer = new Timer();
            logTimer.Interval = 300;
            logTimer.Elapsed += (s, e1) => { MpMainWindowViewModel.SetLogText($"Tray: {dropIdx}"); };
            logTimer.Start();
        }

        public void DragOver(List<MpClipTileViewModel> vml, int overIdx) {
            if(!IsDragDataValid(vml,overIdx)) {
                // don't allow reordering of tiles in recent or all tags
                // only allow composite items to be dragged to create new tiles
                CancelDrop();
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

                var cil = new List<MpCopyItem>();
                foreach(var dctvm in dragObjList) {
                    dctvm.DoCommandSelection();
                    cil.AddRange(dctvm.SelectedItems.Select(x => x.CopyItem));
                }

                //loop through composite item lumps and make new composite items
                for (int i = 0; i < cil.Count; i++) {
                    MpCopyItem ci = cil[i];
                    if (i == 0) {
                        ci.CompositeParentCopyItemId = 0;
                    } else {
                        ci.CompositeParentCopyItemId = cil[0].Id;
                    }
                    ci.CompositeSortOrderIdx = i;
                    ci.WriteToDatabase();
                }

                ctrvm.RefreshClips();
            }
            Reset();
        }

        public void CancelDrop() {
            Reset();
        }

        public bool IsDragDataValid(List<MpClipTileViewModel> vml,int nDropIdx) {
            return true;
            if(vml == null || vml.Count == 0) {
                return false;
            }
            if(vml.All(x => x is MpClipTileViewModel)) {
                //if ALL drag objects are tiles
                if (MpTagTrayViewModel.Instance.SelectedTagTile.IsSudoTag) {
                    // don't allow resorting of sudo tags
                    return false;
                }
            }
            return !IsDragDataDropTarget(vml,nDropIdx);
        }

        public bool IsDragDataDropTarget(List<MpClipTileViewModel> vml, int nDropIdx) {            
            if (vml == null || vml.Count == 0 || nDropIdx < 0) {
                return false;
            }

            var ctrvm = AssociatedObject.DataContext as MpClipTrayViewModel;
            var odctvml = new List<MpClipTileViewModel>();
            foreach (var dctvm in vml) {
                dctvm.DoCommandSelection();
                if (dctvm.ItemViewModels.Count == dctvm.SelectedItems.Count) {
                    odctvml.Add(dctvm);
                } else {
                    return false;
                }
            }
            //getting here means only tiles are being resorted
            odctvml = odctvml.Distinct().OrderBy(x => ctrvm.ClipTileViewModels.IndexOf(x)).ToList();
            int testIdx = nDropIdx;
            if (testIdx > 0) {
                testIdx = testIdx - 1;
            }
            //the only way this is a valid drop is when all selected items aren't affected
            //by drop idx
            for (int i = testIdx; i < odctvml.Count; i++) {
                int odctvmIdx = ctrvm.ClipTileViewModels.IndexOf(odctvml[i]);
                if (i != odctvmIdx) {
                    return false;
                }
            }
            return true;
        }

        private void Reset() {
            dropIdx = -1;
            dragObjList = null;
            lineAdorner.IsShowing = false;
            AssociatedObject.ClipTrayAdornerLayer.Update();
        }
    }
}
