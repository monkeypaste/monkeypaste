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
        private Stopwatch _sw;
        public override async Task CreatePlatformAsync(DateTime startup_datetime) {
#if LINUX
            
                await GtkHelper.EnsureInitialized();
#elif MAC

                MpAvMacHelpers.EnsureInitialized();
#endif

            await Mp.InitAsync(new MpAvWrapper(this, this));

            if (MpPrefViewModel.Instance != null) {
                if (MpPrefViewModel.Instance.LastStartupDateTime == null) {
                    IsInitialStartup = true;
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
            IsCoreLoaded = true;

            await base.FinishLoaderAsync();

            if (Mp.Services.PlatformInfo.IsDesktop) {
                App.MainView.Show();
                MpAvSystemTray.Init();
            }
            IsPlatformLoaded = true;
            _sw.Stop();
        }
        protected override void CreateLoaderItems() {
#if DESKTOP
            BaseItems.Add(new MpLoaderItemViewModel(this, typeof(MpAvCefNetApplication)));
#endif

            CoreItems.AddRange(
               new List<MpLoaderItemViewModel>() {
                    new MpLoaderItemViewModel(this,typeof(MpAvNotificationWindowManager)),
                    new MpLoaderItemViewModel(this,typeof(MpAvThemeViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpConsole)),
                    new MpLoaderItemViewModel(this,typeof(MpTempFileManager)),
                    new MpLoaderItemViewModel(this,typeof(MpPortableDataFormats),Mp.Services.DataObjectRegistrar),
                    new MpLoaderItemViewModel(this,typeof(MpDb)),
                    new MpLoaderItemViewModel(this,typeof(MpAvTemplateModelHelper)),
                    new MpLoaderItemViewModel(this,typeof(MpPluginLoader)),
                    new MpLoaderItemViewModel(this,typeof(MpAvTemplateModelHelper)),
                    new MpLoaderItemViewModel(this,typeof(MpAvSoundPlayerViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvIconCollectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvAppCollectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvUrlCollectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvSystemTrayViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipTileSortFieldViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipTileSortDirectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvSearchBoxViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipboardHandlerCollectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvAnalyticItemCollectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvSettingsViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvClipTrayViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvDragProcessWatcher)),
                    new MpLoaderItemViewModel(this,typeof(MpAvTagTrayViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvExternalPasteHandler)),
                    new MpLoaderItemViewModel(this,typeof(MpDataModelProvider)),
                    new MpLoaderItemViewModel(this,typeof(MpAvTriggerCollectionViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvExternalDropWindowViewModel)),
                    new MpLoaderItemViewModel(this,typeof(MpAvShortcutCollectionViewModel)),
               });

            if (Mp.Services.PlatformInfo.IsDesktop) {
                PlatformItems.AddRange(
                   new List<MpLoaderItemViewModel>() {
                        new MpLoaderItemViewModel(this,typeof(MpAvPlainHtmlConverter)),
                        //new MpLoaderItemViewModel(this,typeof(MpAppendNotificationViewModel)),
                        new MpLoaderItemViewModel(this,typeof(MpAvMainView)),
                        new MpLoaderItemViewModel(this,typeof(MpAvMainWindowViewModel))
                   });
            } else {
                PlatformItems.AddRange(
                   new List<MpLoaderItemViewModel>() {
                        new MpLoaderItemViewModel(this,typeof(MpAvMainView)),
                        new MpLoaderItemViewModel(this,typeof(MpAvPlainHtmlConverter)),
                        new MpLoaderItemViewModel(this,typeof(MpAvMainWindowViewModel))
                   });
            }


        }

        protected override async Task LoadItemAsync(MpLoaderItemViewModel item, int index, bool affectsCount) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            await item.LoadItemAsync();
            sw.Stop();

            if (affectsCount) {
                // NOTE just to be tidy base items (cef only atm) are ignored or count goes over 1

                LoadedCount++;
                OnPropertyChanged(nameof(PercentLoaded));
                OnPropertyChanged(nameof(Detail));
            }

            Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

            int dotCount = index % 4;
            Title = "LOADING";
            for (int i = 0; i < dotCount; i++) {
                Title += ".";
            }

            IsBusy = false;
            MpConsole.WriteLine($"Loaded {item.Label} at idx: {index} Load Count: {LoadedCount} Load Percent: {PercentLoaded} Time(ms): {sw.ElapsedMilliseconds}");
        }
    }
}