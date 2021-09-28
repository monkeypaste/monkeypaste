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
    public class MpContentListDropBehavior : Behavior<MpContentListView> {
        private MpLineAdorner lineAdorner;

        private List<MpClipTileViewModel> dragObjList;
        private int dropIdx = -1;

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            lineAdorner = new MpLineAdorner(AssociatedObject.ContentListAdornerLayer);
            AssociatedObject.ContentListAdornerLayer.Add(lineAdorner);
            AssociatedObject.ContentListAdornerLayer.Update();

            var civm = AssociatedObject.DataContext as MpContentItemViewModel;
            var logTimer = new Timer();
            logTimer.Interval = 300;
            logTimer.Elapsed += (s, e1) => {
                if (dropIdx >= 0 && civm != null) {
                    MpMainWindowViewModel.SetLogText($" Tile{civm.Parent.HeadItem.CopyItem.Id}: {dropIdx}", true);
                }
            };
            logTimer.Start();
        }

        public void DragOver(List<MpClipTileViewModel> vml, int overIdx) {
            if(!IsDragDataValid(vml,overIdx)) {
                CancelDrop();
                return;
            }
            if (dropIdx == overIdx) {
                return;
            }

            dragObjList = vml;
            dropIdx = overIdx;

            lineAdorner.IsShowing = true;
            lineAdorner.Points = AssociatedObject.ContentListBox.GetAdornerPoints(overIdx, false);
            AssociatedObject.ContentListAdornerLayer.Update();
        }

        public void Drop() {
            if(dragObjList == null || dragObjList.Count == 0) {
                //nothing to drop so ignore
            } else {
                //reverse drag list so first item is at insert idx
                dragObjList.Reverse();
                var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;

                var cil = new List<MpCopyItem>();
                cil.AddRange(ctvm.ItemViewModels.Select(x => x.CopyItem).ToList());
                foreach(var dctvm in dragObjList) {
                    dctvm.DoCommandSelection();
                    cil.AddRange(dctvm.SelectedItems.Select(x => x.CopyItem).ToList());
                }
                cil = cil.Distinct().ToList();
                for (int i = 0; i < cil.Count; i++) {
                    MpCopyItem ci = cil[i];
                    if(i == 0) {
                        ci.CompositeParentCopyItemId = 0;
                    } else {
                        ci.CompositeParentCopyItemId = cil[0].Id;
                    }
                    ci.CompositeSortOrderIdx = i;
                    ci.WriteToDatabase();
                }

                MpClipTrayViewModel.Instance.RefreshClips();
            }
            Reset();
        }

        public void CancelDrop() {
            Reset();
        }

        public bool IsDragDataValid(List<MpClipTileViewModel> vml, int nDropIdx) {
            return true;
            //only allow combing same type content
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            if(vml.All(x => x.ItemType == ctvm.ItemType)) {
                return !IsDragDataDropTarget(vml, nDropIdx);
            }
            return true;
        }

        public bool IsDragDataDropTarget(List<MpClipTileViewModel> vml, int nDropIdx) {
            //this method is used to cancel drop by returning true
            //when drag data is at dropIdx which may not be clear when drag data 
            //is already part of the current list
            if (vml == null || vml.Count == 0 || nDropIdx < 0) {
                return false;
            }
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            var odcivml = new List<MpContentItemViewModel>(); //ordered drop civm list
            foreach (var dctvm in vml) {
                dctvm.DoCommandSelection();
                odcivml.AddRange(dctvm.SelectedItems);
            }

            //getting here means drop data is local (reordering)
            //temp list is items ordered by list idx not selection order 
            odcivml = odcivml.Distinct().OrderBy(x => ctvm.ItemViewModels.IndexOf(x)).ToList();
            //since all drop items are local we need to normalize the drop idx
            int testIdx = nDropIdx;
            if(testIdx > 0) {
                testIdx = testIdx - 1;
            }
            //the only way this is a valid drop is when all selected items aren't affected
            //by drop idx
            for (int i = testIdx; i < odcivml.Count; i++) {
                int odcivmIdx = ctvm.ItemViewModels.IndexOf(odcivml[i]);
                if(i != odcivmIdx) {
                    return false;
                }
            }
            return true;
        }

        private void Reset() {
            dropIdx = -1;
            dragObjList = null;
            lineAdorner.IsShowing = false;
            AssociatedObject.ContentListAdornerLayer.Update();
        }

        
    }
}
