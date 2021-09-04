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

namespace MpWpfApp {
    [Serializable]
    public class MpRtbListBoxItemRichTextBoxViewModel : MpUndoableViewModelBase<MpRtbListBoxItemRichTextBoxViewModel> , ICloneable, IDisposable {
        #region Private Variables
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
                    OnPropertyChanged(nameof(IsCompositeChild));
                }
            }
        }

        public MpClipTileRichTextBoxViewModelCollection RichTextBoxViewModelCollection {
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

        
        private ObservableCollection<MpContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpContextMenuItemViewModel>();
        public ObservableCollection<MpContextMenuItemViewModel> TagMenuItems {
            get {
                if (MainWindowViewModel == null || MainWindowViewModel.TagTrayViewModel == null) {
                    return _tagMenuItems;
                }
                _tagMenuItems.Clear();
                foreach (var tagTile in MainWindowViewModel.TagTrayViewModel) {
                    if (tagTile.IsSudoTag) {
                        continue;
                    }
                    _tagMenuItems.Add(
                        new MpContextMenuItemViewModel(
                            tagTile.TagName,
                            RichTextBoxViewModelCollection.LinkTagToSubSelectedClipsCommand,
                            tagTile,
                            tagTile.IsLinkedWithRtbItem(this),
                            string.Empty,
                            null,
                            tagTile.ShortcutKeyString,
                            tagTile.Color));
                }
                return _tagMenuItems;
            }
        }
        #endregion

        #region Controls 
        public RichTextBox Rtb { get; set; }

        public DockPanel RtbListBoxItemOverlayDockPanel { get; set; }

        public MpClipBorder RtbListBoxItemClipBorder { get; set; }

        public TextBlock RtbListBoxItemTitleTextBlock { get; set; }

        public TextBox RtbListBoxItemTitleTextBox { get; set; }

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

        public AdornerLayer RtbListBoxItemAdornerLayer { get; set; }

        #endregion

        #region Appearance
        public BitmapSource HotkeyIconSource {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystick.png"));
                }
                return new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/joystickactive.png"));
            }
        }

        public string HotkeyIconTooltip {
            get {
                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                    return @"Assign Shortcut";
                }
                return ShortcutKeyString;
            }
        }
        public BitmapSource AppIcon {
            get {
                if (MainWindowViewModel == null ||
                   MainWindowViewModel.ClipTrayViewModel == null ||
                  !MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp) {
                    if (CopyItem == null) {
                        return new BitmapImage();
                    }
                    if(CopyItemFavIcon != null) {
                        return CopyItemFavIcon;
                    }
                    return CopyItemAppIcon;
                }
                return MainWindowViewModel.ClipTrayViewModel.FilterByAppIcon;
            }
        }
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

        public Rect DragButtonRect {
            get {
                if(DragButton == null || Rtbc == null) {
                    return new Rect();
                }
                return DragButton.TransformToAncestor((Visual)Rtbc).TransformBounds(LayoutInformation.GetLayoutSlot(DragButton));
            }
        }

        public double DragButtonSize {
            get {
                return MpMeasurements.Instance.RtbCompositeDragButtonSize;
            }
        }

        public double SubItemAppIconSize {
            get {
                return MpMeasurements.Instance.RtbCompositeAppIconSize;
            }
        }

        public double SubItemAppIconBorderSize {
            get {
                return MpMeasurements.Instance.RtbCompositeAppIconBorderSize;
            }
        }

        public double SubItemAppIconInnerBorderSize {
            get {
                return MpMeasurements.Instance.RtbCompositeAppIconBorderSize * 0.8;
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
                    return new Thickness(DragButtonSize + mm, RtbListBoxItemTitleFontSize + mm, mm + SubItemAppIconSize, 0);
                }
                if(IsEditingContent) {
                    return new Thickness(0);
                }
                return new Thickness(mm);
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
                return RtbCanvasWidth;// - 23;
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
                    //if (IsSubSelected) {
                    //    return Math.Max(RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom, MpMeasurements.Instance.RtbCompositeItemMinHeight);
                    //}
                    //if (IsSubHovering) {
                    //    return MpMeasurements.Instance.RtbCompositeItemMinHeight + RtbPadding.Top + RtbPadding.Bottom;
                    //}
                    //return MpMeasurements.Instance.RtbCompositeItemMinHeight;
                    //if(Rtbc == null) {
                    //    return Math.Max(RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom, MpMeasurements.Instance.RtbCompositeItemMinHeight);
                    //} else {
                    //    return Math.Max(Rtbc.ActualHeight, MpMeasurements.Instance.RtbCompositeItemMinHeight);

                    //}
                    return Math.Max(RtbPageHeight + RtbPadding.Top + RtbPadding.Bottom, MpMeasurements.Instance.RtbCompositeItemMinHeight);
                }
                if (RichTextBoxViewModelCollection.Count == 1 && SubItemOverlayVisibility != Visibility.Visible) {
                    if (Rtb == null) {
                        var fd = CopyItemRichText.ToFlowDocument();
                        fd.PageWidth = RtbCanvasWidth - RtbPadding.Left - RtbPadding.Right;
                        return fd.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
                    } else {
                        return HostClipTileViewModel.TileContentHeight;

                        //var fd = Rtb.Document;
                        //fd.PageWidth = RtbCanvasWidth - RtbPadding.Left - RtbPadding.Right;
                        //return fd.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
                    }
                }
                return MpMeasurements.Instance.RtbCompositeItemMinHeight;
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
                return RtbWidth - RtbPadding.Left - RtbPadding.Right - 23;
                
            }
        }

        public double RtbPageHeight {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    if (Rtb == null) {
                        return CopyItemRichText.ToFlowDocument().GetDocumentSize().Height;
                    } else {
                        if(HostClipTileViewModel.IsPastingTemplate) {
                            return Rtb.Document.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
                        }
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
                var w = HostClipTileViewModel.TileContentWidth - MpMeasurements.Instance.ClipTileSubItemOverlayMargin;
                if (RichTextBoxViewModelCollection.VerticalScrollbarVisibility == ScrollBarVisibility.Visible) {
                    w -= MpMeasurements.Instance.ScrollbarWidth;
                }
                return w;
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
                if(HostClipTileViewModel == null) {
                    return Brushes.Transparent;
                }
                if(SubItemOverlayVisibility == Visibility.Visible) {
                    var scb = CopyItemColorBrush;
                    if (HostClipTileViewModel.IsExpanded && IsSubHovering) {
                        scb.Opacity = 0.1;
                    } else if (/*HostClipTileViewModel.IsClipDropping || */
                               (!HostClipTileViewModel.IsExpanded && IsSubHovering && !IsSubSelected)) {
                        scb.Opacity = 0;
                    } else {
                        scb.Opacity = 0.25;
                    }
                    
                    return scb;
                }
                return Brushes.Transparent;
            }
            set {
                if (CopyItem != null && CopyItem.ItemColor != MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color)) {
                    CopyItem.ItemColor = MpHelpers.Instance.ConvertColorToHex(((SolidColorBrush)value).Color);
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
                if(HostClipTileViewModel.IsPastingTemplate) {
                    return Brushes.Transparent;
                }
                if (IsSubSelected) {
                    return Brushes.Pink;
                }
                if (IsSubHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Visibility
        public Visibility MultiSelectOrderMarkerVisibility {
            get {
                if (MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return Visibility.Hidden;
                }
                if (IsSubSelected && 
                    (MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1 ||
                     RichTextBoxViewModelCollection.SubSelectedClipItems.Count > 1)) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private Visibility _subItemVisibility = Visibility.Visible;
        public Visibility SubItemVisibility {
            get {
                return _subItemVisibility;
            }
            set {
                if(_subItemVisibility != value) {
                    _subItemVisibility = value;
                    OnPropertyChanged(nameof(SubItemVisibility));
                }
            }
        }

        public Visibility DragAndHotkeyButtonVisibility {
            get {
                if(MainWindowViewModel.SearchBoxViewModel.HasText) {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }

        public Visibility SubItemToolTipVisibility {
            get {
                if (CopyItem == null || !Properties.Settings.Default.ShowItemPreview) {
                    return Visibility.Collapsed;
                }
                return (MainWindowViewModel.ClipTrayViewModel.IsScrolling || IsSubSelected || HostClipTileViewModel.IsExpanded) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility SubItemOverlayVisibility {
            get {
                OnPropertyChanged(nameof(DragAndHotkeyButtonVisibility));
                if (HostClipTileViewModel == null) {
                    return Visibility.Collapsed;
                }               

                if (IsEditingSubTitle) {
                    return Visibility.Visible;
                }
                if(HostClipTileViewModel.IsPastingTemplate) {
                    return Visibility.Hidden;
                }
                if (HostClipTileViewModel.IsClipDropping) {
                    return Visibility.Collapsed;
                }
                //if(HostClipTileViewModel.IsClipDropping && (!HostClipTileViewModel.IsAnySubItemDragging || HostClipTileViewModel.IsClipDragging)) {
                //    return Visibility.Visible;
                //}
                if (!IsCompositeChild) {
                    return Visibility.Collapsed;
                }
                if(MainWindowViewModel.SearchBoxViewModel.HasText &&
                   HostClipTileViewModel.TileVisibility == Visibility.Visible &&
                   HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel != null &&
                   (HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.RtbItemViewModel != this ||
                    HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType != MpHighlightType.Text)) {                    
                    //this ensures overlay is shown when selected search match is relavant to title or application
                    return Visibility.Visible;
                }
                if(IsSubSelected && !HostClipTileViewModel.IsExpanded) {
                    return Visibility.Visible;
                }

                if(!IsSubSelected && HostClipTileViewModel.IsExpanded) {
                    return Visibility.Visible;
                }

                if (IsSubHovering && (!IsSubSelected || !HostClipTileViewModel.IsEditingTile)) {
                    return Visibility.Visible;
                }
                if (IsSubSelected && RichTextBoxViewModelCollection.SubSelectedClipItems.Count > 1) {
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

        public Visibility AppIconHighlightBorderVisibility {
            get {
                if(HostClipTileViewModel == null) {
                    return Visibility.Hidden;
                }
                return HostClipTileViewModel.HighlightTextRangeViewModelCollection.AppMatchRtbvmList.Contains(this) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility AppIconImageVisibility {
            get {
                if (MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null || !IsSelected) {
                    return Visibility.Visible;
                }
                if (MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1 &&
                   !IsSubHovering) {
                    return Visibility.Hidden;
                }
                return Visibility.Visible;
            }
        }
        #endregion

        #region Business Logic 

        private IDataObject _dragDataObject = null;
        public IDataObject DragDataObject {
            get {
                return _dragDataObject;
            }
            set {
                if(_dragDataObject != value) {
                    _dragDataObject = value;
                    OnPropertyChanged(nameof(DragDataObject));
                }
            }
        }

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
        public string ShortcutKeyString {
            get {
                return _shortcutKeyString;
            }
            set {
                if (_shortcutKeyString != value) {
                    _shortcutKeyString = value;
                    OnPropertyChanged(nameof(ShortcutKeyString));
                    OnPropertyChanged(nameof(HotkeyIconSource));
                    OnPropertyChanged(nameof(HotkeyIconTooltip));
                }
            }
        }

        public bool HasTemplate {
            get {
                return TemplateHyperlinkCollectionViewModel.Count > 0;
            }
        }
        public string TemplateRichText { get; set; }
        #endregion

        #region State
        public Point MouseDownPosition { get; set; } = new Point();

        private int _detailIdx = 1;
        public int DetailIdx {
            get {
                return _detailIdx;
            }
            set {
                if (_detailIdx != value) {
                    _detailIdx = value;
                    OnPropertyChanged(nameof(DetailIdx));
                }
            }
        }

        private TextRange _rtbSelectionRange = null;
        public TextRange RtbSelectionRange {
            get {
                return _rtbSelectionRange;
            }
            set {
                if(_rtbSelectionRange != value) {
                    _rtbSelectionRange = value;
                    OnPropertyChanged(nameof(RtbSelectionRange));
                }
            }
        }

        public string MultiSelectedOrderIdxDisplayValue {
            get {
                if (MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null || !IsSubSelected) {
                    return string.Empty;
                }
                int multiIdx = MainWindowViewModel.ClipTrayViewModel.GetSelectionOrderIdxForItem(this);
                if (multiIdx < 0) {
                    return string.Empty;
                }
                multiIdx++;
                return multiIdx.ToString();
            }
        }

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

        public bool IsSelected {
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

        public bool IsEditingContent {
            get {
                if(RichTextBoxViewModelCollection == null) {
                    return false;
                }
                if(IsSubSelected && 
                   HostClipTileViewModel.IsEditingTile && 
                   RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1) {
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
                if(CopyItem == null) {
                    return false;
                }
                return CopyItem.IsSubCompositeItem;
            }
        }
        #endregion

        #region Model
        public BitmapSource CopyItemFavIcon {
            get {
                if (CopyItemUrlDomain == null) {
                    return null;
                }
                return CopyItemUrlDomain.FavIconImage;
            }
            set {
                if (CopyItemUrlDomain != null && CopyItemUrlDomain.FavIconImage != value) {
                    CopyItemUrlDomain.FavIconImage = value;
                    CopyItemUrlDomain.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemFavIcon));
                    OnPropertyChanged(nameof(AppIcon));
                }
            }
        }

        public BitmapSource CopyItemAppIconHighlightBorder {
            get {
                if (CopyItem == null || HostClipTileViewModel == null) {
                    return new BitmapImage();
                }
                OnPropertyChanged(nameof(AppIconHighlightBorderVisibility));
                if (AppIconHighlightBorderVisibility == Visibility.Visible) {
                    if (HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel != null &&
                       HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.HighlightType == MpHighlightType.App &&
                       HostClipTileViewModel.HighlightTextRangeViewModelCollection.SelectedHighlightTextRangeViewModel.RtbItemViewModel == this) {
                        return CopyItem.App.IconSelectedHighlightBorderImage;
                    }
                    return CopyItem.App.IconHighlightBorderImage;
                }
                return CopyItem.App.IconHighlightBorderImage;
            }
        }

        public BitmapSource CopyItemAppIconBorder {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.App.IconBorderImage;
            }
        }

        public BitmapSource CopyItemAppIcon {
            get {
                if (CopyItem == null) {
                    return new BitmapImage();
                }
                return CopyItem.App.IconImage;
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                if (CopyItem == null) {
                    return MpCopyItemType.None;
                }
                return CopyItem.CopyItemType;
            }
        }

        public int CopyItemAppId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.App.AppId;
            }
        }

        public BitmapSource CopyItemBmp {
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
                    OnPropertyChanged(nameof(CopyItemBmp));
                }
            }
        }

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
                return CopyItem.CopyItemId;
            }
        }

        public  string CopyItemPlainText {
            get {
                if (CopyItem == null || CopyItem.ItemPlainText == null) {
                    return string.Empty;
                }
                return CopyItem.ItemPlainText;
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

        public string CopyItemTitle {
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
                    OnPropertyChanged(nameof(CopyItemTitle));
                }
            }
        }

        public  List<string> CopyItemFileDropList {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return new List<string>();
                }
                return CopyItem.GetFileList();// string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
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
                if (CopyItem == null) {
                    return null;
                } 
                return CopyItem.ItemUrl;
            }
            set {
                if (CopyItem != null && CopyItem.ItemUrl != value) {
                    CopyItem.ItemUrl = value;
                    CopyItemUrlDomain = CopyItemUrl.UrlDomain;
                    CopyItem.ItemUrl.WriteToDatabase();
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemUrl));
                    OnPropertyChanged(nameof(CopyItemUrlDomain));
                    OnPropertyChanged(nameof(AppIcon));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public string CopyItemAppPath {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.App.AppPath;
            }
        }

        public string CopyItemAppName {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                return CopyItem.App.AppName;
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

        private MpCopyItem _copyItem = null;
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                //if(_copyItem != value) 
                    {                   
                    _copyItem = value;
                    if (CopyItem != null && !MpMainWindowViewModel.IsApplicationLoading) {
                        CopyItem.WriteToDatabase();
                    }
                    OnPropertyChanged(nameof(CopyItem));                    
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
                var rtbvm = s as MpRtbListBoxItemRichTextBoxViewModel;
                switch (e.PropertyName) {
                    case nameof(rtbvm.CopyItem):
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemId));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CompositeParentCopyItemId));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CompositeSortOrderIdx));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemFilePath));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemRichText));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemPlainText));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemTitle));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemBmp));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemUrl));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemFavIcon));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemAppIcon));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.PasteCount));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemAppName));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemUrl));
                        rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemUrlDomain));
                        break;
                    case nameof(rtbvm.IsSubSelected):
                        if(rtbvm.IsSubSelected) {
                            rtbvm.LastSubSelectedDateTime = rtbvm.IsSubSelected ? DateTime.Now : DateTime.MaxValue;
                        }
                        if(rtbvm.HostClipTileViewModel.IsClipDragging) {
                            return;
                        }
                        if(rtbvm.IsSubSelected && !rtbvm.HostClipTileViewModel.IsSelected) {
                            rtbvm.HostClipTileViewModel.IsSelected = true;
                        } else if(!rtbvm.IsSubSelected &&
                                  rtbvm.HostClipTileViewModel.IsSelected &&
                                  rtbvm.RichTextBoxViewModelCollection.Count == 1) {
                            //rtbvm.HostClipTileViewModel.IsSelected = false;
                        }
                        if (rtbvm.IsSubSelected) {
                            if (rtbvm.HostClipTileViewModel.IsExpanded) {
                                foreach (var ortbvm in rtbvm.RichTextBoxViewModelCollection) {
                                    if (ortbvm != rtbvm) {
                                        ortbvm.IsSubSelected = false;
                                        ortbvm.IsEditingSubTitle = false;
                                    }
                                }
                                rtbvm.ResetRtb();
                                //RichTextBoxViewModelCollection.Refresh();
                            } else if (!MpHelpers.Instance.IsMultiSelectKeyDown() && 
                                       !rtbvm.IsSubDragging && 
                                       !rtbvm.HostClipTileViewModel.IsContextMenuOpened && 
                                       !rtbvm.IsSubContextMenuOpened &&
                                       !rtbvm.HostClipTileViewModel.IsClipDragging &&
                                       !MainWindowViewModel.ClipTrayViewModel.IsPastingHotKey &&
                                       !MainWindowViewModel.ClipTrayViewModel.IsPastingSelected &&
                                       MpMainWindowViewModel.IsMainWindowOpen) {
                                Console.WriteLine(@"RtbItem Selection was rejected");
                                rtbvm.IsSubSelected = false;
                            }
                            if (rtbvm.HostClipTileViewModel.IsEditingTile) {
                                rtbvm.HostClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(rtbvm.Rtb, false);
                            }              
                        } else if(rtbvm.HostClipTileViewModel.IsEditingTile) {
                            rtbvm.SaveSubItemToDatabase();
                        } else {
                            
                        }
                        if(rtbvm.HostClipTileViewModel.IsExpanded) {
                            rtbvm.RichTextBoxViewModelCollection.UpdateLayout();
                        }
                        break;
                    case nameof(rtbvm.IsSubHovering):
                        if(rtbvm.IsSubHovering) {
                            if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                                rtbvm.IsSubHovering = false;
                            }
                            foreach (var ortbvm in rtbvm.RichTextBoxViewModelCollection) {
                                if (ortbvm != rtbvm) {
                                    ortbvm.IsSubHovering = false;
                                }
                            }
                        } else {
                            //_detailIdx = 1;
                        }
                        break;
                    case nameof(rtbvm.IsHoveringOnTitleTextBlock):
                        if(rtbvm.IsHoveringOnTitleTextBlock) {
                            rtbvm.DetailIdx++;
                            if (rtbvm.DetailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                                rtbvm.DetailIdx = 1;
                            }
                            rtbvm.DetailText = rtbvm.CopyItem.GetDetail((MpCopyItemDetailType)rtbvm.DetailIdx);
                        }
                        break;
                    case nameof(rtbvm.IsSubContextMenuOpened):
                        if(rtbvm.IsSubContextMenuOpened) {
                            rtbvm.IsSubSelected = true;
                        }
                        rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileBorderBrush));
                        rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileBorderBrushRect));
                        break;
                }
            };

            ViewModelLoaded += async (s, e) => {
                var rtbvm = s as MpRtbListBoxItemRichTextBoxViewModel;
                if (!MpMainWindowViewModel.IsApplicationLoading) {
                    await rtbvm.GatherAnalytics();
                } else {
                    if (rtbvm.RichTextBoxViewModelCollection.IndexOf(rtbvm) == 0) {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                            rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.AppIcon));
                            rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileTitleIconSize));
                            rtbvm.HostClipTileViewModel.OnPropertyChanged(nameof(rtbvm.HostClipTileViewModel.TileTitleIconBorderSize));
                        }), DispatcherPriority.Render);
                    }
                }

                rtbvm.RtbSelectionRange = new TextRange(rtbvm.Rtb.Document.ContentEnd, rtbvm.Rtb.Document.ContentEnd);
            };
        }

        public void RtbItem_Loaded(object sender, RoutedEventArgs e) {
            Rtbc = (Canvas)sender;
            Rtb = (RichTextBox)Rtbc.FindName("RtbListBoxItemRichTextBox");
            RtbListBoxItemClipBorder = (MpClipBorder)Rtbc.FindName("RtbListBoxItemOverlayBorder");
            RtbListBoxItemOverlayDockPanel = (DockPanel)Rtbc.FindName("RtbListItemOverlayBorderDockPanel");
            RtbListBoxItemTitleTextBlock = (TextBlock)Rtbc.FindName("RtbListBoxItemTitleTextBlock");
            RtbListBoxItemTitleTextBox = (TextBox)Rtbc.FindName("RtbListBoxItemTitleTextBox");
            DragButton = (Button)Rtbc.FindName("RtbListBoxItemOverlayBorderGridDragButton");
            var titleIconImageButton = (Button)Rtbc.FindName("RtbItemAppIconImageButton");
            var titleIconBorderImage = (Image)Rtbc.FindName("RtbItemAppIconBorderImage");
            var titleIconBorderImageScaleTransform = (ScaleTransform)Rtbc.FindName("RtbItemAppIconBorderImageScaleTransform");
            var titleIconHighlightBorderImage = (Image)Rtbc.FindName("RtbItemAppIconHighlightedBorderImage");

            HostClipTileViewModel.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);

            Rtb.TextChanged += (s, e44) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.RichTextBoxViewModelCollection.UpdateLayout();
            };

            Rtb.SelectionChanged += (s, e3) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                if (rtbvm.IsEditingContent) {
                    rtbvm.HostClipTileViewModel.EditRichTextBoxToolbarViewModel.Rtb_SelectionChanged(rtbvm.Rtb, e3);
                }
            };

            ClearHyperlinks();
            RawRtf = Rtb.Document.ToRichText();
           
            CreateHyperlinks();


            #region Drag & Drop

            int minDragDist = 10;
            DragButton.PreviewGiveFeedback += (s, e3) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                    Application.Current.MainWindow.Cursor = Cursors.Cross;
                    Application.Current.MainWindow.ForceCursor = true;
                } 
            };
            DragButton.PreviewMouseUp += (s, e9) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                Application.Current.MainWindow.ForceCursor = false;
                rtbvm.MouseDownPosition = new Point();
                rtbvm.DragDataObject = null;
                rtbvm.IsSubDragging = false;
                rtbvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, false);
            };
            DragButton.PreviewMouseDown += (s, e8) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, true);
            };
            DragButton.PreviewMouseMove += (s, e7) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                    if(rtbvm.IsEditingContent || 
                      (rtbvm.HostClipTileViewModel.IsExpanded && rtbvm.RichTextBoxViewModelCollection.Count == 1)) {
                        //cannot resort w/ only 1 item and its relative location is not clear
                        //since its isolated
                        return;
                    }
                    rtbvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(false, true);
                    if(rtbvm.MouseDownPosition == new Point()) {
                        rtbvm.MouseDownPosition = e7.GetPosition(rtbvm.Rtbc);
                    }
                    if (MpHelpers.Instance.DistanceBetweenPoints(rtbvm.MouseDownPosition, e7.GetPosition(rtbvm.Rtbc)) < minDragDist) {
                        return;
                    }
                    rtbvm.IsSubDragging = true;
                    rtbvm.IsSubSelected = true;
                    if(rtbvm.DragDataObject == null) {
                        rtbvm.DragDataObject = MainWindowViewModel.ClipTrayViewModel.GetDataObjectFromSelectedClips(true,false).Result;//RichTextBoxViewModelCollection.GetDataObjectFromSubSelectedItems(true).Result;
                    }
                    DragDrop.DoDragDrop(
                                rtbvm.Rtbc,
                                rtbvm.DragDataObject,
                                DragDropEffects.Copy | DragDropEffects.Move);
                    e.Handled = true;
                }
            };
            DragButton.MouseEnter += (s, e2) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsOverDragButton = true;
                rtbvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, false);
            };
            DragButton.MouseLeave += (s, e2) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsOverDragButton = false;
                rtbvm.RichTextBoxViewModelCollection.SyncMultiSelectDragButton(false, false);
            };
            #endregion

            if (HostClipTileViewModel.WasAddedAtRuntime) {
                //force new items to have left alignment
                Rtb.CaretPosition = Rtb.Document.ContentStart;
                Rtb.Document.TextAlignment = TextAlignment.Left;
                Rtb.UpdateLayout();
            }

            RtbListBoxItemAdornerLayer = AdornerLayer.GetAdornerLayer(Rtbc);
            RtbListBoxItemAdornerLayer?.Add(new MpRtbListBoxItemAdorner(Rtbc));
                        
            Rtbc.MouseEnter += (s, e2) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsSubHovering = true;
            };
            Rtbc.MouseLeave += (s, e2) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsSubHovering = false;
            };

            #region Title
            RtbListBoxItemTitleTextBlock.PreviewMouseLeftButtonDown += (s, e7) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                //RichTextBoxViewModelCollection.ClearSubSelection();
                if (!rtbvm.HostClipTileViewModel.IsExpanded) {
                    rtbvm.IsSubSelected = true;
                }
                rtbvm.IsEditingSubTitle = true;
                e7.Handled = true;
            };

            RtbListBoxItemTitleTextBlock.MouseEnter += (s, e5) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsHoveringOnTitleTextBlock = true;
            };
            RtbListBoxItemTitleTextBlock.MouseLeave += (s, e5) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsHoveringOnTitleTextBlock = false;
            };

            RtbListBoxItemTitleTextBox.IsVisibleChanged += (s, e9) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                if (rtbvm.RtbListBoxItemTitleTextBoxVisibility == Visibility.Collapsed) {
                    rtbvm.CopyItemTitle = RtbListBoxItemTitleTextBox.Text;
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                tbx.SelectAll();
            };

            RtbListBoxItemTitleTextBox.LostFocus += (s, e4) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                rtbvm.IsEditingSubTitle = false;
            };
            RtbListBoxItemTitleTextBox.PreviewKeyDown += (s, e5) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;
                if (e5.Key == Key.Enter || e5.Key == Key.Escape) {
                    rtbvm.IsEditingSubTitle = false;
                }
            };

            titleIconImageButton.PreviewMouseLeftButtonUp += (s, e7) => {
                var rtbvm = ((FrameworkElement)s).DataContext as MpRtbListBoxItemRichTextBoxViewModel;

                //MpHelpers.Instance.OpenUrl(CopyItem.App.AppPath);
                rtbvm.RichTextBoxViewModelCollection.ClearSubSelection();
                rtbvm.IsSubSelected = true;

                foreach (var vctvm in MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles) {
                    if (vctvm.CopyItemAppId != rtbvm.CopyItemAppId) {
                        bool hasSubItemWithApp = false;
                        if (vctvm.RichTextBoxViewModelCollection.Count > 1) {
                            foreach (var vrtbvm in vctvm.RichTextBoxViewModelCollection) {
                                if (vrtbvm.CopyItemAppId != rtbvm.CopyItemAppId) {
                                    vrtbvm.SubItemVisibility = Visibility.Collapsed;
                                } else {
                                    hasSubItemWithApp = true;
                                }
                            }
                        }
                        if (!hasSubItemWithApp) {
                            vctvm.TileVisibility = Visibility.Collapsed;
                        }
                    }
                }
                //this triggers clip tray to swap out the app icons for the filtered app
                MainWindowViewModel.ClipTrayViewModel.FilterByAppIcon = rtbvm.CopyItemAppIcon;
                MainWindowViewModel.ClipTrayViewModel.IsFilteringByApp = true;
            };
            #endregion

            UpdateLayout();

            var scvml = MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList();
            if (scvml.Count > 0) {
                ShortcutKeyString = scvml[0].KeyString;
            }

            OnViewModelLoaded();
        }

        #region Context Menu
        public void RtbItem_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            cm.DataContext = this;
            MenuItem cmi = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
                    cmi = (MenuItem)mi;
                    break;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    cm,
                    cmi,
                    (s, e1) => {
                        RichTextBoxViewModelCollection.ChangeSubSelectedClipsColorCommand.Execute((Brush)((Border)s).Tag);
                        foreach (var sctvm in RichTextBoxViewModelCollection.SubSelectedClipItems) {
                            sctvm.CopyItem.WriteToDatabase();
                        }
                    },
                    MpHelpers.Instance.GetColorColumn(CopyItemColorBrush),
                    MpHelpers.Instance.GetColorRow(CopyItemColorBrush)
                );
        }

        public void RtbItem_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            var rtbvm = cm.DataContext as MpRtbListBoxItemRichTextBoxViewModel;

            rtbvm.IsSubContextMenuOpened = false;

            rtbvm.RichTextBoxViewModelCollection.ClearSubSelection();
        }

        public void RtbItem_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var cm = sender as ContextMenu;
            var rtbvm = (MpRtbListBoxItemRichTextBoxViewModel)cm.DataContext;
            cm.Tag = rtbvm;
            rtbvm.IsSubContextMenuOpened = true;
            cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);

            MenuItem eami = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == @"ToolsMenuItem") {
                    foreach (var smi in (mi as MenuItem).Items) {
                        if (smi == null || smi is Separator) {
                            continue;
                        }
                        if ((smi as MenuItem).Name == "ExcludeApplication") {
                            eami = smi as MenuItem;
                        }
                    }
                }
            }
            if (eami != null) {
                eami.Header = @"Exclude Application '" + rtbvm.CopyItem.App.AppName + "'";
            }

            rtbvm.RefreshAsyncCommands();

            rtbvm.OnPropertyChanged(nameof(rtbvm.TagMenuItems));

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }
        #endregion      

        public void ClearSubDragDropState() {
            IsSubDragging = false;
            MouseDownPosition = new Point();
            DragDataObject = null;
        }

        public async Task GatherAnalytics() {
            string detectedUrl = string.Empty;

            if (CopyItem.ItemScreenshot != null) {
                detectedUrl = await MpBrowserUrlDetector.Instance.FindUrlAddressFromScreenshot(CopyItem.ItemScreenshot);
            }

            if (!string.IsNullOrEmpty(detectedUrl)) {
                string urlTitle = await MpHelpers.Instance.GetUrlTitle(detectedUrl); 
                var itemUrl = new MpUrl(detectedUrl, urlTitle);
                if(itemUrl.UrlDomain == null) {
                    string urlDomain = MpHelpers.Instance.GetUrlDomain(detectedUrl);
                    var urlFavIcon = MpHelpers.Instance.GetUrlFavicon(urlDomain);
                    urlDomain = MpHelpers.Instance.GetFullyFormattedUrl(urlDomain);
                    string urlDomainTitle = await MpHelpers.Instance.GetUrlTitle(urlDomain);
                    CopyItemUrlDomain = new MpUrlDomain(urlDomain, urlFavIcon, urlDomainTitle, false);
                }
                itemUrl.UrlDomain = CopyItemUrlDomain;
                CopyItemUrl = itemUrl;

                if (RichTextBoxViewModelCollection.Count == 1) {
                    await Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                        HostClipTileViewModel.CopyItemUrl = CopyItemUrl;
                        HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItemFavIcon));
                        HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.AppIcon));
                        HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileTitleIconSize));
                        HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileTitleIconBorderSize));
                    }), DispatcherPriority.Render);
                }

                

                Console.WriteLine("Detected Browser Address: " + detectedUrl);
            }
        }

        public void RefreshAsyncCommands() {
            MainWindowViewModel.ClipTrayViewModel.HotkeyPasteCommand.RaiseCanExecuteChanged();
            RichTextBoxViewModelCollection.BringSubSelectedClipTilesToFrontCommand.RaiseCanExecuteChanged();
            RichTextBoxViewModelCollection.SendSubSelectedClipTilesToBackCommand.RaiseCanExecuteChanged();
            RichTextBoxViewModelCollection.SpeakSubSelectedClipsAsyncCommand.RaiseCanExecuteChanged();            
        }

        public void UpdateLayout() {
            if (Rtb != null) {
                Rtb.Document.PageWidth = RtbPageWidth;
                Rtb.Document.PageHeight = RtbPageHeight;
            }

            OnPropertyChanged(nameof(SubItemOverlayVisibility));
            OnPropertyChanged(nameof(SubItemToolTipVisibility));
            OnPropertyChanged(nameof(RtbPadding));
            OnPropertyChanged(nameof(RtbCanvasWidth));
            OnPropertyChanged(nameof(RtbCanvasHeight));
            OnPropertyChanged(nameof(RtbWidth));
            OnPropertyChanged(nameof(RtbHeight));
            OnPropertyChanged(nameof(RtbPageWidth));
            OnPropertyChanged(nameof(RtbPageHeight));
            OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));

            
            Rtbc?.UpdateLayout();
            Rtb?.UpdateLayout();
            RtbListBoxItemClipBorder?.UpdateLayout();  
        }

        public void ResetRtb() {
            if (Rtb == null) {
                return;
            }
            Rtb.ScrollToHome();
            Rtb.CaretPosition = Rtb.Document.ContentStart;
            Rtb.Selection.Select(Rtb.Document.ContentStart, Rtb.Document.ContentStart);

            RichTextBoxViewModelCollection.UpdateLayout();
            if(IsSubSelected) {
                Rtb.Focus();
            }
        }

        public void SaveSubItemToDatabase() {
            var sw = new Stopwatch();
            sw.Start();

            //remove links to update model rich text
            ClearHyperlinks();

            if(Rtb != null) {
                
            }
            //clear any search highlighting when saving the document then restore after save
            HostClipTileViewModel.HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(this);

            if(Rtb != null) {
                RawRtf = Rtb.Document.ToRichText();
                CopyItemRichText = RawRtf;
            }

            HostClipTileViewModel.HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(this);
            CreateHyperlinks();

            var cipcsw = new Stopwatch();
            cipcsw.Start();

            HostClipTileViewModel.CopyItemBmp = HostClipTileViewModel.CopyItem.GetSeparatedCompositeFlowDocument().ToBitmapSource();
            //OnPropertyChanged(nameof(CopyItem));
            cipcsw.Stop();
            Console.WriteLine("Saving cliptile copyitem propertychanged time: " + cipcsw.ElapsedMilliseconds + "ms");

            sw.Stop();
            Console.WriteLine("Saving(VIdx:" + RichTextBoxViewModelCollection.IndexOf(this) + "): " + sw.ElapsedMilliseconds + "ms");
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
                new Span(new Run(linkText), hl.ElementStart);
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

            var rtbSelection = Rtb?.Selection.Clone();
            for (int i = 1; i < MpRegEx.Instance.RegExList.Count; i++) {
                var linkType = (MpSubTextTokenType)i;                
                if (linkType == MpSubTextTokenType.StreetAddress) {
                    //doesn't consistently work and presents bugs so disabling for now
                    continue;
                }
                var lastRangeEnd = Rtb.Document.ContentStart;
                var regExStr = MpRegEx.Instance.GetRegExForTokenType(linkType);
                if(linkType == MpSubTextTokenType.TemplateSegment) {
                    regExStr = CopyItem.TemplateRegExMatchString;
                }
                if (string.IsNullOrEmpty(regExStr)) {
                    //this occurs for templates when copyitem has no templates
                    continue;
                }
                if(linkType == MpSubTextTokenType.TemplateSegment) {
                    linkType = MpSubTextTokenType.TemplateSegment;
                }
                var mc = Regex.Matches(CopyItem.ItemPlainText, regExStr, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
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
                                hl = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(this, copyItemTemplate, matchRange);
                                hl.Tag = linkType;
                                TemplateHyperlinkCollectionViewModel.Add((MpTemplateHyperlinkViewModel)hl.DataContext);
                            } else {
                                var matchRun = new Run(matchRange.Text);
                                matchRange.Text = "";
                                // DO NOT REMOVE this extra link ensures selection is retained!
                                var hlink = new Hyperlink(matchRun, matchRange.Start);
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                hl = hlink;
                                var linkText = c.Value;
                                hl.Tag = linkType;
                                if(linkText == @"DragAction.Cancel") {
                                    linkText = linkText;
                                }
                                MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(IsSubSelected)), hl, Hyperlink.IsEnabledProperty);
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
                                        try {
                                            string urlText = MpHelpers.Instance.GetFullyFormattedUrl(linkText);
                                            if(MpHelpers.Instance.IsValidUrl(urlText) &&
                                               Uri.IsWellFormedUriString(urlText,UriKind.RelativeOrAbsolute)) {
                                                hl.NavigateUri = new Uri(urlText);
                                            } else {
                                                Console.WriteLine(@"Rejected Url: " + urlText + @" link text: " + linkText);
                                                var par = hl.Parent.FindParentOfType<Paragraph>();
                                                var s = new Span();
                                                s.Inlines.AddRange(hl.Inlines.ToArray());
                                                par.Inlines.InsertAfter(hl, s);
                                                par.Inlines.Remove(hl);
                                            }
                                        }
                                        catch(Exception ex) {
                                            Console.WriteLine("CreateHyperlinks error creating uri from: " + linkText + " replacing as run and ignoring with exception: "+ex);
                                            var par = hl.Parent.FindParentOfType<Paragraph>();
                                            var s = new Span();
                                            s.Inlines.AddRange(hl.Inlines.ToArray());
                                            par.Inlines.InsertAfter(hl, s);
                                            par.Inlines.Remove(hl);
                                            par.Inlines.Remove(hlink);
                                            break;

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
                                    case MpSubTextTokenType.HexColor8:
                                    case MpSubTextTokenType.HexColor6:
                                        var rgbColorStr = linkText;
                                        if (rgbColorStr.Length > 7) {
                                            rgbColorStr = rgbColorStr.Substring(0, 7);
                                        }
                                        hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);

                                        MenuItem changeColorItem = new MenuItem();
                                        changeColorItem.Header = "Change Color";
                                        changeColorItem.Click += (s, e) => {
                                            var result = MpHelpers.Instance.ShowColorDialog((Brush)new BrushConverter().ConvertFrom(linkText),true);
                                            if(result != null) {
                                                var run = new Run(result.ToString());
                                                hl.Inlines.Clear();
                                                hl.Inlines.Add(run);
                                                var bgBrush = result;
                                                var fgBrush = MpHelpers.Instance.IsBright(((SolidColorBrush)bgBrush).Color) ? Brushes.Black : Brushes.White;
                                                var tr = new TextRange(run.ElementStart, run.ElementEnd);
                                                tr.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
                                                tr.ApplyPropertyValue(TextElement.ForegroundProperty, fgBrush);
                                            }
                                        };
                                        hl.ContextMenu.Items.Add(changeColorItem);

                                        hl.Background = (Brush)new BrushConverter().ConvertFromString(linkText);
                                        hl.Foreground = MpHelpers.Instance.IsBright(((SolidColorBrush)hl.Background).Color) ? Brushes.Black : Brushes.White;
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

        public List<Hyperlink> GetHyperlinkList() {
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

        #region Private methods
        #endregion

        #endregion

        #region Commands
        private RelayCommand _editSubTitleCommand;
        public ICommand EditSubTitleCommand {
            get {
                if (_editSubTitleCommand == null) {
                    _editSubTitleCommand = new RelayCommand(EditSubTitle, CanEditSubTitle);
                }
                return _editSubTitleCommand;
            }
        }
        private bool CanEditSubTitle() {
            if (MpMainWindowViewModel.IsApplicationLoading) {
                return false;
            }
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                   RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1;
        }
        private void EditSubTitle() {
            IsEditingSubTitle = !IsEditingSubTitle;
        }

        private RelayCommand _editSubContentCommand;
        public ICommand EditSubContentCommand {
            get {
                if (_editSubContentCommand == null) {
                    _editSubContentCommand = new RelayCommand(EditSubContent, CanEditSubContent);
                }
                return _editSubContentCommand;
            }
        }
        private bool CanEditSubContent() {
            if (MpMainWindowViewModel.IsApplicationLoading) {
                return false;
            }
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 &&
                   RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1;
        }
        private void EditSubContent() {
            if(!HostClipTileViewModel.IsEditingTile) {
                HostClipTileViewModel.IsEditingTile = true;
                RichTextBoxViewModelCollection.ClearSubSelection();
                IsSubSelected = true;
            }            
        }

        private RelayCommand _sendSubSelectedToEmailCommand;
        public ICommand SendSubSelectedToEmailCommand {
            get {
                if (_sendSubSelectedToEmailCommand == null) {
                    _sendSubSelectedToEmailCommand = new RelayCommand(SendSubSelectedToEmail, CanSendSubSelectedToEmail);
                }
                return _sendSubSelectedToEmailCommand;
            }
        }
        private bool CanSendSubSelectedToEmail() {
            return !IsEditingContent;
        }
        private void SendSubSelectedToEmail() {
            MpHelpers.Instance.OpenUrl(string.Format("mailto:{0}?subject={1}&body={2}", string.Empty, CopyItemTitle, CopyItemPlainText));
            //MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            //IsSelected = true;
            //MpHelpers.Instance.CreateEmail(Properties.Settings.Default.UserEmail,CopyItemTitle, CopyItemPlainText, CopyItemFileDropList[0]);
        }

        private RelayCommand _createQrCodeFromSubSelectedItemCommand;
        public ICommand CreateQrCodeFromSubSelectedItemCommand {
            get {
                if (_createQrCodeFromSubSelectedItemCommand == null) {
                    _createQrCodeFromSubSelectedItemCommand = new RelayCommand(CreateQrCodeFromSubSelectedItem, CanCreateQrCodeFromSubSelectedItem);
                }
                return _createQrCodeFromSubSelectedItemCommand;
            }
        }
        private bool CanCreateQrCodeFromSubSelectedItem() {
            return CopyItemType == MpCopyItemType.RichText && CopyItemPlainText.Length <= Properties.Settings.Default.MaxQrCodeCharLength;
        }
        private void CreateQrCodeFromSubSelectedItem() {
            var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(CopyItemPlainText);
            System.Windows.Clipboard.SetImage(bmpSrc);
        }

        private AsyncCommand<string> _translateSubSelectedItemTextAsyncCommand;
        public IAsyncCommand<string> TranslateSubSelectedItemTextAsyncCommand {
            get {
                if (_translateSubSelectedItemTextAsyncCommand == null) {
                    _translateSubSelectedItemTextAsyncCommand = new AsyncCommand<string>(TranslateSubSelectedItemTextAsync, CanTranslateSubSelectedItemText);
                }
                return _translateSubSelectedItemTextAsyncCommand;
            }
        }
        private bool CanTranslateSubSelectedItemText(object args) {
            return CopyItemType == MpCopyItemType.RichText;
        }
        private async Task TranslateSubSelectedItemTextAsync(string toLanguage) {
            var translatedText = await MpLanguageTranslator.Instance.Translate(CopyItemPlainText, toLanguage, false);
            if (!string.IsNullOrEmpty(translatedText)) {
                CopyItemRichText = MpHelpers.Instance.ConvertPlainTextToRichText(translatedText);
            }
        }

        private RelayCommand _excludeSubSelectedItemApplicationCommand;
        public ICommand ExcludeSubSelectedItemApplicationCommand {
            get {
                if (_excludeSubSelectedItemApplicationCommand == null) {
                    _excludeSubSelectedItemApplicationCommand = new RelayCommand(ExcludeSubSelectedItemApplication, CanExcludeSubSelectedItemApplication);
                }
                return _excludeSubSelectedItemApplicationCommand;
            }
        }
        private bool CanExcludeSubSelectedItemApplication() {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ExcludeSubSelectedItemApplication() {
            MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItemAppId), true);
        }

        private RelayCommand _pasteSubItemCommand;
        public ICommand PasteSubItemCommand {
            get {
                if (_pasteSubItemCommand == null) {
                    _pasteSubItemCommand = new RelayCommand(PasteSubItem);
                }
                return _pasteSubItemCommand;
            }
        }
        private void PasteSubItem() {
            MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            HostClipTileViewModel.IsSelected = true;
            HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
            IsSubSelected = true;
            MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand.Execute(null);
        }

        private RelayCommand _assignHotkeyToSubSelectedItemCommand;
        public ICommand AssignHotkeyToSubSelectedItemCommand {
            get {
                if (_assignHotkeyToSubSelectedItemCommand == null) {
                    _assignHotkeyToSubSelectedItemCommand = new RelayCommand(AssignHotkeyToSubSelectedItem);
                }
                return _assignHotkeyToSubSelectedItemCommand;
            }
        }
        private void AssignHotkeyToSubSelectedItem() {
            ShortcutKeyString = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(
                this,
                "Paste " + CopyItemTitle,
                ShortcutKeyString,
                 MainWindowViewModel.ClipTrayViewModel.HotkeyPasteCommand, CopyItemId);
        }
        #endregion

        #region Overrides
        public void Dispose() {
            if(RichTextBoxViewModelCollection.Contains(this)) {
                RichTextBoxViewModelCollection.Remove(this);
            }
            var scvmToRemoveList = new List<MpShortcutViewModel>();
            foreach (var scvmToRemove in MpShortcutCollectionViewModel.Instance.Where(x => x.CopyItemId == CopyItemId).ToList()) {
                scvmToRemoveList.Add(scvmToRemove);
            }
            foreach (var scvmToRemove in scvmToRemoveList) {
                MpShortcutCollectionViewModel.Instance.Remove(scvmToRemove);
            }
            CopyItem.DeleteFromDatabase();
            Rtb = null;
            RtbListBoxItemOverlayDockPanel = null;
            RtbListBoxItemClipBorder = null;
            RtbListBoxItemTitleTextBlock = null;
            RtbListBoxItemTitleTextBox = null;
            Rtbc = null;
            RtbListBoxItemAdornerLayer = null;
        }
        #endregion
    }

}
