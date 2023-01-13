
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {

    public class MpAvSearchBoxViewModel : MpViewModelBase, 
        MpIAsyncSingletonViewModel<MpAvSearchBoxViewModel>,
        MpIQueryInfoValueProvider {
        #region Private Variables
        #endregion

        #region Properties     

        #region View Models
        public ObservableCollection<MpSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpSearchCriteriaItemViewModel>();

        private MpAvSearchFilterCollectionViewModel _searchFilterCollectionViewModel;
        public MpAvSearchFilterCollectionViewModel SearchFilterCollectionViewModel => _searchFilterCollectionViewModel ?? (_searchFilterCollectionViewModel = new MpAvSearchFilterCollectionViewModel(this));
        #endregion

        #region MpIQueryInfoProvider Implementation

        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(SearchText);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpAvQueryInfoViewModel.Current.SearchText);

        #endregion

        #region Layout

        public GridLength ClearAndBusyColumnWidth {
            get {
                double w = 0;
                if (string.IsNullOrEmpty(LastSearchText) || IsSearching || HasText) {
                    w = 15;
                }
                return new GridLength(w, GridUnitType.Pixel);
            }
        }

        public GridLength NavButtonsColumnWidth {
            get {
                double w = 0;
                if (IsMultipleMatches) {
                    w = 20;
                }
                return new GridLength(w, GridUnitType.Pixel);
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

        public bool IsSearchValid { 
            get {
                if(IsSearching) {
                    return true;
                }
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return true;
                }
                if (MpAvTagTrayViewModel.Instance.SelectedItem == null) {
                    return true;
                } 
                if (MpAvTagTrayViewModel.Instance.SelectedItem.TagClipCount == 0) {
                    return true;
                } 
                // when current tag has items but current search criteria produces no result mark as invalid
                return MpAvClipTrayViewModel.Instance.TotalTilesInQuery > 0;
            }
        }


        public bool IsSearching { get; set; }

        public bool HasText => SearchText.Length > 0;

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
                    OnPropertyChanged(nameof(TextBoxFontStyle));
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

        private static MpAvSearchBoxViewModel _instance;
        public static MpAvSearchBoxViewModel Instance => _instance ?? (_instance = new MpAvSearchBoxViewModel());


        public MpAvSearchBoxViewModel() : base(null) {
            PropertyChanged += MpSearchBoxViewModel_PropertyChanged;
        }

        public async Task InitAsync() {
            await Dispatcher.UIThread.InvokeAsync(() => {
                MpAvQueryInfoViewModel.Current.RegisterProvider(this);
                CriteriaItems.CollectionChanged += CriteriaItems_CollectionChanged;

                SearchFilterCollectionViewModel.Init();


                MpMessenger.RegisterGlobal(ReceiveGlobalMessage);
            });
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpUserSearch us) {
            IsBusy = true;

            if (us == null) {
                UserSearch = null;
            } else {
                var cil = await MpDataModelProvider.GetCriteriaItemsByUserSearchId(us.Id);
                UserSearch = us;
                CriteriaItems.Clear();
                foreach (var ci in cil) {
                    var civm = await CreateCriteriaItemViewModel(ci);
                    CriteriaItems.Add(civm);
                }
            }

            OnPropertyChanged(nameof(CriteriaItems));
            OnPropertyChanged(nameof(HasCriteriaItems));

            IsBusy = false;
        }

        

        public void NotifyHasMultipleMatches() {
            Dispatcher.UIThread.VerifyAccess();
            IsMultipleMatches = true;

            OnPropertyChanged(nameof(ClearAndBusyColumnWidth));
            OnPropertyChanged(nameof(NavButtonsColumnWidth));
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

        private void ReceiveGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    IsSearching = false;
                    OnPropertyChanged(nameof(IsSearchValid));
                    break;

            }
        }

        private void MpSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
               
                case nameof(SearchText):
                    OnPropertyChanged(nameof(ClearAndBusyColumnWidth));
                    OnPropertyChanged(nameof(NavButtonsColumnWidth));
                    break;
                case nameof(IsSearching):
                    OnPropertyChanged(nameof(ClearAndBusyColumnWidth));
                    OnPropertyChanged(nameof(NavButtonsColumnWidth));
                    OnPropertyChanged(nameof(CanAddCriteriaItem));
                    break;
                case nameof(IsTextBoxFocused):
                    if (IsTextBoxFocused) {
                        if (!HasText) {
                            SearchText = string.Empty;
                        }
                    } else {

                        if (HasText) {
                            //MpAvThemeViewModel.Instance.GlobalBgOpacity = double.Parse(SearchText);
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
                    //MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
                    //SetQueryInfo();
                    MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
                }
                LastSearchText = string.Empty;
            },
            () => {
                return SearchText.Length > 0;
            });

        public ICommand PerformSearchCommand => new MpCommand(
            () => {
                //if (!HasText) {
                //    LastSearchText = string.Empty;
                //} else {
                //    IsSearching = true;
                //    LastSearchText = SearchText;
                //}
                IsSearching = true;
                LastSearchText = SearchText;
                IsMultipleMatches = false;

                OnPropertyChanged(nameof(IsSearchValid));

                //SetQueryInfo(); 
                MpAvQueryInfoViewModel.Current.NotifyQueryChanged();
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
