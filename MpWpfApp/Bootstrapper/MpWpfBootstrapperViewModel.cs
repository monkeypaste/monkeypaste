using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MpProcessHelper;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using MonkeyPaste.Common.Wpf;
using System.IO;
using System.Windows.Media;
using System.Collections;

namespace MpWpfApp {
    public class MpWpfBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpWpfBootstrapperViewModel() : base() {
            if(_coreItems == null) {
                _coreItems = new List<MpBootstrappedItemViewModel>();
            }

            _coreItems.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpDocumentHtmlExtension)),
                    new MpBootstrappedItemViewModel(this,typeof(MpProcessManager)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpProcessAutomation)),
                    new MpBootstrappedItemViewModel(this,typeof(MpScreenInformation)),
                    new MpBootstrappedItemViewModel(this,typeof(MpThemeColors)),

                    new MpBootstrappedItemViewModel(this,typeof(MpMeasurements)),
                    new MpBootstrappedItemViewModel(this,typeof(MpFileSystemWatcher)),

                    new MpBootstrappedItemViewModel(this,typeof(MpIconCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpAppCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpUrlCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpSourceCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpSystemTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpClipTileSortViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpSearchBoxViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpAnalyticItemCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpClipboardHandlerCollectionViewModel)),

                    //new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),

                    new MpBootstrappedItemViewModel(this,typeof(MpClipTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpShortcutCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpTagTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMainWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpActionCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpContextMenuView)),

                    new MpBootstrappedItemViewModel(this,typeof(MpDragDropManager)),


                    new MpBootstrappedItemViewModel(this,typeof(MpWpfDataObjectHelper)),
                    new MpBootstrappedItemViewModel(this,typeof(MpQuillHtmlToRtfConverter)),
                    new MpBootstrappedItemViewModel(this,typeof(MpTooltipInfoCollectionViewModel))
                    //new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task InitAsync() {
            var sw = Stopwatch.StartNew();

            // NOTE Move this later (to first load init native data in app.cs) start
            Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr = string.Empty;

            Properties.Settings.Default.IgnoredProcessNames =
                "csrss" + Environment.NewLine + //Client Server Runtime Subsystem
                "dwm" + Environment.NewLine + //desktop window manager
                "mmc"; // Microsoft Management Console (like event viewer)


            Properties.Settings.Default.Save();


            List<int> doNotShowNotifications = null;
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr)) {
                doNotShowNotifications = Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToList();
            }

            await MpNotificationCollectionViewModel.Instance.InitAsync(doNotShowNotifications);

            var nw = new MpNotificationWindow();
            await MpNotificationCollectionViewModel.Instance.RegisterWithWindowAsync(nw);
            //nw.DataContext = MpNotificationCollectionViewModel.Instance;

            await MpNotificationCollectionViewModel.Instance.BeginLoaderAsync(this);

            await Task.Delay(300);
            // Parallel

            //await Task.Run(
            //    () => _items
            //            .AsParallel()
            //            .WithDegreeOfParallelism(_items.Count)
            //            .Select(x => x.LoadItem()).ToList());

            // Async
            //await Task.WhenAll(
            //    _items.Select(
            //        x => LoadItem(_items[_items.IndexOf(x)], _items.IndexOf(x))));

            // Sequential (58831 ms 05/30/2022)
            // Sequential (35649 06/12/2022)
            for (int i = 0; i < _coreItems.Count; i++) {
                await LoadItemAsync(_coreItems[i], i);
                while (_coreItems[i].IsBusy) {
                    await Task.Delay(100);
                }
            }

            await Task.Delay(500);
            MpNotificationCollectionViewModel.Instance.FinishLoading();

            //MpRtfToHtmlConverter.Test();

            //var cil = await MpDb.GetItemsAsync<MpCopyItem>();
            //foreach (var ci in cil) {
            //    switch(ci.ItemType) {
            //        case MpCopyItemType.Text:
            //            //var fd = ci.ItemData.ToFlowDocument();
            //            //var ds = fd.GetDocumentSize();
            //            //ci.ItemSize = new MpSize(ds.Width, ds.Height);
            //            //break;
            //            continue;
            //        case MpCopyItemType.Image:
            //            //var bmpSrc = ci.ItemData.ToBitmapSource();
            //            //ci.ItemSize = new MpSize(bmpSrc.PixelWidth, bmpSrc.PixelHeight);
            //            //break;
            //            continue;
            //        case MpCopyItemType.FileList:
            //            ci.ItemSize = new MpSize();
            //            foreach(var fp in ci.ItemData.Split(new string[] {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries)) {
            //                double width = ((Path.GetFileName(fp).Length + 1) * 16) + 6 + 2 + 16;
            //                ci.ItemHeight += (16 + 6 + 2);
            //                ci.ItemWidth = Math.Max(ci.ItemWidth, width);
            //            }
            //            break;
            //    }
            //    await ci.WriteToDatabaseAsync();
            //}
            //Debugger.Break();

           

            //MpQuillHtmlToRtfConverter.Test();
            sw.Stop();
            MpConsole.WriteLine($"Bootstrapper loaded in {sw.ElapsedMilliseconds} ms");

            MpClipTrayViewModel.Instance.OnPostMainWindowLoaded();

            //var contacts = await MpMasterTemplateModelCollectionViewModel.Instance.GetContacts();

            //var contacts = new List<MpIContact>();

            //var fetchers = MpPluginManager.Plugins.Where(x => x.Value.Component is MpIContactFetcherComponentBase).Select(x => x.Value.Component).Distinct();
            //foreach (var fetcher in fetchers) {
            //    if (fetcher is MpIContactFetcherComponent cfc) {
            //        contacts.AddRange(cfc.FetchContacts(null));
            //    } else if (fetcher is MpIContactFetcherComponentAsync cfac) {
            //        var results = await cfac.FetchContactsAsync(null);
            //        contacts.AddRange(results);
            //    }
            //}
            

            IsCoreLoaded = true;
        }

        protected override async Task LoadItemAsync(MpBootstrappedItemViewModel item, int index) {
            IsBusy = true;
            var sw = Stopwatch.StartNew();

            await item.LoadItemAsync();
            sw.Stop();

            LoadedCount++;
            MpConsole.WriteLine("Loaded " + item.Label + " at idx: " + index + " Load Count: " + LoadedCount + " Load Percent: " + PercentLoaded + " Time(ms): " + sw.ElapsedMilliseconds);

            OnPropertyChanged(nameof(PercentLoaded));

            OnPropertyChanged(nameof(Detail));

            Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

            int dotCount = index % 4;
            Title = "LOADING";
            for (int i = 0; i < dotCount; i++) {
                Title += ".";
            }

            IsBusy = false;
        }
    }
}