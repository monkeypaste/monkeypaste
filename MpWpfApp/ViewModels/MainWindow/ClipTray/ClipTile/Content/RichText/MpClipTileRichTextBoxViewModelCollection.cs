using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
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
    public class MpClipTileRichTextBoxViewModelCollection : MpUndoableObservableCollectionViewModel<MpClipTileRichTextBoxViewModelCollection,MpRtbListBoxItemRichTextBoxViewModel>, ICloneable,/* IDropTarget, */IDisposable {
        #region Private Variables
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;
        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();
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
        
        public MpEventEnabledFlowDocument FullDocument {
            get {
                return GetFullDocument();
            }
        }

        public List<MpRtbListBoxItemRichTextBoxViewModel> VisibleSubRtbViewModels {
            get {
                return this.Where(x => x.SubItemVisibility == Visibility.Visible).ToList();
            }
        }

        #endregion

        #region Selection
        public MpRtbListBoxItemRichTextBoxViewModel PrimarySubSelectedRtbvm {
            get {
                if(SubSelectedRtbvmList == null || SubSelectedRtbvmList.Count < 1) {
                    return null;
                }
                return SubSelectedRtbvmList[0];
            }
        }

        public List<MpRtbListBoxItemRichTextBoxViewModel> SubSelectedRtbvmList {
            get {
                return this.Where(x => x.IsSubSelected).OrderBy(x => x.LastSelectedDateTime).ToList();
            }
        }

        public MpRtbListBoxItemRichTextBoxViewModel SubSelectedRtbvm {
            get {
                if (SubSelectedRtbvmList.Count > 0) {
                    return SubSelectedRtbvmList[0];
                }
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

        //public BitmapSource SubSelectedClipTilesBmp {
        //    get {
        //        bool wasEmptySelection = SubSelectedRtbvmList.Count == 0;
        //        if (wasEmptySelection) {
        //            SubSelectAll();
        //        }
        //        var bmpList = new List<BitmapSource>();
        //        foreach (var srtbvm in SubSelectedRtbvmList.OrderBy(x => x.LastSubSelectedDateTime)) {
        //            bmpList.Add(srtbvm.CopyItemBmp);
        //        }
        //        if (wasEmptySelection) {
        //            ClearSubSelection();
        //        }
        //        return MpHelpers.Instance.CombineBitmap(bmpList, false);
        //    }
        //}

        public string SubSelectedClipTilesCsv {
            get {
                bool wasEmptySelection = SubSelectedRtbvmList.Count == 0;
                if (wasEmptySelection) {
                    SubSelectAll();
                }
                var sb = new StringBuilder();
                foreach (var srtbvm in SubSelectedRtbvmList) {
                    sb.Append(srtbvm.CopyItem.ItemPlainText + ",");
                }
                if (wasEmptySelection) {
                    ClearSubSelection();
                }
                return sb.ToString();
            }
        }

        public string[] SubSelectedClipTilesFileList {
            get {
                bool wasEmptySelection = SubSelectedRtbvmList.Count == 0;
                if (wasEmptySelection) {
                    SubSelectAll();
                }
                var fl = new List<string>();
                foreach (var srtbvm in SubSelectedRtbvmList) {
                    foreach (string f in srtbvm.CopyItemFileDropList) {
                        fl.Add(f);
                    }
                }
                if (wasEmptySelection) {
                    ClearSubSelection();
                }
                return fl.ToArray();
            }
        }

        public string SubSelectedClipTilesMergedPlainText {
            get {
                bool wasEmptySelection = SubSelectedRtbvmList.Count == 0;
                if (wasEmptySelection) {
                    SubSelectAll();
                }
                var sb = new StringBuilder();
                foreach (var sctvm in SubSelectedRtbvmList) {
                    if (sctvm.HasTemplate) {
                        sb.Append(
                            MpHelpers.Instance.ConvertRichTextToPlainText(sctvm.TemplateRichText) + Environment.NewLine);
                    } else {
                        sb.Append(sctvm.CopyItemPlainText + Environment.NewLine);
                    }
                }
                if (wasEmptySelection) {
                    ClearSubSelection();
                }
                return sb.ToString().Trim('\r', '\n');
            }
        }

        public string[] SubSelectedClipTilesMergedPlainTextFileList {
            get {
                
                string mergedPlainTextFilePath = MpHelpers.Instance.WriteTextToFile(
                    System.IO.Path.GetTempFileName(), SubSelectedClipTilesMergedPlainText, true);

                return new string[] { mergedPlainTextFilePath };
            }
        }

        public string SubSelectedClipTilesMergedRtf {
            get {
                bool wasEmptySelection = SubSelectedRtbvmList.Count == 0;
                if(wasEmptySelection) {
                    SubSelectAll();
                }
                MpEventEnabledFlowDocument fd = string.Empty.ToRichText().ToFlowDocument();
                foreach (var sctvm in SubSelectedRtbvmList.OrderBy(x => x.LastSubSelectedDateTime)) {
                    if (sctvm.HasTemplate) {
                        fd = MpHelpers.Instance.CombineFlowDocuments(sctvm.TemplateRichText.ToFlowDocument(), fd);
                    } else {
                        fd = MpHelpers.Instance.CombineFlowDocuments(sctvm.CopyItemRichText.ToFlowDocument(), fd);
                    }
                }
                if(wasEmptySelection) {
                    ClearSubSelection();
                }
                return fd.ToRichText();
            }
        }

        public string[] SubSelectedClipTilesMergedRtfFileList {
            get {
                string mergedRichTextFilePath = MpHelpers.Instance.WriteTextToFile(
                    System.IO.Path.GetTempFileName(), SubSelectedClipTilesMergedRtf, true);

                return new string[] { mergedRichTextFilePath };
            }
        }
        #endregion

        #region Controls
        private Canvas _rtbListBoxCanvas;
        public Canvas RtbListBoxCanvas {
            get {
                return _rtbListBoxCanvas;
            }
            set {
                if(_rtbListBoxCanvas != value) {
                    _rtbListBoxCanvas = value;
                    OnPropertyChanged(nameof(RtbListBoxCanvas));
                }
            }
        }

        private Grid _rtbContainerGrid;
        public Grid RtbContainerGrid {
            get {
                return _rtbContainerGrid;
            }
            set {
                if(_rtbContainerGrid != value) {
                    _rtbContainerGrid = value;
                    OnPropertyChanged(nameof(RtbContainerGrid));
                }
            }
        }

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

        private AdornerLayer _rtbAdornerLayer;
        public AdornerLayer RtbLbAdornerLayer {
            get {
                return _rtbAdornerLayer;
            }
            set {
                if(_rtbAdornerLayer != value) {
                    _rtbAdornerLayer = value;
                    OnPropertyChanged(nameof(RtbLbAdornerLayer));
                }
            }
        }
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
                if (_rtbListBoxHeight != value) {
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
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                double totalHeight = 0;
                foreach (var rtbvm in this) {
                    totalHeight += rtbvm.RtbCanvasHeight + rtbvm.RtbPadding.Top + rtbvm.RtbPadding.Bottom; ;
                }                
                return totalHeight;
            }
        }

        public double RtbLbWidth {
            get {
                if(VerticalScrollbarVisibility == ScrollBarVisibility.Visible) {
                    return RtbLbScrollViewerWidth - MpMeasurements.Instance.ScrollbarWidth;
                }
                return RtbLbScrollViewerWidth;
            }
        }

        private double _rtbLbScrollViewerHeight = MpMeasurements.Instance.ClipTileContentHeight;
        public double RtbLbScrollViewerHeight {
            get {
                if(HostClipTileViewModel == null) {
                    return 0;
                }
                
                return _rtbLbScrollViewerHeight;
            }
            set {
                if(_rtbLbScrollViewerHeight != value) {
                    _rtbLbScrollViewerHeight = value;
                    OnPropertyChanged(nameof(RtbLbScrollViewerHeight));                }
            }
        }

        private double _rtbLbScrollViewerWidth = MpMeasurements.Instance.ClipTileScrollViewerWidth; 
        public double RtbLbScrollViewerWidth {
            get {
                return _rtbLbScrollViewerWidth;
            }
            set {
                if(_rtbLbScrollViewerWidth != value) {
                    _rtbLbScrollViewerWidth = value;
                    OnPropertyChanged(nameof(RtbLbScrollViewerWidth));
                }
            }
        }
        #endregion

        #region Visibility
        public ScrollBarVisibility HorizontalScrollbarVisibility {
            get {
                if(HostClipTileViewModel == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if(HostClipTileViewModel.IsExpanded) {
                    if (RelativeWidthMax > HostClipTileViewModel.TileContentWidth) {
                        return ScrollBarVisibility.Visible;
                    } 
                }
                return ScrollBarVisibility.Hidden;
            }
        }

        public ScrollBarVisibility VerticalScrollbarVisibility {
            get {
                if (HostClipTileViewModel == null) {
                    return ScrollBarVisibility.Hidden;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    if (TotalItemHeight > RtbListBoxHeight) {
                        return ScrollBarVisibility.Visible;
                    }
                } 
                return ScrollBarVisibility.Hidden;
            }
        }
        #endregion

        #region Business Logic
        #endregion

        #region State
        public bool IsAnyEditingContent {
            get {
                return this.Any(x => x.IsEditingContent);
            }
        }

        public bool IsAnyEditingTitle {
            get {
                return this.Any(x => x.IsEditingSubTitle);
            }
        }

        public bool IsAnyEditingTemplate {
            get {
                return this.Any(x => x.IsEditingTemplate);
            }
        }

        public bool IsAnyPastingTemplate {
            get {
                return this.Any(x => x.IsPastingTemplate);
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() : base() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {
            HostClipTileViewModel = ctvm;
            HostClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(HostClipTileViewModel.IsHovering):
                        OnPropertyChanged(nameof(TotalItemHeight));
                        OnPropertyChanged(nameof(RtbLbScrollViewerWidth));
                        OnPropertyChanged(nameof(HorizontalScrollbarVisibility));
                        OnPropertyChanged(nameof(VerticalScrollbarVisibility));
                        break;
                }
            };
            SyncItemsWithModel();
        }
        
        public void ClipTileRichTextBoxViewModelCollection_Loaded(object sender, RoutedEventArgs args) {
            RichTextBoxListBox = (ListBox)sender;
            ListBox = RichTextBoxListBox;
            IsHorizontal = false;
            RtbContainerGrid = RichTextBoxListBox.GetVisualAncestor<Grid>();
            RtbListBoxCanvas = RichTextBoxListBox.GetVisualAncestor<Canvas>();
            ScrollViewer = RichTextBoxListBox.GetVisualAncestor<ScrollViewer>();//(ScrollViewer)HostClipTileViewModel.ClipBorder.FindName("ClipTileRichTextBoxListBoxScrollViewer");//RtbLbAdornerLayer.GetVisualAncestor<ScrollViewer>();
            RichTextBoxListBox.RequestBringIntoView += (s, e65) => { 
                //if(!MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded) {
                //    return;
                //}
                e65.Handled = true; 
            };                                    

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
            RtbLbAdornerLayer.Add(new MpRtbListBoxAdorner(RichTextBoxListBox));
        }

        
        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();            
            RichTextBoxListBox?.Items.Refresh();
            sw.Stop();
            //Console.WriteLine("Rtblb(HVIdx:"+MainWindowViewModel.ClipTrayViewModel.VisibleSubRtbViewModels.IndexOf(HostClipTileViewModel)+") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public MpRtbListBoxItemRichTextBoxViewModel GetRtbItemByCopyItemId(int copyItemId) {
            foreach(var rtbvm in this) {
                if(rtbvm.CopyItemId == copyItemId) {
                    return rtbvm;
                }
            }
            return null;
        }

        public void SyncMultiSelectDragButton(bool isOver, bool isDown) {
            string transBrush = Brushes.Transparent.ToString();
            string outerBrush = isOver ? "#FF7CA0CC" : isDown ? "#FF2E4E76" : transBrush;
            string innerBrush = isOver ? "#FFE4EFFD" : isDown ? "#FF116EE4" : transBrush;
            string innerBg = isOver ? "#FFDAE7F5" : isDown ? "#FF3272B8" : transBrush;
            foreach (var srtbvm in SubSelectedRtbvmList) {
                var outerBorder = (Border)srtbvm.DragButton.Template.FindName("OuterBorder", srtbvm.DragButton);
                if(outerBorder != null) {
                    outerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString(outerBrush);
                }

                var innerBorder = (Border)srtbvm.DragButton.Template.FindName("InnerBorder", srtbvm.DragButton);
                if(innerBorder != null) {
                    innerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString(innerBrush);
                    innerBorder.Background = (Brush)new BrushConverter().ConvertFromString(innerBg);
                }
            }
        }

        public void SyncItemsWithModel() {
            var sw = new Stopwatch();
            sw.Start();
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
            Refresh();
            UpdateLayout();
            sw.Stop();
            Console.WriteLine("Rtbvmc Sync: " + sw.ElapsedMilliseconds + "ms");
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if(fromModel) {
                this.Sort(x => x.CompositeSortOrderIdx);
            } else {
                foreach (var rtbvm in this) {
                    rtbvm.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                    rtbvm.CompositeSortOrderIdx = this.IndexOf(rtbvm);
                    rtbvm.CopyItem.WriteToDatabase();
                    rtbvm.RtbListBoxItemAdornerLayer?.Update();
                }
            }
        }
        public new void Add(MpRtbListBoxItemRichTextBoxViewModel rtbvm) {            
            base.Insert(0,rtbvm);
            UpdateAdorners();
            //ClipTileViewModel.RichTextBoxListBox.Items.Refresh();
        }
        public async Task AddAsync(MpRtbListBoxItemRichTextBoxViewModel rtbvm,DispatcherPriority priority) {
            await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                this.Add(rtbvm);
            }), priority);
        }
        
        public void Remove(MpRtbListBoxItemRichTextBoxViewModel rtbvm, bool isMerge = false) {
            base.Remove(rtbvm);
            if (rtbvm.CopyItem == null) {
                //occurs when duplicate detected on background thread
                return;
            }

            if(isMerge) {
                rtbvm.HostClipTileViewModel.IsClipDragging = false;
                rtbvm.IsSubDragging = false;
                UpdateAdorners();
            } 

            HostClipTileViewModel.CopyItem.UnlinkCompositeChild(rtbvm.CopyItem);

            if(this.Count == 0) {
                HostClipTileViewModel.Dispose();
            } else if(this.Count == 1) {
                var loneCompositeCopyItem = this[0].CopyItem;
                HostClipTileViewModel.CopyItem.UnlinkCompositeChild(loneCompositeCopyItem);
                HostClipTileViewModel.CopyItem.DeleteFromDatabase();
                HostClipTileViewModel.CopyItem = loneCompositeCopyItem;
            } else {
                UpdateSortOrder();
            }
            

            if(!isMerge) {
                rtbvm.CopyItem.DeleteFromDatabase();
            }
            //Refresh();
            UpdateAdorners();
        }

        public async Task RemoveAsync(
            MpRtbListBoxItemRichTextBoxViewModel rtbvm, 
            bool isMerge = false, 
            DispatcherPriority priority = DispatcherPriority.Background) {
            await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                this.Remove(rtbvm, isMerge);
            }), priority);
        }

        public void UpdateAdorners() {
            RtbLbAdornerLayer?.Update();
            foreach (var rtbvm in this) {
                rtbvm.RtbListBoxItemAdornerLayer?.Update();
            }
        }

        public void Resize(double deltaTop, double deltaWidth, double deltaHeight) {
            RtblbCanvasTop += deltaTop;
            RtbListBoxHeight += (-deltaTop + deltaHeight);
            RtbLbScrollViewerWidth += deltaWidth;
            RtbLbScrollViewerHeight += deltaHeight;
            if (RtbListBoxHeight > RtbLbScrollViewerHeight) {
                RtbLbScrollViewerWidth -= MpMeasurements.Instance.ScrollbarWidth;
            }
            UpdateLayout();

            //Refresh();

           // MainWindowViewModel.ClipTrayViewModel.Refresh();
        }

        public void UpdateLayout() {
            foreach (var rtbvm in this) {
                rtbvm.UpdateLayout();
            }

            UpdateAdorners();

            OnPropertyChanged(nameof(RtbListBoxHeight));
            OnPropertyChanged(nameof(TotalItemHeight));
            OnPropertyChanged(nameof(RtblbCanvasTop));
            OnPropertyChanged(nameof(RtbLbScrollViewerWidth));
            OnPropertyChanged(nameof(RtbLbScrollViewerHeight));
            OnPropertyChanged(nameof(HorizontalScrollbarVisibility));
            OnPropertyChanged(nameof(VerticalScrollbarVisibility));
            OnPropertyChanged(nameof(RtbLbWidth));

            if (ListBox != null) {
                ListBox.Height = TotalItemHeight;
                ListBox.Width = RtbLbWidth;
                ListBox.UpdateLayout();
            }
            if (ScrollViewer != null) {
                ScrollViewer.Width = RtbLbScrollViewerWidth;
                ScrollViewer.Height = RtbLbScrollViewerHeight;
                ScrollViewer.UpdateLayout();
            }

            

            
            //if (RichTextBoxListBox == null || ScrollViewer == null) {
            //    return;
            //}
            //Refresh();


            //ScrollViewer.Width = RelativeWidthMax;
            //ScrollViewer.Height = TotalItemHeight;
            //ScrollViewer.ScrollToVerticalOffset(0);
            //ScrollViewer.ScrollToHorizontalOffset(0);
            //ScrollViewer.UpdateLayout();

            //if (HostClipTileViewModel.IsExpanded) {
            //    if (RelativeWidthMax > HostClipTileViewModel.TileContentWidth) {
            //        HorizontalScrollbarVisibility = ScrollBarVisibility.Visible;
            //    } else {
            //        HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    }

            //    if (TotalItemHeight > RtbListBoxHeight) {
            //        VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            //    }
            //} else {
            //    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //}

            //sv.Height = TotalItemHeight;
            //ScrollViewer.UpdateLayout();
            //sv.InvalidateScrollInfo();


            //OnPropertyChanged(nameof(RtbVerticalScrollbarVisibility));
            //OnPropertyChanged(nameof(RtbHorizontalScrollbarVisibility));
            //Console.WriteLine("Total Item Height: " + TotalItemHeight);
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

            var timer = new DispatcherTimer(priority) { Interval = TimeSpan.FromMilliseconds(fps) };
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


        

        public void SubSelectAll() {
            foreach(var rtbvm in this) {
                rtbvm.IsSubSelected = true;
            }
        }
        public void ClearSubSelection(bool clearEditing = true) {
            foreach(var rtbvm in this) {
                rtbvm.IsPrimarySubSelected = false;
                rtbvm.IsSubHovering = false;
                rtbvm.IsSubSelected = false;
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
                MpEventEnabledFlowDocument fd;
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

        public new void Dispose() {
            base.Dispose();
            RtbListBoxCanvas = null;
            RtbContainerGrid = null;
            RichTextBoxListBox = null;
            RtbLbAdornerLayer = null;
    }
        #endregion

        #region Commands
        private RelayCommand _selectNextItemCommand;
        public ICommand SelectNextItemCommand {
            get {
                if (_selectNextItemCommand == null) {
                    _selectNextItemCommand = new RelayCommand(SelectNextItem, CanSelectNextItem);
                }
                return _selectNextItemCommand;
            }
        }
        private bool CanSelectNextItem() {
            return SubSelectedRtbvmList.Count > 0 &&
                   SubSelectedRtbvmList.Any(x => VisibleSubRtbViewModels.IndexOf(x) != VisibleSubRtbViewModels.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SubSelectedRtbvmList.Max(x => VisibleSubRtbViewModels.IndexOf(x));
            ClearSubSelection();
            VisibleSubRtbViewModels[maxItem + 1].IsSelected = true;
        }

        private RelayCommand _selectPreviousItemCommand;
        public ICommand SelectPreviousItemCommand {
            get {
                if (_selectPreviousItemCommand == null) {
                    _selectPreviousItemCommand = new RelayCommand(SelectPreviousItem, CanSelectPreviousItem);
                }
                return _selectPreviousItemCommand;
            }
        }
        private bool CanSelectPreviousItem() {
            return SubSelectedRtbvmList.Count > 0 && SubSelectedRtbvmList.Any(x => VisibleSubRtbViewModels.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SubSelectedRtbvmList.Min(x => VisibleSubRtbViewModels.IndexOf(x));
            ClearSubSelection();
            VisibleSubRtbViewModels[minItem - 1].IsSelected = true;
        }

        private RelayCommand _selectAllCommand;
        public ICommand SelectAllCommand {
            get {
                if (_selectAllCommand == null) {
                    _selectAllCommand = new RelayCommand(SelectAll);
                }
                return _selectAllCommand;
            }
        }
        private void SelectAll() {
            ClearSubSelection();
            foreach (var ctvm in VisibleSubRtbViewModels) {
                ctvm.IsSelected = true;
            }
        }

        private AsyncCommand<Brush> _changeSelectedClipsColorCommand;
        public IAsyncCommand<Brush> ChangeSelectedClipsColorCommand {
            get {
                if (_changeSelectedClipsColorCommand == null) {
                    _changeSelectedClipsColorCommand = new AsyncCommand<Brush>(ChangeSelectedClipsColor);
                }
                return _changeSelectedClipsColorCommand;
            }
        }
        private async Task ChangeSelectedClipsColor(Brush brush) {
            if (brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            //BitmapSource sharedSwirl = null;
                            foreach (var sctvm in SubSelectedRtbvmList) {
                                sctvm.TitleBackgroundColor = brush;
                                sctvm.TitleSwirlViewModel.ForceBrush(brush);
                                //if (sharedSwirl == null) {
                                //    sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(null,true);
                                //    sharedSwirl = sctvm.TitleSwirl;
                                //} else {
                                //    sctvm.TitleSwirl = sctvm.CopyItem.InitSwirl(sharedSwirl);
                                //}
                                //sctvm.CopyItem.WriteToDatabase();
                            }
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private RelayCommand<object> _pasteSelectedClipsCommand;
        public ICommand PasteSelectedClipsCommand {
            get {
                if (_pasteSelectedClipsCommand == null) {
                    _pasteSelectedClipsCommand = new RelayCommand<object>(PasteSelectedClips, CanPasteSelectedClips);
                }
                return _pasteSelectedClipsCommand;
            }
        }
        private bool CanPasteSelectedClips(object ptapId) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate &&
                !IsTrialExpired;
        }
        private void PasteSelectedClips(object ptapId) {
            if (ptapId != null && ptapId.GetType() == typeof(int) && (int)ptapId > 0) {
                //when pasting to a user defined application
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.Where(x => x.PasteToAppPathId == (int)ptapId).ToList()[0];
            } else if (ptapId != null && ptapId.GetType() == typeof(IntPtr) && (IntPtr)ptapId != IntPtr.Zero) {
                //when pasting to a running application
                _selectedPasteToAppPathWindowHandle = (IntPtr)ptapId;
                _selectedPasteToAppPathViewModel = null;
            } else {
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = null;
            }
            //In order to paste the app must hide first 
            //this triggers hidewindow to paste selected items
            MainWindowViewModel.HideWindowCommand.Execute(true);
        }

        private AsyncCommand _bringSelectedClipTilesToFrontCommand;
        public IAsyncCommand BringSelectedClipTilesToFrontCommand {
            get {
                if (_bringSelectedClipTilesToFrontCommand == null) {
                    _bringSelectedClipTilesToFrontCommand = new AsyncCommand(BringSelectedClipTilesToFront, CanBringSelectedClipTilesToFront);
                }
                return _bringSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSelectedClipTilesToFront(object arg) {
            if (IsBusy || MainWindowViewModel.IsLoading || VisibleSubRtbViewModels.Count == 0) {
                return false;
            }
            bool canBringForward = false;
            for (int i = 0; i < SubSelectedRtbvmList.Count && i < VisibleSubRtbViewModels.Count; i++) {
                if (!SubSelectedRtbvmList.Contains(VisibleSubRtbViewModels[i])) {
                    canBringForward = true;
                    break;
                }
            }
            return canBringForward;
        }
        private async Task BringSelectedClipTilesToFront() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SubSelectedRtbvmList;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                this.Move(this.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;
                            }
                            ListBox.ScrollIntoView(SubSelectedRtbvmList[0]);
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private AsyncCommand _sendSelectedClipTilesToBackCommand;
        public IAsyncCommand SendSelectedClipTilesToBackCommand {
            get {
                if (_sendSelectedClipTilesToBackCommand == null) {
                    _sendSelectedClipTilesToBackCommand = new AsyncCommand(SendSelectedClipTilesToBack, CanSendSelectedClipTilesToBack);
                }
                return _sendSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSelectedClipTilesToBack(object args) {
            if (IsBusy || MainWindowViewModel.IsLoading || VisibleSubRtbViewModels.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SubSelectedRtbvmList.Count && i < VisibleSubRtbViewModels.Count; i++) {
                if (!SubSelectedRtbvmList.Contains(VisibleSubRtbViewModels[VisibleSubRtbViewModels.Count - 1 - i])) {
                    canSendBack = true;
                    break;
                }
            }
            return canSendBack;
        }
        private async Task SendSelectedClipTilesToBack() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SubSelectedRtbvmList;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                this.Move(this.IndexOf(sctvm), this.Count - 1);
                                sctvm.IsSelected = true;
                            }
                            ListBox.ScrollIntoView(SubSelectedRtbvmList[SubSelectedRtbvmList.Count - 1]);
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private RelayCommand _deleteSelectedClipsCommand;
        public ICommand DeleteSelectedClipsCommand {
            get {
                if (_deleteSelectedClipsCommand == null) {
                    _deleteSelectedClipsCommand = new RelayCommand(DeleteSelectedClips, CanDeleteSelectedClips);
                }
                return _deleteSelectedClipsCommand;
            }
        }
        private bool CanDeleteSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate; 
        }
        private void DeleteSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SubSelectedRtbvmList) {
                lastSelectedClipTileIdx = VisibleSubRtbViewModels.IndexOf(ct);
                this.Remove(ct);
            }
            ClearSubSelection();
            if (VisibleSubRtbViewModels.Count > 0) {
                if (lastSelectedClipTileIdx <= 0) {
                    VisibleSubRtbViewModels[0].IsSelected = true;
                } else if (lastSelectedClipTileIdx < VisibleSubRtbViewModels.Count) {
                    VisibleSubRtbViewModels[lastSelectedClipTileIdx].IsSelected = true;
                } else {
                    VisibleSubRtbViewModels[lastSelectedClipTileIdx - 1].IsSelected = true;
                }
            }
        }

        private RelayCommand<MpTagTileViewModel> _linkTagToCopyItemCommand;
        public ICommand LinkTagToCopyItemCommand {
            get {
                if (_linkTagToCopyItemCommand == null) {
                    _linkTagToCopyItemCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToCopyItem, CanLinkTagToCopyItem);
                }
                return _linkTagToCopyItemCommand;
            }
        }
        private bool CanLinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            //this checks the selected clips association with tagToLink
            //and only returns if ALL selecteds clips are linked or unlinked 
            if (tagToLink == null || SubSelectedRtbvmList == null || SubSelectedRtbvmList.Count == 0) {
                return false;
            }
            if (SubSelectedRtbvmList.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.IsLinkedWithClipTile(SubSelectedRtbvmList[0]);
            foreach (var selectedClipTile in SubSelectedRtbvmList) {
                if (tagToLink.IsLinkedWithClipTile(selectedClipTile) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinkedWithClipTile(SubSelectedRtbvmList[0]);
            foreach (var selectedClipTile in SubSelectedRtbvmList) {
                if (isUnlink) {
                    tagToLink.RemoveClip(selectedClipTile);
                } else {
                    tagToLink.AddClip(selectedClipTile);
                }
            }
            MainWindowViewModel.TagTrayViewModel.RefreshAllCounts();
            MainWindowViewModel.TagTrayViewModel.UpdateTagAssociation();
        }

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if (_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey, CanAssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private bool CanAssignHotkey() {
            return SubSelectedRtbvmList.Count == 1;
        }
        private void AssignHotkey() {
            SubSelectedRtbvmList[0].AssignHotkeyCommand.Execute(null);
        }

        private RelayCommand _invertSelectionCommand;
        public ICommand InvertSelectionCommand {
            get {
                if (_invertSelectionCommand == null) {
                    _invertSelectionCommand = new RelayCommand(InvertSelection, CanInvertSelection);
                }
                return _invertSelectionCommand;
            }
        }
        private bool CanInvertSelection() {
            return SubSelectedRtbvmList.Count != VisibleSubRtbViewModels.Count;
        }
        private void InvertSelection() {
            var sctvml = SubSelectedRtbvmList;
            ClearSubSelection();
            foreach (var vctvm in VisibleSubRtbViewModels) {
                if (!sctvml.Contains(vctvm)) {
                    vctvm.IsSelected = true;
                }
            }
        }

        private RelayCommand _mergeSelectedClipsCommand;
        public ICommand MergeSelectedClipsCommand {
            get {
                if (_mergeSelectedClipsCommand == null) {
                    _mergeSelectedClipsCommand = new RelayCommand(MergeSelectedClips, CanMergeSelectedClips);
                }
                return _mergeSelectedClipsCommand;
            }
        }
        private bool CanMergeSelectedClips() {
            //return true;
            if (SubSelectedRtbvmList.Count <= 1) {
                return false;
            }
            bool areAllSameType = true;
            foreach (var sctvm in SubSelectedRtbvmList) {
                if (sctvm.CopyItemType != MpCopyItemType.Composite &&
                    sctvm.CopyItemType != MpCopyItemType.RichText) {
                    areAllSameType = false;
                }
            }
            return areAllSameType;
        }
        private void MergeSelectedClips() {
            var sctvml = SubSelectedRtbvmList;
            var ocil = new List<MpCopyItem>();
            foreach (var sctvm in sctvml) {
                if (sctvm == PrimarySubSelectedRtbvm) {
                    continue;
                }
                ocil.Add(sctvm.CopyItem);
            }

            PrimarySubSelectedRtbvm.MergeClip(ocil);
        }

        private AsyncCommand _speakSelectedClipsAsyncCommand;
        public IAsyncCommand SpeakSelectedClipsAsyncCommand {
            get {
                if (_speakSelectedClipsAsyncCommand == null) {
                    _speakSelectedClipsAsyncCommand = new AsyncCommand(SpeakSelectedClipsAsync, CanSpeakSelectedClipsAsync);
                }
                return _speakSelectedClipsAsyncCommand;
            }
        }
        private bool CanSpeakSelectedClipsAsync(object args) {
            foreach (var sctvm in SubSelectedRtbvmList) {
                if (!string.IsNullOrEmpty(sctvm.CopyItemPlainText)) {
                    return true;
                }
            }
            return false;
        }
        private async Task SpeakSelectedClipsAsync() {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                var speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                if(string.IsNullOrEmpty(Properties.Settings.Default.SpeechSynthVoiceName)) {
                    speechSynthesizer.SelectVoice(speechSynthesizer.GetInstalledVoices()[0].VoiceInfo.Name);
                } else {
                    speechSynthesizer.SelectVoice(Properties.Settings.Default.SpeechSynthVoiceName);
                }
                
                speechSynthesizer.Rate = 0;
                speechSynthesizer.SpeakCompleted += (s, e) => {
                    speechSynthesizer.Dispose();
                };
                // Create a PromptBuilder object and append a text string.
                PromptBuilder promptBuilder = new PromptBuilder();

                foreach (var sctvm in SubSelectedRtbvmList) {
                    //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                    promptBuilder.AppendText(Environment.NewLine + sctvm.CopyItemPlainText);
                }

                // Speak the contents of the prompt asynchronously.
                speechSynthesizer.SpeakAsync(promptBuilder);

            }, DispatcherPriority.Background);
        }

        private RelayCommand _duplicateSelectedClipsCommand;
        public ICommand DuplicateSelectedClipsCommand {
            get {
                if (_duplicateSelectedClipsCommand == null) {
                    _duplicateSelectedClipsCommand = new RelayCommand(DuplicateSelectedClips);
                }
                return _duplicateSelectedClipsCommand;
            }
        }
        private void DuplicateSelectedClips() {
            var tempSubSelectedRtbvml = SubSelectedRtbvmList;
            ClearSubSelection();
            foreach (var srtbvm in tempSubSelectedRtbvml) {
                var clonedCopyItem = (MpCopyItem)srtbvm.CopyItem.Clone();
                clonedCopyItem.WriteToDatabase();
                var rtbvm = new MpRtbListBoxItemRichTextBoxViewModel(HostClipTileViewModel, clonedCopyItem);
                //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                this.Add(rtbvm);
                rtbvm.IsSubSelected = true;
            }
        }
        #endregion
    }
}
