using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpPluginBrowserTabType {
        Browse,
        Installed,
        Updates
    }
    public class MpAvPluginBrowserViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel,
        MpIWantsTopmostWindowViewModel,
        MpAvIHeaderMenuViewModel {
        #region Private Variables
        #endregion

        #region Constants      


        #endregion

        #region Statics


        private static MpAvPluginBrowserViewModel _instance;
        public static MpAvPluginBrowserViewModel Instance => _instance ?? (_instance = new MpAvPluginBrowserViewModel());
        #endregion

        #region Interfaces

        #region MpAvIHeaderMenuViewModel Implementation
        IBrush MpAvIHeaderMenuViewModel.HeaderBackground => 
            MpAvThemeViewModel.Instance.IsThemeDark ?
                    Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeLightColor) :
                    Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeDarkColor);
        IBrush MpAvIHeaderMenuViewModel.HeaderForeground =>
            (this as MpAvIHeaderMenuViewModel).HeaderBackground.ToHex().ToContrastForegoundColor().ToAvBrush();
        string MpAvIHeaderMenuViewModel.HeaderTitle =>
            UiStrings.PluginBrowserWindowTitle;
        IEnumerable<MpAvIMenuItemViewModel> MpAvIHeaderMenuViewModel.HeaderMenuItems =>
            null;
        ICommand MpAvIHeaderMenuViewModel.BackCommand =>
            BackCommand;
        object MpAvIHeaderMenuViewModel.BackCommandParameter =>
            null;

        #endregion

        #region MpIWantsTopmostWindowViewModel Implementation
        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        #endregion
        #region MpIChildWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }
        public MpWindowType WindowType =>
            MpWindowType.PopOut;

        #endregion

        #endregion

        #region Properties

        #region View Models

        private ObservableCollection<string> _tabs;
        public ObservableCollection<string> Tabs {
            get {
                if (_tabs == null) {
                    _tabs = new ObservableCollection<string>(typeof(MpPluginBrowserTabType).EnumToUiStrings());
                }
                return _tabs;
            }
        }
        public ObservableCollection<MpManifestFormat> AllManifests { get; set; } = [];
        public IList<string> RecentPluginSearches { get; private set; }

        public ObservableCollection<MpAvPluginItemViewModel> Items { get; private set; } = new ObservableCollection<MpAvPluginItemViewModel>();

        public IEnumerable<MpAvPluginItemViewModel> FilteredItems =>
            Items.Where(x => x.IsVisible).ToList();

        public MpAvPluginItemViewModel SelectedItem { get; set; }
        #endregion

        #region Appearance

        public string WindowTitle {
            get {
                if(SelectedItem == null) {
                    return UiStrings.PluginBrowserWindowTitle;
                }
                return UiStrings.PluginBrowserFormattedWindowTitle.Format(SelectedItem.PluginTitle);
            }
        }

        #endregion

        #region State

        public int CanUpdateCount =>
            Items.Where(x => x.CanUpdate).Count();
        public MpPluginBrowserTabType SelectedTabType =>
            (MpPluginBrowserTabType)SelectedTabIdx;

        public int SelectedTabIdx { get; set; } = (int)MpPluginBrowserTabType.Installed;

        public string FilterText { get; set; } = string.Empty;

        public WindowState WindowState { get; set; }
        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsBusy);

        public bool IsSelectedBusy =>
            SelectedItem != null && SelectedItem.IsAnyBusy;
        public bool IsSelectedDownloading =>
            SelectedItem != null && SelectedItem.IsDownloading;

        public bool IsInitialized { get; set; } = false;

        #endregion

        #endregion

        #region Constructors
        public MpAvPluginBrowserViewModel() {
            PropertyChanged += MpAvPluginBrowserViewModel_PropertyChanged;
            AddOrUpdateRecentFilterTextsAsync(null).FireAndForgetSafeAsync(this);
        }


        #endregion

        #region Public Methods
        public void RefreshItemsState() {
            Items.ForEach(x => x.RefreshState());
            OnPropertyChanged(nameof(CanUpdateCount));
            OnPropertyChanged(nameof(FilteredItems));

            bool is_mobile =
                MpAvThemeViewModel.Instance.IsMobile
                || true; // only for testing

            if(is_mobile) {
                SelectedItem = null;
            } else {
                SelectedItem = FilteredItems.FirstOrDefault();
            }
            
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvPluginBrowserViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(FilterText):
                    PerformFilterCommand.Execute(null);
                    break;
                case nameof(SelectedTabIdx):
                    PerformFilterCommand.Execute(null);
                    RefreshItemsState();
                    break;
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    OnPropertyChanged(nameof(IsSelectedBusy));
                    OnPropertyChanged(nameof(WindowTitle));
                    if(this is MpAvIHeaderMenuViewModel hmivm) {
                        hmivm.OnPropertyChanged(nameof(hmivm.HeaderTitle));
                    }
                    break;
                case nameof(IsAnyBusy):
                    OnPropertyChanged(nameof(IsSelectedBusy));
                    break;
                case nameof(IsWindowOpen):
                    if (!IsWindowOpen) {
                        break;
                    }
                    PerformFilterCommand.Execute(null);
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(FilteredItems));
        }
        private async Task<MpAvPluginItemViewModel> CreatePluginItemViewModelAsync(string plugin_guid) {
            var pivm = new MpAvPluginItemViewModel(this);
            await pivm.InitializeAsync(plugin_guid);
            return pivm;
        }

        private async Task AddOrUpdateRecentFilterTextsAsync(string st) {
            while (MpAvPrefViewModel.Instance == null) {
                await Task.Delay(100);
            }
            RecentPluginSearches = await MpAvPrefViewModel.Instance.AddOrUpdateAutoCompleteTextAsync(nameof(MpAvPrefViewModel.Instance.RecentPluginSearchTexts), st);
        }

        private async Task<IEnumerable<MpManifestFormat>> GetRemoteManifestsAsync() {
            try {
                int timeout_ms = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
                string ledger_uri = await GetLocalizedLedgerUriAsync(timeout_ms);
                string ledger_json = await MpFileIo.ReadTextFromUriAsync(ledger_uri, timeoutMs: timeout_ms);
                var ledger = ledger_json.DeserializeObject<MpManifestLedger>();
                if (ledger == null || ledger.manifests == null) {
                    return Array.Empty<MpManifestFormat>();
                }
                // TODO (should try to avoid creating this problem but in case) filter plugins published before some breaking app
                // version here too
                return ledger.manifests.Where(x => MpPluginLoader.ValidatePluginDependencies(x));
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error reading ledger. ", ex);
                return new List<MpManifestFormat>();
            }
        }
        private async Task<string> GetLocalizedLedgerUriAsync(int timeout_ms) {
            string ledger_index_uri = MpLedgerConstants.USE_LOCAL_LEDGER ?
                MpLedgerConstants.LOCAL_LEDGER_INDEX_URI :
                MpLedgerConstants.REMOTE_LEDGER_INDEX_URI;

            // read ledger index
            string ledger_index_json = await MpFileIo.ReadTextFromUriAsync(ledger_index_uri, timeoutMs: timeout_ms);
            var avail_cultures = ledger_index_json.DeserializeObject<List<string>>();

            // find closest culture
            string cc = MpLocalizationHelpers.FindClosestCultureCode(
                MpAvCurrentCultureViewModel.Instance.CurrentCulture.Name,
                avail_cultures.ToArray());

            // init ledger uri to inv
            string ledger_uri = MpLedgerConstants.USE_LOCAL_LEDGER ?
                MpLedgerConstants.LOCAL_INV_LEDGER_URI :
                MpLedgerConstants.REMOTE_INV_LEDGER_URI;
            if (!string.IsNullOrEmpty(cc)) {
                // localized ledger exists
                string cultures_base_uri = MpLedgerConstants.USE_LOCAL_LEDGER ?
                    MpLedgerConstants.LOCAL_CULTURES_DIR_URI :
                    MpLedgerConstants.REMOTE_CULTURES_DIR_URI;
                string ledger_prefix = MpLedgerConstants.USE_LOCAL_LEDGER ?
                    MpLedgerConstants.LOCAL_LEDGER_PREFIX :
                    MpLedgerConstants.REMOTE_LEDGER_PREFIX;

                ledger_uri = $"{cultures_base_uri}/{ledger_prefix}.{cc}.{MpLedgerConstants.LEDGER_EXT}";
            }
            return ledger_uri;
        }
        private MpAvWindow CreatePluginBrowserWindow(string selectedGuid) {
            MpAvWindow pbw = new MpAvWindow() {
                Width = MpAvThemeViewModel.Instance.IsMobileOrWindowed ? double.NaN : 800,
                Height = MpAvThemeViewModel.Instance.IsMobileOrWindowed ? double.NaN : 500,
                DataContext = this,
                ShowInTaskbar = true,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("JigsawImage", typeof(MpAvWindowIcon), null, null) as MpAvWindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new MpAvPluginBrowserView()
            };
            pbw.Background =
                MpAvThemeViewModel.Instance.IsThemeDark ?
                    Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeDarkColor) :
                    Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeLightColor);

            if (pbw.Content is MpAvPluginBrowserView pbv &&
                pbv.FindControl<TabStrip>("PluginTabStrip") is TabStrip ts) {
                ts.SelectedItem = Tabs[(int)MpPluginBrowserTabType.Installed];
            }

            pbw.Bind(
                Window.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowTitle),
                    Converter = MpAvStringToWindowTitleConverter.Instance
                });

            pbw.Bind(
                Window.WindowStateProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowState),
                    Mode = BindingMode.TwoWay
                });

            if (!string.IsNullOrEmpty(selectedGuid)) {
                pbw.Opened += async (s, e) => {
                    while (IsBusy) {
                        await Task.Delay(100);
                    }
                    if (Items.FirstOrDefault(x => x.PluginGuid == selectedGuid) is MpAvPluginItemViewModel pivm) {
                        SelectedItem = pivm;
                    }
                };
            }
            return pbw;
        }
        public void OpenPluginBrowserWindow(string selectedGuid) {
            if (IsWindowOpen) {
                if (WindowState == WindowState.Minimized) {
                    WindowState = WindowState.Normal;
                }
                return;
            }

            var pbw = CreatePluginBrowserWindow(selectedGuid);
            pbw.Show();
            OnPropertyChanged(nameof(IsWindowOpen));
        }
        private async Task CreateAllItemsAsync() {
            Items.Clear();
            AllManifests.Clear();
            AllManifests.AddRange(await GetRemoteManifestsAsync());
            AllManifests.AddRange(MpPluginLoader.PluginManifestLookup.Select(x => x.Value));

            foreach (var mf in AllManifests.OrderBy(x => x.title).GroupBy(x => x.guid)) {
                var pivm = await CreatePluginItemViewModelAsync(mf.Key);
                Items.Add(pivm);
            }
            RefreshItemsState();

        }
        #endregion

        #region Commands


        public MpIAsyncCommand<object> PerformFilterCommand => new MpAsyncCommand<object>(
            async (args) => {
                while (IsBusy) {
                    await Task.Delay(100);
                }

                IsBusy = true;
                if (!Items.Any() || args != null) {
                    await CreateAllItemsAsync();
                } else {
                    RefreshItemsState();
                }
                IsBusy = false;
                await AddOrUpdateRecentFilterTextsAsync(FilterText);
            });

        public ICommand OpenPluginFolderCommand => new MpCommand(
            () => {
                MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(MpPluginLoader.PluginRootDir.LocalStoragePathToPackagePath());
            });

        public ICommand ClearFilterTextCommand => new MpCommand(
            () => {
                FilterText = string.Empty;
            });

        public ICommand ShowPluginBrowserCommand => new MpCommand(
            () => {
                if (MpAvThisAppVersionViewModel.Instance.IsOutOfDate) {
                    MpAvUriNavigator.Instance.NavigateToUriCommand.Execute(MpAvAccountTools.Instance.ThisProductUri);
                    return;
                }
                OpenPluginBrowserWindow(null);
            });

        public ICommand ClearPluginSelectionCommand => new MpCommand(
            () => {
                SelectedItem = null;
            },
            () => {
                return SelectedItem != null;
            });
        
        public ICommand BackCommand => new MpCommand<object>(
            (args) => {
                if(SelectedItem == null) {
                    IsWindowOpen = false;
                } else {
                    ClearPluginSelectionCommand.Execute(null);
                }
            });

        #endregion
    }
}
