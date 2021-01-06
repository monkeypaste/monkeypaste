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
    public class MpPasteTemplateToolbarViewModel : MpViewModelBase {
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
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));
                    OnPropertyChanged(nameof(ClearSelectedTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(PasteTemplateNavigationButtonStackVisibility));
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxPlaceHolderText));
                    OnPropertyChanged(nameof(SelectedTemplateTextBrush));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxFontStyle));
                }
            }
        }
        #endregion

        #region Properties
        #region Layout Properties
        
        #endregion

        #region Visibility Properties
        
        public Visibility ClearAllTemplateToolbarButtonVisibility {
            get {
                foreach (var templateLookup in ClipTileViewModel.TemplateHyperlinkCollectionViewModel) {
                    if (!string.IsNullOrEmpty(templateLookup.TemplateText)) {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearSelectedTemplateTextboxButtonVisibility {
            get {
                if (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel != null &&
                    SelectedTemplateText.Length > 0 &&
                    SelectedTemplateText != SelectedTemplateTextBoxPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility PasteTemplateNavigationButtonStackVisibility {
            get {
                if (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count > 1) {
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

        public bool IsTemplateReadyToPaste {
            get {
                foreach(var thlvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel) {
                    if(string.IsNullOrEmpty(thlvm.TemplateText)) {
                        return false;
                    }
                }
                return true;
            }
        }

        public string SelectedTemplateText {
            get {
                if(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel == null) {
                    return string.Empty;
                }
                return ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel.TemplateText;
            }
            set {
                if (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel != null &&
                    ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel.TemplateText != value) {
                    ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel.TemplateText = value;
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
                }
            }
        }

        public string SelectedTemplateTextBoxPlaceHolderText {
            get {
                if (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel == null) {
                    return string.Empty;
                }
                return ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel.TemplateName + "...";
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
        public MpPasteTemplateToolbarViewModel(MpClipTileViewModel ctvm) : base() { 
            ClipTileViewModel = ctvm;
        }

        public void ClipTilePasteTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var pasteTemplateToolbarBorderGrid = (Grid)sender;
            var pasteTemplateToolbarBorder = pasteTemplateToolbarBorderGrid.GetVisualAncestor<Border>();
            var rtb = ClipTileViewModel.GetRtb();
            var cb = (MpClipBorder)pasteTemplateToolbarBorder.GetVisualAncestor<MpClipBorder>();
            var et = (Border)cb.FindName("ClipTileEditorToolbar");
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var clearAllTemplatesButton = (Button)pasteTemplateToolbarBorder.FindName("ClearAllTemplatesButton");
            var selectedTemplateTextBox = (TextBox)pasteTemplateToolbarBorder.FindName("SelectedTemplateTextBox");
            var previousTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("PreviousTemplateButton");
            var nextTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("NextTemplateButton");
            var pasteTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("PasteTemplateButton");

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
                if (ClipTileViewModel.PasteTemplateToolbarVisibility == Visibility.Collapsed) {
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

            pasteTemplateToolbarBorder.IsVisibleChanged += (s, e1) => {
                double fromTopToolbar = 0;
                double toTopToolbar = 0;

                double fromWidthTile = cb.ActualWidth;
                double toWidthTile = 0;

                double fromBottomRtb = ClipTileViewModel.RtbBottom;
                double toBottomRtb = 0;
                if (et.Visibility == Visibility.Visible) {
                    fromTopToolbar = cb.ActualHeight;
                    toTopToolbar = cb.ActualHeight - ClipTileViewModel.PasteTemplateToolbarHeight;

                    toWidthTile = Math.Max(625, rtb.Document.GetFormattedText().WidthIncludingTrailingWhitespace);

                    toBottomRtb = ClipTileViewModel.RtbBottom - ClipTileViewModel.PasteTemplateToolbarHeight;

                    ClipTileViewModel.TemplateRichText = string.Empty;
                    SelectedTemplateText = string.Empty;
                } else {
                    fromTopToolbar = cb.ActualHeight - ClipTileViewModel.PasteTemplateToolbarHeight;
                    toTopToolbar = cb.ActualHeight;

                    toWidthTile = MpMeasurements.Instance.ClipTileBorderSize;

                    toBottomRtb = ClipTileViewModel.RtbBottom + ClipTileViewModel.PasteTemplateToolbarHeight;

                    ClipTileViewModel.IsPastingTemplateTile = false;
                }

                MpHelpers.AnimateDoubleProperty(
                    fromTopToolbar,
                    toTopToolbar,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    pasteTemplateToolbarBorder,
                    Canvas.TopProperty,
                    (s1, e) => {

                    });

                MpHelpers.AnimateDoubleProperty(
                    fromBottomRtb,
                    toBottomRtb,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    rtb,
                    Canvas.BottomProperty,
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
            foreach (var thlvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel) {
                thlvm.TemplateText = string.Empty;
            }
            if(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count > 0) {
                ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel = ClipTileViewModel.TemplateHyperlinkCollectionViewModel[0];
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
            return ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count > 1;
        }
        private void NextTemplateToken() {
            int nextIdx = ClipTileViewModel.TemplateHyperlinkCollectionViewModel.IndexOf(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel) + 1;
            if(nextIdx >= ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count) {
                nextIdx = 0;
            }
            ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel = ClipTileViewModel.TemplateHyperlinkCollectionViewModel[nextIdx];            
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
            return ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count > 1;
        }
        private void PreviousTemplateToken() {
            int prevIdx = ClipTileViewModel.TemplateHyperlinkCollectionViewModel.IndexOf(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel) - 1;
            if (prevIdx < 0) {
                prevIdx = ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count - 1;
            }
            ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel = ClipTileViewModel.TemplateHyperlinkCollectionViewModel[prevIdx];
        }
        #endregion
    }
}
