using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
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

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModelCollection : MpObservableCollectionViewModel<MpClipTileRichTextBoxViewModel>, ICloneable, IDropTarget {
        #region Private Variables
        private Point _mouseDownPosition = new Point();
        private Point _lastMousePosition = new Point();
        private bool _isMouseDown = false;
        #endregion        

        #region Properties

        #region ViewModels
        private MpClipTileViewModel _hostClipTileViewModel;
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                return _hostClipTileViewModel;
            }
            set {
                if (_hostClipTileViewModel != value) {
                    _hostClipTileViewModel = value;
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                    OnPropertyChanged(nameof(SelectedClipTileRichTextBoxViewModel));
                    OnPropertyChanged(nameof(SelectedRtb));
                }
            }
        }

        public MpClipTileRichTextBoxViewModel SelectedClipTileRichTextBoxViewModel {
            get {
                if(HostClipTileViewModel == null || this.Count == 0) {
                    return null;
                }
                foreach(var rtbvm in this) {
                    if(rtbvm.ClipTileViewModel.IsSelected) {
                        return rtbvm;
                    }
                }
                return null;
            }
        }

        public RichTextBox SelectedRtb {
            get {
                if(SelectedClipTileRichTextBoxViewModel == null) {
                    return null;
                }
                return SelectedClipTileRichTextBoxViewModel.Rtb;
            }
        }

        public MpEventEnabledFlowDocument FullDocument {
            get {
                var fullDocument = MpHelpers.Instance.ConvertRichTextToFlowDocument(MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty));

                foreach(var rtbvm in this) {
                    MpHelpers.Instance.CombineFlowDocuments((MpEventEnabledFlowDocument)rtbvm.Rtb.Document, fullDocument, true);
                }
                return fullDocument;
            }
        }
        #endregion

        #region Layout
        public double RelativeWidthMax {
            get {
                double maxWidth = 0;
                foreach(var rtbvm in this) {
                    maxWidth = Math.Max(maxWidth, rtbvm.RtbRelativeWidthMax);
                }
                return maxWidth;
            }
        }

        public double TotalItemHeight {
            get {
                double totalHeight = 0;
                foreach(var rtbvm in this) {
                    totalHeight += rtbvm.RtbListBoxItemHeight;
                }
                return totalHeight;
            }
        }
        #endregion

        #region Business Logic
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {
            CanAcceptChildren = true;
            HostClipTileViewModel = ctvm;
            CollectionChanged += (s, e) => {
                foreach (MpClipTileRichTextBoxViewModel newItem in this) {
                    newItem.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                    newItem.CompositeSortOrderIdx = this.IndexOf(newItem);
                    newItem.CopyItem.WriteToDatabase();
                }
                //if(this.Count > 0) {
                //    SelectRichTextBoxViewModel(0);
                //}
            };
        }
        
        public void ClipTileRichTextBoxViewModelCollection_Loaded(object sender, RoutedEventArgs args) {
            var richTextListBox = (ListBox)sender;

            #region Drag & Drop
            //richTextListBox.PreviewMouseDown += ClipTileRichTextBoxViewModel_PreviewMouseDown;
            //richTextListBox.PreviewMouseUp += (s, e) => {
            //    _isMouseDown = false;
            //};
            //richTextListBox.Drop += ClipTileRichTextBoxViewModel_Drop;
            #endregion

        }

        #region Drag & Drop
        void IDropTarget.DragOver(IDropInfo dropInfo) {
            //var sourceItem = dropInfo.Data as MpClipTileRichTextBoxViewModel;
            //MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            //MpClipTileRichTextBoxViewModel targetRtbVm = null;
            //if(dropInfo.TargetItem is MpClipTileRichTextBoxViewModel) {
            //    targetRtbVm = dropInfo.TargetItem as MpClipTileRichTextBoxViewModel;
            //    targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
            //} else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {                
            //    targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
            //    if(targetRtbVmCollection.Count > 0) {
            //        if (dropInfo.DropPosition.Y < 0) {
            //            targetRtbVm = targetRtbVmCollection[0];
            //        } else {
            //            targetRtbVm = targetRtbVmCollection[targetRtbVmCollection.Count - 1];
            //        }
            //    }                
            //}
            //if (sourceItem != null && targetRtbVm != null) {
            //    dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            //    dropInfo.Effects = DragDropEffects.Move;
            //}

            var sourceItem = dropInfo.Data as MpClipTileRichTextBoxViewModel;
            var targetItem = dropInfo.TargetItem as MpClipTileRichTextBoxViewModel;

            if (sourceItem != null && targetItem != null) {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo) {
            var sourceRtbvm = dropInfo.Data as MpClipTileRichTextBoxViewModel;
            if(sourceRtbvm == null) {
                return;
            }
            var sourceRtbVmCollection = sourceRtbvm.RichTextBoxViewModelCollection;

            MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            MpClipTileRichTextBoxViewModel targetRtbVm = null;
            if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModel) {
                targetRtbVm = dropInfo.TargetItem as MpClipTileRichTextBoxViewModel;
                targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
            } else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {
                targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
                if (targetRtbVmCollection.Count > 0) {
                    if (dropInfo.DropPosition.Y < 0) {
                        targetRtbVm = targetRtbVmCollection[0];
                    } else {
                        targetRtbVm = targetRtbVmCollection[targetRtbVmCollection.Count - 1];
                    }
                }
            }
            
            if(targetRtbVmCollection != null) {
                sourceRtbVmCollection.Remove(sourceRtbvm);

                if (sourceRtbVmCollection != targetRtbVmCollection) {
                    sourceRtbVmCollection.UpdateSortOrder();
                }
                int targetIdx = targetRtbVmCollection.IndexOf(targetRtbVm);

                targetRtbVmCollection.Insert(targetIdx, sourceRtbvm);

                targetRtbVmCollection.UpdateSortOrder();               
            }
        }

        public void UpdateSortOrder() {
            HostClipTileViewModel.CopyItem.CompositeItemList.Clear();
            foreach(var rtbvm in this) {
                rtbvm.CompositeSortOrderIdx = this.IndexOf(rtbvm);
                rtbvm.CompositeParentCopyItemId = this.HostClipTileViewModel.CopyItemId;
                HostClipTileViewModel.CopyItem.CompositeItemList.Add(rtbvm.CopyItem);
            }
            HostClipTileViewModel.CopyItem.WriteToDatabase();
        }
        public MpClipTileRichTextBoxViewModel GetItemFromMouseLocation(Point p) {
            var result = VisualTreeHelper.HitTest(HostClipTileViewModel.RichTextBoxListBox, p);
            if(result != null && result.VisualHit != null) {
                if(result.VisualHit.GetType() == typeof(MpClipTileRichTextBoxViewModel)) {
                    return (MpClipTileRichTextBoxViewModel)result.VisualHit;
                }
                for (int i = 0; i < this.Count; i++) {
                    double top = i > 0 ? this[i - 1].RtbListBoxItemHeight : 0;
                    double bottom = this[i].RtbListBoxItemHeight;
                    if(p.Y >= top && p.Y <= bottom) {
                        return this[i];
                    }
                }
                if(this.Count > 0) {
                    if (p.Y <= 0) {
                        return this[0];
                    }
                    return this[this.Count - 1];
                }
            }
            return null;
        }
        #endregion

        public new void Add(MpClipTileRichTextBoxViewModel rtbvm) {            
            base.Add(rtbvm);
            //ClipTileViewModel.RichTextBoxListBox.Items.Refresh();
        }

        public void AnimateItems(double fromWidth,double toWidth, double fromHeight, double toHeight,double fromTop, double toTop,double fromBottom, double toBottom) {
            if(toWidth > 0) {
                foreach (var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromWidth,
                            toWidth,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { rtbvm.Rtb },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                                rtbvm.UpdateLayout();
                            });
                }
            }
            if (toHeight > 0) {

            }
            if (toTop > 0) {
                foreach(var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromTop,
                            toTop,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { rtbvm.Rtb },
                            Canvas.TopProperty,
                            (s1, e44) => {

                            });
                    fromTop += rtbvm.RtbListBoxItemHeight;
                    toTop += rtbvm.RtbListBoxItemHeight;
                }
            }
            if (toBottom > 0) {

            }
        }

        public void SelectRichTextBoxViewModel(int idx) {
            if(idx < 0 || idx >= this.Count) {
                return;
            }
            for (int i = 0; i < this.Count; i++) {
                this[i].SetSelection(i == idx);
            }
        }

        public void SelectRichTextBoxViewModel(MpClipTileRichTextBoxViewModel rtbvm) {
            if(!this.Contains(rtbvm)) {
                return;
            }
            SelectRichTextBoxViewModel(this.IndexOf(rtbvm));
        }

        public void ClearSelection() {
            foreach(var rtbvm in this) {
                rtbvm.SetSelection(false);
            }
        }

        public void ResetSelection() {
            ClearSelection();
            if(this.Count > 0) {
                this[0].SetSelection(true);
            }
        }

        public void ClearAllHyperlinks() {
            foreach(var rtbvm in this) {
                rtbvm.ClearHyperlinks();
            }
        }

        public void CreateAllHyperlinks() {
            foreach (var rtbvm in this) {
                rtbvm.CreateHyperlinks();
            }
        }

        public object Clone() {
            var nrtbvmc = new MpClipTileRichTextBoxViewModelCollection(HostClipTileViewModel);
            foreach(var rtbvm in this) {
                nrtbvmc.Add((MpClipTileRichTextBoxViewModel)rtbvm.Clone());
            }
            return nrtbvmc;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
