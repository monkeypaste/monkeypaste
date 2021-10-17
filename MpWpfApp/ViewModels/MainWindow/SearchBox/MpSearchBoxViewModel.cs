using AsyncAwaitBestPractices.MVVM;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MonkeyPaste;

namespace MpWpfApp {

    public class MpSearchBoxViewModel : MpViewModelBase<object> {
        #region Singleton Definition
        private static readonly Lazy<MpSearchBoxViewModel> _Lazy = new Lazy<MpSearchBoxViewModel>(() => new MpSearchBoxViewModel());
        public static MpSearchBoxViewModel Instance { get { return _Lazy.Value; } }

        public void Init() { }
        #endregion

        #region Private Variables
        #endregion

        #region Properties     

        #region SearchBy Property Settings
        private bool _searchByIsCaseSensitive = Properties.Settings.Default.SearchByIsCaseSensitive;
        public bool SearchByIsCaseSensitive {
            get {
                return _searchByIsCaseSensitive;
            }
            set {
                if (_searchByIsCaseSensitive != value) {
                    _searchByIsCaseSensitive = value;
                    Properties.Settings.Default.SearchByIsCaseSensitive = _searchByIsCaseSensitive;
                    OnPropertyChanged(nameof(SearchByIsCaseSensitive));
                }
            }
        }

        private bool _searchByTitle = Properties.Settings.Default.SearchByTitle;
        public bool SearchByTitle {
            get {
                return _searchByTitle;
            }
            set {
                if (_searchByTitle != value) {
                    _searchByTitle = value;
                    Properties.Settings.Default.SearchByTitle = _searchByTitle;
                    OnPropertyChanged(nameof(SearchByTitle));
                }
            }
        }

        private bool _searchByRichText = Properties.Settings.Default.SearchByRichText;
        public bool SearchByRichText {
            get {
                return _searchByRichText;
            }
            set {
                if (_searchByRichText != value) {
                    _searchByRichText = value;
                    Properties.Settings.Default.SearchByRichText = _searchByRichText;
                    OnPropertyChanged(nameof(SearchByRichText));
                }
            }
        }

        private bool _searchByUrl = Properties.Settings.Default.SearchBySourceUrl;
        public bool SearchByUrl {
            get {
                return _searchByUrl;
            }
            set {
                if (_searchByUrl != value) {
                    _searchByUrl = value;
                    Properties.Settings.Default.SearchBySourceUrl = _searchByUrl;
                    OnPropertyChanged(nameof(SearchByUrl));
                }
            }
        }

        private bool _searchByFileList = Properties.Settings.Default.SearchByFileList;
        public bool SearchByFileList {
            get {
                return _searchByFileList;
            }
            set {
                if (_searchByFileList != value) {
                    _searchByFileList = value;
                    Properties.Settings.Default.SearchByFileList = _searchByFileList;
                    OnPropertyChanged(nameof(SearchByFileList));
                }
            }
        }

        private bool _searchByImage = Properties.Settings.Default.SearchByImage;
        public bool SearchByImage {
            get {
                return _searchByImage;
            }
            set {
                if (_searchByImage != value) {
                    _searchByImage = value;
                    Properties.Settings.Default.SearchByImage = _searchByImage;
                    OnPropertyChanged(nameof(SearchByImage));
                }
            }
        }

        private bool _searchByApplicationName = Properties.Settings.Default.SearchByApplicationName;
        public bool SearchByApplicationName {
            get {
                return _searchByApplicationName;
            }
            set {
                if (_searchByApplicationName != value) {
                    _searchByApplicationName = value;
                    Properties.Settings.Default.SearchByApplicationName = _searchByApplicationName;
                    OnPropertyChanged(nameof(SearchByApplicationName));
                }
            }
        }

        private bool _searchByTag = Properties.Settings.Default.SearchByTag;
        public bool SearchByTag {
            get {
                return _searchByTag;
            }
            set {
                if (_searchByTag != value) {
                    _searchByTag = value;
                    Properties.Settings.Default.SearchByTag = _searchByTitle;
                    OnPropertyChanged(nameof(SearchByTag));
                }
            }
        }

        private bool _searchByProcessName = Properties.Settings.Default.SearchByProcessName;
        public bool SearchByProcessName {
            get {
                return _searchByProcessName;
            }
            set {
                if (_searchByProcessName != value) {
                    _searchByProcessName = value;
                    Properties.Settings.Default.SearchByProcessName = _searchByProcessName;
                    OnPropertyChanged(nameof(SearchByProcessName));
                }
            }
        }

        private bool _searchByDescription = Properties.Settings.Default.SearchByDescription;
        public bool SearchByDescription {
            get {
                return _searchByDescription;
            }
            set {
                if (_searchByDescription != value) {
                    _searchByDescription = value;
                    Properties.Settings.Default.SearchByDescription = _searchByDescription;
                    OnPropertyChanged(nameof(SearchByDescription));
                }
            }
        }

        public MpContentFilterType FilterType {
            get {
                MpContentFilterType ft = MpContentFilterType.None;
                if(SearchByIsCaseSensitive) {
                    ft |= MpContentFilterType.CaseSensitive;
                }
                if(SearchByTitle) {
                    ft |= MpContentFilterType.Title;
                }
                if(SearchByRichText) {
                    ft |= MpContentFilterType.Text;
                }
                if(SearchByFileList) {
                    ft |= MpContentFilterType.File;
                }
                if(SearchByImage) {
                    ft |= MpContentFilterType.Image;
                }
                if(SearchByUrl) {
                    ft |= MpContentFilterType.Url;
                }
                if(SearchByApplicationName) {
                    ft |= MpContentFilterType.AppName;
                }
                if(SearchByProcessName) {
                    ft |= MpContentFilterType.AppPath;
                }
                if(SearchByTag) {
                    ft |= MpContentFilterType.Tag;
                }
                if(SearchByDescription) {
                    ft |= MpContentFilterType.Meta;
                }
                return ft;
            }
        }

        #endregion

        #region Business Logic Properties
        public string PlaceholderText {
            get {
                return Properties.Settings.Default.SearchPlaceHolderText;
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
                if (_isSearchEnabled != value) {
                    _isSearchEnabled = value;
                    OnPropertyChanged(nameof(IsSearchEnabled));
                }
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
        #endregion

        #region Appearance
        public int SearchBorderColumnSpan {
            get {
                if (SearchNavigationButtonPanelVisibility == Visibility.Visible) {
                    return 1;
                }
                return 2;
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
        #endregion

        #region Visibility Proeprties
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

        #endregion

        #region Events
        public event EventHandler<string> OnSearchTextChanged;
        public event EventHandler<MpContentFilterType> OnFilterFlagsChanged;

        public event EventHandler OnSearchTextBoxFocusRequest;
        #endregion

        #region Public Methods
        public MpSearchBoxViewModel() : base(null) {
            Text = PlaceholderText;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(Properties.Settings.Default.SearchBoxTypingDelayInMilliseconds);
            timer.Tick += (s, e) => {
                PerformSearchCommand.Execute(null);
                timer.Stop();
            };
            PropertyChanged += (s, e7) => {
                switch (e7.PropertyName) {
                    case nameof(IsTextBoxFocused):
                        if(IsTextBoxFocused) {
                            if (!HasText) {
                                Text = string.Empty;
                            }

                            MpClipTrayViewModel.Instance.ResetClipSelection(false);                            
                        } else {
                            if(!HasText) {
                                Text = PlaceholderText;
                            }
                        }
                        OnPropertyChanged(nameof(TextBoxFontStyle));
                        OnPropertyChanged(nameof(TextBoxTextBrush));
                        break;
                    case nameof(Text):
                        if(!HasText && !string.IsNullOrWhiteSpace(SearchText) && SearchText != PlaceholderText) {
                            //when there WAS search text but user has deleted all text
                            OnSearchTextChanged?.Invoke(this, Text);
                        } else if(HasText) {
                            OnSearchTextChanged?.Invoke(this, Text);
                        }
                        

                        timer.Stop();
                        timer.Start();
                        break;
                    case nameof(FilterType):
                        OnFilterFlagsChanged?.Invoke(this, FilterType);
                        break;
                }
            };

            OnViewModelLoaded();
        }

        public void RequestSearchBoxFocus() {
            OnSearchTextBoxFocusRequest?.Invoke(this, new EventArgs());
        }

        #endregion

        #region Private Methods
        
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
            SearchText = Text;
            MpClipTrayViewModel.Instance.ResetClipSelection();
            //IsSearching = true;
        }

        public ICommand PerformSearchCommand => new RelayCommand(
            () => {
                SearchText = Text;
                if (!HasText) {
                    IsTextValid = true;
                }
                var ct = MpClipTrayViewModel.Instance;
                //wait till all highlighting is complete then hide non-matching tiles at the same time
                var newVisibilityDictionary = new Dictionary<MpClipTileViewModel, Dictionary<object, Visibility>>();
                bool showMatchNav = false;
                foreach (var ctvm in ct.Items) {
                    //var newVisibility = await ctvm.HighlightTextRangeViewModelCollection.PerformHighlightingAsync(SearchText);
                    //newVisibilityDictionary.Add(ctvm, newVisibility);
                    ctvm.RequestSearch(SearchText);
                    if (ctvm.HighlightTextRangeViewModelCollection.Count > 1) {
                        showMatchNav = true;
                    }
                }
                if (ct.IsAnyTileExpanded) {
                    if (HasText) {
                        IsTextValid = ct.SelectedItems[0].HighlightTextRangeViewModelCollection.Count > 0;
                    } else {
                        IsTextValid = true;
                    }
                } else {
                    foreach (var kvp in newVisibilityDictionary) {
                        foreach (var skvp in kvp.Value) {
                            if (skvp.Key is MpClipTileViewModel) {
                                (skvp.Key as MpClipTileViewModel).ItemVisibility = skvp.Value;
                            }
                            if (skvp.Key is MpClipTileViewModel && skvp.Value == Visibility.Collapsed) {
                                //if tile is collapsed ignore children visibility
                                break;
                            }
                            if (skvp.Key is MpContentItemViewModel) {
                                //(skvp.Key as MpContentItemViewModel).SubItemVisibility = skvp.Value;
                            }
                        }

                    }
                }
                SearchNavigationButtonPanelVisibility = showMatchNav ? Visibility.Visible : Visibility.Collapsed;
            });
        
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
            foreach(var ctvm in MpClipTrayViewModel.Instance.VisibleItems) {
                ctvm.HighlightTextRangeViewModelCollection.SelectNextMatchCommand.Execute(null);
            }
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
            foreach (var ctvm in MpClipTrayViewModel.Instance.VisibleItems) {
                ctvm.HighlightTextRangeViewModelCollection.SelectPreviousMatchCommand.Execute(null);
            }
        }
        #endregion
    }
}
