
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchBoxViewModel : MpViewModelBase,
        MpIAsyncSingletonViewModel<MpAvSearchBoxViewModel>,
        MpIExpandableViewModel {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIExpandableViewModel Implementation

        public bool IsExpanded { get; set; } = false;

        #endregion

        #endregion

        #region Properties     

        #region View Models

        private ObservableCollection<string> _recentSearchTexts;
        public ObservableCollection<string> RecentSearchTexts {
            get {
                if (_recentSearchTexts == null &&
                    MpPrefViewModel.Instance != null) {
                    var rstl = MpPrefViewModel.Instance.RecentSearchTexts.ToListFromCsv(MpCsvFormatProperties.DefaultBase64Value);
                    _recentSearchTexts = new ObservableCollection<string>(rstl);
                }
                return _recentSearchTexts;
            }
        }

        private MpAvSearchFilterCollectionViewModel _searchFilterCollectionViewModel;
        public MpAvSearchFilterCollectionViewModel SearchFilterCollectionViewModel =>
            _searchFilterCollectionViewModel ?? (_searchFilterCollectionViewModel = new MpAvSearchFilterCollectionViewModel(this));
        #endregion


        #region Layout
        #endregion

        #region State
        public DateTime IsExpandedChangedDateTime { get; set; }
        public TimeSpan ExpandAnimationTimeSpan =>
            TimeSpan.FromMilliseconds(300);

        public TimeSpan InactivityUnexpandWaitTimeSpan =>
            TimeSpan.FromMilliseconds(1500);

        public bool IsExpandAnimating {
            get {
                if (DateTime.Now - IsExpandedChangedDateTime < ExpandAnimationTimeSpan) {
                    return true;
                }
                return false;
            }
        }

        public bool IsMultipleMatches { get; private set; } = false;

        public string LastSearchText { get; private set; } = string.Empty;

        public bool IsExpandAdvancedSearchButtonVisible =>
            HasText ||
            MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive ||
            IsExpanded ||
            IsExpandAnimating;

        public bool IsTextBoxFocused { get; set; }
        public bool IsExpandButtonFocused { get; set; }

        public bool IsAnySearchControlFocused {
            get {
                if (SearchFilterCollectionViewModel.IsPopupMenuOpen) {
                    return true;
                }

                var cf = FocusManager.Instance.Current;
                if (cf == null) {
                    return false;
                }
                if (cf is Control c && (
                    c.GetVisualAncestor<MpAvSearchBoxView>() != null ||
                   c.GetVisualAncestor<MpAvSearchCriteriaListBoxView>() != null)) {
                    return true;
                }
                return false;
            }
        }

        public bool IsSearchValid {
            get {
                if (IsSearching) {
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
                return Mp.Services.Query.TotalAvailableItemsInQuery > 0;
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

                if (IsExpanded) {
                    return $"Search Options for '{tag_name}'";
                }
                return $"Search '{tag_name}'...";
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
                    //OnPropertyChanged(nameof(TextBoxFontStyle));
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


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await Task.Delay(1);

            SearchFilterCollectionViewModel.Init();
            MpMessenger.RegisterGlobal(ReceiveGlobalMessage);
            OnPropertyChanged(nameof(RecentSearchTexts));
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
                    OnPropertyChanged(nameof(IsExpandAdvancedSearchButtonVisible));
                    break;
                case MpMessageType.AdvancedSearchExpandedChanged:
                //case MpMessageType.AdvancedSearchExpanded:
                case MpMessageType.SearchCriteriaItemsChanged:
                case MpMessageType.TagSelectionChanged:
                    OnPropertyChanged(nameof(IsExpandAdvancedSearchButtonVisible));
                    break;

            }
        }

        private void MpAvSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

                case nameof(SearchText):
                    //if (IsTextBoxFocused) {
                    PerformSearchCommand.Execute(null);
                    //}

                    if (HasText && !IsExpanded) {
                        IsExpanded = true;
                    }
                    if (HasText && SearchText.StartsWith("#")) {
                        var test = MpAvWindowManager.FindByHashCode(SearchText);
                    }
                    break;
                case nameof(IsTextBoxFocused):
                    if (IsTextBoxFocused) {
                        if (!HasText) {
                            SearchText = string.Empty;
                        }

                        Dispatcher.UIThread.Post(async () => {
                            var result = await Mp.Services.NativeMessageBox.ShowYesNoCancelMessageBoxAsync(
                                title: $"Remove associated clips?",
                                message: $"Would you also like to remove all clips from Test'",
                                iconResourceObj: "WarningImage");
                        });
                    } else {

                        if (HasText) {
                            //MpAvThemeViewModel.Instance.GlobalBgOpacity = double.Parse(SearchText);
                        } else if (!SearchFilterCollectionViewModel.IsPopupMenuOpen && !HasText) {
                            // IsExpanded = false;

                            WaitForUnexpandAsync().FireAndForgetSafeAsync(this);
                        }
                    }
                    //OnPropertyChanged(nameof(TextBoxFontStyle));
                    break;
                case nameof(IsExpanded):
                    IsExpandedChangedDateTime = DateTime.Now;
                    OnPropertyChanged(nameof(IsExpandAnimating));
                    Dispatcher.UIThread.Post(async () => {
                        while (IsExpandAnimating) {
                            await Task.Delay(50);
                        }
                        OnPropertyChanged(nameof(IsExpandAnimating));
                        OnPropertyChanged(nameof(IsExpandAdvancedSearchButtonVisible));

                        if (IsExpanded) {
                            if (!MpAvMainWindowViewModel.Instance.IsMainWindowInitiallyOpening) {
                                IsTextBoxFocused = true;
                            }
                            WaitForUnexpandAsync().FireAndForgetSafeAsync(this);
                        }
                    });
                    break;
                case nameof(IsMultipleMatches):
                    if (IsMultipleMatches && !IsExpanded) {
                        ToggleIsSearchBoxExpandedCommand.Execute(null);
                        break;
                    }

                    break;
            }
        }

        private async Task AddOrUpdateRecentSearchTextsAsync(string st) {
            if (string.IsNullOrEmpty(st)) {
                return;
            }
            while (MpPrefViewModel.Instance.IsSaving) {
                await Task.Delay(100);
            }
            int recentFindIdx = RecentSearchTexts.IndexOf(st);
            if (recentFindIdx < 0) {
                RecentSearchTexts.Insert(0, st);
            } else {
                RecentSearchTexts.Move(recentFindIdx, 0);
            }
            int to_remove = RecentSearchTexts.Count - MpPrefViewModel.Instance.MaxRecentTextsCount;
            while (to_remove > 0) {
                RecentSearchTexts.RemoveAt(RecentSearchTexts.Count - 1);
            }
            MpPrefViewModel.Instance.RecentSearchTexts = RecentSearchTexts.ToCsv(MpCsvFormatProperties.DefaultBase64Value);
        }

        private async Task WaitForUnexpandAsync() {
            if (!IsExpanded) {
                return;
            }
            DateTime? no_focus_start_dt = null;
            while (true) {
                if (!IsExpanded || HasText || IsMultipleMatches) {
                    return;
                }
                if (FocusManager.Instance.Current == null) {
                    // focus is null when inactive (or hidden at least)
                    await Task.Delay(1000);
                    continue;
                }
                if (IsAnySearchControlFocused) {
                    no_focus_start_dt = null;
                } else if (no_focus_start_dt == null) {
                    no_focus_start_dt = DateTime.Now;
                }

                if (no_focus_start_dt.HasValue) {
                    if (DateTime.Now - no_focus_start_dt >= InactivityUnexpandWaitTimeSpan) {
                        IsExpanded = false;
                    }
                }
                await Task.Delay(100);
            }
        }
        #endregion

        #region Commands

        public ICommand HandleSearchButtonClickCommand => new MpCommand<object>(
            (args) => {
                if (!IsExpanded) {
                    ToggleIsSearchBoxExpandedCommand.Execute(null);
                    return;
                }
                IsTextBoxFocused = false;
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
                if (!string.IsNullOrWhiteSpace(LastSearchText) && !suppressNotify) {
                    //MpPlatform.Services.QueryInfo.NotifyQueryChanged();
                    //SetQueryInfo();
                    Mp.Services.Query.NotifyQueryChanged();
                }
                LastSearchText = string.Empty;
            },
            (args) => {
                return SearchText.Length > 0;
            });

        public ICommand PerformSearchCommand => new MpCommand(
            () => {
                IsSearching = true;
                LastSearchText = SearchText;
                IsMultipleMatches = false;

                OnPropertyChanged(nameof(IsSearchValid));

                //SetQueryInfo(); 
                Mp.Services.Query.NotifyQueryChanged(true);
                AddOrUpdateRecentSearchTextsAsync(SearchText).FireAndForgetSafeAsync(this);
            }, () => !MpAvMainWindowViewModel.Instance.IsMainWindowLoading);

        public ICommand NextMatchCommand => new MpCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.SelectNextMatch);
            });

        public ICommand PrevMatchCommand => new MpCommand(
            () => {
                MpMessenger.SendGlobal(MpMessageType.SelectPreviousMatch);
            });

        public ICommand ExpandFromDragCommand => new MpCommand(
            () => {
                ToggleIsSearchBoxExpandedCommand.Execute(null);
            }, () => {
                return !IsExpanded;
            });

        #endregion
    }
}
