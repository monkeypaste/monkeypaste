
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchBoxViewModel : MpAvViewModelBase,
        MpIExpandableViewModel,
        MpIHighlightTextRangesInfoViewModel {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIHighlightTextRangesInfoViewModel Implementation

        ObservableCollection<MpTextRange> MpIHighlightTextRangesInfoViewModel.HighlightRanges { get; } = new ObservableCollection<MpTextRange>();
        int MpIHighlightTextRangesInfoViewModel.ActiveHighlightIdx { get; set; } = -1;

        #endregion

        #region MpIExpandableViewModel Implementation

        public bool IsExpanded { get; set; } = false;

        #endregion

        #endregion

        #region Properties     

        #region View Models

        public IList<string> RecentSearchTexts { get; private set; }

        private MpAvSearchFilterCollectionViewModel _searchFilterCollectionViewModel;
        public MpAvSearchFilterCollectionViewModel SearchFilterCollectionViewModel =>
            _searchFilterCollectionViewModel ?? (_searchFilterCollectionViewModel = new MpAvSearchFilterCollectionViewModel(this));
        #endregion


        #region Layout
        #endregion

        #region State

        public bool IsDragExpanded { get; set; } = false;
        public bool IsExpandAnimating { get; private set; }

        public bool IsMultipleMatches { get; private set; } = false;

        public string LastSearchText { get; private set; } = string.Empty;

        public bool IsExpandAdvancedSearchButtonVisible =>
            HasText ||
            MpAvSearchCriteriaItemCollectionViewModel.Instance.IsAdvSearchActive ||
            IsExpanded ||
            IsExpandAnimating;


        public bool IsAnySearchControlFocused {
            get {
                if (SearchFilterCollectionViewModel.IsPopupMenuOpen) {
                    return true;
                }

                if (Mp.Services.FocusMonitor.FocusElement is Control c && (
                    c.TryGetSelfOrAncestorDataContext<MpAvSearchBoxViewModel>(out _) ||
                    c.TryGetSelfOrAncestorDataContext<MpAvSearchCriteriaItemCollectionViewModel>(out _))) {
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

        public string ExpandTooltipText {
            get {
                if (MpAvSearchCriteriaItemCollectionViewModel.Instance.IsCriteriaWindowOpen) {
                    return "Restore Filters";
                }
                if (IsExpanded) {
                    return "Hide Filters";
                }
                if (!MpAvSearchCriteriaItemCollectionViewModel.Instance.IsSavedQuery) {
                    return "Create Filter";
                }
                return "Show Filters";
            }
        }
        #endregion

        #region Model

        private string _searchText = string.Empty;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                if (_searchText != value) {
                    _searchText = value;
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

        public async Task InitializeAsync() {
            IsBusy = true;
            await AddOrUpdateRecentSearchTextsAsync(null);

            SearchFilterCollectionViewModel.Init();
            MpMessenger.RegisterGlobal(ReceiveGlobalMessage);
            OnPropertyChanged(nameof(RecentSearchTexts));

            IsBusy = false;
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
                case MpMessageType.QueryCompleted:
                case MpMessageType.RequeryCompleted:
                    IsSearching = false;
                    OnPropertyChanged(nameof(IsSearchValid));
                    OnPropertyChanged(nameof(IsExpandAdvancedSearchButtonVisible));
                    break;
                case MpMessageType.AdvancedSearchExpandedChanged:
                case MpMessageType.SearchCriteriaItemsChanged:
                case MpMessageType.TagSelectionChanged:
                    OnPropertyChanged(nameof(IsExpandAdvancedSearchButtonVisible));
                    break;
            }
        }

        private void MpAvSearchBoxViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {

                case nameof(SearchText):
                    PerformSearchCommand.Execute(null);

                    if (HasText && !IsExpanded) {
                        IsExpanded = true;
                    }
#if DEBUG
                    if (HasText && SearchText.StartsWith("#")) {
                        var test = MpAvWindowManager.FindByHashCode(SearchText);
                    }
#endif
                    break;
                case nameof(IsExpanded):
                    if (IsExpanded && MpAvMainWindowViewModel.Instance.IsVerticalOrientation &&
                           MpAvClipTileSortDirectionViewModel.Instance.IsExpanded) {
                        MpAvClipTileSortDirectionViewModel.Instance.IsExpanded = false;
                    }
                    var expanded_changed_sw = Stopwatch.StartNew();

                    OnPropertyChanged(nameof(IsExpandAnimating));
                    MpMessenger.SendGlobal(MpMessageType.FilterItemSizeChanged);
                    Dispatcher.UIThread.Post(async () => {
                        IsExpandAnimating = true;
                        await Task.Delay(MpAvFilterMenuViewModel.Instance.FilterAnimTimeMs);
                        IsExpandAnimating = false;
                        OnPropertyChanged(nameof(IsExpandAdvancedSearchButtonVisible));
                        MpMessenger.SendGlobal(MpMessageType.FilterItemSizeChanged);
                        if (IsExpanded) {
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
            while (MpAvPrefViewModel.Instance == null) {
                await Task.Delay(100);
            }
            RecentSearchTexts = await MpAvPrefViewModel.Instance.AddOrUpdateAutoCompleteTextAsync(nameof(MpAvPrefViewModel.Instance.RecentSearchTexts), st);
        }

        private async Task WaitForUnexpandAsync() {
            //DateTime? no_focus_start_dt = null;
            while (true) {
                if (!IsExpanded) {
                    return;
                }
                if (HasText ||
                    IsMultipleMatches ||
                    IsDragExpanded ||
                    SearchFilterCollectionViewModel.IsPopupMenuOpen ||
                    (MpAvSearchCriteriaItemCollectionViewModel.Instance.IsExpanded &&
                     !MpAvSearchCriteriaItemCollectionViewModel.Instance.IsCriteriaWindowOpen)) {
                    await Task.Delay(1000);
                    continue;
                }

                if (!IsAnySearchControlFocused) {
                    IsExpanded = false;
                }
                await Task.Delay(100);
            }
        }
        #endregion

        #region Commands

        public ICommand BeginAutoSearchCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (MpAvWindowManager.MainWindow.GetVisualDescendant<MpAvSearchBoxView>() is MpAvSearchBoxView sbv &&
                   sbv.FindControl<AutoCompleteBox>("SearchBox") is AutoCompleteBox acb &&
                   acb.GetTemplateChildren().OfType<TextBox>().FirstOrDefault() is TextBox tb) {
                    // NOTE for best performance avoid using binding to set search text 
                    // otherwise search would trigger on 1st character
                    // so using actual control to mimic typical search

                    IsExpanded = true;

                    bool success = await tb.TrySetFocusAsync(NavigationMethod.Pointer);
                    MpConsole.WriteLine($"Auto search focus success: {success}");
                }

            });

        public ICommand HandleSearchFilterButtonClickCommand => new MpCommand<object>(
            (args) => {
                if (!IsExpanded) {
                    ToggleIsSearchBoxExpandedCommand.Execute(null);
                    return;
                }
                var target_control = args as Control;

                MpAvMenuExtension.ShowMenu(
                    target_control,
                    SearchFilterCollectionViewModel.PopupMenuViewModel,
                    hideOnClick: false,
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
                Dispatcher.UIThread.Post(async () => {
                    IsDragExpanded = true;
                    while (MpAvShortcutCollectionViewModel.Instance.GlobalIsMouseLeftButtonDown) {
                        await Task.Delay(100);
                    }
                    IsDragExpanded = false;
                });
                ToggleIsSearchBoxExpandedCommand.Execute(null);
            }, () => {
                return !IsExpanded;
            });


        #endregion
    }
}
