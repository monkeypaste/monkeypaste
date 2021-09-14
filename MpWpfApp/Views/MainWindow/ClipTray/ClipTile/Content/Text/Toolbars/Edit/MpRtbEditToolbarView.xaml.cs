using System;
using System.Collections.Generic;
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

        public MpRtbEditToolbarView() {
            InitializeComponent();
        }

        public void SetCommandTarget(RichTextBox trtb) {
            trtb.SelectionChanged += CurrentRtb_SelectionChanged;
            trtb.TextChanged += CurrentRtb_TextChanged;
            Resources["CurrentRtbTarget"] = trtb;
            var rtbetbvm = DataContext as MpEditRichTextBoxToolbarViewModel;
            rtbetbvm.HasTextChanged = false;
        }

        private void CurrentRtb_TextChanged(object sender, TextChangedEventArgs e) {
            var ertbtvm = DataContext as MpEditRichTextBoxToolbarViewModel;
            ertbtvm.HasTextChanged = true;
        }

        private void CurrentRtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var rtb = sender as RichTextBox;
            var ertbtvm = DataContext as MpEditRichTextBoxToolbarViewModel;

            var fontFamily = rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontFamilyCombo.SelectedItem = fontFamily;

            // Set font size combo
            var fontSize = rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSize == null || fontSize.ToString() == "{DependencyProperty.UnsetValue}") {
                fontSize = string.Empty;
            } else {
                fontSize = Math.Round((double)fontSize);
            }
            FontSizeCombo.Text = fontSize.ToString();

            // Set Font buttons
            BoldButton.IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
            ItalicButton.IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
            UnderlineButton.IsChecked = rtb.Selection?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

            // Set Alignment buttons
            LeftButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
            CenterButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
            RightButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);

            //disable add template button if current selection intersects with a template
            //this may not be necessary since templates are inlineuicontainers...
            MpTemplateHyperlinkViewModel thlvm = null;
            if (rtb.Selection.Start.Parent.GetType().IsSubclassOf(typeof(TextElement)) &&
               rtb.Selection.End.Parent.GetType().IsSubclassOf(typeof(TextElement))) {
                if (((TextElement)rtb.Selection.Start.Parent).DataContext != null && ((TextElement)rtb.Selection.Start.Parent).DataContext.GetType() == typeof(MpTemplateHyperlinkViewModel)) {
                    thlvm = (MpTemplateHyperlinkViewModel)((TextElement)rtb.Selection.Start.Parent).DataContext;
                } else if (((TextElement)rtb.Selection.End.Parent).DataContext != null && ((TextElement)rtb.Selection.End.Parent).DataContext.GetType() == typeof(MpTemplateHyperlinkViewModel)) {
                    thlvm = (MpTemplateHyperlinkViewModel)((TextElement)rtb.Selection.End.Parent).DataContext;
                }
            }
            if (thlvm == null) {
                ertbtvm.IsAddTemplateButtonEnabled = true;
            } else {
                ertbtvm.IsAddTemplateButtonEnabled = false;
            }
        }

        #region Toolbar Events
        private void FontFamilyCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(FontFamilyCombo.SelectedItem == null) {
                return;
            }
            var rtb = Resources["CurrentRtbTarget"] as RichTextBox;
            var fontFamily = FontFamilyCombo.SelectedItem.ToString();
            rtb.Focus();
            var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
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
            var rtb = Resources["CurrentRtbTarget"] as RichTextBox;
            // Process selection
            rtb.Focus();
            var pointSize = FontSizeCombo.SelectedItem.ToString();
            var pixelSize = System.Convert.ToDouble(pointSize); // * (96 / 72);
            var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, pixelSize);
        }

        private void ForegroundColorButton_Click(object sender, RoutedEventArgs e) {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();
            var rtb = Resources["CurrentRtbTarget"] as RichTextBox;

            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.Instance.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    rtb.Selection.ApplyPropertyValue(FlowDocument.ForegroundProperty, (Brush)((Border)s1).Tag);
                }
            );
            ForegroundColorButton.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = ForegroundColorButton;
            colorContextMenu.IsOpen = true;
        }

        private void BackgroundColorButton_Click(object sender, RoutedEventArgs e) {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();

            var rtbetbvm = DataContext as MpEditRichTextBoxToolbarViewModel;
            var hctvm = rtbetbvm.HostClipTileViewModel;
            var rtb = Resources["CurrentRtbTarget"] as RichTextBox;

            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.Instance.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    rtb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)((Border)s1).Tag);
                    hctvm.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(rtb);
                }
            );

            BackgroundColorButton.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = BackgroundColorButton;
            colorContextMenu.IsOpen = true;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e) {
            var rtb = Resources["CurrentRtbTarget"] as RichTextBox;
            var dlg = new PrintDialog();
            dlg.PageRangeSelection = PageRangeSelection.AllPages;
            dlg.UserPageRangeEnabled = true;
            // Show and process save file dialog box results
            if (dlg.ShowDialog() == true) {
                //use either one of the below    
                // dlg.PrintVisual(RichTextControl as Visual, "printing as visual");
                dlg.PrintDocument((((IDocumentPaginatorSource)rtb.Document).DocumentPaginator), "Printing Clipboard Item");
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
        #endregion

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

        private void AddTemplateButton_PreviewMouseDown(object sender, MouseButtonEventArgs e3) {
            e3.Handled = true;
            var rtbetbvm = DataContext as MpEditRichTextBoxToolbarViewModel;
            var hctvm = rtbetbvm.HostClipTileViewModel;
            var rtbcvm = hctvm.ContentContainerViewModel as MpRtbItemCollectionViewModel;
            var rtb = Resources["CurrentRtbTarget"] as RichTextBox;
            var rtblb = rtb.FindParentOfType<MpRtbItemCollectionView>();
            var rtbvm = rtb.DataContext as MpRtbItemViewModel;
            //SubSelectedRtbViewModel.SaveSubItemToDatabase();

            if (rtbvm.TemplateHyperlinkCollectionViewModel.Count == 0) {
                //if templates are NOT in the clip yet add one w/ default name
                //rtblb.EditTemplateView.SetActiveTemplate(null, true);

               rtbcvm.EditTemplateToolbarViewModel.SetTemplate(null, true);
                //rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            } else {
                var templateContextMenu = new ContextMenu();
                foreach (var ttcvm in rtbvm.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder) {
                    Border b = new Border();
                    b.Background = ttcvm.TemplateBrush;
                    b.BorderBrush = Brushes.Black;
                    b.BorderThickness = new Thickness(1);
                    b.Width = 14;
                    b.Height = 14;
                    b.VerticalAlignment = VerticalAlignment.Center;
                    b.HorizontalAlignment = HorizontalAlignment.Left;

                    TextBlock tb = new TextBlock();
                    tb.Text = ttcvm.TemplateDisplayName;//TemplateName.Replace("<", string.Empty).Replace(">", string.Empty);
                    tb.FontSize = 14;
                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.Margin = new Thickness(5, 0, 0, 0);


                    //DockPanel dp1 = new DockPanel();
                    //dp1.Children.Add(b);
                    //dp1.Children.Add(tb);
                    //b.SetValue(DockPanel.DockProperty, Dock.Left);
                    //tb.SetValue(DockPanel.DockProperty, Dock.Right);

                    MenuItem tmi = new MenuItem();
                    tmi.Icon = b;
                    tmi.Header = tb;
                    tmi.Click += (s1, e5) => {
                        rtbcvm.EditTemplateToolbarViewModel.SetTemplate(ttcvm, false);
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

                var img = new Image();
                img.Source = (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Icons/Silk/icons/add.png"));
                addNewMenuItem.Icon = img;
                addNewMenuItem.Header = tb2;
                addNewMenuItem.Click += (s1, e5) => {
                    rtbcvm.EditTemplateToolbarViewModel.SetTemplate(null, true);
                };
                templateContextMenu.Items.Add(addNewMenuItem);
                AddTemplateButton.ContextMenu = templateContextMenu;
                templateContextMenu.PlacementTarget = AddTemplateButton;
                templateContextMenu.IsOpen = true;
            }
        }
    }
}
