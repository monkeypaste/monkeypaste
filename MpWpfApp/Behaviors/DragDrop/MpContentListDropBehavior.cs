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
    public class MpContentListDropBehavior : Behavior<MpContentListView> {
        private MpLineAdorner lineAdorner;

        private List<object> dragObjList;
        private int dropIdx = -1;
        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            lineAdorner = new MpLineAdorner(AssociatedObject.ContentListAdornerLayer);
            AssociatedObject.ContentListAdornerLayer.Add(lineAdorner);
        }

        public void DragOver(List<object> vml, int overIdx) {
            var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
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
                foreach(var obj in dragObjList) {
                    if(obj is MpClipTileViewModel octvm) {
                        cil.InsertRange(dropIdx,octvm.ItemViewModels.Select(x => x.CopyItem).ToList());
                    } else if(obj is MpContentItemViewModel civm) {
                        if(ctvm == civm.Parent) {
                            //resorting not move or copy
                            int civmIdx = cil.IndexOf(civm.CopyItem);
                            cil.RemoveAt(civmIdx);
                        }
                        cil.Insert(dropIdx,civm.CopyItem);
                    }
                }

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

        private void Reset() {
            dragObjList = null;
            lineAdorner.IsShowing = false;
            AssociatedObject.ContentListAdornerLayer.Update();
        }
    }
}
