using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModelCollection : MpObservableCollectionViewModel<MpRtbListBoxItemRichTextBoxViewModel>, ICloneable,/* IDropTarget, */IDisposable {
        #region Private Variables
        //private Point _mouseDownPosition = new Point();
        //private Point _lastMousePosition = new Point();
        //private bool _isMouseDown = false;
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
                var srtbvml = this.Where(x => x.IsSubSelected).ToList();
                if(srtbvml.Count > 0) {
                    return srtbvml[0];
                }
                if(this.Count > 0) {
                    this[0].IsSubSelected = true;
                    return this[0];
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

        #endregion

        #region Controls
        public Canvas RtbListBoxCanvas { get; set; }

        public ListBox RichTextBoxListBox { get; set; }

        public AdornerLayer RtbLbAdornerLayer { get; set; }
        #endregion

        #region Appearance
        public Cursor RtbListBoxCursor {
            get {
                if (HostClipTileViewModel == null) {
                    return Cursors.Arrow;
                }
                if (IsCursorOnItemInnerEdge) {
                    return Cursors.SizeNS;
                }
                return Cursors.Arrow;
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

        #region Visibility
        //public ScrollBarVisibility RtbHorizontalScrollbarVisibility {
        //    get {
        //        if(HostClipTileViewModel == null) {
        //            return ScrollBarVisibility.Hidden;
        //        }
        //        if (HostClipTileViewModel.IsExpanded) {
        //            if (RelativeWidthMax > RichTextBoxListBox.ActualWidth) {
        //                return ScrollBarVisibility.Visible;
        //            }
        //        }
        //        return ScrollBarVisibility.Hidden;
        //    }
        //}

        //public ScrollBarVisibility RtbVerticalScrollbarVisibility {
        //    get {
        //        if (HostClipTileViewModel.IsExpanded) {
        //            if (TotalItemHeight > RichTextBoxListBox.ActualHeight - HostClipTileViewModel.EditRichTextBoxToolbarHeight) {
        //                return ScrollBarVisibility.Visible;
        //            }
        //        }
        //        return ScrollBarVisibility.Hidden;
        //    }
        //}
        #endregion

        #region Business Logic
        //private int _loadCount = 0;
        //public int LoadCount {
        //    get {
        //        return _loadCount;
        //    }
        //    set {
        //        if(_loadCount != value) {
        //            _loadCount = value;
        //            OnPropertyChanged(nameof(LoadCount));
        //        }
        //    }
        //}
        #endregion

        #region State
        private bool _isCursorOnItemInnerEdge = false;
        public bool IsCursorOnItemInnerEdge {
            get {
                return _isCursorOnItemInnerEdge;
            }
            set {
                if (_isCursorOnItemInnerEdge != value) {
                    _isCursorOnItemInnerEdge = value;
                    OnPropertyChanged(nameof(IsCursorOnItemInnerEdge));
                    OnPropertyChanged(nameof(RtbListBoxCursor));
                }
            }
        }
        #endregion
        public ObservableCollection<MpRtbListBoxItemRichTextBoxViewModel> Children { 
            get {
                return this;
            }            
        }
        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() : base() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {
            CanAcceptChildren = true;
            HostClipTileViewModel = ctvm;
            CollectionChanged += (s, e) => {
                if (HostClipTileViewModel.CopyItemType == MpCopyItemType.Composite) {
                    UpdateSortOrder();
                    //HostClipTileViewModel.ContentPreviewToolTipBmpSrc = null;
                    //HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.ContentPreviewToolTipBmpSrc));
                }
            };

            //PropertyChanged += (s, e) => {
            //    switch (e.PropertyName) {
            //        case nameof(LoadCount):
            //            if (LoadCount == this.Count) {
            //                //int loadedRtbCount = 0;
            //                //while(loadedRtbCount < this.Count) {
            //                //    if(this[loadedRtbCount].Rtb != null) {
            //                //        loadedRtbCount++;
            //                //    } else {
            //                //        Thread.Sleep(75);
            //                //    }
            //                //}
            //                //HostClipTileViewModel.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList();
            //            }
            //            break;
            //    }
            //};
        }
        
        public void ClipTileRichTextBoxViewModelCollection_Loaded(object sender, RoutedEventArgs args) {
            RichTextBoxListBox = (ListBox)sender;
            RtbListBoxCanvas = RichTextBoxListBox.GetVisualAncestor<Canvas>();

            RichTextBoxListBox.RequestBringIntoView += (s, e65) => { e65.Handled = true; };
            RichTextBoxListBox.MouseMove += (s, e3) => {
                RtbLbAdornerLayer.Update();
            };

            //after pasting template rtb's are duplicated so clear them upon refresh
            if (HostClipTileViewModel.CopyItemType == MpCopyItemType.Composite) {
                this.Clear();
                foreach (var cci in HostClipTileViewModel.CopyItem.CompositeItemList) {
                    this.Add(new MpRtbListBoxItemRichTextBoxViewModel(HostClipTileViewModel, cci));
                }
            }

            RtbLbAdornerLayer = AdornerLayer.GetAdornerLayer(RichTextBoxListBox);
            RtbLbAdornerLayer.Add(new MpRichTextBoxListBoxOverlayAdorner(RichTextBoxListBox));
        }

        #region Drag & Drop
        //void IDropTarget.DragOver(IDropInfo dropInfo) {
            //var sourceItem = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
            //MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
            //MpRtbListBoxItemRichTextBoxViewModel targetRtbVm = null;
            //if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModel) {
            //    targetRtbVm = dropInfo.TargetItem as MpClipTileRichTextBoxViewModel;
            //    targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
            //} else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {
            //    targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
            //    if (targetRtbVmCollection.Count > 0) {
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

        //    var sourceItem = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
        //    var targetItem = dropInfo.TargetItem as MpRtbListBoxItemRichTextBoxViewModel;

        //    if (sourceItem != null && targetItem != null) {
        //        dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
        //        dropInfo.Effects = DragDropEffects.Move;
        //    }
        //}

        //void IDropTarget.Drop(IDropInfo dropInfo) {
        //    var sourceRtbvm = dropInfo.Data as MpRtbListBoxItemRichTextBoxViewModel;
        //    if (sourceRtbvm == null) {
        //        return;
        //    }
        //    var sourceRtbVmCollection = sourceRtbvm.RichTextBoxViewModelCollection;

        //    MpClipTileRichTextBoxViewModelCollection targetRtbVmCollection = null;
        //    MpRtbListBoxItemRichTextBoxViewModel targetRtbVm = null;
        //    if (dropInfo.TargetItem is MpRtbListBoxItemRichTextBoxViewModel) {
        //        targetRtbVm = dropInfo.TargetItem as MpRtbListBoxItemRichTextBoxViewModel;
        //        targetRtbVmCollection = targetRtbVm.RichTextBoxViewModelCollection;
        //    } else if (dropInfo.TargetItem is MpClipTileRichTextBoxViewModelCollection) {
        //        targetRtbVmCollection = dropInfo.TargetItem as MpClipTileRichTextBoxViewModelCollection;
        //        if (targetRtbVmCollection.Count > 0) {
        //            if (dropInfo.DropPosition.Y < 0) {
        //                targetRtbVm = targetRtbVmCollection[0];
        //            } else {
        //                targetRtbVm = targetRtbVmCollection[targetRtbVmCollection.Count - 1];
        //            }
        //        }
        //    }

        //    if (targetRtbVmCollection != null) {
        //        sourceRtbVmCollection.Remove(sourceRtbvm);

        //        if (sourceRtbVmCollection != targetRtbVmCollection) {
        //            sourceRtbVmCollection.UpdateSortOrder();
        //        }
        //        int targetIdx = targetRtbVmCollection.IndexOf(targetRtbVm);

        //        targetRtbVmCollection.Insert(targetIdx, sourceRtbvm);

        //        targetRtbVmCollection.UpdateSortOrder();
        //    }
        //}
        #endregion

        public void Refresh() {
            RichTextBoxListBox?.Items.Refresh();
        }

        public void UpdateSortOrder() {
            foreach (var rtbvm in this) {
                rtbvm.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                rtbvm.CompositeSortOrderIdx = this.IndexOf(rtbvm);
                rtbvm.CopyItem.WriteToDatabase();
                rtbvm.RtbcAdornerLayer?.Update();
            }
        }

        public void UpdateLayout() {
            //RichTextBoxViewModelCollection.RichTextBoxListBox.Width += widthDiff;   
            //Console.WriteLine("TotalItemHeight: " + TotalItemHeight);
            foreach (var rtbvm in this) {
                rtbvm.OnPropertyChanged(nameof(rtbvm.RtbCanvasHeight));
                rtbvm.OnPropertyChanged(nameof(rtbvm.RtbCanvasWidth));
                rtbvm.OnPropertyChanged(nameof(rtbvm.RtbHeight));
                rtbvm.OnPropertyChanged(nameof(rtbvm.RtbWidth));
                rtbvm.OnPropertyChanged(nameof(rtbvm.SubItemOverlayVisibility));
            }

            //HostClipTileViewModel.RichTextBoxListBox.UpdateLayout();
            var rtblb = RichTextBoxListBox;
            if (rtblb == null || VisualTreeHelper.GetChildrenCount(rtblb) <= 0) {
                return;
            }
            var border = (Border)VisualTreeHelper.GetChild(rtblb, 0);
            var sv = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);

            //if(HostClipTileViewModel.IsExpanded) {
            //    if (RelativeWidthMax > RichTextBoxListBox.ActualWidth) {
            //        sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            //    } else {
            //        sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    }

            //    if (TotalItemHeight > RichTextBoxListBox.ActualHeight - HostClipTileViewModel.EditRichTextBoxToolbarHeight) {
            //        sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            //    }
            //} else {
            //    sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //}

            //sv.Height = TotalItemHeight;
            //sv.UpdateLayout();
            //sv.InvalidateScrollInfo();


            //OnPropertyChanged(nameof(RtbVerticalScrollbarVisibility));
            //OnPropertyChanged(nameof(RtbHorizontalScrollbarVisibility));
            //Console.WriteLine("Total Item Height: " + RichTextBoxViewModelCollection.TotalItemHeight);
        }

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
                //double heightDiff = toHeight - fromHeight;
                foreach (var rtbvm in this) {
                    MpHelpers.Instance.AnimateDoubleProperty(
                            rtbvm.Rtbc.ActualHeight,
                            rtbvm.RtbCanvasHeight + rtbvm.RtbPadding.Top + rtbvm.RtbPadding.Bottom,
                            animMs,
                            new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
                            FrameworkElement.HeightProperty,
                            (s1, e44) => {
                                rtbvm.UpdateLayout();
                                if(!HostClipTileViewModel.IsExpanded) {
                                    rtbvm.IsEditingContent = false;
                                    rtbvm.IsSubSelected = false;
                                    rtbvm.IsSubHovering = false;
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
                rtbvm.IsSubHovering = false;
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

        public void Dispose() {
            //LoadCount = 0;
            
        }
        #endregion
    }
}
