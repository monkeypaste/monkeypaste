
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpRtbEditToolbarView.xaml
    /// </summary>
    public partial class MpRtbEditToolbarView : MpUserControl<MpClipTileViewModel> {
        bool _hasLoaded = false;

        ToggleButton selectedAlignmentButton;
        ToggleButton selectedListButton;

        RichTextBox Rtb {
            get {
                var ctv = this.GetVisualAncestor<MpClipTileView>();
                if (ctv == null) {
                    return null;
                }
                var cv = ctv.GetVisualDescendent<MpRtbContentView>();
                if (cv == null) {
                    return null;
                }
                return cv.Rtb;
            }
        }


        private ObservableCollection<string> _defaultFontSizes = new ObservableCollection<string>() {
            "8",
            "9",
            "10",
            "12",
            "14",
            "16",
            "18",
            "24",
            "36",
            "48",
            "72"
        };

        public MpRtbEditToolbarView() {
            InitializeComponent();
            FontSizeCombo.ItemsSource = _defaultFontSizes;
            Visibility = Visibility.Collapsed;
            
        }


        private void ClipTileEditorToolbar_Loaded(object sender, RoutedEventArgs e) {
            
        }
        private void ClipTileEditorToolbar_Unloaded(object sender, RoutedEventArgs e) {
            if (Rtb != null) {
            }
            _hasLoaded = false;
        }

        private void ClipTileEditorToolbarBorder_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(Rtb == null) {
                return;
            }
            if ((bool)e.NewValue) {
                if(!_hasLoaded) {
                    ClipTileEditorToolbarBorder.Resources["CurrentRtbTarget"] = Rtb;

                    var buttons = new List<ButtonBase>() {
                                PrintButton,
                                //CutButton,
                                //CopyButton,
                                //PasteButton,
                                UndoButton,
                                RedoButton,
                                BoldButton,
                                ItalicButton,
                                UnderlineButton,
                                LeftButton,
                                RightButton,
                                CenterButton,
                                NumberingButton,
                                BulletsButton
                            };

                    foreach (var b in buttons) {
                        b.CommandTarget = Rtb;
                    }
                    _hasLoaded = true;
                }

                Rtb.SelectionChanged += CurrentRtb_SelectionChanged;

                if(BindingContext != null && !BindingContext.IsPasting && !BindingContext.Parent.IsPasting) {
                    BindingContext.IsContentFocused = true;
                }
                
            } else {
                Rtb.SelectionChanged -= CurrentRtb_SelectionChanged;
            }
        }


        #region Toolbar Events

        public void CurrentRtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var fontFamily = Rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamilyCombo.SelectedItem = fontFamily;

            // Set font size combo
            var fontSizeObj = Rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            //FontSizeCombo.SelectedItem = fontSize;

            if (fontSizeObj == null || fontSizeObj.IsUnsetValue()) {
                fontSizeObj = string.Empty;
            } else {
                double fontSize = -1;
                try {
                    fontSize = Convert.ToDouble(fontSizeObj);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error converting font size to double obj str: " + fontSizeObj.ToString(), ex);
                }
                if (fontSize < 0) {
                    fontSizeObj = string.Empty;
                } else {
                    fontSize = Math.Round(fontSize, 0);
                    fontSizeObj = fontSize.ToString();
                }
            }
            if(fontSizeObj is string fontSizeStr) {
                if (fontSizeStr.EndsWith(".")) {
                    //remove trailing . if is whole number
                    fontSizeStr = fontSizeStr.Replace(".", string.Empty);
                }
                if (!_defaultFontSizes.Contains(fontSizeStr)) {

                    Func<string, double> doubleParser = input => {
                        double result;
                        if (!double.TryParse(input, out result))
                            return double.MinValue;

                        return result;
                    };

                    _defaultFontSizes = new ObservableCollection<string>(_defaultFontSizes.OrderBy(x => doubleParser(x)));
                    FontSizeCombo.ItemsSource = _defaultFontSizes;
                }
                
                FontSizeCombo.SelectedItem = fontSizeStr;
            } else {
                FontSizeCombo.SelectedItem = null;
            }

            // Set Font buttons
            BoldButton.IsChecked = Rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
            ItalicButton.IsChecked = Rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
            UnderlineButton.IsChecked = Rtb.Selection?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

            // Set Alignment buttons
            LeftButton.IsChecked = Rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
            CenterButton.IsChecked = Rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
            RightButton.IsChecked = Rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);

            AddTemplateButton.IsEnabled = CanAddTemplate(Rtb.Selection);

            object bgBrushObj = Rtb.Selection.GetPropertyValue(TextElement.BackgroundProperty);
            if (bgBrushObj is Brush bgBrush) {
                BackgroundColorButtonBorder.Background = bgBrush;
            } else {
                BackgroundColorButtonBorder.Background = Brushes.Transparent;
            }

            object fgBrushObj = Rtb.Selection.GetPropertyValue(TextElement.ForegroundProperty);
            if (fgBrushObj is Brush fgBrush) {
                ForegroundColorButtonBorder.Background = fgBrush;
            } else {
                ForegroundColorButtonBorder.Background = Brushes.Transparent;
            }

            Rtb.FitDocToRtb();
        }

        private bool CanAddTemplate(TextSelection ts) {
            if (Rtb == null) {
                return false;
            }
            //disable add template button if:
            //-current selection intersects with a template
            //-contains a space
            //-contains more than 10 characters
            bool canAddTemplate = true;

            if (Rtb.Selection.Start.Parent.GetType().IsSubclassOf(typeof(TextElement)) &&
               Rtb.Selection.End.Parent.GetType().IsSubclassOf(typeof(TextElement))) {
                MpTextTemplateViewModel thlvm = null;
                if (((TextElement)Rtb.Selection.Start.Parent).DataContext != null && ((TextElement)Rtb.Selection.Start.Parent).DataContext.GetType() == typeof(MpTextTemplateViewModel)) {
                    thlvm = (MpTextTemplateViewModel)((TextElement)Rtb.Selection.Start.Parent).DataContext;
                } else if (((TextElement)Rtb.Selection.End.Parent).DataContext != null && ((TextElement)Rtb.Selection.End.Parent).DataContext.GetType() == typeof(MpTextTemplateViewModel)) {
                    thlvm = (MpTextTemplateViewModel)((TextElement)Rtb.Selection.End.Parent).DataContext;
                }
                canAddTemplate = thlvm == null;
            }

            //if(canAddTemplate) {
            //    canAddTemplate = !ts.Text.Contains(" ");
            //}

            //if(canAddTemplate) {
            //    canAddTemplate = ts.Text.Length <= MonkeyPaste.MpPreferences.MaxTemplateTextLength;
            //}

            return canAddTemplate;
        }

        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(FontFamilyCombo.SelectedItem == null) {
                return;
            }
            
            var fontFamily = FontFamilyCombo.SelectedItem.ToString();
            var textRange = new TextRange(Rtb.Selection.Start, Rtb.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);

            Rtb.Document.ConfigureLineHeight();
            // NOTE re-selecting text because selection doesn't resize to new typeface
            Rtb.Selection.Select(Rtb.Selection.Start, Rtb.Selection.End);
        }

        private void FontSizeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
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
            var pixelSize = System.Convert.ToDouble(pointSize); // * (96 / 72);
            var textRange = new TextRange(Rtb.Selection.Start, Rtb.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);

            Rtb.Document.ConfigureLineHeight();
            // NOTE re-selecting text because selection doesn't resize to new typeface
            Rtb.Selection.Select(Rtb.Selection.Start, Rtb.Selection.End);
        }

        private void ForegroundColorButton_Click(object sender, RoutedEventArgs e) {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();
            

            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    Rtb.Selection.ApplyPropertyValue(FlowDocument.ForegroundProperty, (Brush)((Border)s1).Tag);
                }
            );
            ForegroundColorButton.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = ForegroundColorButton;
            colorContextMenu.IsOpen = true;
        }

        private void BackgroundColorButton_Click(object sender, RoutedEventArgs e) {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();           

            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    Rtb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)((Border)s1).Tag);
                    Rtb.GetVisualAncestor<MpRtbHighlightBehavior>()?.UpdateUniqueBackgrounds();
                }
            );

            BackgroundColorButton.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = BackgroundColorButton;
            colorContextMenu.IsOpen = true;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e) {            
            var dlg = new PrintDialog();
            dlg.PageRangeSelection = PageRangeSelection.AllPages;
            dlg.UserPageRangeEnabled = true;
            // Show and process save file dialog box results
            if (dlg.ShowDialog() == true) {
                //use either one of the below    
                // dlg.PrintVisual(RichTextControl as Visual, "printing as visual");
                dlg.PrintDocument((((IDocumentPaginatorSource)Rtb.Document).DocumentPaginator), "Printing Clipboard Item");
            }
        }

        private void AlignmentToggleButton_Click(object sender, RoutedEventArgs e) {
            var clickedButton = (ToggleButton)sender;
            var buttonGroup = new ToggleButton[] { LeftButton, CenterButton, RightButton };
            SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
            selectedAlignmentButton = clickedButton;
        }

        private void ListToggleButton_Click(object sender, RoutedEventArgs e) {
            var clickedButton = (ToggleButton)sender;
            var buttonGroup = new[] { BulletsButton, NumberingButton };
            SetButtonGroupSelection(clickedButton, selectedListButton, buttonGroup, false);
            selectedListButton = clickedButton;
        }

        private void SetButtonGroupSelection(ToggleButton clickedButton, ToggleButton currentSelectedButton, IEnumerable<ToggleButton> buttonGroup, bool ignoreClickWhenSelected) {
            /* In some cases, if the user clicks the currently-selected button, we want to ignore
             * the click; for example, when a text alignment button is clicked. In other cases, we
             * want to deselect the button, but do nothing else; for example, when a list butteting
             * or numbering button is clicked. The ignoreClickWhenSelected variable controls that
             * behavior. */

            // Exit if currently-selected button is clicked
            if (clickedButton == selectedAlignmentButton) {
                clickedButton.IsChecked = true;
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

    }
}
