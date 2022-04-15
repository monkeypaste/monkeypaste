using System.Linq;
using System.Reflection;
using MonkeyPaste;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using MpProcessHelper;
using MonkeyPaste.Plugin;
using System.IO;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpWpfBootstrapperViewModel : MpBootstrapperViewModelBase {

        public MpWpfBootstrapperViewModel(MpINativeInterfaceWrapper niw) : base(niw) {
            if(_items == null) {
                _items = new List<MpBootstrappedItem>();
            }

            _items.AddRange(
                new List<MpBootstrappedItem>() {
                    //new MpBootstrappedItem(typeof(MpDocumentHtmlExtension)),
                    new MpBootstrappedItem(typeof(MpProcessManager)),
                    new MpBootstrappedItem(typeof(MpProcessAutomation)),

                    new MpBootstrappedItem(typeof(MpThemeColors)),

                    new MpBootstrappedItem(typeof(MpMeasurements)),
                    new MpBootstrappedItem(typeof(MpFileSystemWatcher)),

                    new MpBootstrappedItem(typeof(MpIconCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpAppCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpUrlCollectionViewModel)),
                    new MpBootstrappedItem(typeof(MpSourceCollectionViewModel)),


                    new MpBootstrappedItem(typeof(MpSystemTrayViewModel)),

                    new MpBootstrappedItem(typeof(MpSoundPlayerGroupCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpClipTileSortViewModel)),
                    new MpBootstrappedItem(typeof(MpSearchBoxViewModel)),

                    new MpBootstrappedItem(typeof(MpAnalyticItemCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpClipTrayViewModel)),

                    new MpBootstrappedItem(typeof(MpShortcutCollectionViewModel)),


                    new MpBootstrappedItem(typeof(MpTagTrayViewModel)),
                    new MpBootstrappedItem(typeof(MpMainWindowViewModel)),

                    new MpBootstrappedItem(typeof(MpActionCollectionViewModel)),

                    new MpBootstrappedItem(typeof(MpContextMenu)),

                    new MpBootstrappedItem(typeof(MpDragDropManager)),

                    new MpBootstrappedItem(typeof(MpClipboardHelper.MpClipboardManager),MpWpfDataObjectHelper.Instance),

                    new MpBootstrappedItem(typeof(MpWpfDataObjectHelper)),
                    new MpBootstrappedItem(typeof(MpDataObject), new MpWpfPasteObjectBuilder())
                    //new MpBootstrappedItem(typeof(MpMouseHook))
                });
        }

        public override async Task Init() {
            // NOTE Remove this later start
            Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr = string.Empty;
            Properties.Settings.Default.Save();
            // NOTE Remove this later finish


            List<int> doNotShowNotifications = null;
            if(!string.IsNullOrWhiteSpace(Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr)) {
                doNotShowNotifications = Properties.Settings.Default.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToList();
            }

            await MpNotificationCollectionViewModel.Instance.Init(doNotShowNotifications);

            var nw = new MpNotificationWindow();
            await MpNotificationCollectionViewModel.Instance.RegisterWithWindow(nw);
            nw.DataContext = MpNotificationCollectionViewModel.Instance;

            await MpNotificationCollectionViewModel.Instance.BeginLoader(this);

            for (int i = 0; i < _items.Count; i++) {
                ReportItemLoading(_items[i], i);
                await _items[i].Register();
            }

            MpNotificationCollectionViewModel.Instance.FinishLoading();

            //MpRtfToHtmlConverter.Test();

            //var cil = await MpDb.GetItemsAsync<MpCopyItem>();
            //foreach (var ci in cil) {
            //    try {
            //        if (ci.ItemType != MpCopyItemType.Text || string.IsNullOrEmpty(ci.ItemData_rtf)) {
            //            continue;
            //        }

            //        ci.ItemData = ci.ItemData_rtf;
            //        ci.ItemData_rtf = string.Empty;
            //        await ci.WriteToDatabaseAsync();
            //    }
            //    catch (Exception ex) {
            //        Debugger.Break();
            //    }
            //}

            //foreach (var ci in cil) {
            //    try {
            //        if (ci.ItemType != MpCopyItemType.Text || ci.Id <= 2189) {
            //            continue;
            //        }
            //        ci.ItemData_rtf = ci.ItemData;

            //        string itemHtml = MpRtfToHtmlConverter.ConvertRtfToHtml(ci.ItemData,
            //            new Dictionary<string, string>() { { "copyItemBlockGuid", ci.Guid } },
            //            new Dictionary<string, string>() { { "copyItemInlineGuid", ci.Guid } });

            //        var ccil = await MpDataModelProvider.GetCompositeChildrenAsync(ci.Id);
            //        if (ccil.Count > 0) {
            //            ci.RootCopyItemGuid = string.Empty;
            //            foreach (var cci in ccil.OrderBy(x => x.CompositeSortOrderIdx)) {
            //                string encodedItemStr = string.Format(
            //                    @"{0}{1}{2}",
            //                    "{c{",
            //                    cci.Guid,
            //                    "}c}");
            //                itemHtml += encodedItemStr;
            //            }
            //        } else if (ci.CompositeParentCopyItemId > 0) {
            //            var pci = await MpDb.GetItemAsync<MpCopyItem>(ci.CompositeParentCopyItemId);
            //            ci.RootCopyItemGuid = pci.Guid;
            //        } else {
            //            ci.RootCopyItemGuid = string.Empty;
            //        }
            //        ci.ItemData = itemHtml;
            //        await ci.WriteToDatabaseAsync();
            //    } catch(Exception ex) {
            //        Debugger.Break();
            //    }
            //}

            IsLoaded = true;
        }
    }
}