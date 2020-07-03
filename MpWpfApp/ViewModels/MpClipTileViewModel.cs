using MpWinFormsClassLibrary;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;

namespace MpWpfApp {
   public class  MpClipTileViewModel : MpViewModelBase {
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

        #region Appearance Properties
        public Point DragStart;

        private bool _isTitleTextBoxFocused = false;
        public bool IsTitleTextBoxFocused {
            get {
                return _isTitleTextBoxFocused;
            }
            set {
                if(_isTitleTextBoxFocused != value) {
                    _isTitleTextBoxFocused = value;
                    OnPropertyChanged("IsTitleTextBoxFocused");
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
                    OnPropertyChanged("IsSelected");                    
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
                    OnPropertyChanged("IsHovering");
                    if(!IsSelected) {
                        if(_isHovering) {
                            BorderBrush = Brushes.Yellow;
                        } else {
                            BorderBrush = Brushes.Transparent;
                        }
                    }
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
                    //tag names cannot be blank so don't allow the textblock to reappear and change name back to 'untitled'
                    //if(CopyItem.Title.Trim() == string.Empty) {
                    //    Title = "Untitled";
                    //    return;
                    //}
                    _isEditingTitle = value;
                    OnPropertyChanged("IsEditingTitle");
                }
            }
        }
        
        private Brush _borderBrush = Brushes.Transparent;
        public Brush BorderBrush {
            get {
                return _borderBrush;
            }
            set {
                if(_borderBrush != value) {
                    _borderBrush = value;
                    OnPropertyChanged("BorderBrush");
                }
            }
        }
        #endregion

        #region Layout 
        private Visibility _visibility = Visibility.Visible;
        public Visibility Visibility {
            get {
                return _visibility;
            }
            set {
                if(_visibility != value) {
                    _visibility = value;
                    OnPropertyChanged("Visibility");
                }
            }
        }

        private Visibility _textBoxVisibility = Visibility.Collapsed;
        public Visibility TextBoxVisibility {
            get {
                return _textBoxVisibility;
            }
            set {
                if(_textBoxVisibility != value) {
                    _textBoxVisibility = value;
                    OnPropertyChanged("TextBoxVisibility");
                }
            }
        }

        private Visibility _textBlockVisibility = Visibility.Visible;
        public Visibility TextBlockVisibility {
            get {
                return _textBlockVisibility;
            }
            set {
                if(_textBlockVisibility != value) {
                    _textBlockVisibility = value;
                    OnPropertyChanged("TextBlockVisibility");
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
                    OnPropertyChanged("TileSize");
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
                    OnPropertyChanged("TileBorderSize");
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
                    OnPropertyChanged("TileTitleHeight");
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
                    OnPropertyChanged("TileTitleHeight");
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
                    OnPropertyChanged("TileMargin");
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
                    OnPropertyChanged("Title");
                }
            }
        }

        public string Text {
            get {
                return CopyItem.Text;
            }
            set {
                CopyItem.SetData(value);
                OnPropertyChanged("Text");
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
                    OnPropertyChanged("CopyItem");
                }
            }
        }
        #endregion        

        #region Constructor
        public MpClipTileViewModel(MpCopyItem ci,MpMainWindowViewModel mwvm) {
            CopyItem = ci;
            MainWindowViewModel = mwvm;
            PropertyChanged += (s, e) => {
                if(e.PropertyName == "IsSelected") {
                    if(IsSelected) {
                        BorderBrush = Brushes.Red;
                    } else {
                        BorderBrush = Brushes.Transparent;
                    }
                } else if(e.PropertyName == "IsEditingTitle") {
                    if(IsEditingTitle) {
                        //show textbox and select all text
                        TextBoxVisibility = Visibility.Visible;
                        TextBlockVisibility = Visibility.Collapsed;
                        IsTitleTextBoxFocused = false;
                        IsTitleTextBoxFocused = true;
                    } else {
                        TextBoxVisibility = Visibility.Collapsed;
                        TextBlockVisibility = Visibility.Visible;
                        IsTitleTextBoxFocused = false;
                        CopyItem.WriteToDatabase();
                    }
                }
            };
        }
        #endregion

        #region View Events Handlers
        public void MouseEnter() {
            IsHovering = true;
        }

        public void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            if(CopyItem.CopyItemType == MpCopyItemType.Image || CopyItem.CopyItemType == MpCopyItemType.FileList) {
                return;
            }
            //First load the richtextbox with copytext
            var rtb = (RichTextBox)((Border)sender)?.FindName("ClipTileRichTextBox");
            rtb.SetRtf(Text);
            //TextRange rtbRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            //FormattedText ft = new FormattedText(rtbRange.Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(rtb.Document.FontFamily.ToString()), rtb.Document.FontSize, rtb.Document.Foreground,VisualTreeHelper.GetDpi(rtb).PixelsPerDip);
            //rtb.Width = ft.MaxTextWidth + rtb.Padding.Left + rtb.Padding.Right;
            //rtb.Height = ft.MaxTextHeight + rtb.Padding.Top + rtb.Padding.Bottom;           
            //rtb.Document.PageWidth = 10000;
            //rtb.Document.PageHeight = 10000;
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
                    switch (token.TokenType) {
                        case MpCopyItemType.WebLink:
                            if (!tokenText.Contains("https://")) {
                                tokenLink.NavigateUri = new Uri("https://" + tokenText);
                            } else {
                                tokenLink.NavigateUri = new Uri(tokenText);
                            }
                            break;

                        case MpCopyItemType.Email:
                            tokenLink.NavigateUri = new Uri("mailto:" + tokenText);
                            break;

                        case MpCopyItemType.PhoneNumber:
                            tokenLink.NavigateUri = new Uri("tel:" + tokenText);
                            break;
                        default:

                            break;
                    }
                }
            }
        }
        private TextRange FindStringRangeFromPosition(TextPointer position,string str) {
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
        #endregion

        #region Private Methods
        #endregion

        #region Commands

        private DelegateCommand<KeyEventArgs> _keyDownCommand;
        public ICommand KeyDownCommand {
            get {
                if(_keyDownCommand == null) {
                    _keyDownCommand = new DelegateCommand<KeyEventArgs>(KeyDown,CanKeyDown);
                }
                return _keyDownCommand;
            }
        }
        private bool CanKeyDown(KeyEventArgs e) {
            return Visibility == Visibility.Visible;
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
                    var mw = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext);
                    mw.HideWindowCommand.Execute(null);
                    foreach(var clipTile in mw.SelectedClipTiles) {
                        mw.ClipboardManager.PasteCopyItem(clipTile.CopyItem.Text);
                    }
                }
            }
        }
        #endregion
    }
}
