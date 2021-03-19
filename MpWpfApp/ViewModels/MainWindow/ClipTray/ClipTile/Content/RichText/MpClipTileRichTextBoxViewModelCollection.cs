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
using System.Windows.Media.Imaging;

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
                if (this.Count > 0) {
                    this[0].IsSubSelected = true;
                    return this[0];
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
        public Canvas RtbListBoxCanvas { get; set; }

        public ListBox RichTextBoxListBox { get; set; }

        public AdornerLayer RtbLbAdornerLayer { get; set; }
        #endregion

        #region Appearance
        public Point DropLeftPoint { get; set; }

        public Point DropRightPoint { get; set; }

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
        public bool IsDropping { get; set; } = false;

        public bool IsAnyDragging {
            get {
                return this.Any(x => x.IsDragging);
            }
        }

        public bool IsAnyOverDragButton {
            get {
                return this.Any(x => x.IsOverDragButton);
            }
        }

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

        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModelCollection() : base() { }

        public MpClipTileRichTextBoxViewModelCollection(MpClipTileViewModel ctvm) : base() {
            HostClipTileViewModel = ctvm;
            CollectionChanged += (s, e) => {
                if (HostClipTileViewModel.CopyItemType == MpCopyItemType.Composite && e.NewItems != null && e.NewItems.Count > 0) {
                    UpdateSortOrder();                    
                }
            };
        }
        
        public void ClipTileRichTextBoxViewModelCollection_Loaded(object sender, RoutedEventArgs args) {
            RichTextBoxListBox = (ListBox)sender;
            RtbListBoxCanvas = RichTextBoxListBox.GetVisualAncestor<Canvas>();

            RichTextBoxListBox.RequestBringIntoView += (s, e65) => { e65.Handled = true; };
            
            RichTextBoxListBox.SelectionChanged += (s, e4) => {
                OnPropertyChanged(nameof(SubSelectedRtbvmList));
                OnPropertyChanged(nameof(SubSelectedRtbvm));
                OnPropertyChanged(nameof(SubSelectedRtb));
            };

            //after pasting template rtb's are duplicated so clear them upon refresh
            if (HostClipTileViewModel.CopyItemType == MpCopyItemType.Composite) {
                this.Clear();
                //HostClipTileViewModel.CopyItem.CompositeItemList.Sort(x => x.CompositeSortOrderIdx);
                foreach (var cci in HostClipTileViewModel.CopyItem.CompositeItemList) {
                    this.Add(new MpRtbListBoxItemRichTextBoxViewModel(HostClipTileViewModel, cci));
                }
                this.Sort(x => x.CompositeSortOrderIdx);
            }

            #region Drag/Drop
            RichTextBoxListBox.MouseEnter += (s3, e5) => {
                if(MainWindowViewModel.ClipTrayViewModel.IsAnyDragging) {
                    Console.WriteLine("Yo");
                }
            };
            HostClipTileViewModel.ClipBorder.DragLeave += (s2, e1) => {
                //IsDropping = false;
                IsDropping = false;
                RtbLbAdornerLayer.Update();
            };
            HostClipTileViewModel.ClipBorder.PreviewDragOver += (s2, e1) => {
                //e1.Effects = DragDropEffects.None;
                IsDropping = false;
                RtbLbAdornerLayer.Update();
                if (HostClipTileViewModel.IsDragging || IsAnyDragging) {
                    return;
                }
                if (e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    var dctvml = (List<MpClipTileViewModel>)e1.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                    if (dctvml != null) {
                        //foreach (var dctvm in dctvml) {
                        //    if(dctvm.RichTextBoxViewModelCollection == this) {
                        //        return;
                        //    }
                        //}
                    } else {
                        return;
                    }
                } else if (e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                    var drtbvml = (List<MpRtbListBoxItemRichTextBoxViewModel>)e1.Data.GetData(Properties.Settings.Default.ClipTileSubItemDragDropFormat);
                    if (drtbvml == null) {
                        return;
                    }
                } else {
                    return;
                }
                
                int overIdx = GetClosestItemIdx(e1.GetPosition(RichTextBoxListBox));
                if (overIdx >= 0) {
                    var overRect = this[overIdx].ItemRect;
                    double overMidY = overRect.Top + (overRect.Height / 2);
                    if (e1.GetPosition(RichTextBoxListBox).Y > overMidY) {
                        DropLeftPoint = overRect.BottomLeft;
                        DropRightPoint = overRect.BottomRight;

                    } else {
                        DropLeftPoint = overRect.TopLeft;
                        DropRightPoint = overRect.TopRight;
                    }
                    IsDropping = true;
                    e1.Effects = DragDropEffects.Move;
                    e1.Handled = true;
                }

                RtbLbAdornerLayer.Update();
            };

            HostClipTileViewModel.ClipBorder.PreviewDrop += (s3, e2) => {
                bool wasDropped = false;
                List<MpCopyItem> dcil = new List<MpCopyItem>();
                if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    var dctvml = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                    if (dctvml != null) {
                        foreach(var dctvm in dctvml) {
                            dcil.Add(dctvm.CopyItem);
                        }
                    }
                } else if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileSubItemDragDropFormat)) {
                    var drtbvml = (List<MpRtbListBoxItemRichTextBoxViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileSubItemDragDropFormat);
                    if (drtbvml != null) {
                        foreach (var drtbvm in drtbvml) {
                            dcil.Add(drtbvm.CopyItem);
                        }
                    }
                }
                if (dcil != null && dcil.Count > 0) {
                    int dropIdx = GetClosestItemIdx(e2.GetPosition(RichTextBoxListBox));
                    if (dropIdx >= 0) {
                        var overRect = this[dropIdx].ItemRect;
                        double overMidY = overRect.Top + (overRect.Height / 2);
                        if (e2.GetPosition(RichTextBoxListBox).Y > overMidY) {
                            dropIdx++;
                        } else {
                            
                        }
                        for (int i = 0; i < dcil.Count; i++) {
                            HostClipTileViewModel.MergeClip(dcil[i],dropIdx);                            
                        }
                        UpdateSortOrder(true);
                        wasDropped = true;
                    }
                }
                if (!wasDropped) {
                    e2.Effects = DragDropEffects.None;
                    e2.Handled = true;
                } else {
                    e2.Handled = true;
                }
                IsDropping = false;
                RtbLbAdornerLayer.Update();
            };
            #endregion

            RtbLbAdornerLayer = AdornerLayer.GetAdornerLayer(RichTextBoxListBox);
            RtbLbAdornerLayer.Add(new MpRichTextBoxListBoxOverlayAdorner(RichTextBoxListBox));
        }

        
        public void Refresh() {
            RichTextBoxListBox?.Items.Refresh();
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if(fromModel) {
                //foreach(var cci in HostClipTileViewModel.CopyItem.CompositeItemList) {
                //    var rtbvm = this.Where(x => x.CopyItemId == cci.CopyItemId).First();
                //    if(rtbvm != null) {
                //        this.Move(this.IndexOf(rtbvm), cci.CompositeSortOrderIdx);
                //    }
                //}
            } else {
                //foreach (var rtbvm in this) {
                //    rtbvm.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                //    rtbvm.CompositeSortOrderIdx = this.IndexOf(rtbvm);
                //    rtbvm.CopyItem.WriteToDatabase();
                //    rtbvm.RtbcAdornerLayer?.Update();
                //}
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

        public void Remove(MpRtbListBoxItemRichTextBoxViewModel rtbvm, bool isMerge = false) {
            base.Remove(rtbvm);
            if (rtbvm.CopyItem == null) {
                //occurs when duplicate detected on background thread
                return;
            }

            if(isMerge) {
                if(rtbvm.IsCompositeChild) {
                    rtbvm.CopyItem.ItemTitleSwirl = HostClipTileViewModel.TitleSwirl;
                    if (HostClipTileViewModel.CopyItem.CompositeItemList.Contains(rtbvm.CopyItem)) {
                        rtbvm.CopyItem.UnlinkFromCompositeParent();
                        HostClipTileViewModel.CopyItem.CompositeItemList.Remove(rtbvm.CopyItem);
                    }
                }
            } else {
                //foreach (var ttvm in MainWindowViewModel.TagTrayViewModel) {
                //    if (ttvm.Tag.IsLinkedWithCopyItem(rtbvm.CopyItem)) {
                //        ttvm.Tag.UnlinkWithCopyItem(HostClipTileViewModel.CopyItem);
                //        ttvm.TagClipCount--;
                //    }
                //}
                //if (!isMerge) {
                //    rtbvm.CopyItem.DeleteFromDatabase();
                //}

                ////remove any shortcuts associated with clip
                //var scvmToRemoveList = new List<MpShortcutViewModel>();
                //foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == rtbvm.CopyItem.CopyItemId).ToList()) {
                //    scvmToRemoveList.Add(scvmToRemove);
                //}
                //foreach (var scvmToRemove in scvmToRemoveList) {
                //    MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
                //}
                rtbvm.Dispose();
                rtbvm = null;
            }
            
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
                this[0].SetSelection(true,false, false);
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
        private int GetClosestItemIdx(Point mp) {
            double mdy = mp.Y;
            double minDist = double.MaxValue;
            int dropIdx = 0;
            foreach (var rtbvm in this) {
                //var lbi = (ListBoxItem)RichTextBoxListBox.ItemContainerGenerator.ContainerFromItem(rtbvm);
                double lbity = rtbvm.ItemRect.Top;//lbi.TranslatePoint(new Point(0.0, 0.0), RichTextBoxListBox).Y;
                double lbiby = rtbvm.ItemRect.Bottom;//lbi.TranslatePoint(new Point(rtbvm.Rtbc.ActualHeight, 0), RichTextBoxListBox).Y;
                double tDist = Math.Abs(mdy - lbity);
                double bDist = Math.Abs(mdy - lbiby);
                double dist = Math.Min(tDist, bDist);
                if (dist < minDist) {
                    minDist = dist;
                    dropIdx = this.IndexOf(rtbvm);
                }
            }
            return dropIdx;
        }

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
