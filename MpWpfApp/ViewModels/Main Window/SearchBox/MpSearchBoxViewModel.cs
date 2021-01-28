using AsyncAwaitBestPractices.MVVM;
using DataGridAsyncDemoMVVM.filtersort;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpSearchBoxViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region Events
        public event EventHandler NextMatchClicked;
        protected virtual void OnNextMatchClicked() => NextMatchClicked?.Invoke(this, EventArgs.Empty);

        public event EventHandler PrevMatchClicked;
        protected virtual void OnPrevMatchClicked() => PrevMatchClicked?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Properties     
        private double _width = 125;
        public double Width {
            get {
                return _width;
            }
            set {
                if (_width != value) {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double _height = 25;
        public double Height {
            get {
                return _height;
            }
            set {
                if (_height != value) {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private double _fontSize = 16;
        public double FontSize {
            get {
                return _fontSize;
            }
            set {
                if (_fontSize != value) {
                    _fontSize = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        private double _cornerRadius = 10;
        public double CornerRadius {
            get {
                return _cornerRadius;
            }
            set {
                if (_cornerRadius != value) {
                    _cornerRadius = value;
                    OnPropertyChanged(nameof(CornerRadius));
                }
            }
        }

        private string _placeholderText = "Placeholder Text";
        public string PlaceholderText {
            get {
                return _placeholderText;
            }
            set {
                if (_placeholderText != value) {
                    _placeholderText = value;
                    OnPropertyChanged(nameof(PlaceholderText));
                }
            }
        }

        private string _text = string.Empty;
        public string Text {
            get {
                return _text;
            }
            set {
                if (_text != value) {
                    _text = value;
                    //SearchText = Text;
                    OnPropertyChanged(nameof(Text));
                    OnPropertyChanged(nameof(HasText));
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(TextBoxFontStyle));
                    OnPropertyChanged(nameof(TextBoxBorderBrush));
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                //if (_searchText != value) 
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        private bool _isTextBoxFocused = false;
        public bool IsTextBoxFocused {
            get {
                return _isTextBoxFocused;
            }
            set {
                //omitting duplicate check to enforce change in ui
                if (_isTextBoxFocused != value) {
                    _isTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTextBoxFocused));
                }
            }
        }

        private bool _isSearchEnabled = true;
        public bool IsSearchEnabled {
            get {
                return _isSearchEnabled;
            }
            set {
                //omitting duplicate check to enforce change in ui
                if (_isSearchEnabled != value) {
                    _isSearchEnabled = value;
                    OnPropertyChanged(nameof(IsSearchEnabled));
                }
            }
        }

        public int SearchBorderColumnSpan {
            get {
                if(SearchNavigationButtonPanelVisibility == Visibility.Visible) {
                    return 1;
                }
                return 2;
            }
        }

        private bool _isTextValid = true;
        public bool IsTextValid {
            get {
                return _isTextValid;
            }
            set {
                if (_isTextValid != value) {
                    _isTextValid = value;
                    OnPropertyChanged(nameof(IsTextValid));
                    OnPropertyChanged(nameof(TextBoxBorderBrush));
                }
            }
        }

        private bool _isSearching = false;
        public bool IsSearching {
            get {
                return _isSearching;
            }
            set {
                if (_isSearching != value) {
                    _isSearching = value;
                    OnPropertyChanged(nameof(IsSearching));
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(SearchSpinnerVisibility));
                }
            }
        }

        public bool HasText {
            get {
                return Text.Length > 0 && Text != PlaceholderText;
            }
        }

        public Brush TextBoxBorderBrush {
            get {
                if (IsTextValid) {
                    return Brushes.Transparent;
                }
                return Brushes.Red;
            }
        }

        public SolidColorBrush TextBoxTextBrush {
            get {
                if (HasText) {
                    return Brushes.Black;
                }
                return Brushes.DimGray;
            }
        }

        public FontStyle TextBoxFontStyle {
            get {
                if (HasText) {
                    return FontStyles.Normal;
                }
                return FontStyles.Italic;
            }
        }

        public Visibility ClearTextButtonVisibility {
            get {
                if (HasText && !IsSearching) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility SearchSpinnerVisibility {
            get {
                if (IsSearching) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private Visibility _searchNavigationButtonPanelVisibility = Visibility.Collapsed;
        public Visibility SearchNavigationButtonPanelVisibility {
            get {
                return _searchNavigationButtonPanelVisibility;
            }
            set {
                if (_searchNavigationButtonPanelVisibility != value) {
                    _searchNavigationButtonPanelVisibility = value;
                    OnPropertyChanged(nameof(SearchNavigationButtonPanelVisibility));
                    OnPropertyChanged(nameof(SearchBorderColumnSpan));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpSearchBoxViewModel() : base() { }

        public void SearchBoxBorder_Loaded(object sender, RoutedEventArgs args) {
            var tb = (TextBox)((MpClipBorder)sender).FindName("SearchBox");
            tb.GotFocus += (s, e4) => {
                if (!HasText) {
                    Text = string.Empty;
                }

                IsTextBoxFocused = true;
                MainWindowViewModel.ClipTrayViewModel.ResetClipSelection();
                OnPropertyChanged(nameof(TextBoxFontStyle));
                OnPropertyChanged(nameof(TextBoxTextBrush));
            };
            tb.LostFocus += (s, e5) => {
                IsTextBoxFocused = false;
                if (!HasText) {
                    Text = Properties.Settings.Default.SearchPlaceHolderText;
                }
            };
            if (string.IsNullOrEmpty(Text)) {
                Text = Properties.Settings.Default.SearchPlaceHolderText;
            }

            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0,0,0,0,500);
            timer.Tick += (s, e) => {
                PerformSearchCommand.Execute(null);
                timer.Stop();
            };
            PropertyChanged += (s, e7) => {
                switch (e7.PropertyName) {
                    case nameof(Text):
                        timer.Stop();
                        timer.Start();
                        break;
                }
            };
        }
        #endregion

        #region Commands
        private RelayCommand _clearTextCommand;
        public ICommand ClearTextCommand {
            get {
                if (_clearTextCommand == null) {
                    _clearTextCommand = new RelayCommand(ClearText, CanClearText);
                }
                return _clearTextCommand;
            }
        }
        private bool CanClearText() {
            return Text.Length > 0;
        }
        private void ClearText() {
            Text = string.Empty;
            IsTextBoxFocused = true;
            SearchText = Text;
        }

        private RelayCommand _performSearchCommand;
        public ICommand PerformSearchCommand {
            get {
                if(_performSearchCommand == null) {
                    _performSearchCommand = new RelayCommand(PerformSearch);
                }
                return _performSearchCommand;
            }
        }
        private void PerformSearch() {
            //var mpft = new MemberPathFilterText();
            //mpft.FilterText = Text;
            //mpft.MemberPath = "CopyItemPlainText";
            //MainWindowViewModel.ClipTrayViewModel.FilterCommand.Execute(mpft);

            SearchText = Text;

            IsSearching = true;

            //var vt = new List<MpClipTileViewModel>();
            //var ct = new List<MpClipTileViewModel>();

            //foreach (MpClipTileViewModel ctvm in MainWindowViewModel.ClipTrayViewModel.ClipTileViewModels) {
            //    var ttb = ctvm.GetTitleTextBlock();
            //    var rtb = ctvm.GetRtb();
            //    var flb = ctvm.GetFileListBox();
            //    Visibility result = await Dispatcher.CurrentDispatcher.Invoke(
            //        new Func<Task<Visibility>>(async () => await PerformHighlight(ctvm, SearchText,ttb,rtb,flb)));
            //    if(result == Visibility.Visible) {
            //        vt.Add(ctvm);
            //    } else {
            //        ct.Add(ctvm);
            //    }
            //}

            //foreach(var ctvm in vt) {
            //    ctvm.TileVisibility = Visibility.Visible;
            //}

            //foreach (var ctvm in ct) {
            //    ctvm.TileVisibility = Visibility.Collapsed;
            //;}
        }

        //private async Task<Visibility> PerformHighlight(MpClipTileViewModel ctvm, string hlt, TextBlock ttb, RichTextBox rtb, ListBox flb) {
        //    var sttvm = ctvm.MainWindowViewModel.ClipTrayViewModel.MainWindowViewModel.TagTrayViewModel.SelectedTagTile;

        //    //var ttb = ctvm.GetTitleTextBlock();
        //    var hb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightColorHexString);
        //    var hfb = (Brush)new BrushConverter().ConvertFrom(Properties.Settings.Default.HighlightFocusedHexColorString);
        //    var ctbb = Brushes.Transparent;

        //    if (!sttvm.IsLinkedWithClipTile(ctvm)) {
        //        Console.WriteLine("Clip tile w/ title " + ctvm.CopyItemTitle + " is not linked with current tag");
        //        return Visibility.Collapsed;
        //        //return;
        //    }
        //    //bool isInTitle = ttb.Text.ContainsByCaseSetting(hlt);
        //    //bool isInContent = ctvm.ToString().ContainsByCaseSetting(hlt);
        //    //bool isSearchBlank = string.IsNullOrEmpty(hlt.Trim()) || hlt == Properties.Settings.Default.SearchPlaceHolderText;
        //    //ctvm.TileVisibility = isInTitle || isInContent || isSearchBlank ? Visibility.Visible : Visibility.Collapsed;
        //    //return;
        //    Console.WriteLine("Beginning highlight clip with title: " + ctvm.CopyItemTitle + " with highlight text: " + hlt);

        //    ctvm.TileVisibility = Visibility.Visible;

        //    MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, ctbb);
        //    ctvm.LastTitleHighlightRangeList.Clear();

        //    MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, ctbb);
        //    ctvm.LastContentHighlightRangeList.Clear();

        //    if (string.IsNullOrEmpty(hlt.Trim()) ||
        //        hlt == Properties.Settings.Default.SearchPlaceHolderText) {
        //        //if search text is empty clear any highlights and show clip (if associated w/ current tag)
        //        return Visibility.Visible;
        //    }

        //    //highlight title 
        //    if (ttb.Text.ContainsByCaseSetting(hlt)) {
        //        foreach (var mr in MpHelpers.Instance.FindStringRangesFromPosition(ttb.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive)) {
        //            ctvm.LastTitleHighlightRangeList.Add(mr);
        //        }
        //        MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastTitleHighlightRangeList, hb);
        //    }
        //    switch (ctvm.CopyItemType) {
        //        case MpCopyItemType.RichText:
        //            //var rtb = ctvm.GetRtb();
        //            var mc = Regex.Matches(ctvm.CopyItemPlainText, hlt, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
        //            if (mc.Count == 0) {
        //                if (ctvm.LastTitleHighlightRangeList.Count == 0) {
        //                    return Visibility.Collapsed;
        //                }
        //                return Visibility.Visible;
        //            }

        //            rtb.BeginChange();
        //            foreach (var mr in MpHelpers.Instance.FindStringRangesFromPosition(rtb.Document.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive)) {
        //                ctvm.LastContentHighlightRangeList.Add(mr);
        //            }
        //            if (ctvm.LastContentHighlightRangeList.Count > 0) {
        //                MpHelpers.Instance.ApplyBackgroundBrushToRangeList(ctvm.LastContentHighlightRangeList, hb);
        //                //rtb.CaretPosition = ctvm.LastContentHighlightRangeList[0].Start;
        //            } else if (ctvm.LastTitleHighlightRangeList.Count == 0) {
        //                return Visibility.Collapsed;
        //            }
        //            if (ctvm.LastContentHighlightRangeList.Count > 0 || ctvm.LastTitleHighlightRangeList.Count > 0) {
        //                ctvm.CurrentHighlightMatchIdx = 0;
        //            }
        //            rtb.EndChange();
        //            break;
        //        case MpCopyItemType.Image:
        //            foreach (var diovm in ctvm.DetectedImageObjectCollectionViewModel) {
        //                if (diovm.ObjectTypeName.ContainsByCaseSetting(hlt)) {
        //                    return Visibility.Visible;
        //                }
        //            }
        //            if (ctvm.LastContentHighlightRangeList.Count == 0) {
        //                return Visibility.Collapsed;
        //            }
        //            break;
        //        case MpCopyItemType.FileList:
        //            //var flb = ctvm.GetFileListBox();
        //            foreach (var fivm in ctvm.FileListViewModels) {
        //                if (fivm.ItemPath.ContainsByCaseSetting(hlt)) {
        //                    var container = flb.ItemContainerGenerator.ContainerFromItem(fivm) as FrameworkElement;
        //                    if (container != null) {
        //                        var fitb = (TextBlock)container.FindName("FileListItemTextBlock");
        //                        if (fitb != null) {
        //                            var hlr = MpHelpers.Instance.FindStringRangeFromPosition(fitb.ContentStart, hlt, Properties.Settings.Default.IsSearchCaseSensitive);
        //                            if (hlr != null) {
        //                                hlr.ApplyPropertyValue(TextBlock.BackgroundProperty, hb);
        //                                ctvm.LastContentHighlightRangeList.Add(hlr);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if (ctvm.LastContentHighlightRangeList.Count == 0) {
        //                return Visibility.Collapsed;
        //            }
        //            return Visibility.Visible;
        //    }
        //    Console.WriteLine("Ending highlighting clip with title: " + ctvm.CopyItemTitle);

        //    return Visibility.Visible;
        //}

        private RelayCommand _nextMatchCommand;
        public ICommand NextMatchCommand {
            get {
                if (_nextMatchCommand == null) {
                    _nextMatchCommand = new RelayCommand(NextMatch);
                }
                return _nextMatchCommand;
            }
        }
        private void NextMatch() {
            OnNextMatchClicked();
        }

        private RelayCommand _prevMatchCommand;
        public ICommand PrevMatchCommand {
            get {
                if (_prevMatchCommand == null) {
                    _prevMatchCommand = new RelayCommand(PrevMatch);
                }
                return _prevMatchCommand;
            }
        }
        private void PrevMatch() {
            OnPrevMatchClicked();
        }
        #endregion
    }
}
