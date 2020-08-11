using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace MpWpfApp {
    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables
        private static MpClipTileViewModel _sourceSelectedClipTile = null;
        private int _detailIdx = 0;
        private List<string> _tempFileList = new List<string>();
        #endregion

        #region Public Variables
        public Point StartDragPoint { get; set; }
        #endregion

        #region Collections
        public ObservableCollection<MpClipTileTagMenuItemViewModel> TagMenuItems {
            get {
                ObservableCollection<MpClipTileTagMenuItemViewModel> tagMenuItems = new ObservableCollection<MpClipTileTagMenuItemViewModel>();
                var tagTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles;
                foreach (var tagTile in tagTiles) {
                    if (tagTile.TagName == Properties.Settings.Default.HistoryTagTitle) {
                        continue;
                    }
                    tagMenuItems.Add(new MpClipTileTagMenuItemViewModel(tagTile, MainWindowViewModel.LinkTagToCopyItemCommand, tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
                }
                return tagMenuItems;
            }
        }

        private ObservableCollection<MpFileListItemViewModel> _fileListItems = null;
        public ObservableCollection<MpFileListItemViewModel> FileListItems {
            get {
                if (_fileListItems == null && CopyItem.CopyItemType == MpCopyItemType.FileList) {
                    _fileListItems = new ObservableCollection<MpFileListItemViewModel>();
                    foreach (string fileItem in (string[])CopyItem.DataObject) {
                        _fileListItems.Add(new MpFileListItemViewModel(fileItem));
                    }
                }
                return _fileListItems;
            }
            set {
                if (_fileListItems != value) {
                    _fileListItems = value;
                    OnPropertyChanged(nameof(FileListItems));
                }
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
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpClipTileViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }

        }
        #endregion

        #region View Properties
        private bool _isDragging = false;
        public bool IsDragging {
            get {
                return _isDragging;
            }
            set {
                if (_isDragging != value) {
                    _isDragging = value;
                    OnPropertyChanged(nameof(IsDragging));
                }
            }
        }

        private bool _isMouseDown = false;
        public bool IsMouseDown {
            get {
                return _isMouseDown;
            }
            set {
                if(_isMouseDown != value) {
                    _isMouseDown = value;
                    OnPropertyChanged(nameof(IsMouseDown));
                }
            }
        }

        private double _contentWidth = 0;
        public double ContentWidth {
            get {
                return _contentWidth;
            }
            set {
                if(_contentWidth != value) {
                    _contentWidth = value;
                    OnPropertyChanged(nameof(ContentWidth));
                }
            }
        }

        private double _contentHeight = 0;
        public double ContentHeight {
            get {
                return _contentHeight;
            }
            set {
                if (_contentHeight != value) {
                    _contentHeight = value;
                    OnPropertyChanged(nameof(ContentHeight));
                }
            }
        }

        private BitmapSource _titleSwirl = null;
        public BitmapSource TitleSwirl {
            get {
                return _titleSwirl;
            }
            set {
                if(_titleSwirl != value) {
                    _titleSwirl = value;
                    OnPropertyChanged(nameof(TitleSwirl));
                }
            }
        }

        private bool _isTitleTextBoxFocused = false;
        public bool IsTitleTextBoxFocused {
            get {
                return _isTitleTextBoxFocused;
            }
            set {
                if (_isTitleTextBoxFocused != value) {
                    _isTitleTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTitleTextBoxFocused));
                }
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
                }
            }
        }

        private bool _isEditingTitle = false;
        public bool IsEditingTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if (_isEditingTitle != value) {
                    _isEditingTitle = value;
                    OnPropertyChanged(nameof(IsEditingTitle));
                }
            }
        }

        private Brush _tileBorderBrush = Brushes.Transparent;
        public Brush TileBorderBrush {
            get {
                return _tileBorderBrush;
            }
            set {
                if (_tileBorderBrush != value) {
                    _tileBorderBrush = value;
                    OnPropertyChanged(nameof(TileBorderBrush));
                }
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

        private Visibility _tileTitleTextBlockVisibility = Visibility.Visible;
        public Visibility TileTitleTextBlockVisibility {
            get {
                return _tileTitleTextBlockVisibility;
            }
            set {
                if (_tileTitleTextBlockVisibility != value) {
                    _tileTitleTextBlockVisibility = value;
                    OnPropertyChanged(nameof(TileTitleTextBlockVisibility));
                }
            }
        }

        private Visibility _tileTitleTextBoxVisibility = Visibility.Collapsed;
        public Visibility TileTitleTextBoxVisibility {
            get {
                return _tileTitleTextBoxVisibility;
            }
            set {
                if (_tileTitleTextBoxVisibility != value) {
                    _tileTitleTextBoxVisibility = value;
                    OnPropertyChanged(nameof(TileTitleTextBoxVisibility));
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

        private double _tileBorderSize = MpMeasurements.Instance.ClipTileBorderSize;
        public double TileBorderSize {
            get {
                return _tileBorderSize;
            }
            set {
                if (_tileBorderSize != value) {
                    _tileBorderSize = value;
                    OnPropertyChanged(nameof(TileBorderSize));
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
                if (_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged(nameof(TileTitleHeight));
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

        private MpMainWindowViewModel _mainWindowViewModel;
        public MpMainWindowViewModel MainWindowViewModel {
            get {
                return _mainWindowViewModel;
            }
            set {
                if (_mainWindowViewModel != value) {
                    _mainWindowViewModel = value;
                    OnPropertyChanged(nameof(MainWindowViewModel));
                }
            }
        }
        #endregion

        #region View AND Model Properties
        public Brush TitleColor {
            get {
                return (Brush)new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                CopyItem.ItemColor.WriteToDatabase();
                CopyItem.ColorId = CopyItem.ItemColor.ColorId;
                OnPropertyChanged(nameof(TitleColor));
            }
        }

        public Brush TitleColorLighter {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        new SolidColorBrush(CopyItem.ItemColor.Color),
                        -0.5f),
                    100);
            }
        }

        public Brush TitleColorDarker {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        new SolidColorBrush(CopyItem.ItemColor.Color),
                        -0.4f),
                    50);
            }
        }

        public Brush TitleColorAccent {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        new SolidColorBrush(CopyItem.ItemColor.Color),
                        -0.0f),
                    100);
            }
        }

        #endregion

        #region Model Properties

        private object _clipContentData = null;
        public object ClipContentData {
            get {
                return _clipContentData;
            }
            set {
                if(_clipContentData != value) {
                    _clipContentData = value;
                    OnPropertyChanged(nameof(ClipContentData));
                }
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

        private string _detailText = "This is empty detail text";
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

        private Brush _detailTextColor = Brushes.Black;
        public Brush DetailTextColor {
            get {
                return _detailTextColor;
            }
            set {
                if (_detailTextColor != value) {
                    _detailTextColor = value;
                    OnPropertyChanged(nameof(DetailTextColor));
                }
            }
        }

        public string Title {
            get {
                return CopyItem.Title;
            }
            set {
                if (CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private string _richText = string.Empty;
        public string RichText {
            get {
                return _richText;
            }
            set {
                if(_richText != value) {
                    _richText = value;
                    OnPropertyChanged(nameof(RichText));
                }
            }
        }

        public ImageSource Icon {
            get {
                return CopyItem.App.Icon.IconImage;
            }
        }

        private string _text = string.Empty;
        public string RawText {
            get {
                return _text;
            }
            set {
                if(_text != value) {
                    _text = value;
                    OnPropertyChanged(nameof(RawText));
                }
            }
        }

        private MpCopyItem _copyItem;
        public MpCopyItem CopyItem {
            get {
                return _copyItem;
            }
            set {
                if(_copyItem != value) {
                    _copyItem = value;
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }
        #endregion        

        #region Constructor
        public MpClipTileViewModel(MpCopyItem ci,MpMainWindowViewModel mwvm) {            
            CopyItem = ci;
            MainWindowViewModel = mwvm;
            RawText = CopyItem.GetPlainText();
            RichText = MpHelpers.PlainTextToRtf(RawText);
            ClipContentData = CopyItem.DataObject;
            if (TitleSwirl == null) {
                var swirl1 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0001.png"));
                swirl1 = MpHelpers.TintBitmapSource(swirl1, ((SolidColorBrush)TitleColor).Color);
                var swirl2 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0002.png"));
                swirl2 = MpHelpers.TintBitmapSource(swirl2, ((SolidColorBrush)TitleColorLighter).Color);
                var swirl3 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0003.png"));
                swirl3 = MpHelpers.TintBitmapSource(swirl3, ((SolidColorBrush)TitleColorDarker).Color);
                var swirl4 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0004.png"));
                swirl4 = MpHelpers.TintBitmapSource(swirl4, ((SolidColorBrush)TitleColorAccent).Color);
                
                TitleSwirl = MpHelpers.MergeImages(new List<BitmapSource>() { swirl1,swirl2,swirl3,swirl4});
            }
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            TileBorderBrush = Brushes.Red;
                            DetailTextColor = Brushes.Red;
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).IsSearchTextBoxFocused) {
                                IsFocused = true;
                            }
                        } else {
                            TileBorderBrush = Brushes.Transparent;
                            DetailTextColor = Brushes.Transparent;
                            //below must be called to clear focus when deselected (it may not have focus)
                            IsFocused = false;
                        }
                        break;
                    case nameof(IsHovering):
                        if (!IsSelected) {
                            if (IsHovering) {
                                TileBorderBrush = Brushes.Yellow;
                                DetailTextColor = Brushes.DarkKhaki;
                                //this is necessary for dragdrop re-sorting
                                MainWindowViewModel.LastHoveringClipTileViewModel = this;
                            } else {
                                TileBorderBrush = Brushes.Transparent;
                                DetailTextColor = Brushes.Transparent;
                            }
                        }
                        break;
                    case nameof(IsEditingTitle):
                        if (IsEditingTitle) {
                            //show textbox and select all text
                            TileTitleTextBoxVisibility = Visibility.Visible;
                            TileTitleTextBlockVisibility = Visibility.Collapsed;
                            IsTitleTextBoxFocused = false;
                            IsTitleTextBoxFocused = true;
                        } else {
                            TileTitleTextBoxVisibility = Visibility.Collapsed;
                            TileTitleTextBlockVisibility = Visibility.Visible;
                            IsTitleTextBoxFocused = false;
                            CopyItem.WriteToDatabase();
                        }
                        break;
                    case nameof(RichText):
                        if(CopyItemType == MpCopyItemType.RichText && !MainWindowViewModel.IsLoading) {
                            CopyItem.SetData(RichText);
                            CopyItem.WriteToDatabase();
                        }
                        break;
                    case nameof(Title):
                        CopyItem.WriteToDatabase();
                        break;
                }
            };
        }
        #endregion

        #region Public Methods
        public void Highlight(string searchText) {
            _myRtb?.HighlightSearchText(searchText,Brushes.Yellow);
        }

        
        #endregion
        #region View Events Handlers        
        private MpClipTileRichTextBox _myRtb = null;

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (MpClipBorder)sender;

            clipTileBorder.PreviewMouseLeftButtonDown += (s, e6) => {
                if (e6.ClickCount == 2) {
                    ((MpClipTileViewModel)((Border)s).DataContext).MainWindowViewModel.PasteSelectedClipsCommand.Execute(null);
                }
                IsMouseDown = true;
                StartDragPoint = e6.GetPosition((ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray"));
            };
            clipTileBorder.PreviewMouseMove += (s, e7) => {
                var clipTray = (ListBox)((MpMainWindow)Application.Current.MainWindow).FindName("ClipTray");
                var curDragPoint = e7.GetPosition(clipTray);
                
                if (IsMouseDown && e7.MouseDevice.LeftButton == MouseButtonState.Pressed 
                /*&& Math.Abs(curDragPoint.X-StartDragPoint.X) > 50*/) {
                    string text = string.Empty;
                    string rtf = string.Empty;
                    var bmp = MpHelpers.ConvertRichTextToImage(RichText, (int)ContentWidth, (int)ContentHeight);
                    List<string> fileDrop = new List<string>();
                    MpCopyItemType lastType = CopyItemType;
                    foreach (MpClipTileViewModel ctvm in MainWindowViewModel.SelectedClipTiles) {
                        if(ctvm.CopyItemType != lastType) {
                            continue;
                        } else {
                            lastType = ctvm.CopyItemType;
                        }
                        text += ctvm.RawText + Environment.NewLine;
                        rtf += ctvm.RichText + Environment.NewLine;
                        bmp = MpHelpers.MergeImages(new List<BitmapSource>() { bmp, MpHelpers.ConvertRichTextToImage(ctvm.RichText, (int)ContentWidth, (int)ContentHeight) });
                        if(ctvm.CopyItemType == MpCopyItemType.FileList) {
                            foreach(string f in (string[])ctvm.ClipContentData) {
                                fileDrop.Add(f);
                            }
                        } else {
                            fileDrop.Add(ctvm.WriteCopyItemToFile(Path.GetTempPath(), true));
                        }    
                    }
                    //this case is when non file drop item's being dragged
                    if(fileDrop.Count == 0) {
                        if(CopyItemType == MpCopyItemType.RichText) {
                            fileDrop.Add(WriteTextToFile(Path.GetTempFileName(), text, true));
                        } else {
                            fileDrop.Add(WriteBitmapSourceToFile(Path.GetTempFileName(), bmp, true));
                        }
                    }
                    IDataObject d = new DataObject();
                    d.SetData(DataFormats.Text, text);
                    d.SetData(DataFormats.Rtf, rtf);
                    d.SetData(DataFormats.Bitmap, bmp);
                    d.SetData(DataFormats.FileDrop, fileDrop.ToArray());
                    d.SetData(Properties.Settings.Default.ClipTileDragDropFormatName, MainWindowViewModel.SelectedClipTiles.ToList());
                    DragDrop.DoDragDrop(clipTray, d, DragDropEffects.Copy | DragDropEffects.Move);
                } else {
                    //this occurs when mouse up is outside the application (like during a dragdrop)
                    IsMouseDown = false;
                    StartDragPoint = new Point();
                }
            };
            clipTileBorder.PreviewMouseUp += (s, e8) => {
                IsMouseDown = false;
                StartDragPoint = new Point();
            };
            clipTileBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            clipTileBorder.MouseLeave += (s,e2) => {
                IsHovering = false;
            };
            clipTileBorder.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var clipTileTitleTextBox = (TextBox)clipTileBorder.FindName("ClipTileTitleTextBox");
            clipTileTitleTextBox.KeyUp += MainWindowViewModel.MainWindow_KeyDown;
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var titleIconImage = (Image)clipTileBorder.FindName("ClipTileAppIconImage");
            Canvas.SetLeft(titleIconImage, TileBorderSize - TileTitleHeight - 10);
            Canvas.SetTop(titleIconImage, 2);
            
            var titleDetailTextBlock = (TextBlock)clipTileBorder.FindName("ClipTileTitleDetailTextBlock");
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, TileTitleHeight - 14);

            var flb = (ListBox)((Border)sender)?.FindName("ClipTileFileListBox"); 
            var img = (Image)((Border)sender)?.FindName("ClipTileImage");
            var rtb = (MpClipTileRichTextBox)((Border)sender)?.FindName("ClipTileRichTextBox");
            
            if (CopyItem.CopyItemType == MpCopyItemType.FileList) {
                //assume dimensions are plaintext path list length
                rtb.SetRtf(RichText);
                ContentWidth = rtb.RenderSize.Width;
                ContentHeight = rtb.RenderSize.Height;
                
                rtb.Visibility = Visibility.Collapsed;
                img.Visibility = Visibility.Collapsed;                
            }
            if (CopyItem.CopyItemType == MpCopyItemType.Image) {
                img.Source = (BitmapSource)CopyItem.DataObject;
                ContentWidth = img.Width;
                ContentHeight = img.Height;

                rtb.Visibility = Visibility.Collapsed;
                flb.Visibility = Visibility.Collapsed;
                
            } else if(CopyItem.CopyItemType == MpCopyItemType.RichText) {
                _myRtb = rtb;
                //overwriting richtext in constructor to rawtext
                RichText = (string)ClipContentData;
                if(!MpHelpers.IsStringRichText(RichText)) {
                    RichText = MpHelpers.PlainTextToRtf(RichText);
                }
                img.Visibility = Visibility.Collapsed;
                flb.Visibility = Visibility.Collapsed;

                // since document is enabled this overrides the rtb's context menu with the tile's defined in the xaml
                rtb.ContextMenu = (ContextMenu)clipTileBorder.FindName("ClipTile_ContextMenu");

                rtb.SetRtf(RichText);
                ContentWidth = rtb.RenderSize.Width;
                ContentHeight = rtb.RenderSize.Height;
                rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
                rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;

                //sort item's tokens by block and then by start idx
                var sortedTokenList = CopyItem.SubTextTokenList.OrderBy(stt => stt.BlockIdx).ThenBy(stt => stt.StartIdx).ToList();
                foreach(var sortedToken in sortedTokenList) {
                    rtb.AddSubTextToken(sortedToken);
                }
                Highlight(string.Empty);
            }
        }
        public string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            StreamWriter of = new StreamWriter(filePath);
            of.Write(text);
            of.Close();
            return filePath;
        }
        public string WriteBitmapSourceToFile(string filePath,BitmapSource bmpSrc,bool isTemporary = false) {
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(MpHelpers.ConvertBitmapSourceToBitmap(bmpSrc));
            bmp.Save(filePath, ImageFormat.Png);
            return filePath;
        }
        //writes <Title>.txt | .png | *.* to rootPath, if <Title> exists Title is postfiex incrementally
        //isTemporary infers this is for a drag drop and should be deleted when application closes
        // TODO Work out best time to delete drag drop files so the target doesn't loose the source file
        public string WriteCopyItemToFile(string rootPath,bool isTemporary = false) {
            //file path
            // TODO Title needs to be cleaned of anyspecial characters that invalidate file name
            string tempTitle = string.IsNullOrEmpty(Title.Trim()) ? "temp" : Title.Trim();
            string fp = rootPath + tempTitle;
            //file extension
            string fe = string.Empty;
            switch (CopyItemType) {
                case MpCopyItemType.RichText:
                    fe = ".txt";
                    break;
                case MpCopyItemType.Image:
                    fe = ".png";
                    break;
                    //ignore file list since file type is part of item
            }
            if (CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Image) {
                //file name count
                int fnc = 0;
                string fp2 = fp;
                while (File.Exists(fp2 + fe)) {
                    fp2 = fp + fnc;
                    int result = fnc;
                    try {
                        var match = Regex.Match(fp2, @"\d+$");
                        result = match == null ? 0: Convert.ToInt32(match.Value);
                    }
                    catch (Exception e) {
                        result = fnc;
                    }

                    fnc = ++result;
                    fp2 = fp + fnc;
                }
                fp = fp2;
            } else {
                fp = fe = string.Empty;
            }
            //output file
            switch (CopyItemType) {
                case MpCopyItemType.RichText:
                    WriteTextToFile(fp + fe, CopyItem.GetPlainText(), isTemporary);
                    break;
                case MpCopyItemType.Image:
                    WriteBitmapSourceToFile(fp + fe, (BitmapSource)CopyItem.DataObject);
                    break;
                case MpCopyItemType.FileList:
                    foreach (string f in (string[])CopyItem.DataObject) {
                        try {
                            string fn = Path.GetFileName(f);
                            //file name count
                            int fnc = 1;
                            if (MpHelpers.IsPathDirectory(f)) {
                                while (Directory.Exists(rootPath + @"\" + fn)) {
                                    fn = fn + (fnc++);
                                }
                                MpHelpers.DirectoryCopy(f, rootPath + @"\" + fn, true);
                            } else {
                                while (File.Exists(rootPath + @"\" + fn)) {
                                    fn = fn + (fnc++);
                                }
                                File.Copy(f, rootPath + @"\" + fn);
                            }
                        }
                        catch (Exception e) {
                            MessageBox.Show("Source file '" + f + "' no longer exists!");
                        }
                    }
                    break;
            }
            if(!string.IsNullOrEmpty(fp+fe) && isTemporary) {
                _tempFileList.Add(fp + fe);
            }
            return fp + fe;
        }

        public void DeleteTempFiles() {
            foreach(var f in _tempFileList) {
                if(File.Exists(f)) {
                    File.Delete(f);
                }
            }
        }
        
        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(RawText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            System.Diagnostics.Process.Start(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(RawText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            System.Diagnostics.Process.Start(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(RawText));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            System.Diagnostics.Process.Start(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(RawText));
        }

        #endregion

        #region Private Methods
        private TextRange FindStringRangeFromPosition(TextPointer position, string str) {
            while (position != null) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text) {
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "word".
                    int indexInRun = textRun.IndexOf(str);
                    if (indexInRun >= 0) {
                        return new TextRange(position.GetPositionAtOffset(indexInRun), position.GetPositionAtOffset(indexInRun + str.Length));
                    }
                }
                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // position will be null if "word" is not found.
            return null;
        }

        
        #endregion

        #region Commands

        private RelayCommand _speakClipCommand;
        public ICommand SpeakClipCommand {
            get {
                if(_speakClipCommand == null) {
                    _speakClipCommand = new RelayCommand(SpeakClip, CanSpeakClip);
                }
                return _speakClipCommand;
            }
        }
        private bool CanSpeakClip() {
            return CopyItem.CopyItemType != MpCopyItemType.Image && CopyItem.CopyItemType != MpCopyItemType.FileList;
        }
        private void SpeakClip() {
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer()) {
                speechSynthesizer.Speak(RawText);
            }                
        }

        private RelayCommand _convertTokenToQrCodeCommand;
        public ICommand ConvertTokenToQrCodeCommand {
            get {
                if (_convertTokenToQrCodeCommand == null) {
                    _convertTokenToQrCodeCommand = new RelayCommand(ConvertTokenToQrCode);
                }
                return _convertTokenToQrCodeCommand;
            }
        }
        private bool CanConvertTokenToQrCode() {
            return CopyItem.CopyItemType != MpCopyItemType.Image && CopyItem.CopyItemType != MpCopyItemType.FileList;
        }
        private void ConvertTokenToQrCode() {
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer()) {
                speechSynthesizer.Speak(RawText);
            }
        }
        #endregion

        #region Overrides
        public override string ToString() {
            return CopyItem.GetPlainText();
        }
        #endregion

    }
}
