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
using System.Windows.Threading;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpPasteTemplateToolbarViewModel : MpUndoableViewModelBase<MpPasteTemplateToolbarViewModel>, IDisposable {
        #region Private Variables
        private Grid _borderGrid = null;
        #endregion        

        #region Properties

        #region View Models
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if(RtbItemCollectionViewModel == null) {
                    return null;
                }

                return RtbItemCollectionViewModel.HostClipTileViewModel;
            }
        }

        private MpRtbItemCollectionViewModel _rtbItemCollectionViewModel;
        public MpRtbItemCollectionViewModel RtbItemCollectionViewModel {
            get {
                return _rtbItemCollectionViewModel;
            }
            private set {
                if(_rtbItemCollectionViewModel != value) {
                    _rtbItemCollectionViewModel = value;
                    OnPropertyChanged(nameof(RtbItemCollectionViewModel));
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));
                    OnPropertyChanged(nameof(ClearSelectedTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(PasteTemplateNavigationButtonStackVisibility));
                    OnPropertyChanged(nameof(HaveAllSubItemTemplatesBeenVisited));
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxPlaceHolderText));
                    OnPropertyChanged(nameof(SelectedTemplateTextBrush));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxFontStyle));
                }
            }
        }

        public MpObservableCollection<MpTemplateHyperlinkViewModel> UniqueTemplateHyperlinkViewModelListByDocOrder {
            get {
                if (SubSelectedRtbViewModel == null) {
                    return null;
                }
                return null;// SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder;
            }
        }

        private MpRtbItemViewModel _subSelectedRtbViewModel = null;
        public MpRtbItemViewModel SubSelectedRtbViewModel {
            get {
                return _subSelectedRtbViewModel;
            }
            set {
                if(_subSelectedRtbViewModel != value) {
                    _subSelectedRtbViewModel = value;
                    OnPropertyChanged(nameof(SubSelectedRtbViewModel));
                    OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelListByDocOrder));
                }
            }
        }
        #endregion

        #region Controls
        public TextBox SelectedTemplateTextBox { get; set; }

        public Button NextTemplateButton { get; set; }

        public Button PreviousTemplateButton { get; set; }

        public Button PasteTemplateButton { get; set; }
        #endregion

        #region Layout
        private double _pasteTemplateBorderCanvasTop = MpMeasurements.Instance.ClipTileContentHeight;
        public double PasteTemplateBorderCanvasTop {
            get {
                return _pasteTemplateBorderCanvasTop;
            }
            set {
                if (_pasteTemplateBorderCanvasTop != value) {
                    _pasteTemplateBorderCanvasTop = value;
                    OnPropertyChanged(nameof(PasteTemplateBorderCanvasTop));
                }
            }
        }
        #endregion

        #region Appearance
        public string PasteButtonText {
            get {
                if(HostClipTileViewModel == null || SubSelectedRtbViewModel == null) {
                    return string.Empty;
                }
                if(HostClipTileViewModel.ContentContainerViewModel.SubSelectedContentItems.Where(x=>x.IsDynamicPaste).ToList().Count > 1) {
                    foreach (MpRtbItemViewModel rtbvm in HostClipTileViewModel.ContentContainerViewModel.SubSelectedContentItems) {
                        if (rtbvm.IsDynamicPaste && rtbvm.TemplateHyperlinkCollectionViewModel.Templates.Any(x => string.IsNullOrEmpty(x.TemplateText))) {
                            return @"CONTINUE";
                        }
                    }
                }
                
                return @"PASTE";
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

        public Brush SelectedTemplateTextBoxBorderBrush {
            get {
                if (SelectedTemplateTextBox == null || !SelectedTemplateTextBox.IsFocused) {
                    return Brushes.DimGray;
                }
                return Brushes.Red;
            }
        }

        public Brush NextButtonBorderBrush {
            get {
                if (NextTemplateButton == null || !NextTemplateButton.IsFocused) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }

        public Brush PreviousButtonBorderBrush {
            get {
                if (PreviousTemplateButton == null || !PreviousTemplateButton.IsFocused) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }

        public Brush PasteButtonBorderBrush {
            get {
                if (PasteTemplateButton == null || !PasteTemplateButton.IsFocused) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
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

        public Brush SelectedTemplateBrush {
            get {
                if (SelectedTemplate == null) {
                    return Brushes.Orange;
                }
                return SelectedTemplate.TemplateBrush;
            }
        }
        #endregion

        #region Visibility
        private Visibility _pasteTemplateToolbarVisibility = Visibility.Collapsed;
        public Visibility PasteTemplateToolbarVisibility {
            get {
                return _pasteTemplateToolbarVisibility;
            }
            set {
                if (_pasteTemplateToolbarVisibility != value) {
                    _pasteTemplateToolbarVisibility = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarVisibility));
                }
            }
        }

        public Visibility PasteButtonVisibility {
            get {
                if(HostClipTileViewModel == null || SubSelectedRtbViewModel == null) {
                    return Visibility.Hidden;
                }
                return DoAllSubItemTemplatesHaveText ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility ClearAllTemplateToolbarButtonVisibility {
            get {
                if(UniqueTemplateHyperlinkViewModelListByDocOrder == null) {
                    return Visibility.Collapsed;
                }
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
                if (SelectedTemplateText.Length > 0 &&
                    SelectedTemplateText != SelectedTemplateTextBoxPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility PasteTemplateNavigationButtonStackVisibility {
            get {
                if(SubSelectedRtbViewModel == null) {
                    return Visibility.Collapsed;
                }
                if (SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.Templates.Count > 1) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region State

        private bool _hasTextChanged = false;
        public bool HasTextChanged {
            get {
                return _hasTextChanged;
            }
            set {
                if (_hasTextChanged != value) {
                    _hasTextChanged = value;
                    OnPropertyChanged(nameof(HasTextChanged));
                }
            }
        }

        public bool HaveAllSubItemTemplatesBeenVisited {
            get {
                if (SubSelectedRtbViewModel == null) {
                    return false;
                }
                return SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.Templates.All(x => x.WasVisited);
            }
        }

        public bool DoAllSubItemTemplatesHaveText {
            get {
                if (SubSelectedRtbViewModel == null) {
                    return false;
                }
                return SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.Templates.All(x => !string.IsNullOrEmpty(x.TemplateText));
            }
        }
        #endregion

        #region Selection
        private int _selectedTemplateIdx = 0;
        public int SelectedTemplateIdx {
            get {
                return _selectedTemplateIdx;
            }
            set {
                if (_selectedTemplateIdx != value) {
                    _selectedTemplateIdx = value;
                    OnPropertyChanged(nameof(SelectedTemplateIdx));
                }
            }
        }

        public MpTemplateHyperlinkViewModel SelectedTemplate {
            get {
                if (UniqueTemplateHyperlinkViewModelListByDocOrder == null ||
                    UniqueTemplateHyperlinkViewModelListByDocOrder.All(x => x.IsSelected == false)) {
                    return null;
                }
                return UniqueTemplateHyperlinkViewModelListByDocOrder.Where(x => x.IsSelected).ToList()[0];
            }
        }

        public string SelectedTemplateName {
            get {
                if (SelectedTemplate == null) {
                    return string.Empty;
                }
                return SelectedTemplate.TemplateName;
            }
        }

        public string SelectedTemplateText {
            get {
                if (HostClipTileViewModel == null || string.IsNullOrEmpty(SelectedTemplateName)) {
                    return string.Empty;
                }
                return SelectedTemplate.TemplateText;
            }
            set {
                if (HostClipTileViewModel != null &&
                    !string.IsNullOrEmpty(SelectedTemplateName) &&
                    value != SelectedTemplateTextBoxPlaceHolderText &&
                    SelectedTemplate.TemplateText != value) {
                    SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SetTemplateText(SelectedTemplateName, value);
                    OnPropertyChanged(nameof(SelectedTemplateText));
                    OnPropertyChanged(nameof(HaveAllSubItemTemplatesBeenVisited));
                    OnPropertyChanged(nameof(SelectedTemplateTextBoxPlaceHolderText));
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));
                    OnPropertyChanged(nameof(ClearSelectedTemplateTextboxButtonVisibility));
                }
            }
        }

        public string SelectedTemplateTextBoxPlaceHolderText {
            get {
                if (HostClipTileViewModel == null || SelectedTemplate == null) {
                    return string.Empty;
                }
                return SelectedTemplate.TemplateDisplayName + "...";
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpPasteTemplateToolbarViewModel() : base() {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedTemplateIdx):
                        if (SubSelectedRtbViewModel == null || SelectedTemplateIdx < 0) {
                            break;
                        }
                        //SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectTemplate(UniqueTemplateHyperlinkViewModelListByDocOrder[SelectedTemplateIdx].TemplateName);
                        OnPropertyChanged(nameof(SelectedTemplate));
                        break;
                    case nameof(SelectedTemplate):
                        OnPropertyChanged(nameof(SelectedTemplateName));
                        OnPropertyChanged(nameof(ClearSelectedTemplateTextboxButtonVisibility));
                        OnPropertyChanged(nameof(SelectedTemplateText));
                        OnPropertyChanged(nameof(SelectedTemplateTextBoxPlaceHolderText));
                        OnPropertyChanged(nameof(SelectedTemplateTextBrush));
                        OnPropertyChanged(nameof(SelectedTemplateTextBoxFontStyle));

                        break;
                }
            };
        }

        public MpPasteTemplateToolbarViewModel(MpRtbItemCollectionViewModel rtbicvm) : this() {            
            RtbItemCollectionViewModel = rtbicvm;
        }

        public void ClipTilePasteTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            if (HostClipTileViewModel.CopyItemType != MpCopyItemType.RichText) {
                return;
            }
            _borderGrid = (Grid)sender;           
        }       

        public void Resize(double deltaTemplateTop) {
            if (HostClipTileViewModel.IsPastingTemplate) {
                PasteTemplateBorderCanvasTop = HostClipTileViewModel.TileContentHeight - HostClipTileViewModel.PasteTemplateToolbarHeight;       
            } else {
                PasteTemplateBorderCanvasTop = HostClipTileViewModel.TileContentHeight + 10;
            }
        }

        public void SetSubItem(MpRtbItemViewModel rtbvm) {            
            MpClipTrayViewModel.Instance.RequestScrollIntoView(HostClipTileViewModel);
            if (HostClipTileViewModel.IsPastingTemplate) {
                //InitWithRichTextBox(rtbvm.Rtb);
            } else {
                //this doesn't get called because tile is shrunk before setting ispastingtemplate to false
                //so that tile content is resized 'right'
                ClearAllTemplates();
                PasteTemplateToolbarVisibility = Visibility.Collapsed;
                MpClipTrayViewModel.Instance.RequestScrollToHome();
            }
        }
        #endregion

        #region Private Methods
        
        private void InitWithRichTextBox(RichTextBox rtb) {
            SubSelectedRtbViewModel = rtb.DataContext as MpRtbItemViewModel;

            var pasteTemplateToolbarBorder = _borderGrid.GetVisualAncestor<Border>();
            var cb = (MpClipBorder)pasteTemplateToolbarBorder.GetVisualAncestor<MpClipBorder>();
            var editRichTextToolbarBorder = (Border)cb.FindName("ClipTileEditorToolbar");
            var editTemplateToolbarBorder = (Border)cb.FindName("ClipTileEditTemplateToolbar");
            var clipTray = new ListBox();// MpClipTrayViewModel.Instance.ClipTileViewModels.ListBox;
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxListBoxGridContainerCanvas");
            var rtblb = (ListBox)cb.FindName("ClipTileRichTextBoxListBox");
            var ctttg = (Grid)cb.FindName("ClipTileTitleTextGrid");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var clearAllTemplatesButton = (Button)pasteTemplateToolbarBorder.FindName("ClearAllTemplatesButton");
            SelectedTemplateTextBox = (TextBox)pasteTemplateToolbarBorder.FindName("SelectedTemplateTextBox");
            PreviousTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("PreviousTemplateButton");
            NextTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("NextTemplateButton");
            PasteTemplateButton = (Button)pasteTemplateToolbarBorder.FindName("PasteTemplateButton");
            var selectedTemplateComboBox = (ComboBox)pasteTemplateToolbarBorder.FindName("SelectedTemplateComboBox");
            //var ds = HostClipTileViewModel.ContentContainerViewModel.FullDocument.GetDocumentSize();

            #region Focus Appearance 
            PreviousTemplateButton.GotFocus += (s, e4) => {
                UpdateFocusAppearance();
            };
            PreviousTemplateButton.LostFocus += (s, e5) => {
                UpdateFocusAppearance();
            };
            NextTemplateButton.GotFocus += (s, e4) => {
                UpdateFocusAppearance();
            };
            NextTemplateButton.LostFocus += (s, e5) => {
                UpdateFocusAppearance();
            };
            PasteTemplateButton.GotFocus += (s, e4) => {
                UpdateFocusAppearance();
            };
            PasteTemplateButton.LostFocus += (s, e5) => {
                UpdateFocusAppearance();
            };
            SelectedTemplateTextBox.GotFocus += (s, e4) => {
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = string.Empty;
                }
                UpdateFocusAppearance();
            };
            SelectedTemplateTextBox.LostFocus += (s, e5) => {
                //ensures current template textbox show the template name as a placeholder when input is empty                
                if (string.IsNullOrEmpty(SelectedTemplateText) ||
                    SelectedTemplateText == SelectedTemplateTextBoxPlaceHolderText) {
                    SelectedTemplateText = SelectedTemplateTextBoxPlaceHolderText;
                }
                UpdateFocusAppearance();
            };
            #endregion

            SelectedTemplateTextBox.IsVisibleChanged += (s, e9) => {
                //this is used to capture the current template text box when the paste toolbar is shown
                //to
                if (PasteTemplateToolbarVisibility == Visibility.Collapsed) {
                    return;
                }
                var tbx = (TextBox)s;
                tbx.Focus();
                //tbx.SelectAll();
            };
            SelectedTemplateTextBox.PreviewKeyDown += (s, e8) => {
                if(IsBusy) {
                    IsBusy = false;
                    e8.Handled = true;
                    return;
                }
                if (e8.Key == Key.Enter) {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
                        PreviousTemplateTokenCommand.Execute(null);
                    } else {
                        NextTemplateTokenCommand.Execute(null);
                    }
                    SelectedTemplateTextBox.Focus();
                    e8.Handled = true;
                }
            };
            SelectedTemplateTextBox.TextChanged += (s, e4) => {
                OnPropertyChanged(nameof(PasteButtonVisibility));
                OnPropertyChanged(nameof(PasteButtonText));
            };

            clearAllTemplatesButton.MouseLeftButtonUp += (s, e2) => {
                //when clear all is clicked it performs the ClearAllTemplate Command and this switches focus to 
                //first template tbx
                SelectedTemplateTextBox.Focus();
                //e2.Handled = false;
            };

            OnPropertyChanged(nameof(PasteTemplateNavigationButtonStackVisibility));
            SetTemplate(SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.Templates[0].TemplateName);

            SelectedTemplateTextBox.Focus();
        }

        private void SetTemplate(string templateName) {
            if (string.IsNullOrEmpty(templateName)) {
                return;
            }
            //SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectTemplate(templateName);
            HostClipTileViewModel.ContentContainerViewModel.RequestScrollIntoView(SubSelectedRtbViewModel);
            var rtb = SubSelectedRtbViewModel.Rtb;
            var characterRect = SelectedTemplate.TemplateHyperlinkRange.End.GetCharacterRect(LogicalDirection.Forward);
            HostClipTileViewModel.ContentContainerViewModel.RequestScrollIntoView(rtb);
            rtb.ScrollToHorizontalOffset(characterRect.Left - rtb.ActualWidth / 2d);
            rtb.ScrollToVerticalOffset(characterRect.Top - rtb.ActualHeight / 2d);

            SelectedTemplateIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplate);
        }

        private void UpdateFocusAppearance() {
            OnPropertyChanged(nameof(SelectedTemplateTextBoxBorderBrush));
            OnPropertyChanged(nameof(PreviousButtonBorderBrush));
            OnPropertyChanged(nameof(NextButtonBorderBrush));
            OnPropertyChanged(nameof(PasteButtonBorderBrush));
        }
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
            return HostClipTileViewModel != null &&
                HostClipTileViewModel.IsPastingTemplate &&
                ClearAllTemplateToolbarButtonVisibility == Visibility.Visible;
        }
        private void ClearAllTemplates() {
            foreach (var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SetTemplateText(uthlvm.TemplateName, string.Empty);
            }

            foreach(MpRtbItemViewModel srtbvm in HostClipTileViewModel.ContentContainerViewModel.SubSelectedContentItems) {
                //foreach(var thlvm in srtbvm.TemplateHyperlinkCollectionViewModel) {
                //    thlvm.IsSelected = false;
                //}                
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
            return HostClipTileViewModel != null && 
                HostClipTileViewModel.IsPastingTemplate && 
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
            return SubSelectedRtbViewModel != null && 
                   HostClipTileViewModel.IsPastingTemplate;
        }
        private void NextTemplateToken() {
            if(!string.IsNullOrEmpty(SelectedTemplateName)) {
                //foreach(var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel) {
                //    if(thlvm.TemplateName == SelectedTemplateName && !string.IsNullOrEmpty(thlvm.TemplateText)) {
                //        thlvm.WasVisited = true;
                //    }
                //}
            }
            int nextIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SelectedTemplate) + 1;
            if(nextIdx >= UniqueTemplateHyperlinkViewModelListByDocOrder.Count) {
                nextIdx = 0;
            }
           // SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectTemplate(UniqueTemplateHyperlinkViewModelListByDocOrder[nextIdx].TemplateName);
            SelectedTemplateIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplate);
            SelectedTemplateTextBox.Focus();
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
            return SubSelectedRtbViewModel != null &&
                   HostClipTileViewModel.IsPastingTemplate;
        }
        private void PreviousTemplateToken() {
            //if (!string.IsNullOrEmpty(SelectedTemplateName)) {
            //    foreach (var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel) {
            //        if (thlvm.TemplateName == SelectedTemplateName && !string.IsNullOrEmpty(thlvm.TemplateText)) {
            //            thlvm.WasVisited = true;
            //        }
            //    }
            //}
            //int prevIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SelectedTemplate) - 1;
            //if (prevIdx < 0) {
            //    prevIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.Count - 1;
            //}
            //SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectTemplate(UniqueTemplateHyperlinkViewModelListByDocOrder[prevIdx].TemplateName);
            //SelectedTemplateIdx = UniqueTemplateHyperlinkViewModelListByDocOrder.IndexOf(SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplate);
            //SelectedTemplateTextBox.Focus();
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
            //only allow template to be pasted once all template types have been visited
            return DoAllSubItemTemplatesHaveText;            
        }
        private void PasteTemplate() {
            if(!HaveAllSubItemTemplatesBeenVisited) {
                //this occurs when user clicks paste instead of pressing enter on last template in subitem
                NextTemplateTokenCommand.Execute(null);
                return;
            }

            foreach(var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                SubSelectedRtbViewModel.RawRtf = SubSelectedRtbViewModel.RawRtf.Replace(uthlvm.TemplateName, uthlvm.TemplateText);
            }
            SubSelectedRtbViewModel.TemplateRichText = SubSelectedRtbViewModel.RawRtf;
            return;
            //to paste w/ templated text a clone of the templates is created then 
            //the links are cleared (returning the edited text to the template names
            var uthlvmlc = new List<MpTemplateHyperlinkViewModel>();
            foreach (var uthlvm in UniqueTemplateHyperlinkViewModelListByDocOrder) {
                uthlvmlc.Add((MpTemplateHyperlinkViewModel)uthlvm.Clone());
            }
            //ClipTileViewModel.TileVisibility = Visibility.Hidden;
            var sw = new Stopwatch();
            sw.Start();
            var srtbvm = SubSelectedRtbViewModel;
            srtbvm.RequestClearHyperlinks();
            var docClone = srtbvm.Rtb.Document.Clone();
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine(@"Clear: " + sw.ElapsedMilliseconds + "ms");
            sw.Start();
            foreach (var uthlvm in uthlvmlc) {
                var matchList = MpHelpers.Instance.FindStringRangesFromPosition(
                    docClone.ContentStart,
                    uthlvm.TemplateName,
                    true);
                foreach (var tr in matchList) {
                    tr.Text = uthlvm.TemplateText;
                }
            }
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine(@"Replace: " + sw.ElapsedMilliseconds + "ms");
            sw.Start();
            srtbvm.RequestCreateHyperlinks();
            sw.Stop();
            MonkeyPaste.MpConsole.WriteLine(@"Create: " + sw.ElapsedMilliseconds + "ms");
            srtbvm.TemplateRichText = MpHelpers.Instance.ConvertFlowDocumentToRichText(docClone);
            //Returned to GetPastableRichText
        }

        #endregion

        #region IDisposable
        public void Dispose() {
            SelectedTemplateTextBox = null;
            NextTemplateButton = null;
            PreviousTemplateButton = null;
    }
        #endregion
    }
}
