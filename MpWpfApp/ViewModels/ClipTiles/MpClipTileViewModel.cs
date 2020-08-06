
using Newtonsoft.Json;
using GalaSoft.MvvmLight.CommandWpf;
using QRCoder;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Management;
using GongSolutions.Wpf.DragDrop.Utilities;
using System.Reflection;

namespace MpWpfApp {
    public class MpClipTileViewModel : MpViewModelBase {
        #region Private Variables
        private static MpClipTileViewModel _sourceSelectedClipTile = null;
        private bool _isMouseDown = false;
        private Point _startDragPoint;

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
        #endregion

        #region Layout 
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

        public string RichText {
            get {
                if (CopyItem.CopyItemType == MpCopyItemType.RichText) {
                    return (string)CopyItem.DataObject;
                } else if (CopyItem.CopyItemType == MpCopyItemType.FileList) {
                    return MpCopyItem.PlainTextToRtf(CopyItem.GetPlainText());
                } else if (CopyItem.CopyItemType == MpCopyItemType.Image) {
                    // TODO add image to text conversion here
                    return MpCopyItem.PlainTextToRtf(Enum.GetName(typeof(MpCopyItemType), CopyItem.CopyItemType));
                }
                return string.Empty;
            }
            set {
                CopyItem.SetData(value);
                OnPropertyChanged(nameof(RichText));
            }
        }

        public ImageSource Icon {
            get {
                return CopyItem.App.Icon.IconImage;
            }
        }
        public string Text {
            get {
                return CopyItem.GetPlainText();
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
            PropertyChanged += (s, e1) => {
                switch (e1.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            TileBorderBrush = Brushes.Red;
                            //this check ensures that as user types in search that 
                            //resetselection doesn't take the focus from the search box
                            if (!((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).IsSearchTextBoxFocused) {
                                IsFocused = true;
                            }
                        } else {
                            TileBorderBrush = Brushes.Transparent;
                            //below must be called to clear focus when deselected (it may not have focus)
                            IsFocused = false;
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
                        CopyItem.WriteToDatabase();
                        break;
                    case nameof(Title):
                        CopyItem.WriteToDatabase();
                        break;
                    case nameof(IsHovering):
                        if (!IsSelected) {
                            if (IsHovering) {
                                TileBorderBrush = Brushes.Yellow;
                            } else {
                                TileBorderBrush = Brushes.Transparent;
                            }
                        }
                        break;
                }
            };
        }
        #endregion

        #region View Events Handlers        

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTileBorder = (Border)sender;
            
            clipTileBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            clipTileBorder.MouseLeave += (s,e2) => {
                IsHovering = false;
            };
            clipTileBorder.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };
            clipTileBorder.MouseLeftButtonDown += (s, e5) => {
                if(e5.ClickCount == 2) {
                    ((MpClipTileViewModel)((Border)s).DataContext).MainWindowViewModel.PasteSelectedClipsCommand.Execute(null);
                }
            };

            var clipTileTitleTextBox = (TextBox)clipTileBorder.FindName("ClipTileTitleTextBox");
            clipTileTitleTextBox.KeyUp += MainWindowViewModel.MainWindow_KeyDown;
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var titleIconImage = (Image)clipTileBorder.FindName("ClipTileAppIconImage");
            Canvas.SetLeft(titleIconImage, TileBorderSize - TileTitleHeight - 10);
            Canvas.SetTop(titleIconImage, 2);// TileBorderSize * 0.5);

            var flb = (ListBox)((Border)sender)?.FindName("ClipTileFileListBox"); 
            var img = (Image)((Border)sender)?.FindName("ClipTileImage");
            var rtb = (MpClipTileRichTextBox)((Border)sender)?.FindName("ClipTileRichTextBox");

            if (CopyItem.CopyItemType == MpCopyItemType.FileList) {
                rtb.Visibility = Visibility.Collapsed;
                img.Visibility = Visibility.Collapsed;

                //flb.PreviewKeyUp += MainWindowViewModel.ClipTile_KeyUp;
            }
            if (CopyItem.CopyItemType == MpCopyItemType.Image) {
                img.Source = (BitmapSource)CopyItem.DataObject;
                rtb.Visibility = Visibility.Collapsed;
                flb.Visibility = Visibility.Collapsed;

                //img.PreviewKeyUp += MainWindowViewModel.ClipTile_KeyUp;
            } else if(CopyItem.CopyItemType == MpCopyItemType.RichText) {
                img.Visibility = Visibility.Collapsed;
                flb.Visibility = Visibility.Collapsed;


                // since document is enabled this overrides the rtb's context menu with the tile's defined in the xaml
                rtb.ContextMenu = (ContextMenu)clipTileBorder.FindName("ClipTile_ContextMenu");

                //First load the richtextbox with copytext
                rtb.SetRtf(RichText);
                //rtb.PreviewKeyDown += MainWindowViewModel.ClipTile_KeyUp;
                //rtb.PreviewMouseLeftButtonUp += Rtb_PreviewMouseLeftButtonUp;
                //rtb.PreviewMouseDoubleClick += Rtb_PreviewMouseLeftButtonUp;
                //rtb.PreviewMouseRightButtonUp += ClipTileContent_MouseRightButtonUp;
                //clipTileBorder.PreviewMouseRightButtonUp += ClipTileContent_MouseRightButtonUp;

                //Text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
                //var dpi4 = VisualTreeHelper.GetDpi(rtb);
                //FormattedText ft = new FormattedText(Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(rtb.Document.FontFamily.ToString()), rtb.Document.FontSize, rtb.Document.Foreground, dpi4.PixelsPerDip);//VisualTreeHelper.GetDpi(rtb).PixelsPerDip);
                //rtb.Width = ft.Width + rtb.Padding.Left + rtb.Padding.Right;
                //rtb.Height = ft.Height + rtb.Padding.Top + rtb.Padding.Bottom;
                rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
                rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;

                //sort item's tokens by block and then by start idx
                var sortedTokenList = CopyItem.SubTextTokenList.OrderBy(stt => stt.BlockIdx).ThenBy(stt => stt.StartIdx).ToList();
                foreach(var sortedToken in sortedTokenList) {
                    rtb.AddSubTextToken(sortedToken);
                }   
            }
        }
        
        public void ContextMenuMouseLeftButtonUpOnSearchGoogle() {
            System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + System.Uri.EscapeDataString(Text));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchBing() {
            System.Diagnostics.Process.Start(@"https://www.bing.com/search?q=" + System.Uri.EscapeDataString(Text));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchDuckDuckGo() {
            System.Diagnostics.Process.Start(@"https://duckduckgo.com/?q=" + System.Uri.EscapeDataString(Text));
        }
        public void ContextMenuMouseLeftButtonUpOnSearchYandex() {
            System.Diagnostics.Process.Start(@"https://yandex.com/search/?text=" + System.Uri.EscapeDataString(Text));
        }

        #endregion

        #region Drag Drop Event Handlers

        //public void ClipTile_DragEnter(object sender, DragEventArgs e) {
        //    if (!e.Data.GetDataPresent("MonkeyPasteFormat") || sender == e.Source) {
        //        e.Effects = DragDropEffects.None;
        //    }
        //}

        //public void ClipTile_LeftMouseButtonDown(object sender, MouseEventArgs e) {
        //    _startDragPoint = e.GetPosition(null);
        //    _isMouseDown = true;
        //}
        //public void ClipTile_MouseMove(object sender, MouseEventArgs e) {
        //    // Get the current mouse position
        //    Point mousePos = e.GetPosition(null);
        //    Vector diff = _startDragPoint - mousePos;

        //    if (_isMouseDown) {
        //        var clipBorder = (MpClipBorder)sender;

        //        var clipTileViewModel = (MpClipTileViewModel)clipBorder.DataContext;
        //        // Initialize the drag & drop operation
        //        DataObject dragData = new DataObject("MonkeyPasteFormat", clipTileViewModel);

        //        dragData.SetData(DataFormats.Rtf, RichText);
        //        dragData.SetData(DataFormats.Text, clipTileViewModel.CopyItem.GetPlainText());
        //        //dragData.SetData(dragData);
        //        DragDrop.DoDragDrop(clipBorder, dragData, DragDropEffects.None);
        //    }
        //}
        //// Helper to search up the VisualTree
        //private T FindAnchestor<T>(DependencyObject current)
        //    where T : DependencyObject {
        //    do {
        //        if (current is T) {
        //            return (T)current;
        //        }
        //        current = VisualTreeHelper.GetParent(current);
        //    }
        //    while (current != null);
        //    return null;
        //}
        //public void ClipTile_LeftMouseButtonUp(object sender, MouseEventArgs e) {
        //    _isMouseDown = false;
        //}

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
                speechSynthesizer.Speak(Text);
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
                speechSynthesizer.Speak(Text);
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
