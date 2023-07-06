using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvLoaderViewModel : MpLoaderViewModelBase {
        #region Private Variables
        private Stopwatch _sw;
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        public MpAvLoaderViewModel(bool wasStartedAtLogin) {
            StartupFlags |= wasStartedAtLogin ? MpStartupFlags.Login : MpStartupFlags.UserInvoked;
        }

        #endregion

        #region Public Methods

        public override async Task CreatePlatformAsync(DateTime startup_datetime) {
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

        public override async Task InitAsync() {
            _sw = Stopwatch.StartNew();

            CreateLoaderItems();

            // init cefnet (if needed) BEFORE window creation
            // or chromium child process stuff will re-initialize (and show loader again)
            await LoadItemsAsync(BaseItems);
            await MpNotificationBuilder.ShowLoaderNotificationAsync(this);
        }

        public override async Task FinishLoaderAsync() {

            await base.FinishLoaderAsync();

            if (Mp.Services.PlatformInfo.IsDesktop) {
                App.MainView.Show();
                MpAvSystemTray.Init();
            }
            IsPlatformLoaded = true;
            _sw.Stop();
        }
        #endregion

        #region Protected Methods

        protected override void CreateLoaderItems() {
#if DESKTOP
            BaseItems.Add(new MpLoaderItemViewModel(this, typeof(MpAvCefNetApplication), "Rich Content Editor"));
#endif

            CoreItems.AddRange(
               new List<MpLoaderItemViewModel>() {
                    new MpLoaderItemViewModel(this,typeof(MpAvNotificationWindowManager),"Notifications"),
                    new MpLoaderItemViewModel(this,typeof(MpAvThemeViewModel),"Theme"),
                    new MpLoaderItemViewModel(this,typeof(MpConsole),"Logger"),
                    new MpLoaderItemViewModel(this,typeof(MpTempFileManager),"Temp File Manager"),
                    new MpLoaderItemViewModel(this,typeof(MpPortableDataFormats),"Supported Clipboard Formats",Mp.Services.DataObjectRegistrar),
                    new MpLoaderItemViewModel(this,typeof(MpDb), "Data"),
                    new MpLoaderItemViewModel(this,typeof(MpAvTemplateModelHelper), "Templates"),
                    new MpLoaderItemViewModel(this,typeof(MpPluginLoader), "Plugins"),
                    new MpLoaderItemViewModel(this,typeof(MpAvSoundPlayerViewModel), "Sound Player"),
                    new MpLoaderItemViewModel(this,typeof(MpAvIconCollectionViewModel), "Icons"),
                    new MpLoaderItemViewModel(this,typeof(MpAvAppCollectionViewModel), "App Interop"),
                    new MpLoaderItemViewModel(this,typeof(MpAvUrlCollectionViewModel), "Web Interop"),
                    new MpLoaderItemViewModel(this,typeof(MpAvSystemTrayViewModel), "System Tray"),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipTileSortFieldViewModel), "Content Sort"),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipTileSortDirectionViewModel), "Content Sort"),
                    new MpLoaderItemViewModel(this,typeof(MpAvSearchBoxViewModel), "Content Search"),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipboardHandlerCollectionViewModel), "Clipboard Listener"),
                    new MpLoaderItemViewModel(this,typeof(MpAvAnalyticItemCollectionViewModel), "Analyzers"),
                    new MpLoaderItemViewModel(this,typeof(MpAvSettingsViewModel), "Preferences"),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipTrayViewModel), "Content"),
                    new MpLoaderItemViewModel(this,typeof(MpAvDragProcessWatcher), "Drag-and-Drop"),
                    new MpLoaderItemViewModel(this,typeof(MpAvTagTrayViewModel), "Collections"),
                    new MpLoaderItemViewModel(this,typeof(MpAvExternalPasteHandler), "Paste Interop"),
                    new MpLoaderItemViewModel(this,typeof(MpDataModelProvider), "Querying"),
                    new MpLoaderItemViewModel(this,typeof(MpAvTriggerCollectionViewModel), "Triggers"),
                    new MpLoaderItemViewModel(this,typeof(MpAvExternalDropWindowViewModel), "Drop Widget"),
                    new MpLoaderItemViewModel(this,typeof(MpAvShortcutCollectionViewModel), "Shortcuts"),
               });

            if (Mp.Services.PlatformInfo.IsDesktop) {
                PlatformItems.AddRange(
                   new List<MpLoaderItemViewModel>() {
                        new MpLoaderItemViewModel(this,typeof(MpAvPlainHtmlConverter), "Content Converters"),
                        //new MpLoaderItemViewModel(this,typeof(MpAppendNotificationViewModel)),
                        new MpLoaderItemViewModel(this,typeof(MpAvMainView), "User Interface"),
                        new MpLoaderItemViewModel(this,typeof(MpAvMainWindowViewModel), "User Experience")
                   });
            } else {
                PlatformItems.AddRange(
                   new List<MpLoaderItemViewModel>() {
                        new MpLoaderItemViewModel(this,typeof(MpAvMainView), "User Interface"),
                        new MpLoaderItemViewModel(this,typeof(MpAvPlainHtmlConverter), "Content Converters"),
                        new MpLoaderItemViewModel(this,typeof(MpAvMainWindowViewModel), "User Experience")
                   });
            }


        }

        protected override async Task LoadItemAsync(MpLoaderItemViewModel item, int index, bool affectsCount) {
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
        #endregion

        #region Commands
        #endregion
    }
}