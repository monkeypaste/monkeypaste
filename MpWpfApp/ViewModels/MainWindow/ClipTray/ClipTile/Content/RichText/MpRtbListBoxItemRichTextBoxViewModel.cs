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
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MpWpfApp {
    [Serializable]
    public class MpRtbListBoxItemRichTextBoxViewModel : MpUndoableViewModelBase<MpRtbListBoxItemRichTextBoxViewModel> , ICloneable, IDisposable {
        #region Private Variables
        private int _detailIdx = 1;
        private IDataObject _dragDataObject = null;
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
        public new ObservableCollection<MpContextMenuItemViewModel> TagMenuItems {
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
                            MainWindowViewModel.ClipTrayViewModel.LinkTagToCopyItemCommand,
                            tagTile,
                            tagTile.IsLinkedWithRtbItem(this)));
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
                        return CopyItemRichText.ToFlowDocument().GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
                    } else {
                        return Rtb.Document.GetDocumentSize().Height + RtbPadding.Top + RtbPadding.Bottom;
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

        public double RtbOverlayWidth {
            get {
                if(RichTextBoxViewModelCollection == null) {
                    return 0;
                }
                var w = RichTextBoxViewModelCollection.RtbLbScrollViewerWidth;
                if (RichTextBoxViewModelCollection.VerticalScrollbarVisibility == ScrollBarVisibility.Visible) {
                    w -= MpMeasurements.Instance.ScrollbarWidth;
                }
                return w;
            }
        }

        public double RtbOverlayHeight {
            get {
                if (RichTextBoxViewModelCollection == null) {
                    return 0;
                }
                var w = RichTextBoxViewModelCollection.RtbLbScrollViewerWidth;
                if (RichTextBoxViewModelCollection.VerticalScrollbarVisibility == ScrollBarVisibility.Visible) {
                    w -= MpMeasurements.Instance.ScrollbarWidth;
                }
                return w;
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
                    } else if (!HostClipTileViewModel.IsExpanded && IsSubHovering && !IsSubSelected) {
                        scb.Opacity = 0;
                    } else {
                        scb.Opacity = 0.25;
                    }
                    
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
        public Visibility SubItemToolTipVisibility {
            get {
                if (CopyItem == null) {
                    return Visibility.Collapsed;
                }
                return (MainWindowViewModel.ClipTrayViewModel.IsScrolling || IsSubSelected || HostClipTileViewModel.IsExpanded) ? Visibility.Collapsed : Visibility.Visible;
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
                if(HostClipTileViewModel.IsClipDropping && (!HostClipTileViewModel.IsAnySubItemDragging || HostClipTileViewModel.IsClipDragging)) {
                    return Visibility.Visible;
                }
                if(!IsCompositeChild) {
                    return Visibility.Collapsed;
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
        #endregion

        #region Header & Footer 
        public MpRtbListBoxItemRichTextBoxViewModel Next {
            get {
                if (HostClipTileViewModel == null ||
                   RichTextBoxViewModelCollection == null ||
                   CompositeSortOrderIdx < 0 ||
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
                   CompositeSortOrderIdx < 0 ||
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
                    OnPropertyChanged(nameof(CopyItem));
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
                return CopyItem.GetFileList();// string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
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
                        if(HostClipTileViewModel.IsClipDragging) {
                            return;
                        }

                        LastSubSelectedDateTime = IsSubSelected ? DateTime.Now : DateTime.MaxValue;

                        if(IsSubSelected && !HostClipTileViewModel.IsSelected) {
                            HostClipTileViewModel.IsSelected = true;
                        } else if(!IsSubSelected && 
                                  HostClipTileViewModel.IsSelected && 
                                  RichTextBoxViewModelCollection.Count == 1) {
                            //HostClipTileViewModel.IsSelected = false;
                        }
                        if (IsSubSelected) {
                            if (HostClipTileViewModel.IsExpanded) {
                                foreach (var rtbvm in RichTextBoxViewModelCollection) {
                                    if (rtbvm != this) {
                                        rtbvm.IsSubSelected = false;
                                        rtbvm.IsEditingSubTitle = false;
                                    }
                                }
                                ResetRtb();
                                //RichTextBoxViewModelCollection.Refresh();
                            } else if (!MpHelpers.Instance.IsMultiSelectKeyDown() && 
                                       !IsSubDragging && 
                                       !HostClipTileViewModel.IsContextMenuOpened && 
                                       !IsSubContextMenuOpened) {
                                IsSubSelected = false;
                            }
                            if (HostClipTileViewModel.IsEditingTile) {
                                HostClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(Rtb, false);
                            }
                            if (HostClipTileViewModel.IsPastingTemplate) {
                                HostClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(Rtb, false);
                            }                      
                        } else if(HostClipTileViewModel.IsEditingTile) {
                            SaveSubItemToDatabase();
                        } else {
                            
                        }
                        if(HostClipTileViewModel.IsExpanded) {
                            RichTextBoxViewModelCollection.UpdateLayout();
                        }
                        break;
                    case nameof(IsSubHovering):
                        if(IsSubHovering) {
                            if (MainWindowViewModel.ClipTrayViewModel.IsScrolling) {
                                //HostClipTileViewModel.IsHovering = false;
                                IsSubHovering = false;
                            }
                            foreach (var rtbvm in RichTextBoxViewModelCollection) {
                                if (rtbvm != this) {
                                    rtbvm.IsSubHovering = false;
                                }
                            }
                            //RichTextBoxViewModelCollection.UpdateLayout();
                        } else {
                            _detailIdx = 0;
                        }
                        //RichTextBoxViewModelCollection.OnPropertyChanged(nameof(RichTextBoxViewModelCollection.RtbListBoxHeight));
                        break;
                    case nameof(IsHoveringOnTitleTextBlock):
                        if(IsHoveringOnTitleTextBlock) {
                            _detailIdx++;
                            if (_detailIdx >= Enum.GetValues(typeof(MpCopyItemDetailType)).Length) {
                                _detailIdx = 1;
                            }
                            if ((MpCopyItemDetailType)_detailIdx == MpCopyItemDetailType.Shortcut) {
                                if (string.IsNullOrEmpty(ShortcutKeyString)) {
                                    _detailIdx++;
                                } else {
                                    DetailText = ShortcutKeyString;
                                    break;
                                }
                            }
                            DetailText = CopyItem.GetDetail((MpCopyItemDetailType)_detailIdx);
                        }
                        break;
                    case nameof(IsSubContextMenuOpened):
                        if(IsSubContextMenuOpened) {
                            IsSubSelected = true;
                        }
                        HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileBorderBrush));
                        HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.TileBorderBrushRect));
                        break;
                }
            };
        }

        public void ClipTileRichTextBoxListItemCanvas_Loaded(object sender, RoutedEventArgs e) {
            Rtbc = (Canvas)sender;
            Rtb = (RichTextBox)Rtbc.FindName("RtbListBoxItemRichTextBox");
            RtbListBoxItemClipBorder = (MpClipBorder)Rtbc.FindName("RtbListBoxItemOverlayBorder");
            RtbListBoxItemOverlayDockPanel = (DockPanel)Rtbc.FindName("RtbListItemOverlayBorderDockPanel");
            RtbListBoxItemTitleTextBlock = (TextBlock)Rtbc.FindName("RtbListBoxItemTitleTextBlock");
            RtbListBoxItemTitleTextBox = (TextBox)Rtbc.FindName("RtbListBoxItemTitleTextBox");
            DragButton = (Button)Rtbc.FindName("RtbListBoxItemOverlayBorderGridDragButton");

            HostClipTileViewModel.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);

            Rtb.TextChanged += (s, e44) => {
                RichTextBoxViewModelCollection.UpdateLayout();
            };

            Rtb.SelectionChanged += (s, e3) => {
                if(IsEditingContent) {
                    HostClipTileViewModel.EditRichTextBoxToolbarViewModel.Rtb_SelectionChanged(Rtb, e3);
                }
            };

            //if (HasTemplate) 
            {
                ClearHyperlinks();
            }
            CreateHyperlinks();

            #region Drag & Drop
            var mouseDownPosition = new Point();
            int minDragDist = 10;
            DragButton.PreviewGiveFeedback += (s, e3) => {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                    Application.Current.MainWindow.Cursor = Cursors.Cross;
                    Application.Current.MainWindow.ForceCursor = true;
                } 
            };
            DragButton.PreviewMouseUp += (s, e9) => {
                Application.Current.MainWindow.ForceCursor = false;
                mouseDownPosition = new Point();
                _dragDataObject = null;
                IsSubDragging = false;
                RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, false);
            };
            DragButton.PreviewMouseMove += (s, e7) => {
                if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                    if(IsEditingContent || (HostClipTileViewModel.IsExpanded && RichTextBoxViewModelCollection.Count == 1)) {
                        //cannot resort w/ only 1 item and its relative location is not clear
                        //since its isolated
                        return;
                    }
                    RichTextBoxViewModelCollection.SyncMultiSelectDragButton(false, true);
                    if(mouseDownPosition == new Point()) {
                        mouseDownPosition = e7.GetPosition(Rtbc);
                    }
                    if (MpHelpers.Instance.DistanceBetweenPoints(mouseDownPosition, e7.GetPosition(Rtbc)) < minDragDist) {
                        return;
                    }
                    IsSubDragging = true;
                    IsSubSelected = true;
                    if(_dragDataObject == null) {
                        _dragDataObject = MainWindowViewModel.ClipTrayViewModel.GetDataObjectFromSelectedClips(true).Result;//RichTextBoxViewModelCollection.GetDataObjectFromSubSelectedItems(true).Result;
                    }
                    DragDrop.DoDragDrop(
                                Rtbc,
                                _dragDataObject,
                                DragDropEffects.Copy | DragDropEffects.Move);
                    e.Handled = true;
                }
            };
            DragButton.MouseEnter += (s, e2) => {
                IsOverDragButton = true;
                RichTextBoxViewModelCollection.SyncMultiSelectDragButton(true, false);
            };
            DragButton.MouseLeave += (s, e2) => {
                IsOverDragButton = false;
                RichTextBoxViewModelCollection.SyncMultiSelectDragButton(false, false);
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
                IsSubHovering = true;
            };
            Rtbc.MouseLeave += (s, e2) => {
                IsSubHovering = false;
            };

            //Rtbc.PreviewMouseDown += (s, e4) => {
            //    IsSubSelected = true;
            //    e4.Handled = true;
            //};
            //Rtb.MouseEnter += (s, e2) => {
            //    if(MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1) {
            //        IsSubHovering = true;
            //    }
            //};
            //Rtb.MouseLeave += (s, e2) => {
            //    if (MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count > 1) {
            //        IsSubHovering = false;
            //    }
            //};

            //Rtbc.PreviewMouseRightButtonDown += (s, e6) => {
            //    e6.Handled = true;
            //    HostClipTileViewModel.IsSubContextMenuOpened = true;
            //    IsSubSelected = true;
            //    var contextEvent = new RoutedEventArgs(
            //        MpClipBorder.ContextMenuOpeningEvent,
            //        Rtbc);
            //    //var rightClickEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)DateTime.Now.Ticks, MouseButton.Right);
            //    HostClipTileViewModel.ClipTile_ContextMenu_Opened(Rtbc, contextEvent);
            //};

            #region Title
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
            #endregion

            UpdateLayout();
            

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

            RefreshCommands();

            OnPropertyChanged(nameof(TagMenuItems));

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }
        #endregion      

        public void RefreshCommands() {
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
            OnPropertyChanged(nameof(RtbOverlayWidth));

            
            Rtbc?.UpdateLayout();
            Rtb?.UpdateLayout();
            RtbListBoxItemClipBorder?.UpdateLayout();  

        }


        public IDataObject GetTextDataObject() {
            if(Rtb == null) {
                return null;
            }
            string selectedText = string.Empty;
            if(Rtb.Selection.IsEmpty) {
                selectedText = CopyItemPlainText;
            } else {
                selectedText = Rtb.Selection.Text;
            }
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Text, selectedText);
            dataObject.SetData(DataFormats.Rtf, selectedText.ToRichText());
            return dataObject;
        }

        public TextPointer GetRtbPointerUnderPosition(Point mp) {
            if (Rtb == null) {
                return null;
            }
            foreach(var rtbvm in RichTextBoxViewModelCollection) {
                if(rtbvm.IsSubHovering && rtbvm != this) {
                    return rtbvm.Rtb.CaretPosition;
                }
            }
            return null;
        }

        public void Resize(double deltaWidth) {
            if(IsSubSelected && HostClipTileViewModel.IsExpanded) {

            }
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

            //int sso = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.Selection.Start);
            //int seo = Rtb.Document.ContentStart.GetOffsetToPosition(Rtb.Selection.End);

            //remove links to update model rich text
            ClearHyperlinks();

            //clear any search highlighting when saving the document then restore after save
            HostClipTileViewModel.HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(this);

            CopyItemRichText = Rtb.Document.ToRichText();
            HostClipTileViewModel.HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(this);
            CreateHyperlinks();

            var cipcsw = new Stopwatch();
            cipcsw.Start();
            //if(CopyItemType == MpCopyItemType.RichText) {
            //    CopyItemBmp = RichTextBoxViewModelCollection[0].CopyItemBmp;
            //}

            HostClipTileViewModel.CopyItemBmp = HostClipTileViewModel.CopyItem.GetSeparatedCompositeFlowDocument().ToBitmapSource();
            //OnPropertyChanged(nameof(CopyItem));
            cipcsw.Stop();
            Console.WriteLine("Saving cliptile copyitem propertychanged time: " + cipcsw.ElapsedMilliseconds + "ms");

            sw.Stop();
            Console.WriteLine("Saving(VIdx:" + RichTextBoxViewModelCollection.IndexOf(this) + "): " + sw.ElapsedMilliseconds + "ms");

            //since rtb.document is recreated when ci.rtf is updated the initial selection
            //must be retrieved since the document is not the same
            //var stp = Rtb.Document.ContentStart.GetPositionAtOffset(sso);
            //var etp = Rtb.Document.ContentStart.GetPositionAtOffset(seo);
            //Rtb.Selection.Select(stp, etp);
        }

        //public async Task<string> GetPastableRichText() {
        //    if (HasTemplate) {
        //        TemplateRichText = string.Empty;
        //        //RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(RichTextBoxViewModelCollection.IndexOf(this),false);
                
        //        await Task.Run(() => {
        //            while (string.IsNullOrEmpty(TemplateRichText)) {
        //                System.Threading.Thread.Sleep(500);
        //            }
        //            //TemplateRichText is set in PasteTemplateCommand
        //        });

        //        return TemplateRichText;
        //    }
        //    return CopyItemRichText;

        //    //both return to ClipTray.GetDataObjectFromSelectedClips
        //}

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
                //File or folder path
                @"^(?:[\w]\:|\\)(\\[a-zA-Z_\-\s0-9\.()~!@#$%^&=+';,{}\[\]]+)+(\.("+Properties.Settings.Default.KnownFileExtensionsPsv+@")|(\\|\w))$",
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
                var linkType = (MpSubTextTokenType)(i + 1);                
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
                if(linkType == MpSubTextTokenType.TemplateSegment) {
                    linkType = MpSubTextTokenType.TemplateSegment;
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
            if (MainWindowViewModel.IsLoading) {
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
            if (MainWindowViewModel.IsLoading) {
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
                PasteSubItemCommand, null);
        }
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
            RtbListBoxItemAdornerLayer = null;
        }
        #endregion
    }

}
