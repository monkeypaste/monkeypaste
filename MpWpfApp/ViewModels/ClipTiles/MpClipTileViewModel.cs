
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

namespace MpWpfApp {
    public class  MpClipTileViewModel : MpViewModelBase {
        private static MpClipTileViewModel _sourceSelectedClipTile = null;

        public ObservableCollection<MpClipTileTagMenuItemViewModel> TagMenuItems {
            get {
                ObservableCollection<MpClipTileTagMenuItemViewModel> tagMenuItems = new ObservableCollection<MpClipTileTagMenuItemViewModel>();
                var tagTiles = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles;
                foreach(var tagTile in tagTiles) {
                    if(tagTile.TagName == "History") {
                        continue;
                    }
                    tagMenuItems.Add(new MpClipTileTagMenuItemViewModel(tagTile.TagName, tagTile.LinkTagToCopyItemCommand, tagTile.Tag.IsLinkedWithCopyItem(CopyItem)));
                }
                return tagMenuItems;
            }
        }

        private ObservableCollection<MpFileListItemViewModel> _fileListItems = null;
        public ObservableCollection<MpFileListItemViewModel> FileListItems {
            get {
                if(_fileListItems == null && CopyItem.CopyItemType == MpCopyItemType.FileList) {
                    _fileListItems = new ObservableCollection<MpFileListItemViewModel>();
                    foreach(string fileItem in (string[])CopyItem.GetData()) {
                        _fileListItems.Add(new MpFileListItemViewModel(fileItem));
                    }
                }
                return _fileListItems;
            }
            set {
                if(_fileListItems != value) {
                    _fileListItems = value;
                    OnPropertyChanged(nameof(FileListItems));
                }
            }
        }

        #region Appearance Properties
        private bool _hasScrollBars = false;
        public bool HasScrollBars {
            get {
                return _hasScrollBars;
            }
            set {
                if(_hasScrollBars != value) {
                    _hasScrollBars = value;
                    OnPropertyChanged(nameof(HasScrollBars));
                }
            }
        }

        private bool _isTitleTextBoxFocused = false;
        public bool IsTitleTextBoxFocused {
            get {
                return _isTitleTextBoxFocused;
            }
            set {
                if(_isTitleTextBoxFocused != value) {
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
                if(_isSelected != value) {
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
                if(_isHovering != value) {
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
                if(_isDragging != value) {
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
                if(_isEditingTitle != value) {
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
                if(_tileBorderBrush != value) {
                    _tileBorderBrush = value;
                    OnPropertyChanged(nameof(TileBorderBrush));
                }
            }
        }
        #endregion

        #region Layout 
        private Visibility _tileVisibility = Visibility.Visible;
        public Visibility TileVisibility {
            get {
                return _tileVisibility;
            }
            set {
                if(_tileVisibility != value) {
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
                if(_tileSize != value) {
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
                if(_tileTitleIconSize != value) {
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
                if(_tileBorderSize != value) {
                    _tileBorderSize = value;
                    OnPropertyChanged(nameof(TileBorderSize));
                }
            }
        }

        private double _tileBorderThickness = MpMeasurements.Instance.ClipTileBorderThickness;
        public double TileBorderThickness
        {
            get
            {
                return _tileBorderThickness;
            }
            set
            {
                if (_tileBorderThickness != value)
                {
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
                if(_tileTitleHeight != value) {
                    _tileTitleHeight = value;
                    OnPropertyChanged(nameof(TileTitleHeight));
                }
            }
        }

        private double _tileContentHeight = MpMeasurements.Instance.TileContentHeight;
        public double TileContentHeight {
            get {
                return _tileContentHeight;
            }
            set {
                if(_tileTitleHeight != value) {
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
                if(_tileMargin != value) {
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
                if(_tileDropShadowRadius != value) {
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
                if(_mainWindowViewModel != value) {
                    _mainWindowViewModel = value;
                    OnPropertyChanged(nameof(MainWindowViewModel));
                }
            }
        }
        #endregion

        #region Model Properties
        public Brush TitleColor {
            get {
                return new SolidColorBrush(CopyItem.ItemColor.Color);
            }
            set {
                CopyItem.ItemColor = new MpColor(((SolidColorBrush)value).Color);
                CopyItem.ItemColor.WriteToDatabase();
                CopyItem.ColorId = CopyItem.ItemColor.ColorId;
                OnPropertyChanged(nameof(TitleColor));
            }
        }

        public string Title {
            get {
                return CopyItem.Title;
            }
            set {
                if(CopyItem.Title != value) {
                    CopyItem.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private string _text = string.Empty;
        public string Text {
            get {
                return _text;
            }
            set {
                if(_text != value) {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public string RichText {
            get {
                return (string)CopyItem.GetData();
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
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsSelected):
                        if (IsSelected) {
                            TileBorderBrush = Brushes.Red;
                        } else {
                            TileBorderBrush = Brushes.Transparent;
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
        public void MouseEnter() {
            IsHovering = true;
        }

        private PolyBezierSegment CreateCurveRect(double x,double y,double w,double h, double m1,double m2,double m3,double m4) {
            PolyBezierSegment plineSeg = new PolyBezierSegment();
            plineSeg.Points.Add(new Point(x,y));
            plineSeg.Points.Add(new Point(x,y));
            plineSeg.Points.Add(new Point(x,y));

            plineSeg.Points.Add(new Point(w, y));
            plineSeg.Points.Add(new Point(w, y));
            plineSeg.Points.Add(new Point(w, y));

            plineSeg.Points.Add(new Point(w, h));
            plineSeg.Points.Add(new Point(w, h));
            plineSeg.Points.Add(new Point(w, h));

            //test m1=0.75 m2=2.0 m3=0.5 m4=0.5
            plineSeg.Points.Add(new Point(w * m1, h * m2));
            plineSeg.Points.Add(new Point(w * m3, h * m4));
            plineSeg.Points.Add(new Point(x, h));

            return plineSeg;
        }
        
        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var tileBorder = (Border)sender;
            PathFigure pthFigure = new PathFigure();
            pthFigure.IsClosed = true;
            pthFigure.StartPoint = new Point(0, 0);
            
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(CreateCurveRect(0,0,TileBorderSize,TileTitleHeight,0.375,1.0,0.25,0.25));
            pthFigure.Segments = myPathSegmentCollection;

            PathFigureCollection pthFigureCollection = new PathFigureCollection();
            pthFigureCollection.Add(pthFigure);
            PathGeometry pthGeometry = new PathGeometry();
            pthGeometry.Figures = pthFigureCollection;

            var titlePath = (System.Windows.Shapes.Path)((Border)sender)?.FindName("ClipTileTitlePath3");
            titlePath.Data = pthGeometry;
            TileTitleHeight = titlePath.RenderSize.Height;

            var titleIconImage = (Image)((Border)sender)?.FindName("ClipTileAppIconImage");
            Canvas.SetLeft(titleIconImage, TileBorderSize - TileTitleHeight);
            Canvas.SetTop(titleIconImage, 2);// TileBorderSize * 0.5);

            var flb = (ListBox)((Border)sender)?.FindName("ClipTileFileListBox"); 
            var image = (Image)((Border)sender)?.FindName("ClipTileImage");
            var rtb = (RichTextBox)((Border)sender)?.FindName("ClipTileRichTextBox");

            if (CopyItem.CopyItemType == MpCopyItemType.FileList) {
                rtb.Visibility = Visibility.Collapsed;
                image.Visibility = Visibility.Collapsed;
                return;
            }
            if (CopyItem.CopyItemType == MpCopyItemType.Image) {
                image.Source = (BitmapSource)CopyItem.GetData();
                rtb.Visibility = Visibility.Collapsed;
                flb.Visibility = Visibility.Collapsed;
                return;
            }
            image.Visibility = Visibility.Collapsed;
            flb.Visibility = Visibility.Collapsed;
            //First load the richtextbox with copytext
            rtb.SetRtf(RichText); 
            rtb.PreviewMouseLeftButtonDown += ClipTileRichTextBox_PreviewLeftMouseButtonDown;
            rtb.MouseRightButtonUp += ClipTileRichTextBox_MouseRightButtonUp;
            Text = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text;
            var dpi4 = VisualTreeHelper.GetDpi(rtb);
            FormattedText ft = new FormattedText(Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(rtb.Document.FontFamily.ToString()), rtb.Document.FontSize, rtb.Document.Foreground, dpi4.PixelsPerDip);//VisualTreeHelper.GetDpi(rtb).PixelsPerDip);
            rtb.Width = ft.Width + rtb.Padding.Left + rtb.Padding.Right;
            rtb.Height = ft.Height + rtb.Padding.Top + rtb.Padding.Bottom;
            rtb.Document.PageWidth = ft.Width;
            rtb.Document.PageHeight = ft.Height;

            var scrollViewer = (ScrollViewer)((Border)sender)?.FindName("ClipTileRichTextBoxScrollViewer"); 
            HasScrollBars = scrollViewer.Height < rtb.Height || scrollViewer.Width < rtb.Width;
            
            var sortedTokenList = CopyItem.SubTextTokenList.OrderBy(stt => stt.BlockIdx).ThenBy(stt=>stt.StartIdx).ToList();
            if (sortedTokenList.Count > 0) {
                var doc = rtb.Document;
                TextRange lastTokenRange = null;
                //iterate over each token
                for(int i = 0;i < sortedTokenList.Count;i++) {
                    MpSubTextToken token = sortedTokenList[i];
                    Paragraph para = (Paragraph)doc.Blocks.ToArray()[token.BlockIdx]; 
                    //find and remove the inline with the token
                    Span inline = (Span)para.Inlines.ToArray()[token.InlineIdx];
                    //para.Inlines.Remove(inline);
                    TextRange runRange = new TextRange(inline.ContentStart, inline.ContentEnd);            
                    string tokenText = runRange.Text.Substring(token.StartIdx,token.EndIdx-token.StartIdx);
                    TextPointer searchStartPointer = inline.ContentStart;
                    if(i > 0) {
                        var lastToken = sortedTokenList[i - 1];
                        if(token.BlockIdx == lastToken.BlockIdx && token.InlineIdx == lastToken.InlineIdx) {
                            searchStartPointer = lastTokenRange.End;
                        }
                    }
                    TextRange tokenRange = FindStringRangeFromPosition(searchStartPointer, tokenText);
                    lastTokenRange = tokenRange;
                    Hyperlink tokenLink = new Hyperlink(tokenRange.Start,tokenRange.End);
                    tokenLink.IsEnabled = true;
                    tokenLink.RequestNavigate += Hyperlink_RequestNavigate;
                    MenuItem convertToQrCodeMenuItem = new MenuItem();
                    convertToQrCodeMenuItem.Header = "Convert to QR Code";
                    convertToQrCodeMenuItem.Click += ConvertToQrCodeMenuItem_Click;
                    convertToQrCodeMenuItem.Tag = tokenLink;
                    tokenLink.ContextMenu = new ContextMenu();
                    tokenLink.ContextMenu.Items.Add(convertToQrCodeMenuItem);
                    switch (token.TokenType) {
                        case MpSubTextTokenType.Uri:
                            if (!tokenText.Contains("https://")) {
                                tokenLink.NavigateUri = new Uri("https://" + tokenText);
                            } else {
                                tokenLink.NavigateUri = new Uri(tokenText);
                            }
                            MenuItem minifyUrl = new MenuItem();
                            minifyUrl.Header = "Minify with bit.ly";
                            minifyUrl.Click += MinifyUrl_Click;
                            minifyUrl.Tag = tokenLink;
                            tokenLink.ContextMenu.Items.Add(minifyUrl);
                            break;

                        case MpSubTextTokenType.Email:
                            tokenLink.NavigateUri = new Uri("mailto:" + tokenText);
                            break;

                        case MpSubTextTokenType.PhoneNumber:
                            tokenLink.NavigateUri = new Uri("tel:" + tokenText);
                            break;
                        default:

                            break;
                    }
                }
            }
        }

        private void MinifyUrl_Click(object sender, RoutedEventArgs e) {
            Hyperlink link = ((Hyperlink)((MenuItem)sender).Tag);
            string minifiedLink = ShortenUrl(link.NavigateUri.ToString()).Result;
            MpCopyItem newCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.Text, minifiedLink, MainWindowViewModel.ClipboardMonitor.LastWindowWatcher.ThisAppPath, MpHelperSingleton.Instance.GetRandomColor());
            newCopyItem.WriteToDatabase();
            MpTag historyTag = new MpTag(1);
            historyTag.LinkWithCopyItem(newCopyItem);
            MainWindowViewModel.ClearSelection();
            MainWindowViewModel.AddClipTile(newCopyItem);
        }
        
        private void ClipTileRichTextBox_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            MenuItem searchMenuItem = new MenuItem();
            searchMenuItem.Click += (s, e1) => {
                RichTextBox rtb = (RichTextBox)sender;
                string searchStr = new TextRange(rtb.Selection.Start, rtb.Selection.End).Text;
                System.Diagnostics.Process.Start("http://www.google.com.au/search?q=" + Uri.EscapeDataString(searchStr));
            };
            searchMenuItem.Header = "Search Web";

            ContextMenu cmnu = new ContextMenu();
            cmnu.Items.Add(searchMenuItem);
        }

        private void ConvertToQrCodeMenuItem_Click(object sender, RoutedEventArgs e) {
            var hyperLink = (Hyperlink)(((MenuItem)sender).Tag);

            Url generator = new Url(hyperLink.NavigateUri.ToString());
            string payload = generator.ToString();

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator()) {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
                using(QRCode qrCode = new QRCode(qrCodeData)) {
                    var qrCodeAsBitmap = qrCode.GetGraphic(20);
                    MpCopyItem qrCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.Image, MpHelperSingleton.Instance.ConvertBitmapToBitmapSource(qrCodeAsBitmap), MainWindowViewModel.ClipboardMonitor.LastWindowWatcher.ThisAppPath, MpHelperSingleton.Instance.GetRandomColor());
                    qrCopyItem.WriteToDatabase();
                    MpTag historyTag = new MpTag(1);
                    historyTag.LinkWithCopyItem(qrCopyItem);
                    MainWindowViewModel.ClearSelection();
                    MainWindowViewModel.AddClipTile(qrCopyItem);
                }
            }                
        }

        private void ClipTileRichTextBox_PreviewLeftMouseButtonDown(object sender, MouseButtonEventArgs e) {
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control) {
                IsSelected = !IsSelected;
                _sourceSelectedClipTile = this;
            } else if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Shift) {
                if (_sourceSelectedClipTile == null) {
                    IsSelected = true;
                    _sourceSelectedClipTile = this;
                } else {
                    int curIdx = MainWindowViewModel.ClipTiles.IndexOf(this);
                    int targetIdx = MainWindowViewModel.ClipTiles.IndexOf(_sourceSelectedClipTile);
                    foreach (var selectedClipTile in MainWindowViewModel.SelectedClipTiles) {
                        selectedClipTile.IsSelected = false;
                    }
                    if (curIdx > targetIdx) {
                        for (int i = targetIdx; i <= curIdx; i++) {
                            MainWindowViewModel.ClipTiles[i].IsSelected = true;
                        }
                    } else {
                        for (int i = curIdx; i <= targetIdx; i++) {
                            MainWindowViewModel.ClipTiles[i].IsSelected = true;
                        }
                    }
                }
            } else {
                foreach (var selectedClipTile in MainWindowViewModel.SelectedClipTiles) {
                    selectedClipTile.IsSelected = false;
                }
                IsSelected = true;
                _sourceSelectedClipTile = this;
            }

            ((RichTextBox)sender).Selection.Select(((RichTextBox)sender).Document.ContentEnd, ((RichTextBox)sender).Document.ContentEnd);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public void MouseLeave() {
            IsHovering = false;
        }

        public void LostFocus() {
            //occurs when editing tag text
            IsEditingTitle = false;
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

        private async Task<string> ShortenUrl(string url) {
            string _bitlyToken = @"f6035b9ed05ac82b42d4853c984e34a4f1ba05d8";
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                "https://api-ssl.bitly.com/v4/shorten") {
                Content = new StringContent($"{{\"long_url\":\"{url}\"}}",
                                                Encoding.UTF8,
                                                "application/json")
            };

            try {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bitlyToken);

                var response = await client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode) {

                    Console.WriteLine("Minify error: " + response.Content.ToString());
                    return string.Empty;
                }

                var responsestr = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(responsestr);
                return jsonResponse["link"];
            }
            catch (Exception ex) {
                Console.WriteLine("Minify exception: " + ex.ToString());
                return string.Empty;
            }
        }
        #endregion

        #region Commands
        private RelayCommand<KeyEventArgs> _keyDownCommand;
        public ICommand KeyDownCommand {
            get {
                if(_keyDownCommand == null) {
                    _keyDownCommand = new RelayCommand<KeyEventArgs>(KeyDown,CanKeyDown);
                }
                return _keyDownCommand;
            }
        }
        private bool CanKeyDown(KeyEventArgs e) {
            return TileVisibility == Visibility.Visible;
        }
        private void KeyDown(KeyEventArgs e) {
            Key key = e.Key;
            if(key == Key.Delete || key == Key.Back && !IsEditingTitle) {
                //delete clip which shifts focus to neighbor
                MainWindowViewModel.DeleteClipCommand.Execute(null);
            } else if(key == Key.Enter) {
                if(IsEditingTitle) {
                    IsEditingTitle = false;
                    e.Handled = true;
                    return;
                } else {
                    //In order to paste the app must hide first
                    MainWindowViewModel.HideWindowCommand.Execute(null);
                    foreach(var clipTile in MainWindowViewModel.SelectedClipTiles) {
                        MainWindowViewModel.ClipboardMonitor.PasteCopyItem(clipTile.RichText);
                    }
                }
            }
        }

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
    }
}
