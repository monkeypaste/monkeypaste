using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private TextBox _selectedTemplateTextBox = null;
        private ComboBox _selectedTemplateComboBox = null;

        private Grid _borderGrid = null;
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
        public MpObservableCollection<MpTemplateHyperlinkViewModel> UniqueTemplateHyperlinkViewModelListByDocOrder {
            get {
                if(ClipTileViewModel == null || 
                    ClipTileViewModel.RichTextBoxViewModelCollection == null ||
                    ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel == null ||
                    ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel == null ||
                    ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.Count == 0
                    ) {
                    return new MpObservableCollection<MpTemplateHyperlinkViewModel>();
                }
                return ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder;
            }
        }
        #endregion

        #region Properties

        public Visibility ClearAllTemplateToolbarButtonVisibility {
            get {
                foreach (var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                    if (!string.IsNullOrEmpty(uthlvm.TemplateText)) {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearSelectedTemplateTextboxButtonVisibility {
            get {
                if (//SelectedTemplateHyperlinkViewModel != null &&
                    SelectedTemplateText.Length > 0 &&
                    SelectedTemplateText != SelectedTemplateTextBoxPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility PasteTemplateNavigationButtonStackVisibility {
            get {
                if(ClipTileViewModel == null || ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel == null) {
                    return Visibility.Collapsed;
                }
                if (ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder.Count > 1) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private MpTemplateHyperlinkViewModel _selectedTemplate = null;
        public MpTemplateHyperlinkViewModel SelectedTemplate {
            get {
                return _selectedTemplate;
            }
            set {
                if (_selectedTemplate != value) 
                    {
                    _selectedTemplate = value;
                    if (SelectedTemplate != null) {
                        foreach (var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel) {
                            if (thlvm.TemplateName == SelectedTemplateName) {
                                thlvm.IsSelected = true;
                            } else {
                                thlvm.IsSelected = false;
                            }
                        }
                    }
                    OnPropertyChanged(nameof(SelectedTemplate));
                    OnPropertyChanged(nameof(SelectedTemplateName));
                    OnPropertyChanged(nameof(ClearSelectedTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxPlaceHolderText));
                    OnPropertyChanged(nameof(SelectedTemplateTextBrush));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxFontStyle));
                }
            }
        }

        public string SelectedTemplateName {
            get {
                if(SelectedTemplate == null) {
                    return string.Empty;
                }
                return SelectedTemplate.TemplateName;
            }
        }

        public bool IsTemplateReadyToPaste {
            get {
                foreach(var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                    if(string.IsNullOrEmpty(uthlvm.TemplateText)) {
                        return false;
                    }
                }
                return true;
            }
        }

        public string SelectedTemplateText {
            get {
                if(ClipTileViewModel == null || string.IsNullOrEmpty(SelectedTemplateName)) {
                    return string.Empty;
                }
                return SelectedTemplate.TemplateText;
            }
            set {
                if (ClipTileViewModel != null &&
                    !string.IsNullOrEmpty(SelectedTemplateName) &&
                    value != SelectedTemplateTextBoxPlaceHolderText &&
                    SelectedTemplate.TemplateText != value) {
                    ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.SetTemplateText(SelectedTemplateName,value);
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxPlaceHolderText));
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));
                    OnPropertyChanged(nameof(ClearSelectedTemplateTextboxButtonVisibility));
                }
            }
        }

        public Brush SelectedTemplateBrush {
            get {
                if(SelectedTemplate == null) {
                    return Brushes.Orange;
                }
                return SelectedTemplate.TemplateBrush;
            }
        }

        public string SelectedTemplateTextBoxPlaceHolderText {
            get {
                if (ClipTileViewModel == null || SelectedTemplate == null) {
                    return string.Empty;
                }
                return SelectedTemplate.TemplateDisplayName + "...";
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

        #region Public Methods
        public MpPasteTemplateToolbarViewModel(MpClipTileViewModel ctvm) : base() { 
            ClipTileViewModel = ctvm;
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedTemplate):
                        if (SelectedTemplate == null && UniqueTemplateHyperlinkViewModelListByDocOrder != null && UniqueTemplateHyperlinkViewModelListByDocOrder.Count > 0) {
                            //SelectedTemplate = UniqueTemplateHyperlinkViewModelListByDocOrder[0];
                            SetTemplate(UniqueTemplateHyperlinkViewModelListByDocOrder[0].TemplateName);
                            return;
                        } else if (SelectedTemplate == null) {
                            return;
                        }
                        var templateRect = SelectedTemplate.TemplateHyperlinkRange.Start.GetCharacterRect(LogicalDirection.Forward);
                        double y = templateRect.Y - (templateRect.Height / 2);
                        ClipTileViewModel.RichTextBoxViewModelCollection.SelectedRtb.ScrollToVerticalOffset(y);
                        break;
                }
            };
        }

        public void ClipTilePasteTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            if (ClipTileViewModel.CopyItemType != MpCopyItemType.RichText && ClipTileViewModel.CopyItemType != MpCopyItemType.Composite) {
                return;
            }
            _borderGrid = (Grid)sender;           
        }

        public void InitWithRichTextBox(RichTextBox rtb, bool doAnimation) {
            foreach (var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel) {
                thlvm.PropertyChanged += (s, e) => {
                    switch (e.PropertyName) {
                        case nameof(thlvm.IsSelected):
                            if (thlvm.IsSelected && SelectedTemplate != thlvm) {
                                SetTemplate(thlvm.TemplateName);
                            }
                            break;
                    }
                };
            }

            var pasteTemplateToolbarBorder = _borderGrid.GetVisualAncestor<Border>();
            var cb = (MpClipBorder)pasteTemplateToolbarBorder.GetVisualAncestor<MpClipBorder>();
            var editRichTextToolbarBorder = (Border)cb.FindName("ClipTileEditorToolbar");
            var editTemplateToolbarBorder = (Border)cb.FindName("ClipTileEditTemplateToolbar");
            var clipTray = MainWindowViewModel.ClipTrayViewModel.ClipTrayListView;
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxListBoxGridContainerCanvas");
            var rtblb = (ListBox)cb.FindName("ClipTileRichTextBoxListBox");
            var ctttg = (Grid)cb.FindName("ClipTileTitleTextGrid");
            //var rtb = rtbc.FindName("ClipTileRichTextBox") as RichTextBox;
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var clearAllTemplatesButton = (Button)pasteTemplateToolbarBorder.FindName("ClearAllTemplatesButton");
            var selectedTemplateTextBox = (TextBox)pasteTemplateToolbarBorder.FindName("SelectedTemplateTextBox");
            var previousTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("PreviousTemplateButton");
            var nextTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("NextTemplateButton");
            var pasteTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("PasteTemplateButton");
            var selectedTemplateComboBox = (ComboBox)pasteTemplateToolbarBorder.FindName("SelectedTemplateComboBox");
            var ds = ClipTileViewModel.RichTextBoxViewModelCollection.FullDocument.GetDocumentSize();

            _selectedTemplateComboBox = selectedTemplateComboBox;
            _selectedTemplateTextBox = selectedTemplateTextBox;

            selectedTemplateTextBox.GotFocus += (s, e4) => {
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = string.Empty;
                }

                //IsSelectedTemplateTextBoxFocused = true;
            };
            selectedTemplateTextBox.LostFocus += (s, e5) => {
                //ensures current template textbox show the template name as a placeholder when input is empty
                //IsSelectedTemplateTextBoxFocused = false;
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

            clearAllTemplatesButton.MouseLeftButtonUp += (s, e2) => {
                //when clear all is clicked it performs the ClearAllTemplate Command and this switches focus to 
                //first template tbx
                selectedTemplateTextBox.Focus();
                //e2.Handled = false;
            };

            if(doAnimation) {
                double animMs = 0;
                double tileWidthMax = Math.Max(MpMeasurements.Instance.ClipTileEditModeMinWidth, ds.Width);
                double tileWidthMin = ClipTileViewModel.TileBorderWidth;

                double contentWidthMax = tileWidthMax - MpMeasurements.Instance.ClipTileEditModeContentMargin;
                double contentWidthMin = ClipTileViewModel.TileContentWidth;

                double rtbBottomMax = ClipTileViewModel.TileContentHeight;
                double rtbBottomMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.PasteTemplateToolbarHeight;

                double pasteTemplateToolbarTopMax = ClipTileViewModel.TileContentHeight;
                double pasteTemplateToolbarTopMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.PasteTemplateToolbarHeight + 5;

                double iconLeftMax = tileWidthMax - 125;// tileWidthMax - ClipTileViewModel.TileTitleIconSize;
                double iconLeftMin = 204;// tileWidthMin - ClipTileViewModel.TileTitleIconSize;

                if (ClipTileViewModel.IsPastingTemplateTile) {
                    OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelListByDocOrder));
                    OnPropertyChanged(nameof(PasteTemplateNavigationButtonStackVisibility));
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));

                    ClipTileViewModel.PasteTemplateToolbarVisibility = Visibility.Visible;
                    if (UniqueTemplateHyperlinkViewModelListByDocOrder.Count > 0) {
                        //selectedTemplateComboBox.SelectedItem = UniqueTemplateHyperlinkViewModelListByDocOrder[0];
                        SetTemplate(UniqueTemplateHyperlinkViewModelListByDocOrder[0].TemplateName);
                    }
                } else {
                    ClearAllTemplates();
                    //SelectedTemplate = UniqueTemplateHyperlinkViewModelListByDocOrder[0];
                    //SetTemplate(UniqueTemplateHyperlinkViewModelListByDocOrder[0].TemplateName);
                    ClipTileViewModel.PasteTemplateToolbarVisibility = Visibility.Collapsed;
                    clipTray.ScrollViewer.ScrollToHome();
                }

                MpHelpers.Instance.AnimateDoubleProperty(
                    ClipTileViewModel.IsPastingTemplateTile ? pasteTemplateToolbarTopMax : pasteTemplateToolbarTopMin,
                    ClipTileViewModel.IsPastingTemplateTile ? pasteTemplateToolbarTopMin : pasteTemplateToolbarTopMax,
                    animMs,
                    pasteTemplateToolbarBorder,
                    Canvas.TopProperty,
                    (s1, e44) => {
                        if (!ClipTileViewModel.IsPastingTemplateTile) {
                            ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Collapsed;
                        } else {
                            selectedTemplateTextBox.PreviewKeyDown += (s6, e8) => {
                                if (e8.Key == Key.Enter) {
                                    if (IsTemplateReadyToPaste) {
                                        PasteTemplateCommand.Execute(null);
                                    } else {
                                        NextTemplateTokenCommand.Execute(null);
                                    }
                                }
                            };
                            selectedTemplateTextBox.Focus();

                        }
                    });

                MpHelpers.Instance.AnimateDoubleProperty(
                    ClipTileViewModel.IsPastingTemplateTile ? tileWidthMin : tileWidthMax,
                    ClipTileViewModel.IsPastingTemplateTile ? tileWidthMax : tileWidthMin,
                    animMs,
                    new List<FrameworkElement> { cb, titleSwirl },
                    FrameworkElement.WidthProperty,
                    (s1, e44) => {
                    });

                MpHelpers.Instance.AnimateDoubleProperty(
                    ClipTileViewModel.IsPastingTemplateTile ? contentWidthMin : contentWidthMax,
                    ClipTileViewModel.IsPastingTemplateTile ? contentWidthMax : contentWidthMin,
                    animMs,
                    new List<FrameworkElement> {rtblb, editRichTextToolbarBorder, rtbc, editTemplateToolbarBorder, pasteTemplateToolbarBorder },
                    FrameworkElement.WidthProperty,
                    (s1, e44) => {
                        clipTray.ScrollIntoView(ClipTileViewModel);
                        //this is to remove scrollbar flicker during animation
                        //ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbHorizontalScrollbarVisibility));
                        //ClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(ClipTileViewModel.RichTextBoxViewModelCollection.RtbVerticalScrollbarVisibility));
                    });

                ClipTileViewModel.RichTextBoxViewModelCollection.AnimateItems(
                    ClipTileViewModel.IsPastingTemplateTile ? contentWidthMin : contentWidthMax,
                    ClipTileViewModel.IsPastingTemplateTile ? contentWidthMax : contentWidthMin,
                    0, 0,
                    0, 0,
                    0, 0,
                    animMs
                );

                MpHelpers.Instance.AnimateDoubleProperty(
                    ClipTileViewModel.IsPastingTemplateTile ? iconLeftMin : iconLeftMax,
                    ClipTileViewModel.IsPastingTemplateTile ? iconLeftMax : iconLeftMin,
                    animMs,
                    titleIconImageButton,
                    Canvas.LeftProperty,
                    (s1, e23) => {

                    });
            }
        }
        
        public void SetTemplate(string templateName) {
            if(string.IsNullOrEmpty(templateName)) {
                return;
            }
            foreach (var t in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                if (t.TemplateName == templateName) {
                    SelectedTemplate = t;
                    SelectedTemplate.IsSelected = true;
                } else if(SelectedTemplate != null) {
                    SelectedTemplate.IsSelected = false;
                }
            }
            ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.SelectTemplate(templateName);
            SelectedTemplate = ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel;
            OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelListByDocOrder));
        }

        public TextBox GetSelectedTemplateTextBox() {
            return _selectedTemplateTextBox;
        }

        public ComboBox GetSelectedTemplateComboBox() {
            return _selectedTemplateComboBox;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        private RelayCommand _clearAllTemplatesCommand;
        public ICommand ClearAllTemplatesCommand {
            get {
                if (_clearAllTemplatesCommand == null) {
                    _clearAllTemplatesCommand = new RelayCommand(ClearAllTemplates);
                }
                return _clearAllTemplatesCommand;
            }
        }
        private bool CanClearAllTemplates() {
            return ClipTileViewModel != null &&
                ClipTileViewModel.IsPastingTemplateTile &&
                ClearAllTemplateToolbarButtonVisibility == Visibility.Visible;
        }
        private void ClearAllTemplates() {
            foreach (var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.SetTemplateText(uthlvm.TemplateName, string.Empty);
            }

            if(UniqueTemplateHyperlinkViewModelListByDocOrder.Count > 0) {
                SelectedTemplate = UniqueTemplateHyperlinkViewModelListByDocOrder[0];
            } else {
                SelectedTemplate = null;
            }

            foreach(var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel) {
                thlvm.IsSelected = false;
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
            return ClipTileViewModel != null && 
                ClipTileViewModel.IsPastingTemplateTile && 
                ClearSelectedTemplateTextboxButtonVisibility == Visibility.Visible;
        }
        private void ClearCurrentTemplates() {
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
            return ClipTileViewModel != null && 
                ClipTileViewModel.IsPastingTemplateTile && 
                UniqueTemplateHyperlinkViewModelListByDocOrder.Count > 1;
        }
        private void NextTemplateToken() {
            int nextIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SelectedTemplate) + 1;
            if(nextIdx >= UniqueTemplateHyperlinkViewModelListByDocOrder.Count) {
                nextIdx = 0;
            }
            SelectedTemplate = UniqueTemplateHyperlinkViewModelListByDocOrder[nextIdx];
            GetSelectedTemplateTextBox().Focus();
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
            return ClipTileViewModel != null && 
                ClipTileViewModel.IsPastingTemplateTile && 
                UniqueTemplateHyperlinkViewModelListByDocOrder.Count > 1;
        }
        private void PreviousTemplateToken() {
            int prevIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SelectedTemplate) - 1;
            if (prevIdx < 0) {
                prevIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.Count - 1;
            }
            SelectedTemplate = UniqueTemplateHyperlinkViewModelListByDocOrder[prevIdx];

            GetSelectedTemplateTextBox().Focus();
        }

        private RelayCommand _pasteTemplateCommand;
        public ICommand PasteTemplateCommand {
            get {
                if (_pasteTemplateCommand == null) {
                    _pasteTemplateCommand = new RelayCommand(PasteTemplate, CanPasteTemplate);
                }
                return _pasteTemplateCommand;
            }
        }
        private bool CanPasteTemplate() {
            //only allow template to be pasted once all template types have been viewed
            return ClipTileViewModel != null && 
                ClipTileViewModel.IsPastingTemplateTile && 
                IsTemplateReadyToPaste;
        }
        private void PasteTemplate() {
            //to paste w/ templated text a clone of the templates is created then 
            //the links are cleared (returning the edited text to the template names
            //
            var uthlvmlc = new List<MpTemplateHyperlinkViewModel>();
            foreach (var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                uthlvmlc.Add((MpTemplateHyperlinkViewModel)uthlvm.Clone());
            }
            //ClipTileViewModel.TileVisibility = Visibility.Hidden;
            var srtbvm = ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel;
            srtbvm.ClearHyperlinks();
            var docClone = srtbvm.Rtb.Document.Clone();

            foreach (var uthlvm in uthlvmlc) {
                var matchList = MpHelpers.Instance.FindStringRangesFromPosition(
                    docClone.ContentStart,
                    uthlvm.TemplateName,
                    true);
                foreach (var tr in matchList) {
                    tr.Text = uthlvm.TemplateText;
                }
            }
            srtbvm.CreateHyperlinks();
            srtbvm.TemplateRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(docClone);
            //Returned to GetPastableRichText
        }
        #endregion
    }
}
