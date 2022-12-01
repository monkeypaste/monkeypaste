using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using System.IO;
using System.Collections;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvBootstrapperViewModel : MpBootstrapperViewModelBase {
        public override async Task InitAsync() {
            if (OperatingSystem.IsLinux()) {
                await GtkHelper.EnsureInitialized();
            } else if (OperatingSystem.IsMacOS()) {
                MpAvMacHelpers.EnsureInitialized();
            }
            
            var pw = new MpAvWrapper();
            await pw.InitializeAsync();
            await MpPlatformWrapper.InitAsync(pw);

            CreateLoaderItems();
            await MpNotificationBuilder.ShowLoaderNotificationAsync(this);           
        }

        public override async Task FinishLoaderAsync() {
            IsCoreLoaded = true;

            MpConsole.WriteLine("Core and ViewModel Bootstrap complete");

            // swap main window then close loader
            var lw = App.Desktop.MainWindow;
            App.Desktop.MainWindow = new MpAvMainWindow();
            App.Desktop.MainWindow.Show();
            lw.Close();

            // MainWindow is now being created and Av AppLifetime desktop is being swapped to MainWindow
            // wait for mw instance to exist
            while (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                await Task.Delay(100);
            }

            await base.FinishLoaderAsync();
            IsPlatformLoaded = true;
        }
        protected override void CreateLoaderItems() {
            base.CreateLoaderItems();

            _coreItems.AddRange(
               new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpPortableDataFormats),MpPlatformWrapper.Services.DataObjectRegistrar),
                    //new MpBootstrappedItemViewModel(this,typeof(MpAvQueryInfoViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMasterTemplateModelCollectionViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpDocumentHtmlExtension)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpProcessManager), Properties.Settings.Default.IgnoredProcessNames),
                    ////new MpBootstrappedItemViewModel(this,typeof(MpProcessAutomation)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpScreenInformation)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpThemeColors)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpMeasurements)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpFileSystemWatcher)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvIconCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvAppCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvUrlCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSourceCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTrayViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTileSortFieldViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTileSortDirectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSearchBoxViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpAnalyticItemCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipboardHandlerCollectionViewModel)),

                    ////new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSettingsWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvShortcutCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpAvTagTrayViewModel)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvExternalPasteHandler)),
                   //new MpBootstrappedItemViewModel(this,typeof(MpActionCollectionViewModel)),

                   //new MpBootstrappedItemViewModel(this,typeof(MpContextMenuView)),

                   //new MpBootstrappedItemViewModel(this,typeof(MpAvDragDropManager)),


                   //new MpBootstrappedItemViewModel(this,typeof(MpWpfDataObjectHelper)),
                   //new MpBootstrappedItemViewModel(this,typeof(MpQuillHtmlToRtfConverter)),
                   //new MpBootstrappedItemViewModel(this,typeof(MpTooltipInfoCollectionViewModel))
                   ////new MpBootstrappedItem(typeof(MpMouseHook))
                   new MpBootstrappedItemViewModel(this,typeof(MpDataModelProvider))
               });

            _platformItems.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpAvPlainHtmlConverter)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvExternalDropWindow)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvAppendNotificationWindow)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTray)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindow)),
                });
        }
        protected override async Task LoadItemAsync(MpBootstrappedItemViewModel item, int index) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            await item.LoadItemAsync();
            sw.Stop();

            if(item.IsViewDependant) {
                // nothing at this point will join somehow later
            } else {
                LoadedCount++;

                OnPropertyChanged(nameof(PercentLoaded));
                OnPropertyChanged(nameof(Detail));

                Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

                int dotCount = index % 4;
                Title = "LOADING";
                for (int i = 0; i < dotCount; i++) {
                    Title += ".";
                }
            }

            IsBusy = false;
            MpConsole.WriteLine("Loaded " + item.Label + " at idx: " + index + " IsViewDependant: " + (item.IsViewDependant ? "YES" : "NO") + " Load Count: " + LoadedCount + " Load Percent: " + PercentLoaded + " Time(ms): " + sw.ElapsedMilliseconds);
        }
    }
}