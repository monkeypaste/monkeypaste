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
            var cb = (MpClipBorder)et.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var rtb = rtbc.FindName("ClipTileRichTextBox") as RichTextBox;
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var addTemplateButton = (Button)et.FindName("AddTemplateButton");
            var editTemplateToolbarBorder = (Border)cb.FindName("ClipTileEditTemplateToolbar");
            var pasteTemplateToolbarBorder = (Border)cb.FindName("ClipTilePasteTemplateToolbar");

            #region Editor
            ToggleButton selectedAlignmentButton = null;
            ToggleButton selectedListButton = null;

            var fontFamilyComboBox = (ComboBox)et.FindName("FontFamilyCombo");
            fontFamilyComboBox.SelectionChanged += (s, e1) => {
                if (fontFamilyComboBox.SelectedItem == null) {
                    return;
                }
                var fontFamily = fontFamilyComboBox.SelectedItem.ToString();
                rtb.Focus();
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
                rtb.Focus();
                var pointSize = fontSizeCombo.SelectedItem.ToString();
                var pixelSize = System.Convert.ToDouble(pointSize); // * (96 / 72);
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
            //rtb.SelectAll();
            //var alignment = (TextAlignment)rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty);
            //rtb.CaretPosition = rtb.Document.ContentStart;
            //SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);

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

            addTemplateButton.PreviewMouseDown += (s, e3) => {
                e3.Handled = true;

                var rtbSelection = rtb.Selection;

                ClipTileViewModel.SaveToDatabase();

                if (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count == 0) {
                    //if templates are NOT in the clip yet add one w/ default name
                    ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(null, true);
                    //rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                } else {
                    var templateContextMenu = new ContextMenu();
                    foreach (var ttcvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelList) {
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

                        DockPanel dp1 = new DockPanel();
                        dp1.Children.Add(b);
                        dp1.Children.Add(tb);
                        b.SetValue(DockPanel.DockProperty, Dock.Left);
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
                    };
                    templateContextMenu.Items.Add(addNewMenuItem);
                    addTemplateButton.ContextMenu = templateContextMenu;
                    templateContextMenu.PlacementTarget = addTemplateButton;
                    templateContextMenu.IsOpen = true;
                }
            };

            //animation
            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(ClipTileViewModel.IsEditingTile):
                        double tileWidthMax = 625;
                        double tileWidthMin = ClipTileViewModel.TileBorderWidth;

                        double contentWidthMax = 615;
                        double contentWidthMin = ClipTileViewModel.TileContentWidth;

                        double rtbTopMax = ClipTileViewModel.EditRichTextBoxToolbarHeight;
                        double rtbTopMin = 0;

                        double editRtbToolbarTopMax = 0;
                        double editRtbToolbarTopMin = -ClipTileViewModel.EditRichTextBoxToolbarHeight;

                        double iconLeftMax = 500;// tileWidthMax - ClipTileViewModel.TileTitleIconSize;
                        double iconLeftMin = 204;// tileWidthMin - ClipTileViewModel.TileTitleIconSize;
                        
                        if (ClipTileViewModel.IsEditingTile) {
                            //show rtb edit toolbar so its visible during animation
                            ClipTileViewModel.EditToolbarVisibility = Visibility.Visible;
                        } else if(ClipTileViewModel.IsEditingTemplate) {
                            //animate edit template toolbar when tile is minimizing
                            ClipTileViewModel.IsEditingTemplate = false;
                        } else {
                            //this is to remove scrollbar flicker during animation
                            ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.RtbHorizontalScrollbarVisibility));
                            ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.RtbVerticalScrollbarVisibility));
                        }

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTile ? rtbTopMin : rtbTopMax,
                            ClipTileViewModel.IsEditingTile ? rtbTopMax : rtbTopMin,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            rtb,
                            Canvas.TopProperty,
                            (s1, e44) => {

                            });

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTile ? editRtbToolbarTopMin : editRtbToolbarTopMax,
                            ClipTileViewModel.IsEditingTile ? editRtbToolbarTopMax : editRtbToolbarTopMin,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            et,
                            Canvas.TopProperty,
                            (s1, e44) => {
                                if (!ClipTileViewModel.IsEditingTile) {
                                    ClipTileViewModel.EditToolbarVisibility = Visibility.Collapsed;
                                }
                            });

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTile ? tileWidthMin : tileWidthMax,
                            ClipTileViewModel.IsEditingTile ? tileWidthMax : tileWidthMin,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { cb, titleSwirl },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                            });

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTile ? contentWidthMin : contentWidthMax,
                            ClipTileViewModel.IsEditingTile ? contentWidthMax : contentWidthMin,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            new List<FrameworkElement> { rtb, et, rtbc, editTemplateToolbarBorder, pasteTemplateToolbarBorder },
                            FrameworkElement.WidthProperty,
                            (s1, e44) => {
                                rtb.Document.PageWidth = ClipTileViewModel.IsEditingTile ? contentWidthMax - rtb.Padding.Left - rtb.Padding.Right - 20 : contentWidthMin - rtb.Padding.Left - rtb.Padding.Right;// - rtb.Padding.Left - rtb.Padding.Right;
                                //rtb.Document.PageHeight = rtb.ActualHeight;// ClipTileViewModel.IsEditingTile ? rtb.Document.GetDocumentSize().Height : ClipTileViewModel.TileContentHeight;
                                //rtb.UpdateDocumentLayout();
                                //this is to remove scrollbar flicker during animation
                                ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.RtbHorizontalScrollbarVisibility));
                                ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.RtbVerticalScrollbarVisibility));                                
                            });

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTile ? iconLeftMin : iconLeftMax,
                            ClipTileViewModel.IsEditingTile ? iconLeftMax : iconLeftMin,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            titleIconImageButton,
                            Canvas.LeftProperty,
                            (s1, e23) => {

                            });                        
                        break;
                }
            };
                        
            rtb.SelectionChanged += (s, e6) => {
                // Set font family combo
                var fontFamily = rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
                fontFamilyComboBox.SelectedItem = fontFamily;

                // Set font size combo
                var fontSize = Math.Round((double)rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty));
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
                    IsAddTemplateButtonEnabled = true;
                } else {
                    IsAddTemplateButtonEnabled = false;
                }
            };

            rtb.KeyUp += (s, e6) => {
                ClipTileViewModel.GetRtb().ClearHyperlinks();
                ClipTileViewModel.GetRtb().CreateHyperlinks();
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
