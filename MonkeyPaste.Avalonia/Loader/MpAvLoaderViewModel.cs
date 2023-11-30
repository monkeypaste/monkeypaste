using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvLoaderViewModel :
        MpAvViewModelBase,
        MpIStartupState,
        MpIProgressLoaderViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static Stopwatch LoaderStopWatch;
        #endregion

        #region Interfaces

        #region MpIStartupState Implementation

        public DateTime? LoadedDateTime { get; private set; } = null;
        public bool IsCoreLoaded { get; protected set; } = false;
        public bool IsPlatformLoaded { get; protected set; } = false;
        public bool IsLoginLoad { get; set; }
        public bool IsInitialStartup { get; set; } = true;

        public bool IsReady { get; private set; }
        #endregion

        #region MpIProgressLoaderViewModel Implementation

        public string IconResourceKey =>
            MpBase64Images.AppIcon;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Detail {
            get => $"{(int)(PercentLoaded * 100.0)}%";
            set => throw new NotImplementedException();
        }

        public double PercentLoaded =>
            (double)LoadedCount / (double)(Items.Count);

        public MpNotificationType DialogType => MpNotificationType.Loader;

        public bool ShowSpinner =>
            IsCoreLoaded;
        //PercentLoaded >= 100;
        #endregion
        #endregion


        #region Properties
        #region View Models
        public List<MpAvLoaderItemViewModel> BaseItems { get; private set; } = new List<MpAvLoaderItemViewModel>();
        public List<MpAvLoaderItemViewModel> CoreItems { get; private set; } = new List<MpAvLoaderItemViewModel>();
        public List<MpAvLoaderItemViewModel> PlatformItems { get; private set; } = new List<MpAvLoaderItemViewModel>();

        public IList<MpAvLoaderItemViewModel> Items =>
            CoreItems.Union(PlatformItems).ToList();

        public IList<MpAvLoaderItemViewModel> PendingItems =>
            Items.Where(x => !x.IsLoaded).ToList();

        #endregion

        #region State
        public bool IsParallelLoadingEnabled =>
        // NOTE db create takes extra time and breaks when vm's query in parallel
        // (this maybe a sign this needs more organization)
        !IsInitialStartup;
        //false;

        public int LoadedCount { get; set; } = 0;


        #endregion
        #endregion

        #region Constructors
        public MpAvLoaderViewModel(bool wasStartedAtLogin) {
            IsLoginLoad = wasStartedAtLogin;
            LoaderStopWatch = Stopwatch.StartNew();
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            PropertyChanged += MpAvLoaderViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task CreatePlatformAsync(DateTime startup_datetime) {
#if LINUX            
                await GtkHelper.EnsureInitialized();
#elif MAC
            MpAvMacHelpers.EnsureInitialized();
#endif

            await Mp.InitAsync(new MpAvWrapper(this));

            if (MpAvPrefViewModel.Instance == null) {
                return;
            }
            IsInitialStartup = !MpAvPrefViewModel.Instance.IsWelcomeComplete;
            MpAvPrefViewModel.Instance.StartupDateTime = startup_datetime;
            MpAvThemeViewModel.Instance.UpdateThemeResources();
        }

        public async Task InitAsync() {
            await MpAvWelcomeNotificationViewModel.ShowWelcomeNotificationAsync();

            CreateLoaderItems();

            // init cefnet (if needed) BEFORE window creation
            // or chromium child process stuff will re-initialize (and show loader again)
            await LoadItemsAsync(BaseItems);
            await Mp.Services.NotificationBuilder.ShowLoaderNotificationAsync(this);
        }
        public async Task BeginLoaderAsync() {
            await LoadItemsAsync(CoreItems);
            IsCoreLoaded = true;
            MpConsole.WriteLine("Core load complete");
#if DEBUG
            MpConsole.WriteLine($"Pref Password: '{MpAvPrefViewModel.arg1}'");
            MpConsole.WriteLine($"Backup Pref Password: '{MpAvPrefViewModel.arg3}'");
            MpConsole.WriteLine($"Db Password: '{MpAvPrefViewModel.arg2}'");
#endif
        }
        public async Task FinishLoaderAsync() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                // once mw and all mw views are loaded load platform items
                await LoadItemsAsync(PlatformItems);
                MpConsole.WriteLine("Platform load complete");

                LoadedDateTime = DateTime.Now;

                //if (Mp.Services.PlatformInfo.IsDesktop) {
                //App.MainView.Show();
                //}
                IsPlatformLoaded = true;
            }, DispatcherPriority.Background);
        }
        #endregion

        #region Protected Methods

        private void CreateLoaderItems() {
            //#if DESKTOP
            BaseItems.AddRange(new[] {
                //new MpAvLoaderItemViewModel(typeof(MpAvCefNetApplication), "Rich Content Editor"),
                new MpAvLoaderItemViewModel(typeof(MpConsole),UiStrings.LoaderLoggerLabel, Mp.Services.PlatformInfo),
                new MpAvLoaderItemViewModel(typeof(MpAvSystemTray), UiStrings.LoaderSysTrayLabel),
                new MpAvLoaderItemViewModel(typeof(MpAvThemeViewModel),UiStrings.LoaderThemeLabel),
                new MpAvLoaderItemViewModel(typeof(MpDb), UiStrings.LoaderDataLabel),
                new MpAvLoaderItemViewModel(typeof(MpAvAccountViewModel), UiStrings.LoaderAccountLabel),
            }.ToList());
            //#endif

            CoreItems.AddRange(
               new List<MpAvLoaderItemViewModel>() {
                    //new MpAvLoaderItemViewModel(typeof(MpAvPlainHtmlConverter), "Content Converters"),
                    //new MpAvLoaderItemViewModel(typeof(MpAvNotificationWindowManager),"Notifications"),
                    //new MpAvLoaderItemViewModel(typeof(MpAvThemeViewModel),"Theme"),
                    new MpAvLoaderItemViewModel(typeof(MpPortableDataFormats),UiStrings.LoaderClipboardLabel, Mp.Services.DataObjectRegistrar),
                    //new MpAvLoaderItemViewModel(typeof(MpAvTemplateModelHelper), "Templates"),
                    new MpAvLoaderItemViewModel(typeof(MpPluginLoader), UiStrings.LoaderAnalyzersLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvSoundPlayerViewModel), UiStrings.LoaderSoundLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvIconCollectionViewModel), UiStrings.LoaderIconsLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvAppCollectionViewModel), UiStrings.LoaderAppLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvUrlCollectionViewModel), UiStrings.LoaderUrlLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvSystemTrayViewModel), UiStrings.LoaderSysTrayLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipTileSortFieldViewModel), UiStrings.LoaderSortLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipTileSortDirectionViewModel), UiStrings.LoaderDirLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvSearchBoxViewModel), UiStrings.LoaderSearchLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipboardHandlerCollectionViewModel), UiStrings.LoaderClipboardLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvAnalyticItemCollectionViewModel), UiStrings.LoaderAnalyzersLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvSettingsViewModel), UiStrings.LoaderPrefLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipTrayViewModel), UiStrings.LoaderContentLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvDndProcessWatcher), UiStrings.LoaderDndLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvTagTrayViewModel), UiStrings.LoaderTagsLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvExternalPasteHandler), UiStrings.LoaderPasteLabel),
                    //new MpAvLoaderItemViewModel(typeof(MpDataModelProvider), "Querying"),
                    new MpAvLoaderItemViewModel(typeof(MpAvTriggerCollectionViewModel), UiStrings.LoaderTriggersLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvExternalDropWindowViewModel), UiStrings.LoaderDropWidgetLabel),
                    new MpAvLoaderItemViewModel(typeof(MpAvShortcutCollectionViewModel), UiStrings.LoaderShortcutsLabel),
               }); ;

            if (Mp.Services.PlatformInfo.IsDesktop) {
                PlatformItems.AddRange(
                   new List<MpAvLoaderItemViewModel>() {
                        //new MpLoaderItemViewModel(typeof(MpAppendNotificationViewModel)),
                        new MpAvLoaderItemViewModel(typeof(MpAvPlainHtmlConverter), UiStrings.LoaderConvertersLabel),
                        new MpAvLoaderItemViewModel(typeof(MpAvMainView), UiStrings.LoaderMainWindowLabel),
                        new MpAvLoaderItemViewModel(typeof(MpAvMainWindowViewModel),  UiStrings.LoaderMainWindowLabel)
                   });
            } else {
                PlatformItems.AddRange(
                   new List<MpAvLoaderItemViewModel>() {
                        new MpAvLoaderItemViewModel(typeof(MpAvMainView),  UiStrings.LoaderMainWindowLabel),
                        new MpAvLoaderItemViewModel(typeof(MpAvPlainHtmlConverter), UiStrings.LoaderConvertersLabel),
                        new MpAvLoaderItemViewModel(typeof(MpAvMainWindowViewModel),  UiStrings.LoaderMainWindowLabel)
                   });
            }
        }

        private async Task LoadItemAsync(MpAvLoaderItemViewModel item, int index, bool affectsCount) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

            int dotCount = index % 4;
            Title = UiStrings.NtfLoaderTitle;
            for (int i = 0; i < dotCount; i++) {
                Title += ".";
            }
            await item.LoadItemAsync();
            item.IsLoaded = true;
            sw.Stop();

            if (affectsCount) {
                // NOTE just to be tidy base items (cef only atm) are ignored or count goes over 1

                LoadedCount++;
                OnPropertyChanged(nameof(PercentLoaded));
                OnPropertyChanged(nameof(Detail));
            }


            IsBusy = false;
            MpConsole.WriteLine($"Loaded {item.Label} at idx: {index} Load Count: {LoadedCount} Load Percent: {PercentLoaded} Time(ms): {sw.ElapsedMilliseconds}");

            //Items.Where(x => !x.IsLoaded).ForEach(x => MpConsole.WriteLine($"Still not loaded: {x.ItemType}"));

        }
        #endregion

        #region Private Methods

        private void MpAvLoaderViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsCoreLoaded):
                    OnPropertyChanged(nameof(ShowSpinner));
                    if (ShowSpinner) {

                    }
                    break;
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowLoadComplete:
                    IsReady = true;
                    break;
            }
        }
        private async Task LoadItemsAsync(List<MpAvLoaderItemViewModel> items, bool affectsCount = true) {
            if (IsParallelLoadingEnabled) {
                await LoadItemsParallelAsync(items, affectsCount);
            } else {
                await LoadItemsSequentialAsync(items, affectsCount);
            }
        }

        private async Task LoadItemsParallelAsync(List<MpAvLoaderItemViewModel> items, bool affectsCount) {
            await Task.WhenAll(items.Select((x, idx) => LoadItemAsync(x, idx, affectsCount)));
            while (items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
        }

        private async Task LoadItemsSequentialAsync(List<MpAvLoaderItemViewModel> items, bool affectsCount) {
            for (int i = 0; i < items.Count; i++) {
                await LoadItemAsync(items[i], i, affectsCount);
                while (IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}