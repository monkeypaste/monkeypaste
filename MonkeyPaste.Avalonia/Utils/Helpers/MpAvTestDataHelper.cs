using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvTestDataHelper {
        public static async Task InitHelpContentAsync() {
            // NOTE called in clip tray init when initial startup is flagged

            var helpContentDefinitions = new List<string[]> {
                new string[] {
                    "Welcome to the jungle!",
                    "<h1>Monkey paste is the <b>best</b> am I <i>right</i>?!</h1>"
                },
                new string[] {
                    "Help Test 1",
                    "<h1>Here at Monkey paste we earn our bananas by aiding you with business logic automation</h1>"
                }
            };

            var thisApp = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            var thisAppRef = Mp.Services.SourceRefTools.ConvertToRefUrl(thisApp);

            var hci_idl = new List<int>();
            foreach (var hcd in helpContentDefinitions) {
                var hci_mpdo = new MpPortableDataObject() {
                    DataFormatLookup = new Dictionary<MpPortableDataFormat, object>() {
                            {
                                MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT),
                                hcd[0]
                            },
                            {
                                MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefHtml),
                                hcd[1]
                            }
                        }
                };

                //var hci_do = await MpDataObject.CreateAsync(pdo: hci_mpdo);

                //var hci = await MpCopyItem.CreateAsync(
                //    title: hcd[0],
                //    data: hcd[1],
                //    dataObjectId: hci_do.Id);

                //await MpPlatform.Services.TransactionBuilder.ReportTransactionAsync(
                //            copyItemId: hci.Id,
                //            reqType: MpJsonMessageFormatType.DataObject,
                //            req: hci_mpdo.SerializeData(),
                //            respType: MpJsonMessageFormatType.None,
                //            resp: null,
                //            ref_urls: new[] { thisAppRef },
                //            transType: MpTransactionType.Created);

                var hci = await Mp.Services.CopyItemBuilder.BuildAsync(
                    pdo: hci_mpdo,
                    transType: MpTransactionType.System,
                    force_ext_sources: false);

                hci_idl.Add(hci.Id);
            }

            await Task.WhenAll(hci_idl.Select((x, idx) => MpCopyItemTag.CreateAsync(
                tagId: MpTag.HelpTagId,
                copyItemId: x,
                sortIdx: idx)));
        }
    }
}
