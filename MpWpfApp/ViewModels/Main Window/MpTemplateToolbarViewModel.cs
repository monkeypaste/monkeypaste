using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpTemplateToolbarViewModel : MpObservableCollectionViewModel<MpTemplateHyperlinkViewModel> {
        #region Private Variables
        private Button _colorButtonRef = null;
        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }

        private MpEditTemplateHyperlinkViewModel _editTemplateHyperlinkViewModel = null;
        public MpEditTemplateHyperlinkViewModel EditTemplateHyperlinkViewModel {
            get {
                return _editTemplateHyperlinkViewModel;
            }
            set {
                if(_editTemplateHyperlinkViewModel != value) {
                    _editTemplateHyperlinkViewModel = value;
                    OnPropertyChanged(nameof(EditTemplateHyperlinkViewModel));
                }
            }
        }

        public MpTemplateHyperlinkViewModel SelectedTemplateHyperlinkViewModel {
            get {
                foreach (var ttcvm in this) {
                    if (ttcvm.IsSelected) {
                        return ttcvm;
                    }
                }
                return null;
            }
            set {
                if (SelectedTemplateHyperlinkViewModel != value) {
                    foreach (var ttcvm in this) {
                        //clear any other selections
                        ttcvm.IsSelected = false;
                    }
                    if(value != null && this.Contains(value)) {
                        this[this.IndexOf(value)].IsSelected = true;
                    }
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }
        #endregion

        #region Properties
        #region Layout Properties
        public double TileContentPasteToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTilePasteToolbarHeight;
            }
        }

        private double _templateToolbarTop = 0;
        public double TemplateToolbarTop {
            get {
                return _templateToolbarTop;
            }
            set {
                if(_templateToolbarTop != value) {
                    _templateToolbarTop = value;
                    OnPropertyChanged(nameof(TemplateToolbarTop));
                }
            }
        }
        #endregion

        #region Visibility Properties
        public Visibility TemplateToolbarVisibility {
            get {
                if (IsEditingTemplate || IsPastingTemplateTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }       

        public Visibility PasteTemplateToolbarVisibility {
            get {
                if (IsPastingTemplateTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearAllTemplateToolbarButtonVisibility {
            get {
                foreach (var templateLookup in this) {
                    if (!string.IsNullOrEmpty(templateLookup.TemplateText)) {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearSelectedTemplateTextboxButtonVisibility {
            get {
                if (SelectedTemplateHyperlinkViewModel != null &&
                    SelectedTemplateText.Length > 0 &&
                    SelectedTemplateText != SelectedTemplateTextBoxPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TemplateNavigationButtonStackVisibility {
            get {
                if (this.Count > 1) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        #endregion

        #region Brush Properties

        #endregion

        #region State Properties
        private bool _isSelectedTemplateTextBoxFocused = false;
        public bool IsSelectedTemplateTextBoxFocused {
            get {
                return _isSelectedTemplateTextBoxFocused;
            }
            set {
                if (_isSelectedTemplateTextBoxFocused != value) {
                    _isSelectedTemplateTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsSelectedTemplateTextBoxFocused));
                }
            }
        }

        private bool _isEditingTemplate = false;
        public bool IsEditingTemplate {
            get {
                return _isEditingTemplate;
            }
            set {
                if (_isEditingTemplate != value) {
                    _isEditingTemplate = value;
                    OnPropertyChanged(nameof(IsEditingTemplate)); 
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxHeight));
                    OnPropertyChanged(nameof(TemplateToolbarVisibility));
                    //OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                    EditTemplateHyperlinkViewModel.OnPropertyChanged(nameof(EditTemplateHyperlinkViewModel.EditTemplateToolbarVisibility));
                    
                }
            }
        }

        private bool _isPastingTemplateTile = false;
        public bool IsPastingTemplateTile {
            get {
                return _isPastingTemplateTile;
            }
            set {
                if (_isPastingTemplateTile != value) {
                    _isPastingTemplateTile = value;
                    OnPropertyChanged(nameof(IsPastingTemplateTile));
                    OnPropertyChanged(nameof(TemplateToolbarVisibility));
                    OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                    ClipTileViewModel.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxHeight));
                }
            }
        }

        public bool IsTemplateReadyToPaste {
            get {
                foreach(var thlvm in this) {
                    if(string.IsNullOrEmpty(thlvm.TemplateText)) {
                        return false;
                    }
                }
                return true;
            }
        }

        public string SelectedTemplateText {
            get {
                if(SelectedTemplateHyperlinkViewModel == null) {
                    return string.Empty;
                }
                return SelectedTemplateHyperlinkViewModel.TemplateText;
            }
            set {
                if (SelectedTemplateHyperlinkViewModel != null &&
                    SelectedTemplateHyperlinkViewModel.TemplateText != value) {
                    SelectedTemplateHyperlinkViewModel.TemplateText = value;
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
                }
            }
        }

        public string SelectedTemplateTextBoxPlaceHolderText {
            get {
                if (SelectedTemplateHyperlinkViewModel == null) {
                    return string.Empty;
                }
                return SelectedTemplateHyperlinkViewModel.TemplateName + "...";
            }
        }

        public Brush SelectedTemplateTextBrush {
            get {
                if (SelectedTemplateText != SelectedTemplateTextBoxPlaceHolderText) {
                    return Brushes.Black;
                }
                return Brushes.DimGray;
            }
        }

        public FontStyle SelectedTemplateTextBoxFontStyle {
            get {
                if (SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    return FontStyles.Italic;
                }
                return FontStyles.Normal;
            }
        }
        #endregion

        #endregion

        #region Public Methods
        //public MpPasteTemplateToolbarViewModel() : this(new MpClipTileViewModel()) { }

        public MpTemplateToolbarViewModel(MpClipTileViewModel ctvm) {            
            ClipTileViewModel = ctvm;
            EditTemplateHyperlinkViewModel = new MpEditTemplateHyperlinkViewModel(this);

            foreach(var cit in ClipTileViewModel.CopyItem.TemplateList) {
                this.Add(new MpTemplateHyperlinkViewModel(this, cit));
            }
        }

        public void ClipTilePasteTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var ttbb = (Border)sender;
            var ettbg = (Grid)ttbb.FindName("EditTemplateToolbarGrid");
            var pttbg = (Grid)ttbb.FindName("PasteTemplateToolbarGrid");
            var rtb = ClipTileViewModel.GetRtb();
            var cb = (MpClipBorder)ttbb.GetVisualAncestor<MpClipBorder>();
            var et = (Border)cb.FindName("ClipTileEditorToolbar");
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var addTemplateButton = (Button)et.FindName("AddTemplateButton");
            var clearAllTemplatesButton = (Button)ttbb.FindName("ClearAllTemplatesButton");
            var selectedTemplateTextBox = (TextBox)ttbb.FindName("SelectedTemplateTextBox");
            var previousTemplateButton = (Button)ttbb.FindName("PreviousTemplateButton");
            var nextTemplateButton = (Button)ttbb.FindName("NextTemplateButton");
            var pasteTemplateButton = (Button)ttbb.FindName("PasteTemplateButton");

            Canvas.SetZIndex(ttbb, 5);
            addTemplateButton.PreviewMouseDown += (s, e3) => {
                e3.Handled = true;
                if (this.Count == 0) {
                    //if templates are NOT in the clip yet add one w/ default name
                    //MpEditTemplateHyperlinkViewModel.ShowTemplateTokenEditModalWindow(this, null, true);
                    var rtbSelection = rtb.Selection;
                    EditTemplateHyperlinkViewModel.SetTemplate(null, true);
                    IsEditingTemplate = true;
                    //ettbg.UpdateLayout();                    
                    //cb.GetVisualAncestor<ListBox>().Items.Refresh();
                    rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                } else {
                    var templateContextMenu = new ContextMenu();
                    foreach (var ttcvm in this) {
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
                            //MpEditTemplateHyperlinkViewModel.ShowTemplateTokenEditModalWindow(this, ttcvm, false);
                            EditTemplateHyperlinkViewModel.SetTemplate(ttcvm, false);
                            IsEditingTemplate = true;
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
                        //MpEditTemplateHyperlinkViewModel.ShowTemplateTokenEditModalWindow(this, null, true);
                        EditTemplateHyperlinkViewModel.SetTemplate(null, true);
                        IsEditingTemplate = true;

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

            selectedTemplateTextBox.GotFocus += (s, e4) => {
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = string.Empty;
                }

                IsSelectedTemplateTextBoxFocused = true;
            };
            selectedTemplateTextBox.LostFocus += (s, e5) => {
                //ensures current template textbox show the template name as a placeholder when input is empty
                IsSelectedTemplateTextBoxFocused = false;
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = SelectedTemplateTextBoxPlaceHolderText;
                }
            };
            selectedTemplateTextBox.IsVisibleChanged += (s, e9) => {
                //this is used to capture the current template text box when the paste toolbar is shown
                //to
                if (PasteTemplateToolbarVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                //tbx.SelectAll();
            };

            clearAllTemplatesButton.PreviewMouseLeftButtonUp += (s, e2) => {
                //when clear all is clicked it performs the ClearAllTemplate Command and this switches focus to 
                //first template tbx
                selectedTemplateTextBox.Focus();
                e2.Handled = false;
            };

            previousTemplateButton.MouseLeftButtonUp += (s, e1) => {
                if (IsTemplateReadyToPaste) {
                    //when navigating and all templates are filled set the fox to the 
                    //paste button which is enabled by IsTemplateReadyToPaste also
                    pasteTemplateButton.Focus();
                } else {
                    selectedTemplateTextBox.Focus();
                }
            };
            nextTemplateButton.MouseLeftButtonUp += (s, e1) => {
                if (IsTemplateReadyToPaste) {
                    //when navigating and all templates are filled set the fox to the 
                    //paste button which is enabled by IsTemplateReadyToPaste also
                    pasteTemplateButton.Focus();
                } else {
                    selectedTemplateTextBox.Focus();
                }
            };

            ttbb.IsVisibleChanged += (s, e1) => {
                double fromTopToolbar = 0;
                double toTopToolbar = 0;

                if (et.Visibility == Visibility.Visible) {
                    fromTopToolbar = cb.ActualHeight;
                    toTopToolbar = cb.ActualHeight - TileContentPasteToolbarHeight;
                } else {
                    fromTopToolbar = cb.ActualHeight - TileContentPasteToolbarHeight;
                    toTopToolbar = cb.ActualHeight;
                }

                MpHelpers.AnimateDoubleProperty(
                    fromTopToolbar,
                    toTopToolbar,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    ttbb,
                    Canvas.TopProperty,
                    (s1, e) => {

                    });
            };

            pttbg.IsVisibleChanged += (s, e1) => {
                double fromWidthTile = cb.ActualWidth;
                double toWidthTile = 0;

                if (ttbb.Visibility == Visibility.Visible) {
                    ClipTileViewModel.TemplateRichText = string.Empty; 
                    SelectedTemplateText = string.Empty;
                    toWidthTile = Math.Max(625, rtb.Document.GetFormattedText().WidthIncludingTrailingWhitespace);
                } else {
                    IsPastingTemplateTile = false;
                    toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;
                }

                MpHelpers.AnimateDoubleProperty(
                    fromWidthTile,
                    toWidthTile,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    new List<FrameworkElement> { cb, titleSwirl, rtb, ttbb, rtbc },
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
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        private RelayCommand _clearAllTemplatesCommand;
        public ICommand ClearAllTemplatesCommand {
            get {
                if (_clearAllTemplatesCommand == null) {
                    _clearAllTemplatesCommand = new RelayCommand(ClearAllTemplates, CanClearAllTemplates);
                }
                return _clearAllTemplatesCommand;
            }
        }
        private bool CanClearAllTemplates() {
            return ClearAllTemplateToolbarButtonVisibility == Visibility.Visible;
        }
        private void ClearAllTemplates() {
            foreach (var thlvm in this) {
                thlvm.TemplateText = string.Empty;
            }
            if(this.Count > 0) {
                SelectedTemplateHyperlinkViewModel = this[0];
            }
        }

        private RelayCommand _clearCurrentTemplatesCommand;
        public ICommand ClearCurrentTemplatesCommand {
            get {
                if (_clearCurrentTemplatesCommand == null) {
                    _clearCurrentTemplatesCommand = new RelayCommand(ClearCurrentTemplates, CanClearCurrentTemplates);
                }
                return _clearCurrentTemplatesCommand;
            }
        }
        private bool CanClearCurrentTemplates() {
            return ClearSelectedTemplateTextboxButtonVisibility == Visibility.Visible;
        }
        private void ClearCurrentTemplates() {
            IsSelectedTemplateTextBoxFocused = true;
            SelectedTemplateText = string.Empty;
        }

        private RelayCommand _nextTemplateTokenCommand;
        public ICommand NextTemplateTokenCommand {
            get {
                if (_nextTemplateTokenCommand == null) {
                    _nextTemplateTokenCommand = new RelayCommand(NextTemplateToken, CanNextTemplateToken);
                }
                return _nextTemplateTokenCommand;
            }
        }
        private bool CanNextTemplateToken() {
            return this.Count > 1;
        }
        private void NextTemplateToken() {
            int nextIdx = this.IndexOf(SelectedTemplateHyperlinkViewModel) + 1;
            if(nextIdx >= this.Count) {
                nextIdx = 0;
            }
            SelectedTemplateHyperlinkViewModel = this[nextIdx];            
        }

        private RelayCommand _previousTemplateTokenCommand;
        public ICommand PreviousTemplateTokenCommand {
            get {
                if (_previousTemplateTokenCommand == null) {
                    _previousTemplateTokenCommand = new RelayCommand(PreviousTemplateToken, CanPreviousTemplateToken);
                }
                return _previousTemplateTokenCommand;
            }
        }
        private bool CanPreviousTemplateToken() {
            return this.Count > 1;
        }
        private void PreviousTemplateToken() {
            int prevIdx = this.IndexOf(SelectedTemplateHyperlinkViewModel) - 1;
            if (prevIdx < 0) {
                prevIdx = this.Count - 1;
            }
            SelectedTemplateHyperlinkViewModel = this[prevIdx];
        }
        #endregion

        #region Overrides
        public new Hyperlink Add(MpTemplateHyperlinkViewModel thlvm, TextRange tr) {
            if(!this.Contains(thlvm)) {
                base.Add(thlvm);
            }
            var thlb = new MpTemplateHyperlinkBorder(thlvm);
            var container = new InlineUIContainer(thlb);
            var hl = new Hyperlink(tr.Start, tr.End);
            hl.Inlines.Clear();
            hl.Inlines.Add(container);

            return hl;
        }
        #endregion
    }
}
