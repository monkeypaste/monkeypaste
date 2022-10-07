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

namespace MonkeyPaste.Avalonia {
    public class MpAvBootstrapperViewModel : MpBootstrapperViewModelBase {
        public override async Task InitAsync() {
            if (MpAvCefNetApplication.UseCefNet) {
                MpAvCefNetApplication.InitCefNet();
            } 

            // if (OperatingSystem.IsLinux()) {
            //     await GtkHelper.EnsureInitialized();
            // } else if (OperatingSystem.IsMacOS()) {
            //     MpAvMacHelpers.EnsureInitialized();
            // }

            var pw = new MpAvWrapper();
            await pw.InitializeAsync();
            await MpPlatformWrapper.InitAsync(pw);

            await MpNotificationCollectionViewModel.Instance.InitAsync(null);

            var nw = new MpAvNotificationWindow();
            nw.Opened += Nw_Opened;

            await MpNotificationCollectionViewModel.Instance.RegisterWithWindowAsync(nw);
            CreateLoaderItems();
            await MpNotificationCollectionViewModel.Instance.BeginLoaderAsync(this);
            while (IsCoreLoaded == false) {
                await Task.Delay(100);
            }

            MpConsole.WriteLine("Core and ViewModel Bootstrap complete");


            App.Desktop.MainWindow = new MpAvMainWindow();
            App.Desktop.MainWindow.Show();


        }

        private async void Nw_Opened(object sender, EventArgs e) {
            for (int i = 0; i < _coreItems.Count; i++) {
                await LoadItemAsync(_coreItems[i], i);
                while(IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (_coreItems.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            await Task.Delay(1000);

            //for (int i = 1; i <= 1000; i++) {
            //    await MpCopyItem.Create(
            //        sourceId: MpPrefViewModel.Instance.ThisAppSourceId,
            //        title: $"{i} This is a test title BOO!",
            //        data: $"This is the content for test {i}",
            //        itemType: MpCopyItemType.Text);
            //}


            MpNotificationCollectionViewModel.Instance.FinishLoading();
            //MpAvClipTrayViewModel.Instance.OnPostMainWindowLoaded();
            IsCoreLoaded = true;

            // MainWindow is now being created and Av AppLifetime desktop is being swapped to MainWindow
            // wait for mw instance to exist
            while(MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                await Task.Delay(100);
            }

            // once mw and all mw views are loaded load platform items
            for (int i = 0; i < _platformItems.Count(); i++) {
                await LoadItemAsync(_platformItems[i], i);
                while (IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (_platformItems.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
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

                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTileSortViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSearchBoxViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpAnalyticItemCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipboardHandlerCollectionViewModel)),

                    ////new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSettingsWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvClipTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvShortcutCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpAvTagTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvMainWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAvDataObjectHelper)),
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
                    new MpBootstrappedItemViewModel(this,typeof(MpAvHtmlClipboardData)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAvSystemTray))
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