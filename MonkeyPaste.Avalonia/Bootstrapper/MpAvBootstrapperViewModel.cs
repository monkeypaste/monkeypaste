using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var pw = new MpAvWrapper();
            pw.StartupState = this;
            pw.StartupObjectLocator = this;
            await pw.InitializeAsync();
            await MpPlatform.InitAsync(pw);

            CreateLoaderItems();
            await MpNotificationBuilder.ShowLoaderNotificationAsync(this);
        }

        public override async Task FinishLoaderAsync() {
            IsCoreLoaded = true;
            Window lw = App.MainWindow;

            await base.FinishLoaderAsync();

            if (lw != null) {
                lw.Close();
            }
            App.MainWindow.Show();
            IsPlatformLoaded = true;
            _sw.Stop();

            MpAvSystemTray.Init();
        }
        protected override void CreateLoaderItems() {
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

            _platformItems.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpAvPlainHtmlConverter)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvExternalDropWindow)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvAppendNotificationWindow)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTray)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindow)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindowViewModel))
                });

        }
        protected override async Task LoadItemAsync(MpBootstrappedItemViewModel item, int index) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            await item.LoadItemAsync();
            sw.Stop();

            LoadedCount++;

            OnPropertyChanged(nameof(PercentLoaded));
            OnPropertyChanged(nameof(Detail));

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