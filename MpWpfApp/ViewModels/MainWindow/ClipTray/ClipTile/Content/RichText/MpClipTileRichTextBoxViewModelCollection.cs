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
                    OnPropertyChanged(nameof(SubSelectedClipItems));
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
        public MpRtbListBoxItemRichTextBoxViewModel PrimarySubSelectedClipItem {
            get {
                if(SubSelectedClipItems == null || SubSelectedClipItems.Count < 1) {
                    return null;
                }
                return SubSelectedClipItems[0];
            }
        }

        public List<MpRtbListBoxItemRichTextBoxViewModel> SubSelectedClipItems {
            get {
                return this.Where(x => x.IsSubSelected).OrderBy(x => x.LastSubSelectedDateTime).ToList();
            }
        }

        //public MpRtbListBoxItemRichTextBoxViewModel SubSelectedRtbvm {
        //    get {
        //        if (SubSelectedClipItems.Count > 0) {
        //            return SubSelectedClipItems[0];
        //        }
        //        return null;
        //    }
        //}

        //public RichTextBox SubSelectedRtb {
        //    get {
        //        if (SubSelectedRtbvm == null) {
        //            return null;
        //        }
        //        return SubSelectedRtbvm.Rtb;
        //    }
        //}

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
                bool wasEmptySelection = SubSelectedClipItems.Count == 0;
                if (wasEmptySelection) {
                    SubSelectAll();
                }
                var sb = new StringBuilder();
                foreach (var srtbvm in SubSelectedClipItems) {
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
                bool wasEmptySelection = SubSelectedClipItems.Count == 0;
                if (wasEmptySelection) {
                    SubSelectAll();
                }
                var fl = new List<string>();
                foreach (var srtbvm in SubSelectedClipItems) {
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
                bool wasEmptySelection = SubSelectedClipItems.Count == 0;
                if (wasEmptySelection) {
                    SubSelectAll();
                }
                var sb = new StringBuilder();
                foreach (var sctvm in SubSelectedClipItems) {
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
                bool wasEmptySelection = SubSelectedClipItems.Count == 0;
                if(wasEmptySelection) {
                    SubSelectAll();
                }
                MpEventEnabledFlowDocument fd = string.Empty.ToRichText().ToFlowDocument();
                foreach (var sctvm in SubSelectedClipItems.OrderBy(x => x.LastSubSelectedDateTime)) {
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

        public double RtbListBoxHeight {
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
                if(HostClipTileViewModel.DetailGridVisibility != Visibility.Visible) {
                    ch += HostClipTileViewModel.TileDetailHeight;
                }
                if (this.Count == 1) {
                    return ch;
                }
                return Math.Max(RtbLbScrollViewerHeight, Math.Max(ch,TotalItemHeight));
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
                return HostClipTileViewModel.IsEditingTemplate;
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
            ListBox = (ListBox)sender;
            IsHorizontal = false;
            RtbContainerGrid = ListBox.GetVisualAncestor<Grid>();
            RtbListBoxCanvas = ListBox.GetVisualAncestor<Canvas>();
            ScrollViewer = ListBox.GetVisualAncestor<ScrollViewer>();//(ScrollViewer)HostClipTileViewModel.ClipBorder.FindName("ClipTileRichTextBoxListBoxScrollViewer");//RtbLbAdornerLayer.GetVisualAncestor<ScrollViewer>();
            ListBox.RequestBringIntoView += (s, e65) => { 
                //if(!MainWindowViewModel.ClipTrayViewModel.IsAnyTileExpanded) {
                //    return;
                //}
                e65.Handled = true; 
            };                                    

            //after pasting template rtb's are duplicated so clear them upon refresh
            SyncItemsWithModel();            

            ListBox.SelectionChanged += (s, e8) => {
                if (SubSelectedClipItems.Count > 1) {
                    //order selected tiles by ascending datetime 
                    var subSelectedRtbvmListBySelectionTime = SubSelectedClipItems.OrderBy(x => x.LastSubSelectedDateTime).ToList();
                    foreach (var srtbvm in subSelectedRtbvmListBySelectionTime) {
                        if (srtbvm == subSelectedRtbvmListBySelectionTime[0]) {
                            srtbvm.IsPrimarySubSelected = true;
                        } else {
                            srtbvm.IsPrimarySubSelected = false;
                        }
                    }
                } else if (SubSelectedClipItems.Count == 1) {
                    SubSelectedClipItems[0].IsPrimarySubSelected = false;
                }

                foreach (var osctvm in e8.RemovedItems) {
                    if (osctvm.GetType() == typeof(MpRtbListBoxItemRichTextBoxViewModel)) {
                        ((MpRtbListBoxItemRichTextBoxViewModel)osctvm).IsSubSelected = false;
                        ((MpRtbListBoxItemRichTextBoxViewModel)osctvm).IsPrimarySubSelected = false;
                    }
                }

            };

            RtbLbAdornerLayer = AdornerLayer.GetAdornerLayer(ListBox);
            RtbLbAdornerLayer.Add(new MpRtbListBoxAdorner(ListBox));
        }

        
        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();            
            ListBox?.Items.Refresh();
            sw.Stop();
            //Console.WriteLine("Rtblb(HVIdx:"+MainWindowViewModel.ClipTrayViewModel.VisibleSubRtbViewModels.IndexOf(HostClipTileViewModel)+") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public async Task FillAllTemplates() {
            bool hasExpanded = false;
            foreach (var rtbvm in this) {
                if (rtbvm.HasTemplate) {
                    rtbvm.IsSubSelected = true;
                    rtbvm.IsPastingTemplate = true;
                    if (!hasExpanded) {
                        //tile will be shrunk in on completed of hide window
                        MainWindowViewModel.ExpandClipTile(HostClipTileViewModel);
                        hasExpanded = true;
                    } else {
                        HostClipTileViewModel.PasteTemplateToolbarViewModel.SetTemplate(rtbvm.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder[0].TemplateName);
                    }
                    await Task.Run(() => {
                        while (!HostClipTileViewModel.PasteTemplateToolbarViewModel.IsTemplateReadyToPaste) {
                            System.Threading.Thread.Sleep(100);
                        }
                        //TemplateRichText is set in PasteTemplateCommand
                    });
                }
                
            }
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
            foreach (var srtbvm in SubSelectedClipItems) {
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
            //RtbListBoxHeight += (-deltaTop + deltaHeight);
            RtbLbScrollViewerWidth += deltaWidth;
            RtbLbScrollViewerHeight += deltaHeight;
            //if (RtbListBoxHeight > RtbLbScrollViewerHeight) {
            //    RtbLbScrollViewerWidth -= MpMeasurements.Instance.ScrollbarWidth;
            //}
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

            if (base.ListBox != null) {
                base.ListBox.Height = RtbListBoxHeight;
                base.ListBox.Width = RtbLbWidth;
                base.ListBox.UpdateLayout();
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
                    //RtbListBoxHeight += dh;
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
                if(ListBox != null) {
                    ((ListBoxItem)ListBox.ItemContainerGenerator.ContainerFromItem(this[0]))?.Focus();
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
            ListBox = null;
            RtbLbAdornerLayer = null;
    }
        #endregion

        #region Commands

        private RelayCommand<object> _toggleEditSubSelectedItemCommand;
        public ICommand ToggleEditSubSelectedItemCommand {
            get {
                if (_toggleEditSubSelectedItemCommand == null) {
                    _toggleEditSubSelectedItemCommand = new RelayCommand<object>(ToggleEditSubSelectedItem, CanToggleEditSubSelectedItem);
                }
                return _toggleEditSubSelectedItemCommand;
            }
        }
        private bool CanToggleEditSubSelectedItem(object args) {
            if (MainWindowViewModel.IsLoading) {
                return false;
            }
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                   this.SubSelectedClipItems.Count == 1;
        }
        private void ToggleEditSubSelectedItem(object args) {
            var selectedRtbvm = this.SubSelectedClipItems[0];
            if (!HostClipTileViewModel.IsEditingTile) {
                HostClipTileViewModel.IsEditingTile = true;
            }
            selectedRtbvm.IsSubSelected = true;
        }

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
            return SubSelectedClipItems.Count > 0 &&
                   SubSelectedClipItems.Any(x => VisibleSubRtbViewModels.IndexOf(x) != VisibleSubRtbViewModels.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SubSelectedClipItems.Max(x => VisibleSubRtbViewModels.IndexOf(x));
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
            return SubSelectedClipItems.Count > 0 && SubSelectedClipItems.Any(x => VisibleSubRtbViewModels.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SubSelectedClipItems.Min(x => VisibleSubRtbViewModels.IndexOf(x));
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

        private AsyncCommand<Brush> _changeSubSelectedClipsColorCommand;
        public IAsyncCommand<Brush> ChangeSubSelectedClipsColorCommand {
            get {
                if (_changeSubSelectedClipsColorCommand == null) {
                    _changeSubSelectedClipsColorCommand = new AsyncCommand<Brush>(ChangeSubSelectedClipsColor);
                }
                return _changeSubSelectedClipsColorCommand;
            }
        }
        private async Task ChangeSubSelectedClipsColor(Brush brush) {
            if (brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            foreach (var sctvm in SubSelectedClipItems) {
                                sctvm.CopyItemColorBrush = brush;
                            }
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private RelayCommand<object> _pasteSubSelectedClipsCommand;
        public ICommand PasteSubSelectedClipsCommand {
            get {
                if (_pasteSubSelectedClipsCommand == null) {
                    _pasteSubSelectedClipsCommand = new RelayCommand<object>(PasteSubSelectedClips, CanPasteSubSelectedClips);
                }
                return _pasteSubSelectedClipsCommand;
            }
        }
        private bool CanPasteSubSelectedClips(object ptapId) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate &&
                !IsTrialExpired;
        }
        private void PasteSubSelectedClips(object ptapId) {
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

        private AsyncCommand _bringSubSelectedClipTilesToFrontCommand;
        public IAsyncCommand BringSubSelectedClipTilesToFrontCommand {
            get {
                if (_bringSubSelectedClipTilesToFrontCommand == null) {
                    _bringSubSelectedClipTilesToFrontCommand = new AsyncCommand(BringSubSelectedClipTilesToFront, CanBringSubSelectedClipTilesToFront);
                }
                return _bringSubSelectedClipTilesToFrontCommand;
            }
        }
        private bool CanBringSubSelectedClipTilesToFront(object arg) {
            if (IsBusy || MainWindowViewModel.IsLoading || VisibleSubRtbViewModels.Count == 0) {
                return false;
            }
            bool canBringForward = false;
            for (int i = 0; i < SubSelectedClipItems.Count && i < VisibleSubRtbViewModels.Count; i++) {
                if (!SubSelectedClipItems.Contains(VisibleSubRtbViewModels[i])) {
                    canBringForward = true;
                    break;
                }
            }
            return canBringForward;
        }
        private async Task BringSubSelectedClipTilesToFront() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SubSelectedClipItems;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                this.Move(this.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;
                            }
                            base.ListBox.ScrollIntoView(SubSelectedClipItems[0]);
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private AsyncCommand _sendSubSelectedClipTilesToBackCommand;
        public IAsyncCommand SendSubSelectedClipTilesToBackCommand {
            get {
                if (_sendSubSelectedClipTilesToBackCommand == null) {
                    _sendSubSelectedClipTilesToBackCommand = new AsyncCommand(SendSubSelectedClipTilesToBack, CanSendSubSelectedClipTilesToBack);
                }
                return _sendSubSelectedClipTilesToBackCommand;
            }
        }
        private bool CanSendSubSelectedClipTilesToBack(object args) {
            if (IsBusy || MainWindowViewModel.IsLoading || VisibleSubRtbViewModels.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SubSelectedClipItems.Count && i < VisibleSubRtbViewModels.Count; i++) {
                if (!SubSelectedClipItems.Contains(VisibleSubRtbViewModels[VisibleSubRtbViewModels.Count - 1 - i])) {
                    canSendBack = true;
                    break;
                }
            }
            return canSendBack;
        }
        private async Task SendSubSelectedClipTilesToBack() {
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            var tempSelectedClipTiles = SubSelectedClipItems;
                            ClearSubSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                this.Move(this.IndexOf(sctvm), this.Count - 1);
                                sctvm.IsSelected = true;
                            }
                            base.ListBox.ScrollIntoView(SubSelectedClipItems[SubSelectedClipItems.Count - 1]);
                        }));
            } finally {
                IsBusy = false;
            }
        }


        private RelayCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if(_searchWebCommand == null) {
                    _searchWebCommand = new RelayCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if(args == null || args.GetType() != typeof(string)) {
                return;
            }
            MpHelpers.Instance.OpenUrl(args.ToString() + System.Uri.EscapeDataString(SubSelectedClipTilesMergedPlainText));
        }

        private RelayCommand _deleteSubSelectedClipsCommand;
        public ICommand DeleteSubSelectedClipsCommand {
            get {
                if (_deleteSubSelectedClipsCommand == null) {
                    _deleteSubSelectedClipsCommand = new RelayCommand(DeleteSubSelectedClips, CanDeleteSubSelectedClips);
                }
                return _deleteSubSelectedClipsCommand;
            }
        }
        private bool CanDeleteSubSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsAnyEditingContent &&
                !IsAnyEditingTitle &&
                !IsAnyPastingTemplate; 
        }
        private void DeleteSubSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SubSelectedClipItems) {
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

        private RelayCommand<MpTagTileViewModel> _linkTagToSubSelectedClipsCommand;
        public ICommand LinkTagToSubSelectedClipsCommand {
            get {
                if (_linkTagToSubSelectedClipsCommand == null) {
                    _linkTagToSubSelectedClipsCommand = new RelayCommand<MpTagTileViewModel>(LinkTagToSubSelectedClips, CanLinkTagToSubSelectedClips);
                }
                return _linkTagToSubSelectedClipsCommand;
            }
        }
        private bool CanLinkTagToSubSelectedClips(MpTagTileViewModel tagToLink) {
            //this checks the selected clips association with tagToLink
            //and only returns if ALL selecteds clips are linked or unlinked 
            if (tagToLink == null || SubSelectedClipItems == null || SubSelectedClipItems.Count == 0) {
                return false;
            }
            if (SubSelectedClipItems.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.IsLinkedWithRtbItem(SubSelectedClipItems[0]);
            foreach (var selectedClipTile in SubSelectedClipItems) {
                if (tagToLink.IsLinkedWithRtbItem(selectedClipTile) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToSubSelectedClips(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinkedWithRtbItem(SubSelectedClipItems[0]);
            foreach (var selectedClipTile in SubSelectedClipItems) {
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
            return SubSelectedClipItems.Count == 1;
        }
        private void AssignHotkey() {
            SubSelectedClipItems[0].AssignHotkeyToSubSelectedItemCommand.Execute(null);
        }

        private RelayCommand _invertSubSelectionCommand;
        public ICommand InvertSubSelectionCommand {
            get {
                if (_invertSubSelectionCommand == null) {
                    _invertSubSelectionCommand = new RelayCommand(InvertSubSelection, CanSubInvertSelection);
                }
                return _invertSubSelectionCommand;
            }
        }
        private bool CanSubInvertSelection() {
            return SubSelectedClipItems.Count != VisibleSubRtbViewModels.Count;
        }
        private void InvertSubSelection() {
            var sctvml = SubSelectedClipItems;
            ClearSubSelection();
            foreach (var vctvm in VisibleSubRtbViewModels) {
                if (!sctvml.Contains(vctvm)) {
                    vctvm.IsSelected = true;
                }
            }
        }

        private AsyncCommand _speakSubSelectedClipsAsyncCommand;
        public IAsyncCommand SpeakSubSelectedClipsAsyncCommand {
            get {
                if (_speakSubSelectedClipsAsyncCommand == null) {
                    _speakSubSelectedClipsAsyncCommand = new AsyncCommand(SpeakSubSelectedClipsAsync, CanSpeakSubSelectedClipsAsync);
                }
                return _speakSubSelectedClipsAsyncCommand;
            }
        }
        private bool CanSpeakSubSelectedClipsAsync(object args) {
            foreach (var sctvm in SubSelectedClipItems) {
                if (!string.IsNullOrEmpty(sctvm.CopyItemPlainText)) {
                    return true;
                }
            }
            return false;
        }
        private async Task SpeakSubSelectedClipsAsync() {
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

                foreach (var sctvm in SubSelectedClipItems) {
                    //speechSynthesizer.SpeakAsync(sctvm.CopyItemPlainText);
                    promptBuilder.AppendText(Environment.NewLine + sctvm.CopyItemPlainText);
                }

                // Speak the contents of the prompt asynchronously.
                speechSynthesizer.SpeakAsync(promptBuilder);

            }, DispatcherPriority.Background);
        }

        private RelayCommand _duplicateSubSelectedClipsCommand;
        public ICommand DuplicateSubSelectedClipsCommand {
            get {
                if (_duplicateSubSelectedClipsCommand == null) {
                    _duplicateSubSelectedClipsCommand = new RelayCommand(DuplicateSubSelectedClips);
                }
                return _duplicateSubSelectedClipsCommand;
            }
        }
        private void DuplicateSubSelectedClips() {
            var tempSubSelectedRtbvml = SubSelectedClipItems;
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
