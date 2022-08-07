
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
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {

    public class MpAvSearchBoxViewModel : MpViewModelBase, 
        MpIAsyncSingletonViewModel<MpAvSearchBoxViewModel>,
        MpIPopupMenuViewModel {
        #region Private Variables
        #endregion

        #region Properties     

        #region View Models

        public ObservableCollection<MpSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpSearchCriteriaItemViewModel>();

        private ObservableCollection<MpAvSearchFilterViewModel> _filters;
        public ObservableCollection<MpAvSearchFilterViewModel> Filters {
            get {
                if (_filters == null) {
                    _filters = new ObservableCollection<MpAvSearchFilterViewModel>() {
                        new MpAvSearchFilterViewModel(
                            this,
                            "Content",
                            nameof(MpPrefViewModel.Instance.SearchByContent),
                            MpContentFilterType.Content),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Title",
                            nameof(MpPrefViewModel.Instance.SearchByTitle),
                            MpContentFilterType.Title),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url",
                            nameof(MpPrefViewModel.Instance.SearchBySourceUrl),
                            MpContentFilterType.Url),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Url Title",
                            nameof(MpPrefViewModel.Instance.SearchByUrlTitle),
                            MpContentFilterType.UrlTitle),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Path",
                            nameof(MpPrefViewModel.Instance.SearchByProcessName),
                            MpContentFilterType.AppPath),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Application Name",
                            nameof(MpPrefViewModel.Instance.SearchByApplicationName),
                            MpContentFilterType.AppName),
                        //new MpSearchFilterViewModel(
                        //    this,
                        //    "Collections",
                        //    nameof(MpJsonPreferenceIO.Instance.SearchByTag),
                        //    MpContentFilterType.Tag),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Description",
                            nameof(MpPrefViewModel.Instance.SearchByDescription),
                            MpContentFilterType.Meta),
                        new MpAvSearchFilterViewModel(this,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Text Type",
                            nameof(MpPrefViewModel.Instance.SearchByTextType),
                            MpContentFilterType.TextType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "File Type",
                            nameof(MpPrefViewModel.Instance.SearchByFileType),
                            MpContentFilterType.FileType),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Image Type",
                            nameof(MpPrefViewModel.Instance.SearchByImageType),
                            MpContentFilterType.ImageType),
                        new MpAvSearchFilterViewModel(this,true),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Case Sensitive",
                            nameof(MpPrefViewModel.Instance.SearchByIsCaseSensitive),
                            MpContentFilterType.CaseSensitive),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Whole Word",
                            nameof(MpPrefViewModel.Instance.SearchByWholeWord),
                            MpContentFilterType.WholeWord),
                        new MpAvSearchFilterViewModel(
                            this,
                            "Regular Expression",
                            nameof(MpPrefViewModel.Instance.SearchByRegex),
                            MpContentFilterType.Regex)
                    };
                }
                return _filters;
            }
        }

        #endregion

        #region MpIPopupMenuViewModel Implementation

        public MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    //SubItems = Filters.Select(
                    //    x => new MpMenuItemViewModel() {
                    //        IsSeparator = x.IsSeperator,
                    //        IsChecked = x.IsChecked,
                    //        IsEnabled = x.IsEnabled,
                    //        Header = x.Label,
                    //        IconHexStr = MpSystemColors.Transparent,
                    //    }).Cast<MpMenuItemViewModel>().ToList()
                    SubItems = Filters.Select(x=>x.MenuItemViewModel).ToList()
                };
            }
        }
        #endregion

        #region Layout

        public GridLength ClearAndAddCriteriaColumnWidth {
            get {
                if (IsMultipleMatches) {
                    return new GridLength(0.3, GridUnitType.Star);
                } else if (string.IsNullOrEmpty(LastSearchText) || IsSearching || HasText) {
                    return new GridLength(0.1, GridUnitType.Star);
                } else {
                    return new GridLength(0, GridUnitType.Star);
                }
            }
        }

        public double SearchCriteriaListBoxItemHeight => 50;

        public double SearchCriteriaListBoxHeight {
            get {
                //return ((MpMeasurements.Instance.SearchDetailRowHeight * CriteriaItems.Count) +
                //       ((MpMeasurements.Instance.SearchDetailBorderThickness * 2) * CriteriaItems.Count));
                return SearchCriteriaListBoxItemHeight * CriteriaItems.Count;
            }
        }

        public double SearchBoxViewWidth { get; set; }

        #endregion

        #region Business Logic Properties

        public string PlaceholderText {
            get {
                return MpPrefViewModel.Instance.SearchPlaceHolderText;
            }
        }

        #endregion

        #region State
        public ObservableCollection<string> RecentSearchTexts {
            get => new ObservableCollection<string>(MpPrefViewModel.Instance.RecentSearchTexts.Split(new string[] { MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN }, StringSplitOptions.RemoveEmptyEntries));
            set => MpPrefViewModel.Instance.RecentSearchTexts = string.Join(MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN, value);
        }

        public bool CanDeleteSearch => UserSearch != null && UserSearch.Id > 0;

        public bool CanSaveSearch => HasCriteriaItems || HasText;

        public bool IsMultipleMatches { get; private set; } = false;

        public bool HasCriteriaItems => CriteriaItems.Count > 0;

        public bool IsSaved => UserSearch != null && UserSearch.Id > 0;

        public bool CanAddCriteriaItem => true;//!string.IsNullOrEmpty(LastSearchText) && !IsSearching;

        public string LastSearchText { get; private set; } = string.Empty;

        public bool IsTextBoxFocused { get; set; }

        public bool IsOverClearTextButton { get; set; } = false;

        public bool IsTextValid { get; private set; } = true;

        private bool _isSearching = false;
        public bool IsSearching {
            get {
                return _isSearching;
            }
            set {
                if (_isSearching != value) {
                    _isSearching = value;
                    OnPropertyChanged(nameof(IsSearching));
                    OnPropertyChanged(nameof(IsClearTextButtonVisible));
                }
            }
        }

        public bool HasText {
            get {
                return SearchText.Length > 0;
            }
        }

        public bool IsOverSearchByButton { get; set; }
        public bool IsOverSaveSearchButton { get; set; }

        public bool IsOverDeleteSearchButton { get; set; }

        #endregion

        #region Appearance

        public string SaveSearchButtonBorderHexColor {
            get {
                if(IsOverSaveSearchButton) {
                    return MpSystemColors.LightGray;
                }
                return MpSystemColors.dimgray;
            }
        }

        public string DeleteSearchButtonBorderHexColor {
            get {
                if (IsOverDeleteSearchButton) {
                    return MpSystemColors.Red;
                }
                return MpSystemColors.LightGray;
            }
        }

        public FontStyle TextBoxFontStyle {
            get {
                if (HasText) {
                    return FontStyle.Normal; //FontStyles.Normal;
                }
                return FontStyle.Italic; //FontStyles.Italic;
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
        public bool IsClearTextButtonVisible {
            get {
                if (HasText && !IsSearching) {
                    return true;// Visibility.Visible;
                }
                return false;// Visibility.Collapsed;
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
                    OnPropertyChanged(nameof(IsClearTextButtonVisible));
                    OnPropertyChanged(nameof(TextBoxFontStyle));
                }
            }
        }

        public MpUserSearch UserSearch { get; set; }

        public MpContentFilterType FilterType {
            get {
                MpContentFilterType ft = MpContentFilterType.None;
                foreach (var sfvm in Filters) {
                    ft |= sfvm.FilterValue;
                }
                return ft;
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler OnSearchTextBoxFocusRequest;

        #endregion

        #region Constructors

        private static MpAvSearchBoxViewModel _instance;
        public static MpAvSearchBoxViewModel Instance => _instance ?? (_instance = new MpAvSearchBoxViewModel());


        public MpAvSearchBoxViewModel() : base(null) {
            PropertyChanged += MpSearchBoxViewModel_PropertyChanged;
        }

        public async Task InitAsync() {
            await Dispatcher.UIThread.InvokeAsync(() => {
                CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;


                ValidateFilters();
                foreach (var sfvm in Filters.Where(x=>!x.IsSeperator)) {
                    sfvm.PropertyChanged += Sfvm_PropertyChanged;
                }


                MpMessenger.Register<MpMessageType>(MpAvClipTrayViewModel.Instance, ReceiveClipTrayViewModelMessage);
            });
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpUserSearch us) {
            IsBusy = true;

            if (us == null) {
                UserSearch = null;
            } else {
                if (us.CriteriaItems == null || us.CriteriaItems.Count == 0) {
                    us = await MpDb.GetItemAsync<MpUserSearch>(us.Id);
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

        private void Sfvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var sfvm = sender as MpAvSearchFilterViewModel;
            switch(e.PropertyName) {
                case nameof(sfvm.IsChecked):
                    ValidateFilters();
                    break;
            }
        }

        private void ValidateFilters() {
            var resfvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByRegex));
            var cssfvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByIsCaseSensitive));
            if (resfvm.IsChecked) {
                cssfvm.IsChecked = false;
                cssfvm.IsEnabled = false;
            } else {
                cssfvm.IsEnabled = true;
            }

            var sbtfvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByTextType));
            var sbifvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByImageType));
            var sbffvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPrefViewModel.Instance.SearchByFileType));

            if(!sbtfvm.IsChecked && !sbifvm.IsChecked && !sbffvm.IsChecked) {
                sbtfvm.IsChecked = sbifvm.IsChecked = sbffvm.IsChecked = true;
            }
        }

        public void NotifyHasMultipleMatches() {
            IsMultipleMatches = true;

            OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
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
                    OnPropertyChanged(nameof(IsClearTextButtonVisible));
                    OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
                    break;
            }
        }

        private void MpSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SearchBoxViewWidth):
                    MpAvTagTrayViewModel.Instance.OnPropertyChanged(nameof(MpAvTagTrayViewModel.Instance.TagTrayWidth));
                    break;
                case nameof(SearchText):
                    Validate();
                    OnPropertyChanged(nameof(IsClearTextButtonVisible));
                    OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
                    break;
                case nameof(IsSearching):
                    OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
                    OnPropertyChanged(nameof(IsClearTextButtonVisible));
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
                if (!MpAvMainWindowViewModel.Instance.IsMainWindowLoading &&
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
                if(MpAvClipTrayViewModel.Instance.TotalTilesInQuery == 0) {
                    IsTextValid = false;
                }
            }
            return IsTextValid;
        }

        private void UpdateRecentSearchTexts() {
            if (!string.IsNullOrEmpty(_text)) {
                var rftl = RecentSearchTexts;
                int recentFindIdx = rftl.IndexOf(_text);
                if (recentFindIdx < 0) {
                    rftl.Insert(0, _text);
                    rftl = new ObservableCollection<string>(rftl.Take(MpPrefViewModel.Instance.MaxRecentTextsCount));
                } else {
                    rftl.RemoveAt(recentFindIdx);
                    rftl.Insert(0, _text);
                }
                RecentSearchTexts = rftl;
            }
        }
        #endregion

        #region Commands

        public ICommand ClearTextCommand => new MpCommand(
            () => {
                IsMultipleMatches = false;
                SearchText = string.Empty;
                if(!string.IsNullOrWhiteSpace(LastSearchText)) {
                    MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                }
                LastSearchText = string.Empty;
            },
            () => {
                return SearchText.Length > 0;
            });

        public ICommand PerformSearchCommand => new MpCommand(
            () => {
                if (!HasText) {
                    IsTextValid = true;
                    LastSearchText = string.Empty;
                } else {
                    IsSearching = true;
                    LastSearchText = SearchText;
                }
                IsMultipleMatches = false;

                MpDataModelProvider.QueryInfo.NotifyQueryChanged();
                UpdateRecentSearchTexts();
            },()=>!MpAvMainWindowViewModel.Instance.IsMainWindowLoading);

        public ICommand NextMatchCommand => new MpCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.SelectNextMatch);
            });

        public ICommand PrevMatchCommand => new MpCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.SelectPreviousMatch);
            });

        public ICommand ClearSearchCriteriaItemsCommand => new MpCommand(
            () => {
                CriteriaItems.Clear();
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            });

        public ICommand AddSearchCriteriaItemCommand => new MpCommand(
            async () => {
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = CriteriaItems.Count
                };
                MpSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                CriteriaItems.Add(nscivm);
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            },()=>CanAddCriteriaItem);

        public ICommand RemoveSearchCriteriaItemCommand => new MpCommand<MpSearchCriteriaItemViewModel>(
            async (scivm) => {
                int scivmIdx = CriteriaItems.IndexOf(scivm);
                CriteriaItems.RemoveAt(scivmIdx);
                if (scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                await UpdateCriteriaSortOrder();
                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            });

        public ICommand SaveSearchCommand => new MpCommand(
            async () => {
                string searchName = UserSearch == null ? string.Empty : UserSearch.Name;
                //searchName = MpTextBoxMessageBox.ShowCustomMessageBox(searchName);
                if(!string.IsNullOrEmpty(searchName)) {
                    if(UserSearch == null) {
                        UserSearch = await MpUserSearch.Create(searchName, DateTime.Now);
                    } else {
                        UserSearch.Name = searchName;
                        await UserSearch.WriteToDatabaseAsync();
                    }
                    OnPropertyChanged(nameof(CanDeleteSearch));
                }
            });

        public ICommand DeleteSearchCommand => new MpCommand(
            async () => {
                await UserSearch.DeleteFromDatabaseAsync();
            },()=>UserSearch != null);

        #endregion
    }
}
