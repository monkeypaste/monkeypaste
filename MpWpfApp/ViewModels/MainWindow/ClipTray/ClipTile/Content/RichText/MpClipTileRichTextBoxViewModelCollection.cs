using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
                    OnPropertyChanged(nameof(SubSelectedRtbvmList));
                }
            }
        }

        public List<MpRtbListBoxItemRichTextBoxViewModel> SubSelectedRtbvmList {
            get {
                return this.Where(x => x.IsSubSelected).ToList();
            }
        }

        public MpRtbListBoxItemRichTextBoxViewModel SubSelectedRtbvm {
            get {
                var srtbvml = this.Where(x => x.IsSubSelected).ToList();
                if (srtbvml.Count > 0) {
                    return srtbvml[0];
                }
                //if (this.Count > 0) {
                //    this[0].IsSubSelected = true;
                //    return this[0];
                //}
                return null;
            }
        }

        public RichTextBox SubSelectedRtb {
            get {
                if (SubSelectedRtbvm == null) {
                    return null;
                }
                return SubSelectedRtbvm.Rtb;
            }
        }

        public MpEventEnabledFlowDocument FullDocument {
            get {
                return GetFullDocument();
            }
        }

        #endregion

        #region Selection
        public string SubSelectedClipTilesPlainText {
            get {
                string outStr = string.Empty;
                foreach (var srtbvm in SubSelectedRtbvmList) {
                    if (srtbvm.HasTemplate) {
                        outStr += MpHelpers.Instance.ConvertRichTextToPlainText(srtbvm.TemplateRichText) + Environment.NewLine;
                    } else {
                        outStr += srtbvm.CopyItemPlainText + Environment.NewLine;
                    }
                }
                return outStr.Trim('\r', '\n');
            }
        }

        //public string SelectedClipTilesRichText {
        //    get {
        //        string outStr = MpHelpers.Instance.ConvertPlainTextToRichText(string.Empty);
        //        foreach (var sctvm in SelectedClipTiles) {
        //            outStr = MpHelpers.Instance.CombineRichText2(outStr, sctvm.CopyItemRichText);                    
        //        }
        //        return outStr;
        //    }
        //}

        public BitmapSource SubSelectedClipTilesBmp {
            get {
                var bmpList = new List<BitmapSource>();
                foreach (var srtbvm in SubSelectedRtbvmList) {
                    bmpList.Add(srtbvm.CopyItemBmp);
                }
                return MpHelpers.Instance.CombineBitmap(bmpList, false);
            }
        }

        public string SubSelectedClipTilesCsv {
            get {
                string outStr = string.Empty;
                foreach (var srtbvm in SubSelectedRtbvmList) {
                    outStr = srtbvm.CopyItem.ItemPlainText + ",";
                }
                return outStr;
            }
        }

        public string[] SubSelectedClipTilesFileList {
            get {
                var fl = new List<string>();
                foreach (var srtbvm in SubSelectedRtbvmList) {
                    foreach (string f in srtbvm.CopyItemFileDropList) {
                        fl.Add(f);
                    }
                }
                return fl.ToArray();
            }
        }
        #endregion

        #region Controls
        public Canvas RtbListBoxCanvas;

        public Grid RtbContainerGrid;

        private ListBox _richTextListBox;
        public ListBox RichTextBoxListBox {
            get {
                return _richTextListBox;
            }
            set {
                if(_richTextListBox != value) {
                    _richTextListBox = value;
                    OnPropertyChanged(nameof(RichTextBoxListBox));
                }
            }
        }

        public AdornerLayer RtbLbAdornerLayer;
        #endregion

        #region Appearance
        public Point DropLeftPoint { get; set; }

        public Point DropRightPoint { get; set; }

        #endregion

        #region Layout
        private double _rtblbCanvasTop = 0;
        public double RtblbCanvasTop {
            get {
                return _rtblbCanvasTop;
            }
            set {
                if(_rtblbCanvasTop != value) {
                    _rtblbCanvasTop = value;
                    OnPropertyChanged(nameof(RtblbCanvasTop));
                }
            }
        }

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
                if (HostClipTileViewModel.IsPastingTemplate) {
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
        #endregion

        #region State
        
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() : base() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {
            HostClipTileViewModel = ctvm;
            SyncItemsWithModel();
        }
        
        public void ClipTileRichTextBoxViewModelCollection_Loaded(object sender, RoutedEventArgs args) {
            RichTextBoxListBox = (ListBox)sender;
            ListBox = RichTextBoxListBox;
            IsHorizontal = false;
            RtbContainerGrid = RichTextBoxListBox.GetVisualAncestor<Grid>();
            RtbListBoxCanvas = RichTextBoxListBox.GetVisualAncestor<Canvas>();

            RichTextBoxListBox.RequestBringIntoView += (s, e65) => { e65.Handled = true; };

            //RichTextBoxListBox.SelectionChanged += (s, e4) => {
            //    OnPropertyChanged(nameof(SubSelectedRtbvmList));
            //    OnPropertyChanged(nameof(SubSelectedRtbvm));
            //    OnPropertyChanged(nameof(SubSelectedRtb));
            //};

            //after pasting template rtb's are duplicated so clear them upon refresh
            SyncItemsWithModel();
            

            RichTextBoxListBox.SelectionChanged += (s, e8) => {
                if (SubSelectedRtbvmList.Count > 1) {
                    //order selected tiles by ascending datetime 
                    var subSelectedRtbvmListBySelectionTime = SubSelectedRtbvmList.OrderBy(x => x.LastSubSelectedDateTime).ToList();
                    foreach (var srtbvm in subSelectedRtbvmListBySelectionTime) {
                        if (srtbvm == subSelectedRtbvmListBySelectionTime[0]) {
                            srtbvm.IsPrimarySubSelected = true;
                        } else {
                            srtbvm.IsPrimarySubSelected = false;
                        }
                    }
                } else if (SubSelectedRtbvmList.Count == 1) {
                    SubSelectedRtbvmList[0].IsPrimarySubSelected = false;
                }

                foreach (var osctvm in e8.RemovedItems) {
                    if (osctvm.GetType() == typeof(MpRtbListBoxItemRichTextBoxViewModel)) {
                        ((MpRtbListBoxItemRichTextBoxViewModel)osctvm).IsSubSelected = false;
                        ((MpRtbListBoxItemRichTextBoxViewModel)osctvm).IsPrimarySubSelected = false;
                    }
                }
            };

            RtbLbAdornerLayer = AdornerLayer.GetAdornerLayer(RichTextBoxListBox);
            RtbLbAdornerLayer.Add(new MpRichTextBoxListBoxOverlayAdorner(RichTextBoxListBox));
        }

        
        public void Refresh() {
            RichTextBoxListBox?.Items.Refresh();
        }

        public MpRtbListBoxItemRichTextBoxViewModel GetRtbItemByCopyItemId(int copyItemId) {
            foreach(var rtbvm in this) {
                if(rtbvm.CopyItemId == copyItemId) {
                    return rtbvm;
                }
            }
            return null;
        }

        public void SyncItemsWithModel() {
            var hci = HostClipTileViewModel.CopyItem;
            if (HostClipTileViewModel.CopyItemType == MpCopyItemType.Composite) {
                //this.Clear();
                foreach (var cci in hci.CompositeItemList) {
                    var rtbvm = this.Where(x => x.CopyItemId == cci.CopyItemId).FirstOrDefault();
                    if(rtbvm == null) {
                        this.Add(new MpRtbListBoxItemRichTextBoxViewModel(HostClipTileViewModel, cci));
                    }
                }
                UpdateSortOrder(true);
            } else if(HostClipTileViewModel.CopyItemType == MpCopyItemType.RichText) {
                var rtbvm = this.Where(x => x.CopyItemId == hci.CopyItemId).FirstOrDefault();
                if(rtbvm == null) {
                    this.Add(new MpRtbListBoxItemRichTextBoxViewModel(HostClipTileViewModel, hci));                    
                } 
            }
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if(fromModel) {
                this.Sort(x => x.CompositeSortOrderIdx);
            } else {
                foreach (var rtbvm in this) {
                    rtbvm.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                    rtbvm.CompositeSortOrderIdx = this.IndexOf(rtbvm);
                    rtbvm.CopyItem.WriteToDatabase();
                    rtbvm.RtbcAdornerLayer?.Update();
                }
            }
        }

        public async Task<IDataObject> GetDataObjectFromSubSelectedItems(bool isDragDrop = false) {
            IDataObject d = new DataObject();
            //only when pasting into explorer must have file drop
            if (string.IsNullOrEmpty(MpClipboardManager.Instance.LastWindowWatcher.LastTitle.Trim()) &&
                isDragDrop) {
                if (SubSelectedClipTilesFileList != null) {
                    d.SetData(DataFormats.FileDrop, SubSelectedClipTilesFileList);
                }
            }
            if (SubSelectedClipTilesBmp != null) {
                d.SetData(DataFormats.Bitmap, SubSelectedClipTilesBmp);
            }
            if (SubSelectedClipTilesCsv != null) {
                d.SetData(DataFormats.CommaSeparatedValue, SubSelectedClipTilesCsv);
            }

            string rtf = string.Empty;
            foreach (var srtbvm in SubSelectedRtbvmList) {
                var task = srtbvm.GetPastableRichText();
                string rt = await task;
                if (string.IsNullOrEmpty(rtf)) {
                    rtf = rt;
                } else {
                    rtf = MpHelpers.Instance.CombineRichText(rtf, rt);
                }
            }
            if (!string.IsNullOrEmpty(rtf)) {
                d.SetData(DataFormats.Rtf, rtf);
                d.SetData(DataFormats.Text, rtf.ToPlainText());
            }


            if (isDragDrop && SubSelectedRtbvmList != null && SubSelectedRtbvmList.Count > 0) {
                d.SetData(Properties.Settings.Default.ClipTileSubItemDragDropFormat, SubSelectedRtbvmList.ToList());
            }
            return d;
            //awaited in MainWindowViewModel.HideWindow
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

            if (HostClipTileViewModel.IsExpanded) {
                if (RelativeWidthMax > RichTextBoxListBox.ActualWidth) {
                    sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                } else {
                    sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }

                if (TotalItemHeight > RichTextBoxListBox.ActualHeight - HostClipTileViewModel.EditRichTextBoxToolbarHeight) {
                    sv.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                }
            } else {
                sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                sv.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }

            //sv.Height = TotalItemHeight;
            //sv.UpdateLayout();
            //sv.InvalidateScrollInfo();


            //OnPropertyChanged(nameof(RtbVerticalScrollbarVisibility));
            //OnPropertyChanged(nameof(RtbHorizontalScrollbarVisibility));
            //Console.WriteLine("Total Item Height: " + RichTextBoxViewModelCollection.TotalItemHeight);
        }

        public new void Add(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {            
            base.Insert(0,rtbvm);           
            //ClipTileViewModel.RichTextBoxListBox.Items.Refresh();
        }

        public void Remove(MpRtbListBoxItemRichTextBoxViewModel rtbvm, bool isMergeToTray = false, bool isMergeToTile = false) {
            base.Remove(rtbvm);
            if (rtbvm.CopyItem == null) {
                //occurs when duplicate detected on background thread
                return;
            }

            if(isMergeToTray && rtbvm.IsCompositeChild) {
                //rtbvm.CopyItem.ItemTitleSwirl = HostClipTileViewModel.TitleSwirl.Clone();
            } 

            HostClipTileViewModel.CopyItem.UnlinkCompositeChild(rtbvm.CopyItem);

            if(this.Count == 0) {
                HostClipTileViewModel.Dispose();
            } else if(this.Count == 1) {
                var loneCompositeCopyItem = this[0].CopyItem;
                HostClipTileViewModel.CopyItem.UnlinkCompositeChild(loneCompositeCopyItem);
                HostClipTileViewModel.CopyItem.DeleteFromDatabase();
                HostClipTileViewModel.CopyItem = loneCompositeCopyItem;
            }

            if(!isMergeToTile && !isMergeToTray) {
                rtbvm.CopyItem.DeleteFromDatabase();
            }           
        }
        public void Resize(double deltaTop,double deltaContentHeight) {
            RtblbCanvasTop += deltaTop;
            Canvas.SetTop(RtbContainerGrid, RtblbCanvasTop);
            RtbListBoxHeight += deltaContentHeight;
            UpdateLayout();
        }
        public void Animate(
            double deltaTop, 
            double deltaHeight, 
            double tt, 
            EventHandler onCompleted, 
            double fps = 30,
            DispatcherPriority priority = DispatcherPriority.Render) {
            double fromTop = RtblbCanvasTop;
            double toTop = fromTop + deltaTop;
            double dt = (deltaTop / tt) / fps;

            double fromHeight = RtbListBoxHeight;
            double toHeight = fromHeight + deltaHeight;
            double dh = (deltaHeight / tt) / fps;

            var timer = new DispatcherTimer(priority);
            timer.Interval = TimeSpan.FromMilliseconds(fps);
            timer.Tick += (s, e32) => {
                bool isTopDone = false;
                bool isHeightDone = false;
                if (MpHelpers.Instance.DistanceBetweenValues(RtblbCanvasTop, toTop) > 0.5) {
                    RtblbCanvasTop += dt;
                    Canvas.SetTop(RtbContainerGrid, RtblbCanvasTop);
                } else {
                    isTopDone = true;
                }

                if (MpHelpers.Instance.DistanceBetweenValues(RtbListBoxHeight, toHeight) > 0.5) {
                    RtbListBoxHeight += dh;
                    //foreach (var rtbvm in this) {
                    //    rtbvm.OnPropertyChanged(nameof(rtbvm.RtbCanvasHeight));
                    //    rtbvm.OnPropertyChanged(nameof(rtbvm.RtbHeight));
                    //    rtbvm.OnPropertyChanged(nameof(rtbvm.RtbPageHeight));
                    //}
                } else {
                    isHeightDone = true;
                }
                if (isTopDone && isHeightDone) {
                    timer.Stop();
                    UpdateLayout();
                    if(onCompleted != null) {
                        onCompleted.BeginInvoke(this, new EventArgs(), null, null);
                    }
                }
            };
            timer.Start();
        }

        //public void AnimateItems(
        //    double fromWidth,double toWidth, 
        //    double fromHeight, double toHeight,
        //    double fromTop, double toTop,
        //    double fromBottom, double toBottom, 
        //    double animMs) {
        //    if(toWidth > 0) {
        //        foreach (var rtbvm in this) {
        //            if(rtbvm.Rtbc == null) {
        //                //not sure why theres nulls here, maybe happens when there's more items than can be visible
        //                continue;
        //            }
        //            MpHelpers.Instance.AnimateDoubleProperty(
        //                    fromWidth,
        //                    toWidth,
        //                    animMs,
        //                    new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
        //                    FrameworkElement.WidthProperty,
        //                    (s1, e44) => {
        //                        rtbvm.UpdateLayout();
        //                    });
        //        }
        //    }
        //    if (toHeight > 0) {
        //        //double heightDiff = toHeight - fromHeight;
        //        foreach (var rtbvm in this) {
        //            if (rtbvm.Rtbc == null) {
        //                //not sure why theres nulls here, maybe happens when there's more items than can be visible
        //                continue;
        //            }
        //            MpHelpers.Instance.AnimateDoubleProperty(
        //                    rtbvm.Rtbc.ActualHeight,
        //                    rtbvm.RtbCanvasHeight + rtbvm.RtbPadding.Top + rtbvm.RtbPadding.Bottom,
        //                    animMs,
        //                    new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
        //                    FrameworkElement.HeightProperty,
        //                    (s1, e44) => {
        //                        rtbvm.UpdateLayout();
        //                        if(!HostClipTileViewModel.IsExpanded) {
        //                            rtbvm.IsEditingContent = false;
        //                            rtbvm.IsSubSelected = false;
        //                            rtbvm.IsSubHovering = false;
        //                            rtbvm.OnPropertyChanged(nameof(rtbvm.SubItemOverlayVisibility));
        //                        }
        //                    });
        //        }
        //    }
        //    if (toTop > 0) {
        //        foreach(var rtbvm in this) {
        //            if (rtbvm.Rtbc == null) {
        //                //not sure why theres nulls here, maybe happens when there's more items than can be visible
        //                continue;
        //            }
        //            MpHelpers.Instance.AnimateDoubleProperty(
        //                    fromTop,
        //                    toTop,
        //                    animMs,
        //                    new List<FrameworkElement> { rtbvm.Rtb, rtbvm.Rtbc, rtbvm.RtbListBoxItemClipBorder, rtbvm.RtbListBoxItemOverlayDockPanel },
        //                    Canvas.TopProperty,
        //                    (s1, e44) => {

        //                    });
        //            fromTop += rtbvm.RtbCanvasHeight;
        //            toTop += rtbvm.RtbCanvasHeight;
        //        }
        //    }
        //    if (toBottom > 0) {

        //    }
        //}

        //public void SelectRichTextBoxViewModel(int idx, bool isInitEdit, bool isInitPaste) {
        //    if(idx < 0 || idx >= this.Count/* || this[idx].IsSubSelected*/) {
        //        return;
        //    }
        //    for (int i = 0; i < this.Count; i++) {
        //        this[i].SetSelection(i == idx,isInitEdit, isInitPaste);
        //    }
        //}

        //public void SelectRichTextBoxViewModel(MpRtbListBoxItemRichTextBoxViewModel rtbvm, bool isInitEdit, bool isInitPaste) {
        //    if(!this.Contains(rtbvm)) {
        //        return;
        //    }
        //    SelectRichTextBoxViewModel(this.IndexOf(rtbvm),isInitEdit, isInitPaste);
        //}

        public void ClearSubSelection() {
            foreach(var rtbvm in this) {
                rtbvm.IsPrimarySubSelected = false;
                rtbvm.IsSubHovering = false;
                rtbvm.IsSubSelected = false;
                rtbvm.IsEditingContent = false;
                rtbvm.IsEditingSubTitle = false;
            }
        }

        public void ResetSubSelection() {
            ClearSubSelection();
            if(this.Count > 0) {
                this[0].IsSubSelected = true;
                if(RichTextBoxListBox != null) {
                    ((ListBoxItem)RichTextBoxListBox.ItemContainerGenerator.ContainerFromItem(this[0]))?.Focus();
                }
            }
        }

        public void SelectAll() {
            foreach(var rtbvm in this) {
                rtbvm.IsSubSelected = true;
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
