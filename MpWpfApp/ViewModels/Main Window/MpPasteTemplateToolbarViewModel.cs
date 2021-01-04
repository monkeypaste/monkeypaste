using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpPasteTemplateToolbarViewModel : MpObservableCollectionViewModel<MpTemplateTokenCollectionViewModel> {
        #region Private Variables

        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel = new MpClipTileViewModel();
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    ClipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }

        public MpTemplateTokenCollectionViewModel SelectedTemplateTokenCollectionViewModel {
            get {
                foreach (var ttcvm in this) {
                    if (ttcvm.IsSelected) {
                        return ttcvm;
                    }
                }
                return null;
            }
            set {
                if (SelectedTemplateTokenCollectionViewModel != value) {
                    foreach (var ttcvm in this) {
                        //clear any other selections
                        ttcvm.IsSelected = false;
                    }
                    this[this.IndexOf(value)].IsSelected = true;
                    OnPropertyChanged(nameof(SelectedTemplateTokenCollectionViewModel));
                }
            }
        }
        #endregion

        #region Properties
        #region Layout Properties
        public double TileContentPasteToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditToolbarHeight;
            }
        }
        #endregion

        #region Visibility Properties
        public Visibility PasteTemplateToolbarVisibility {
            get {
                if (ClipTileViewModel.IsPastingTemplateTile) {
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
                if (SelectedTemplateText.Length > 0 &&
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

        private bool _isPastingTemplateTile = false;
        public bool IsPastingTemplateTile {
            get {
                return _isPastingTemplateTile;
            }
            set {
                if (_isPastingTemplateTile != value) {
                    _isPastingTemplateTile = value;
                    OnPropertyChanged(nameof(IsPastingTemplateTile));
                }
            }
        }

        private bool _isTemplateReadyToPaste = false;
        public bool IsTemplateReadyToPaste {
            get {
                return _isTemplateReadyToPaste;
            }
            set {
                if (_isTemplateReadyToPaste != value) {
                    _isTemplateReadyToPaste = value;
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
                }
            }
        }

        public string SelectedTemplateText {
            get {
                return SelectedTemplateTokenCollectionViewModel.TemplateText;
            }
            set {
                if (SelectedTemplateTokenCollectionViewModel.TemplateText != value) {
                    SelectedTemplateTokenCollectionViewModel.TemplateText = value;
                    OnPropertyChanged(nameof(SelectedTemplateText));
                }
            }
        }

        public string SelectedTemplateTextBoxPlaceHolderText {
            get {
                return SelectedTemplateTokenCollectionViewModel.TemplateName + "...";
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
        public MpPasteTemplateToolbarViewModel() : this(new MpClipTileViewModel()) { }

        public MpPasteTemplateToolbarViewModel(MpClipTileViewModel ctvm) {
            ClipTileViewModel = ctvm;
            foreach(var cit in ClipTileViewModel.CopyItem.TemplateList) {
                this.Add(new MpTemplateTokenCollectionViewModel(this, cit));
            }
        }
        public void ClipTilePasteTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var pt = (Border)sender;
            var rtb = ClipTileViewModel.GetRtb();
            var cb = (MpClipBorder)pt.GetVisualAncestor<MpClipBorder>();
            var et = (Border)cb.FindName("ClipTileEditorToolbar");
            var sp = (StackPanel)cb.FindName("ClipTileRichTextStackPanel");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var addTemplateButton = (Button)et.FindName("AddTemplateButton");
            var catb = (Button)pt.FindName("ClearAllTemplatesButton");
            var cttb = (TextBox)pt.FindName("SelectedTemplateTextBox");
            var prvtb = (Button)pt.FindName("PreviousTemplateButton");
            var ntb = (Button)pt.FindName("NextTemplateButton");
            var ptb = (Button)pt.FindName("PasteTemplateButton");

            addTemplateButton.PreviewMouseDown += (s, e3) => {                
                if (this.Count == 0) {
                    //if templates are NOT in the clip yet add one w/ default name
                    MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(this, null, true);
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

                        DockPanel dp = new DockPanel();
                        dp.Children.Add(rect);
                        dp.Children.Add(tb);
                        rect.SetValue(DockPanel.DockProperty, Dock.Left);
                        tb.SetValue(DockPanel.DockProperty, Dock.Right);

                        MenuItem tmi = new MenuItem();
                        tmi.Header = dp;
                        tmi.Click += (s1, e5) => {
                            MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(this, ttcvm, false);
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
                        MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(this, null, true);
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

            cttb.GotFocus += (s, e4) => {
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = string.Empty;
                }

                IsSelectedTemplateTextBoxFocused = true;
            };
            cttb.LostFocus += (s, e5) => {
                //ensures current template textbox show the template name as a placeholder when input is empty
                IsSelectedTemplateTextBoxFocused = false;
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = SelectedTemplateTextBoxPlaceHolderText;
                }
            };
            cttb.IsVisibleChanged += (s, e9) => {
                //this is used to capture the current template text box when the paste toolbar is shown
                //to
                if (PasteTemplateToolbarVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                //tbx.SelectAll();
            };

            catb.PreviewMouseLeftButtonUp += (s, e2) => {
                //when clear all is clicked it performs the ClearAllTemplate Command and this switches focus to 
                //first template tbx
                cttb.Focus();
                e2.Handled = false;
            };

            prvtb.MouseLeftButtonUp += (s, e1) => {
                if (IsTemplateReadyToPaste) {
                    //when navigating and all templates are filled set the fox to the 
                    //paste button which is enabled by IsTemplateReadyToPaste also
                    ptb.Focus();
                } else {
                    cttb.Focus();
                }
            };
            ntb.MouseLeftButtonUp += (s, e1) => {
                if (IsTemplateReadyToPaste) {
                    //when navigating and all templates are filled set the fox to the 
                    //paste button which is enabled by IsTemplateReadyToPaste also
                    ptb.Focus();
                } else {
                    cttb.Focus();
                }
            };

            pt.IsVisibleChanged += (s, e1) => {
                if (pt.Visibility == Visibility.Visible) {
                    //this only occurs when templates exist in the clip and here they're dynamically gathered
                    //and the asycn TemplateRichText is cleared so GetPastableRichText awaits its setting
                    ClipTileViewModel.TemplateRichText = string.Empty;
                    //HasTemplate = rtb.Tag != null && ((List<Hyperlink>)rtb.Tag).Count > 0;
                    //TemplateTokenLookupDictionary = new Dictionary<string, string>();
                    //var templatePattern = string.Format(
                    //    @"[{0}].*?[{0}].*?[{0}]",
                    //    Properties.Settings.Default.TemplateTokenMarker);
                    //MatchCollection mc = Regex.Matches(
                    //    CopyItemPlainText,
                    //    templatePattern,
                    //    RegexOptions.IgnoreCase |
                    //    RegexOptions.Compiled |
                    //    RegexOptions.ExplicitCapture |
                    //    RegexOptions.Multiline);
                    //foreach (Match m in mc) {
                    //    foreach (Group mg in m.Groups) {
                    //        foreach (Capture c in mg.Captures) {
                    //            var templateName = c.Value.Split(new string[] { Properties.Settings.Default.TemplateTokenMarker }, StringSplitOptions.RemoveEmptyEntries)[0];
                    //            if (TemplateTokenLookupDictionary.ContainsKey(templateName)) {
                    //                continue;
                    //            } else {
                    //                TemplateTokenLookupDictionary.Add(templateName, string.Empty);
                    //            }
                    //        }
                    //    }
                    //}
                    //OnPropertyChanged(nameof(TemplateNavigationButtonStackVisibility));

                    //TemplateTokens.Clear();
                    //foreach (var thl in rtb.GetTemplateHyperlinkList()) {
                    //    TemplateTokens.Add((MpTemplateHyperlinkViewModel)thl.DataContext);
                    //}
                    SelectedTemplateText = string.Empty;
                    //cttb.Focus();
                    //IsSelectedTemplateTextBoxFocused = true;
                } else {
                    IsPastingTemplateTile = false;
                }
            };
        }
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
                SelectedTemplateTokenCollectionViewModel = this[0];
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
            int nextIdx = this.IndexOf(SelectedTemplateTokenCollectionViewModel) + 1;
            if(nextIdx >= this.Count) {
                nextIdx = 0;
            }
            SelectedTemplateTokenCollectionViewModel = this[nextIdx];            
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
            int prevIdx = this.IndexOf(SelectedTemplateTokenCollectionViewModel) - 1;
            if (prevIdx < 0) {
                prevIdx = this.Count - 1;
            }
            SelectedTemplateTokenCollectionViewModel = this[prevIdx];
        }
        #endregion
    }
}
