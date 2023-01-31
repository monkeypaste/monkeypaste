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
            pw.StartupState = this;
            pw.StartupObjectLocator = this;
            await pw.InitializeAsync();
            await MpPlatform.InitAsync(pw);

            CreateLoaderItems();
            await MpNotificationBuilder.ShowLoaderNotificationAsync(this);           
        }

        public override async Task FinishLoaderAsync() {
            IsCoreLoaded = true;

            MpConsole.WriteLine("Core Load complete");
            Window lw = App.Desktop.MainWindow;

            await base.FinishLoaderAsync();

            if (lw != null) {
                lw.Close();
            }
            App.Desktop.MainWindow.Show();
            IsPlatformLoaded = true;
            MpConsole.WriteLine("Platform Load complete");


            //var cil = await MpDataModelProvider.GetItemsAsync<MpCopyItem>();
            //foreach(var ci in cil) {
            //    var doil = await MpDataModelProvider.GetDataObjectItemsByDataObjectId(ci.DataObjectId);

            //    switch (ci.ItemType) {
            //        case MpCopyItemType.Text:
            //            if(doil.FirstOrDefault(x=>x.ItemFormat == MpPortableDataFormats.Text) is MpDataObjectItem doi) {
            //                ci.ItemSize1 = doi.ItemData.Length;
            //                ci.ItemSize2 = doi.ItemData.Split("\n").Length + 1;
            //            } else {
            //                Debugger.Break();
            //            }
            //            break;
            //        case MpCopyItemType.Image:
            //            var bmp = ci.ItemData.ToAvBitmap();
            //            ci.ItemSize1 = bmp.PixelSize.Width;
            //            ci.ItemSize2 = bmp.PixelSize.Height;
            //            break;
            //        case MpCopyItemType.FileList:
            //            ci.ItemSize1 = 0;
            //            ci.ItemSize2 = doil.Where(x => x.ItemFormat == MpPortableDataFormats.AvFileNames).Count();
            //            break;
            //    }
            //    await ci.WriteToDatabaseAsync();
            //}

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
                    new MpBootstrappedItemViewModel(this,typeof(MpAvTriggerCollectionViewModel))
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