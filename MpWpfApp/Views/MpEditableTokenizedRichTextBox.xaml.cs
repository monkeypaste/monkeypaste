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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GongSolutions.Wpf.DragDrop.Utilities;
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

        #region Properties
        public bool ContainsTemplate { get; set; } = false;
        #endregion

        #region Dependency Properties
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
        [Browsable(true)]
        [Category("Visibility")]
        [Description("Whether the code controls are visible in the toolbar.")]
        [DefaultValue("Visible")]
        public Visibility AddTemplateButtonVisibility {
            get {
                return (Visibility)GetValue(AddTemplateButtonVisibilityProperty);
            }
            set {
                SetValue(AddTemplateButtonVisibilityProperty, value);
            }
        }
        public static readonly DependencyProperty AddTemplateButtonVisibilityProperty =
            DependencyProperty.RegisterAttached(
                nameof(AddTemplateButtonVisibility),
                typeof(Visibility),
                typeof(MpEditableTokenizedRichTextBox));

        //------------------------------------------------------------------------------------
        //public FlowDocument Document {
        //    get { 
        //        return (FlowDocument)GetValue(DocumentProperty); 
        //    }
        //    set { 
        //        SetValue(DocumentProperty, value); 
        //    }
        //}
        //public static readonly DependencyProperty DocumentProperty =
        //    DependencyProperty.RegisterAttached(
        //        nameof(Document), 
        //        typeof(FlowDocument),
        //        typeof(MpEditableTokenizedRichTextBox), 
        //        new FrameworkPropertyMetadata {
        //            BindsTwoWayByDefault = true,
        //            PropertyChangedCallback = (s, e) => {
        //                /* For unknown reasons, this method gets called twice when the 
        //                 * Document property is set. Until we figure out why, we initialize
        //                 * the flag to 2 and decrement it each time through this method. */

        //                // Initialize
        //                var thisControl = (MpEditableTokenizedRichTextBox)s;

        //                // Exit if this update was internally generated
        //                if (thisControl._internalUpdatePending > 0) {

        //                    // Decrement flags and exit
        //                    thisControl._internalUpdatePending--;
        //                    return;
        //                }
        //                var newDocument = e.NewValue == null ? new FlowDocument() : (FlowDocument)e.NewValue;
        //                //instead of directly setting document this workaround ensures document reassignment doesn't fail
        //                TextRange newRange = new TextRange(newDocument.ContentStart, newDocument.ContentEnd);
        //                MemoryStream stream = new MemoryStream();
        //                System.Windows.Markup.XamlWriter.Save(newRange, stream);
        //                newRange.Save(stream, DataFormats.XamlPackage);

        //                var doc = new FlowDocument();
        //                var range = new TextRange(doc.ContentStart, doc.ContentEnd);
        //                range.Load(stream, DataFormats.XamlPackage);

        //                thisControl.TokenizedRichTextBox.Document = doc;

        //                thisControl.RichText = MpHelpers.ConvertFlowDocumentToRichText(thisControl.TokenizedRichTextBox.Document);

        //                // Reset flag
        //                thisControl._textHasChanged = false;
        //            }
        //        });        

        //------------------------------------------------------------------------------------

        #endregion

        #region Constructor
        public MpEditableTokenizedRichTextBox() {
            InitializeComponent();
            this.Initialize();
        }

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
            if (FontFamilyCombo.SelectedItem == null) {
                return;
            }
            var fontFamily = FontFamilyCombo.SelectedItem.ToString();
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
        }

        /// <summary>
        /// Changes the font size of selected text.
        /// </summary>
        private void OnFontSizeComboSelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Exit if no selection
            if (FontSizeCombo.SelectedItem == null) {
                return;
            }

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

        private void OnAddTemplateButtonClick(object sender, RoutedEventArgs e) {
            var textRange = new TextRange(TokenizedRichTextBox.Selection.Start, TokenizedRichTextBox.Selection.End);
            //TokenizedRichTextBox.Selection.Text = "@?@<1_1>@?@";

           
            //BlockUIContainer container = new BlockUIContainer(stttcb);
            //TokenizedRichTextBox.Document.Blocks.InsertBefore(TokenizedRichTextBox.Document.Blocks.FirstBlock, container);

            ContainsTemplate = true;

            
            MpSubTextToken newTemplateToken = new MpSubTextToken(
                MpHelpers.GetRandomString(5) + "1_1" + MpHelpers.GetRandomString(5),
                MpSubTextTokenType.TemplateSegment, 
                0, 
                0, 
                0, 
                0,
                "Template #1",
                ((MpClipTileViewModel)((UIElement)sender).GetVisualAncestor<MpClipBorder>().DataContext).CopyItemId);
            TokenizedRichTextBox.Selection.Text = newTemplateToken.TokenText;
            TokenizedRichTextBox.AddSubTextToken(newTemplateToken);
            //TokenizedRichTextBox.DocumentRtf = TokenizedRichTextBox.Document;
            
            //TokenizedRichTextBox.AddSubTextToken()
            //save to string:
            //TokenizedRichTextBox.RichText = MpHelpers.ConvertFlowDocumentToRichText(TokenizedRichTextBox.Document);//XamlWriter.Save(TokenizedRichTextBox.Document);
            //TokenizedRichTextBox
        }

        private void OnRenameTemplateButtonClick(object sender, RoutedEventArgs e) {

        }

        private void OnRemoveTemplateButtonClick(object sender, RoutedEventArgs e) {

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
            //SetValue(TokenizedRichTextBox.Ric DocumentProperty, this.TokenizedRichTextBox.Document);
        }

        #endregion

        #region Private Methods
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
            //if (!String.IsNullOrEmpty(textRange.Text)) {

            //}
            BoldButton.IsChecked = textRange.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
            ItalicButton.IsChecked = textRange.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
            UnderlineButton.IsChecked = textRange?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

            // Set Alignment buttons
            LeftButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
            CenterButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
            RightButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
            JustifyButton.IsChecked = textRange.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Justify);
        }

        #endregion        
    }
}