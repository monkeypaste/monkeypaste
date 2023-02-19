using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvBootstrapperViewModel : MpBootstrapperViewModelBase {
        private Stopwatch _sw;
        public override async Task InitAsync() {
            _sw = Stopwatch.StartNew();

            if (OperatingSystem.IsLinux()) {
                await GtkHelper.EnsureInitialized();
            } else if (OperatingSystem.IsMacOS()) {
                MpAvMacHelpers.EnsureInitialized();
            }

            await MpPlatform.InitAsync(new MpAvWrapper(this, this));

            CreateLoaderItems();

            // init cefnet (if needed) BEFORE window creation
            // or chromium child process stuff will re-initialize (and show loader again)
            await LoadItemsAsync(_baseItems);
            await MpNotificationBuilder.ShowLoaderNotificationAsync(this);
        }

        public override async Task FinishLoaderAsync() {
            IsCoreLoaded = true;

            await base.FinishLoaderAsync();

            if (MpPlatform.Services.PlatformInfo.IsDesktop) {
                App.MainView.Show();
                MpAvSystemTray.Init();
            }
            IsPlatformLoaded = true;
            _sw.Stop();
        }
        protected override void CreateLoaderItems() {
            if (MpPlatform.Services.PlatformInfo.IsDesktop) {
                _baseItems.Add(new MpBootstrappedItemViewModel(this, typeof(MpAvCefNetApplication)));
            }

            _coreItems.AddRange(
               new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpAvNotificationWindowManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvThemeViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpConsole)),
                    new MpBootstrappedItemViewModel(this,typeof(MpTempFileManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpDb)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMasterTemplateModelCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPluginLoader)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPortableDataFormats),MpPlatform.Services.DataObjectRegistrar),
                    new MpBootstrappedItemViewModel(this,typeof(MpMasterTemplateModelCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvIconCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvAppCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvUrlCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTileSortFieldViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTileSortDirectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSearchBoxViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipboardHandlerCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvAnalyticItemCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSettingsWindowViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvShortcutCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvDragProcessWatcher)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvTagTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvExternalPasteHandler)),
                    new MpBootstrappedItemViewModel(this,typeof(MpDataModelProvider)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvTriggerCollectionViewModel)),
                   // new MpBootstrappedItemViewModel(this,typeof(MpAvFilterMenuViewModel))
               });

            if (MpPlatform.Services.PlatformInfo.IsDesktop) {
                _platformItems.AddRange(
                   new List<MpBootstrappedItemViewModel>() {
                        new MpBootstrappedItemViewModel(this,typeof(MpAvPlainHtmlConverter)),
                        new MpBootstrappedItemViewModel(this,typeof(MpAvExternalDropWindow)),
                        new MpBootstrappedItemViewModel(this,typeof(MpAvAppendNotificationWindow)),
                        //new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTray)),
                        new MpBootstrappedItemViewModel(this,typeof(MpAvMainView)),
                        new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindowViewModel))
                   });
            } else {
                _platformItems.AddRange(
                   new List<MpBootstrappedItemViewModel>() {
                        new MpBootstrappedItemViewModel(this,typeof(MpAvMainView)),
                        new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindowViewModel))
                   });
            }


        }

        protected override async Task LoadItemAsync(MpBootstrappedItemViewModel item, int index, bool affectsCount) {
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