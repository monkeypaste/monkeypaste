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

    public class MpEditRichTextBoxToolbarViewModel : MpUndoableViewModelBase<MpEditRichTextBoxToolbarViewModel>, IDisposable {
        #region Private Variables
        #endregion        

        #region Properties

        #region View Models
        private MpClipTileViewModel _hostClipTileViewModel = null;
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                return _hostClipTileViewModel;
            }
            set {
                if (_hostClipTileViewModel != value) {
                    _hostClipTileViewModel = value;
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                }
            }
        }

        public MpRtbListBoxItemRichTextBoxViewModel SubSelectedRtbViewModel {
            get {
                if (HostClipTileViewModel == null) {
                    return null;
                }
                if (HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 0 ||
                   HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems.Count != 1) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0];
            }
        }
        #endregion

        #region Controls
        public Border EditToolbarBorder { get; set; }

        public StackPanel BorderStackPanel { get; set; }
        public RichTextBox LastRtb { get; set; }
        public RichTextBox SelectedRtb { get; set; }
        #endregion

        #region Layout Properties      
        private double _editBorderCanvasTop = -MpMeasurements.Instance.ClipTileEditToolbarHeight;
        public double EditBorderCanvasTop {
            get {
                return _editBorderCanvasTop;
            }
            set {
                if (_editBorderCanvasTop != value) {
                    _editBorderCanvasTop = value;
                    OnPropertyChanged(nameof(EditBorderCanvasTop));
                }
            }
        }
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
        private bool _useSpellCheck = Properties.Settings.Default.UseSpellCheck;
        public bool UseSpellCheck {
            get {
                return _useSpellCheck;
            }
            set {
                if (_useSpellCheck != value) {
                    _useSpellCheck = value;
                    OnPropertyChanged(nameof(UseSpellCheck));
                }
            }
        }

        private bool _hasTextChanged = false;
        public bool HasTextChanged {
            get {
                return _hasTextChanged;
            }
            set {
                if(_hasTextChanged != value) {
                    _hasTextChanged = value;
                    OnPropertyChanged(nameof(HasTextChanged));
                }
            }
        }

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
            HostClipTileViewModel = ctvm;
            
        }

        public void ClipTileEditorToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            if (HostClipTileViewModel.CopyItemType != MpCopyItemType.RichText && 
                HostClipTileViewModel.CopyItemType != MpCopyItemType.Composite) {
                return;
            }
            BorderStackPanel = (StackPanel)sender;
            EditToolbarBorder = BorderStackPanel.GetVisualAncestor<Border>();
        }

        public void InitWithRichTextBox(RichTextBox rtb, bool doAnimation) {
            HasTextChanged = false;
            SelectedRtb = rtb;
            EditToolbarBorder = BorderStackPanel.GetVisualAncestor<Border>();
            var cb = HostClipTileViewModel.ClipBorder;
            var rtblbgc = (Canvas)cb.FindName("ClipTileRichTextBoxListBoxGridContainerCanvas");
            var rtblbg = (Grid)rtblbgc.FindName("ClipTileRichTextboxListBoxContainerGrid");
            var rtblb = (ListBox)cb.FindName("ClipTileRichTextBoxListBox");
            var ctttg = (Grid)cb.FindName("ClipTileTitleTextGrid");
            var clipTray = MainWindowViewModel.ClipTrayViewModel.ListBox;
            var clipTrayScrollViewer = clipTray.GetDescendantOfType<ScrollViewer>();
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var addTemplateButton = (Button)EditToolbarBorder.FindName("AddTemplateButton");
            var editTemplateToolbarBorder = (Border)cb.FindName("ClipTileEditTemplateToolbar");
            var pasteTemplateToolbarBorder = (Border)cb.FindName("ClipTilePasteTemplateToolbar");
            //var ds = ClipTileViewModel.RichTextBoxViewModelCollection.FullDocument.GetDocumentSize();

            Canvas.SetTop(EditToolbarBorder, EditBorderCanvasTop);
            #region Editor

            #region Toolbar
            ToggleButton selectedAlignmentButton = null;
            ToggleButton selectedListButton = null;

            var fontFamilyComboBox = (ComboBox)EditToolbarBorder.FindName("FontFamilyCombo");
            var fontSizeCombo = (ComboBox)EditToolbarBorder.FindName("FontSizeCombo");
            var foregroundColorButton = (Button)EditToolbarBorder.FindName("ForegroundColorButton");
            var backgroundColorButton = (Button)EditToolbarBorder.FindName("BackgroundColorButton");
            var leftAlignmentButton = (ToggleButton)EditToolbarBorder.FindName("LeftButton");
            var centerAlignmentButton = (ToggleButton)EditToolbarBorder.FindName("CenterButton");
            var rightAlignmentButton = (ToggleButton)EditToolbarBorder.FindName("RightButton");
            var printButton = (Button)EditToolbarBorder.FindName("PrintButton");
            var bulletsButton = (ToggleButton)EditToolbarBorder.FindName("BulletsButton");
            var numberingButton = (ToggleButton)EditToolbarBorder.FindName("NumberingButton");
            var italicButton = (ToggleButton)EditToolbarBorder.FindName("ItalicButton");
            var boldButton = (ToggleButton)EditToolbarBorder.FindName("BoldButton");
            var underlineButton = (ToggleButton)EditToolbarBorder.FindName("UnderlineButton");

            var cutButton = (Button)EditToolbarBorder.FindName("CutButton");
            var copyButton = (Button)EditToolbarBorder.FindName("CopyButton");
            var pasteButton = (Button)EditToolbarBorder.FindName("PasteButton");
            var undoButton = (Button)EditToolbarBorder.FindName("UndoButton");
            var redoButton = (Button)EditToolbarBorder.FindName("RedoButton");

            cutButton.CommandTarget = rtb;
            copyButton.CommandTarget = rtb;
            pasteButton.CommandTarget = rtb;
            undoButton.CommandTarget = rtb;
            redoButton.CommandTarget = rtb;
            printButton.CommandTarget = rtb;
            italicButton.CommandTarget = rtb;
            boldButton.CommandTarget = rtb;
            underlineButton.CommandTarget = rtb;
            leftAlignmentButton.CommandTarget = rtb;
            centerAlignmentButton.CommandTarget = rtb;
            rightAlignmentButton.CommandTarget = rtb;
            bulletsButton.CommandTarget = rtb;
            numberingButton.CommandTarget = rtb;

            //SelectionChangedEventHandler FontFamilyComboBox_SelectionChanged = (s4,e1) => {
            //    if (fontFamilyComboBox.SelectedItem == null) {
            //        return;
            //    }
            //    var fontFamily = fontFamilyComboBox.SelectedItem.ToString();
            //    rtb.Focus();
            //    var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
            //    textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
            //};

            //if(_lastRtb != null) {
            //    fontFamilyComboBox.SelectionChanged -= FontFamilyComboBox_SelectionChanged;
            //}
            fontFamilyComboBox.SelectionChanged += (s4, e1) => {
                if (fontFamilyComboBox.SelectedItem == null) {
                    return;
                }
                var fontFamily = fontFamilyComboBox.SelectedItem.ToString();
                rtb.Focus();
                var textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
                textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
            };

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
            backgroundColorButton.Click += (s, e3) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.Instance.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        rtb.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, (Brush)((Border)s1).Tag);
                        HostClipTileViewModel.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(rtb);
                    }
                );

                backgroundColorButton.ContextMenu = colorContextMenu;
                colorContextMenu.PlacementTarget = backgroundColorButton;
                colorContextMenu.IsOpen = true;
            };

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

            #region Selection Changed
            rtb.SelectionChanged += Rtb_SelectionChanged;
            #endregion

            #endregion

            rtb.TextChanged += (s1, e3) => {
                HasTextChanged = true;                
            };

            #region Add Template Button
            addTemplateButton.PreviewMouseDown += (s, e3) => {
                e3.Handled = true;

                //SubSelectedRtbViewModel.SaveSubItemToDatabase();
                
                if (SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.Count == 0) {
                    //if templates are NOT in the clip yet add one w/ default name
                    HostClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(null, true);
                    //rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                } else {
                    var templateContextMenu = new ContextMenu();
                    foreach (var ttcvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder) {
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
                            HostClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(ttcvm, false);
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
                        HostClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(null, true);
                    };
                    templateContextMenu.Items.Add(addNewMenuItem);
                    addTemplateButton.ContextMenu = templateContextMenu;
                    templateContextMenu.PlacementTarget = addTemplateButton;
                    templateContextMenu.IsOpen = true;
                }
            };
            #endregion

            #region Animation
            if(doAnimation) {
                if(false) {
                    //double fromWidth = ClipTileViewModel.TileBorderMinWidth;
                    //double toWidth = ClipTileViewModel.TileBorderMaxWidth;

                    //DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
                    //timer.Interval = TimeSpan.FromSeconds(1/30);
                    //timer.Tick += (s, e32) => {
                    //    bool isWidthDone = false;
                    //    bool isHeightDone = false;

                    //    if(ClipTileViewModel.TileBorderWidth < toWidth) {
                    //        ClipTileViewModel.TileBorderWidth += 3;
                    //        ClipTileViewModel.TileContentWidth += 3;
                    //    } else {
                    //        isWidthDone = true;
                    //    }
                        
                    //    if(Canvas.GetTop(rtblbg) < ClipTileViewModel.EditRichTextBoxToolbarHeight) {
                    //        Canvas.SetTop(et, Canvas.GetTop(et) + 3);
                    //        Canvas.SetTop(rtblbg, Canvas.GetTop(rtblbg) + 3);
                    //        if(ClipTileViewModel.RichTextBoxViewModelCollection.RtbListBoxHeight > ClipTileViewModel.RichTextBoxViewModelCollection.RtbListBoxDesiredHeight) {
                    //            ClipTileViewModel.RichTextBoxViewModelCollection.RtbListBoxHeight -= 3;
                    //        }
                    //        foreach(var rtbvm in ClipTileViewModel.RichTextBoxViewModelCollection) {
                    //            rtbvm.OnPropertyChanged(nameof(rtbvm.RtbCanvasHeight));
                    //            rtbvm.OnPropertyChanged(nameof(rtbvm.RtbHeight));
                    //            rtbvm.OnPropertyChanged(nameof(rtbvm.RtbPageHeight));
                    //            rtbvm.OnPropertyChanged(nameof(rtbvm.RtbCanvasWidth));
                    //            rtbvm.OnPropertyChanged(nameof(rtbvm.RtbWidth));
                    //            rtbvm.OnPropertyChanged(nameof(rtbvm.RtbPageWidth));
                    //        }
                    //    } else {
                    //        isHeightDone = true;
                    //    }
                    //    if(isWidthDone && isHeightDone) {
                    //        timer.Stop();
                    //    }
                    //};
                    //timer.Start();
                } else {
                    //double animMs = 0;// Properties.Settings.Default.ShowMainWindowAnimationMilliseconds;
                    //double tileWidthMax = ClipTileViewModel.TileBorderMaxWidth;//Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, ds.Width);
                    //double tileWidthMin = ClipTileViewModel.TileBorderMinWidth;//MpMeasurements.Instance.ClipTileBorderMinSize;

                    //double contentWidthMax = ClipTileViewModel.TileContentMaxWidth;//tileWidthMax - MpMeasurements.Instance.ClipTileEditModeContentMargin;
                    //double contentWidthMin = ClipTileViewModel.TileContentMinWidth;//ClipTileViewModel.TileContentWidth;

                    //double rtbTopMax = ClipTileViewModel.EditRichTextBoxToolbarHeight;
                    //double rtbTopMin = 0;

                    //double rtbHeightMax = ClipTileViewModel.TileContentHeight;
                    //double rtbHeightMin = rtbHeightMax - rtbTopMax;

                    //double editRtbToolbarTopMax = 0;
                    //double editRtbToolbarTopMin = -ClipTileViewModel.EditRichTextBoxToolbarHeight;

                    //double iconLeftMax = tileWidthMax - 125;// tileWidthMax - ClipTileViewModel.TileTitleIconSize;
                    //double iconLeftMin = 204;// tileWidthMin - ClipTileViewModel.TileTitleIconSize;

                    //if (ClipTileViewModel.IsEditingTile) {
                    //    //show rtb edit toolbar so its visible during animation
                    //    ClipTileViewModel.EditToolbarVisibility = Visibility.Visible;
                    //    //ClipTileViewModel.ContainerVisibility = Visibility.Hidden;
                    //} else if (ClipTileViewModel.IsEditingTemplate) {
                    //    //animate edit template toolbar when tile is minimizing
                    //    ClipTileViewModel.IsEditingTemplate = false;
                    //} else {
                    //    ClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
                    //}

                    //MpHelpers.Instance.AnimateDoubleProperty(
                    //    ClipTileViewModel.IsEditingTile ? rtbTopMin : rtbTopMax,
                    //    ClipTileViewModel.IsEditingTile ? rtbTopMax : rtbTopMin,
                    //    animMs,
                    //    new List<FrameworkElement> { rtblbg },
                    //    Canvas.TopProperty,
                    //    (s1, e44) => {
                    //        ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbListBoxHeight));
                    //    });

                    //MpHelpers.Instance.AnimateDoubleProperty(
                    //    ClipTileViewModel.IsEditingTile ? rtbHeightMax : rtbHeightMin,
                    //    ClipTileViewModel.IsEditingTile ? rtbHeightMin : rtbHeightMax,
                    //    animMs,
                    //    new List<FrameworkElement> { rtblbg, rtblb },
                    //    FrameworkElement.HeightProperty,
                    //    (s1, e44) => {
                    //    //ClipTileViewModel.ContainerVisibility = Visibility.Visible;
                    //});

                    //MpHelpers.Instance.AnimateDoubleProperty(
                    //    ClipTileViewModel.IsEditingTile ? editRtbToolbarTopMin : editRtbToolbarTopMax,
                    //    ClipTileViewModel.IsEditingTile ? editRtbToolbarTopMax : editRtbToolbarTopMin,
                    //    animMs,
                    //    et,
                    //    Canvas.TopProperty,
                    //    (s1, e44) => {
                    //        if (ClipTileViewModel.IsEditingTile) {
                    //            ClipTileViewModel.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(0, false, false);
                    //            Rtb_SelectionChanged(this, new RoutedEventArgs());
                    //        } else {
                    //            ClipTileViewModel.EditToolbarVisibility = Visibility.Collapsed;

                    //            ClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();

                    //            ClipTileViewModel.RichTextBoxViewModelCollection.Refresh();
                    //        }
                    //    });

                    //MpHelpers.Instance.AnimateDoubleProperty(
                    //    ClipTileViewModel.IsEditingTile ? tileWidthMin : tileWidthMax,
                    //    ClipTileViewModel.IsEditingTile ? tileWidthMax : tileWidthMin,
                    //    animMs,
                    //    new List<FrameworkElement> { cb, titleSwirl },
                    //    FrameworkElement.WidthProperty,
                    //    (s1, e44) => {
                    //        //ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbHorizontalScrollbarVisibility));
                    //        //ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbVerticalScrollbarVisibility));
                    //        if (ClipTileViewModel.IsEditingTile) {
                    //            clipTray.ScrollIntoView(ClipTileViewModel);
                    //        } else {
                                
                    //        }
                    //        //ClipTileViewModel.TileBorderWidth = ClipTileViewModel.IsEditingTile ? tileWidthMax : tileWidthMin;
                    //    });

                    //MpHelpers.Instance.AnimateDoubleProperty(
                    //    ClipTileViewModel.IsEditingTile ? contentWidthMin : contentWidthMax,
                    //    ClipTileViewModel.IsEditingTile ? contentWidthMax : contentWidthMin,
                    //    animMs,
                    //    new List<FrameworkElement> { rtblbg, rtblb, rtblbgc, et, editTemplateToolbarBorder, pasteTemplateToolbarBorder },
                    //    FrameworkElement.WidthProperty,
                    //    (s1, e44) => {
                    //        //this is to remove scrollbar flicker during animation
                    //        //ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbHorizontalScrollbarVisibility));
                    //        //ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbVerticalScrollbarVisibility));
                    //        //ClipTileViewModel.TileContentWidth = ClipTileViewModel.IsEditingTile ? contentWidthMax : contentWidthMin;
                    //    });

                    //ClipTileViewModel.RichTextBoxViewModelCollection.AnimateItems(
                    //    ClipTileViewModel.IsEditingTile ? contentWidthMin : contentWidthMax,
                    //    ClipTileViewModel.IsEditingTile ? contentWidthMax : contentWidthMin,
                    //    ClipTileViewModel.IsEditingTile ? rtbHeightMax : rtbHeightMin,
                    //    ClipTileViewModel.IsEditingTile ? rtbHeightMin : rtbHeightMax,
                    //    0, 0,
                    //    0, 0,
                    //    animMs
                    //);

                    //MpHelpers.Instance.AnimateDoubleProperty(
                    //    ClipTileViewModel.IsEditingTile ? iconLeftMin : iconLeftMax,
                    //    ClipTileViewModel.IsEditingTile ? iconLeftMax : iconLeftMin,
                    //    animMs,
                    //    titleIconImageButton,
                    //    Canvas.LeftProperty,
                    //    (s1, e23) => {

                    //    });
                }
            }
            #endregion

            LastRtb = rtb;
        }

        public void Rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var rtb = sender as RichTextBox;

            //ToggleButton selectedAlignmentButton = null;
            //ToggleButton selectedListButton = null;

            var fontFamilyComboBox = (ComboBox)EditToolbarBorder.FindName("FontFamilyCombo");
            var fontSizeCombo = (ComboBox)EditToolbarBorder.FindName("FontSizeCombo");
            var foregroundColorButton = (Button)EditToolbarBorder.FindName("ForegroundColorButton");
            var backgroundColorButton = (Button)EditToolbarBorder.FindName("BackgroundColorButton");
            var leftAlignmentButton = (ToggleButton)EditToolbarBorder.FindName("LeftButton");
            var centerAlignmentButton = (ToggleButton)EditToolbarBorder.FindName("CenterButton");
            var rightAlignmentButton = (ToggleButton)EditToolbarBorder.FindName("RightButton");
            var printButton = (Button)EditToolbarBorder.FindName("PrintButton");
            var bulletsButton = (ToggleButton)EditToolbarBorder.FindName("BulletsButton");
            var numberingButton = (ToggleButton)EditToolbarBorder.FindName("NumberingButton");
            var italicButton = (ToggleButton)EditToolbarBorder.FindName("ItalicButton");
            var boldButton = (ToggleButton)EditToolbarBorder.FindName("BoldButton");
            var underlineButton = (ToggleButton)EditToolbarBorder.FindName("UnderlineButton");

            var cutButton = (Button)EditToolbarBorder.FindName("CutButton");
            var copyButton = (Button)EditToolbarBorder.FindName("CopyButton");
            var pasteButton = (Button)EditToolbarBorder.FindName("PasteButton");
            var undoButton = (Button)EditToolbarBorder.FindName("UndoButton");
            var redoButton = (Button)EditToolbarBorder.FindName("RedoButton");
            //Console.WriteLine("(SelectionChanged)Selection Text: " + rtb.Selection.Text);
            // Set font family combo
            var fontFamily = rtb.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            fontFamilyComboBox.SelectedItem = fontFamily;

            // Set font size combo
            var fontSize = rtb.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (fontSize == null || fontSize.ToString() == "{DependencyProperty.UnsetValue}") {
                fontSize = string.Empty;
            } else {
                fontSize = Math.Round((double)fontSize);
            }
            fontSizeCombo.Text = fontSize.ToString();

            // Set Font buttons
            ((ToggleButton)EditToolbarBorder.FindName("BoldButton")).IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontWeightProperty).Equals(FontWeights.Bold);
            ((ToggleButton)EditToolbarBorder.FindName("ItalicButton")).IsChecked = rtb.Selection.GetPropertyValue(TextElement.FontStyleProperty).Equals(FontStyles.Italic);
            ((ToggleButton)EditToolbarBorder.FindName("UnderlineButton")).IsChecked = rtb.Selection?.GetPropertyValue(Inline.TextDecorationsProperty)?.Equals(TextDecorations.Underline);

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
        }

        public void Resize(double deltaEditToolbarTop, double deltaWidth) {
            EditBorderCanvasTop += deltaEditToolbarTop;
            Canvas.SetTop(EditToolbarBorder, EditBorderCanvasTop);
            EditToolbarBorder.Width += deltaWidth;

            if (HostClipTileViewModel.IsEditingTile) {
                MainWindowViewModel.ClipTrayViewModel.ListBox.ScrollIntoView(HostClipTileViewModel);
                HostClipTileViewModel.RichTextBoxViewModelCollection.ResetSubSelection();
                //Rtb_SelectionChanged(this, new RoutedEventArgs());
            } else if(!HostClipTileViewModel.IsPastingTemplate) {
                HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
                //ClipTileViewModel.RichTextBoxViewModelCollection.Refresh();
            }
        }
        public void Animate(
            double deltaTop, 
            double tt, 
            EventHandler onCompleted, 
            double fps = 30,
            DispatcherPriority priority = DispatcherPriority.Render) {
            double fromTop = EditBorderCanvasTop;
            double toTop = fromTop + deltaTop;
            double dt = (deltaTop / tt) / fps;

            var timer = new DispatcherTimer(priority);
            timer.Interval = TimeSpan.FromMilliseconds(fps);
            timer.Tick += (s, e32) => {
                if (MpHelpers.Instance.DistanceBetweenValues(EditBorderCanvasTop, toTop) > 0.5) {
                    EditBorderCanvasTop += dt;
                    Canvas.SetTop(EditToolbarBorder, EditBorderCanvasTop);
                } else {
                    timer.Stop();
                    if (HostClipTileViewModel.IsEditingTile) {
                        MainWindowViewModel.ClipTrayViewModel.ListBox.ScrollIntoView(HostClipTileViewModel);
                        HostClipTileViewModel.RichTextBoxViewModelCollection.ResetSubSelection();
                        //Rtb_SelectionChanged(this, new RoutedEventArgs());
                    } else {
                        HostClipTileViewModel.RichTextBoxViewModelCollection.ClearSubSelection();
                        HostClipTileViewModel.RichTextBoxViewModelCollection.Refresh();
                    }
                    if (onCompleted != null) {
                        onCompleted.BeginInvoke(this, new EventArgs(), null, null);
                    }
                }
            };
            timer.Start();
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
                    _refreshDocumentCommand = new RelayCommand(RefreshDocument,CanRefreshDocument);
                }
                return _refreshDocumentCommand;
            }
        }
        private bool CanRefreshDocument() {
            return HasTextChanged && 
                   HostClipTileViewModel != null && 
                   HostClipTileViewModel.IsEditingTile &&
                   HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems.Count == 1;
        }
        private void RefreshDocument() {
            HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0].SaveSubItemToDatabase();
            HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0].ClearHyperlinks();
            HostClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedClipItems[0].CreateHyperlinks();
        }
        #endregion

        #region IDisposable
        public void Dispose() {
            EditToolbarBorder = null;
        }
        #endregion
    }
}
