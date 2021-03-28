using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpRtbListBoxItemRichTextBoxViewModel : /*MpUndoableViewModelBase<MpRtbListBoxItemRichTextBoxViewModel>*/ MpClipTileViewModel, ICloneable, IDisposable {
        #region Private Variables
        private int _detailIdx = 1;
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
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(Next));
                    OnPropertyChanged(nameof(Previous));
                    OnPropertyChanged(nameof(IsCompositeChild));
                }
            }
        }

        public new MpClipTileRichTextBoxViewModelCollection RichTextBoxViewModelCollection {
            get {
                if (HostClipTileViewModel == null) {
                    return null;
                } 
                return HostClipTileViewModel.RichTextBoxViewModelCollection;
            }
        }

        private MpTemplateHyperlinkCollectionViewModel _templateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel();
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
        #endregion

        #region Controls 
        public RichTextBox Rtb;

        public DockPanel RtbListBoxItemOverlayDockPanel;

        public MpClipBorder RtbListBoxItemClipBorder;

        public TextBlock RtbListBoxItemTitleTextBlock;

        public TextBox RtbListBoxItemTitleTextBox;

        public Button DragButton { get; set; }

        private Canvas _rtbc;
        public Canvas Rtbc {
            get {
                return _rtbc;
            }
            set {
                if(_rtbc != value) {
                    _rtbc = value;
                    OnPropertyChanged(nameof(Rtbc));
                }
            }
        }

        public AdornerLayer RtbcAdornerLayer;
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
        public Rect ItemRect {
            get {
                return RichTextBoxViewModelCollection.GetListBoxItemRect(RichTextBoxViewModelCollection.IndexOf(this));
                //if (Rtbc == null || Rtbc.Parent == null) {
                //    return new Rect(new Point(double.MinValue, double.MinValue), new Size());
                //}
                //return Rtbc.TransformToAncestor((Visual)Rtbc.Parent).TransformBounds(LayoutInformation.GetLayoutSlot(Rtbc));

                //if (Rtbc != null || 
                //    HostClipTileViewModel.RichTextBoxViewModelCollection.RichTextBoxListBox != null ||
                //    HostClipTileViewModel.RichTextBoxViewModelCollection.Contains(this)) {
                //    return Rtbc.TransformToVisual(HostClipTileViewModel.RichTextBoxViewModelCollection.RichTextBoxListBox).TransformBounds(LayoutInformation.GetLayoutSlot(Rtbc));
                //}
                //return new Rect();
            }
        }

        public double DragButtonSize {
            get {
                return MpMeasurements.Instance.RtbCompositeDragButtonSize;
            }
        }

        public double RtbListBoxItemTitleFontSize {
            get {
                return MpMeasurements.Instance.RtbCompositeItemTitleFontSize;
            }
        }

        public Thickness RtbPadding {
            get {
                if (HostClipTileViewModel == null) {
                    return new Thickness(0);
                }
                var mm = MpMeasurements.Instance.RtbEditModeMinMargin;

                if(SubItemOverlayVisibility == Visibility.Visible) {
                    return new Thickness(DragButtonSize + mm, RtbListBoxItemTitleFontSize + mm, mm, 0);
                }
                if(IsEditingContent) {
                    return new Thickness(0);
                }
                return new Thickness(mm);
                //if (!IsCompositeChild) {
                //    return new Thickness(mm);
                //}
                //if (!HostClipTileViewModel.IsExpanded) {
                //    if (HostClipTileViewModel.IsSelected) {
                //        if (IsSubHovering) {
                //            return new Thickness(DragButtonSize + mm, RtbListBoxItemTitleFontSize + mm, mm, 0);
                //        }
                //    }
                //} else if (!IsSubSelected) {
                //    return new Thickness(DragButtonSize + mm, RtbListBoxItemTitleFontSize + mm, mm, 0);
                //}
                //return new Thickness(mm);
            }
        }

        public double RtbWidth {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    return Math.Max(
                        RtbPageWidth + RtbPadding.Left + RtbPadding.Right,
                        HostClipTileViewModel.TileContentWidth);
                }
                return RtbCanvasWidth - 23;
            }
        }

        public double RtbHeight {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    if (RichTextBoxViewModelCollection.Count == 1) {
                        return Math.Max(
                            RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom,
                            RichTextBoxViewModelCollection.RtbListBoxHeight);
                    }
                    //if(IsSubSelected) {
                    //    return Math.Max(RtbPageHeight+20, MpMeasurements.Instance.RtbCompositeItemMinHeight);
                    //}
                    //if (IsSubHovering) {
                    //    return MpMeasurements.Instance.RtbCompositeItemMinHeight + RtbPadding.Top + RtbPadding.Bottom;
                    //}
                    //return MpMeasurements.Instance.RtbCompositeItemMinHeight;
                    return Math.Max(RtbPageHeight + 20, MpMeasurements.Instance.RtbCompositeItemMinHeight);
                }
                if (RichTextBoxViewModelCollection.Count == 1 && SubItemOverlayVisibility != Visibility.Visible) {
                    if(Rtb == null) {
                        return CopyItemRichText.ToFlowDocument().GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;//HostClipTileViewModel.TileContentHeight - RtbPadding.Top - RtbPadding.Bottom;
                    } else {
                        return Rtb.Document.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;//HostClipTileViewModel.TileContentHeight - RtbPadding.Top - RtbPadding.Bottom;
                    }
                }
                return MpMeasurements.Instance.RtbCompositeItemMinHeight;
            }
        }

        private Size _rtbDocumentSize = new Size();
        public Size RtbDocumentSize {
            get {
                return _rtbDocumentSize;
            }
            set {
                if(_rtbDocumentSize != value) {
                    _rtbDocumentSize = value;
                    OnPropertyChanged(nameof(RtbDocumentSize));
                }
            }
        }

        public double RtbPageWidth {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    
                    if (Rtb == null) {
                        double curFontSize = 16;
                        var width = Math.Max(
                        CopyItemRichText.ToFlowDocument().GetDocumentSize().Width + (curFontSize * 3),
                        RtbCanvasWidth + (curFontSize * 3));

                        return width;// - (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
                    } else {
                        var fs = Rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                        double curFontSize = fs.GetType() == typeof(double) ? (double)fs : 16;
                        var width = Math.Max(
                        Rtb.Document.GetDocumentSize().Width + (curFontSize * 3),
                        Rtbc.ActualWidth /*RtbCanvasWidth*/ + (curFontSize * 3));

                        return width;// - (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
                    }
                    
                }
                return RtbWidth - RtbPadding.Left - RtbPadding.Right;
            }
        }

        public double RtbPageHeight {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    if(Rtb == null) {
                        return CopyItemRichText.ToFlowDocument().GetDocumentSize().Height;
                    } else {
                        return Rtb.Document.GetDocumentSize().Height;
                    }
                }
                return RtbHeight - RtbPadding.Top - RtbPadding.Bottom;
            }
        }

        public double RtbCanvasWidth {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                return HostClipTileViewModel.TileContentWidth - MpMeasurements.Instance.ClipTileSubItemOverlayMargin;
                //return HostClipTileViewModel.TileContentWidth + RtbMargin.Left + RtbMargin.Right;
            }
        }

        public double RtbCanvasHeight {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                return RtbHeight;
            }
        }

        public double RtbRelativeWidthMax {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                var doc = Rtb == null ? CopyItem.ItemFlowDocument : Rtb.Document;
                return doc.GetDocumentSize().Width;
            }
        }
        #endregion

        #region Brushes
        public Brush RtbListBoxItemBorderBrush {
            get {
                if (!IsCompositeChild) {
                    return Brushes.Transparent;
                }
                if (IsPrimarySubSelected) {
                    return Brushes.Blue;
                }
                if (IsSubSelected) {
                    return Brushes.Red;
                }
                if (IsSubHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        public Brush RtbListBoxItemBackgroundColor {
            get {
                //if (CopyItem == null || HostClipTileViewModel == null) {
                //    return Brushes.Transparent;
                //}
                //if (!IsCompositeChild) {
                //    return Brushes.Transparent;
                //}
                //if (IsSubSelected && HostClipTileViewModel.IsExpanded) {
                //    return Brushes.Transparent;
                //}
                //if (IsSubHovering || 
                //    (HostClipTileViewModel.IsExpanded && !IsSubSelected && IsSubHovering) ||
                //    (!HostClipTileViewModel.IsExpanded && IsSubSelected)) {
                //    var scb = CopyItemColorBrush;
                //    scb.Opacity = 0.25;
                //    return scb;
                //}
                //return Brushes.Transparent;
                if(SubItemOverlayVisibility == Visibility.Visible) {
                    var scb = CopyItemColorBrush;
                    scb.Opacity = 0.25;
                    return scb;
                }
                return Brushes.Transparent;
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    //CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public Brush RtbListBoxItemTitleTextBlockTextColor {
            get {
                if (!IsCompositeChild) {
                    return Brushes.Transparent;
                }
                if (IsHoveringOnTitleTextBlock) {
                    return Brushes.DarkGray;
                }
                return Brushes.Black;
            }
        }

        public Brush RtbOverlayBorderBrush {
            get {
                if (HostClipTileViewModel == null) {
                    return Brushes.Transparent;
                }
                if (!HostClipTileViewModel.IsExpanded) {
                    return Brushes.Transparent;
                }
                if (IsSubSelected) {
                    return Brushes.Red;
                }
                if (IsSubHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Visibility
        public Visibility SubItemToolTipVisibility {
            get {
                if (CopyItem == null) {
                    return Visibility.Collapsed;
                }
                return (MainWindowViewModel.ClipTrayViewModel.IsScrolling || IsSubSelected) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility SubItemOverlayVisibility {
            get {
                if (HostClipTileViewModel == null) {
                    return Visibility.Collapsed;
                }
                if (IsEditingSubTitle) {
                    return Visibility.Visible;
                }
                if(HostClipTileViewModel.IsClipDropping && (!HostClipTileViewModel.IsAnySubItemDragging || IsClipDragging)) {
                    return Visibility.Visible;
                }
                if(!IsCompositeChild) {
                    return Visibility.Collapsed;
                }
                if(IsSubSelected && !HostClipTileViewModel.IsExpanded) {
                    return Visibility.Visible;
                }
                if (IsSubHovering && (!IsSubSelected || !HostClipTileViewModel.IsEditingTile)) {
                    return Visibility.Visible;
                }
                if (IsSubSelected && RichTextBoxViewModelCollection.SubSelectedRtbvmList.Count > 1) {
                    return Visibility.Visible;
                }                
                return Visibility.Collapsed;
            }
        }

        public Visibility RtbListBoxItemTitleTextBlockVisibility {
            get {
                if (IsEditingSubTitle) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility RtbListBoxItemTitleTextBoxVisibility {
            get {
                if (IsEditingSubTitle) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Header & Footer 
        public MpRtbListBoxItemRichTextBoxViewModel Next {
            get {
                if (HostClipTileViewModel == null ||
                   RichTextBoxViewModelCollection == null ||
                   RichTextBoxViewModelCollection.Count <= 1) {
                    return null;
                }
                int nextIdx = CompositeSortOrderIdx + 1;
                if (nextIdx > RichTextBoxViewModelCollection.Count) {
                    return null;
                }
                return RichTextBoxViewModelCollection[nextIdx-1];
            }
        }

        public MpRtbListBoxItemRichTextBoxViewModel Previous {
            get {
                if (HostClipTileViewModel == null ||
                   RichTextBoxViewModelCollection == null ||
                   RichTextBoxViewModelCollection.Count <= 1) {
                    return null;
                }
                int prevIdx = CompositeSortOrderIdx - 1;
                if (prevIdx < 1) {
                    return null;
                }
                return RichTextBoxViewModelCollection[prevIdx-1];
            }
        }
        #endregion 

        #region Business Logic 
        private DateTime _lastSubSelectedDateTime;
        public DateTime LastSubSelectedDateTime {
            get {
                return _lastSubSelectedDateTime;
            }
            set {
                if (_lastSubSelectedDateTime != value) {
                    _lastSubSelectedDateTime = value;
                    OnPropertyChanged(nameof(LastSubSelectedDateTime));
                }
            }
        }

        private string _shortcutKeyString = string.Empty;
        public new string ShortcutKeyString {
            get {
                return _shortcutKeyString;
            }
            set {
                if (_shortcutKeyString != value) {
                    _shortcutKeyString = value;
                    OnPropertyChanged(nameof(ShortcutKeyString));
                }
            }
        }

        public new bool HasTemplate {
            get {
                return TemplateHyperlinkCollectionViewModel.Count > 0;
            }
        }
        public new string TemplateRichText { get; set; }
        #endregion

        #region State
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

        private bool _isResizingTop = false;
        public bool IsResizingTop {
            get {
                return _isResizingTop;
            }
            set {
                if(_isResizingTop != value) {
                    _isResizingTop = value;
                    OnPropertyChanged(nameof(IsResizingTop));
                    OnPropertyChanged(nameof(RtbListBoxItemCursor));
                }
            }
        }

        private bool _isResizingBottom = false;
        public bool IsResizingBottom {
            get {
                return _isResizingBottom;
            }
            set {
                if (_isResizingBottom != value) {
                    _isResizingBottom = value;
                    OnPropertyChanged(nameof(IsResizingBottom));
                    OnPropertyChanged(nameof(RtbListBoxItemCursor));
                }
            }
        }

        private bool _isHoveringOverTop = false;
        public bool IsHoveringOverTop {
            get {
                return _isHoveringOverTop;
            }
            set {
                if (_isHoveringOverTop != value) {
                    _isHoveringOverTop = value;
                    OnPropertyChanged(nameof(IsHoveringOverTop));
                    OnPropertyChanged(nameof(RtbListBoxItemCursor));
                }
            }
        }

        private bool _isHoveringOverBottom = false;
        public bool IsHoveringOverBottom {
            get {
                return _isHoveringOverBottom;
            }
            set {
                if (_isHoveringOverBottom != value) {
                    _isHoveringOverBottom = value;
                    OnPropertyChanged(nameof(IsHoveringOverBottom));
                    OnPropertyChanged(nameof(RtbListBoxItemCursor));
                }
            }
        }

        private bool _isHoveringOnTitleTextBlock = false;
        public bool IsHoveringOnTitleTextBlock {
            get {
                return _isHoveringOnTitleTextBlock;
            }
            set {
                if (_isHoveringOnTitleTextBlock != value) {
                    _isHoveringOnTitleTextBlock = value;
                    OnPropertyChanged(nameof(IsHoveringOnTitleTextBlock));
                    OnPropertyChanged(nameof(RtbListBoxItemTitleTextBlockTextColor));
                    if(IsHoveringOnTitleTextBlock) {
                        OnPropertyChanged(nameof(DetailText));
                    }
                }
            }
        }

        private bool _isPrimarySubSelected = false;
        public bool IsPrimarySubSelected {
            get {
                return _isPrimarySubSelected;
            }
            set {
                if (_isPrimarySubSelected != value) {
                    _isPrimarySubSelected = value;
                    OnPropertyChanged(nameof(IsPrimarySubSelected));
                    OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
                }
            }
        }

        private bool _isSubSelected = false;
        public bool IsSubSelected {
            get {
                return _isSubSelected;
            }
            set {
                //if (_isSubSelected != value && 
                //    (HostClipTileViewModel.IsExpanded || (IsSubSelected && value == false) || IsDragging)) {
                if (_isSubSelected != value) { 
                    _isSubSelected = value;
                    OnPropertyChanged(nameof(IsSubSelected));
                    OnPropertyChanged(nameof(IsEditingContent));
                    OnPropertyChanged(nameof(SubItemOverlayVisibility));
                    OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
                    OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
                    OnPropertyChanged(nameof(RtbOverlayBorderBrush));
                    OnPropertyChanged(nameof(RtbPadding));
                    OnPropertyChanged(nameof(RtbListBoxItemCursor));
                    OnPropertyChanged(nameof(RtbPageWidth));
                    OnPropertyChanged(nameof(RtbPageHeight));
                    OnPropertyChanged(nameof(SubItemToolTipVisibility));
                }
            }
        }

        public new bool IsSelected {
            get {
                return IsSubSelected;
            }
            set {
                IsSubSelected = value;
            }
        }

        private bool _isSubHovering = false;
        public bool IsSubHovering {
            get {
                return _isSubHovering;
            }
            set {
                if (_isSubHovering != value) {
                    _isSubHovering = value;
                    OnPropertyChanged(nameof(IsSubHovering));
                    OnPropertyChanged(nameof(SubItemOverlayVisibility));
                    OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
                    OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
                    OnPropertyChanged(nameof(RtbListBoxItemCursor));
                    OnPropertyChanged(nameof(RtbPadding));
                    OnPropertyChanged(nameof(RtbCanvasHeight));
                    OnPropertyChanged(nameof(RtbCanvasWidth));
                    OnPropertyChanged(nameof(RtbCanvasHeight));
                    OnPropertyChanged(nameof(RtbPageWidth));
                    OnPropertyChanged(nameof(RtbPageHeight));
                    OnPropertyChanged(nameof(RtbOverlayBorderBrush));
                }
            }
        }

        private bool _isEditingTitle = false;
        public bool IsEditingSubTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if (_isEditingTitle != value) {
                    _isEditingTitle = value;
                    OnPropertyChanged(nameof(IsEditingSubTitle));
                    OnPropertyChanged(nameof(RtbListBoxItemTitleTextBlockVisibility));
                    OnPropertyChanged(nameof(RtbListBoxItemTitleTextBoxVisibility));
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        private bool _isEditingContent = false;
        public bool IsEditingContent {
            get {
                return _isEditingContent;
            }
            set {
                if (_isEditingContent != value) {
                    _isEditingContent = value;
                    OnPropertyChanged(nameof(IsEditingContent));
                    OnPropertyChanged(nameof(SubItemOverlayVisibility));
                }
            }
        }

        private bool _isPastingTemplate = false;
        public new bool IsPastingTemplate {
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
                if(CopyItem == null) {
                    return false;
                }
                //return RichTextBoxViewModelCollection.Count > 1;
                return CopyItem.IsSubCompositeItem;
            }
        }
        #endregion

        #region Model
        public new BitmapSource CopyItemBmp {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.ItemBitmapSource;
            }
            set {
                if (CopyItem.ItemBitmapSource != value) {
                    CopyItem.ItemBitmapSource = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public new string DetailText {
            get {
                if(CopyItem == null) {
                    return string.Empty;
                }
                _detailIdx++;
                if(_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                    _detailIdx = 1;
                }
                return CopyItem.GetDetail((MpCopyItemDetailType)_detailIdx);
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
                    OnPropertyChanged(nameof(CopyItem));
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
                    OnPropertyChanged(nameof(CopyItem));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
                }
            }
        }

        public new int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CopyItemId;
            }
        }

        public new string CopyItemPlainText {
            get {
                if (CopyItem == null || CopyItem.ItemPlainText == null) {
                    return string.Empty;
                }
                return CopyItem.ItemPlainText;
            }
        }

        public new string CopyItemRichText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(CopyItem.ItemRichText)) {
                    return string.Empty.ToRichText();
                }
                return CopyItem.ItemRichText;
            }
            set {
                if (CopyItem != null && CopyItem.ItemRichText != value) {
                    //value should be raw rtf where templates are encoded into #name#color# groups
                    CopyItem.SetData(value);
                    CopyItem.WriteToDatabase();                    
                    OnPropertyChanged(nameof(CopyItemRichText));
                }
            }
        }

        private string _copyItemFilePath = string.Empty;
        public string CopyItemFilePath {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(_copyItemFilePath)) {
                    _copyItemFilePath = CopyItem.GetFileList()[0];
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

        public new string CopyItemTitle {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.Title;
            }
            set {
                if (CopyItem != null && CopyItem.Title != value) {
                    AddUndo(this, nameof(CopyItemTitle), CopyItem.Title, value);
                    CopyItem.Title = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public new List<string> CopyItemFileDropList {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return new List<string>();
                }
                return CopyItem.GetFileList(string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
            }
        }

        public Brush CopyItemColorBrush {
            get {
                if(CopyItem == null || CopyItem.ItemColor == null) {
                    return Brushes.Transparent;
                }
                return CopyItem.ItemColor.ColorBrush;
            }
            set {
                if(CopyItem.ItemColor.ColorBrush != value) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    CopyItem.InitSwirl(null, true);
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        private MpCopyItem _copyItem = null;
        public new MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                //if(_copyItem != value) 
                    {
                    _copyItem = value;
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CopyItemId));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(CopyItemFilePath));
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CopyItemPlainText));
                    OnPropertyChanged(nameof(CopyItemTitle));
                    OnPropertyChanged(nameof(CopyItemBmp));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpRtbListBoxItemRichTextBoxViewModel() : this(null,null) { }

        public MpRtbListBoxItemRichTextBoxViewModel(MpClipTileViewModel ctvm, MpCopyItem ci) : base() {
            CopyItem = ci;
            HostClipTileViewModel = ctvm;
            TemplateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel(HostClipTileViewModel, this);            

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsSubSelected):
                        //if(!IsSubSelected && IsEditingSubTitle) {
                        //    IsEditingSubTitle = false;
                        //}
                        if(IsSubSelected) {
                            if (HostClipTileViewModel.IsExpanded) {
                                foreach (var rtbvm in RichTextBoxViewModelCollection) {
                                    if (rtbvm != this) {
                                        rtbvm.IsSubSelected = false;
                                        rtbvm.IsEditingSubTitle = false;
                                    }
                                }
                                ResetRtb();
                                //RichTextBoxViewModelCollection.Refresh();
                            } else if(!MpHelpers.Instance.IsMultiSelectKeyDown() && !IsSubDragging && !HostClipTileViewModel.IsContextMenuOpened) {
                                IsSubSelected = false;
                            }
                            if (HostClipTileViewModel.IsEditingTile) {
                                HostClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(Rtb, false);
                            }
                            if (HostClipTileViewModel.IsPastingTemplate) {
                                HostClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(Rtb, false);
                            }
                            

                            LastSubSelectedDateTime = DateTime.Now;                            
                        } else if(HostClipTileViewModel.IsExpanded) {
                            // triggers set data in model which updates the preview
                            CopyItemRichText = Rtb.Document.ToRichText();
                        }
                        if(HostClipTileViewModel.IsExpanded) {
                            UpdateLayout();
                        }
                        //RichTextBoxViewModelCollection.OnPropertyChanged(nameof(RichTextBoxViewModelCollection.SubSelectedRtbvmList));                        
                        break;
                    case nameof(IsSubHovering):
                        if(IsSubHovering) {
                            if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                                IsHovering = false;
                                IsSubHovering = false;
                            }
                            foreach (var rtbvm in RichTextBoxViewModelCollection) {
                                if (rtbvm != this) {
                                    rtbvm.IsSubHovering = false;
                                }
                            }
                            UpdateLayout();
                        }
                        //RichTextBoxViewModelCollection.OnPropertyChanged(nameof(RichTextBoxViewModelCollection.RtbListBoxHeight));
                        break;
                }
            };
        }

        public void ClipTileRichTextBoxListItemCanvas_Loaded(object sender, RoutedEventArgs e) {
            //if (Rtbc != null || ((Canvas)sender).DataContext.GetType().ToString() == "MS.Internal.NamedObject") {
            //    return;
            //}
            Rtbc = (Canvas)sender;
            Rtb = (RichTextBox)Rtbc.FindName("RtbListBoxItemRichTextBox");
            RtbListBoxItemClipBorder = (MpClipBorder)Rtbc.FindName("RtbListBoxItemOverlayBorder");
            RtbListBoxItemOverlayDockPanel = (DockPanel)Rtbc.FindName("RtbListItemOverlayBorderDockPanel");
            RtbListBoxItemTitleTextBlock = (TextBlock)Rtbc.FindName("RtbListBoxItemTitleTextBlock");
            RtbListBoxItemTitleTextBox = (TextBox)Rtbc.FindName("RtbListBoxItemTitleTextBox");
            DragButton = (Button)Rtbc.FindName("RtbListBoxItemOverlayBorderGridDragButton");

            HostClipTileViewModel.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);

            Rtb.TextChanged += (s, e44) => {
                UpdateLayout();
            };

            if (HasTemplate) {
                ClearHyperlinks();
            }
            CreateHyperlinks();

            #region Drag & Drop
            var mouseDownPosition = new Point();
            int minDragDist = 20;
            DragButton.PreviewMouseUp += (s, e9) => {
                mouseDownPosition = new Point();
                IsSubDragging = false;
                foreach (var srtbvm in RichTextBoxViewModelCollection.SubSelectedRtbvmList) {
                    if (srtbvm != this && srtbvm.IsSubSelected) {
                        var outerBorder = (Border)srtbvm.DragButton.Template.FindName("OuterBorder", srtbvm.DragButton);
                        outerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF7CA0CC");

                        var innerBorder = (Border)srtbvm.DragButton.Template.FindName("InnerBorder", srtbvm.DragButton);
                        innerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FFE4EFFD");
                        innerBorder.Background = (Brush)new BrushConverter().ConvertFromString("#FFDAE7F5");
                    }
                }
            };
            DragButton.PreviewMouseMove += (s, e7) => {
                if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                    if(HostClipTileViewModel.IsExpanded && RichTextBoxViewModelCollection.Count == 1) {
                        //cannot resort w/ only 1 item and its relative location is not clear
                        //since its isolated
                        return;
                    }
                    foreach(var srtbvm in RichTextBoxViewModelCollection.SubSelectedRtbvmList) {
                        if(srtbvm.IsSubSelected && srtbvm != this) {
                            var outerBorder = (Border)srtbvm.DragButton.Template.FindName("OuterBorder", srtbvm.DragButton);
                            outerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF2E4E76");

                            var innerBorder = (Border)srtbvm.DragButton.Template.FindName("InnerBorder", srtbvm.DragButton);
                            innerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF116EE4");
                            innerBorder.Background = (Brush)new BrushConverter().ConvertFromString("#FF3272B8");
                        }
                    }
                    if(mouseDownPosition == new Point()) {
                        mouseDownPosition = e7.GetPosition(Rtbc);
                    }
                    if (MpHelpers.Instance.DistanceBetweenPoints(mouseDownPosition, e7.GetPosition(Rtbc)) < minDragDist) {
                        return;
                    }
                    IsSubDragging = true;
                    IsSubSelected = true;
                    DragDrop.DoDragDrop(
                                Rtbc,
                                RichTextBoxViewModelCollection.GetDataObjectFromSubSelectedItems(true).Result,
                                DragDropEffects.Copy | DragDropEffects.Move);
                    e.Handled = true;
                }
            };
            DragButton.MouseEnter += (s, e2) => {
                IsOverDragButton = true;
                if(IsSubSelected) {
                    foreach (var srtbvm in RichTextBoxViewModelCollection.SubSelectedRtbvmList) {
                        if (srtbvm != this && srtbvm.IsSubSelected) {
                            var outerBorder = (Border)srtbvm.DragButton.Template.FindName("OuterBorder", srtbvm.DragButton);
                            outerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FF7CA0CC");

                            var innerBorder = (Border)srtbvm.DragButton.Template.FindName("InnerBorder", srtbvm.DragButton);
                            innerBorder.BorderBrush = (Brush)new BrushConverter().ConvertFromString("#FFE4EFFD");
                            innerBorder.Background = (Brush)new BrushConverter().ConvertFromString("#FFDAE7F5");
                        }
                    }
                }
            };
            DragButton.MouseLeave += (s, e2) => {
                IsOverDragButton = false;
                if (IsSubSelected) {
                    foreach (var srtbvm in RichTextBoxViewModelCollection.SubSelectedRtbvmList) {
                        if (srtbvm != this && srtbvm.IsSubSelected) {
                            var outerBorder = (Border)srtbvm.DragButton.Template.FindName("OuterBorder", srtbvm.DragButton);
                            outerBorder.BorderBrush = Brushes.Transparent;

                            var innerBorder = (Border)srtbvm.DragButton.Template.FindName("InnerBorder", srtbvm.DragButton);
                            innerBorder.BorderBrush = Brushes.Transparent;
                            innerBorder.Background = Brushes.Transparent;
                        }
                    }
                }
            };

            #endregion


            if (HostClipTileViewModel.WasAddedAtRuntime) {
                //force new items to have left alignment
                Rtb.SelectAll();
                Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                Rtb.CaretPosition = Rtb.Document.ContentStart;
            }

            RtbcAdornerLayer = AdornerLayer.GetAdornerLayer(Rtbc);
            RtbcAdornerLayer.Add(new MpRichTextBoxOverlayAdorner(Rtbc));

            Rtbc.MouseEnter += (s, e2) => {
                IsSubHovering = true;
            };
            Rtbc.MouseLeave += (s, e2) => {
                IsSubHovering = false;
            };

            Rtbc.PreviewMouseRightButtonDown += (s, e6) => {
                e6.Handled = true;
                HostClipTileViewModel.IsContextMenuOpened = true;
                IsSubSelected = true;
                var contextEvent = new RoutedEventArgs(
                    MpClipBorder.ContextMenuOpeningEvent,
                    HostClipTileViewModel.ClipBorder);
                //var rightClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Right);
                HostClipTileViewModel.ClipTile_ContextMenu_Opened(HostClipTileViewModel.ClipBorder.FindName("ClipTile_ContextMenu"), contextEvent);
            };
           
            RtbListBoxItemTitleTextBlock.PreviewMouseLeftButtonDown += (s, e7) => {
                //RichTextBoxViewModelCollection.ClearSubSelection();
                if(!HostClipTileViewModel.IsExpanded) {
                    IsSubSelected = true;
                }
                IsEditingSubTitle = true;
                e7.Handled = true;
            };

            RtbListBoxItemTitleTextBlock.MouseEnter += (s, e5) => {
                IsHoveringOnTitleTextBlock = true;
            };
            RtbListBoxItemTitleTextBlock.MouseLeave += (s, e5) => {
                IsHoveringOnTitleTextBlock = false;
            };

            RtbListBoxItemTitleTextBox.IsVisibleChanged += (s, e9) => {
                if (RtbListBoxItemTitleTextBoxVisibility == Visibility.Collapsed) {
                    CopyItemTitle = RtbListBoxItemTitleTextBox.Text;
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                tbx.SelectAll();
            };

            RtbListBoxItemTitleTextBox.LostFocus += (s, e4) => {
                IsEditingSubTitle = false;
            };
            RtbListBoxItemTitleTextBox.PreviewKeyDown += (s, e5) => {
                if (e5.Key == Key.Enter || e5.Key == Key.Escape) {
                    IsEditingSubTitle = false;
                }
            };

            //UpdateLayout();
        }

        public void UpdateLayout() {
            OnPropertyChanged(nameof(SubItemOverlayVisibility));
            OnPropertyChanged(nameof(RtbPadding));
            OnPropertyChanged(nameof(RtbHeight));
            OnPropertyChanged(nameof(RtbCanvasHeight));
            OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));

            if(Rtb != null) {
                Rtb.Document.PageWidth = RtbPageWidth;
                Rtb.Document.PageHeight = RtbPageHeight;
            }
            //ResetRtb();
            
            RichTextBoxViewModelCollection.UpdateLayout();


            //double widthDiff = RtbPageWidth - RichTextBoxViewModelCollection.RichTextBoxListBox.ActualWidth;

            //HostClipTileViewModel.TileBorderWidth += widthDiff;
            //HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileBorderWidth));
            //HostClipTileViewModel.TileContentWidth += widthDiff;


            //HostClipTileViewModel.ClipBorder.Width += widthDiff;            
        }

        public void ResetRtb() {
            if (Rtb == null) {
                return;
            }
            Rtb.Focus();
            Rtb.SelectAll();
            var rtbAlignment = Rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty);
            if (rtbAlignment == null ||
                rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}" ||
                (TextAlignment)rtbAlignment == TextAlignment.Justify) {
                Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
            }


            Rtb.ScrollToHome();
            Rtb.CaretPosition = Rtb.Document.ContentStart;
            Rtb.Selection.Select(Rtb.Document.ContentStart, Rtb.Document.ContentStart);

            UpdateLayout();
        }

        public new async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                TemplateRichText = string.Empty;
                //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(RichTextBoxViewModelCollection.IndexOf(this),false);
                
                await Task.Run(() => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        System.Threading.Thread.Sleep(500);
                    }
                    //TemplateRichText is set in PasteTemplateCommand
                });

                return TemplateRichText;
            }
            return CopyItemRichText;

            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        #region Hyperlinks
        public void ClearHyperlinks() {
            var rtbSelection = Rtb?.Selection;
            var hlList = GetHyperlinkList();
            foreach (var hl in hlList) {
                string linkText = string.Empty;
                if (hl.DataContext == null || hl.DataContext is MpRtbListBoxItemRichTextBoxViewModel) {
                    linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
                } else {
                    var thlvm = (MpTemplateHyperlinkViewModel)hl.DataContext;
                    linkText = thlvm.TemplateName;
                }
                hl.Inlines.Clear();
                new Span(new Run(linkText), hl.ContentStart);
            }
            TemplateHyperlinkCollectionViewModel.Clear();
            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }

        public void CreateHyperlinks() {
            if(Rtb == null) {
                return;
            }
            var regExGroupList = new List<string> {
                //WebLink
                @"(?:https?://|www\.)\S+", 
                //Email
                @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
                //PhoneNumber
                @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
                //Currency
                @"[$|£|€|¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?",
                //HexColor (no alpha)
                @"#([0-9]|[a-fA-F]){6}",
                //StreetAddress
                @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
                //Text Template (dynamically matching from CopyItemTemplate.TemplateName)
                CopyItem.TemplateRegExMatchString,                
                //HexColor (with alpha)
                @"#([0-9]|[a-fA-F]){8}",
            };
            //var docPlainText = new TextRange(Rtb.Document.ContentStart, Rtb.Document.ContentEnd).Text;

            var rtbSelection = Rtb?.Selection.Clone();
            for (int i = 0; i < regExGroupList.Count; i++) {
                var linkType = i + 1 > (int)MpSubTextTokenType.TemplateSegment ? MpSubTextTokenType.HexColor : (MpSubTextTokenType)(i + 1);                
                if (linkType == MpSubTextTokenType.StreetAddress) {
                    //doesn't consistently work and presents bugs so disabling for now
                    continue;
                }
                var lastRangeEnd = Rtb.Document.ContentStart;
                var regExStr = regExGroupList[i];
                if (string.IsNullOrEmpty(regExStr)) {
                    //this occurs for templates when copyitem has no templates
                    continue;
                }
                if(linkType == MpSubTextTokenType.HexColor) {
                    linkType = MpSubTextTokenType.HexColor;
                }
                var mc = Regex.Matches(CopyItem.ItemPlainText, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            Hyperlink hl = null;
                            var matchRange = MpHelpers.Instance.FindStringRangeFromPosition(lastRangeEnd, c.Value, true);
                            if (matchRange == null || string.IsNullOrEmpty(matchRange.Text)) {
                                continue;
                            }
                            lastRangeEnd = matchRange.End;
                            if (linkType == MpSubTextTokenType.TemplateSegment) {
                                var copyItemTemplate = CopyItem.GetTemplateByName(matchRange.Text);
                                hl = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(HostClipTileViewModel, copyItemTemplate, matchRange);
                                TemplateHyperlinkCollectionViewModel.Add((MpTemplateHyperlinkViewModel)hl.DataContext);
                            } else {
                                var matchRun = new Run(matchRange.Text);
                                matchRange.Text = "";
                                // DO NOT REMOVE this extra link ensures selection is retained!
                                var hlink = new Hyperlink(matchRun, matchRange.Start);
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                var linkText = c.Value;
                                hl.Tag = linkType;
                                MpHelpers.Instance.CreateBinding(HostClipTileViewModel, new PropertyPath(nameof(HostClipTileViewModel.IsSelected)), hl, Hyperlink.IsEnabledProperty);
                                hl.MouseEnter += (s3, e3) => {
                                    hl.Cursor = HostClipTileViewModel.IsSelected ? Cursors.Hand : Cursors.Arrow;
                                };
                                hl.MouseLeave += (s3, e3) => {
                                    hl.Cursor = Cursors.Arrow;
                                };
                                hl.MouseLeftButtonDown += (s4, e4) => {
                                    if (hl.NavigateUri != null && HostClipTileViewModel.IsSelected) {
                                        MpHelpers.Instance.OpenUrl(hl.NavigateUri.ToString());
                                    }
                                };

                                var convertToQrCodeMenuItem = new MenuItem();
                                convertToQrCodeMenuItem.Header = "Convert to QR Code";
                                convertToQrCodeMenuItem.Click += (s5, e1) => {
                                    var hyperLink = (Hyperlink)((MenuItem)s5).Tag;
                                    var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString());
                                    Clipboard.SetImage(bmpSrc);
                                };
                                convertToQrCodeMenuItem.Tag = hl;
                                hl.ContextMenu = new ContextMenu();
                                hl.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                                switch ((MpSubTextTokenType)hl.Tag) {
                                    case MpSubTextTokenType.StreetAddress:
                                        hl.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
                                        break;
                                    case MpSubTextTokenType.Uri:
                                        if (!linkText.Contains("https://")) {
                                            hl.NavigateUri = new Uri("https://" + linkText);
                                        } else {
                                            hl.NavigateUri = new Uri(linkText);
                                        }
                                        MenuItem minifyUrl = new MenuItem();
                                        minifyUrl.Header = "Minify with bit.ly";
                                        minifyUrl.Click += async (s1, e2) => {
                                            Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
                                            string minifiedLink = await MpMinifyUrl.Instance.ShortenUrl(link.NavigateUri.ToString());
                                            if (!string.IsNullOrEmpty(minifiedLink)) {
                                                matchRange.Text = minifiedLink;
                                                ClearHyperlinks();
                                                CreateHyperlinks();
                                            }
                                            //Clipboard.SetText(minifiedLink);
                                        };
                                        minifyUrl.Tag = hl;
                                        hl.ContextMenu.Items.Add(minifyUrl);
                                        break;
                                    case MpSubTextTokenType.Email:
                                        hl.NavigateUri = new Uri("mailto:" + linkText);
                                        break;
                                    case MpSubTextTokenType.PhoneNumber:
                                        hl.NavigateUri = new Uri("tel:" + linkText);
                                        break;
                                    case MpSubTextTokenType.Currency:
                                        try {
                                            //"https://www.google.com/search?q=%24500.80+to+yen"
                                            MenuItem convertCurrencyMenuItem = new MenuItem();
                                            convertCurrencyMenuItem.Header = "Convert Currency To";
                                            var fromCurrencyType = MpHelpers.Instance.GetCurrencyTypeFromString(linkText);
                                            foreach (MpCurrency currency in MpCurrencyConverter.Instance.CurrencyList) {
                                                if (currency.Id == Enum.GetName(typeof(CurrencyType), fromCurrencyType)) {
                                                    continue;
                                                }
                                                MenuItem subItem = new MenuItem();
                                                subItem.Header = currency.CurrencyName + "(" + currency.CurrencySymbol + ")";
                                                subItem.Click += async (s2, e2) => {
                                                    Enum.TryParse(currency.Id, out CurrencyType toCurrencyType);
                                                    var convertedValue = await MpCurrencyConverter.Instance.ConvertAsync(
                                                        MpHelpers.Instance.GetCurrencyValueFromString(linkText),
                                                        fromCurrencyType,
                                                        toCurrencyType);
                                                    convertedValue = Math.Round(convertedValue, 2);
                                                    if (Rtb.Tag != null && ((List<Hyperlink>)Rtb.Tag).Contains(hl)) {
                                                        ((List<Hyperlink>)Rtb.Tag).Remove(hl);
                                                    }
                                                    Run run = new Run(currency.CurrencySymbol + convertedValue);
                                                    hl.Inlines.Clear();
                                                    hl.Inlines.Add(run);
                                                };

                                                convertCurrencyMenuItem.Items.Add(subItem);
                                            }

                                            hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
                                        }
                                        catch (Exception ex) {
                                            Console.WriteLine("Create Hyperlinks warning, cannot connect to currency converter: " + ex);
                                        }
                                        break;
                                    case MpSubTextTokenType.HexColor:
                                        var rgbColorStr = linkText;
                                        if (rgbColorStr.Length > 7) {
                                            rgbColorStr = rgbColorStr.Substring(0, 7);
                                        }
                                        hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);

                                        MenuItem changeColorItem = new MenuItem();
                                        changeColorItem.Header = "Change Color";
                                        changeColorItem.Click += (s, e) => {
                                            var result = MpHelpers.Instance.ShowColorDialog((Brush)new BrushConverter().ConvertFrom(linkText));
                                        };
                                        hl.ContextMenu.Items.Add(changeColorItem);
                                        break;
                                    default:
                                        Console.WriteLine("Unhandled token type: " + Enum.GetName(typeof(MpSubTextTokenType), (MpSubTextTokenType)hl.Tag) + " with value: " + linkText);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }

        private List<Hyperlink> GetHyperlinkList() {
            if(Rtb == null) {
                return new List<Hyperlink>();
            }
            var rtbSelection = Rtb?.Selection;
            var hlList = new List<Hyperlink>();
            for (TextPointer position = Rtb.Document.ContentStart;
                position != null && position.CompareTo(Rtb.Document.ContentEnd) <= 0;
                position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    var hl = MpHelpers.Instance.FindParentOfType(position.Parent, typeof(Hyperlink)) as Hyperlink;
                    if (hl != null && !hlList.Contains(hl)) {
                        hlList.Add(hl);
                    }
                }
            }
            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return hlList;
        }
        #endregion

        #region Clone Implementation
        public object Clone() {
            var nrtbvm = new MpRtbListBoxItemRichTextBoxViewModel(HostClipTileViewModel, CopyItem);
            nrtbvm.Rtb = new RichTextBox();
            nrtbvm.Rtb.Document = Rtb.Document.Clone();
            return nrtbvm;
        }
        #endregion

        #endregion

        #region Commands
        //private RelayCommand _selectItemCommand;
        //public ICommand SelectItemCommand {
        //    get {
        //        if (_selectItemCommand == null) {
        //            _selectItemCommand = new RelayCommand(SelectItem, CanSelectItem);
        //        }
        //        return _selectItemCommand;
        //    }
        //}
        //private bool CanSelectItem() {
        //    return HostClipTileViewModel.IsExpanded && !MainWindowViewModel.ClipTrayViewModel.IsEditingClipTitle;
        //}
        //private void SelectItem() {
        //    SetSelection(true, false, false);
        //}
        #endregion

        #region Overrides
        public new void Dispose() {
            if(RichTextBoxViewModelCollection.Contains(this)) {
                RichTextBoxViewModelCollection.Remove(this);
            }
            Rtb = null;
            RtbListBoxItemOverlayDockPanel = null;
            RtbListBoxItemClipBorder = null;
            RtbListBoxItemTitleTextBlock = null;
            RtbListBoxItemTitleTextBox = null;
            Rtbc = null;
            RtbcAdornerLayer = null;
        }
        #endregion
    }

}
