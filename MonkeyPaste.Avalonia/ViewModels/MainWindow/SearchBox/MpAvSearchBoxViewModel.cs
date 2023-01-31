
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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

        //public ObservableCollection<MpAvSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpAvSearchCriteriaItemViewModel>();

        private MpAvSearchFilterCollectionViewModel _searchFilterCollectionViewModel;
        public MpAvSearchFilterCollectionViewModel SearchFilterCollectionViewModel => _searchFilterCollectionViewModel ?? (_searchFilterCollectionViewModel = new MpAvSearchFilterCollectionViewModel(this));
        #endregion

        #region MpIQueryInfoProvider Implementation
        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(SearchText);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpIQueryInfo.MatchValue);

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

        

        #endregion

        #region State
        public ObservableCollection<string> RecentSearchTexts {
            get => new ObservableCollection<string>(MpPrefViewModel.Instance.RecentSearchTexts.Split(new string[] { MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN }, StringSplitOptions.RemoveEmptyEntries));
            set => MpPrefViewModel.Instance.RecentSearchTexts = string.Join(MpPrefViewModel.STRING_ARRAY_SPLIT_TOKEN, value);
        }

        public bool IsMultipleMatches { get; private set; } = false;


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

        #endregion

        #region Appearance


        public FontStyle TextBoxFontStyle {
            get {
                if (HasText) {
                    return FontStyle.Normal; //FontStyles.Normal;
                }
                return FontStyle.Italic; //FontStyles.Italic;
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


        #endregion

        #endregion

        #region Events

        public event EventHandler OnSearchTextBoxFocusRequest;

        #endregion

        #region Constructors

        private static MpAvSearchBoxViewModel _instance;
        public static MpAvSearchBoxViewModel Instance => _instance ?? (_instance = new MpAvSearchBoxViewModel());


        public MpAvSearchBoxViewModel() : base(null) {
            PropertyChanged += MpAvSearchBoxViewModel_PropertyChanged;
        }

        public async Task InitAsync() {
            await Task.Delay(1);

            MpPlatform.Services.Query.RegisterProvider(this);

            SearchFilterCollectionViewModel.Init();


            MpMessenger.RegisterGlobal(ReceiveGlobalMessage);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int currentQueryTagId) {
            
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


        private void ReceiveGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.RequeryCompleted:
                    IsSearching = false;
                    OnPropertyChanged(nameof(IsSearchValid));
                    if(MpAvTagTrayViewModel.Instance.SelectedItem.TagType == MpTagType.Query) {

                    }
                    break;

            }
        }

        private void MpAvSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
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

        public ICommand ClearTextCommand => new MpCommand<object>(
            (args) => {
                bool suppressNotify = args != null;
                IsMultipleMatches = false;
                SearchText = string.Empty;
                if(!string.IsNullOrWhiteSpace(LastSearchText) && !suppressNotify) {
                    //MpPlatform.Services.QueryInfo.NotifyQueryChanged();
                    //SetQueryInfo();
                    MpPlatform.Services.Query.NotifyQueryChanged();
                }
                LastSearchText = string.Empty;
            },
            (args) => {
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
                MpPlatform.Services.Query.NotifyQueryChanged();
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



        #endregion
    }
}
