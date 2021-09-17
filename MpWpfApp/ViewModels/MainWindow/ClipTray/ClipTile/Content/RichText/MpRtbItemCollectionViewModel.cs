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
using MonkeyPaste;
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    public class MpRtbItemCollectionViewModel : MpContentContainerViewModel  {
        #region Properties

        #region ViewModels

        //private MpEditTemplateToolbarViewModel _editTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel();
        //public MpEditTemplateToolbarViewModel EditTemplateToolbarViewModel {
        //    get {
        //        return _editTemplateToolbarViewModel;
        //    }
        //    set {
        //        if (_editTemplateToolbarViewModel != value) {
        //            _editTemplateToolbarViewModel = value;
        //            OnPropertyChanged(nameof(EditTemplateToolbarViewModel));
        //        }
        //    }
        //}

        //private MpPasteTemplateToolbarViewModel _pasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel();
        //public MpPasteTemplateToolbarViewModel PasteTemplateToolbarViewModel {
        //    get {
        //        return _pasteTemplateToolbarViewModel;
        //    }
        //    set {
        //        if (_pasteTemplateToolbarViewModel != value) {
        //            _pasteTemplateToolbarViewModel = value;
        //            OnPropertyChanged(nameof(PasteTemplateToolbarViewModel));
        //        }
        //    }
        //}


        //private ObservableCollection<MpRtbItemViewModel> _rtbItemViewModels = new ObservableCollection<MpRtbItemViewModel>();
        //public ObservableCollection<MpRtbItemViewModel> RtbItemViewModels {
        //    get {
        //        return _rtbItemViewModels;
        //    }
        //    private set {
        //        if (_rtbItemViewModels != value) {
        //            _rtbItemViewModels = value;
        //            OnPropertyChanged(nameof(RtbItemViewModels));
        //        }
        //    }
        //}

        public MpEventEnabledFlowDocument FullDocument {
            get {
                return GetFullDocument();
            }
        }

        //public List<MpRtbItemViewModel> VisibleSubRtbViewModels {
        //    get {
        //        return RtbItemViewModels.Where(x => x.SubItemVisibility == Visibility.Visible).ToList();
        //    }
        //}

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
                if(HostClipTileViewModel == null) {
                    return 0;
                }
                double ch = MpMeasurements.Instance.ClipTileContentHeight;
                if (HostClipTileViewModel.IsEditingContent) {
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
                if (ItemViewModels.Count == 1) {
                    return ch;
                }
                return Math.Max(RtbLbScrollViewerHeight, Math.Max(ch,TotalItemHeight));
            }
        }

        public double RelativeWidthMax {
            get {
                double maxWidth = 0;
                foreach(MpRtbItemViewModel rtbvm in ItemViewModels) {
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
                foreach (MpRtbItemViewModel rtbvm in ItemViewModels) {
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
        

        

        
        #endregion

        #region Business Logic
        
        #endregion

        #region State
        
        #endregion

        #endregion


        #region Public Methods
        public MpRtbItemCollectionViewModel() : base() { }

        public MpRtbItemCollectionViewModel(MpClipTileViewModel ctvm, MpCopyItem ci) : base(ctvm,ci) {
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
            //EditRichTextBoxToolbarViewModel = new MpEditRichTextBoxToolbarViewModel(this);
            //EditTemplateToolbarViewModel = new MpEditTemplateToolbarViewModel(this);
            //PasteTemplateToolbarViewModel = new
            //this.Add(new MpContentListItemViewModel(HostClipTileViewModel, ci));
        }



        public void ClearAllHyperlinks() {
            foreach (MpRtbItemViewModel rtbvm in ItemViewModels) {
                rtbvm.RequestClearHyperlinks();
            }
        }

        public void CreateAllHyperlinks() {
            foreach (MpRtbItemViewModel rtbvm in ItemViewModels) {
                rtbvm.RequestCreateHyperlinks();
            }
        }

        public void Refresh() {
            var sw = new Stopwatch();
            sw.Start();
            //ListBox?.Items.Refresh();
            //MpConsole.WriteLine("Refresh is commented out");
            sw.Stop();
            //MonkeyPaste.MpConsole.WriteLine("Rtblb(HVIdx:"+MpClipTrayViewModel.Instance.VisibleSubRtbViewModels.IndexOf(HostClipTileViewModel)+") Refreshed (" + sw.ElapsedMilliseconds + "ms)");
        }

        public void SyncItemsWithModel() {
            return;
            if(HostClipTileViewModel == null) {
                return;
            }
            var sw = new Stopwatch();
            sw.Start();
            var hci = HostClipTileViewModel.CopyItem;
            var rtbvm = ItemViewModels.Where(x => x.CopyItem.Id == hci.Id).FirstOrDefault();
            if (rtbvm == null) {
              //  this.Add(new MpContentListItemViewModel(HostClipTileViewModel, hci));
            }
            //below was supposed to be for composite types but pulled out to compile
            foreach (var cci in MpCopyItem.GetCompositeChildren(hci)) {
                rtbvm = ItemViewModels.Where(x => x.CopyItem.Id == cci.Id).FirstOrDefault();
                if (rtbvm == null) {
                   // this.Add(new MpContentListItemViewModel(HostClipTileViewModel, cci));
                }
            }
            UpdateSortOrder(true);
            //Refresh();
            UpdateLayout();
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine("Rtbvmc Sync: " + sw.ElapsedMilliseconds + "ms");
        }

        public void UpdateSortOrder(bool fromModel = false) {
            if(fromModel) {
                ItemViewModels.Sort(x => x.CopyItem.CompositeSortOrderIdx);
            } else {
                foreach (var rtbvm in ItemViewModels) {
                    rtbvm.CopyItem.CompositeParentCopyItemId = HostClipTileViewModel.CopyItemId;
                    rtbvm.CopyItem.CompositeSortOrderIdx = ItemViewModels.IndexOf(rtbvm);
                    rtbvm.CopyItem.WriteToDatabase();
                    rtbvm.RequestUiUpdate();
                }
            }
        }
        //public void Add(MpRtbItemViewModel rtbvm, int forceIdx = 0, bool isMerge = false) {    
        //    if(isMerge) {
        //        HostClipTileViewModel.CopyItem.LinkCompositeChild(rtbvm.CopyItem);
        //    }
        //    if (forceIdx >= 0) {
        //        if (forceIdx >= RtbItemViewModels.Count) {
        //            RtbItemViewModels.Add(rtbvm);
        //        } else {
        //            RtbItemViewModels.Insert(forceIdx, rtbvm);
        //        }
        //    } else {
        //        RtbItemViewModels.Add(rtbvm);
        //    }
        //    rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItem));
        //    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
        //    SyncItemsWithModel();
        //    UpdateAdorners();
        //    //ClipTileViewModel.RichTextBoxListBox.Items.Refresh();
        //}

        
        //public void Remove(MpRtbItemViewModel rtbvm, bool isMerge = false) {
        //    if (rtbvm.CopyItem == null) {
        //        //occurs when duplicate detected on background thread
        //        return;
        //    }

        //    if(isMerge) {
        //        rtbvm.HostClipTileViewModel.IsClipDragging = false;
        //        rtbvm.IsSubDragging = false;
        //        UpdateAdorners();
        //    } else {
        //        HostClipTileViewModel.CopyItem.UnlinkCompositeChild(rtbvm.CopyItem);
        //    }


        //    RtbItemViewModels.Remove(rtbvm);
        //    if (RtbItemViewModels.Count == 0) {
        //        //remove empty composite or RichText container
        //        HostClipTileViewModel.Dispose(isMerge);
        //        return;
        //    } else if(RtbItemViewModels.Count == 1) {
        //        var loneCompositeCopyItem = RtbItemViewModels[0].CopyItem;
        //        HostClipTileViewModel.CopyItem.UnlinkCompositeChild(loneCompositeCopyItem);
        //        HostClipTileViewModel.CopyItem.DeleteFromDatabase();
        //       // HostClipTileViewModel.CopyItem = loneCompositeCopyItem;

        //        //now since tile is a single clip update the tiles shortcut button
        //        var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == loneCompositeCopyItem.Id).ToList();
        //        if (scvml.Count > 0) {
        //            HostClipTileViewModel.ShortcutKeyString = scvml[0].KeyString;
        //        } else {
        //            HostClipTileViewModel.ShortcutKeyString = string.Empty;
        //        }
        //    } else {
        //        //update composite sort order without removed item
        //        UpdateSortOrder();
        //    }
        //    //HostClipTileViewModel.CopyItemBmp = HostClipTileViewModel.GetSeparatedCompositeFlowDocument().ToBitmapSource();

        //    if (!isMerge) {
        //        rtbvm.Dispose(isMerge);
        //    }
        //    //Refresh();
        //    UpdateAdorners();
        //}

        public void UpdateAdorners() {
            if(!HostClipTileViewModel.IsClipDropping) {
                foreach (var rtbvm in ItemViewModels) {
                    rtbvm.RequestUiUpdate();
                }
            }
            RtbLbAdornerLayer?.Update();
        }

        public void Resize(double deltaTop, double deltaWidth, double deltaHeight) {
            RtblbCanvasTop += deltaTop;
            RtbLbScrollViewerWidth += deltaWidth;
            RtbLbScrollViewerHeight += deltaHeight;

            //EditRichTextBoxToolbarViewModel.Resize(deltaTop, deltaWidth);

            //PasteTemplateToolbarViewModel.Resize(deltaHeight);
            UpdateLayout();
        }

        public void UpdateLayout() {
            foreach (MpRtbItemViewModel rtbvm in ItemViewModels) {
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

            //if (ListBox != null) {
            //    ListBox.Height = RtbListBoxHeight;
            //    ListBox.Width = RtbLbWidth;
            //    ListBox.UpdateLayout();
            //}
            //if (ScrollViewer != null) {
            //    ScrollViewer.Width = RtbLbScrollViewerWidth;
            //    ScrollViewer.Height = RtbLbScrollViewerHeight;
            //    ScrollViewer.UpdateLayout();
            //}
        }

        public object Clone() {
            var nrtbvmc = new MpRtbItemCollectionViewModel(HostClipTileViewModel,ItemViewModels[0].CopyItem.Clone() as MpCopyItem);
            foreach(MpRtbItemViewModel rtbvm in ItemViewModels) {
                //nrtbvmc.Add((MpRtbItemViewModel)rtbvm.Clone());
            }
            return nrtbvmc;
        }       

        #endregion

        #region Private Methods       

        private MpEventEnabledFlowDocument GetFullDocument() {
            var fullDocument = string.Empty.ToRichText().ToFlowDocument();
            foreach (MpRtbItemViewModel rtbvm in ItemViewModels) {
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

        public void Dispose() {
            RtbListBoxCanvas = null;
            RtbContainerGrid = null;
            //ListBox = null;
            RtbLbAdornerLayer = null;
    }
        #endregion

        
    }
}
