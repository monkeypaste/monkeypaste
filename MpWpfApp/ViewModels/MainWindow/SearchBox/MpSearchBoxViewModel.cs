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

    public class MpSearchBoxViewModel : MpSingletonViewModel<MpSearchBoxViewModel> {
        #region Private Variables
        #endregion

        #region Properties     

        #region View Models

        public ObservableCollection<MpSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpSearchCriteriaItemViewModel>();

        #endregion

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

        #region Layout

        public GridLength ClearAndAddCriteriaColumnWidth {
            get {
                if(string.IsNullOrEmpty(LastSearchText) || IsSearching) {
                    return new GridLength(0.1, GridUnitType.Star);
                } else {
                    return new GridLength(0.3, GridUnitType.Star);
                }
            }
        }

        public double SearchCriteriaListBoxHeight {
            get {
                return ((MpMeasurements.Instance.SearchDetailRowHeight * CriteriaItems.Count) +
                       ((MpMeasurements.Instance.SearchDetailBorderThickness * 2) * CriteriaItems.Count));
            }
        }

        #endregion

        #region Business Logic Properties

        public string PlaceholderText {
            get {
                return MpPreferences.Instance.SearchPlaceHolderText;
            }
        }

        #endregion

        #region State

        public bool HasCriteriaItems => CriteriaItems.Count > 0;

        public bool IsSaved => UserSearch != null && UserSearch.Id > 0;

        public bool CanAddCriteriaItem => !string.IsNullOrEmpty(LastSearchText) && !IsSearching;

        public string LastSearchText /*{ get; private set; } =*/ => string.Empty;

        public bool IsTextBoxFocused { get; set; }

        public bool IsOverClearTextButton { get; set; } = false;

        public bool IsSearchEnabled { get; set; }

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
                }
            }
        }

        public bool HasText {
            get {
                return SearchText.Length > 0;
            }
        }

        public bool IsPlaceholderVisible {
            get {
                if(IsTextBoxFocused) {
                    return false;
                }
                return !HasText;
            }
        }

        public bool IsOverSaveSearchButton { get; set; }

        #endregion

        #region Appearance

        public Brush SaveSearchButtonBorderBrush {
            get {
                if(IsOverSaveSearchButton) {
                    return Brushes.DimGray;
                }
                return Brushes.LightGray;
            }
        }
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

        public Brush CaretBrush => IsTextBoxFocused ? Brushes.Black : Brushes.Transparent;

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

        public string ClearButtonImagePath {
            get {
                if(IsOverClearTextButton) {
                    return @"/Images/close1.png";
                } else {
                    return @"/Images/close2.png";
                }
            }
        }


        public string AddOrClearSearchCriteriaImagePath {
            get {
                if (HasCriteriaItems) {
                    return @"/Resources/Images/minus2.png";
                } else {
                    return @"/Resources/Images/add2.png";
                }
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

        #region Model

        private string _text = string.Empty;
        public string SearchText {
            get {
                return _text;
            }
            set {
                if (_text != value) {
                    _text = value;
                    //SearchText = Text;
                    OnPropertyChanged(nameof(SearchText));
                    OnPropertyChanged(nameof(HasText));
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(TextBoxFontStyle));
                    OnPropertyChanged(nameof(TextBoxBorderBrush));
                }
            }
        }

        public MpUserSearch UserSearch { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler OnSearchTextBoxFocusRequest;

        #endregion

        #region Constructors

        public async Task Init() {
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                PropertyChanged += MpSearchBoxViewModel_PropertyChanged;
                CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;

                MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceiveClipTrayViewModelMessage);
            });
        }

        public MpSearchBoxViewModel() : base() { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpUserSearch us) {
            IsBusy = true;

            if (us == null) {
                UserSearch = null;
            } else {
                if (us.CriteriaItems == null || us.CriteriaItems.Count == 0) {
                    us = await MpDb.Instance.GetItemAsync<MpUserSearch>(us.Id);
                }

                UserSearch = us;
                CriteriaItems.Clear();
                foreach (var ci in UserSearch.CriteriaItems) {
                    var civm = await CreateCriteriaItemViewModel(ci);
                    CriteriaItems.Add(civm);
                }
            }

            OnPropertyChanged(nameof(CriteriaItems));
            OnPropertyChanged(nameof(HasCriteriaItems));

            IsBusy = false;
        }

        #region View Method Invokers

        public void RequestSearchBoxFocus() {
            OnSearchTextBoxFocusRequest?.Invoke(this, new EventArgs());
        }

        #endregion

        #endregion

        #region Private Methods

        private async Task<MpSearchCriteriaItemViewModel> CreateCriteriaItemViewModel(MpSearchCriteriaItem sci) {
            MpSearchCriteriaItemViewModel nscivm = new MpSearchCriteriaItemViewModel(this);
            await nscivm.InitializeAsync(sci);
            return nscivm;
        }

        private void ReceiveClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    IsSearching = false;
                    Validate();
                    break;
            }
        }

        private void MpSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SearchText):
                    Validate();
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    break;
                case nameof(IsSearching):
                    OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(CanAddCriteriaItem));
                    break;
                case nameof(IsTextBoxFocused):
                    if (IsTextBoxFocused) {
                        if (!HasText) {
                            SearchText = string.Empty;
                        }
                    } else {
                        if (!HasText) {
                            //SearchText = PlaceholderText;
                        }
                    }
                    OnPropertyChanged(nameof(TextBoxFontStyle));
                    OnPropertyChanged(nameof(TextBoxTextBrush));
                    OnPropertyChanged(nameof(IsPlaceholderVisible));
                    OnPropertyChanged(nameof(CaretBrush));
                    break;

                case nameof(HasCriteriaItems):
                    OnPropertyChanged(nameof(AddOrClearSearchCriteriaImagePath));
                    break;
            }
        }

        private async void CriteriaItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (IsBusy) {
                return;
            }

            OnPropertyChanged(nameof(AddOrClearSearchCriteriaImagePath));
            OnPropertyChanged(nameof(HasCriteriaItems));

            MpMainWindowViewModel.Instance.OnPropertyChanged(nameof(MpMainWindowViewModel.Instance.ClipTrayAndCriteriaListHeight));
            OnPropertyChanged(nameof(SearchCriteriaListBoxHeight));

            await UpdateCriteriaSortOrder();
        }

        private async Task UpdateCriteriaSortOrder(bool fromModel = false) {
            if (fromModel) {
                CriteriaItems.Sort(x => x.SortOrderIdx);
            } else {
                foreach (var scivm in CriteriaItems) {
                    scivm.SortOrderIdx = CriteriaItems.IndexOf(scivm);
                }
                if (!MpMainWindowViewModel.Instance.IsMainWindowLoading &&
                    IsSaved) {
                    IsBusy = true;

                    foreach (var scivm in CriteriaItems) {
                        await scivm.SearchCriteriaItem.WriteToDatabaseAsync();
                    }

                    IsBusy = false;
                }
            }
        }

        private bool Validate() {
            if (!HasText) {
                IsTextValid = true;
            } else {
                if(MpClipTrayViewModel.Instance.TotalItemsInQuery == 0) {
                    IsTextValid = false;
                }
            }
            return IsTextValid;
        }
        #endregion

        #region Commands

        public ICommand ClearTextCommand => new RelayCommand(
            () => {
                SearchText = string.Empty;
                if(!string.IsNullOrWhiteSpace(LastSearchText)) {
                    MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged();
                }
                //LastSearchText = string.Empty;
            },
            () => {
                return SearchText.Length > 0;
            });

        public ICommand PerformSearchCommand => new RelayCommand(
            () => {
                if (!HasText) {
                    IsTextValid = true;
                    //LastSearchText = string.Empty;
                } else {
                    IsSearching = true;
                    //LastSearchText = SearchText;
                }
                MpDataModelProvider.Instance.QueryInfo.NotifyQueryChanged();
            });

        public ICommand NextMatchCommand => new RelayCommand(
            () => {
                foreach (var ctvm in MpClipTrayViewModel.Instance.Items) {
                    ctvm.HighlightTextRangeViewModelCollection.SelectNextMatchCommand.Execute(null);
                }
            });

        public ICommand PrevMatchCommand => new RelayCommand(
            () => {
                foreach (var ctvm in MpClipTrayViewModel.Instance.Items) {
                    ctvm.HighlightTextRangeViewModelCollection.SelectPreviousMatchCommand.Execute(null);
                }
            });

        public ICommand CreateOrClearSearchCriteriaItemsCommand => new RelayCommand(
            async () => {
                if(HasCriteriaItems) {
                    CriteriaItems.Clear();

                } else {
                    MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                        SortOrderIdx = CriteriaItems.Count
                    };
                    MpSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                    CriteriaItems.Add(nscivm);
                }
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
            }, CanAddCriteriaItem);

        public ICommand AddSearchCriteriaItemCommand => new RelayCommand(
            async () => {
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = CriteriaItems.Count
                };
                MpSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                CriteriaItems.Add(nscivm);
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
            },CanAddCriteriaItem);

        public ICommand RemoveSearchCriteriaItemCommand => new RelayCommand<MpSearchCriteriaItemViewModel>(
            async (scivm) => {
                int scivmIdx = CriteriaItems.IndexOf(scivm);
                CriteriaItems.RemoveAt(scivmIdx);
                if (scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                await UpdateCriteriaSortOrder();
            });

        public ICommand SaveSearchCommand => new RelayCommand(
            async () => {
                await UserSearch.WriteToDatabaseAsync();
            });

        #endregion
    }
}
