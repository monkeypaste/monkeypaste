using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Droid {
    public class MpAndroidBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpAndroidBootstrapperViewModel(MpIPlatformWrapper niw) : base(niw) {
            if(_items == null) {
                _items = new List<MpBootstrappedItem>();
            }

            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    //new MpBootstrappedItem(typeof(MpDocumentHtmlExtension)),
                    //new MpBootstrappedItem(typeof(MpProcessManager), Properties.Settings.Default.IgnoredProcessNames),
                    //new MpBootstrappedItem(typeof(MpProcessAutomation)),

                    //new MpBootstrappedItem(typeof(MpThemeColors)),

                    //new MpBootstrappedItem(typeof(MpMeasurements)),
                    //new MpBootstrappedItem(typeof(MpFileSystemWatcher)),

                    //new MpBootstrappedItem(typeof(MpIconCollectionViewModel)),
                    //new MpBootstrappedItem(typeof(MpAppCollectionViewModel)),
                    //new MpBootstrappedItem(typeof(MpUrlCollectionViewModel)),
                    //new MpBootstrappedItem(typeof(MpSourceCollectionViewModel)),


                    //new MpBootstrappedItem(typeof(MpSystemTrayViewModel)),

                    //new MpBootstrappedItem(typeof(MpSoundPlayerGroupCollectionViewModel)),

                    //new MpBootstrappedItem(typeof(MpClipTileSortViewModel)),
                    //new MpBootstrappedItem(typeof(MpSearchBoxViewModel)),

                    //new MpBootstrappedItem(typeof(MpAnalyticItemCollectionViewModel)),

                    //new MpBootstrappedItem(typeof(MpClipTrayViewModel)),

                    //new MpBootstrappedItem(typeof(MpShortcutCollectionViewModel)),


                    //new MpBootstrappedItem(typeof(MpTagTrayViewModel)),
                    //new MpBootstrappedItem(typeof(MpMainWindowViewModel)),

                    //new MpBootstrappedItem(typeof(MpActionCollectionViewModel)),

                    //new MpBootstrappedItem(typeof(MpContextMenu)),

                    //new MpBootstrappedItem(typeof(MpDragDropManager)),

                    //new MpBootstrappedItem(typeof(MpClipboardHelper.MpClipboardManager),MpWpfDataObjectHelper.Instance),

                    //new MpBootstrappedItem(typeof(MpWpfDataObjectHelper))
                    //new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task Init() {
            // NOTE Move this later (to first load init native data in app.cs) start
            //Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr = string.Empty;

            //Properties.Settings.Default.IgnoredProcessNames =
            //    "csrss" + Environment.NewLine + //Client Server Runtime Subsystem
            //    "dwm" + Environment.NewLine + //desktop window manager
            //    "mmc"; // Microsoft Management Console (like event viewer)


            //Properties.Settings.Default.Save();
            //// NOTE Remove this later finish


            //List<int> doNotShowNotifications = null;
            //if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr)) {
            //    doNotShowNotifications = Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr
            //        .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
            //        .Select(x => Convert.ToInt32(x)).ToList();
            //}

            //await MpNotificationCollectionViewModel.Instance.Init(doNotShowNotifications);

            //var nw = new MpNotificationWindow();
            //await MpNotificationCollectionViewModel.Instance.RegisterWithWindow(nw);
            //nw.DataContext = MpNotificationCollectionViewModel.Instance;

            //await MpNotificationCollectionViewModel.Instance.BeginLoader(this);

            for (int i = 0; i < _items.Count; i++) {
                //ReportItemLoading(_items[i], i);
                await _items[i].Register();
            }

            //MpPlatformWrapper.Services.ClipboardMonitor = MpClipboardHelper.MpClipboardManager.MonitorService;

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

            IsLoaded = true;
        }
    }
}