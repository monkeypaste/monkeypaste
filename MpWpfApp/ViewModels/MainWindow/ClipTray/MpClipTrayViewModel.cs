using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MpWpfApp {
    public class MpClipTrayViewModel : MpUndoableObservableCollectionViewModel<MpClipTrayViewModel, MpClipTileViewModel>/*, IDropTarget*/ {
        #region Private Variables      
        private IntPtr _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
        
        private MpPasteToAppPathViewModel _selectedPasteToAppPathViewModel = null;

        

        private List<MpClipTileViewModel> _hiddenTiles = new List<MpClipTileViewModel>();

        //private double _originalExpandedTileX = 0;
        //private int _expandedTileVisibleIdx = 0;
        #endregion

        #region Properties

        #region View Models
        public MpClipTileViewModel ExpandedClipTile {
            get {
                foreach(var ctvm in VisibileClipTiles) {
                    if(ctvm.IsExpanded) {
                        return ctvm;
                    }
                }
                return null;
            }
        }

        public MpObservableCollection<MpClipTileViewModel> SelectedClipTiles {
            get {
                return new MpObservableCollection<MpClipTileViewModel>(this.Where(ct => ct.IsSelected).ToList().OrderBy(x => x.LastSelectedDateTime));
            }
        }
        public MpObservableCollection<MpClipTileViewModel> VisibileClipTiles {
            get {
                return new MpObservableCollection<MpClipTileViewModel>(this.Where(ct => ct.TileVisibility == Visibility.Visible && ct.GetType() != typeof(MpRtbListBoxItemRichTextBoxViewModel)).ToList());
            }
        }

        public MpClipTileViewModel LastSelectedClipTile {
            get {
                if (SelectedClipTiles.Count == 0) {
                    return null;
                }
                return SelectedClipTiles[SelectedClipTiles.Count - 1];
            }
        }

        public MpClipTileViewModel PrimarySelectedClipTile {
            get {
                if (SelectedClipTiles.Count == 0) {
                    return null;
                }
                return SelectedClipTiles[0];
            }
        }

        private ObservableCollection<MpContextMenuItemViewModel> _translateLanguageMenuItems = null;
        public ObservableCollection<MpContextMenuItemViewModel> TranslateLanguageMenuItems {
            get {
                if (_translateLanguageMenuItems == null) {
                    _translateLanguageMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
                    foreach (var languageName in MpLanguageTranslator.Instance.LanguageList) {
                        _translateLanguageMenuItems.Add(new MpContextMenuItemViewModel(languageName, TranslateSelectedClipTextAsyncCommand, languageName, false));
                    }
                }
                return _translateLanguageMenuItems;
            }
        }
        #endregion

        #region Controls
        public Grid ClipTrayContainerGrid;

        public VirtualizingStackPanel ClipTrayVirtualizingStackPanel;

        public AdornerLayer ClipTrayAdornerLayer;
        #endregion

        #region Layout
        public Point DropTopPoint { get; set; }
        public Point DropBottomPoint { get; set; }
        #endregion

        #region Selection 
        //public BitmapSource SelectedClipTilesBmp {
        //    get {
        //        var bmpList = new List<BitmapSource>();
        //        foreach (var sctvm in SelectedClipTiles) {
        //            bmpList.Add(sctvm.CopyItemBmp);
        //        }
        //        return MpHelpers.Instance.CombineBitmap(bmpList, false);
        //    }
        //}

        public string SelectedClipTilesCsv {
            get {
                var sb = new StringBuilder();
                foreach (var sctvm in SelectedClipTiles) {
                    if ((sctvm.CopyItemType != MpCopyItemType.Composite &&
                         sctvm.CopyItemType != MpCopyItemType.RichText) &&
                        MpHelpers.Instance.IsStringCsv(sctvm.CopyItem.ItemCsv)) {
                        sb.Append(sctvm.CopyItem.ItemCsv + ",");
                        continue;
                    } else {
                        sb.Append(sctvm.RichTextBoxViewModelCollection.SubSelectedClipTilesCsv + ",");
                    }
                }
                return sb.ToString();
            }
        }

        public string[] SelectedClipTilesFileList {
            get {
                var fl = new List<string>();
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.CopyItemType != MpCopyItemType.Composite ||
                       sctvm.CopyItemType != MpCopyItemType.RichText) {
                        foreach (string f in sctvm.CopyItemFileDropList) {
                            fl.Add(f);
                        }
                        continue;
                    } else {
                        foreach (var srtbvm in sctvm.RichTextBoxViewModelCollection.SubSelectedClipItems) {
                            foreach (string f in srtbvm.CopyItemFileDropList) {
                                fl.Add(f);
                            }
                        }
                    }
                                   
                }
                return fl.ToArray();
            }
        }

        public string SelectedClipTilesMergedPlainText {
            get {
                var sb = new StringBuilder();
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.CopyItemType == MpCopyItemType.Composite ||
                       sctvm.CopyItemType == MpCopyItemType.RichText) {
                        sb.Append(sctvm.RichTextBoxViewModelCollection.SubSelectedClipTilesMergedPlainText + Environment.NewLine);
                    } else {
                        sb.Append(sctvm.CopyItemPlainText + Environment.NewLine);
                    }
                }
                return sb.ToString().Trim('\r', '\n');
            }
        }

        public string[] SelectedClipTilesMergedPlainTextFileList {
            get {
                string mergedPlainTextFilePath = MpHelpers.Instance.WriteTextToFile(
                    Path.GetTempFileName(), SelectedClipTilesMergedPlainText, true);

                return new string[] { mergedPlainTextFilePath };
            }
        }

        public string SelectedClipTilesMergedRtf {
            get {
                MpEventEnabledFlowDocument fd = string.Empty.ToRichText().ToFlowDocument();
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.CopyItemType == MpCopyItemType.Composite ||
                       sctvm.CopyItemType == MpCopyItemType.RichText) {
                        fd = MpHelpers.Instance.CombineFlowDocuments(
                            sctvm.RichTextBoxViewModelCollection.SubSelectedClipTilesMergedRtf.ToFlowDocument(),
                            fd);
                    } else {
                        fd = MpHelpers.Instance.CombineFlowDocuments(sctvm.CopyItemRichText.ToFlowDocument(), fd);
                    }
                }
                return fd.ToRichText();
            }
        }


        public string[] SelectedClipTilesMergedRtfFileList {
            get {
                string mergedRichTextFilePath = MpHelpers.Instance.WriteTextToFile(
                    Path.GetTempFileName(), SelectedClipTilesMergedRtf, true);

                return new string[] { mergedRichTextFilePath };
            }
        }
        #endregion

        #region State
        public bool IsHotKeyPasting { get; set; } = false;

        public bool IsAnyContextMenuOpened {
            get {
                return this.Any(x => x.IsContextMenuOpened || x.IsAnySubContextMenuOpened);
            }
        }

        private BitmapSource _filterByAppIcon = null;
        public BitmapSource FilterByAppIcon {
            get {
                if(_filterByAppIcon == null) {
                    return new BitmapImage();
                }
                return _filterByAppIcon;
            }
            set {
                if(_filterByAppIcon != value) {
                    _filterByAppIcon = value;
                    OnPropertyChanged(nameof(FilterByAppIcon));
                }
            }
        }

        public bool WasItemAdded { get; set; } = false;

        public bool IsTrayDropping { get; set; } = false;

        public bool IsAnyClipOrSubItemDragging {
            get {
                foreach(var ctvm in VisibileClipTiles) {
                    if(ctvm.IsClipDragging || ctvm.IsAnySubItemDragging) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool IsAnyClipDropping {
            get {
                foreach (var ctvm in VisibileClipTiles) {
                    if (ctvm.IsClipDropping) {
                        return true;
                    }
                }
                return false;
            }
        }

        public Point StartDragPoint;

        public bool IsAnyTileExpanded {
            get {
                foreach (var ctvm in this) {
                    if (ctvm.IsExpanded) {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool _isFilteringByApp = false;
        public bool IsFilteringByApp {
            get {
                return _isFilteringByApp;
            }
            set {
                if (_isFilteringByApp != value) {
                    _isFilteringByApp = value;
                    OnPropertyChanged(nameof(IsFilteringByApp));
                }
            }
        }

        private bool _isMouseDown = false;
        public bool IsMouseDown {
            get {
                return _isMouseDown;
            }
            set {
                if (_isMouseDown != value) {
                    _isMouseDown = value;
                    OnPropertyChanged(nameof(IsMouseDown));
                }
            }
        }

        //private bool _isLoading = true;
        //public bool IsLoading {
        //    get {
        //        return _isLoading;
        //    }
        //    set {
        //        if (_isLoading != value) {
        //            _isLoading = value;
        //            OnPropertyChanged(nameof(IsLoading));
        //        }
        //    }
        //}

        //public bool IsItemLoading {
        //    get {
        //        foreach(var ctvm in this) {
        //            if(ctvm.IsLoading) {
        //                return true;
        //            }
        //        }
        //        return false;
        //    }
        //}

        public bool IsAnyHovering {
            get {
                return VisibileClipTiles.Where(x => x.IsHovering).ToList().Count > 0;
            }
        }

        private bool _isScrolling = false;
        public bool IsScrolling {
            get {
                return _isScrolling;
            }
            set {
                if (_isScrolling != value) {
                    _isScrolling = value;
                    OnPropertyChanged(nameof(IsScrolling));
                }
            }
        }

        public bool IsEditingClipTitle {
            get {
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.IsEditingTitle) {
                        return true;
                    }
                    foreach (var subctvm in sctvm.RichTextBoxViewModelCollection) {
                        if (subctvm.IsEditingSubTitle) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool IsEditingClipTile {
            get {
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.IsEditingTile) {
                        return true;
                    }
                    foreach (var subctvm in sctvm.RichTextBoxViewModelCollection) {
                        if (subctvm.IsEditingContent) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public bool IsPastingTemplate {
            get {
                foreach (var sctvm in SelectedClipTiles) {
                    if (sctvm.IsPastingTemplate) {
                        return true;
                    }
                    foreach (var subctvm in sctvm.RichTextBoxViewModelCollection) {
                        if (subctvm.IsPastingTemplate) {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        #endregion

        #region Visibility

        public Visibility EmptyListMessageVisibility {
            get {
                if (VisibileClipTiles.Count == 0) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private Visibility _clipTrayVisibility = Visibility.Visible;
        public Visibility ClipTrayVisibility {
            get {
                return _clipTrayVisibility;
            }
            set {
                if(_clipTrayVisibility != value) {
                    _clipTrayVisibility = value;
                    OnPropertyChanged(nameof(ClipTrayVisibility));
                }
            }
        }

        private Visibility _mergeClipsCommandVisibility = Visibility.Collapsed;
        public Visibility MergeClipsCommandVisibility {
            get {
                return _mergeClipsCommandVisibility;
            }
            set {
                if (_mergeClipsCommandVisibility != value) {
                    _mergeClipsCommandVisibility = value;
                    OnPropertyChanged(nameof(MergeClipsCommandVisibility));
                }
            }
        }

        #endregion

        #endregion

        #region Events
        public event EventHandler ItemsVisibilityChanged;
        public virtual void OnItemsVisibilityChanged() => ItemsVisibilityChanged?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Public Methods

        public MpClipTrayViewModel() : base() {
            this.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(EmptyListMessageVisibility));
                OnPropertyChanged(nameof(ClipTrayVisibility));                
            };
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsFilteringByApp):
                        foreach (var ctvm in VisibileClipTiles) {
                            ctvm.OnPropertyChanged(nameof(ctvm.AppIcon));
                        }
                        break;
                }
            };
            var allItems = MpCopyItem.GetAllCopyItems(out int _totalEntryCount);
            foreach (var ci in allItems) {
                if (ci.IsSubCompositeItem) {
                    continue;
                }
                Add(new MpClipTileViewModel(ci));
            }

            //int emptyItemsToAdd = Properties.Settings.Default.MaxRecentClipItems - this.Count;
            //while (emptyItemsToAdd > 0) {
            //    var ectvm = new MpClipTileViewModel(true) { TileVisibility = Visibility.Collapsed };
            //    ectvm.RichTextBoxViewModelCollection.Add(new MpRtbListBoxItemRichTextBoxViewModel(ectvm,ectvm.CopyItem));
            //    base.Add(ectvm);
            //    emptyItemsToAdd--;
            //}
        }

        public void ClipTray_Loaded(object sender, RoutedEventArgs e) {
            ListBox = (ListBox)sender;
            IsHorizontal = true;

            ScrollViewer = ListBox.GetDescendantOfType<ScrollViewer>();
            ScrollViewer.Margin = new Thickness(5, 0, 5, 0);
            ClipTrayContainerGrid = ListBox.GetVisualAncestor<Grid>();

            ClipTrayAdornerLayer = AdornerLayer.GetAdornerLayer(ListBox);
            ClipTrayAdornerLayer.Add(new MpClipTrayAdorner(ListBox));

            #region Drag/Drop            
            ListBox.DragLeave += (s2, e1) => {
                IsTrayDropping = false;
                ClipTrayAdornerLayer.Update();
            };
            ListBox.DragOver += (s2, e1) => {
                IsTrayDropping = false;
                e1.Effects = DragDropEffects.None;
                ClipTrayAdornerLayer.Update();
                if(IsAnyClipDropping) {
                    return;
                }
                AutoScrollByMouse();
                if (e1.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    int dropIdx = GetDropIdx(MpHelpers.Instance.GetMousePosition(ListBox));
                    if (dropIdx >= 0/* && (dropIdx >= this.Count || (dropIdx < this.Count && !this[dropIdx].IsClipOrAnySubItemDragging))*/) {
                        DropTopPoint = this.GetAdornerPoints(dropIdx)[0];
                        DropBottomPoint = this.GetAdornerPoints(dropIdx)[1];
                        IsTrayDropping = true;
                        e1.Effects = DragDropEffects.Move;
                    }
                }
                ClipTrayAdornerLayer.Update();
            };

            ListBox.Drop += (s3, e2) => {
                if(!IsTrayDropping) {
                    return;
                }
                bool wasDropped = false;
                var dctvml = new List<MpClipTileViewModel>();
                if (e2.Data.GetDataPresent(Properties.Settings.Default.ClipTileDragDropFormatName)) {
                    dctvml = (List<MpClipTileViewModel>)e2.Data.GetData(Properties.Settings.Default.ClipTileDragDropFormatName);
                    dctvml = dctvml.OrderByDescending(x => x.SortOrderIdx).ToList();
                    int dropIdx = GetDropIdx(MpHelpers.Instance.GetMousePosition(ListBox));
                    if (dropIdx >= 0 && (dropIdx >= this.Count || (dropIdx < this.Count && !this[dropIdx].IsClipDragging))) {
                        if (dropIdx < this.Count && this[dropIdx].IsClipDragging) {
                            //ignore dropping dragged tile onto itself
                            e2.Effects = DragDropEffects.None;
                            e2.Handled = true;
                            IsTrayDropping = false;
                            ClipTrayAdornerLayer.Update();
                            return;
                        }
                        /* 
                         On tray drop: 
                         1. if all rtbvm of sctvm are selected or rtbvm count is 0, do move to dropidx, 
                         2. if partial selection, remove from parent and make new composite in merge then insert at dropidx. 
                         3.Order sctvml by asc hctvm.selecttime then subsort composites by asc rtbvm subselectdatetime
                        */
                        foreach (var dctvm in dctvml) {
                            int dragCtvmIdx = this.IndexOf(dctvm); 
                            bool wasEmptySelection = dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0;
                            if (wasEmptySelection) {
                                dctvm.RichTextBoxViewModelCollection.SubSelectAll();
                            }
                            if (dctvm.RichTextBoxViewModelCollection.Count == 0 ||
                                wasEmptySelection ||
                                dctvm.RichTextBoxViewModelCollection.Count == dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count) {
                                //1. if all rtbvm of sctvm are selected or rtbvm count is 0, do move to dropidx
                                if (dragCtvmIdx < dropIdx) {
                                    this.Move(dragCtvmIdx, dropIdx - 1);
                                } else {
                                    this.Move(dragCtvmIdx, dropIdx);
                                }                                
                                wasDropped = true;
                            } else {
                                //2. if partial selection, remove from parent and make new
                                //   composite in merge then insert at dropidx.

                                //var ncci = dctvm.RichTextBoxViewModelCollection.SubSelectedRtbvmList[0].CopyItem;
                                //var compositeItem = new MpCopyItem(MpCopyItemType.Composite, ncci.Title, null, ncci.ItemColor.Color, IntPtr.Zero, ncci.App);
                                //compositeItem.WriteToDatabase();
                                //compositeItem = await MpCopyItem.MergeAsync(ncci, compositeItem);

                                var drtbvm = dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.OrderBy(x=>x.CompositeSortOrderIdx).ToList()[0];
                                dctvm.RichTextBoxViewModelCollection.Remove(drtbvm,true);
                                var nctvm = new MpClipTileViewModel(drtbvm.CopyItem);
                                foreach (var ssrtbvm in dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.OrderBy(x => x.CompositeSortOrderIdx).ToList()) {
                                    nctvm.MergeClip(new List<MpCopyItem>() { ssrtbvm.CopyItem });
                                }

                                this.Add(nctvm, dropIdx);
                                //var nctvm = new MpClipTileViewModel(compositeItem);
                                //await nctvm.MergeClipAsync(ossrtbvml);
                                wasDropped = true;
                            }
                        }                        
                    }
                }
                if (wasDropped) {
                    foreach (var dctvm in dctvml) {
                        dctvm.IsClipDragging = false;
                        foreach (var rtbvm in dctvm.RichTextBoxViewModelCollection) {
                            rtbvm.IsSubDragging = false;
                        }
                    }
                    var dropCtvml = this.Where(x => x.IsClipDropping).ToList();
                    if(dropCtvml != null && dropCtvml.Count > 0) {
                        ClearClipSelection();
                        dropCtvml[0].IsSelected = true;
                    }
                    e2.Effects = DragDropEffects.Move;
                    //Refresh();
                } else {
                    e2.Effects = DragDropEffects.None;                    
                }
                e2.Handled = true;
                IsTrayDropping = false;
                ClipTrayAdornerLayer.Update();
            };
            #endregion

            ListBox.SelectionChanged += (s, e8) => {
                MergeClipsCommandVisibility = MergeSelectedClipsCommand.CanExecute(null) ? Visibility.Visible : Visibility.Collapsed;

                MainWindowViewModel.TagTrayViewModel.UpdateTagAssociation();

                if (PrimarySelectedClipTile != null) {
                    PrimarySelectedClipTile.OnPropertyChanged(nameof(PrimarySelectedClipTile.TileBorderBrush));
                }

                //multi-select label stuff (disabled)
                //foreach (var sctvm in SelectedClipTiles) {
                //    sctvm.OnPropertyChanged(nameof(sctvm.MultiSelectOrderMarkerVisibility));
                //    sctvm.OnPropertyChanged(nameof(sctvm.MultiSelectedOrderIdxDisplayValue));
                //    sctvm.OnPropertyChanged(nameof(sctvm.ClipTileTitleAppIconImageVisibility));
                //    sctvm.OnPropertyChanged(nameof(sctvm.CopyItemAppIconHighlightBorder));
                //    sctvm.OnPropertyChanged(nameof(sctvm.AppIconHighlightBorderVisibility));
                //    foreach(var srtbvm in sctvm.RichTextBoxViewModelCollection.SubSelectedClipItems) {
                //        srtbvm.OnPropertyChanged(nameof(srtbvm.MultiSelectOrderMarkerVisibility));
                //        srtbvm.OnPropertyChanged(nameof(srtbvm.AppIconImageVisibility));
                //        srtbvm.OnPropertyChanged(nameof(srtbvm.MultiSelectedOrderIdxDisplayValue));
                //    }
                //}
            };

            ListBox.MouseLeftButtonDown += (s, e9) => {
                if (!IsAnyTileExpanded) {
                    return;
                }
                var selectedClipTilesHoveringOnMouseDown = SelectedClipTiles.Where(x => x.IsHovering).ToList();
                if(selectedClipTilesHoveringOnMouseDown.Count == 0 && 
                   !MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                    ClearClipEditing();
                }
            };
            
            MpClipboardManager.Instance.Init();
            MpClipboardManager.Instance.ClipboardChanged += (s, e53) => AddItemFromClipboard();

            if (Properties.Settings.Default.IsInitialLoad) {
                InitIntroItems();
            }
        }

        private int GetDropIdx(Point mp) {
            double mdx = mp.X;
            double minDist = double.MaxValue;
            int dropIdx = -1;
            foreach (var vctvm in VisibileClipTiles) {
                if (vctvm.ClipBorder == null) {
                    //not sure why this happens but during a composite->tray
                    //drop the new tile is already added but not loaded
                    continue;
                }
                double lbilx = vctvm.TileRect.Left;
                double lbirx = vctvm.TileRect.Right;
                double lDist = Math.Abs(mdx - lbilx);
                double rDist = Math.Abs(mdx - lbirx);
                double dist = Math.Min(lDist, rDist);
                if (dist < minDist) {
                    minDist = dist;
                    if(minDist == lDist) {
                        dropIdx = VisibileClipTiles.IndexOf(vctvm);
                    } else {
                        dropIdx = VisibileClipTiles.IndexOf(vctvm) + 1;
                    }
                    
                }
            }
            //var overRect = this[dropIdx].TileRect;
            //double overMidX = overRect.Left + (overRect.Right / 2);
            //if (mp.X > overMidX) {
            //    dropIdx++;
            //}
            return dropIdx;
        }

        public void ClipTrayVirtualizingStackPanel_Loaded(object sender, RoutedEventArgs args) {
            ClipTrayVirtualizingStackPanel = (VirtualizingStackPanel)sender;
        }

        public void AutoScrollByMouse() {
            double minScrollDist = 20;
            double autoScrollOffset = 15;
            var mp = MpHelpers.Instance.GetMousePosition(ListBox);
            double leftDiff = MpHelpers.Instance.DistanceBetweenValues(mp.X, 0);
            double rightDiff = MpHelpers.Instance.DistanceBetweenValues(mp.X, MainWindowViewModel.ClipTrayWidth);
            if (leftDiff < minScrollDist) {
                autoScrollOffset += Math.Pow(leftDiff, 2);
                this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset - autoScrollOffset);
            } else if (rightDiff < minScrollDist) {
                autoScrollOffset += Math.Pow(rightDiff, 2);
                this.ScrollViewer.ScrollToHorizontalOffset(this.ScrollViewer.HorizontalOffset + autoScrollOffset);
            }
        }

        public void IsolateClipTile(MpClipTileViewModel tileToIsolate) {
            if(!VisibileClipTiles.Contains(tileToIsolate)) {
                Console.WriteLine("Warning tile to isolate was hidden and is now being shown");
                tileToIsolate.TileVisibility = Visibility.Visible;
            }
            //var subSelectedItems = tileToIsolate.RichTextBoxViewModelCollection.SubSelectedClipItems;
            //ClearClipSelection(false);
            _hiddenTiles = VisibileClipTiles.ToList();
            _hiddenTiles.Remove(tileToIsolate);
            foreach (var ctvm in _hiddenTiles) {
                ctvm.IsSelected = false;
                //ctvm.IsPrimarySelected = false;
                ctvm.TileVisibility = Visibility.Collapsed;
            }
            if(!tileToIsolate.IsSelected) {
                tileToIsolate.IsSelected = true;
                //foreach(var rtbvm in tileToIsolate.RichTextBoxViewModelCollection) {
                //    if(subSelectedItems.Contains(rtbvm)) {
                //        rtbvm.IsSubSelected = true;
                //    }
                //}
            }
            ClipTrayVirtualizingStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
        }
        public void RestoreVisibleTiles() {
            //var _hiddenTileCanvasList = new List<FrameworkElement>();
            foreach (var ctvm in _hiddenTiles) {
                //_hiddenTileCanvasList.Add(ctvm.ClipBorder);
                ctvm.IsSelected = false;
                //ctvm.IsPrimarySelected = false;
                ctvm.TileVisibility = Visibility.Visible;
            }

            ClipTrayVirtualizingStackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            //Refresh();
            //if (_hiddenTileCanvasList.Count > 0) {
            //    MpHelpers.Instance.AnimateVisibilityChange(
            //        _hiddenTileCanvasList,
            //        Visibility.Visible,
            //        (s, e) => Refresh(),
            //        animMs,
            //        animMs);
            //}
        }

        public void Resize(
            MpClipTileViewModel tileToResize,
            double deltaWidth,
            double deltaHeight,
            double deltaEditToolbarTop) {
            MainWindowViewModel.ClipTrayHeight += deltaHeight;
            ListBox.Height = MainWindowViewModel.ClipTrayHeight;
            ListBox.UpdateLayout();

            tileToResize.Resize(
                deltaWidth,
                deltaHeight, 
                deltaEditToolbarTop);
        }

        public void HideVisibleTiles(double ms = 1000) {
            double delay = 0;
            double curDelay = 0;
            foreach(var ctvm in VisibileClipTiles) {
                _hiddenTiles.Add(ctvm);
                ctvm.FadeOut(Visibility.Hidden,curDelay,ms);
                curDelay += delay;
            }
        }

        public void ShowVisibleTiles(double ms = 1000) {
            double delay = 0;
            double curDelay = 0;
            foreach (var ctvm in _hiddenTiles) {
                ctvm.FadeIn(delay,ms);
                curDelay += delay;
            }
            _hiddenTiles.Clear();
        }

        public MpCopyItemType GetSelectedClipsType() {
            //returns none if all clips aren't the same type
            if(SelectedClipTiles.Count == 0) {
                return MpCopyItemType.None;
            }
            MpCopyItemType firstType = SelectedClipTiles[0].CopyItemType;
            foreach(var sctvm in SelectedClipTiles) {
                if(sctvm.CopyItemType != firstType) {
                    return MpCopyItemType.None;
                }
            }
            return firstType;
        }

        public void ClearClipEditing() {
            foreach (var ctvm in this) {
                ctvm.IsEditingTitle = false;
                if (ctvm.IsEditingTile) {
                    ctvm.IsEditingTile = false;
                }
                ctvm.IsEditingTemplate = false;
                if (ctvm.IsPastingTemplate) {
                    MainWindowViewModel.ShrinkClipTile(ctvm);
                    ctvm.IsPastingTemplate = false;
                    
                }
                foreach (var rtbvm in ctvm.RichTextBoxViewModelCollection) {
                    //rtbvm.IsEditingContent = false;
                    rtbvm.IsEditingSubTitle = false;
                    rtbvm.IsPastingTemplate = false;
                }
                if (ctvm.DetectedImageObjectCollectionViewModel != null) {
                    foreach (var diovm in ctvm.DetectedImageObjectCollectionViewModel) {
                        diovm.IsNameReadOnly = true;
                    }
                }
            }
            MainWindowViewModel.OnPropertyChanged(nameof(MainWindowViewModel.AppModeButtonGridWidth));
            MainWindowViewModel.AppModeViewModel.OnPropertyChanged(nameof(MainWindowViewModel.AppModeViewModel.AppModeColumnVisibility));
        }

        public void ClearClipSelection(bool clearEditing = true) {
            if(clearEditing) {
                ClearClipEditing();
            }
            foreach (var ctvm in this) {    
                //ctvm.IsPastingTemplate = false;
                ctvm.IsEditingTemplate = false;
                ctvm.IsHovering = false;                
                ctvm.IsSelected = false;
                ctvm.LastSelectedDateTime = DateTime.MaxValue;
                //ctvm.IsPrimarySelected = false;
                ctvm.RichTextBoxViewModelCollection.ClearSubSelection();
                ctvm.FileListCollectionViewModel.ClearSubSelection();
            }
            ///OnPropertyChanged(nameof(SelectedClipTiles));
        }

        public void ResetClipSelection(bool clearEditing = true) {
            ClearClipSelection(clearEditing);

            if (VisibileClipTiles.Count > 0) {
                VisibileClipTiles[0].IsSelected = true;
                if(!MainWindowViewModel.SearchBoxViewModel.IsTextBoxFocused) {
                    if(ListBox != null) {
                        //ListBox.ScrollIntoView(VisibileClipTiles[0]);
                        //ScrollViewer.ScrollToHorizontalOffset(0);
                        //ScrollViewer.InvalidateArrange();
                        //ScrollViewer.InvalidateScrollInfo();
                        //ListBox.AnimatedScrollViewer.ScrollToHorizontalOffset(0);
                        ((ListBoxItem)ListBox.ItemContainerGenerator.ContainerFromItem(VisibileClipTiles[0]))?.Focus();
                    }
                }
            }
           // OnPropertyChanged(nameof(SelectedClipTiles));
        }

        public void RefreshAllCommands() {
            foreach(MpClipTileViewModel ctvm in this) {
                ctvm.RefreshAsyncCommands();
            }
        }
        public async Task AddItemFromClipboard() {
            var sw = new Stopwatch();
            sw.Start();          

            var ncisw = new Stopwatch();
            ncisw.Start();
            var newCopyItem = await MpCopyItem.CreateFromClipboardAsync(MpClipboardManager.Instance.LastWindowWatcher.LastHandle);
            ncisw.Stop();
            Console.WriteLine("CreateFromClipboardAsync: " + ncisw.ElapsedMilliseconds + "ms");

            if (newCopyItem == null) {
                //this occurs if the copy item is not a known format
                return;
            }
           
            if (MainWindowViewModel.AppModeViewModel.IsInAppendMode && SelectedClipTiles.Count > 0) {
                //when in append mode just append the new items text to selecteditem
                var primarySelectedClipTile = PrimarySelectedClipTile;
                if(SelectedClipTiles.Count > 1) {
                    ClearClipSelection();
                    primarySelectedClipTile.IsSelected = true;
                }
                primarySelectedClipTile.MergeClip(new List<MpCopyItem>() { newCopyItem });
                
                if (Properties.Settings.Default.NotificationShowAppendBufferToast) {
                    MpStandardBalloonViewModel.ShowBalloon(
                    "Append Buffer",
                    SelectedClipTiles[0].CopyItemPlainText,
                    Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                }
                if (Properties.Settings.Default.NotificationDoCopySound) {
                    MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                }
                return;
            }
            if (newCopyItem.CopyItemId > 0) {
                //item is a duplicate
                var existingClipTile = GetClipTileByCopyItemId(newCopyItem.CopyItemId);
                if (existingClipTile != null) {
                    Console.WriteLine("Ignoring duplicate copy item");
                    existingClipTile.CopyCount++;
                    existingClipTile.CopyDateTime = DateTime.Now;
                    this.Move(this.IndexOf(existingClipTile), 0);
                    ClearClipSelection();
                    existingClipTile.IsSelected = true;
                }
            } else {
                var nctvm = new MpClipTileViewModel(newCopyItem);

                await this.AddAsync(nctvm);

                if (Properties.Settings.Default.NotificationDoCopySound) {
                    MpSoundPlayerGroupCollectionViewModel.Instance.PlayCopySoundCommand.Execute(null);
                }
                if (IsTrialExpired) {
                    MpStandardBalloonViewModel.ShowBalloon(
                    "Trial Expired",
                    "Please update your membership to use Monkey Paste",
                    Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                }
                //MainWindowViewModel.TagTrayViewModel.AddClipToSudoTags(nctvm);
                //ListBox.Items.Refresh();
            }
            ResetClipSelection();

            sw.Stop();
            Console.WriteLine("Time to create new copyitem: " + sw.ElapsedMilliseconds + " ms");
        }
               

        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();
            ListBox?.Items.Refresh();
            sw.Stop();
            Console.WriteLine("ClipTray Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public void Add(MpClipTileViewModel ctvm, int forceIdx = 0) {
            if(MainWindowViewModel != null && MainWindowViewModel.TagTrayViewModel != null) {
                if (MainWindowViewModel.TagTrayViewModel.SelectedTagTile != MainWindowViewModel.TagTrayViewModel.GetRecentTagTileViewModel()) {
                    //always add new clips to recent tag
                    MainWindowViewModel.TagTrayViewModel.GetRecentTagTileViewModel().IsSelected = true;
                }
            }

            //int nextVisibleIdx = 0;
            //if (VisibileClipTiles.Count < Properties.Settings.Default.MaxRecentClipItems) {
            //    //occurs when there are not enough items to fill all recent tiles
            //    //and the tile after all visible needs to be 'unstubbed'
            //    if(VisibileClipTiles.Count > 0) {
            //        nextVisibleIdx = this.IndexOf(VisibileClipTiles[VisibileClipTiles.Count - 1]) + 1;
            //    }
            //    this[nextVisibleIdx].TileVisibility = Visibility.Visible;
            //} else {
            //    //otherwise reuse tail of recent to virtualize item adding
            //    nextVisibleIdx = this.IndexOf(VisibileClipTiles[VisibileClipTiles.Count - 1]);
            //}

            if (forceIdx >= 0 && forceIdx < this.Count) {
                base.Insert(forceIdx, ctvm);
            } else {
                //var ctvm = this[nextVisibleIdx];
                //for (int i = 1; i < ctvm.RichTextBoxViewModelCollection.Count; i++) {
                //    ctvm.RichTextBoxViewModelCollection.RemoveAt(i);
                //}
                //ctvm.CopyItem = ci;
                //if(ctvm.IsTextItem) {
                //    ctvm.RichTextBoxViewModelCollection[0].CopyItem = ci;
                //}
                base.Add(ctvm);
            }
            

            // NOTE removing this refresh will confuse the tiles flowdocument owner or something
            // it probably is something I can fix to avoid the refresh but not sure how
            if (MainWindowViewModel == null || !MainWindowViewModel.IsMainWindowLocked) {
                //Refresh();
            } else {
               // ctvm.IsSelected = false;
            }
            //not calling this doesn't associate the items clipborder to this listbox I don't know why
            if (MpMainWindowViewModel.IsMainWindowOpen) {
                Refresh();
            } else {
                WasItemAdded = true;
            }
        }
        
        public async Task AddAsync(MpClipTileViewModel ctvm, int forceIdx = 0, DispatcherPriority priority = DispatcherPriority.Background) {
            IsBusy = true;
            await Application.Current.Dispatcher.BeginInvoke(priority,
                (Action)(()=> { 
                    this.Add(ctvm, forceIdx); 
                }));
            IsBusy = false;
        }

        public void Remove(MpClipTileViewModel clipTileToRemove, bool isMerge = false) {
            base.Remove(clipTileToRemove);
            
            if (clipTileToRemove.CopyItem == null) {
                //occurs when duplicate detected on background thread
                return;
            } 
            
            if(isMerge) {
                clipTileToRemove.IsClipDragging = false;
                foreach (var rtbvm in clipTileToRemove.RichTextBoxViewModelCollection) {
                    rtbvm.IsSubDragging = false;
                }
            }
            else {                
                clipTileToRemove.Dispose();
                clipTileToRemove = null;
            }            
        }

        public async Task RemoveAsync(MpClipTileViewModel ctvm, bool isMerge, DispatcherPriority priority = DispatcherPriority.Background) {
            IsBusy = true;
            await Application.Current.Dispatcher.BeginInvoke(priority,
                (Action)(() => {
                    this.Remove(ctvm, isMerge);
                }));
            IsBusy = false;
        }

        public async Task<IDataObject> GetDataObjectFromSelectedClips(bool isDragDrop = false) {
            IDataObject d = new DataObject();

            var sb = new StringBuilder();
            sb.Append(string.Empty.ToRichText());
            foreach (var sctvm in SelectedClipTiles) {
                if (sctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0) {
                    sctvm.RichTextBoxViewModelCollection.SubSelectAll();
                }
                sb.Append(MpHelpers.Instance.CombineRichText(await sctvm.GetPastableRichText(), sb.ToString()));
            }
            string rtf = sb.ToString();

            if (!string.IsNullOrEmpty(rtf)) {
                d.SetData(DataFormats.Rtf, rtf);
                d.SetData(DataFormats.Text, rtf.ToPlainText());
            }

            //only when pasting into explorer or notepad must have file drop
            if (MpHelpers.Instance.IsProcessNeedFileDrop(MpRunningApplicationManager.Instance.ActiveProcessPath) &&
                isDragDrop) {
                var sctfl = SelectedClipTilesFileList;
                if (sctfl != null) {
                    if (MpHelpers.Instance.IsProcessLikeNotepad(MpRunningApplicationManager.Instance.ActiveProcessPath)) {
                        d.SetData(DataFormats.FileDrop, SelectedClipTilesMergedPlainTextFileList);
                    } else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                        d.SetData(DataFormats.FileDrop, SelectedClipTilesMergedRtfFileList);
                    } else {
                        d.SetData(DataFormats.FileDrop, sctfl);
                    }                    
                }    
            } 
            if(SelectedClipTiles.Count == 1 && SelectedClipTiles[0].CopyItemBmp != null) {
                d.SetData(DataFormats.Bitmap, SelectedClipTiles[0].CopyItemBmp);
            }

            var sctcsv = SelectedClipTilesCsv;
            if(sctcsv != null) {
                d.SetData(DataFormats.CommaSeparatedValue, sctcsv);
            }


            if (isDragDrop && SelectedClipTiles != null && SelectedClipTiles.Count > 0) {
                foreach (var dctvm in SelectedClipTiles) {
                    if (dctvm.RichTextBoxViewModelCollection.Count == 0 ||
                        dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == dctvm.RichTextBoxViewModelCollection.Count ||
                        dctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 0) {
                        dctvm.IsClipDragging = true;
                    }
                }
                d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, SelectedClipTiles.ToList());
            }
            return d;
            //awaited in MainWindowViewModel.HideWindow
        }

        public void PerformPaste(IDataObject pasteDataObject, bool fromHotKey = false) {
            //called in the oncompleted of hide command in mwvm
            if (pasteDataObject != null) {
                Console.WriteLine("Pasting " + SelectedClipTiles.Count + " items");
                IntPtr pasteToWindowHandle = IntPtr.Zero;
                if(_selectedPasteToAppPathViewModel != null) {
                    pasteToWindowHandle = MpRunningApplicationManager.Instance.SetActiveProcess(
                        _selectedPasteToAppPathViewModel.AppPath, 
                        _selectedPasteToAppPathViewModel.IsAdmin,
                        _selectedPasteToAppPathViewModel.IsSilent,
                        _selectedPasteToAppPathViewModel.Args,
                        IntPtr.Zero,
                        _selectedPasteToAppPathViewModel.WindowState);
                } else if(_selectedPasteToAppPathWindowHandle != IntPtr.Zero) {
                    var windowState = WinApi.SW_SHOWMAXIMIZED;
                    if (MpRunningApplicationManager.Instance.LastWindowStateHandleDictionary.ContainsKey(_selectedPasteToAppPathWindowHandle)) {
                        windowState = MpHelpers.Instance.GetShowWindowValue(MpRunningApplicationManager.Instance.LastWindowStateHandleDictionary[_selectedPasteToAppPathWindowHandle]);
                    }
                    WinApi.ShowWindowAsync(_selectedPasteToAppPathWindowHandle, windowState);
                    pasteToWindowHandle = _selectedPasteToAppPathWindowHandle;
                } else {
                    pasteToWindowHandle = MpClipboardManager.Instance.LastWindowWatcher.LastHandle;
                }

                MpClipboardManager.Instance.PasteDataObject(pasteDataObject, pasteToWindowHandle);

                if(_selectedPasteToAppPathViewModel != null && _selectedPasteToAppPathViewModel.PressEnter) {
                    System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                }
                
                if(!fromHotKey) {
                    //resort list so pasted items are in front and paste is tracked
                    for (int i = SelectedClipTiles.Count - 1; i >= 0; i--) {
                        var sctvm = SelectedClipTiles[i];
                        this.Move(this.IndexOf(sctvm), 0);
                        new MpPasteHistory(sctvm.CopyItem, MpClipboardManager.Instance.LastWindowWatcher.LastHandle);
                    }
                    //Refresh();
                }
            } else if (pasteDataObject == null) {
                Console.WriteLine("MainWindow Hide Command pasteDataObject was null, ignoring paste");
            }
            _selectedPasteToAppPathViewModel = null;
            if(!fromHotKey) {
                ResetClipSelection();
            }            
        }

        public List<MpClipTileViewModel> GetClipTilesByAppId(int appId) {
            var ctvml = new List<MpClipTileViewModel>();
            foreach(MpClipTileViewModel ctvm in this) {
                if(ctvm.CopyItemAppId == appId) {
                    ctvml.Add(ctvm);
                }
            }
            return ctvml;
        }

        public MpClipTileViewModel GetClipTileByCopyItemId(int copyItemId) {
            foreach (MpClipTileViewModel ctvm in this) {
                if (ctvm.CopyItemId == copyItemId) {
                    return ctvm;
                }
            }
            return null;
        }

        public MpCopyItemType GetTargetFileType() {
            //string targetTitle = MpClipboardManager.Instance?.LastWindowWatcher.LastTitle.ToLower();
            string activeProcessPath = MpRunningApplicationManager.Instance.ActiveProcessPath;
            //when targetTitle is empty assume it is explorer and paste as filedrop
            if (string.IsNullOrEmpty(activeProcessPath)) {
                return MpCopyItemType.FileList;
            }
            foreach (var imgApp in Properties.Settings.Default.PasteAsImageDefaultProcessNameCollection) {
                if (activeProcessPath.ToLower().Contains(imgApp.ToLower())) {
                    return MpCopyItemType.Image;
                }
            }
            foreach (var fileApp in Properties.Settings.Default.PasteAsFileDropDefaultProcessNameCollection) {
                if (activeProcessPath.ToLower().Contains(fileApp.ToLower())) {
                    return MpCopyItemType.FileList;
                }
            }
            foreach (var csvApp in Properties.Settings.Default.PasteAsCsvDefaultProcessNameCollection) {
                if (activeProcessPath.ToLower().Contains(csvApp.ToLower())) {
                    return MpCopyItemType.Csv;
                }
            }
            foreach (var textApp in Properties.Settings.Default.PasteAsTextFileDefaultProcessNameCollection) {
                if (activeProcessPath.ToLower().Contains(textApp.ToLower())) {
                    return MpCopyItemType.RichText;
                }
            }
            //paste as rtf by default
            return MpCopyItemType.None;
        }

        public string ExportClipsToFile(List<MpClipTileViewModel> clipList, string rootPath) {
            string outStr = string.Empty;
            foreach (MpClipTileViewModel ctvm in clipList) {
                foreach(string f in ctvm.CopyItem.GetFileList(rootPath)) {
                    outStr += f + Environment.NewLine;
                }
            }
            return outStr;
        }

        public string ExportClipsToCsvFile(List<MpClipTileViewModel> clipList, string filePath) {
            string csvText = string.Empty;
            foreach (MpClipTileViewModel ctvm in clipList) {
                csvText += ctvm.CopyItem.ItemPlainText + ",";
            }
            using (StreamWriter of = new StreamWriter(filePath)) {
                of.Write(csvText);
                of.Close();
            }
            return filePath;
        }

        public string ExportClipsToZipFile(List<MpClipTileViewModel> clipList, string filePath) {
            using (ZipArchive zip = ZipFile.Open(filePath, ZipArchiveMode.Create)) {
                foreach (var ctvm in clipList) {
                    foreach (var p in ctvm.CopyItemFileDropList) {
                        zip.CreateEntryFromFile(p, Path.GetFileName(p));
                    }
                }
            }
            return filePath;
        }

        public int GetSelectionOrderIdxForItem(object vm) {
            //returns -1 if vm is not associated with selection
            //returns -2 if vm is a ctvm with sub-selected rtbvms
            if(vm == null) {
                return -1;
            }
            int vmIdx = -1;
            for (int i = 0; i < SelectedClipTiles.Count; i++) {
                var sctvm = SelectedClipTiles[i];
                if(sctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count <= 1 && 
                   sctvm.RichTextBoxViewModelCollection.Count <= 1) {
                    vmIdx++; 
                    if (sctvm == vm) {
                        return vmIdx;
                    }
                    continue;
                }                
                for (int j = 0; j < sctvm.RichTextBoxViewModelCollection.SubSelectedClipItems.Count; j++) {
                    var srtbvm = sctvm.RichTextBoxViewModelCollection.SubSelectedClipItems[j];
                    vmIdx++;                    
                    if(srtbvm == vm) {
                        return vmIdx;
                    }
                    if(srtbvm.HostClipTileViewModel == vm) {
                        return -2;
                    }
                }
            }
            return -1;
        }
        #endregion

        #region Private Methods
              

        private void InitIntroItems() {
            //var introItem1 = new MpCopyItem(
            //        MpCopyItemType.RichText,
            //        "Welcome to MonkeyPaste!",
            //        MpHelpers.Instance.ConvertPlainTextToRichText("Take a moment to look through the available features in the following tiles, which are always available in the 'Help' pinboard"));

            //var introItem2 = new MpCopyItem(
            //    MpCopyItemType.RichText,
            //    "One place for your clipboard",
            //    MpHelpers.Instance.ConvertPlainTextToRichText(""));
            //Properties.Settings.Default.IsInitialLoad = false;
            //Properties.Settings.Default.Save();
        }

        #endregion

        #region Commands
        private RelayCommand<object> _searchWebCommand;
        public ICommand SearchWebCommand {
            get {
                if (_searchWebCommand == null) {
                    _searchWebCommand = new RelayCommand<object>(SearchWeb);
                }
                return _searchWebCommand;
            }
        }
        private void SearchWeb(object args) {
            if (args == null || args.GetType() != typeof(string)) {
                return;
            }
            MpHelpers.Instance.OpenUrl(args.ToString() + System.Uri.EscapeDataString(SelectedClipTilesMergedPlainText));
        }

        private RelayCommand _selectNextItemCommand;
        public ICommand SelectNextItemCommand {
            get {
                if(_selectNextItemCommand == null) {
                    _selectNextItemCommand = new RelayCommand(SelectNextItem, CanSelectNextItem);
                }
                return _selectNextItemCommand;
            }
        }
        private bool CanSelectNextItem() {
            return SelectedClipTiles.Count > 0 && 
                   SelectedClipTiles.Any(x => VisibileClipTiles.IndexOf(x) != VisibileClipTiles.Count - 1);
        }
        private void SelectNextItem() {
            var maxItem = SelectedClipTiles.Max(x => VisibileClipTiles.IndexOf(x));
            ClearClipSelection();
            VisibileClipTiles[maxItem + 1].IsSelected = true;
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
            return SelectedClipTiles.Count > 0 && SelectedClipTiles.Any(x => VisibileClipTiles.IndexOf(x) != 0);
        }
        private void SelectPreviousItem() {
            var minItem = SelectedClipTiles.Min(x => VisibileClipTiles.IndexOf(x));
            ClearClipSelection();
            VisibileClipTiles[minItem - 1].IsSelected = true;
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
            ClearClipSelection();
            foreach (var ctvm in VisibileClipTiles) {
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
            if(brush == null) {
                return;
            }
            try {
                IsBusy = true;
                await Dispatcher.CurrentDispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (Action)(() => {
                            //BitmapSource sharedSwirl = null;
                            foreach (var sctvm in SelectedClipTiles) {
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

        private AsyncCommand<object> _hotkeyPasteCommand;
        public IAsyncCommand<object> HotkeyPasteCommand {
            get {
                if (_hotkeyPasteCommand == null) {
                    _hotkeyPasteCommand = new AsyncCommand<object>(HotkeyPaste, CanHotkeyPaste);
                }
                return _hotkeyPasteCommand;
            }
        }
        private bool CanHotkeyPaste(object args) {
            return !MpMainWindowViewModel.IsMainWindowOpen;
        }
        private async Task HotkeyPaste(object args) {
            if(args == null) {
                return;
            }
            IsHotKeyPasting = true;
            int copyItemId = (int)args;
            IDataObject pasteDataObject = null;
            var pctvm = MainWindowViewModel.ClipTrayViewModel.GetClipTileByCopyItemId(copyItemId);
            if (pctvm != null) {
                ClearClipSelection();
                pctvm.IsSelected = true;
                pasteDataObject = await GetDataObjectFromSelectedClips();
            } else {
                //otherwise check if it is a composite within a tile
                MpRtbListBoxItemRichTextBoxViewModel prtbvm = null;
                foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    prtbvm = ctvm.RichTextBoxViewModelCollection.GetRtbItemByCopyItemId(copyItemId);
                    if (prtbvm != null) {
                        break;
                    }
                }
                if (prtbvm != null) {
                    ClearClipSelection();
                    prtbvm.HostClipTileViewModel.IsSelected = true;
                    prtbvm.HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
                    prtbvm.IsSubSelected = true;
                    pasteDataObject = await GetDataObjectFromSelectedClips();
                }
            }
            //In order to paste the app must hide first 
            //this triggers hidewindow to paste selected items
            if(pasteDataObject != null) {
                MainWindowViewModel.PasteDataObject(pasteDataObject);
            }
            IsHotKeyPasting = false;
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
        private bool CanPasteSelectedClips(object args) {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false && 
                !IsEditingClipTile && 
                !IsEditingClipTitle && 
                !IsPastingTemplate &&
                !IsTrialExpired;
        }
        private void PasteSelectedClips(object args) {
            if(args != null && args.GetType() == typeof(int) && (int)args > 0) {
                //when pasting to a user defined application
                _selectedPasteToAppPathWindowHandle = IntPtr.Zero;
                _selectedPasteToAppPathViewModel = MpPasteToAppPathViewModelCollection.Instance.Where(x => x.PasteToAppPathId == (int)args).ToList()[0];                
            } else if(args != null && args.GetType() == typeof(IntPtr) && (IntPtr)args != IntPtr.Zero) {
                //when pasting to a running application
                _selectedPasteToAppPathWindowHandle = (IntPtr)args;
                _selectedPasteToAppPathViewModel = null;
            }
            else {
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
            if(IsBusy || 
                MainWindowViewModel.IsLoading || 
                VisibileClipTiles.Count == 0 || 
                SelectedClipTiles.Count == 0) {
                return false;
            }
            bool canBringForward = false;
            for (int i = 0; i < SelectedClipTiles.Count && i < VisibileClipTiles.Count; i++) {
              if (!SelectedClipTiles.Contains(VisibileClipTiles[i])) {
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
                            var tempSelectedClipTiles = SelectedClipTiles;
                            ClearClipSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                this.Move(this.IndexOf(sctvm), 0);
                                sctvm.IsSelected = true;                                
                            }
                            ListBox.ScrollIntoView(SelectedClipTiles[0]);
                        }));
            }
            finally {
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
            if (IsBusy || 
                MainWindowViewModel.IsLoading || 
                VisibileClipTiles.Count == 0 || 
                SelectedClipTiles.Count == 0) {
                return false;
            }
            bool canSendBack = false;
            for (int i = 0; i < SelectedClipTiles.Count && i < VisibileClipTiles.Count; i++) {
                if (!SelectedClipTiles.Contains(VisibileClipTiles[VisibileClipTiles.Count - 1 - i])) {
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
                            var tempSelectedClipTiles = SelectedClipTiles;
                            ClearClipSelection();

                            foreach (var sctvm in tempSelectedClipTiles) {
                                this.Move(this.IndexOf(sctvm), this.Count - 1);
                                sctvm.IsSelected = true;
                            }
                            ListBox.ScrollIntoView(SelectedClipTiles[SelectedClipTiles.Count-1]);
                        }));
            } finally {
                IsBusy = false;
            }
        }

        private RelayCommand _deleteSelectedClipsCommand;
        public ICommand DeleteSelectedClipsCommand {
            get {
                if (_deleteSelectedClipsCommand == null) {
                    _deleteSelectedClipsCommand = new RelayCommand(DeleteSelectedClips,CanDeleteSelectedClips);
                }
                return _deleteSelectedClipsCommand;
            }
        }
        private bool CanDeleteSelectedClips() {
            return MpAssignShortcutModalWindowViewModel.IsOpen == false &&
                !IsEditingClipTile &&
                !IsEditingClipTitle &&
                !IsPastingTemplate;
        }
        private void DeleteSelectedClips() {
            int lastSelectedClipTileIdx = -1;
            foreach (var ct in SelectedClipTiles) {
                lastSelectedClipTileIdx = VisibileClipTiles.IndexOf(ct);
                this.Remove(ct);
            }
            ClearClipSelection();
            if (VisibileClipTiles.Count > 0) {
                if (lastSelectedClipTileIdx <= 0) {
                    VisibileClipTiles[0].IsSelected = true;
                } else if(lastSelectedClipTileIdx < VisibileClipTiles.Count) {
                    VisibileClipTiles[lastSelectedClipTileIdx].IsSelected = true;
                } else {
                    VisibileClipTiles[lastSelectedClipTileIdx - 1].IsSelected = true;
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
            if (tagToLink == null || SelectedClipTiles == null || SelectedClipTiles.Count == 0) {
                return false;
            }
            if (SelectedClipTiles.Count == 1) {
                return true;
            }
            bool isLastClipTileLinked = tagToLink.IsLinkedWithClipTile(SelectedClipTiles[0]);
            foreach (var selectedClipTile in SelectedClipTiles) {
                if (tagToLink.IsLinkedWithClipTile(selectedClipTile) != isLastClipTileLinked) {
                    return false;
                }
            }
            return true;
        }
        private void LinkTagToCopyItem(MpTagTileViewModel tagToLink) {
            bool isUnlink = tagToLink.IsLinkedWithClipTile(SelectedClipTiles[0]);
            foreach (var selectedClipTile in SelectedClipTiles) {
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
            return SelectedClipTiles.Count == 1;
        }
        private void AssignHotkey() {
            SelectedClipTiles[0].AssignHotkeyCommand.Execute(null);
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
            return SelectedClipTiles.Count != VisibileClipTiles.Count;
        }
        private void InvertSelection() {
            var sctvml = SelectedClipTiles;
            ClearClipSelection();
            foreach(var vctvm in VisibileClipTiles) {
                if(!sctvml.Contains(vctvm)) {
                    vctvm.IsSelected = true;
                }
            }
        }

        private RelayCommand _editSelectedTitleCommand;
        public ICommand EditSelectedTitleCommand {
            get {
                if (_editSelectedTitleCommand == null) {
                    _editSelectedTitleCommand = new RelayCommand(EditSelectedTitle, CanEditSelectedTitle);
                }
                return _editSelectedTitleCommand;
            }
        }
        private bool CanEditSelectedTitle() {
            if (MainWindowViewModel.IsLoading) {
                return false;
            }
            return SelectedClipTiles.Count == 1 && 
                  SelectedClipTiles[0].RichTextBoxViewModelCollection.SubSelectedClipItems.Count <= 1;
        }
        private void EditSelectedTitle() {
            SelectedClipTiles[0].EditTitleCommand.Execute(null);
        }

        private RelayCommand _editSelectedContentCommand;
        public ICommand EditSelectedContentCommand {
            get {
                if (_editSelectedContentCommand == null) {
                    _editSelectedContentCommand = new RelayCommand(EditSelectedContent, CanEditSelectedContent);
                }
                return _editSelectedContentCommand;
            }
        }
        private bool CanEditSelectedContent() {
            if (MainWindowViewModel.IsLoading) {
                return false;
            }
            return SelectedClipTiles.Count == 1 &&
                  SelectedClipTiles[0].RichTextBoxViewModelCollection.SubSelectedClipItems.Count <= 1 &&
                  SelectedClipTiles[0].IsTextItem;
        }
        private void EditSelectedContent() {
            SelectedClipTiles[0].EditContentCommand.Execute(null);
        }

        private RelayCommand _sendSelectedClipsToEmailCommand;
        public ICommand SendSelectedClipsToEmailCommand {
            get {
                if (_sendSelectedClipsToEmailCommand == null) {
                    _sendSelectedClipsToEmailCommand = new RelayCommand(SendSelectedClipsToEmail, CanSendSelectedClipsToEmail);
                }
                return _sendSelectedClipsToEmailCommand;
            }
        }
        private bool CanSendSelectedClipsToEmail() {
            return !IsEditingClipTile && SelectedClipTiles.Count > 0;
        }
        private void SendSelectedClipsToEmail() {
            MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, SelectedClipTiles[0].CopyItemTitle, SelectedClipTilesMergedPlainText));
            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }

        private RelayCommand<int> _exportSelectedClipTilesCommand;
        public ICommand ExportSelectedClipTilesCommand {
            get {
                if (_exportSelectedClipTilesCommand == null) {
                    _exportSelectedClipTilesCommand = new RelayCommand<int>(ExportSelectedClipTiles);
                }
                return _exportSelectedClipTilesCommand;
            }
        }
        private void ExportSelectedClipTiles(int exportType) {
            CommonFileDialog dlg = ((MpExportType)exportType == MpExportType.Csv || (MpExportType)exportType == MpExportType.Zip) ? new CommonSaveFileDialog() as CommonFileDialog : new CommonOpenFileDialog();
            dlg.Title = (MpExportType)exportType == MpExportType.Csv ? "Export CSV" : (MpExportType)exportType == MpExportType.Zip ? "Export Zip":"Export Items to Directory...";
            if ((MpExportType)exportType != MpExportType.Files) {
                dlg.DefaultFileName = "Mp_Exported_Data_" + MpHelpers.Instance.RemoveSpecialCharacters(DateTime.Now.ToString());
                dlg.DefaultExtension = (MpExportType)exportType == MpExportType.Csv ? "csv" : "zip";
            } else {
                ((CommonOpenFileDialog)dlg).IsFolderPicker = true;
            }
            dlg.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            dlg.AddToMostRecentlyUsedList = false;
            //dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            //dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) {
                if ((MpExportType)exportType == MpExportType.Csv) {
                    ExportClipsToCsvFile(SelectedClipTiles.ToList(), dlg.FileName);
                } else if ((MpExportType)exportType == MpExportType.Zip) {
                    ExportClipsToZipFile(SelectedClipTiles.ToList(), dlg.FileName);
                } else {
                    ExportClipsToFile(SelectedClipTiles.ToList(), dlg.FileName + @"\");
                }
            }
        }

        private AsyncCommand _mergeSelectedClipsCommand;
        public IAsyncCommand MergeSelectedClipsCommand {
            get {
                if (_mergeSelectedClipsCommand == null) {
                    _mergeSelectedClipsCommand = new AsyncCommand(MergeSelectedClips, CanMergeSelectedClips);
                }
                return _mergeSelectedClipsCommand;
            }
        }
        private bool CanMergeSelectedClips(object args) {
            //return true;
            if (SelectedClipTiles.Count <= 1) {
                return false;
            }
            bool areAllSameType = true;
            foreach (var sctvm in SelectedClipTiles) {
                if (sctvm.CopyItemType != MpCopyItemType.Composite && 
                    sctvm.CopyItemType != MpCopyItemType.RichText) {
                    areAllSameType = false;
                }
            }
            return areAllSameType;
        }
        private async Task MergeSelectedClips() {
            var sctvml = SelectedClipTiles;
            var ocil = new List<MpCopyItem>();
            foreach (var sctvm in sctvml) {
                if (sctvm == PrimarySelectedClipTile) {
                    continue;
                }
                ocil.Add(sctvm.CopyItem);
            }

            await PrimarySelectedClipTile.MergeClipAsync(ocil);
        }

        private AsyncCommand<string> _translateSelectedClipTextAsyncCommand;
        public IAsyncCommand<string> TranslateSelectedClipTextAsyncCommand {
            get {
                if (_translateSelectedClipTextAsyncCommand == null) {
                    _translateSelectedClipTextAsyncCommand = new AsyncCommand<string>(TranslateSelectedClipTextAsync, CanTranslateSelectedClipText);
                }
                return _translateSelectedClipTextAsyncCommand;
            }
        }
        private bool CanTranslateSelectedClipText(object args) {
            return SelectedClipTiles.Count == 1 && SelectedClipTiles[0].IsTextItem;
        }
        private async Task TranslateSelectedClipTextAsync(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.Translate(SelectedClipTiles[0].CopyItemPlainText, toLanguage, false);
            if (!string.IsNullOrEmpty(translatedText)) {
                SelectedClipTiles[0].CopyItemRichText = MpHelpers.Instance.ConvertPlainTextToRichText(translatedText);
            }
        }

        private RelayCommand _createQrCodeFromSelectedClipsCommand;
        public ICommand CreateQrCodeFromSelectedClipsCommand {
            get {
                if (_createQrCodeFromSelectedClipsCommand == null) {
                    _createQrCodeFromSelectedClipsCommand = new RelayCommand(CreateQrCodeFromSelectedClips, CanCreateQrCodeFromSelectedClips);
                }
                return _createQrCodeFromSelectedClipsCommand;
            }
        }
        private bool CanCreateQrCodeFromSelectedClips() {
            return (GetSelectedClipsType() == MpCopyItemType.Composite || GetSelectedClipsType() == MpCopyItemType.RichText) &&
                    SelectedClipTilesMergedPlainText.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromSelectedClips() {
            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(SelectedClipTilesMergedPlainText);
            System.Windows.Clipboard.SetImage(bmpSrc);
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
            foreach(var sctvm in SelectedClipTiles) {
                if(!string.IsNullOrEmpty(sctvm.CopyItemPlainText)) {
                    return true;
                }
            }
            return false;
        }
        private async Task SpeakSelectedClipsAsync() {
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => {
                var speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.SetOutputToDefaultAudioDevice();
                if (string.IsNullOrEmpty(Properties.Settings.Default.SpeechSynthVoiceName)) {
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

                foreach (var sctvm in SelectedClipTiles) {
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
            var tempSelectedClipTiles = SelectedClipTiles;
            ClearClipSelection();
            foreach(var sctvm in tempSelectedClipTiles) {
                var clonedCopyItem = (MpCopyItem)sctvm.CopyItem.Clone();
                clonedCopyItem.WriteToDatabase();
                var ctvm = new MpClipTileViewModel(clonedCopyItem);
                //MainWindowViewModel.TagTrayViewModel.GetHistoryTagTileViewModel().AddClip(ctvm);
                this.Add(ctvm);
                ctvm.IsSelected = true;
            }
        }
        #endregion

        
    }
    public enum MpExportType {
        None = 0,
        Files,
        Csv,
        Zip
    }
}
