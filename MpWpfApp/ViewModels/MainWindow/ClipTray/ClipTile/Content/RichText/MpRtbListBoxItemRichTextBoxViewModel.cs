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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpRtbListBoxItemRichTextBoxViewModel : MpUndoableViewModelBase<MpRtbListBoxItemRichTextBoxViewModel>, ICloneable {
        #region Private Variables
        private int _detailIdx = 0;
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
                if(HostClipTileViewModel == null) {
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

        private MpClipTileRichTextBoxOverlayViewModel _clipTileRichTextBoxOverlayViewModel;
        public MpClipTileRichTextBoxOverlayViewModel ClipTileRichTextBoxOverlayViewModel {
            get {
                return _clipTileRichTextBoxOverlayViewModel;
            }
            set {
                if(_clipTileRichTextBoxOverlayViewModel != value) {
                    _clipTileRichTextBoxOverlayViewModel = value;
                    OnPropertyChanged(nameof(ClipTileRichTextBoxOverlayViewModel));
                }
            }
        }
        #endregion

        #region Controls 
        public RichTextBox Rtb { get; set; }

        public DockPanel RtbListBoxItemOverlayDockPanel { get; set; }

        public MpClipBorder RtbListBoxItemClipBorder { get; set; }

        public TextBlock RtbListBoxItemTitleTextBlock { get; set; }

        public TextBox RtbListBoxItemTitleTextBox { get; set; }

        public Canvas Rtbc { get; set; }

        public AdornerLayer RtbcAdornerLayer { get; set; }
        #endregion

        #region Layout
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

        public Thickness RtbMargin {
            get {
                if (HostClipTileViewModel == null) {
                    return new Thickness(0);
                }
                var mm = MpMeasurements.Instance.RtbEditModeMinMargin;
                if (IsCompositeChild && (!IsSubSelected || !HostClipTileViewModel.IsExpanded)) {
                    if(IsHovering) {
                        return new Thickness(DragButtonSize + mm, RtbListBoxItemTitleFontSize + mm, mm, mm);
                    }
                    return new Thickness(mm);
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
                        RtbPageWidth + RtbMargin.Left + RtbMargin.Right,
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
                    if (HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 1) {
                        return Math.Max(
                            RtbPageHeight + RtbMargin.Top + RtbMargin.Bottom,
                            HostClipTileViewModel.RichTextBoxViewModelCollection.RtbListBoxHeight);
                    }
                    return RtbPageHeight;// + RtbMargin.Top + RtbMargin.Bottom;
                }
                if (HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 1) {
                    return HostClipTileViewModel.TileContentHeight - RtbMargin.Top - RtbMargin.Bottom;
                }
                var doc = Rtb == null ? CopyItem.ItemFlowDocument: Rtb.Document;
                doc.PageWidth = HostClipTileViewModel.TileContentWidth;
                double itemHeight = Math.Max(MpMeasurements.Instance.RtbCompositeItemMinHeight, doc.GetDocumentSize().Height);
                return itemHeight + RtbMargin.Top + RtbMargin.Bottom;
            }
        }      

        public double RtbPageWidth {
            get {
                if(HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    var width = Math.Max(
                        Rtb.Document.GetDocumentSize().Width,
                        HostClipTileViewModel.TileContentWidth);
                    return width - (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
                }
                return RtbWidth - RtbMargin.Left - RtbMargin.Right;
            }
        }

        public double RtbPageHeight {
            get {
                if (HostClipTileViewModel == null) {
                    return 0;
                }
                if (HostClipTileViewModel.IsExpanded) {
                    return Rtb.Document.GetDocumentSize().Height;
                }
                return RtbHeight - RtbMargin.Top - RtbMargin.Bottom;
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
                if(CopyItem == null) {
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
                if(!IsCompositeChild) {
                    return Brushes.Transparent;
                }
                if (IsPrimarySelected) {
                    return Brushes.Blue;
                }
                if (IsSubSelected) {
                    return Brushes.Red;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        public Brush RtbListBoxItemBackgroundColor {
            get {
                if (CopyItem == null || HostClipTileViewModel == null) {
                    return Brushes.Transparent;
                }
                if(!IsCompositeChild) {
                    return Brushes.Transparent;
                }
                if(IsSubSelected && HostClipTileViewModel.IsExpanded) {
                    return Brushes.Transparent;
                }
                if (IsHovering) {
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

        public Brush RtbBorderBrush {
            get {
                if(HostClipTileViewModel == null) {
                    return Brushes.Transparent;
                }
                if(!HostClipTileViewModel.IsExpanded) {
                    return Brushes.Transparent;
                }
                if(IsSubSelected) {
                    return Brushes.Red;
                }
                if(IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region Visibility
        public Visibility ToolTipVisibility {
            get {
                if (HostClipTileViewModel == null) {
                    return Visibility.Collapsed;
                }
                return HostClipTileViewModel.IsSelected ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility SubItemOverlayVisibility {
            get {
                if(HostClipTileViewModel == null) {
                    return Visibility.Collapsed;
                }
                if(IsEditingSubTitle) {
                    return Visibility.Visible;
                }
                if (HostClipTileViewModel.IsExpanded && IsSubSelected) {
                    return Visibility.Collapsed;
                }
                if(IsCompositeChild && IsHovering) {
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
                if(HostClipTileViewModel == null || 
                   HostClipTileViewModel.RichTextBoxViewModelCollection == null || 
                   HostClipTileViewModel.RichTextBoxViewModelCollection.Count <= 1) {
                    return null;
                }
                int nextIdx = CompositeSortOrderIdx + 1;
                if(nextIdx >= HostClipTileViewModel.RichTextBoxViewModelCollection.Count) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection[nextIdx];
            }
        }

        public MpRtbListBoxItemRichTextBoxViewModel Previous {
            get {
                if (HostClipTileViewModel == null ||
                   HostClipTileViewModel.RichTextBoxViewModelCollection == null ||
                   HostClipTileViewModel.RichTextBoxViewModelCollection.Count <= 1) {
                    return null;
                }
                int prevIdx = CompositeSortOrderIdx - 1;
                if (prevIdx < 0) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection[prevIdx];
            }
        }
        #endregion 

        #region Business Logic 
        private string _shortcutKeyString = string.Empty;
        public string ShortcutKeyString {
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

        public bool HasTemplate {
            get {
                return TemplateHyperlinkCollectionViewModel.Count > 0;
            }
        }
        public string TemplateRichText { get; set; }

        private BitmapSource _previewBmpSrc;
        public BitmapSource PreviewBmpSrc {
            get {
                return _previewBmpSrc;
            }
            set {
                if(_previewBmpSrc != value) {
                    _previewBmpSrc = value;
                    CopyItem.ItemBitmapSource = _previewBmpSrc;
                    OnPropertyChanged(nameof(PreviewBmpSrc));
                }
            }
        }
        #endregion

        #region State
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

        private bool _isPrimarySelected = false;
        public bool IsPrimarySelected {
            get {
                return _isPrimarySelected;
            }
            set {
                if (_isPrimarySelected != value) {
                    _isPrimarySelected = value;
                    OnPropertyChanged(nameof(IsPrimarySelected));
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
                    OnPropertyChanged(nameof(SubItemOverlayVisibility));
                    OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
                    OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
                    OnPropertyChanged(nameof(RtbBorderBrush));
                    OnPropertyChanged(nameof(ToolTipVisibility));
                    OnPropertyChanged(nameof(RtbMargin));
                }
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(SubItemOverlayVisibility));
                    OnPropertyChanged(nameof(RtbListBoxItemBackgroundColor));
                    OnPropertyChanged(nameof(RtbListBoxItemBorderBrush));
                    OnPropertyChanged(nameof(RtbMargin));
                    OnPropertyChanged(nameof(RtbCanvasHeight));
                    OnPropertyChanged(nameof(RtbCanvasWidth));
                    OnPropertyChanged(nameof(RtbCanvasHeight));
                    OnPropertyChanged(nameof(RtbPageWidth));
                    OnPropertyChanged(nameof(RtbPageHeight));
                    OnPropertyChanged(nameof(ToolTipVisibility));
                    OnPropertyChanged(nameof(RtbBorderBrush));
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
                }
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
                if(HostClipTileViewModel == null) {
                    return false;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection.Count > 1;
            }
        }
        #endregion

        #region Model
        public string DetailText {
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

        public string CopyItemPlainText {
            get {
                if (CopyItem == null || CopyItem.ItemPlainText == null) {
                    return string.Empty;
                }
                return CopyItem.ItemPlainText;
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
                    OnPropertyChanged(nameof(CopyItem));
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
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                if(_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CopyItemId));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(CopyItemFilePath));
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CopyItemPlainText));
                    OnPropertyChanged(nameof(CopyItemTitle));
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
            ClipTileRichTextBoxOverlayViewModel = new MpClipTileRichTextBoxOverlayViewModel(this);

            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsSubSelected):
                        //if(!IsSubSelected && IsEditingSubTitle) {
                        //    IsEditingSubTitle = false;
                        //}
                        if(IsSubSelected) {
                            foreach(var rtbvm in HostClipTileViewModel.RichTextBoxViewModelCollection) {
                                if(rtbvm != this) {
                                    rtbvm.IsSubSelected = false;
                                    rtbvm.IsEditingSubTitle = false;
                                }
                            }
                        } else {
                            PreviewBmpSrc = FlowDocumentToBitmap(Rtb.Document.Clone(), Rtb.Document.GetDocumentSize());
                            CopyItem.WriteToDatabase();
                        }
                        HostClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel));
                        HostClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.SelectedRtb));
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

            Rtb.Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItemRichText);

            Rtb.SelectAll();
            var rtbAlignment = Rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty);
            if (rtbAlignment == null || 
                rtbAlignment.ToString() == "{DependencyProperty.UnsetValue}" ||
                (TextAlignment)rtbAlignment == TextAlignment.Justify) {
                Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
            }
            Rtb.CaretPosition = Rtb.Document.ContentStart;

            Rtb.TextChanged += (s, e44) => {
                //RtbcAdornerLayer.Update();
                OnPropertyChanged(nameof(RtbPageWidth));
                OnPropertyChanged(nameof(RtbPageHeight));
                OnPropertyChanged(nameof(RtbHeight));
                OnPropertyChanged(nameof(RtbCanvasHeight));
                //HostClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.RtbListBoxHeight));
                //HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.RtbVerticalScrollbarVisibility));
                //HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.RtbHorizontalScrollbarVisibility));
                //HostClipTileViewModel.RichTextBoxListBox.UpdateLayout();
                var rtblb = HostClipTileViewModel.RichTextBoxListBox;
                var border = (Border)VisualTreeHelper.GetChild(rtblb, 0);
                var sv = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                sv.InvalidateScrollInfo();
                Console.WriteLine("Total Item Height: " + HostClipTileViewModel.RichTextBoxViewModelCollection.TotalItemHeight);
            };

            if (HasTemplate) {
                ClearHyperlinks();
            }
            CreateHyperlinks();

            #region Drag & Drop
            #endregion


            if (HostClipTileViewModel.WasAddedAtRuntime) {
                //force new items to have left alignment
                Rtb.SelectAll();
                Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                Rtb.CaretPosition = Rtb.Document.ContentStart;

                PreviewBmpSrc = FlowDocumentToBitmap(Rtb.Document.Clone(), Rtb.Document.GetDocumentSize());
                CopyItem.WriteToDatabase();
            } else {
                PreviewBmpSrc = CopyItem.ItemBitmapSource;
            }

            RtbcAdornerLayer = AdornerLayer.GetAdornerLayer(Rtbc);
            RtbcAdornerLayer.Add(new MpRichTextBoxOverlayAdorner(Rtbc));

            Rtbc.MouseEnter += (s, e2) => {
                if(!MainWindowViewModel.ClipTrayViewModel.IsEditingClipTitle &&
                   !IsSubSelected) {
                    IsHovering = true;
                }
                
            };
            Rtbc.MouseLeave += (s, e2) => {
                if (!MainWindowViewModel.ClipTrayViewModel.IsEditingClipTitle &&
                    !IsSubSelected) {
                    IsHovering = false;
                }
            };

            RtbListBoxItemClipBorder.MouseLeftButtonDown += (s, e9) => {
                SelectItemCommand.Execute(null);
            };

            RtbListBoxItemTitleTextBlock.PreviewMouseLeftButtonDown += (s, e7) => {
                //HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
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

            OnPropertyChanged(nameof(SubItemOverlayVisibility));
            
            OnPropertyChanged(nameof(RtbMargin));
            OnPropertyChanged(nameof(RtbHeight));
            OnPropertyChanged(nameof(RtbCanvasHeight));
        }

        public BitmapSource FlowDocumentToBitmap(FlowDocument document, Size size) {
            document.PagePadding = new Thickness(0);
            document.ColumnWidth = size.Width;
            document.PageWidth = size.Width;
            document.PageHeight = size.Height;

            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            paginator.PageSize = size;
            
            var visual = new DrawingVisual(); 
            using (var drawingContext = visual.RenderOpen()) {
                // draw white background
                drawingContext.DrawRectangle(Brushes.White, null, new Rect(size));
            }
            visual.Children.Add(paginator.GetPage(0).Visual);

            var bitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height,
                                                96, 96, PixelFormats.Pbgra32);
            
            bitmap.Render(visual);
            return bitmap;
        }

        public void SetSelection(bool newSelection, bool isInitEdit, bool isInitPaste) {
            //if(IsSubSelected == newSelection) {
            //    return;
            //}
            //HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
            IsSubSelected = newSelection;
            if(IsSubSelected) {
                if (isInitEdit) {
                    HostClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(Rtb, isInitEdit);
                    HostClipTileViewModel.EditTemplateToolbarViewModel.InitWithRichTextBox(Rtb);
                } else if (isInitPaste) {
                    HostClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(Rtb, isInitPaste);
                }

                //Rtb.Focus();
                //Rtb.ScrollToEnd();                
            }
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                TemplateRichText = string.Empty;
                //HostClipTileViewModel.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(HostClipTileViewModel.RichTextBoxViewModelCollection.IndexOf(this),false);
                

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
            var rtbSelection = Rtb.Selection;
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

            var rtbSelection = Rtb.Selection.Clone();
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
                            if (matchRange == null) {
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
                                convertToQrCodeMenuItem.Click += async (s5, e1) => {
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
            var rtbSelection = Rtb.Selection;
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
        private RelayCommand _selectItemCommand;
        public ICommand SelectItemCommand {
            get {
                if (_selectItemCommand == null) {
                    _selectItemCommand = new RelayCommand(SelectItem, CanSelectItem);
                }
                return _selectItemCommand;
            }
        }
        private bool CanSelectItem() {
            return HostClipTileViewModel.IsExpanded && !MainWindowViewModel.ClipTrayViewModel.IsEditingClipTitle;
        }
        private void SelectItem() {
            SetSelection(true, false, false);
        }
        #endregion
    }

}
