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
    public class MpClipTileRichTextBoxViewModelCollection : MpObservableCollectionViewModel<MpRtbListBoxItemRichTextBoxViewModel>, ICloneable, IDropTarget {
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

        public MpRtbListBoxItemRichTextBoxViewModel SelectedClipTileRichTextBoxViewModel {
            get {
                if(HostClipTileViewModel == null || this.Count == 0) {
                    return null;
                }
                foreach(var rtbvm in this) {
                    if(rtbvm.IsSubSelected) {
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
                return GetFullDocument();
            }
        }

        public MpEventEnabledFlowDocument FullSeparatedDocument {
            get {
                return GetFullSeperatedDocument();
            }
        }
        #endregion

        #region Layout
        private double _rtbListBoxHeight = MpMeasurements.Instance.ClipTileContentHeight;
        public double RtbListBoxHeight {
            get {
                return _rtbListBoxHeight;
            }
            set {
                if(_rtbListBoxHeight != value) {
                    _rtbListBoxHeight = value;
                    OnPropertyChanged(nameof(RtbListBoxHeight));
                }
            }
        }

        public double RtbListBoxDesiredHeight {
            get {
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (HostClipTileViewModel.IsEditingTile) {
                    ch -= MpMeasurements.Instance.ClipTileEditToolbarHeight;
                }
                if (HostClipTileViewModel.IsPastingTemplateTile) {
                    ch -= MpMeasurements.Instance.ClipTilePasteTemplateToolbarHeight;
                }
                if (HostClipTileViewModel.IsEditingTemplate) {
                    ch -= MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
                }
                if (this.Count == 1) {
                    return ch;
                }
                return Math.Max(ch,TotalItemHeight);
            }
        }

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
                    totalHeight += rtbvm.RtbCanvasHeight;
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
                if (HostClipTileViewModel.CopyItemType == MpCopyItemType.Composite) {
                    foreach (MpRtbListBoxItemRichTextBoxViewModel newItem in this) {
                        newItem.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                        newItem.CompositeSortOrderIdx = this.IndexOf(newItem);
                        newItem.CopyItem.WriteToDatabase();
                    }
                    HostClipTileViewModel.ContentPreviewToolTipBmpSrc = null;
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.ContentPreviewToolTipBmpSrc));
                }
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

            var sourceItem = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
            var targetItem = dropInfo.TargetItem as MpRtbListBoxItemRichTextBoxViewModel;

            if (sourceItem != null && targetItem != null) {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        void IDropTarget.Drop(IDropInfo dropInfo) {
            var sourceRtbvm = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
            if(sourceRtbvm == null) {
                return;
            }
            var sourceRtbVmCollection = sourceRtbvm.RichTextBoxViewModelCollection;

            MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            MpRtbListBoxItemRichTextBoxViewModel targetRtbVm = null;
            if (dropInfo.TargetItem is MpRtbListBoxItemRichTextBoxViewModel) {
                targetRtbVm = dropInfo.TargetItem as MpRtbListBoxItemRichTextBoxViewModel;
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
        public MpRtbListBoxItemRichTextBoxViewModel GetItemFromMouseLocation(Point p) {
            var result = VisualTreeHelper.HitTest(HostClipTileViewModel.RichTextBoxListBox, p);
            if(result != null && result.VisualHit != null) {
                if(result.VisualHit.GetType() == typeof(MpRtbListBoxItemRichTextBoxViewModel)) {
                    return (MpRtbListBoxItemRichTextBoxViewModel)result.VisualHit;
                }
                for (int i = 0; i < this.Count; i++) {
                    double top = i > 0 ? this[i - 1].RtbCanvasHeight : 0;
                    double bottom = this[i].RtbCanvasHeight;
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

        public new void Add(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {            
            base.Add(rtbvm);
            //ClipTileViewModel.RichTextBoxListBox.Items.Refresh();
        }

        public void AnimateItems(
            double fromWidth,double toWidth, 
            double fromHeight, double toHeight,
            double fromTop, double toTop,
            double fromBottom, double toBottom, 
            double animMs) {
            if(toWidth > 0) {
                foreach (var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromWidth,
                            toWidth,
                            animMs,
                            new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                                rtbvm.UpdateLayout();
                            });
                }
            }
            if (toHeight > 0) {
                double heightDiff = toHeight - fromHeight;
                foreach (var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            rtbvm.Rtbc.ActualHeight,
                            rtbvm.Rtbc.ActualHeight - heightDiff,
                            animMs,
                            new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
                            FrameworkElement.HeightProperty,
                            (s1, e44) => {
                                rtbvm.UpdateLayout();
                                if(!HostClipTileViewModel.IsExpanded) {
                                    rtbvm.IsEditingContent = false;
                                    rtbvm.IsSubSelected = false;
                                    rtbvm.IsHovering = false;
                                    rtbvm.OnPropertyChanged(nameof(rtbvm.SubItemOverlayVisibility));
                                }
                            });
                }
            }
            if (toTop > 0) {
                foreach(var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            fromTop,
                            toTop,
                            animMs,
                            new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
                            Canvas.TopProperty,
                            (s1, e44) => {

                            });
                    fromTop += rtbvm.RtbCanvasHeight;
                    toTop += rtbvm.RtbCanvasHeight;
                }
            }
            if (toBottom > 0) {

            }
        }

        public void SelectRichTextBoxViewModel(int idx, bool isInitEdit, bool isInitPaste) {
            if(idx < 0 || idx >= this.Count/* || this[idx].IsSubSelected*/) {
                return;
            }
            for (int i = 0; i < this.Count; i++) {
                this[i].SetSelection(i == idx,isInitEdit, isInitPaste);
            }
        }

        public void SelectRichTextBoxViewModel(MpRtbListBoxItemRichTextBoxViewModel rtbvm, bool isInitEdit, bool isInitPaste) {
            if(!this.Contains(rtbvm)) {
                return;
            }
            SelectRichTextBoxViewModel(this.IndexOf(rtbvm),isInitEdit, isInitPaste);
        }

        public void ClearSubSelection() {
            foreach(var rtbvm in this) {
                rtbvm.IsPrimarySelected = false;
                rtbvm.IsHovering = false;
                rtbvm.IsSubSelected = false;
                rtbvm.IsEditingContent = false;
                rtbvm.IsEditingSubTitle = false;
            }
        }

        public void ResetSubSelection() {
            ClearSubSelection();
            if(this.Count > 0) {
                this[0].SetSelection(true,false, false);
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
                nrtbvmc.Add((MpRtbListBoxItemRichTextBoxViewModel)rtbvm.Clone());
            }
            return nrtbvmc;
        }
        #endregion

        #region Private Methods
        private MpEventEnabledFlowDocument GetFullDocument() {
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            foreach (var rtbvm in this) {
                MpEventEnabledFlowDocument fd = null;
                if (rtbvm.Rtb == null) {
                    fd = rtbvm.CopyItemRichText.ToFlowDocument();
                } else {
                    fd = rtbvm.Rtb.Document.Clone();
                }
                MpHelpers.Instance.CombineFlowDocuments(
                    fd,
                    fullDocument,
                    true);
            }
            return fullDocument;
        }

        private MpEventEnabledFlowDocument GetFullSeperatedDocument(string separatorChar = "- ") {
            int maxCols = int.MinValue;
            foreach (var rtbvm in this) {
                maxCols = Math.Max(maxCols, MpHelpers.Instance.GetColCount(rtbvm.CopyItemPlainText));
            }
            string separatorLine = string.Empty;
            for(int i = 0;i < maxCols;i++) {
                separatorLine += separatorChar;
            }
            var separatorDocument = separatorLine.ToRichText().ToFlowDocument();
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            for (int i = 0; i < this.Count; i++) {
                var rtbvm = this[i];
                if(i % 2 == 1) {
                    MpHelpers.Instance.CombineFlowDocuments(
                    separatorDocument,
                    fullDocument,
                    true);
                }
                MpEventEnabledFlowDocument fd = null;
                if (rtbvm.Rtb == null) {
                    fd = rtbvm.CopyItemRichText.ToFlowDocument();
                } else {
                    fd = rtbvm.Rtb.Document.Clone();
                }
                MpHelpers.Instance.CombineFlowDocuments(
                    fd,
                    fullDocument,
                    true);
            }
            return fullDocument;
        }
        #endregion
    }
}
