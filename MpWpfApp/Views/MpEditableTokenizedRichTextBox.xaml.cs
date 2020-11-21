using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public partial class MpEditableTokenizedRichTextBox : UserControl {
        #region Private Variables

        private TextRange _lastTokenRange = null;
        private MpSubTextToken _lastToken = null;

        #endregion

        #region Fields

        // Static member variables
        private static ToggleButton _SelectedAlignmentButton;
        private static ToggleButton _SelectedListButton;

        // Member variables
        private int _internalUpdatePending;
        private bool _textHasChanged;

        #endregion

        #region Dependency Property Declarations
        //------------------------------------------------------------------------------------
        [DefaultValue("Collapsed")]
        public Visibility ToolbarVisibility {
            get { 
                return (Visibility)GetValue(ToolbarVisibilityProperty); 
            }
            set { 
                SetValue(ToolbarVisibilityProperty, value); 
            }
        }
        public static readonly DependencyProperty ToolbarVisibilityProperty =
            DependencyProperty.RegisterAttached(
                nameof(ToolbarVisibility),
                typeof(Visibility),
                typeof(MpEditableTokenizedRichTextBox));

        //------------------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Brushes")]
        [Description("The background color of the formatting toolbar on the control.")]
        [DefaultValue("Gainsboro")]
        public Brush ToolbarBackground {
            get { return (Brush)GetValue(ToolbarBackgroundProperty); }
            set { SetValue(ToolbarBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ToolbarBackgroundProperty =
            DependencyProperty.RegisterAttached(
                nameof(ToolbarBackground),
                typeof(Brush),
                typeof(MpEditableTokenizedRichTextBox));

        //------------------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Brushes")]
        [Description("The color of the formatting toolbar border.")]
        [DefaultValue("Gray")]
        public Brush ToolbarBorderBrush {
            get {
                return (Brush)GetValue(ToolbarBorderBrushProperty);
            }
            set {
                SetValue(ToolbarBorderBrushProperty, value);
            }
        }
        public static readonly DependencyProperty ToolbarBorderBrushProperty =
            DependencyProperty.RegisterAttached(
                nameof(ToolbarBorderBrush),
                typeof(Brush),
                typeof(MpEditableTokenizedRichTextBox));

        //------------------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Other")]
        [Description("The thickness of the formatting toolbar border.")]
        [DefaultValue("1,1,1,0")]
        public Thickness ToolbarBorderThickness {
            get {
                return (Thickness)GetValue(ToolbarBorderThicknessProperty);
            }
            set {
                SetValue(ToolbarBorderThicknessProperty, value);
            }
        }
        public static readonly DependencyProperty ToolbarBorderThicknessProperty =
            DependencyProperty.RegisterAttached(
                nameof(ToolbarBorderThickness),
                typeof(Thickness),
                typeof(MpEditableTokenizedRichTextBox));

        //------------------------------------------------------------------------------------
        [Browsable(true)]
        [Category("Visibility")]
        [Description("Whether the code controls are visible in the toolbar.")]
        [DefaultValue("Collapsed")]
        public Visibility CodeControlsVisibility {
            get { 
                return (Visibility)GetValue(CodeControlsVisibilityProperty); 
            }
            set { 
                SetValue(CodeControlsVisibilityProperty, value); 
            }
        }
        public static readonly DependencyProperty CodeControlsVisibilityProperty =
            DependencyProperty.RegisterAttached(
                nameof(CodeControlsVisibility), 
                typeof(Visibility),
                typeof(MpEditableTokenizedRichTextBox));

        //------------------------------------------------------------------------------------
        public FlowDocument Document {
            get { 
                return (FlowDocument)GetValue(DocumentProperty); 
            }
            set { 
                SetValue(DocumentProperty, value); 
            }
        }
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached(
                nameof(Document), 
                typeof(FlowDocument),
                typeof(MpEditableTokenizedRichTextBox), 
                new FrameworkPropertyMetadata {
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = (s, e) => {
                        /* For unknown reasons, this method gets called twice when the 
                         * Document property is set. Until we figure out why, we initialize
                         * the flag to 2 and decrement it each time through this method. */

                        // Initialize
                        var thisControl = (MpEditableTokenizedRichTextBox)s;

                        // Exit if this update was internally generated
                        if (thisControl._internalUpdatePending > 0) {

                            // Decrement flags and exit
                            thisControl._internalUpdatePending--;
                            return;
                        }
                        var newDocument = e.NewValue == null ? new FlowDocument() : (FlowDocument)e.NewValue;
                        //instead of directly setting document this workaround ensures document reassignment doesn't fail
                        TextRange newRange = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);
                        MemoryStream stream = new MemoryStream();
                        System.Windows.Markup.XamlWriter.Save(newRange, stream);
                        newRange.Save(stream, DataFormats.XamlPackage);

                        var doc = new FlowDocument();
                        var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                        range.Load(stream, DataFormats.XamlPackage);

                        thisControl.TokenizedRichTextBox.Document = doc;

                        thisControl.RichText = MpHelpers.ConvertFlowDocumentToRichText(thisControl.TokenizedRichTextBox.Document);
                        
                        // Reset flag
                        thisControl._textHasChanged = false;
                    }
                });        

        //------------------------------------------------------------------------------------
        public string SearchText {
            get {
                return (string)GetValue(SearchTextProperty);
            }
            set {
                if ((string)GetValue(SearchTextProperty) != value) {
                    SetValue(SearchTextProperty, value);
                }
            }
        }
        public static readonly DependencyProperty SearchTextProperty =
          DependencyProperty.RegisterAttached(
            nameof(SearchText),
            typeof(string),
            typeof(MpEditableTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    var trtb = (MpEditableTokenizedRichTextBox)s;
                    trtb.HighlightSearchText(Brushes.Yellow);
                },
            });

        //------------------------------------------------------------------------------------
        public string RichText {
            get {
                return (string)GetValue(RichTextProperty);
            }
            set {
                if ((string)GetValue(RichTextProperty) != value) {
                    SetValue(RichTextProperty, value);
                }
            }
        }
        public static readonly DependencyProperty RichTextProperty =
          DependencyProperty.RegisterAttached(
           nameof(RichText),
            typeof(string),
            typeof(MpEditableTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    if (!string.IsNullOrEmpty((string)e.NewValue)) {
                        //((MpEditableTokenizedRichTextBox)s).TokenizedRichTextBox.SetRtf((string)e.NewValue);
                    }
                }
            });

        //------------------------------------------------------------------------------------
        public ObservableCollection<MpSubTextToken> Tokens {
            get {
                return (ObservableCollection<MpSubTextToken>)GetValue(TokensProperty);
            }
            set {
                if ((ObservableCollection<MpSubTextToken>)GetValue(TokensProperty) != value) {
                    SetValue(TokensProperty, value);
                }
            }
        }
        public static readonly DependencyProperty TokensProperty =
          DependencyProperty.RegisterAttached(
            nameof(Tokens),
            typeof(ObservableCollection<MpSubTextToken>),
            typeof(MpEditableTokenizedRichTextBox),
            new FrameworkPropertyMetadata {
                BindsTwoWayByDefault = true,
                PropertyChangedCallback = (s, e) => {
                    if (e.NewValue != null) {
                        foreach (var token in (ObservableCollection<MpSubTextToken>)e.NewValue) {
                            ((MpEditableTokenizedRichTextBox)s).AddSubTextToken(token);
                        }
                    }
                },
            });
        #endregion

        #region Constructor
        public MpEditableTokenizedRichTextBox() {
            InitializeComponent();
            this.Initialize();
        }

        #endregion

        #region Properties

        #endregion

        #region PropertyChanged Callback Methods


        #endregion

        #region Event Handlers

        /// <summary>
        /// Implements single-select on the alignment button group.
        /// </summary>
        private void OnAlignmentButtonClick(object sender, RoutedEventArgs e) {
            var clickedButton = (ToggleButton)sender;
            var buttonGroup = new[] { LeftButton, CenterButton, RightButton, JustifyButton };
            this.SetButtonGroupSelection(clickedButton, _SelectedAlignmentButton, buttonGroup, true);
            _SelectedAlignmentButton = clickedButton;
        }

        /// <summary>
        /// Formats code blocks.
        /// </summary>
        private void OnCodeBlockClick(object sender, RoutedEventArgs e) {
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Consolas");
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, "FireBrick");
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, 11D);
            textRange.ApplyPropertyValue(Block.MarginProperty, new Thickness(0));
        }

        /// <summary>
        /// Changes the font family of selected text.
        /// </summary>
        private void OnFontFamilyComboSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (FontFamilyCombo.SelectedItem == null) return;
            var fontFamily = FontFamilyCombo.SelectedItem.ToString();
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
        }

        /// <summary>
        /// Changes the font size of selected text.
        /// </summary>
        private void OnFontSizeComboSelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Exit if no selection
            if (FontSizeCombo.SelectedItem == null) return;

            // clear selection if value unset
            if (FontSizeCombo.SelectedItem.ToString() == "{DependencyProperty.UnsetValue}") {
                FontSizeCombo.SelectedItem = null;
                return;
            }

            // Process selection
            var pointSize = FontSizeCombo.SelectedItem.ToString();
            var pixelSize = Convert.ToDouble(pointSize) * (96 / 72);
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);
        }

        /// <summary>
        /// Formats inline code.
        /// </summary>
        private void OnInlineCodeClick(object sender, RoutedEventArgs e) {
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, "Consolas");
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, 11D);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, "FireBrick");
        }

        /// <summary>
        /// Implements single-select on the alignment button group.
        /// </summary>
        private void OnListButtonClick(object sender, RoutedEventArgs e) {
            var clickedButton = (ToggleButton)sender;
            var buttonGroup = new[] { BulletsButton, NumberingButton };
            this.SetButtonGroupSelection(clickedButton, _SelectedListButton, buttonGroup, false);
            _SelectedListButton = clickedButton;
        }

        /// <summary>
        /// Formats regular text
        /// </summary>
        private void OnNormalTextClick(object sender, RoutedEventArgs e) {
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamily);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, FontSize);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, Foreground);
            textRange.ApplyPropertyValue(Block.MarginProperty, new Thickness(Double.NaN));
        }

        /// <summary>
        /// Updates the toolbar when the text selection changes.
        /// </summary>
        private void OnTextBoxSelectionChanged(object sender, RoutedEventArgs e) {
            this.SetToolbar();
        }

        /// <summary>
        ///  Invoked when the user changes text in this user control.
        /// </summary>
        private void OnTextChanged(object sender, TextChangedEventArgs e) {
            // Set the TextChanged flag
            _textHasChanged = true;

            //TokenizedRichTextBox.Width = TokenizedRichTextBox.Document.GetFormattedText().WidthIncludingTrailingWhitespace + 20;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces an update of the Document property.
        /// </summary>
        public void UpdateDocumentBindings() {
            // Exit if text hasn't changed
            if (!_textHasChanged) return;

            // Set 'Internal Update Pending' flag
            _internalUpdatePending = 2;

            // Set Document property
            SetValue(DocumentProperty, this.TokenizedRichTextBox.Document);
        }

        public void AddSubTextToken(MpSubTextToken token) {
            try {
                if (token.TokenType == MpSubTextTokenType.CopyItemSegment) {
                    return;
                }

                Hyperlink tokenLink = GetTokenLink(token);
                if (tokenLink == null) {
                    Console.WriteLine("TokenizedTextbox error, GetTokenLink null for token: " + token.ToString());
                    return;
                }
                tokenLink.IsEnabled = true;
                tokenLink.RequestNavigate += (s, e) => {
                    System.Diagnostics.Process.Start(e.Uri.ToString());
                };

                MenuItem convertToQrCodeMenuItem = new MenuItem();
                convertToQrCodeMenuItem.Header = "Convert to QR Code";
                convertToQrCodeMenuItem.Click += (s, e1) => {
                    var hyperLink = (Hyperlink)((MenuItem)s).Tag;
                    Clipboard.SetImage(MpHelpers.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString()));
                };
                convertToQrCodeMenuItem.Tag = tokenLink;
                tokenLink.ContextMenu = new ContextMenu();
                tokenLink.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                switch (token.TokenType) {
                    case MpSubTextTokenType.StreetAddress:
                        tokenLink.NavigateUri = new Uri("https://google.com/maps/place/" + token.TokenText.Replace(' ', '+'));
                        break;
                    case MpSubTextTokenType.Uri:
                        if (!token.TokenText.Contains("https://")) {
                            tokenLink.NavigateUri = new Uri("https://" + token.TokenText);
                        } else {
                            tokenLink.NavigateUri = new Uri(token.TokenText);
                        }
                        MenuItem minifyUrl = new MenuItem();
                        minifyUrl.Header = "Minify with bit.ly";
                        minifyUrl.Click += (s, e2) => {
                            Hyperlink link = (Hyperlink)((MenuItem)s).Tag;
                            string minifiedLink = MpHelpers.ShortenUrl(link.NavigateUri.ToString()).Result;
                            Clipboard.SetText(minifiedLink);
                        };
                        minifyUrl.Tag = tokenLink;
                        tokenLink.ContextMenu.Items.Add(minifyUrl);
                        break;
                    case MpSubTextTokenType.Email:
                        tokenLink.NavigateUri = new Uri("mailto:" + token.TokenText);
                        break;
                    case MpSubTextTokenType.PhoneNumber:
                        tokenLink.NavigateUri = new Uri("tel:" + token.TokenText);
                        break;
                    case MpSubTextTokenType.Currency:
                        //"https://www.google.com/search?q=%24500.80+to+yen"
                        MenuItem convertCurrencyMenuItem = new MenuItem();
                        convertCurrencyMenuItem.Header = "Convert Currency To";
                        foreach (MpCurrencyType ct in Enum.GetValues(typeof(MpCurrencyType))) {
                            if (ct == MpCurrencyType.None || ct == MpHelpers.GetCurrencyTypeFromString(token.TokenText)) {
                                continue;
                            }
                            MenuItem subItem = new MenuItem();
                            subItem.Header = Enum.GetName(typeof(MpCurrencyType), ct);
                            subItem.Click += (s, e2) => {
                                // use https://free.currencyconverterapi.com/ instead of google
                                //string convertedCurrency = MpHelpers.CurrencyConvert(
                                //    (decimal)MpHelpers.GetCurrencyValueFromString(token.TokenText),
                                //    Enum.GetName(typeof(MpCurrencyType), MpHelpers.GetCurrencyTypeFromString(token.TokenText)),
                                //    Enum.GetName(typeof(MpCurrencyType), ct));
                                //tokenLink.Inlines.Clear();
                                //tokenLink.Inlines.Add(new Run(convertedCurrency));
                                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).HideWindowCommand.Execute(null);
                                System.Diagnostics.Process.Start(@"https://www.google.com/search?q=" + token.TokenText + "+to+" + subItem.Header);
                            };
                            convertCurrencyMenuItem.Items.Add(subItem);
                        }

                        tokenLink.ContextMenu.Items.Add(convertCurrencyMenuItem);
                        break;
                    default:

                        break;
                }
            }
            catch (Exception ex) {
                Console.WriteLine("TokenizedTextbox error, cannot add token text: " + token.TokenText + " of type: " + Enum.GetName(typeof(MpSubTextTokenType), token.TokenType) + Environment.NewLine + "with exception: " + ex.ToString());
            }
        }
        #endregion

        #region Private Methods
        private Hyperlink GetTokenLink(MpSubTextToken token) {
            Block block = Document.Blocks.ToArray()[token.BlockIdx];
            TextPointer searchStartPointer = block.ContentStart;
            if (_lastToken != null) {
                if (token.BlockIdx == _lastToken.BlockIdx) {
                    searchStartPointer = _lastTokenRange.End;
                }
            }
            TextRange tokenRange = MpHelpers.FindStringRangeFromPosition(searchStartPointer, token.TokenText);
            if (tokenRange == null) {
                Console.WriteLine("TokenizedRichTextBox error, cannot find textrange for token: " + token.ToString());
                return null;
            }
            _lastTokenRange = tokenRange;
            _lastToken = token;
            return new Hyperlink(tokenRange.Start, tokenRange.End);
        }

        private void HighlightSearchText(SolidColorBrush highlightColor) {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (Action)(() => {
                    var cb = (MpClipBorder)this.GetVisualAncestor<MpClipBorder>();
                    if (cb == null) {
                        throw new Exception("TokenizedRichTextBox error, cannot find clipborder");
                    }
                    if (cb.DataContext.GetType() != typeof(MpClipTileViewModel)) {
                        return;
                    }
                    var ctvm = (MpClipTileViewModel)cb.DataContext;
                    if (ctvm == null) {
                        throw new Exception("TokenizedRichTextBox error, cannot find cliptile viewmodel");
                    }
                    var sttvm = ctvm.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

                    TokenizedRichTextBox.BeginChange();
                    new TextRange(Document.ContentStart, Document.ContentEnd).ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                    ctvm.TileVisibility = Visibility.Collapsed;
                    if (!sttvm.Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                        ctvm.TileVisibility = Visibility.Collapsed;
                        TokenizedRichTextBox.EndChange();
                        //return;
                    } else if (SearchText == null ||
                        string.IsNullOrEmpty(SearchText.Trim()) ||
                        SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                        ctvm.TileVisibility = Visibility.Visible;
                        TokenizedRichTextBox.EndChange();
                        //return;
                    } else {
                        TextRange lastSearchTextRange = null;
                        for (TextPointer position = Document.ContentStart;
                         position != null && position.CompareTo(Document.ContentEnd) <= 0;
                         position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                            if (position.CompareTo(Document.ContentEnd) == 0) {
                                break;
                            }
                            string textRun = string.Empty;
                            int indexInRun = -1;
                            if (Properties.Settings.Default.IsSearchCaseSensitive) {
                                textRun = position.GetTextInRun(LogicalDirection.Forward);
                                indexInRun = textRun.IndexOf(SearchText, StringComparison.CurrentCulture);
                            } else {
                                textRun = position.GetTextInRun(LogicalDirection.Forward).ToLower();
                                indexInRun = textRun.IndexOf(SearchText.ToLower(), StringComparison.CurrentCulture);
                            }
                            if (indexInRun >= 0) {
                                position = position.GetPositionAtOffset(indexInRun);
                                if (position != null) {
                                    TextPointer nextPointer = position.GetPositionAtOffset(SearchText.Length);
                                    lastSearchTextRange = new TextRange(position, nextPointer);
                                    lastSearchTextRange.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                                }
                            }
                        }

                        if (lastSearchTextRange != null) {
                            ctvm.TileVisibility = Visibility.Visible;
                            TokenizedRichTextBox.ScrollToHome();
                            TokenizedRichTextBox.CaretPosition = Document.ContentStart;
                            Rect r = lastSearchTextRange.End.GetCharacterRect(LogicalDirection.Backward);
                            TokenizedRichTextBox.ScrollToVerticalOffset(500);// VerticalOffset r.Y - (FontSize * 0.5));
                                                        //var characterRect = lastTokenPointer.GetCharacterRect(LogicalDirection.Forward);
                                                        //this.ScrollToHorizontalOffset(this.HorizontalOffset + characterRect.Left - this.ActualWidth / 2d);
                                                        //this.ScrollToVerticalOffset(this.VerticalOffset + characterRect.Top - this.ActualHeight / 2d);
                                                        //ScrollToEnd();
                        } else {
                            ctvm.TileVisibility = Visibility.Collapsed;
                        }
                        TokenizedRichTextBox.EndChange();
                    }
                    var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
                    if (mwvm.ClipTrayViewModel.VisibileClipTiles.Count == 0 &&
                       !string.IsNullOrEmpty(SearchText) &&
                       SearchText != Properties.Settings.Default.SearchPlaceHolderText) {
                        mwvm.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Red;
                        mwvm.ClipTrayViewModel.ClipListVisibility = Visibility.Collapsed;
                        mwvm.ClipTrayViewModel.EmptyListMessageVisibility = Visibility.Visible;
                    } else {
                        mwvm.SearchBoxViewModel.SearchTextBoxBorderBrush = Brushes.Transparent;
                        mwvm.ClipTrayViewModel.ClipListVisibility = Visibility.Visible;
                        mwvm.ClipTrayViewModel.EmptyListMessageVisibility = Visibility.Collapsed;
                    }
                    //var fullDocRange = new TextRange(Document.ContentStart, Document.ContentEnd);
                    ////fullDocRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.White);

                    //ScrollToHome();
                    //if (SearchText != Properties.Settings.Default.SearchPlaceHolderText && !string.IsNullOrEmpty(SearchText)) {
                    //    string rtbt = fullDocRange.Text.ToLower();
                    //    SearchText = SearchText.ToLower();
                    //    var tokenIdxList = rtbt.AllIndexesOf(SearchText);
                    //    TextRange lastTokenRange = null;
                    //    CaretPosition = Document.ContentStart;
                    //    foreach (int idx in tokenIdxList) {
                    //        TextPointer startPoint = lastTokenRange == null ? Document.ContentStart : lastTokenRange.End;
                    //        startPoint.Po
                    //        var range = MpHelpers.FindStringRangeFromPosition(startPoint, SearchText);
                    //        if (range == null) {
                    //            Console.WriteLine("Cannot find '" + SearchText + "' in tile");
                    //        }
                    //        range?.ApplyPropertyValue(TextElement.BackgroundProperty, highlightColor);
                    //        lastTokenRange = range;
                    //    }
                    //    if (lastTokenRange != null) {
                    //        Rect r = lastTokenRange.End.GetCharacterRect(LogicalDirection.Backward);
                    //        ScrollToVerticalOffset(r.Y - (FontSize * 0.5));
                    //    }
                    //}
                    //EndChange();
                }));
        }

        /// <summary>
        /// Initializes the control.
        /// </summary>
        private void Initialize() {
            FontFamilyCombo.ItemsSource = Fonts.SystemFontFamilies;
            FontSizeCombo.Items.Add("10");
            FontSizeCombo.Items.Add("12");
            FontSizeCombo.Items.Add("14");
            FontSizeCombo.Items.Add("18");
            FontSizeCombo.Items.Add("24");
            FontSizeCombo.Items.Add("36");
        }

        /// <summary>
        /// Sets a selection in a button group.
        /// </summary>
        /// <param name="clickedButton">The button that was clicked.</param>
        /// <param name="currentSelectedButton">The currently-selected button in the group.</param>
        /// <param name="buttonGroup">The button group to which the button belongs.</param>
        /// <param name="ignoreClickWhenSelected">Whether to ignore a click on the button when it is selected.</param>
        private void SetButtonGroupSelection(ToggleButton clickedButton, ToggleButton currentSelectedButton, IEnumerable<ToggleButton> buttonGroup, bool ignoreClickWhenSelected) {
            /* In some cases, if the user clicks the currently-selected button, we want to ignore
             * the click; for example, when a text alignment button is clicked. In other cases, we
             * want to deselect the button, but do nothing else; for example, when a list butteting
             * or numbering button is clicked. The ignoreClickWhenSelected variable controls that
             * behavior. */

            // Exit if currently-selected button is clicked
            if (clickedButton == currentSelectedButton) {
                if (ignoreClickWhenSelected) clickedButton.IsChecked = true;
                return;
            }

            // Deselect all buttons
            foreach (var button in buttonGroup) {
                button.IsChecked = false;
            }

            // Select the clicked button
            clickedButton.IsChecked = true;
        }

        /// <summary>
        /// Sets the toolbar.
        /// </summary>
        private void SetToolbar() {
            // Set font family combo
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            var fontFamily = textRange.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamilyCombo.SelectedItem = fontFamily;

            // Set font size combo
            var fontSize = textRange.GetPropertyValue(TextElement.FontSizeProperty);
            FontSizeCombo.Text = fontSize.ToString();

            // Set Font buttons
            if (!String.IsNullOrEmpty(textRange.Text)) {
                BoldButton.IsChecked = textRange.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
                ItalicButton.IsChecked = textRange.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
                UnderlineButton.IsChecked = textRange.GetPropertyValue(Inline.TextDecorationsProperty).Equals(TextDecorations.Underline);
            }

            // Set Alignment buttons
            LeftButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
            CenterButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
            RightButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
            JustifyButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Justify);
        }

        #endregion
    }
}