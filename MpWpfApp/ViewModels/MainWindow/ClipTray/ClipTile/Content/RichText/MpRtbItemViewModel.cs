using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MonkeyPaste;
using System.IO;

namespace MpWpfApp {
    [Serializable]
    public class MpRtbItemViewModel : MpContentItemViewModel, ICloneable {
        #region Private Variables
        #endregion

        #region Properties

        #region ViewModels
             
        
        public MpRtbItemCollectionViewModel RtbItemCollectionViewModel {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent as MpRtbItemCollectionViewModel;
            }
        }
        #endregion

        #region Controls 
        //public RichTextBox Rtb { get; set; }

        //public DockPanel RtbListBoxItemOverlayDockPanel { get; set; }

        //public MpClipBorder RtbListBoxItemClipBorder { get; set; }

        //public TextBlock RtbListBoxItemTitleTextBlock { get; set; }

        //public TextBox RtbListBoxItemTitleTextBox { get; set; }

        //public Button DragButton { get; set; }

        //private Grid _rtbc;
        //public Grid Rtbc {
        //    get {
        //        return _rtbc;
        //    }
        //    set {
        //        if(_rtbc != value) {
        //            _rtbc = value;
        //            OnPropertyChanged(nameof(Rtbc));
        //        }
        //    }
        //}

        //public AdornerLayer RtbListBoxItemAdornerLayer { get; set; }

        #endregion

        #region Appearance
        public Cursor RtbListBoxItemCursor {
            get {
                if (HostClipTileViewModel == null) {
                    return Cursors.Arrow;
                }
                if (HostClipTileViewModel.IsExpanded &&
                    IsSubSelected) {
                    return Cursors.IBeam;                    
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Layout

        #endregion

        #region Brushes
        #endregion

        #region Visibility
       

        #endregion

        #region Business Logic  

        private string _rawRtf;
        public string RawRtf {
            get {
                return _rawRtf;
            }
            set {
                if(_rawRtf != value) {
                    _rawRtf = value;
                    OnPropertyChanged(nameof(RawRtf));
                }
            }
        }


        public override bool IsDynamicPaste {
            get {
                return TokenCollection.Tokens.Count > 0;
            }
        }

        public string TemplateRichText { get; set; }
        #endregion

        #region Editor
        #endregion

        #region State

        private bool _hasTextChanged = false;
        public bool HasTextChanged {
            get {
                return _hasTextChanged;
            }
            set {
                if (_hasTextChanged != value) {
                    _hasTextChanged = value;
                    OnPropertyChanged(nameof(HasTextChanged));
                }
            }
        }

        //private TextRange _rtbSelectionRange = null;
        //public TextRange RtbSelectionRange {
        //    get {
        //        return _rtbSelectionRange;
        //    }
        //    set {
        //        if(_rtbSelectionRange != value) {
        //            _rtbSelectionRange = value;
        //            OnPropertyChanged(nameof(RtbSelectionRange));
        //        }
        //    }
        //}

        //public string MultiSelectedOrderIdxDisplayValue {
        //    get {
        //        if (MainWindowViewModel == null || MpClipTrayViewModel.Instance == null || !IsSubSelected) {
        //            return string.Empty;
        //        }
        //        int multiIdx = MpClipTrayViewModel.Instance.GetSelectionOrderIdxForItem(this);
        //        if (multiIdx < 0) {
        //            return string.Empty;
        //        }
        //        multiIdx++;
        //        return multiIdx.ToString();
        //    }
        //}

        //private bool _isResizingTop = false;
        //public bool IsResizingTop {
        //    get {
        //        return _isResizingTop;
        //    }
        //    set {
        //        if(_isResizingTop != value) {
        //            _isResizingTop = value;
        //            OnPropertyChanged(nameof(IsResizingTop));
        //            OnPropertyChanged(nameof(RtbListBoxItemCursor));
        //        }
        //    }
        //}


        //private bool _isResizingBottom = false;
        //public bool IsResizingBottom {
        //    get {
        //        return _isResizingBottom;
        //    }
        //    set {
        //        if (_isResizingBottom != value) {
        //            _isResizingBottom = value;
        //            OnPropertyChanged(nameof(IsResizingBottom));
        //            OnPropertyChanged(nameof(RtbListBoxItemCursor));
        //        }
        //    }
        //}

        //private bool _isHoveringOverTop = false;
        //public bool IsHoveringOverTop {
        //    get {
        //        return _isHoveringOverTop;
        //    }
        //    set {
        //        if (_isHoveringOverTop != value) {
        //            _isHoveringOverTop = value;
        //            OnPropertyChanged(nameof(IsHoveringOverTop));
        //            OnPropertyChanged(nameof(RtbListBoxItemCursor));
        //        }
        //    }
        //}

        //private bool _isHoveringOverBottom = false;
        //public bool IsHoveringOverBottom {
        //    get {
        //        return _isHoveringOverBottom;
        //    }
        //    set {
        //        if (_isHoveringOverBottom != value) {
        //            _isHoveringOverBottom = value;
        //            OnPropertyChanged(nameof(IsHoveringOverBottom));
        //            OnPropertyChanged(nameof(RtbListBoxItemCursor));
        //        }
        //    }
        //}

        //private bool _isHoveringOnTitleTextBlock = false;
        //public bool IsHoveringOnTitleTextBlock {
        //    get {
        //        return _isHoveringOnTitleTextBlock;
        //    }
        //    set {
        //        if (_isHoveringOnTitleTextBlock != value) {
        //            _isHoveringOnTitleTextBlock = value;
        //            OnPropertyChanged(nameof(IsHoveringOnTitleTextBlock));
        //            OnPropertyChanged(nameof(RtbListBoxItemTitleTextBlockTextColor));
        //            if(IsHoveringOnTitleTextBlock) {
        //                OnPropertyChanged(nameof(DetailText));
        //            }
        //        }
        //    }
        //}


        //public bool IsSelected {
        //    get {
        //        return IsSubSelected;
        //    }
        //    set {
        //        IsSubSelected = value;
        //    }
        //}

        //private bool _isSubHovering = false;
        //public bool IsSubHovering {
        //    get {
        //        return _isSubHovering;
        //    }
        //    set {
        //        if (_isSubHovering != value) {
        //            _isSubHovering = value;
        //            OnPropertyChanged(nameof(IsSubHovering));
        //            OnPropertyChanged(nameof(SubItemOverlayVisibility));
        //            OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
        //            OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
        //            OnPropertyChanged(nameof(RtbListBoxItemCursor));
        //            //OnPropertyChanged(nameof(RtbPadding));
        //            //OnPropertyChanged(nameof(RtbCanvasHeight));
        //            //OnPropertyChanged(nameof(RtbCanvasWidth));
        //            //OnPropertyChanged(nameof(RtbCanvasHeight));
        //            //OnPropertyChanged(nameof(RtbPageWidth));
        //            //OnPropertyChanged(nameof(RtbPageHeight));
        //            OnPropertyChanged(nameof(RtbOverlayBorderBrush));
        //        }
        //    }
        //}

        //private bool _isEditingTitle = false;
        //public bool IsSubEditingTitle {
        //    get {
        //        return _isEditingTitle;
        //    }
        //    set {
        //        if (_isEditingTitle != value) {
        //            _isEditingTitle = value;
        //            OnPropertyChanged(nameof(IsSubEditingTitle));
        //            OnPropertyChanged(nameof(RtbListBoxItemTitleTextBlockVisibility));
        //            OnPropertyChanged(nameof(RtbListBoxItemTitleTextBoxVisibility));
        //            OnPropertyChanged(nameof(CopyItemTitle));
        //        }
        //    }
        //}

        public bool IsEditingContent {
            get {
                if(Parent == null) {
                    return false;
                }
                if(IsSubSelected && 
                   HostClipTileViewModel.IsEditingContent && 
                   Parent.SelectedItems.Count == 1) {
                    return true;
                }
                return false;
            }
        }

        private bool _isPastingTemplate = false;
        public bool IsPastingTemplate {
            get {
                return _isPastingTemplate;
            }
            set {
                if (_isPastingTemplate != value) {
                    _isPastingTemplate = value;
                    OnPropertyChanged(nameof(IsPastingTemplate));
                }
            }
        }

        
        #endregion

        #region Model
        public MpEventEnabledFlowDocument ItemFlowDocument {
            get {
                if(CopyItem == null) {
                    return string.Empty.ToFlowDocument();
                }
                return CopyItem.ItemData.ToFlowDocument();
            }
            set {
                if(CopyItem == null) {
                    throw new Exception("Cannot set ItemFlowDocument w/o copy item model");
                }
                CopyItem.ItemData = (value as FlowDocument).ToRichText();
            }
        }
        //public BitmapSource CopyItemFavIcon {
        //    get {
        //        if (CopyItemUrlDomain == null) {
        //            return null;
        //        }
        //        return CopyItemUrlDomain.FavIcon.IconImage.ImageBase64.ToBitmapSource();
        //    }
        //    set {
        //        if (CopyItemUrlDomain != null) {
        //            CopyItemUrlDomain.FavIcon.IconImage.ImageBase64 = value.ToBase64String();
        //            CopyItemUrlDomain.FavIcon.IconImage.WriteToDatabase();
        //            OnPropertyChanged(nameof(CopyItemFavIcon));
        //            //OnPropertyChanged(nameof(AppIcon));
        //        }
        //    }
        //}

        //public BitmapSource CopyItemAppIconHighlightBorder {
        //    get {
        //        if (CopyItem == null || HostClipTileViewModel == null || CopyItem.Source == null || CopyItem.Source.App == null) {
        //            return new BitmapImage();
        //        }
        //        OnPropertyChanged(nameof(AppIconHighlightBorderVisibility));
        //        if (AppIconHighlightBorderVisibility == Visibility.Visible) {
        //            if (HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel != null &&
        //               HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType == MpHighlightType.App &&
        //               HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.RtbItemViewModel == this) {
        //                return CopyItem.Source.App.Icon.IconBorderHighlightSelectedImage.ImageBase64.ToBitmapSource();
        //            }
        //            return CopyItem.Source.App.Icon.IconBorderHighlightImage.ImageBase64.ToBitmapSource(); 
        //        }
        //        return CopyItem.Source.App.Icon.IconBorderHighlightImage.ImageBase64.ToBitmapSource();
        //    }
        //}

        //public BitmapSource CopyItemAppIconBorder {
        //    get {
        //        if (CopyItem == null || CopyItem.Source == null) {
        //            return new BitmapImage();
        //        }
        //        return CopyItem.Source.App.Icon.IconBorderImage.ImageBase64.ToBitmapSource();
        //    }
        //}

        //public BitmapSource CopyItemAppIcon {
        //    get {
        //        if (CopyItem == null) {
        //            return new BitmapImage();
        //        }
        //        return CopyItem.Source.App.Icon.IconImage.ImageBase64.ToBitmapSource();
        //    }
        //}

        public MpCopyItemType CopyItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.ItemType;
            }
        }

        public int CopyItemAppId {
            get {
                if (CopyItem == null || CopyItem.Source == null || CopyItem.Source.App == null) {
                    return 0;
                }
                return CopyItem.Source.App.Id;
            }
        }

        //public BitmapSource CopyItemBmp {
        //    get {
        //        if (CopyItem == null) {
        //            return new BitmapImage();
        //        }
        //        return CopyItem.ItemData.ToBitmapSource();
        //    }
        //}

        private string _detailText = string.Empty;
        public string DetailText {
            get {
                return _detailText;
            }
            set {
                if(_detailText != value) {
                    _detailText = value;
                    OnPropertyChanged(nameof(DetailText));
                }
            }
        }

        public int CompositeParentCopyItemId {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeParentCopyItemId;
            }
            set {
                if(CopyItem != null && CopyItem.CompositeParentCopyItemId != value) {
                    CopyItem.CompositeParentCopyItemId = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
                }
            }
        }

        public int CompositeSortOrderIdx {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeSortOrderIdx;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeSortOrderIdx != value) {
                    CopyItem.CompositeSortOrderIdx = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
                }
            }
        }

        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.Id;
            }
        }

        public string CopyItemPlainText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.ItemData.ToPlainText();
            }
        }

        public string CopyItemDescription {
            get {
                if (CopyItem == null || CopyItem.ItemDescription == null) {
                    return string.Empty;
                }
                return CopyItem.ItemDescription;
            }
            set {
                if (CopyItem != null && CopyItem.ItemDescription != value) {
                    CopyItem.ItemDescription = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemDescription));
                }
            }
        }

        public string CopyItemRichText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(CopyItem.ItemData)) {
                    return string.Empty.ToRichText();
                }
                return CopyItem.ItemData;
            }
            set {
                if (CopyItem != null && CopyItem.ItemData != value) {
                    //value should be raw rtf where templates are encoded into #name#color# groups
                    CopyItem.ItemData = value;
                    CopyItem.WriteToDatabase();                    
                    OnPropertyChanged(nameof(CopyItemRichText));
                }
            }
        }

        private string _copyItemFilePath = string.Empty;
        public string CopyItemFilePath {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MpClipTrayViewModel.Instance == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(_copyItemFilePath)) {
                    // TODO move file list stuff to this view model
                    _copyItemFilePath = string.Empty; //CopyItem.GetFileList()[0];
                }
                return _copyItemFilePath;
            }
            set {
                if(_copyItemFilePath != value) {
                    _copyItemFilePath = value;
                    OnPropertyChanged(nameof(CopyItemFilePath));
                }
            }
        }

        public string CopyItemTitle {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Title;
            }
            set {
                if (CopyItem != null && CopyItem.Title != value) { 
                    //AddUndo(this, nameof(CopyItemTitle), CopyItem.Title, value);
                    CopyItem.Title = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public  List<string> CopyItemFileDropList {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MpClipTrayViewModel.Instance == null) {
                    return new List<string>();
                }
                // TODO add file list stuff
                return new List<string>(); //return CopyItem.GetFileList();// string.Empty, MpClipTrayViewModel.Instance.GetTargetFileType());
            }
        }

        public Brush CopyItemColorBrush {
            get {
                if(CopyItem == null || CopyItem.ItemColor == null) {
                    return Brushes.Transparent;
                }
                return new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(CopyItem.ItemColor));
            }
            set {
                if(new SolidColorBrush(MpHelpers.Instance.ConvertHexToColor(CopyItem.ItemColor)) != value) {
                    CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemColorBrush));
                }
            }
        }

        public MpUrlDomain CopyItemUrlDomain {
            get {
                if (CopyItemUrl == null) {
                    return null;
                }
                return CopyItemUrl.UrlDomain;
            }
            set {
                if (CopyItemUrl != null && CopyItemUrl.UrlDomain != value) {
                    CopyItemUrl.UrlDomain = value;
                    CopyItemUrl.UrlDomain.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemUrlDomain));
                }
            }
        }

        public MpUrl CopyItemUrl {
            get {
                if (CopyItem == null || CopyItem.Source == null || CopyItem.Source.Url == null) {
                    return null;
                } 
                return CopyItem.Source.Url;
            }
            set {
                if (CopyItem != null && CopyItem.Source != null && CopyItem.Source.Url != value) {
                    CopyItem.Source.Url = value;
                    CopyItemUrlDomain = CopyItemUrl.UrlDomain;
                    CopyItem.Source.Url.WriteToDatabase();
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemUrl));
                    OnPropertyChanged(nameof(CopyItemUrlDomain));
                    //OnPropertyChanged(nameof(AppIcon));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public string CopyItemAppPath {
            get {
                if (CopyItem == null || CopyItem.Source == null || CopyItem.Source.App == null) {
                    return string.Empty;
                }
                return CopyItem.Source.App.AppPath;
            }
        }

        public string CopyItemAppName {
            get {
                if (CopyItem == null || CopyItem.Source == null || CopyItem.Source.App == null) {
                    return string.Empty;
                }
                return CopyItem.Source.App.AppName;
            }
        }

        public int PasteCount {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.PasteCount;
            }
            set {
                if (CopyItem != null && CopyItem.PasteCount != value) {
                    CopyItem.PasteCount = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(PasteCount));
                }
            }
        }

        public List<MpCopyItemTemplate> TemplateList {
            get {
                if(CopyItem == null) {
                    return new List<MpCopyItemTemplate>();
                }
                return MpDb.Instance.GetItems<MpCopyItemTemplate>().Where(x => x.CopyItemId == CopyItem.Id).ToList();
            }
        }

        
        #endregion

        #endregion

        #region Events
        public event EventHandler<bool> OnRtbResetRequest;
        public event EventHandler OnClearTokensRequest;
        public event EventHandler OnCreateTokensRequest;
        public event EventHandler OnSyncModels;
        #endregion

        #region ContentItem Overrides
        

        #endregion

        #region Public Methods
        public MpRtbItemViewModel() : this(null,null) { }

        public MpRtbItemViewModel(MpContentContainerViewModel ccvm, MpCopyItem ci) : base(ccvm,ci) {
            TokenCollection = new MpTokenCollectionViewModel(this);
            
            PropertyChanged += (s, e) => {
                var rtbvm = s as MpRtbItemViewModel;
                switch (e.PropertyName) {
                    case nameof(rtbvm.CopyItem):
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemId));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CompositeParentCopyItemId));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CompositeSortOrderIdx));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemFilePath));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemRichText));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemPlainText));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemTitle));
                        //rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemBmp));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemUrl));
                        //rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemFavIcon));
                        //rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemAppIcon));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.PasteCount));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemAppName));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemUrl));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemUrlDomain));
                        break;
                    case nameof(rtbvm.IsSubSelected):
                        if (rtbvm.IsSubSelected) {
                            rtbvm.LastSubSelectedDateTime = rtbvm.IsSubSelected ? DateTime.Now : DateTime.MaxValue;
                        }
                        if (rtbvm.HostClipTileViewModel.IsClipDragging) {
                            return;
                        }
                        if (rtbvm.IsSubSelected && !rtbvm.HostClipTileViewModel.IsSelected) {
                            rtbvm.HostClipTileViewModel.IsSelected = true;
                        } else if (!rtbvm.IsSubSelected &&
                                  rtbvm.HostClipTileViewModel.IsSelected &&
                                  rtbvm.Parent.ItemViewModels.Count == 1) {
                            //rtbvm.HostClipTileViewModel.IsSelected = false;
                        }
                        if (rtbvm.IsSubSelected) {
                            if (rtbvm.HostClipTileViewModel.IsExpanded) {
                                foreach (var ortbvm in rtbvm.Parent.ItemViewModels) {
                                    if (ortbvm != rtbvm) {
                                        ortbvm.IsSubSelected = false;
                                        ortbvm.IsSubEditingTitle = false;
                                    }
                                }
                                rtbvm.RequestRtbReset();
                                //RichTextBoxViewModelCollection.Refresh();
                            } else {
                                var test = !MpHelpers.Instance.IsMultiSelectKeyDown();
                                test = !rtbvm.IsSubDragging;
                                test = !rtbvm.HostClipTileViewModel.IsContextMenuOpened;
                                test = !rtbvm.IsSubContextMenuOpened;
                                test = !rtbvm.HostClipTileViewModel.IsClipDragging;
                                test = !MpClipTrayViewModel.Instance.IsPastingHotKey;
                                test = !MpClipTrayViewModel.Instance.IsPastingSelected;
                                test = MpMainWindowViewModel.IsMainWindowOpen;
                                if (!MpHelpers.Instance.IsMultiSelectKeyDown() &&
                                       !rtbvm.IsSubDragging &&
                                       !rtbvm.HostClipTileViewModel.IsContextMenuOpened &&
                                       !rtbvm.IsSubContextMenuOpened &&
                                       !rtbvm.HostClipTileViewModel.IsClipDragging &&
                                       !MpClipTrayViewModel.Instance.IsPastingHotKey &&
                                       !MpClipTrayViewModel.Instance.IsPastingSelected &&
                                       MpMainWindowViewModel.IsMainWindowOpen) {
                                    MonkeyPaste.MpConsole.WriteLine(@"RtbItem Selection was rejected");
                                    rtbvm.IsSubSelected = false;
                                }
                            }
                            if (rtbvm.HostClipTileViewModel.IsEditingContent) {
                                //rtbvm.HostClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(rtbvm.Rtb, false);
                            }
                        } else if (rtbvm.HostClipTileViewModel.IsEditingContent) {
                            rtbvm.SaveSubItemToDatabase();
                        } else {

                        }
                        if (rtbvm.HostClipTileViewModel.IsExpanded) {
                            rtbvm.Parent.RequestUiUpdate();
                        }
                        break;
                    case nameof(rtbvm.IsSubHovering):
                        if(rtbvm.IsSubHovering) {
                            if (MpClipTrayViewModel.Instance.IsScrolling) {
                                rtbvm.IsSubHovering = false;
                            }
                            foreach (var ortbvm in rtbvm.Parent.ItemViewModels) {
                                if (ortbvm != rtbvm) {
                                    ortbvm.IsSubHovering = false;
                                }
                            }
                        } else {
                            //_detailIdx = 1;
                        }
                        break;
                    //case nameof(rtbvm.IsHoveringOnTitleTextBlock):
                    //    if(rtbvm.IsHoveringOnTitleTextBlock) {
                    //        rtbvm.DetailIdx++;
                    //        if (rtbvm.DetailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    //            rtbvm.DetailIdx = 1;
                    //        }
                    //        rtbvm.DetailText = rtbvm.GetDetail((MpCopyItemDetailType)rtbvm.DetailIdx);
                    //    }
                    //    break;
                    case nameof(rtbvm.IsSubContextMenuOpened):
                        if(rtbvm.IsSubContextMenuOpened) {
                            rtbvm.IsSubSelected = true;
                        }
                        rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileBorderBrush));
                        rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileBorderBrushRect));
                        break;
                }
            };

            ViewModelLoaded += (s, e) => {
                var rtbvm = s as MpRtbItemViewModel;
                if (!MpMainWindowViewModel.IsMainWindowLoading) {
                    //Task.Run(rtbvm.GatherAnalytics);
                } else {
                    if (rtbvm.Parent.ItemViewModels.IndexOf(rtbvm) == 0) {
                        MpHelpers.Instance.RunOnMainThread((Action)(() => {
                            //rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.AppIcon));
                            rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileTitleIconSize));
                            rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileTitleIconBorderSize));
                        }), DispatcherPriority.Render);
                    }
                }

                //rtbvm.RtbSelectionRange = new TextRange(rtbvm.Rtb.Document.ContentEnd, rtbvm.Rtb.Document.ContentEnd);
            };

            OnPropertyChanged(nameof(CopyItem));
        }

        //public void TemporarySetRtb(RichTextBox rtb) {
        //    Rtbc = rtb.GetVisualAncestor<Grid>();
        //    Rtb = rtb;//(RichTextBox)Rtbc.FindName("Rtb");
        //    RtbListBoxItemClipBorder = (MpClipBorder)Rtbc.FindName("RtbListBoxItemOverlayBorder");
        //    RtbListBoxItemOverlayDockPanel = (DockPanel)Rtbc.FindName("RtbListItemOverlayBorderDockPanel");
        //    RtbListBoxItemTitleTextBlock = (TextBlock)Rtbc.FindName("RtbTitleTextBlock");
        //    RtbListBoxItemTitleTextBox = (TextBox)Rtbc.FindName("RtbTitleTextBox");
        //    DragButton = (Button)Rtbc.FindName("DragButton");
        //    var titleIconImageButton = (Button)Rtbc.FindName("RtbItemAppIconImageButton");
        //    var titleIconBorderImage = (Image)Rtbc.FindName("RtbItemAppIconBorderImage");
        //    var titleIconBorderImageScaleTransform = (ScaleTransform)Rtbc.FindName("RtbItemAppIconBorderImageScaleTransform");
        //    var titleIconHighlightBorderImage = (Image)Rtbc.FindName("RtbItemAppIconHighlightedBorderImage");

            


            

        //    RequestUiUpdate();

            

        //    OnViewModelLoaded();
        //}

        public void RequestSyncModels() {
            OnSyncModels?.Invoke(this, null);
        }

        public void RequestRtbReset() {
            OnRtbResetRequest?.Invoke(this, IsSubSelected);
        }

        public void RequestClearHyperlinks() {
            OnClearTokensRequest?.Invoke(this, null);
        }

        public void RequestCreateHyperlinks() {
            OnCreateTokensRequest?.Invoke(this, null);
        }

        public void ClearSubDragDropState() {
            IsSubDragging = false;
            MouseDownPosition = new Point();
            DragDataObject = null;
        }

        public async Task GatherAnalytics() {
            string detectedUrl = string.Empty;

            //if (CopyItem.ItemScreenshot != null) {
            //    detectedUrl = await MpBrowserUrlDetector.Instance.FindUrlAddressFromScreenshot(CopyItem.ItemScreenshot);
            //}

            //if (!string.IsNullOrEmpty(detectedUrl)) {
            //    string urlTitle = await MpHelpers.Instance.GetUrlTitle(detectedUrl); 
            //    var itemUrl = new MpUrl(detectedUrl, urlTitle);
            //    if(itemUrl.UrlDomain == null) {
            //        string urlDomain = MpHelpers.Instance.GetUrlDomain(detectedUrl);
            //        var urlFavIcon = MpHelpers.Instance.GetUrlFavicon(urlDomain);
            //        urlDomain = MpHelpers.Instance.GetFullyFormattedUrl(urlDomain);
            //        string urlDomainTitle = await MpHelpers.Instance.GetUrlTitle(urlDomain);
            //        CopyItemUrlDomain = new MpUrlDomain(urlDomain, urlFavIcon, urlDomainTitle, false);
            //    }
            //    itemUrl.UrlDomain = CopyItemUrlDomain;
            //    CopyItemUrl = itemUrl;

            //    if (RichTextBoxViewModelCollection.Count == 1) {
            //        await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
            //            HostClipTileViewModel.CopyItemUrl = CopyItemUrl;
            //            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItemFavIcon));
            //            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.AppIcon));
            //            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileTitleIconSize));
            //            HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileTitleIconBorderSize));
            //        }), DispatcherPriority.Render);
            //    }

                

                MonkeyPaste.MpConsole.WriteLine("Detected Browser Address: " + detectedUrl);
            
        }

        public void RefreshAsyncCommands() {
            MpClipTrayViewModel.Instance.RefreshAllCommands();
        }

        //public void UpdateLayout() {
        //    if (Rtb != null) {
        //        Rtb.Document.PageWidth = RtbPageWidth;
        //        Rtb.Document.PageHeight = RtbPageHeight;
        //    }

        //    OnPropertyChanged(nameof(SubItemOverlayVisibility));
        //    OnPropertyChanged(nameof(SubItemToolTipVisibility));
        //    OnPropertyChanged(nameof(RtbPadding));
        //    OnPropertyChanged(nameof(RtbCanvasWidth));
        //    OnPropertyChanged(nameof(RtbCanvasHeight));
        //    OnPropertyChanged(nameof(RtbWidth));
        //    OnPropertyChanged(nameof(RtbHeight));
        //    OnPropertyChanged(nameof(RtbPageWidth));
        //    OnPropertyChanged(nameof(RtbPageHeight));
        //    OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));


        //    Rtbc?.UpdateLayout();
        //    Rtb?.UpdateLayout();
        //    RtbListBoxItemClipBorder?.UpdateLayout();
        //}

        //public void ResetRtb() {
        //    if (Rtb == null) {
        //        return;
        //    }
        //    Rtb.ScrollToHome();
        //    Rtb.CaretPosition = Rtb.Document.ContentStart;
        //    Rtb.Selection.Select(Rtb.Document.ContentStart, Rtb.Document.ContentStart);

        //    ContainerViewModel.RequestUiUpdate();
        //    if(IsSubSelected) {
        //        Rtb.Focus();
        //    }
        //}

        public void SaveSubItemToDatabase() {
            RequestSyncModels();

            CopyItem.WriteToDatabase();
        }

        
        #region Hyperlinks
        
        #endregion

        #region Clone Implementation
        public object Clone() {
            var nrtbvm = new MpRtbItemViewModel(Parent, CopyItem);
            return nrtbvm;
        }
        #endregion

        #region Private methods
        private void UpdateDetails() {
            Size itemSize;
            int fc, lc, cc;
            double ds;
            switch (CopyItem.ItemType) {
                case MpCopyItemType.Image:
                    var bmp = CopyItem.ItemData.ToBitmapSource();
                    itemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MpCopyItemType.FileList:
                    fc = GetFileList().Count;
                    ds = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.RichText:
                    lc = MpHelpers.Instance.GetRowCount(CopyItemPlainText);
                    cc = CopyItemPlainText.Length;
                    itemSize = ItemFlowDocument.GetDocumentSize();
                    break;
            }
        }

        public override string GetDetail(MpCopyItemDetailType detailType) {
            Size itemSize;
            int fc = 0, lc = 0, cc = 0;
            double ds = 0;
            switch (CopyItem.ItemType) {
                case MpCopyItemType.Image:
                    var bmp = CopyItem.ItemData.ToBitmapSource();
                    itemSize = new Size(bmp.Width, bmp.Height);
                    break;
                case MpCopyItemType.FileList:
                    fc = GetFileList().Count;
                    ds = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.RichText:
                    lc = MpHelpers.Instance.GetRowCount(CopyItemPlainText);
                    cc = CopyItemPlainText.Length;
                    itemSize = ItemFlowDocument.GetDocumentSize();
                    break;
            }
            string info = "I dunno";// string.Empty;
            switch (detailType) {
                //created
                case MpCopyItemDetailType.DateTimeCreated:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc
                    info = "Copied " + CopyItem.CopyDateTime.ToString();
                    break;
                //chars/lines
                case MpCopyItemDetailType.DataSize:
                    if (CopyItem.ItemType == MpCopyItemType.Image) {
                        var bmp = CopyItem.ItemData.ToBitmapSource();
                        info = "(" + (int)bmp.Width + "px) x (" + (int)bmp.Height + "px)";
                    } else if (CopyItem.ItemType == MpCopyItemType.RichText) {
                        info = cc + " chars | " + lc + " lines";
                    } else if (CopyItemType == MpCopyItemType.FileList) {
                        info = fc + " files | " + ds + " MB";
                    }
                    break;
                //# copies/# pastes
                case MpCopyItemDetailType.UsageStats:
                    info = cc + " copies | " + PasteCount + " pastes";
                    break;
                default:
                    info = "Unknown detailId: " + (int)detailType;
                    break;
            }

            return info;
        }
        #endregion

        #endregion


        #region Overrides
        public void Dispose(bool isMerge = false) {
            Parent.RemoveRange(new List<MpCopyItem> { CopyItem });
            
            
            if (!isMerge) {
                CopyItem.DeleteFromDatabase();
            }
            
            //Rtb = null;
            //RtbListBoxItemOverlayDockPanel = null;
            //RtbListBoxItemClipBorder = null;
            //RtbListBoxItemTitleTextBlock = null;
            //RtbListBoxItemTitleTextBox = null;
            //Rtbc = null;
            //RtbListBoxItemAdornerLayer = null;
        }        
        #endregion

        public ICommand RefreshDocumentCommand {
            get {
                return new RelayCommand(
                    () => {
                        SaveSubItemToDatabase();
                    },
                    () => {
                        return HasTextChanged;
                    });
            }
        }
    }

}
