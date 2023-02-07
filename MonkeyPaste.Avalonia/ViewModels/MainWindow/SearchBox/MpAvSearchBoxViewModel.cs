
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
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchBoxViewModel : MpViewModelBase, 
        MpIAsyncSingletonViewModel<MpAvSearchBoxViewModel>,
        MpIExpandableViewModel,
        MpIQueryInfoValueProvider {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIExpandableViewModel Implementation

        public bool IsExpanded { get; set; }

        #endregion

        #region MpIQueryInfoProvider Implementation
        object MpIQueryInfoValueProvider.Source => this;
        string MpIQueryInfoValueProvider.SourcePropertyName => nameof(SearchText);

        string MpIQueryInfoValueProvider.QueryValueName => nameof(MpIQueryInfo.MatchValue);

        #endregion

        #endregion

        #region Properties     

        #region View Models

        //public ObservableCollection<MpAvSearchCriteriaItemViewModel> CriteriaItems { get; set; } = new ObservableCollection<MpAvSearchCriteriaItemViewModel>();

        private MpAvSearchFilterCollectionViewModel _searchFilterCollectionViewModel;
        public MpAvSearchFilterCollectionViewModel SearchFilterCollectionViewModel => _searchFilterCollectionViewModel ?? (_searchFilterCollectionViewModel = new MpAvSearchFilterCollectionViewModel(this));
        #endregion


        #region Layout
        #endregion

        #region State
        public DateTime IsExpandedChangedDateTime { get; set; }
        public TimeSpan ExpandAnimationTimeSpan =>
            TimeSpan.FromMilliseconds(300);

        public bool IsExpandAnimating {
            get {
                if(DateTime.Now - IsExpandedChangedDateTime > ExpandAnimationTimeSpan) {
                    return false;
                }
                return true;
            }
        }

        private ObservableCollection<string> _recentSearchTexts;
        public ObservableCollection<string> RecentSearchTexts {
            get {
                if(_recentSearchTexts == null) {
                    var rstl = MpPrefViewModel.Instance.RecentSearchTexts.ToListFromCsv(MpPrefViewModel.Instance);
                    _recentSearchTexts = new ObservableCollection<string>(rstl);
                    _recentSearchTexts.CollectionChanged += _recentSearchTexts_CollectionChanged;
                }
                return _recentSearchTexts;
            }
        }

        private void _recentSearchTexts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            MpPrefViewModel.Instance.RecentSearchTexts =
                RecentSearchTexts.ToCsv(MpPrefViewModel.Instance);
        }

        public bool IsMultipleMatches { get; private set; } = false;


        public bool CanAddCriteriaItem => true;//!string.IsNullOrEmpty(LastSearchText) && !IsSearching;

        public string LastSearchText { get; private set; } = string.Empty;

        public bool IsToAdvancedButtonVisible =>
            HasText || MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive;
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
                return MpPlatform.Services.Query.TotalAvailableItemsInQuery > 0;
            }
        }


        public bool IsSearching { get; set; }

        public bool HasText => SearchText.Length > 0;

        #endregion

        #region Appearance

        public string SearchIconToolTipText {
            get {
                string tag_name =
                    MpAvTagTrayViewModel.Instance.SelectedItem == null ? string.Empty : MpAvTagTrayViewModel.Instance.SelectedItem.TagName;

                if(IsExpanded) {
                    return $"Search Options for '{tag_name}'";
                }
                return $"Search '{tag_name}'...";
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
                    OnPropertyChanged(nameof(IsToAdvancedButtonVisible));
                    break;
                case MpMessageType.AdvancedSearchUnexpanded:
                case MpMessageType.AdvancedSearchExpanded:
                case MpMessageType.SearchCriteriaItemsChanged:
                case MpMessageType.TagSelectionChanged:
                    OnPropertyChanged(nameof(IsToAdvancedButtonVisible));
                    break;

            }
        }

        private void MpAvSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
               
                case nameof(SearchText):
                    if(IsTextBoxFocused) {
                        PerformSearchCommand.Execute(null);
                    }

                    if(HasText && !IsExpanded) {
                        IsExpanded = true;
                    }
                    break;
                case nameof(IsSearching):
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
                        } else if(!SearchFilterCollectionViewModel.IsPopupMenuOpen) {
                            IsExpanded = false;
                        }
                    }
                    OnPropertyChanged(nameof(TextBoxFontStyle));
                    break;
                case nameof(IsExpanded):
                    IsExpandedChangedDateTime = DateTime.Now;
                    OnPropertyChanged(nameof(IsExpandAnimating));
                    Dispatcher.UIThread.Post(async () => {
                        while(IsExpandAnimating) {
                            await Task.Delay(50);
                        }
                        OnPropertyChanged(nameof(IsExpandAnimating));

                        if(IsExpanded && 
                            !MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening) {
                            IsTextBoxFocused = true;
                        }
                    });
                    break;
            }
        }

        private void UpdateRecentSearchTexts(string st) {
            if (!string.IsNullOrEmpty(st)) {
                int recentFindIdx = RecentSearchTexts.IndexOf(st);
                if (recentFindIdx < 0) {
                    RecentSearchTexts.Insert(0, st);
                } else {
                    RecentSearchTexts.RemoveAt(recentFindIdx);
                    RecentSearchTexts.Insert(0, st);
                }
                int to_remove = RecentSearchTexts.Count - MpPrefViewModel.Instance.MaxRecentTextsCount;
                if (to_remove > 0) {
                    RecentSearchTexts.RemoveRange(MpPrefViewModel.Instance.MaxRecentTextsCount - 1, to_remove);
                }
            }
        }
        #endregion

        #region Commands

        public ICommand HandleSearchButtonClickCommand => new MpCommand<object>(
            (args) => {
                if(!IsExpanded) {
                    ToggleIsSearchBoxExpandedCommand.Execute(null);
                    return;
                }
                var target_control = args as Control;
                MpAvMenuExtension.ShowMenu(
                    target_control,
                    SearchFilterCollectionViewModel.PopupMenuViewModel,

                    placement: PlacementMode.Pointer);
            });

        public ICommand ToggleIsSearchBoxExpandedCommand => new MpCommand(
            () => {
                IsExpanded = !IsExpanded;
            });

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
                UpdateRecentSearchTexts(SearchText);
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
