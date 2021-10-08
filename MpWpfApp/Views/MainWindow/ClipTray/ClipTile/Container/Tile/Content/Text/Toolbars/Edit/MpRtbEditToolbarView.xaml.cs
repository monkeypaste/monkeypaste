using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpRtbEditToolbarView.xaml
    /// </summary>
    public partial class MpRtbEditToolbarView : UserControl {
        ToggleButton selectedAlignmentButton;
        ToggleButton selectedListButton;
        RichTextBox artb;
        List<ButtonBase> buttons;

        public MpRtbEditToolbarView() {
            InitializeComponent();

            FontFamilyCombo.ItemsSource = Fonts.SystemFontFamilies;
            Visibility = Visibility.Collapsed;
        }


        private void ClipTileEditorToolbar_Loaded(object sender, RoutedEventArgs e) {
            
        }

        public void SetActiveRtb(RichTextBox trtb) {
            AddTemplateButton.SetActiveRtb(trtb);
            artb = trtb;
            DataContext = artb.DataContext as MpContentItemViewModel;

            if(buttons == null) {
                buttons = new List<ButtonBase>() {
                PrintButton,
                CutButton,
                CopyButton,
                PasteButton,
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
            }

            foreach(var b in buttons) {
                b.CommandTarget = trtb;
            }
            trtb.SelectionChanged += CurrentRtb_SelectionChanged;
            artb = trtb;
            artb.IsReadOnly = false;
            artb.IsManipulationEnabled = true;
            artb.Focus();
            artb.CaretPosition = artb.Document.ContentStart;
            artb.Selection.Select(artb.Document.ContentStart, artb.Document.ContentStart);
            CurrentRtb_SelectionChanged(this, null);
            
        }

        #region Toolbar Events

        public void CurrentRtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var ertbtvm = DataContext as MpContentItemViewModel;

            var fontFamily = artb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamilyCombo.SelectedItem = fontFamily;

            // Set font size combo
            var fontSize = artb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSize == null || fontSize.ToString() == "{DependencyProperty.UnsetValue}") {
                fontSize = string.Empty;
            } else {
                fontSize = Math.Round((double)fontSize);
            }
            FontSizeCombo.Text = fontSize.ToString();

            // Set Font buttons
            BoldButton.IsChecked = artb.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
            ItalicButton.IsChecked = artb.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
            UnderlineButton.IsChecked = artb.Selection?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

            // Set Alignment buttons
            LeftButton.IsChecked = artb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
            CenterButton.IsChecked = artb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
            RightButton.IsChecked = artb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);

            AddTemplateButton.IsEnabled = CanAddTemplate(artb.Selection);

            //artb.UpdateLayout();
        }

        private bool CanAddTemplate(TextSelection ts) {
            //disable add template button if:
            //-current selection intersects with a template
            //-contains a space
            //-contains more than 10 characters
            bool canAddTemplate = true;

            if (artb.Selection.Start.Parent.GetType().IsSubclassOf(typeof(TextElement)) &&
               artb.Selection.End.Parent.GetType().IsSubclassOf(typeof(TextElement))) {
                MpTemplateViewModel thlvm = null;
                if (((TextElement)artb.Selection.Start.Parent).DataContext != null && ((TextElement)artb.Selection.Start.Parent).DataContext.GetType() == typeof(MpTemplateViewModel)) {
                    thlvm = (MpTemplateViewModel)((TextElement)artb.Selection.Start.Parent).DataContext;
                } else if (((TextElement)artb.Selection.End.Parent).DataContext != null && ((TextElement)artb.Selection.End.Parent).DataContext.GetType() == typeof(MpTemplateViewModel)) {
                    thlvm = (MpTemplateViewModel)((TextElement)artb.Selection.End.Parent).DataContext;
                }
                canAddTemplate = thlvm == null;
            }

            //if(canAddTemplate) {
            //    canAddTemplate = !ts.Text.Contains(" ");
            //}

            //if(canAddTemplate) {
            //    canAddTemplate = ts.Text.Length <= MonkeyPaste.MpPreferences.Instance.MaxTemplateTextLength;
            //}

            return canAddTemplate;
        }

        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(FontFamilyCombo.SelectedItem == null) {
                return;
            }
            
            var fontFamily = FontFamilyCombo.SelectedItem.ToString();
            artb.Focus();
            var textRange = new TextRange(artb.Selection.Start, artb.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);            
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
            artb.Focus();
            var pointSize = FontSizeCombo.SelectedItem.ToString();
            var pixelSize = System.Convert.ToDouble(pointSize); // * (96 / 72);
            var textRange = new TextRange(artb.Selection.Start, artb.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);
        }

        private void ForegroundColorButton_Click(object sender, RoutedEventArgs e) {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();
            

            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.Instance.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    artb.Selection.ApplyPropertyValue(FlowDocument.ForegroundProperty, (Brush)((Border)s1).Tag);
                }
            );
            ForegroundColorButton.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = ForegroundColorButton;
            colorContextMenu.IsOpen = true;
        }

        private void BackgroundColorButton_Click(object sender, RoutedEventArgs e) {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();

            var rtbetbvm = DataContext as MpContentItemViewModel;
            var hctvm = rtbetbvm.Parent;
            

            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.Instance.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    artb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)((Border)s1).Tag);
                    hctvm.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(artb);
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
                dlg.PrintDocument((((IDocumentPaginatorSource)artb.Document).DocumentPaginator), "Printing Clipboard Item");
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
