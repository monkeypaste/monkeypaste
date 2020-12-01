namespace MpWpfApp {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Speech.Synthesis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Xml;
    using GalaSoft.MvvmLight.CommandWpf;
    using Gma.System.MouseKeyHook;
    using GongSolutions.Wpf.DragDrop.Utilities;
    using NativeCode;
    using Windows.ApplicationModel.DataTransfer;
    using Windows.Storage;
    using WPF.JoshSmith.ServiceProviders.UI;

    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables

        private int _detailIdx = 0;

        private List<string> _tempFileList = new List<string>();

        private ToggleButton _selectedAlignmentButton = null;
        private ToggleButton _selectedListButton = null;
        #endregion

        #region View Models

        private ObservableCollection<MpClipTileContextMenuItemViewModel> _convertClipTypes = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
        public ObservableCollection<MpClipTileContextMenuItemViewModel> ConvertClipTypes {
            get {
                return _convertClipTypes;
            }
            set {
                if (_convertClipTypes != value) {
                    _convertClipTypes = value;
                    OnPropertyChanged(nameof(ConvertClipTypes));
                }
            }
        }

        private ObservableCollection<MpClipTileContextMenuItemViewModel> _tagMenuItems = new ObservableCollection<MpClipTileContextMenuItemViewModel>();
        public ObservableCollection<MpClipTileContextMenuItemViewModel> TagMenuItems {
            get {
                return _tagMenuItems;
            }
            set {
                if(_tagMenuItems != value) {
                    _tagMenuItems = value;
                    OnPropertyChanged(nameof(TagMenuItems));
                }
            }
        }

        public ObservableCollection<MpFileListItemViewModel> FileListViewModels {
            get {
                var fileListViewModels = new ObservableCollection<MpFileListItemViewModel>();
                foreach (var path in CopyItem.GetFileList()) {
                    fileListViewModels.Add(new MpFileListItemViewModel(this, path));
                }
                return fileListViewModels;
            }
        }

        #endregion

        #region Property Reflection Referencer
        public object this[string propertyName] {
            get {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null) {
                    throw new Exception("Unable to find property: " + propertyName);
                }
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
        #endregion

        #region Properties

        #region Layout Properties
        private double _tileTitleIconSize = MpMeasurements.Instance.ClipTileTitleIconSize;
        public double TileTitleIconSize {
            get {
                return _tileTitleIconSize;
            }
            set {
                if (_tileTitleIconSize != value) {
                    _tileTitleIconSize = value;
                    OnPropertyChanged(nameof(TileTitleIconSize));
                }
            }
        }

        private double _tileSize = MpMeasurements.Instance.ClipTileSize;
        public double TileSize {
            get {
                return _tileSize;
            }
            set {
                if (_tileSize != value) {
                    _tileSize = value;
                    OnPropertyChanged(nameof(TileSize));
                }
            }
        }

        private double _tileBorderWidth = MpMeasurements.Instance.ClipTileBorderSize;
        public double TileBorderWidth {
            get {
                return _tileBorderWidth;
            }
            set {
                if (_tileBorderWidth != value) {
                    _tileBorderWidth = value;
                    OnPropertyChanged(nameof(TileBorderWidth));
                }
            }
        }

        private double _tileBorderHeight = MpMeasurements.Instance.ClipTileSize;
        public double TileBorderHeight {
            get {
                return _tileBorderHeight;
            }
            set {
                if (_tileBorderHeight != value) {
                    _tileBorderHeight = value;
                    OnPropertyChanged(nameof(TileBorderHeight));
                }
            }
        }

        private double _tileTitleHeight = MpMeasurements.Instance.ClipTileTitleHeight;
        public double TileTitleHeight {
            get {
                return _tileTitleHeight;
            }
            set {
                if (_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged(nameof(TileTitleHeight));
                }
            }
        }

        private double _tileContentHeight = MpMeasurements.Instance.ClipTileContentHeight;
        public double TileContentHeight {
            get {
                return _tileContentHeight;
            }
            set {
                if (_tileContentHeight != value) {
                    _tileContentHeight = value;
                    OnPropertyChanged(nameof(TileContentHeight));
                }
            }
        }

        private double _tileContentWidth = MpMeasurements.Instance.ClipTileContentWidth;
        public double TileContentWidth {
            get {
                return _tileContentWidth;
            }
            set {
                if (_tileContentWidth != value) {
                    _tileContentWidth = value;
                    OnPropertyChanged(nameof(TileContentWidth));
                }
            }
        }

        private double _tileBorderThickness = MpMeasurements.Instance.ClipTileBorderThickness;
        public double TileBorderThickness {
            get {
                return _tileBorderThickness;
            }
            set {
                if (_tileBorderThickness != value) {
                    _tileBorderThickness = value;
                    OnPropertyChanged(nameof(TileBorderThickness));
                }
            }
        }

        private double _tileMargin = MpMeasurements.Instance.ClipTileMargin;
        public double TileMargin {
            get {
                return _tileMargin;
            }
            set {
                if (_tileMargin != value) {
                    _tileMargin = value;
                    OnPropertyChanged(nameof(TileMargin));
                }
            }
        }

        private double _tileContentMargin = MpMeasurements.Instance.ClipTileContentMargin;
        public double TileContentMargin {
            get {
                return _tileContentMargin;
            }
            set {
                if (_tileContentMargin != value) {
                    _tileContentMargin = value;
                    OnPropertyChanged(nameof(TileContentMargin));
                }
            }
        }

        private double _tileDropShadowRadius = MpMeasurements.Instance.ClipTileDropShadowRadius;
        public double TileDropShadowRadius {
            get {
                return _tileDropShadowRadius;
            }
            set {
                if (_tileDropShadowRadius != value) {
                    _tileDropShadowRadius = value;
                    OnPropertyChanged(nameof(TileDropShadowRadius));
                }
            }
        }
        #endregion

        #region Visibility Properties
        public Visibility ImgVisibility {
            get {
                if (CopyItemType == MpCopyItemType.Image) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility FileListVisibility {
            get {
                if (CopyItemType == MpCopyItemType.FileList) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility RtbVisibility {
            get {
                if(CopyItemType == MpCopyItemType.RichText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TileTitleTextBlockVisibility {
            get {
                if(IsEditingTitle) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility TileTitleTextBoxVisibility {
            get {
                if (IsEditingTitle) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private Visibility _tileVisibility = Visibility.Visible;
        public Visibility TileVisibility {
            get {
                return _tileVisibility;
            }
            set {
                if (_tileVisibility != value) {
                    _tileVisibility = value;
                    OnPropertyChanged(nameof(TileVisibility));
                }
            }
        }

        public Visibility EditToolbarVisibility {
            get {
                if(IsEditingTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Business Logic Properties
        public string SearchText {
            get {
                return MainWindowViewModel.SearchBoxViewModel.SearchText;
            }
        }

        public bool IsLoading { get; set; } = false;

        public string DetailText {
            get {
                return GetCurrentDetail(_detailIdx);
            }
        }

        private int _sortOrderIdx = -1;
        public int SortOrderIdx {
            get {
                return _sortOrderIdx;
            }
            set {
                if (_sortOrderIdx != value) {
                    _sortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }
        #endregion

        #region Brush Properties
        public Brush DetailTextColor {
            get {
                if(IsSelected) {
                    return Brushes.DarkGray;
                }
                if(IsHovering) {
                    return Brushes.DimGray;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TitleColor {
            get {
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                if (CopyItem.ItemColor.Color != ((SolidColorBrush)value).Color) {
                    CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                    OnPropertyChanged(nameof(TitleColor));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public Brush TileBorderBrush {
            get {
                if (IsSelected) {
                    return Brushes.Red;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region State Properties
        private bool _isEditingTitle = false;
        public bool IsEditingTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if (_isEditingTitle != value) {
                    _isEditingTitle = value;
                    OnPropertyChanged(nameof(IsEditingTitle));
                    OnPropertyChanged(nameof(TileTitleTextBlockVisibility));
                    OnPropertyChanged(nameof(TileTitleTextBoxVisibility));
                }
            }
        }

        private bool _isEditingTile = false;
        public bool IsEditingTile {
            get {
                return _isEditingTile;
            }
            set {
                if (_isEditingTile != value) {
                    _isEditingTile = value;
                    OnPropertyChanged(nameof(IsEditingTile));
                    OnPropertyChanged(nameof(IsRtbReadOnly));
                    OnPropertyChanged(nameof(ContentCursor));
                    OnPropertyChanged(nameof(EditToolbarVisibility));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        public bool IsRtbReadOnly {
            get {
                return !IsEditingTile;
            }
        }

        public bool IsNew {
            get {
                return CopyItem.CopyItemId == 0;
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
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
                    OnPropertyChanged(nameof(TileBorderBrush));
                    OnPropertyChanged(nameof(DetailTextColor));
                }
            }
        }
        #endregion

        #region Text Editor Properties
        public ICollection<FontFamily> SystemFonts {
            get {
                return Fonts.SystemFontFamilies;
            }
        }

        private List<string> _fontSizeList = null;
        public List<string> FontSizeList {
            get {
                if(_fontSizeList == null) {
                    _fontSizeList = new List<string>() {
                        "10",
                        "12",
                        "14",
                        "18",
                        "24",
                        "36"
                    };
                }
                return _fontSizeList;
            }
            set {
                if(_fontSizeList != value) {
                    _fontSizeList = value;
                    OnPropertyChanged(nameof(FontSizeList));
                }
            }
        }

        public Cursor ContentCursor {
            get {
                if(IsEditingTile) {
                    return Cursors.IBeam;
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Model Propertiese
        private string _shortcutKeyList = string.Empty;
        public string ShortcutKeyList {
            get {
                return _shortcutKeyList;
            }
            set {
                if (_shortcutKeyList != value) {
                    _shortcutKeyList = value;
                    OnPropertyChanged(nameof(ShortcutKeyList));
                }
            }
        }

        public int CopyItemId {
            get {
                return CopyItem.CopyItemId;
            }
        }

        public string CopyItemTitle {
            get {
                return CopyItem.Title;
            }
            set {
                if (CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged(nameof(CopyItemTitle));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        //public string CopyItemXamlText {
        //    get {
        //        return CopyItem.ItemXaml;
        //    }
        //    set {
        //        if (CopyItem.ItemXaml != value) {
        //            CopyItem.ItemXaml = value;
        //            OnPropertyChanged(nameof(CopyItemXamlText));
        //            OnPropertyChanged(nameof(CopyItem));
        //        }
        //    }
        //}

        //for drag/drop,export and paste not view
        public string CopyItemPlainText {
            get {
                return CopyItem.ItemPlainText;
            }
        }

        public string CopyItemRichText {
            get {
                return CopyItem.ItemRichText;
            }
            set {
                //if (CopyItem.ItemRichText != value) 
                    {
                    CopyItem.SetData(value);
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }       

        public BitmapSource CopyItemBmp {
            get {
                return CopyItem.ItemBitmapSource;
            }
        }

        public List<string> CopyItemFileDropList {
            get {
                return CopyItem.GetFileList(string.Empty, MainWindowViewModel.ClipTrayViewModel.GetTargetFileType());
            }
        }

        private List<Hyperlink> _tokens = new List<Hyperlink>();
        public List<Hyperlink> TokenList {
            get {
                return _tokens;
            }
            set {
                if (_tokens != value) {
                    _tokens = value;
                    OnPropertyChanged(nameof(TokenList));
                }
            }
        }

        private BitmapSource _titleSwirl = null;
        public BitmapSource TitleSwirl {
            get {
                return _titleSwirl;
            }
            set {
                if (_titleSwirl != value) {
                    _titleSwirl = value;
                    OnPropertyChanged(nameof(TitleSwirl));
                }
            }
        }

        public BitmapSource CopyItemAppIcon {
            get {
                return CopyItem.App.Icon.IconImage;
            }
        }

        public string CopyItemAppName {
            get {
                return CopyItem.App.AppName;
            }
        }

        public int CopyItemUsageScore {
            get {
                return CopyItem.RelevanceScore;
            }
        }

        public int CopyItemAppId {
            get {
                return CopyItem.AppId;
            }
        }

        public MpCopyItemType CopyItemType {
            get {
                return CopyItem.CopyItemType;
            }
        }

        public DateTime CopyItemCreatedDateTime {
            get {
                return CopyItem.CopyDateTime;
            }
        }

        private MpCopyItem _copyItem = new MpCopyItem();
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                if (_copyItem != value) {
                    _copyItem = value;

                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CopyItemId));
                    OnPropertyChanged(nameof(CopyItemTitle));
                    OnPropertyChanged(nameof(CopyItemPlainText));
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CopyItemBmp));
                    OnPropertyChanged(nameof(CopyItemFileDropList));
                    OnPropertyChanged(nameof(CopyItemAppIcon));
                    OnPropertyChanged(nameof(CopyItemAppName));
                    OnPropertyChanged(nameof(CopyItemUsageScore));
                    OnPropertyChanged(nameof(CopyItemAppId));
                    OnPropertyChanged(nameof(CopyItemType));
                    OnPropertyChanged(nameof(CopyItemCreatedDateTime));
                    OnPropertyChanged(nameof(DetailText));
                    OnPropertyChanged(nameof(FileListViewModels));

                    CopyItem.WriteToDatabase();
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileViewModel() : this(new MpCopyItem()) { }

        public MpClipTileViewModel(MpCopyItem ci) {
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.SearchBoxViewModel.IsFocused) {
                                IsFocused = true;
                            }
                        } else {
                            //below must be called to clear focus when deselected (it may not have focus)
                            IsFocused = false;
                            IsEditingTile = false;
                        }
                        break;
                }
            };            

            IsLoading = true;
            CopyItem = ci;

            InitSwirl();
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            MainWindowViewModel.SearchBoxViewModel.PropertyChanged += (s, e2) => {
                switch (e2.PropertyName) {
                    case nameof(MainWindowViewModel.SearchBoxViewModel.SearchText):
                        OnPropertyChanged(nameof(SearchText));
                        break;
                }
            };

            var clipTileBorder = (MpClipBorder)sender;
            clipTileBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            clipTileBorder.MouseLeave += (s, e2) => {
                IsHovering = false;
            };
            clipTileBorder.LostFocus += (s, e4) => {
                if(!IsSelected) {
                    IsEditingTitle = false;
                }
            };
            clipTileBorder.PreviewMouseLeftButtonDown += (s, e5) => {
                if (e5.ClickCount == 2) {
                    //only for richtext type
                    EditClipTextCommand.Execute(null);
                    e5.Handled = true;
                    return;
                }
            };

        }

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            var titleCanvas = (Canvas)sender;
            
            var clipTileTitleTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleTextBlock");
            clipTileTitleTextBlock.MouseEnter += (s, e1) => {
                Application.Current.MainWindow.Cursor = Cursors.IBeam;
            };
            clipTileTitleTextBlock.MouseLeave += (s, e7) => {
                Application.Current.MainWindow.Cursor = Cursors.Arrow;
            };
            clipTileTitleTextBlock.PreviewMouseLeftButtonUp += (s, e7) => {
                IsEditingTitle = true;
            };

            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
            clipTileTitleTextBox.IsVisibleChanged += (s, e9) => {
                if(TileTitleTextBoxVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                tbx.SelectAll();
            };
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var titleIconImageButton = (Button)titleCanvas.FindName("ClipTileAppIconImageButton");
            Canvas.SetLeft(titleIconImageButton, TileBorderWidth - TileTitleHeight - 10);
            Canvas.SetTop(titleIconImageButton, 2);
            titleIconImageButton.PreviewMouseUp += (s, e7) => {
                // TODO (somehow) force mainwindow to stay active when switching or opening app process
                // TODO check if shift is down if so perform paste into target application
                // TODO check if App is running if it is switch to it or start its process

                System.Diagnostics.Process.Start(CopyItem.App.AppPath);
                
            };

            var titleDetailTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleDetailTextBlock");
            titleDetailTextBlock.MouseEnter += (s, e5) => {
                if(++_detailIdx > 2) {
                    _detailIdx = 0;
                }
                titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            };
            titleDetailTextBlock.Text = GetCurrentDetail(_detailIdx);
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, TileTitleHeight - 14);
        }

        public void ClipTileRichTextBox_Loaded(object sender, RoutedEventArgs e) {
            var rtb = (RichTextBox)sender;
            rtb.ContextMenu = (ContextMenu)rtb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
            rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
            rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;
        }

        public void ClipTileEditorToolbar_Loaded(object sender, RoutedEventArgs e) {
            var ctet = (Border)sender; 
            var cb = (MpClipBorder)ctet.GetVisualAncestor<MpClipBorder>();

            var rtb = (RichTextBox)cb.FindName("ClipTileRichTextBox");
            rtb.LostFocus += (s, e5) => {
                
            };
            
            var fontFamilyComboBox = (ComboBox)ctet.FindName("FontFamilyCombo");
            fontFamilyComboBox.SelectionChanged += (s, e1) => {
                if(fontFamilyComboBox.SelectedItem == null) {
                    return;
                }
                var fontFamily = fontFamilyComboBox.SelectedItem.ToString();
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
            };
            
            var fontSizeCombo = (ComboBox)ctet.FindName("FontSizeCombo");
            fontSizeCombo.SelectionChanged += (s, e1) => {
                // Exit if no selection
                if (fontSizeCombo.SelectedItem == null) {
                    return;
                }

                // clear selection if value unset
                if (fontSizeCombo.SelectedItem.ToString() == "{DependencyProperty.UnsetValue}") {
                    fontSizeCombo.SelectedItem = null;
                    return;
                }

                // Process selection
                var pointSize = fontSizeCombo.SelectedItem.ToString();
                var pixelSize = System.Convert.ToDouble(pointSize) * (96 / 72);
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);
            };

            var leftAlignmentButton = (ToggleButton)ctet.FindName("LeftButton");
            var centerAlignmentButton = (ToggleButton)ctet.FindName("CenterButton");
            var rightAlignmentButton = (ToggleButton)ctet.FindName("RightButton");
            var justifyAlignmentButton = (ToggleButton)ctet.FindName("JustifyButton");            
            leftAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };
            centerAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };
            rightAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };
            justifyAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, _selectedAlignmentButton, buttonGroup, true);
                _selectedAlignmentButton = clickedButton;
            };

            var bulletsButton = (ToggleButton)ctet.FindName("BulletsButton");
            var numberingButton = (ToggleButton)ctet.FindName("NumberingButton");
            bulletsButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new[] { bulletsButton, numberingButton };
                this.SetButtonGroupSelection(clickedButton, _selectedListButton, buttonGroup, false);
                _selectedListButton = clickedButton;
            };
            numberingButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new[] { bulletsButton, numberingButton };
                this.SetButtonGroupSelection(clickedButton, _selectedListButton, buttonGroup, false);
                _selectedListButton = clickedButton;
            };

            var addTemplateButton = (Button)ctet.FindName("AddTemplateButton");
            addTemplateButton.PreviewMouseDown += (s, e2) => {
                MainWindowViewModel.IsShowingDialog = true;
                e2.Handled = true;
                var result = MpTemplateTokenModalWindowViewModel.ShowTemplateTokenModalWindow(rtb);
                if(result) {
                    CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);

                } else {
                    //clear any link if cancle
                    rtb.Selection.Text = rtb.Selection.Text;
                }

                MainWindowViewModel.IsShowingDialog = false;
            };

            rtb.SelectionChanged += (s, e6) => {
                // Set font family combo
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                var fontFamily = textRange.GetPropertyValue(TextElement.FontFamilyProperty);
                fontFamilyComboBox.SelectedItem = fontFamily;

                // Set font size combo
                var fontSize = textRange.GetPropertyValue(TextElement.FontSizeProperty);
                fontSizeCombo.Text = fontSize.ToString();
                // Set Font buttons
                //if (!String.IsNullOrEmpty(textRange.Text)) {

                //}
                ((ToggleButton)ctet.FindName("BoldButton")).IsChecked = textRange.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
                ((ToggleButton)ctet.FindName("ItalicButton")).IsChecked = textRange.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
                ((ToggleButton)ctet.FindName("UnderlineButton")).IsChecked = textRange?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

                // Set Alignment buttons
                leftAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
                centerAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
                rightAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
                justifyAlignmentButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Justify);
            };

            ctet.IsVisibleChanged += (s, e1) => {
                //var cb = (MpClipBorder)etrtb.GetVisualAncestor<MpClipBorder>();
                //var trtb = (MpTokenizedRichTextBox)cb.FindName("TokenizedRichTextBox");
                var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
                var titleSwirl = (Image)cb.FindName("TitleSwirl");
                var dp = (DockPanel)cb.FindName("ClipTileRichTextDockPanel");

                double fromWidthTile = 0;
                double toWidthTile = 0;
                double fromWidthContent = 0;
                double toWidthContent = 0;
                double scrollbarWidth = 20;

                if (ctet.Visibility == Visibility.Visible) {
                    //etrtb.TokenizedRichTextBox.Document = (MpEventEnabledFlowDocument)etrtb.TokenizedRichTextBox.Document;
                    //etrtb.TokenizedRichTextBox.IsDocumentEnabled = true;
                    //etrtb.TokenizedRichTextBox.Document.IsEnabled = true;
                    fromWidthTile = MpMeasurements.Instance.ClipTileBorderSize;
                    fromWidthContent = fromWidthTile;
                    toWidthTile = Math.Max(625, rtb.Document.GetFormattedText().WidthIncludingTrailingWhitespace);
                    toWidthContent = toWidthTile - scrollbarWidth;
                    rtb.Focusable = true;
                    //etrtb.TokenizedRichTextBox.Focusable = true;                    
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                } else {
                    // TODO add check to see if template token was added if not use convertflowdoctoricktext
                    //etrtb.TokenizedRichTextBox.Document = etrtb.TokenizedRichTextBox.GetTemplateDocument();
                    //CopyItem.ItemXaml = MpHelpers.ConvertFlowDocumentToXaml(etrtb.TokenizedRichTextBox.GetTemplateDocument());
                    //CopyItem.SubTextTokenList = etrtb.TokenizedRichTextBox.Tokens.ToList();
                    CopyItem.WriteToDatabase();
                    fromWidthTile = rtb.Width;
                    fromWidthContent = fromWidthTile - scrollbarWidth;
                    toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;
                    toWidthContent = toWidthTile;
                    rtb.Focusable = false;
                    //etrtb.TokenizedRichTextBox.Focusable = false;
                    rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                }
                double fromLeft = Canvas.GetLeft(titleIconImageButton);
                double toLeft = toWidthTile - TileTitleHeight - 10;

                DoubleAnimation twa = new DoubleAnimation();
                twa.From = fromWidthTile;
                twa.To = toWidthTile;
                twa.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                CubicEase easing = new CubicEase();
                easing.EasingMode = EasingMode.EaseIn;
                twa.EasingFunction = easing;
                twa.Completed += (s1, e2) => {
                    ((MpMultiSelectListBox)cb.GetVisualAncestor<MpMultiSelectListBox>()).ScrollIntoView(cb);
                };
                cb.BeginAnimation(MpClipBorder.WidthProperty, twa);
                titleSwirl.BeginAnimation(Image.WidthProperty, twa);
                rtb.BeginAnimation(RichTextBox.WidthProperty, twa);
                ctet.BeginAnimation(Border.WidthProperty, twa);
                dp.BeginAnimation(DockPanel.WidthProperty, twa);

                DoubleAnimation cwa = new DoubleAnimation();
                cwa.From = fromWidthContent;
                cwa.To = toWidthContent;
                cwa.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                easing = new CubicEase();
                easing.EasingMode = EasingMode.EaseIn;
                cwa.EasingFunction = easing;
                

                DoubleAnimation la = new DoubleAnimation();
                la.From = fromLeft;
                la.To = toLeft;
                la.Duration = new Duration(TimeSpan.FromMilliseconds(Properties.Settings.Default.ShowMainWindowAnimationMilliseconds));
                la.EasingFunction = easing;
                titleIconImageButton.BeginAnimation(Canvas.LeftProperty, la);
            };
        }

        public void ClipTileImage_Loaded(object sender, RoutedEventArgs e) {
            var img = (Image)sender;
            img.ContextMenu = (ContextMenu)img.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
            //aspect ratio
            double ar = CopyItemBmp.Width / CopyItemBmp.Height;
            double contentWidth = 0;
            double contentHeight = 0;
            if (CopyItemBmp.Width >= CopyItemBmp.Height) {
                contentWidth = TileBorderWidth;
                contentHeight = contentWidth * ar;
            } else {
                contentHeight = TileContentHeight;
                contentWidth = contentHeight * ar;
            }
            MpHelpers.ResizeBitmapSource(CopyItemBmp, new Size((int)contentWidth, (int)contentHeight));

            Canvas.SetLeft(img, (TileBorderWidth / 2) - (contentWidth / 2));
            Canvas.SetTop(img, (TileContentHeight / 2) - (contentHeight / 2));
        }

        public void ClipTileFileListBox_Loaded(object sender, RoutedEventArgs e) {
            var flb = (ListBox)sender;
            flb.ContextMenu = (ContextMenu)flb.GetVisualAncestor<MpClipBorder>().FindName("ClipTile_ContextMenu");
        }

        public void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            TagMenuItems.Clear();
            foreach (var tagTile in MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel) {
                if (tagTile.TagName == Properties.Settings.Default.HistoryTagTitle) {
                    continue;
                }
                TagMenuItems.Add(new MpClipTileContextMenuItemViewModel(tagTile.TagName, MainWindowViewModel.ClipTrayViewModel.LinkTagToCopyItemCommand, tagTile, tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
            }
            ConvertClipTypes.Clear();
            switch(MainWindowViewModel.ClipTrayViewModel.GetSelectedClipsType()) {
                case MpCopyItemType.None:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
                case MpCopyItemType.FileList:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
                case MpCopyItemType.Image:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Text", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.RichText, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    break;
                case MpCopyItemType.RichText:
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("File List", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.FileList, false));
                    ConvertClipTypes.Add(new MpClipTileContextMenuItemViewModel("Image", MainWindowViewModel.ClipTrayViewModel.ConvertSelectedClipsCommand, (int)MpCopyItemType.Image, false));
                    break;
            }
        }

        public void AppendContent(MpClipTileViewModel octvm) {
            CopyItem.Combine(octvm.CopyItem);
            OnPropertyChanged(nameof(CopyItem));

            //reinitialize item view properties
            //PlainText = CopyItem.ItemPlainText;
            //RichText = CopyItem.ItemRichText;
            //DocumentRtf = CopyItem.ItemFlowDocument;
            //Bmp = CopyItem.ItemBitmapSource;
            //TokenList = CopyItem.SubTextTokenList;
            //FileDropList = CopyItem.GetFileList();
            FileListViewModels.Clear();
            foreach(var path in CopyItemFileDropList) {
                FileListViewModels.Add(new MpFileListItemViewModel(this,path));
            }
            OnPropertyChanged(nameof(FileListViewModels));
        }

        public void InitSwirl(BitmapSource sharedSwirl = null) {
            if (sharedSwirl == null) {
                SolidColorBrush lighterColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness((SolidColorBrush)TitleColor, -0.5f), 100);
                SolidColorBrush darkerColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness((SolidColorBrush)TitleColor, -0.4f), 50);
                SolidColorBrush accentColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness((SolidColorBrush)TitleColor, -0.0f), 100);
                var path = @"pack://application:,,,/Resources/Images/";
                var swirl1 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0001.png"));
                swirl1 = MpHelpers.TintBitmapSource(swirl1, ((SolidColorBrush)TitleColor).Color);

                var swirl2 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0002.png"));
                swirl2 = MpHelpers.TintBitmapSource(swirl2, lighterColor.Color);

                var swirl3 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0003.png"));
                swirl3 = MpHelpers.TintBitmapSource(swirl3, darkerColor.Color);

                var swirl4 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0004.png"));
                swirl4 = MpHelpers.TintBitmapSource(swirl4, accentColor.Color);

                TitleSwirl = MpHelpers.MergeImages(new List<BitmapSource>() { swirl1, swirl2, swirl3, swirl4 });
            } else {
                TitleSwirl = sharedSwirl;
            }
        }

        public void DeleteTempFiles() {
            foreach (var f in _tempFileList) {
                if (File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }

        public void Convert(MpCopyItemType newType) {
            CopyItem.ConvertType(newType);
            OnPropertyChanged(nameof(CopyItem));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            System.Diagnostics.Process.Start(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            System.Diagnostics.Process.Start(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            System.Diagnostics.Process.Start(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(CopyItemPlainText));
        }

        #endregion

        #region Private Methods 
        private void SetButtonGroupSelection(ToggleButton clickedButton, ToggleButton currentSelectedButton, IEnumerable<ToggleButton> buttonGroup, bool ignoreClickWhenSelected) {
            /* In some cases, if the user clicks the currently-selected button, we want to ignore
             * the click; for example, when a text alignment button is clicked. In other cases, we
             * want to deselect the button, but do nothing else; for example, when a list butteting
             * or numbering button is clicked. The ignoreClickWhenSelected variable controls that
             * behavior. */

            // Exit if currently-selected button is clicked
            if (clickedButton == currentSelectedButton) {
                if (ignoreClickWhenSelected) {
                    clickedButton.IsChecked = true;
                }
                return;
            }

            // Deselect all buttons
            foreach (var button in buttonGroup) {
                button.IsChecked = false;
            }

            // Select the clicked button
            clickedButton.IsChecked = true;
        }
        private string GetCurrentDetail(int detailId) {
            string info = "I dunno";// string.Empty;
            switch (detailId) {
                //created
                case 0:
                    // TODO convert to human readable time span like "Copied an hour ago...23 days ago etc
                    //TimeSpan dur = DateTime.Now - CopyItemCreatedDateTime;
                    info = "Copied " + CopyItemCreatedDateTime.ToString(); //dur.ToString();
                    break;
                //chars/lines
                case 1:
                    if (CopyItemType == MpCopyItemType.Image) {
                        info = "(" + CopyItemBmp.Width + ") x (" + CopyItemBmp.Height + ")";
                    } else if (CopyItemType == MpCopyItemType.RichText) {
                        info = CopyItem.ItemPlainText.Length + " chars | " + MpHelpers.GetRowCount(CopyItem.ItemPlainText) + " lines";
                    } else if (CopyItemType == MpCopyItemType.FileList) {
                        info = FileListViewModels.Count + " files | " + MpHelpers.FileListSize(CopyItem.GetFileList().ToArray()) + " bytes";
                    }
                    break;
                //# copies/# pastes
                case 2:
                    info = CopyItem.CopyCount + " copies | " + CopyItem.PasteCount + " pastes";
                    break;
                default:
                    info = "Unknown detailId: " + detailId;
                    break;
            }

            return info;
        }
        #endregion

        #region Commands
        private RelayCommand _addTemplateCommand;
        public ICommand AddTemplateCommand {
            get {
                if (_addTemplateCommand == null) {
                    _addTemplateCommand = new RelayCommand(AddTemplate);
                }
                return _addTemplateCommand;
            }
        }
        private void AddTemplate() {
            
        }

        private RelayCommand _editClipTextCommand;
        public ICommand EditClipTextCommand {
            get {
                if (_editClipTextCommand == null) {
                    _editClipTextCommand = new RelayCommand(EditClipText, CanEditClipText);
                }
                return _editClipTextCommand;
            }
        }
        private bool CanEditClipText() {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1 && 
                  CopyItemType == MpCopyItemType.RichText && 
                  !IsEditingTile;
        }
        private void EditClipText() {
            IsEditingTile = true;
            IsSelected = true;
            //all other action is handled in the ertb visibility changed handler in ertb_loaded
        }

        private RelayCommand _commitEditClipTextCommand;
        public ICommand CommitEditClipTextCommand {
            get {
                if (_commitEditClipTextCommand == null) {
                    _commitEditClipTextCommand = new RelayCommand(CommitEditClipText, CanCommitEditClipText);
                }
                return _commitEditClipTextCommand;
            }
        }
        private bool CanCommitEditClipText() {
            return IsEditingTile;// !IsEditingTitle;
        }
        private void CommitEditClipText() {
            //RtbVisibility = Visibility.Visible;
            //ErtbVisibility = Visibility.Collapsed;
            IsEditingTile = false;
            //all other action is handled in the ertb visibility changed handler in ertb_loaded
        }

        private RelayCommand _shareClipCommand;
        public ICommand ShareClipCommand {
            get {
                if (_shareClipCommand == null) {
                    _shareClipCommand = new RelayCommand(ShareClip, CanShareClip);
                }
                return _shareClipCommand;
            }
        }
        private bool CanShareClip() {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ShareClip() {
            MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = true;
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            var dtmHelper = new DataTransferManagerHelper(windowHandle);
            dtmHelper.DataTransferManager.DataRequested += (s, e) => {
                DataPackage dp = e.Request.Data;
                dp.Properties.Title = CopyItemTitle;
                switch(CopyItemType) {
                    case MpCopyItemType.RichText:
                        dp.SetText(MpHelpers.ConvertRichTextToPlainText(this.CopyItemRichText));
                        break;
                    case MpCopyItemType.FileList:
                    case MpCopyItemType.Image:
                        var filesToShare = new List<IStorageItem>();
                        foreach(string path in CopyItem.GetFileList()) {
                            //StorageFile sf = StorageFile.
                            //filesToShare.Add(sf);
                        }

                        dp.SetStorageItems(filesToShare);
                        break;
                }
            };

            dtmHelper.ShowShareUI();
            //MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.IsShowingDialog = false;
        }

        private RelayCommand _excludeApplicationCommand;
        public ICommand ExcludeApplicationCommand {
            get {
                if(_excludeApplicationCommand == null) {
                    _excludeApplicationCommand = new RelayCommand(ExcludeApplication,CanExcludeApplication);
                }
                return _excludeApplicationCommand;
            }
        }
        private bool CanExcludeApplication() {
            return MainWindowViewModel.ClipTrayViewModel.SelectedClipTiles.Count == 1;
        }
        private void ExcludeApplication() {
            MpAppCollectionViewModel.Instance.UpdateRejection(MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(CopyItemAppId), true);
        }

        private RelayCommand _assignHotkeyCommand;
        public ICommand AssignHotkeyCommand {
            get {
                if(_assignHotkeyCommand == null) {
                    _assignHotkeyCommand = new RelayCommand(AssignHotkey);
                }
                return _assignHotkeyCommand;
            }
        }
        private void AssignHotkey() {
            ShortcutKeyList = MpShortcutCollectionViewModel.Instance.RegisterViewModelShortcut(this, "Paste " + CopyItemTitle, ShortcutKeyList, PasteClipCommand);
        }

        private RelayCommand _pasteClipCommand;
        public ICommand PasteClipCommand {
            get {
                if (_pasteClipCommand == null) {
                    _pasteClipCommand = new RelayCommand(PasteClip);
                }
                return _pasteClipCommand;
            }
        }
        private void PasteClip() {
            MainWindowViewModel.ClipTrayViewModel.ClearClipSelection();
            IsSelected = true;
            MainWindowViewModel.ClipTrayViewModel.PasteSelectedClipsCommand.Execute(null);
        }

        private RelayCommand _runClipInShellCommand;
        public ICommand RunClipInShellCommand {
            get {
                if (_runClipInShellCommand == null) {
                    _runClipInShellCommand = new RelayCommand(RunClipInShell, CanRunClipInShell);
                }
                return _runClipInShellCommand;
            }
        }
        private bool CanRunClipInShell() {
            return CopyItemType == MpCopyItemType.RichText;
        }
        private void RunClipInShell() {
            MpHelpers.RunInShell(CopyItemPlainText, false, false, IntPtr.Zero);
        }
        #endregion

        #region Overrides

        public override string ToString() {
            return CopyItem.ItemPlainText;
        }

        #endregion
    }
}
