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

    public class MpSearchBoxViewModel : MpViewModelBase, MpISingletonViewModel<MpSearchBoxViewModel> {
        #region Private Variables
        #endregion

        #region Properties     

        #region View Models

        public ObservableCollection<MpSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpSearchCriteriaItemViewModel>();

        private ObservableCollection<MpSearchFilterViewModel> _filters;
        public ObservableCollection<MpSearchFilterViewModel> Filters {
            get {
                if (_filters == null) {
                    _filters = new ObservableCollection<MpSearchFilterViewModel>() {
                        new MpSearchFilterViewModel(
                            this,
                            "Content",
                            nameof(MpPreferences.SearchByContent),
                            MpContentFilterType.Content),
                        new MpSearchFilterViewModel(
                            this,
                            "Title",
                            nameof(MpPreferences.SearchByTitle),
                            MpContentFilterType.Title),
                        new MpSearchFilterViewModel(
                            this,
                            "Url",
                            nameof(MpPreferences.SearchBySourceUrl),
                            MpContentFilterType.Url),
                        new MpSearchFilterViewModel(
                            this,
                            "Url Title",
                            nameof(MpPreferences.SearchByUrlTitle),
                            MpContentFilterType.UrlTitle),
                        new MpSearchFilterViewModel(
                            this,
                            "Application Path",
                            nameof(MpPreferences.SearchByProcessName),
                            MpContentFilterType.AppPath),
                        new MpSearchFilterViewModel(
                            this,
                            "Application Name",
                            nameof(MpPreferences.SearchByApplicationName),
                            MpContentFilterType.AppName),
                        //new MpSearchFilterViewModel(
                        //    this,
                        //    "Collections",
                        //    nameof(MpPreferences.SearchByTag),
                        //    MpContentFilterType.Tag),
                        new MpSearchFilterViewModel(
                            this,
                            "Description",
                            nameof(MpPreferences.SearchByDescription),
                            MpContentFilterType.Meta),
                        new MpSearchFilterViewModel(this,true),
                        new MpSearchFilterViewModel(
                            this,
                            "Text Type",
                            nameof(MpPreferences.SearchByTextType),
                            MpContentFilterType.TextType),
                        new MpSearchFilterViewModel(
                            this,
                            "File Type",
                            nameof(MpPreferences.SearchByFileType),
                            MpContentFilterType.FileType),
                        new MpSearchFilterViewModel(
                            this,
                            "Image Type",
                            nameof(MpPreferences.SearchByImageType),
                            MpContentFilterType.ImageType),
                        new MpSearchFilterViewModel(this,true),
                        new MpSearchFilterViewModel(
                            this,
                            "Case Sensitive",
                            nameof(MpPreferences.SearchByIsCaseSensitive),
                            MpContentFilterType.CaseSensitive),
                        new MpSearchFilterViewModel(
                            this,
                            "Regular Expression",
                            nameof(MpPreferences.SearchByRegex),
                            MpContentFilterType.Regex)
                    };
                }
                return _filters;
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

        #endregion

        #region Business Logic Properties

        public string PlaceholderText {
            get {
                return MpPreferences.SearchPlaceHolderText;
            }
        }

        #endregion

        #region State

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

        public bool IsOverDeleteSearchButton { get; set; }

        #endregion

        #region Appearance

        public Brush SaveSearchButtonBorderBrush {
            get {
                if(IsOverSaveSearchButton) {
                    return Brushes.LightGray;
                }
                return Brushes.DimGray;
            }
        }

        public Brush DeleteSearchButtonBorderBrush {
            get {
                if (IsOverDeleteSearchButton) {
                    return Brushes.Red;
                }
                return Brushes.LightGray;
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

        private static MpSearchBoxViewModel _instance;
        public static MpSearchBoxViewModel Instance => _instance ?? (_instance = new MpSearchBoxViewModel());


        public MpSearchBoxViewModel() : base(null) {
            PropertyChanged += MpSearchBoxViewModel_PropertyChanged;
        }

        public async Task Init() {
            await MpHelpers.RunOnMainThreadAsync(() => {
                CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;


                ValidateFilters();
                foreach (var sfvm in Filters.Where(x=>!x.IsSeperator)) {
                    sfvm.PropertyChanged += Sfvm_PropertyChanged;
                }


                MpMessenger.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceiveClipTrayViewModelMessage);
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
            var sfvm = sender as MpSearchFilterViewModel;
            switch(e.PropertyName) {
                case nameof(sfvm.IsChecked):
                    ValidateFilters();
                    break;
            }
        }

        private void ValidateFilters() {
            var resfvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPreferences.SearchByRegex));
            var cssfvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPreferences.SearchByIsCaseSensitive));
            if (resfvm.IsChecked) {
                cssfvm.IsChecked = false;
                cssfvm.IsEnabled = false;
            } else {
                cssfvm.IsEnabled = true;
            }

            var sbtfvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPreferences.SearchByTextType));
            var sbifvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPreferences.SearchByImageType));
            var sbffvm = Filters.FirstOrDefault(x => x.PreferenceName == nameof(MpPreferences.SearchByFileType));

            if(!sbffvm.IsChecked && !sbifvm.IsChecked && !sbffvm.IsChecked) {
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
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
                    break;
            }
        }

        private void MpSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SearchText):
                    Validate();
                    OnPropertyChanged(nameof(ClearTextButtonVisibility));
                    OnPropertyChanged(nameof(ClearAndAddCriteriaColumnWidth));
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
                if(MpClipTrayViewModel.Instance.TotalTilesInQuery == 0) {
                    IsTextValid = false;
                }
            }
            return IsTextValid;
        }
        #endregion

        #region Commands

        public ICommand ClearTextCommand => new RelayCommand(
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

        public ICommand PerformSearchCommand => new RelayCommand(
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
            },!MpMainWindowViewModel.Instance.IsMainWindowLoading);

        public ICommand NextMatchCommand => new RelayCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.SelectNextMatch);
            });

        public ICommand PrevMatchCommand => new RelayCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.SelectPreviousMatch);
            });

        public ICommand ClearSearchCriteriaItemsCommand => new RelayCommand(
            () => {
                CriteriaItems.Clear();
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            });

        public ICommand AddSearchCriteriaItemCommand => new RelayCommand(
            async () => {
                MpSearchCriteriaItem nsci = new MpSearchCriteriaItem() {
                    SortOrderIdx = CriteriaItems.Count
                };
                MpSearchCriteriaItemViewModel nscivm = await CreateCriteriaItemViewModel(nsci);
                CriteriaItems.Add(nscivm);
                OnPropertyChanged(nameof(CriteriaItems));
                OnPropertyChanged(nameof(HasCriteriaItems));
                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            },CanAddCriteriaItem);

        public ICommand RemoveSearchCriteriaItemCommand => new RelayCommand<MpSearchCriteriaItemViewModel>(
            async (scivm) => {
                int scivmIdx = CriteriaItems.IndexOf(scivm);
                CriteriaItems.RemoveAt(scivmIdx);
                if (scivm.SearchCriteriaItem.Id > 0) {
                    await scivm.SearchCriteriaItem.DeleteFromDatabaseAsync();
                }
                await UpdateCriteriaSortOrder();
                MpMessenger.SendGlobal<MpMessageType>(MpMessageType.SearchCriteriaItemsChanged);
            });

        public ICommand SaveSearchCommand => new RelayCommand(
            async () => {
                string searchName = UserSearch == null ? string.Empty : UserSearch.Name;
                searchName = MpTextBoxMessageBox.ShowCustomMessageBox(searchName);
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

        public ICommand DeleteSearchCommand => new RelayCommand(
            async () => {
                await UserSearch.DeleteFromDatabaseAsync();
            },UserSearch != null);

        #endregion
    }
}
