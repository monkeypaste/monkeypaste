using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using MpProcessHelper;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.IO;
using System.Windows.Media;
using MpClipboardHelper;
using System.Collections;

namespace MpWpfApp {
    public class MpWpfBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpWpfBootstrapperViewModel(MpIPlatformWrapper niw) : base(niw) {
            if(_items == null) {
                _items = new List<MpBootstrappedItemViewModel>();
            }

            _items.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpDocumentHtmlExtension)),
                    new MpBootstrappedItemViewModel(this,typeof(MpProcessManager), Properties.Settings.Default.IgnoredProcessNames),
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

                    new MpBootstrappedItemViewModel(this,typeof(MpClipTrayViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpShortcutCollectionViewModel)),


                    new MpBootstrappedItemViewModel(this,typeof(MpTagTrayViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMainWindowViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpActionCollectionViewModel)),

                    new MpBootstrappedItemViewModel(this,typeof(MpContextMenuView)),

                    new MpBootstrappedItemViewModel(this,typeof(MpDragDropManager)),

                    new MpBootstrappedItemViewModel(this,typeof(MpClipboardManager)),

                    new MpBootstrappedItemViewModel(this,typeof(MpWpfDataObjectHelper))
                    //new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task Init() {
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

            await MpNotificationCollectionViewModel.Instance.Init(doNotShowNotifications);

            var nw = new MpNotificationWindow();
            await MpNotificationCollectionViewModel.Instance.RegisterWithWindow(nw);
            //nw.DataContext = MpNotificationCollectionViewModel.Instance;

            await MpNotificationCollectionViewModel.Instance.BeginLoader(this);

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
            //        x => LoadItem(_items[_items.IndexOf(x)],_items.IndexOf(x))));

            // Sequential (58831 ms 05/30/2022)
            for (int i = 0; i < _items.Count; i++) {
                await LoadItem(_items[i], i);
            }

            MpPlatformWrapper.Services.ClipboardMonitor = MpClipboardManager.MonitorService;
            MpPlatformWrapper.Services.DataObjectRegistrar = MpClipboardManager.RegistrarService;
            MpPortableDataFormats.Init(MpPlatformWrapper.Services.DataObjectRegistrar);

            await Task.Delay(500);
            MpNotificationCollectionViewModel.Instance.FinishLoading();

            //MpRtfToHtmlConverter.Test();

            //var cil = await MpDb.GetItemsAsync<MpCopyItem>();
            //foreach (var ci in cil.Where(x=>x.ItemType != MpCopyItemType.Image)) {
            //    if(ci.ItemType == MpCopyItemType.Text) {
            //        int cci_sIdx = ci.ItemData.IndexOf(@"\{c\{");
            //        if(cci_sIdx >= 0) {
            //            ci.ItemData_rtf = ci.ItemData;
            //            while(cci_sIdx >= 0) {
            //                string endToken = @"\}c\}";
            //                int cci_eIdx = ci.ItemData.IndexOf(endToken);
            //                if(cci_eIdx < 0) {
            //                    Debugger.Break();
            //                }
            //                string encodeToReplace = ci.ItemData.Substring(cci_sIdx, cci_eIdx - cci_sIdx + endToken.Length);
            //                ci.ItemData = ci.ItemData.Replace(encodeToReplace, string.Empty);
            //                cci_sIdx = ci.ItemData.IndexOf(@"\{c\{");
            //            }
            //        }
            //    }

            //    ci.CompositeParentCopyItemId = 0;
            //    ci.CompositeSortOrderIdx = 0;
            //    ci.RootCopyItemGuid = string.Empty;
            //    ci.CopyItemSourceGuid = string.Empty;

            //    await ci.WriteToDatabaseAsync();
            //}
            //Debugger.Break();
            
            sw.Stop();
            MpConsole.WriteLine($"Bootstrapper loaded in {sw.ElapsedMilliseconds} ms");

            IsLoaded = true;
        }
    }
}