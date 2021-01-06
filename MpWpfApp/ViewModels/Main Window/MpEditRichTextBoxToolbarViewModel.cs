using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpEditRichTextBoxToolbarViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }
        #endregion

        #region Properties

        #region Layout Properties      

        
        #endregion

        #region Visibility Properties
        
        #endregion

        #region Brush Properties
        public Brush AddTemplateButtonBackgroundBrush {
            get {
                if (IsAddTemplateButtonEnabled) {
                    return Brushes.Transparent;
                }
                return Brushes.LightGray;
            }
        }
        #endregion

        #region State Properties
        private bool _isAddTemplateButtonEnabled = true;
        public bool IsAddTemplateButtonEnabled {
            get {
                return _isAddTemplateButtonEnabled;
            }
            set {
                if (_isAddTemplateButtonEnabled != value) {
                    _isAddTemplateButtonEnabled = value;
                    OnPropertyChanged(nameof(IsAddTemplateButtonEnabled));
                    OnPropertyChanged(nameof(AddTemplateButtonBackgroundBrush));
                }
            }
        }
        #endregion

        #region Business Logic Properties
        public ICollection<FontFamily> SystemFonts {
            get {
                return Fonts.SystemFontFamilies;
            }
        }

        private List<string> _fontSizeList = null;
        public List<string> FontSizeList {
            get {
                if (_fontSizeList == null) {
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
                if (_fontSizeList != value) {
                    _fontSizeList = value;
                    OnPropertyChanged(nameof(FontSizeList));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpEditRichTextBoxToolbarViewModel() :base() { }
        public MpEditRichTextBoxToolbarViewModel(MpClipTileViewModel ctvm) : base() {
            ClipTileViewModel = ctvm;
        }

        public void ClipTileEditorToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var sp = (StackPanel)sender;
            var et = sp.GetVisualAncestor<Border>();
            var rtb = ClipTileViewModel.GetRtb();
            var cb = (MpClipBorder)et.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var addTemplateButton = (Button)et.FindName("AddTemplateButton");
            var editTemplateToolbarBorder = (Border)cb.FindName("ClipTileEditTemplateToolbar");
            var pasteTemplateToolbarBorder = (Border)cb.FindName("ClipTilePasteTemplateToolbar");
            Canvas.SetZIndex(et, 3);
            Canvas.SetZIndex(rtb, 1);

            addTemplateButton.PreviewMouseDown += (s, e3) => {
                e3.Handled = true;
                if (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count == 0) {
                    //if templates are NOT in the clip yet add one w/ default name
                    //var rtbSelection = rtb.Selection;
                    ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(null, true);
                    //rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                } else {
                    var templateContextMenu = new ContextMenu();
                    foreach (var ttcvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelList) {
                        Rectangle rect = new Rectangle();
                        rect.Fill = ttcvm.TemplateBrush;
                        rect.Width = 14;
                        rect.Height = 14;
                        rect.VerticalAlignment = VerticalAlignment.Center;
                        rect.HorizontalAlignment = HorizontalAlignment.Left;

                        TextBlock tb = new TextBlock();
                        tb.Text = ttcvm.TemplateName;
                        tb.FontSize = 14;
                        tb.HorizontalAlignment = HorizontalAlignment.Left;
                        tb.VerticalAlignment = VerticalAlignment.Center;

                        DockPanel dp1 = new DockPanel();
                        dp1.Children.Add(rect);
                        dp1.Children.Add(tb);
                        rect.SetValue(DockPanel.DockProperty, Dock.Left);
                        tb.SetValue(DockPanel.DockProperty, Dock.Right);

                        MenuItem tmi = new MenuItem();
                        tmi.Header = dp1;
                        tmi.Click += (s1, e5) => {
                            ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(ttcvm, false);
                            //ClipTileViewModel.EditTemplateToolbarViewModel.IsEditingTemplate = true;
                        };
                        templateContextMenu.Items.Add(tmi);
                    }
                    var addNewMenuItem = new MenuItem();
                    TextBlock tb2 = new TextBlock();
                    tb2.Text = "Add New...";
                    tb2.FontSize = 14;
                    tb2.HorizontalAlignment = HorizontalAlignment.Left;
                    tb2.VerticalAlignment = VerticalAlignment.Center;
                    addNewMenuItem.Header = tb2;
                    addNewMenuItem.Click += (s1, e5) => {
                        ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(null, true);
                        //ClipTileViewModel.EditTemplateToolbarViewModel.IsEditingTemplate = true;

                    };
                    templateContextMenu.Items.Add(addNewMenuItem);
                    addTemplateButton.ContextMenu = templateContextMenu;
                    templateContextMenu.PlacementTarget = addTemplateButton;
                    templateContextMenu.IsOpen = true;
                    //rtb.Selection.Select(ts.Start,ts.End);
                }
                //rtb.ScrollToHome();
                //rtb.CaretPosition = rtb.Document.ContentStart;
                //HasTemplate = rtb.GetTemplateHyperlinkList().Count > 0;
            };

            #region Editor
            ToggleButton selectedAlignmentButton = null;
            ToggleButton selectedListButton = null;

            var fontFamilyComboBox = (ComboBox)et.FindName("FontFamilyCombo");
            fontFamilyComboBox.SelectionChanged += (s, e1) => {
                if (fontFamilyComboBox.SelectedItem == null) {
                    return;
                }
                var fontFamily = fontFamilyComboBox.SelectedItem.ToString();
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
            };

            var fontSizeCombo = (ComboBox)et.FindName("FontSizeCombo");
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

            var leftAlignmentButton = (ToggleButton)et.FindName("LeftButton");
            var centerAlignmentButton = (ToggleButton)et.FindName("CenterButton");
            var rightAlignmentButton = (ToggleButton)et.FindName("RightButton");
            var justifyAlignmentButton = (ToggleButton)et.FindName("JustifyButton");
            leftAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
                selectedAlignmentButton = clickedButton;
            };
            centerAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
                selectedAlignmentButton = clickedButton;
            };
            rightAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
                selectedAlignmentButton = clickedButton;
            };
            justifyAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton, justifyAlignmentButton };
                SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
                selectedAlignmentButton = clickedButton;
            };

            var bulletsButton = (ToggleButton)et.FindName("BulletsButton");
            var numberingButton = (ToggleButton)et.FindName("NumberingButton");
            bulletsButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new[] { bulletsButton, numberingButton };
                this.SetButtonGroupSelection(clickedButton, selectedListButton, buttonGroup, false);
                selectedListButton = clickedButton;
            };
            numberingButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new[] { bulletsButton, numberingButton };
                this.SetButtonGroupSelection(clickedButton, selectedListButton, buttonGroup, false);
                selectedListButton = clickedButton;
            };
            #endregion

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(ClipTileViewModel.IsEditingTile):
                        //animates tile width, rtb top and edit tb top
                        double fromWidthTile = 0;
                        double toWidthTile = 0;

                        double fromWidthContent = 0;
                        double toWidthContent = 0;

                        double fromTopToolbar = 0;
                        double toTopToolbar = 0;

                        double fromTopRtb = 0;
                        double toTopRtb = 0;
                        if (ClipTileViewModel.IsEditingTile) {
                            ClipTileViewModel.EditToolbarVisibility = Visibility.Visible;
                            fromWidthTile = cb.ActualWidth;
                            toWidthTile = Math.Max(625, rtb.Document.GetFormattedText().WidthIncludingTrailingWhitespace);

                            fromWidthContent = rtb.ActualWidth;
                            toWidthContent = ClipTileViewModel.TileContentWidth;

                            fromTopToolbar = -ClipTileViewModel.EditRichTextBoxToolbarHeight;
                            toTopToolbar = 0;

                            fromTopRtb = 0;
                            toTopRtb = ClipTileViewModel.EditRichTextBoxToolbarHeight;
                        } else {
                            fromWidthTile = cb.ActualWidth;
                            toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;

                            fromWidthContent = rtb.ActualWidth;
                            toWidthContent = ClipTileViewModel.TileContentWidth;

                            fromTopToolbar = 0;
                            toTopToolbar = -ClipTileViewModel.EditRichTextBoxToolbarHeight;

                            fromTopRtb = ClipTileViewModel.EditRichTextBoxToolbarHeight;
                            toTopRtb = 0;
                        }

                        MpHelpers.AnimateDoubleProperty(
                            fromTopRtb,
                            toTopRtb,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            rtb,
                            Canvas.TopProperty,
                            (s1, e44) => {

                            });

                        MpHelpers.AnimateDoubleProperty(
                            fromTopToolbar,
                            toTopToolbar,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            et,
                            Canvas.TopProperty,
                            (s1, e4) => {
                                if(!ClipTileViewModel.IsEditingTile) {
                                    ClipTileViewModel.EditToolbarVisibility = Visibility.Collapsed;
                                }
                            });

                        MpHelpers.AnimateDoubleProperty(
                            fromWidthTile,
                            toWidthTile,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { cb, titleSwirl, rtb, et, rtbc, editTemplateToolbarBorder, pasteTemplateToolbarBorder },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                                rtb.Document.PageWidth = rtb.Width - rtb.Padding.Left - rtb.Padding.Right;
                                rtb.Document.PageHeight = rtb.Height - rtb.Padding.Top - rtb.Padding.Bottom;
                            });

                        double fromLeft = Canvas.GetLeft(titleIconImageButton);
                        double toLeft = toWidthTile - ClipTileViewModel.TileTitleHeight - 10;
                        MpHelpers.AnimateDoubleProperty(
                            fromLeft,
                            toLeft,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            titleIconImageButton,
                            Canvas.LeftProperty,
                            (s1, e23) => {

                            });
                        break;
                }
            };
            et.IsVisibleChanged += (s, e1) => {
                
            };

            rtb.SelectionChanged += (s, e6) => {
                // Set font family combo
                var fontFamily = rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
                fontFamilyComboBox.SelectedItem = fontFamily;

                // Set font size combo
                var fontSize = rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                fontSizeCombo.Text = fontSize.ToString();

                // Set Font buttons
                ((ToggleButton)et.FindName("BoldButton")).IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
                ((ToggleButton)et.FindName("ItalicButton")).IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
                ((ToggleButton)et.FindName("UnderlineButton")).IsChecked = rtb.Selection?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

                // Set Alignment buttons
                leftAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
                centerAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
                rightAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
                justifyAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Justify);

                //Console.WriteLine("Selection parent: " + rtb.Selection.Start.Parent.ToString());
                IsAddTemplateButtonEnabled = true;// rtb.Selection.Start.Parent.GetType() != typeof(InlineUIContainer);

                //foreach (var templateHyperlink in rtb.GetTemplateHyperlinkList()) {
                //    if (!rtb.Selection.Start.IsInSameDocument(templateHyperlink.ContentStart) ||
                //       !rtb.Selection.Start.IsInSameDocument(templateHyperlink.ContentEnd)) {
                //        continue;
                //    }
                //    
                //}

                if (((TextElement)rtb.Selection.Start.Parent).DataContext != null && ((TextElement)rtb.Selection.Start.Parent).DataContext.GetType() == typeof(MpTemplateHyperlinkViewModel)) {
                    var thlvm = (MpTemplateHyperlinkViewModel)((TextElement)rtb.Selection.Start.Parent).DataContext;
                    rtb.Selection.Select(thlvm.TemplateTextRange.Start, thlvm.TemplateTextRange.End);
                } else if (((TextElement)rtb.Selection.End.Parent).DataContext != null && ((TextElement)rtb.Selection.End.Parent).DataContext.GetType() == typeof(MpTemplateHyperlinkViewModel)) {
                    var thlvm = (MpTemplateHyperlinkViewModel)((TextElement)rtb.Selection.End.Parent).DataContext;
                    rtb.Selection.Select(thlvm.TemplateTextRange.Start, thlvm.TemplateTextRange.End);
                }

                //OnPropertyChanged(nameof(IsAddTemplateButtonEnabled));
            };
            Key lastKey = Key.None;
            TextPointer lastKeyPosition = null;
            rtb.PreviewKeyDown += (s, e4) => {
                lastKey = e4.Key;
                if (lastKey == Key.Back) {
                    lastKeyPosition = rtb.CaretPosition.GetNextContextPosition(LogicalDirection.Backward);
                } else if (lastKey == Key.Delete) {
                    lastKeyPosition = rtb.CaretPosition.GetNextInsertionPosition(LogicalDirection.Forward);
                }
                e4.Handled = false;
            };
            rtb.TextChanged += (s, e4) => {
                if(lastKeyPosition != null) {
                    MpTemplateHyperlinkViewModel thlvm = null;
                    if (((TextElement)lastKeyPosition.Parent).DataContext != null && ((TextElement)lastKeyPosition.Parent).DataContext.GetType() == typeof(MpTemplateHyperlinkViewModel)) {
                        thlvm = (MpTemplateHyperlinkViewModel)((TextElement)lastKeyPosition.Parent).DataContext;
                        rtb.Selection.Select(thlvm.TemplateTextRange.Start, thlvm.TemplateTextRange.End);
                        rtb.Selection.Text = string.Empty;
                        thlvm.Dispose();
                    } 
                }                
            };
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
        #endregion
        #region Commands

        #endregion
    }
}
