using GalaSoft.MvvmLight.CommandWpf;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        public MpObservableCollection<FontFamily> SystemFonts {
            get {
                return new MpObservableCollection<FontFamily>(Fonts.SystemFontFamilies);
            }
        }

        private MpObservableCollection<string> _fontSizes = null;
        public MpObservableCollection<string> FontSizes {
            get {
                if (_fontSizes == null) {
                    _fontSizes = new MpObservableCollection<string>() {
                         "8",
                        "9",
                        "10",
                        "11",
                        "12",
                        "14",
                        "16",
                        "18",
                        "20",
                        "22",
                        "24",
                        "26",
                        "28",
                        "36",
                        "48",
                        "72"
                };
                }
                return _fontSizes;
            }
            set {
                if (_fontSizes != value) {
                    _fontSizes = value;
                    OnPropertyChanged(nameof(FontSizes));
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
            if(ClipTileViewModel.CopyItemType != MpCopyItemType.RichText) {
                return;
            }
            var sp = (StackPanel)sender;
            var et = sp.GetVisualAncestor<Border>();            
            var cb = (MpClipBorder)et.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var rtb = rtbc.FindName("ClipTileRichTextBox") as RichTextBox;
            var clipTray = MainWindowViewModel.ClipTrayViewModel.GetClipTray();
            var clipTrayScrollViewer = clipTray.GetDescendantOfType<ScrollViewer>(); 
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var addTemplateButton = (Button)et.FindName("AddTemplateButton");
            var editTemplateToolbarBorder = (Border)cb.FindName("ClipTileEditTemplateToolbar");
            var pasteTemplateToolbarBorder = (Border)cb.FindName("ClipTilePasteTemplateToolbar");
            var ds = MpHelpers.Instance.ConvertRichTextToFlowDocument(ClipTileViewModel.CopyItemRichText).GetDocumentSize();

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

            var foregroundColorButton = (Button)et.FindName("ForegroundColorButton");
            foregroundColorButton.Click += (s, e3) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.Instance.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        rtb.Selection.ApplyPropertyValue(FlowDocument.ForegroundProperty, (Brush)((Border)s1).Tag);
                    }
                );
                foregroundColorButton.ContextMenu = colorContextMenu;
                colorContextMenu.PlacementTarget = foregroundColorButton;
                colorContextMenu.IsOpen = true;
            };
            var backgroundColorButton = (Button)et.FindName("BackgroundColorButton");
            backgroundColorButton.Click += (s, e3) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.Instance.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        rtb.Selection.ApplyPropertyValue(FlowDocument.BackgroundProperty, (Brush)((Border)s1).Tag);
                    }
                );
                
                backgroundColorButton.ContextMenu = colorContextMenu;
                colorContextMenu.PlacementTarget = backgroundColorButton;
                colorContextMenu.IsOpen = true;
            };

            var printButton = (Button)et.FindName("PrintButton");
            printButton.Click += (s, e3) => {
                // Configure printer dialog box
                var dlg = new PrintDialog();
                dlg.PageRangeSelection = PageRangeSelection.AllPages;
                dlg.UserPageRangeEnabled = true;
                // Show and process save file dialog box results
                if (dlg.ShowDialog() == true) {
                    //use either one of the below    
                    // dlg.PrintVisual(RichTextControl as Visual, "printing as visual");
                    dlg.PrintDocument((((IDocumentPaginatorSource)rtb.Document).DocumentPaginator), "Printing Clipboard Item");
                }
            };

            var leftAlignmentButton = (ToggleButton)et.FindName("LeftButton");
            var centerAlignmentButton = (ToggleButton)et.FindName("CenterButton");
            var rightAlignmentButton = (ToggleButton)et.FindName("RightButton");
            leftAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton };
                SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
                selectedAlignmentButton = clickedButton;
            };
            centerAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton };
                SetButtonGroupSelection(clickedButton, selectedAlignmentButton, buttonGroup, true);
                selectedAlignmentButton = clickedButton;
            };
            rightAlignmentButton.Click += (s, e3) => {
                var clickedButton = (ToggleButton)s;
                var buttonGroup = new ToggleButton[] { leftAlignmentButton, centerAlignmentButton, rightAlignmentButton };
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

            ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).ApplicationHook.MouseWheel += (s, e) => {
                if (ClipTileViewModel.IsEditingTile) {
                    rtb.ScrollToVerticalOffset(rtb.VerticalOffset - e.Delta);
                }
            };

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
                    foreach (var ttcvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder) {
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

                    var img = new Image();
                    img.Source = (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Icons/Silk/icons/add.png"));
                    addNewMenuItem.Icon = img;
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
                        
                        double tileWidthMax = Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, ds.Width);
                        double tileWidthMin = ClipTileViewModel.TileBorderWidth;

                        double contentWidthMax = tileWidthMax - MpMeasurements.Instance.ClipTileEditModeContentMargin;
                        double contentWidthMin = ClipTileViewModel.TileContentWidth;

                        double rtbTopMax = ClipTileViewModel.EditRichTextBoxToolbarHeight;
                        double rtbTopMin = 0;

                        double editRtbToolbarTopMax = 0;
                        double editRtbToolbarTopMin = -ClipTileViewModel.EditRichTextBoxToolbarHeight;

                        double iconLeftMax = tileWidthMax - 125;// tileWidthMax - ClipTileViewModel.TileTitleIconSize;
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
                                if(ClipTileViewModel.IsEditingTile) {
                                    // Point transform = ((ListViewItem)clipTray.ItemContainerGenerator.ContainerFromItem(ClipTileViewModel)).TransformToVisual((Visual)clipTray.Parent).Transform(new Point());
                                    //Rect listViewItemBounds = VisualTreeHelper.GetDescendantBounds(cb);
                                    //listViewItemBounds.Offset(transform.X, transform.Y);
                                    clipTray.ScrollIntoView(ClipTileViewModel);
                                } else {

                                }
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
                var fontSize = rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
                if(fontSize == null || fontSize.ToString() == "{DependencyProperty.UnsetValue}") {
                    fontSize = string.Empty;
                } else {
                    fontSize = Math.Round((double)fontSize);
                }
                fontSizeCombo.Text = fontSize.ToString();

                // Set Font buttons
                ((ToggleButton)et.FindName("BoldButton")).IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
                ((ToggleButton)et.FindName("ItalicButton")).IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
                ((ToggleButton)et.FindName("UnderlineButton")).IsChecked = rtb.Selection?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

                // Set Alignment buttons
                leftAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Left);
                centerAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Center);
                rightAlignmentButton.IsChecked = rtb.Selection.GetPropertyValue(FlowDocument.TextAlignmentProperty).Equals(TextAlignment.Right);
                
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
        private RelayCommand _refreshDocumentCommand = null;
        public ICommand RefreshDocumentCommand {
            get {
                if(_refreshDocumentCommand == null) {
                    _refreshDocumentCommand = new RelayCommand(RefreshDocument);
                }
                return _refreshDocumentCommand;
            }
        }
        private void RefreshDocument() {
            ClipTileViewModel.GetRtb().ClearHyperlinks();
            ClipTileViewModel.GetRtb().CreateHyperlinks();
        }
        #endregion
    }
}
