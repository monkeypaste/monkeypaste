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
        public double TileContentEditToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditToolbarHeight;
            }
        }
        private double _editToolbarTop = 0;
        public double EditToolbarTop {
            get {
                return _editToolbarTop;
            }
            set {
                if (_editToolbarTop != value) {
                    _editToolbarTop = value;
                    OnPropertyChanged(nameof(EditToolbarTop));
                }
            }
        }
        #endregion

        #region Visibility Properties
        public Visibility EditToolbarVisibility {
            get {
                if (ClipTileViewModel.IsEditingTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
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
        //public MpEditRichTextBoxToolbarViewModel() : this(new MpClipTileViewModel()) { }

        public MpEditRichTextBoxToolbarViewModel(MpClipTileViewModel ctvm) {
            ClipTileViewModel = ctvm;
        }

        public void ClipTileEditorToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var et = (Border)sender;
            var rtb = ClipTileViewModel.GetRtb();
            var cb = (MpClipBorder)et.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");

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

            et.IsVisibleChanged += (s, e1) => {
                double fromWidthTile = cb.ActualWidth;
                double toWidthTile = 0;

                double fromTopToolbar = 0;
                double toTopToolbar = 0;

                double fromTopRtb = 0;
                double toTopRtb = 0;
                if (et.Visibility == Visibility.Visible) {
                    toWidthTile = Math.Max(625, rtb.Document.GetFormattedText().WidthIncludingTrailingWhitespace);
                    
                    fromTopToolbar = -TileContentEditToolbarHeight;
                    toTopToolbar = 0;

                    fromTopRtb = 0;
                    toTopRtb = TileContentEditToolbarHeight;
                } else {
                    toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;

                    fromTopToolbar = 0;
                    toTopToolbar = -TileContentEditToolbarHeight;

                    fromTopRtb = TileContentEditToolbarHeight;
                    toTopRtb = 0;
                }

                MpHelpers.AnimateDoubleProperty(
                    fromTopRtb,
                    toTopRtb,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    rtb,
                    Canvas.TopProperty,
                    (s1, e) => {

                    });

                MpHelpers.AnimateDoubleProperty(
                    fromTopToolbar,
                    toTopToolbar,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    et,
                    Canvas.TopProperty,
                    (s1, e) => {

                    });

                MpHelpers.AnimateDoubleProperty(
                    fromWidthTile,
                    toWidthTile,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    new List<FrameworkElement> { cb, titleSwirl, rtb, et, rtbc },
                    FrameworkElement.WidthProperty,
                    (s1, e) => {
                        
                    });

                double fromLeft = Canvas.GetLeft(titleIconImageButton);
                double toLeft = toWidthTile - ClipTileViewModel.TileTitleHeight - 10;
                MpHelpers.AnimateDoubleProperty(
                    fromLeft,
                    toLeft,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    titleIconImageButton,
                    Canvas.LeftProperty,
                    (s1, e) => {

                    });
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

                Console.WriteLine("Selection parent: " + rtb.Selection.Start.Parent.ToString());
                IsAddTemplateButtonEnabled = true;// rtb.Selection.Start.Parent.GetType() != typeof(InlineUIContainer);

                //foreach (var templateHyperlink in rtb.GetTemplateHyperlinkList()) {
                //    if (!rtb.Selection.Start.IsInSameDocument(templateHyperlink.ContentStart) ||
                //       !rtb.Selection.Start.IsInSameDocument(templateHyperlink.ContentEnd)) {
                //        continue;
                //    }
                //    if ((rtb.Selection.Start.CompareTo(templateHyperlink.ContentStart) >= 0 &&
                //        rtb.Selection.Start.CompareTo(templateHyperlink.ContentEnd) <= 0) ||
                //       (rtb.Selection.End.CompareTo(templateHyperlink.ContentStart) >= 0 &&
                //        rtb.Selection.End.CompareTo(templateHyperlink.ContentEnd) <= 0)) {
                //        IsAddTemplateButtonEnabled = false;
                //    }
                //}

                OnPropertyChanged(nameof(IsAddTemplateButtonEnabled));
            };
            //rtb.LostFocus += (s, e3) => {
            //    IsEditingTile = false;
            //};
            //rtb.LostKeyboardFocus += (s, e7) => {
            //    e7.Handled = true;
            //};

            //rtb.LostFocus += (s, e7) => {
            //    e7.Handled = true;
            //};
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
