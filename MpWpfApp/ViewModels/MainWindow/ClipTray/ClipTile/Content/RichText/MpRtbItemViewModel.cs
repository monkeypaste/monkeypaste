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
        private MpTemplateHyperlinkCollectionViewModel _templateHyperlinkCollectionViewModel;
        public MpTemplateHyperlinkCollectionViewModel TemplateHyperlinkCollectionViewModel {
            get {
                return _templateHyperlinkCollectionViewModel;
            }
            set {
                if (_templateHyperlinkCollectionViewModel != value) {
                    _templateHyperlinkCollectionViewModel = value;
                    OnPropertyChanged(nameof(TemplateHyperlinkCollectionViewModel));
                }
            }
        }        
        
        public MpRtbItemCollectionViewModel RtbItemCollectionViewModel {
            get {
                if(ContainerViewModel == null) {
                    return null;
                }
                return ContainerViewModel as MpRtbItemCollectionViewModel;
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
        //public Rect DragButtonRect {
        //    get {
        //        if(DragButton == null || Rtbc == null) {
        //            return new Rect();
        //        }
        //        return DragButton.TransformToAncestor((Visual)Rtbc).TransformBounds(LayoutInformation.GetLayoutSlot(DragButton));
        //    }
        //}

        //public double SubItemAppIconSize {
        //    get {
        //        return MpMeasurements.Instance.RtbCompositeAppIconSize;
        //    }
        //}

        //public double SubItemAppIconBorderSize {
        //    get {
        //        return MpMeasurements.Instance.RtbCompositeAppIconBorderSize;
        //    }
        //}

        //public double SubItemAppIconInnerBorderSize {
        //    get {
        //        return MpMeasurements.Instance.RtbCompositeAppIconBorderSize * 0.8;
        //    }
        //}

        //public double RtbListBoxItemTitleFontSize {
        //    get {
        //        return MpMeasurements.Instance.RtbCompositeItemTitleFontSize;
        //    }
        //}

        //public Thickness RtbPadding {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return new Thickness(0);
        //        }
        //        var mm = MpMeasurements.Instance.RtbEditModeMinMargin;

        //        if(SubItemOverlayVisibility == Visibility.Visible) {
        //            return new Thickness(DragButtonSize + mm, RtbListBoxItemTitleFontSize + mm, mm + SubItemAppIconSize, 0);
        //        }
        //        if(IsEditingContent) {
        //            return new Thickness(0);
        //        }
        //        return new Thickness(mm);
        //    }
        //}

        //public double RtbWidth {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return 0;
        //        }
        //        if (HostClipTileViewModel.IsExpanded) {
        //            return Math.Max(
        //                RtbPageWidth + RtbPadding.Left + RtbPadding.Right,
        //                HostClipTileViewModel.TileContentWidth);
        //        }
        //        return RtbCanvasWidth;// - 23;
        //    }
        //}

        //public double RtbHeight {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return 0;
        //        }
        //        if (HostClipTileViewModel.IsExpanded) {
        //            if (ContainerViewModel.Count == 1) {
        //                return GetExpandedSize().Height;
        //            }
        //            //if (IsSubSelected) {
        //            //    return Math.Max(RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom, MpMeasurements.Instance.RtbCompositeItemMinHeight);
        //            //}
        //            //if (IsSubHovering) {
        //            //    return MpMeasurements.Instance.RtbCompositeItemMinHeight + RtbPadding.Top + RtbPadding.Bottom;
        //            //}
        //            //return MpMeasurements.Instance.RtbCompositeItemMinHeight;
        //            //if(Rtbc == null) {
        //            //    return Math.Max(RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom, MpMeasurements.Instance.RtbCompositeItemMinHeight);
        //            //} else {
        //            //    return Math.Max(Rtbc.ActualHeight, MpMeasurements.Instance.RtbCompositeItemMinHeight);

        //            //}
        //            return Math.Max(RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom, MpMeasurements.Instance.RtbCompositeItemMinHeight);
        //        }
        //        if (ContainerViewModel.Count == 1 && SubItemOverlayVisibility != Visibility.Visible) {
        //            if (Rtb == null) {
        //                var fd = CopyItemRichText.ToFlowDocument();
        //                fd.PageWidth = RtbCanvasWidth - RtbPadding.Left - RtbPadding.Right;
        //                return fd.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
        //            } else {
        //                return HostClipTileViewModel.TileContentHeight;

        //                //var fd = Rtb.Document;
        //                //fd.PageWidth = RtbCanvasWidth - RtbPadding.Left - RtbPadding.Right;
        //                //return fd.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
        //            }
        //        }
        //        return MpMeasurements.Instance.RtbCompositeItemMinHeight;
        //    }
        //}

        //public double RtbPageWidth {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return 0;
        //        }
        //        if (HostClipTileViewModel.IsExpanded) {                    
        //            if (Rtb == null) {
        //                double curFontSize = 16;
        //                var width = Math.Max(
        //                CopyItemRichText.ToFlowDocument().GetDocumentSize().Width + (curFontSize * 3),
        //                RtbCanvasWidth + (curFontSize * 3));

        //                return width;// - (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
        //            } else {
        //                var fs = Rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
        //                double curFontSize = fs.GetType() == typeof(double) ? (double)fs : 16;
        //                var width = Math.Max(
        //                Rtb.Document.GetDocumentSize().Width + (curFontSize * 3),
        //                Rtbc.ActualWidth /*RtbCanvasWidth*/ + (curFontSize * 3));

        //                return width;// - (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
        //            }
                    
        //        }
        //        return RtbWidth - RtbPadding.Left - RtbPadding.Right - 23;
                
        //    }
        //}

        //public double RtbPageHeight {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return 0;
        //        }
        //        if (HostClipTileViewModel.IsExpanded) {
        //            if (Rtb == null) {
        //                return CopyItemRichText.ToFlowDocument().GetDocumentSize().Height;
        //            } else {
        //                if(HostClipTileViewModel.IsPastingTemplate) {
        //                    return Rtb.Document.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
        //                }
        //                return Rtb.Document.GetDocumentSize().Height;
        //            }
        //        }
        //        return RtbHeight - RtbPadding.Top - RtbPadding.Bottom;
        //    }
        //}

        //public double RtbCanvasWidth {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return 0;
        //        }
        //        var w = HostClipTileViewModel.TileContentWidth - MpMeasurements.Instance.ClipTileSubItemOverlayMargin;
        //        if (ContainerViewModel.VerticalScrollbarVisibility == ScrollBarVisibility.Visible) {
        //            w -= MpMeasurements.Instance.ScrollbarWidth;
        //        }
        //        return w;
        //    }
        //}

        //public double RtbCanvasHeight {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return 0;
        //        }
        //        return RtbHeight;
        //    }
        //}

        //public double RtbRelativeWidthMax {
        //    get {
        //        if (CopyItem == null) {
        //            return 0;
        //        }
        //        var doc = Rtb == null ? ItemFlowDocument : Rtb.Document;
        //        return doc.GetDocumentSize().Width;
        //    }
        //}
        #endregion

        #region Brushes
        //public Brush RtbListBoxItemBorderBrush {
        //    get {
        //        if (!IsCompositeChild) {
        //            return Brushes.Transparent;
        //        }
        //        if (IsPrimarySubSelected) {
        //            return Brushes.Blue;
        //        }
        //        if (IsSubSelected) {
        //            return Brushes.Red;
        //        }
        //        if (IsSubHovering) {
        //            return Brushes.Yellow;
        //        }
        //        return Brushes.Transparent;
        //    }
        //}

        //public Brush RtbListBoxItemBackgroundColor {
        //    get {
        //        if(HostClipTileViewModel == null) {
        //            return Brushes.Transparent;
        //        }
        //        if(SubItemOverlayVisibility == Visibility.Visible) {
        //            var scb = CopyItemColorBrush;
        //            if (HostClipTileViewModel.IsExpanded && IsSubHovering) {
        //                scb.Opacity = 0.1;
        //            } else if (/*HostClipTileViewModel.IsClipDropping || */
        //                       (!HostClipTileViewModel.IsExpanded && IsSubHovering && !IsSubSelected)) {
        //                scb.Opacity = 0;
        //            } else {
        //                scb.Opacity = 0.25;
        //            }
                    
        //            return scb;
        //        }
        //        return Brushes.Transparent;
        //    }
        //    set {
        //        if (CopyItem != null && CopyItem.ItemColor != MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color)) {
        //            CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
        //            //CopyItem.WriteToDatabase();
        //            OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
        //            OnPropertyChanged(nameof(CopyItem));
        //        }
        //    }
        //}

        //public Brush RtbListBoxItemTitleTextBlockTextColor {
        //    get {
        //        if (!IsCompositeChild) {
        //            return Brushes.Transparent;
        //        }
        //        if (IsHoveringOnTitleTextBlock) {
        //            return Brushes.DarkGray;
        //        }
        //        return Brushes.Black;
        //    }
        //}

        //public Brush RtbOverlayBorderBrush {
        //    get {
        //        if (HostClipTileViewModel == null) {
        //            return Brushes.Transparent;
        //        }
        //        if(HostClipTileViewModel.IsPastingTemplate) {
        //            return Brushes.Transparent;
        //        }
        //        if (IsSubSelected) {
        //            return Brushes.Pink;
        //        }
        //        if (IsSubHovering) {
        //            return Brushes.Yellow;
        //        }
        //        return Brushes.Transparent;
        //    }
        //}
        #endregion

        #region Visibility
        //public Visibility MultiSelectOrderMarkerVisibility {
        //    get {
        //        if (MainWindowViewModel == null || MpClipTrayViewModel.Instance == null) {
        //            return Visibility.Hidden;
        //        }
        //        if (IsSubSelected && 
        //            (MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 1 ||
        //             ContainerViewModel.SubSelectedContentItems.Count > 1)) {
        //            return Visibility.Visible;
        //        }
        //        return Visibility.Collapsed;
        //    }
        //}

        //private Visibility _subItemVisibility = Visibility.Visible;
        //public Visibility SubItemVisibility {
        //    get {
        //        return _subItemVisibility;
        //    }
        //    set {
        //        if(_subItemVisibility != value) {
        //            _subItemVisibility = value;
        //            OnPropertyChanged(nameof(SubItemVisibility));
        //        }
        //    }
        //}

        //public Visibility DragAndHotkeyButtonVisibility {
        //    get {
        //        if(MainWindowViewModel.SearchBoxViewModel.HasText) {
        //            return Visibility.Hidden;
        //        }
        //        return Visibility.Visible;
        //    }
        //}

        //public Visibility SubItemToolTipVisibility {
        //    get {
        //        if (CopyItem == null || !Properties.Settings.Default.ShowItemPreview) {
        //            return Visibility.Collapsed;
        //        }
        //        return (MpClipTrayViewModel.Instance.IsScrolling || IsSubSelected || HostClipTileViewModel.IsExpanded) ? Visibility.Collapsed : Visibility.Visible;
        //    }
        //}

        //public Visibility SubItemOverlayVisibility {
        //    get {
        //        OnPropertyChanged(nameof(DragAndHotkeyButtonVisibility));
        //        if (HostClipTileViewModel == null) {
        //            return Visibility.Collapsed;
        //        }               

        //        if (IsSubEditingTitle) {
        //            return Visibility.Visible;
        //        }
        //        if(HostClipTileViewModel.IsPastingTemplate) {
        //            return Visibility.Hidden;
        //        }
        //        if (HostClipTileViewModel.IsClipDropping) {
        //            return Visibility.Collapsed;
        //        }
        //        //if(HostClipTileViewModel.IsClipDropping && (!HostClipTileViewModel.IsAnySubItemDragging || HostClipTileViewModel.IsClipDragging)) {
        //        //    return Visibility.Visible;
        //        //}
        //        if (!IsCompositeChild) {
        //            return Visibility.Collapsed;
        //        }
        //        if(MainWindowViewModel.SearchBoxViewModel.HasText &&
        //           HostClipTileViewModel.TileVisibility == Visibility.Visible &&
        //           HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel != null &&
        //           (HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.ContentItemViewModel != this ||
        //            HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType != MpHighlightType.Text)) {                    
        //            //this ensures overlay is shown when selected search match is relavant to title or application
        //            return Visibility.Visible;
        //        }
        //        if(IsSubSelected && !HostClipTileViewModel.IsExpanded) {
        //            return Visibility.Visible;
        //        }

        //        if(!IsSubSelected && HostClipTileViewModel.IsExpanded) {
        //            return Visibility.Visible;
        //        }

        //        if (IsSubHovering && (!IsSubSelected || !HostClipTileViewModel.IsEditingContent)) {
        //            return Visibility.Visible;
        //        }
        //        if (IsSubSelected && ContainerViewModel.SubSelectedContentItems.Count > 1) {
        //            return Visibility.Visible;
        //        }                
        //        return Visibility.Collapsed;
        //    }
        //}

        //public Visibility RtbListBoxItemTitleTextBlockVisibility {
        //    get {
        //        if (IsSubEditingTitle) {
        //            return Visibility.Collapsed;
        //        }
        //        return Visibility.Visible;
        //    }
        //}

        //public Visibility RtbListBoxItemTitleTextBoxVisibility {
        //    get {
        //        if (IsSubEditingTitle) {
        //            return Visibility.Visible;
        //        }
        //        return Visibility.Collapsed;
        //    }
        //}

        //public Visibility AppIconHighlightBorderVisibility {
        //    get {
        //        if(HostClipTileViewModel == null) {
        //            return Visibility.Hidden;
        //        }
        //        return HostClipTileViewModel.HighlightTextRangeViewModelCollection.AppMatchRtbvmList.Contains(this) ? Visibility.Visible : Visibility.Hidden;
        //    }
        //}

        //public Visibility AppIconImageVisibility {
        //    get {
        //        if (MainWindowViewModel == null || MpClipTrayViewModel.Instance == null || !IsSelected) {
        //            return Visibility.Visible;
        //        }
        //        if (MpClipTrayViewModel.Instance.SelectedClipTiles.Count > 1 &&
        //           !IsSubHovering) {
        //            return Visibility.Hidden;
        //        }
        //        return Visibility.Visible;
        //    }
        //}
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
                return TemplateHyperlinkCollectionViewModel.Templates.Count > 0;
            }
        }

        public string TemplateRichText { get; set; }
        #endregion

        #region Editor
        private bool _useSpellCheck = Properties.Settings.Default.UseSpellCheck;
        public bool UseSpellCheck {
            get {
                return _useSpellCheck;
            }
            set {
                if (_useSpellCheck != value) {
                    _useSpellCheck = value;
                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

        private ObservableCollection<FontFamily> _systemFonts;
        public ObservableCollection<FontFamily> SystemFonts {
            get {
                if(_systemFonts == null) {
                    _systemFonts = new ObservableCollection<FontFamily>(Fonts.SystemFontFamilies);
                }
                return _systemFonts;
            }
            set {
                if(_systemFonts != value) {
                    _systemFonts = value;
                    OnPropertyChanged(nameof(SystemFonts));
                }
            }
        }
        private ObservableCollection<string> _fontSizes;
        public ObservableCollection<string> FontSizes {
            get {
                if (_fontSizes == null) {
                    _fontSizes = new ObservableCollection<string>() {
                         "8",
                        "9",
                        "10",
                        "11",
                        "12",
                        "14",
                        "16",
                        "18",
                        "20",
                        "22",
                        "24",
                        "26",
                        "28",
                        "36",
                        "48",
                        "72" };
                }
                return _fontSizes;
            }
            set {
                if(_fontSizes != value) {
                    _fontSizes = value;
                    OnPropertyChanged(nameof(FontSizes));
                }
            }
        }
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

        private bool _isOverDragButton = false;
        public bool IsOverDragButton {
            get {
                return _isOverDragButton;
            }
            set {
                if (_isOverDragButton != value) {
                    _isOverDragButton = value;
                    OnPropertyChanged(nameof(IsOverDragButton));
                }
            }
        }

        private bool _isSubDragging = false;
        public bool IsSubDragging {
            get {
                return _isSubDragging;
            }
            set {
                if(_isSubDragging != value) {
                    _isSubDragging = value;
                    OnPropertyChanged(nameof(IsSubDragging));
                }
            }
        }

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

        private bool _isSubContextMenuOpened = false;
        public bool IsSubContextMenuOpened {
            get {
                return _isSubContextMenuOpened;
            }
            set {
                if (_isSubContextMenuOpened != value) {
                    _isSubContextMenuOpened = value;
                    OnPropertyChanged(nameof(IsSubContextMenuOpened));
                }
            }
        }

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
                if(ContainerViewModel == null) {
                    return false;
                }
                if(IsSubSelected && 
                   HostClipTileViewModel.IsEditingContent && 
                   ContainerViewModel.SubSelectedContentItems.Count == 1) {
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

        public bool IsCompositeChild {
            get {
                if(CopyItem == null || HostClipTileViewModel == null) {
                    return false;
                }
                return CopyItem.CompositeParentCopyItemId > 0 || ContainerViewModel.Count > 1;
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
        public event EventHandler OnClearHyperlinksRequest;
        public event EventHandler OnCreateHyperlinksRequest;
        public event EventHandler OnSyncModels;
        #endregion

        #region ContentItem Overrides
        public override Size GetExpandedSize() {
            var ds = ItemFlowDocument.GetDocumentSize();
            return new Size(
                Math.Max(ds.Width,MpMeasurements.Instance.ClipTileContentMinMaxWidth),
                Math.Max(ds.Height,MpMeasurements.Instance.ClipTileContentHeight)
                );
        }

        public override Size GetUnexpandedSize() {
            double h = ContainerViewModel.ItemViewModels.Count > 1 ?
                            MpMeasurements.Instance.RtbCompositeItemMinHeight :
                            MpMeasurements.Instance.ClipTileContentHeight;

            return new Size(MpMeasurements.Instance.ClipTileContentMinWidth, h);
        }

        #endregion

        #region Public Methods
        public MpRtbItemViewModel() : this(null,null) { }

        public MpRtbItemViewModel(MpContentContainerViewModel ccvm, MpCopyItem ci) : base(ccvm,ci) {
            TemplateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel(this);
            
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
                                  rtbvm.ContainerViewModel.ItemViewModels.Count == 1) {
                            //rtbvm.HostClipTileViewModel.IsSelected = false;
                        }
                        if (rtbvm.IsSubSelected) {
                            if (rtbvm.HostClipTileViewModel.IsExpanded) {
                                foreach (var ortbvm in rtbvm.ContainerViewModel.ItemViewModels) {
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
                            rtbvm.ContainerViewModel.RequestUiUpdate();
                        }
                        break;
                    case nameof(rtbvm.IsSubHovering):
                        if(rtbvm.IsSubHovering) {
                            if (MpClipTrayViewModel.Instance.IsScrolling) {
                                rtbvm.IsSubHovering = false;
                            }
                            foreach (var ortbvm in rtbvm.ContainerViewModel.ItemViewModels) {
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
                    if (rtbvm.ContainerViewModel.ItemViewModels.IndexOf(rtbvm) == 0) {
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
            OnClearHyperlinksRequest?.Invoke(this, null);
        }

        public void RequestCreateHyperlinks() {
            OnCreateHyperlinksRequest?.Invoke(this, null);
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
            var nrtbvm = new MpRtbItemViewModel(ContainerViewModel, CopyItem);
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
            ContainerViewModel.RemoveRange(new List<MpCopyItem> { CopyItem });
            
            
            if (!isMerge) {
                var scvmToRemoveList = new List<MpShortcutViewModel>();
                foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList()) {
                    scvmToRemoveList.Add(scvmToRemove);
                }
                foreach (var scvmToRemove in scvmToRemoveList) {
                    MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
                }
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
