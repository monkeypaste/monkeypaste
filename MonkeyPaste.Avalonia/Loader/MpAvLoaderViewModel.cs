using Avalonia.Controls;
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
        MpViewModelBase,
        MpIStartupState,
        MpIProgressLoaderViewModel {
        #region Private Variables
        private Stopwatch _sw;
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #region MpIStartupState Implementation

        public DateTime? LoadedDateTime { get; private set; } = null;
        public bool IsCoreLoaded { get; protected set; } = false;
        public bool IsPlatformLoaded { get; protected set; } = false;
        public MpStartupFlags StartupFlags { get; protected set; }

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

        #endregion

        #region State
        public bool IS_PARALLEL_LOADING_ENABLED =>
            false;

        public int LoadedCount { get; set; } = 0;

        #endregion
        #endregion

        #region Constructors
        public MpAvLoaderViewModel(bool wasStartedAtLogin) {
            StartupFlags |= wasStartedAtLogin ? MpStartupFlags.Login : MpStartupFlags.UserInvoked;
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

            if (MpPrefViewModel.Instance != null) {
                if (MpPrefViewModel.Instance.LastStartupDateTime == null) {
                    StartupFlags |= MpStartupFlags.Initial;
                }
                MpPrefViewModel.Instance.LastStartupDateTime = MpPrefViewModel.Instance.StartupDateTime;
                MpPrefViewModel.Instance.StartupDateTime = startup_datetime;

                MpAvThemeViewModel.Instance.SyncThemePrefs();
            }
        }

        public async Task InitAsync() {
            _sw = Stopwatch.StartNew();

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
        }
        public async Task FinishLoaderAsync() {
            await Dispatcher.UIThread.InvokeAsync(async () => {
                // once mw and all mw views are loaded load platform items
                await LoadItemsAsync(PlatformItems);
                MpConsole.WriteLine("Platform load complete");
                LoadedDateTime = DateTime.Now;

                if (Mp.Services.PlatformInfo.IsDesktop) {
                    App.MainView.Show();
                    MpAvSystemTray.Init();
                }
                IsPlatformLoaded = true;
                _sw.Stop();
            }, DispatcherPriority.Background);
        }
        #endregion

        #region Protected Methods

        private void CreateLoaderItems() {
#if DESKTOP
            BaseItems.Add(new MpAvLoaderItemViewModel(typeof(MpAvCefNetApplication), "Rich Content Editor"));
#endif

            CoreItems.AddRange(
               new List<MpAvLoaderItemViewModel>() {
                    new MpAvLoaderItemViewModel(typeof(MpAvNotificationWindowManager),"Notifications"),
                    new MpAvLoaderItemViewModel(typeof(MpAvThemeViewModel),"Theme"),
                    new MpAvLoaderItemViewModel(typeof(MpConsole),"Logger"),
                    new MpAvLoaderItemViewModel(typeof(MpTempFileManager),"Temp File Manager"),
                    new MpAvLoaderItemViewModel(typeof(MpPortableDataFormats),"Supported Clipboard Formats",Mp.Services.DataObjectRegistrar),
                    new MpAvLoaderItemViewModel(typeof(MpDb), "Data"),
                    new MpAvLoaderItemViewModel(typeof(MpAvTemplateModelHelper), "Templates"),
                    new MpAvLoaderItemViewModel(typeof(MpPluginLoader), "Plugins"),
                    new MpAvLoaderItemViewModel(typeof(MpAvSoundPlayerViewModel), "Sound Player"),
                    new MpAvLoaderItemViewModel(typeof(MpAvIconCollectionViewModel), "Icons"),
                    new MpAvLoaderItemViewModel(typeof(MpAvAppCollectionViewModel), "App Interop"),
                    new MpAvLoaderItemViewModel(typeof(MpAvUrlCollectionViewModel), "Web Interop"),
                    new MpAvLoaderItemViewModel(typeof(MpAvSystemTrayViewModel), "System Tray"),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipTileSortFieldViewModel), "Content Sort"),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipTileSortDirectionViewModel), "Content Sort"),
                    new MpAvLoaderItemViewModel(typeof(MpAvSearchBoxViewModel), "Content Search"),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipboardHandlerCollectionViewModel), "Clipboard Listener"),
                    new MpAvLoaderItemViewModel(typeof(MpAvAnalyticItemCollectionViewModel), "Analyzers"),
                    new MpAvLoaderItemViewModel(typeof(MpAvSettingsViewModel), "Preferences"),
                    new MpAvLoaderItemViewModel(typeof(MpAvClipTrayViewModel), "Content"),
                    new MpAvLoaderItemViewModel(typeof(MpAvDragProcessWatcher), "Drag-and-Drop"),
                    new MpAvLoaderItemViewModel(typeof(MpAvTagTrayViewModel), "Collections"),
                    new MpAvLoaderItemViewModel(typeof(MpAvExternalPasteHandler), "Paste Interop"),
                    new MpAvLoaderItemViewModel(typeof(MpDataModelProvider), "Querying"),
                    new MpAvLoaderItemViewModel(typeof(MpAvTriggerCollectionViewModel), "Triggers"),
                    new MpAvLoaderItemViewModel(typeof(MpAvExternalDropWindowViewModel), "Drop Widget"),
                    new MpAvLoaderItemViewModel(typeof(MpAvShortcutCollectionViewModel), "Shortcuts"),
               });

            if (Mp.Services.PlatformInfo.IsDesktop) {
                PlatformItems.AddRange(
                   new List<MpAvLoaderItemViewModel>() {
                        new MpAvLoaderItemViewModel(typeof(MpAvPlainHtmlConverter), "Content Converters"),
                        //new MpLoaderItemViewModel(typeof(MpAppendNotificationViewModel)),
                        new MpAvLoaderItemViewModel(typeof(MpAvMainView), "User Interface"),
                        new MpAvLoaderItemViewModel(typeof(MpAvMainWindowViewModel), "User Experience")
                   });
            } else {
                PlatformItems.AddRange(
                   new List<MpAvLoaderItemViewModel>() {
                        new MpAvLoaderItemViewModel(typeof(MpAvMainView), "User Interface"),
                        new MpAvLoaderItemViewModel(typeof(MpAvPlainHtmlConverter), "Content Converters"),
                        new MpAvLoaderItemViewModel(typeof(MpAvMainWindowViewModel), "User Experience")
                   });
            }
        }

        private async Task LoadItemAsync(MpAvLoaderItemViewModel item, int index, bool affectsCount) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

            int dotCount = index % 4;
            Title = "LOADING";
            for (int i = 0; i < dotCount; i++) {
                Title += ".";
            }
            await item.LoadItemAsync();
            sw.Stop();

            if (affectsCount) {
                // NOTE just to be tidy base items (cef only atm) are ignored or count goes over 1

                LoadedCount++;
                OnPropertyChanged(nameof(PercentLoaded));
                OnPropertyChanged(nameof(Detail));
            }


            IsBusy = false;
            MpConsole.WriteLine($"Loaded {item.Label} at idx: {index} Load Count: {LoadedCount} Load Percent: {PercentLoaded} Time(ms): {sw.ElapsedMilliseconds}");
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
        private async Task LoadItemsAsync(List<MpAvLoaderItemViewModel> items, bool affectsCount = true) {
            if (IS_PARALLEL_LOADING_ENABLED) {
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