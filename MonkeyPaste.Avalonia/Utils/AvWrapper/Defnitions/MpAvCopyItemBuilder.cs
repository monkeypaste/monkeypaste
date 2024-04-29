using Cairo;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCopyItemBuilder {
        #region Private Variables
        #endregion

        #region Statics

        private static int _LastAddId = -1;
        #endregion



        #region Properties
        #endregion

        #region Events
        // arg is ci guid, icon id
        public event EventHandler<(string ciguid,int iconid,bool rejected)> OnSourceInfoGathered;

        #endregion

        #region Public Methods

        public async Task<MpCopyItem> BuildAsync(
            MpAvDataObject avdo,
            MpTransactionType transType = MpTransactionType.None,
            bool force_allow_dup = false) {
            MpCopyItem result = null;
//#if LINUX
//            result = await BuildAsync_linux(avdo, transType, force_allow_dup);
//#else
            result = await BuildAsync_default(avdo, transType, force_allow_dup);
//#endif
            return result;
        }

        #endregion

        #region Private Methods
        private async Task<MpCopyItem> BuildAsync_default(
            MpAvDataObject avdo,
            MpTransactionType transType = MpTransactionType.None,
            bool force_allow_dup = false) {
            if (avdo == null || avdo.DataFormatLookup.Count == 0) {
                return null;
            }
            if (transType == MpTransactionType.None) {
                throw new Exception("Must have transacion type");
            }

            (MpCopyItemType itemType,
                string itemData,
                string itemDelta,
                string itemPlainText) = await DecodeContentDataAsync(avdo);

            if (itemType == MpCopyItemType.None ||
                itemData == null) {
                MpConsole.WriteLine("Warning! CopyItemBuilder could not create itemData");
                return null;
            }

            (MpCopyItem ci, string checksum) = await PerformDupCheckAsync(
                compare_data: itemPlainText,
                itemType: itemType,
                force_allow_dup, false);

            string ci_guid = ci == null ? System.Guid.NewGuid().ToString() : ci.Guid;
            CancellationTokenSource source_cts = new CancellationTokenSource();

            BuildSupplementalsAsync(ci_guid, avdo, ci == null, transType, source_cts.Token).FireAndForgetSafeAsync();


            if (ci == null) {
                // new, non-duplicate or don't care
                var dobj = await MpDataObject.CreateAsync(pdo: avdo);
                string default_title = await GetDefaultItemTitleAsync(itemType, avdo);

                MpDataObjectSourceType dataObjectSourceType = avdo.GetDataObjectSourceType();

                ci = await MpCopyItem.CreateAsync(
                    guid: ci_guid,
                    dataObjectId: dobj.Id,
                    title: default_title,
                    data: itemData,
                    itemType: itemType,
                    checksum: checksum,
                    dataObjectSourceType: dataObjectSourceType,
                    suppressWrite: false);
                if (ci == null) {
                    // probably null data, clean up pre-create
                    source_cts.Cancel();
                    await dobj.DeleteFromDatabaseAsync();
                    return null;
                }
            } else {
                if (transType == MpTransactionType.Created) {
                    MpConsole.WriteLine($"Item '{ci}' duplication detected. Marking '{MpTransactionType.Created}' as '{MpTransactionType.Recreated}'");
                    // try to prevent multiple 'create' transactions, 'Recreate' will imply dup ref
                    transType = MpTransactionType.Recreated;
                }
            }
            return ci;
        }

        private async Task<MpCopyItem> BuildAsync_linux(
            MpAvDataObject avdo,
            MpTransactionType transType = MpTransactionType.None,
            bool force_allow_dup = false) {
            if (avdo == null || avdo.DataFormatLookup.Count == 0) {
                return null;
            }
            if (transType == MpTransactionType.None) {
                throw new Exception("Must have transacion type");
            }

            (MpCopyItemType itemType,
                string itemData,
                string itemDelta,
                string itemPlainText) = await DecodeContentDataAsync(avdo);

            if (itemType == MpCopyItemType.None ||
                itemData == null) {
                MpConsole.WriteLine("Warning! CopyItemBuilder could not create itemData");
                return null;
            }

            (MpCopyItem ci, string checksum) = await PerformDupCheckAsync(
                compare_data: itemPlainText,
                itemType: itemType,
                force_allow_dup, false);

            IEnumerable<MpISourceRef> refs = await Mp.Services.SourceRefTools.GatherSourceRefsAsync(avdo);

            if (Mp.Services.SourceRefTools.IsAnySourceRejected(refs)) {
                return null;
            }

            bool is_new = ci == null;
            if (is_new) {
                // new, non-duplicate or don't care
                var dobj = await MpDataObject.CreateAsync(pdo: avdo);
                string default_title = await GetDefaultItemTitleAsync(itemType, avdo);

                MpDataObjectSourceType dataObjectSourceType = avdo.GetDataObjectSourceType();
                string ci_guid = is_new ? System.Guid.NewGuid().ToString() : ci.Guid;
                int icon_id = PickIconIdFromSourceRefs(refs);
                ci = await MpCopyItem.CreateAsync(
                    guid: ci_guid,
                    dataObjectId: dobj.Id,
                    iconId: icon_id,
                    title: default_title,
                    data: itemData,
                    itemType: itemType,
                    checksum: checksum,
                    dataObjectSourceType: dataObjectSourceType,
                    suppressWrite: false);
                if (ci == null) {
                    // probably null data, clean up pre-create
                    //source_cts.Cancel();
                    await dobj.DeleteFromDatabaseAsync();
                    return null;
                }
            } else {
                if (transType == MpTransactionType.Created) {
                    MpConsole.WriteLine($"Item '{ci}' duplication detected. Marking '{MpTransactionType.Created}' as '{MpTransactionType.Recreated}'");
                    // try to prevent multiple 'create' transactions, 'Recreate' will imply dup ref
                    transType = MpTransactionType.Recreated;
                }
            }

            List<string> ref_urls = refs.Select(x => Mp.Services.SourceRefTools.ConvertToInternalUrl(x)).ToList();
            if (avdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> urls)) {
                var urlList = urls.ToList();
                for (int i = 0; i < ref_urls.Count; i++) {
                    var provided_url = urlList.FirstOrDefault(x => x.ToLower().StartsWith(ref_urls[i].ToLower()));
                    if (provided_url != null) {
                        // prefer provided url in case it has args and remove so not added later
                        ref_urls[i] = provided_url;
                        urlList.Remove(provided_url);
                    }
                }
                // add remaining urls for transaction
                if (urlList.Count > 0) {
                    ref_urls.AddRange(urlList);
                }
            }
            await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                            copyItemId: ci.Id,
                            reqType: MpJsonMessageFormatType.DataObject,
                            //req: avdo.SerializeData(),
                            respType: MpJsonMessageFormatType.Delta,
                            //resp: string.IsNullOrEmpty(itemDelta) ? ci.ToDelta():itemDelta,
                            ref_uris: ref_urls,
                            transType: transType);
            return ci;
        }

        #region Source Helpers
        private async Task BuildSupplementalsAsync(
            string ci_guid, 
            MpAvDataObject avdo, 
            bool is_new,
            MpTransactionType transType,
            CancellationToken ct) {
            // source (supplementals) gathering is off-loaded as a subtask to decrease load time

            IEnumerable<MpISourceRef> refs = await Mp.Services.SourceRefTools.GatherSourceRefsAsync(avdo);
            
            if (Mp.Services.SourceRefTools.IsAnySourceRejected(refs)) {
                OnSourceInfoGathered?.Invoke(this, (ci_guid,0,true));
                return;
            }

            List<string> ref_urls = refs.Select(x => Mp.Services.SourceRefTools.ConvertToInternalUrl(x)).ToList();
            if (avdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> urls)) {
                var urlList = urls.ToList();
                for (int i = 0; i < ref_urls.Count; i++) {
                    var provided_url = urlList.FirstOrDefault(x => x.ToLower().StartsWith(ref_urls[i].ToLower()));
                    if (provided_url != null) {
                        // prefer provided url in case it has args and remove so not added later
                        ref_urls[i] = provided_url;
                        urlList.Remove(provided_url);
                    }
                }
                // add remaining urls for transaction
                if (urlList.Count > 0) {
                    ref_urls.AddRange(urlList);
                }
            }
            if (ct.IsCancellationRequested) {
                // probably empty data
                return;
            }
            int icon_id = -1;
            if (is_new) {
                // arg is ci guid
                icon_id = PickIconIdFromSourceRefs(refs);
            }
            if (ct.IsCancellationRequested) {
                // probably empty data
                return;
            }
            int ciid = await MpDataModelProvider.GetItemIdByGuidAsync<MpCopyItem>(ci_guid);
            if(ciid <= 0) {
                var sw = Stopwatch.StartNew();
                while (ciid <= 0) {
                    if(sw.Elapsed > TimeSpan.FromSeconds(5)) {
                        MpConsole.WriteLine($"Ci builder error cannot find ci w/ guid '{ci_guid}'. Timed out, shouldn't happen...");
                        return;
                    }
                    MpConsole.WriteLine($"Ci builder error cannot find ci w/ guid '{ci_guid}'. Will retry...");
                    await Task.Delay(100);
                    ciid = await MpDataModelProvider.GetItemIdByGuidAsync<MpCopyItem>(ci_guid);
                }
            }
            await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                            copyItemId: ciid,
                            reqType: MpJsonMessageFormatType.DataObject,
                            //req: avdo.SerializeData(),
                            respType: MpJsonMessageFormatType.Delta,
                            //resp: string.IsNullOrEmpty(itemDelta) ? ci.ToDelta():itemDelta,
                            ref_uris: ref_urls,
                            transType: transType);
            OnSourceInfoGathered?.Invoke(this, (ci_guid,icon_id,false));
        }
        #endregion

        #region Content Helpers

        private int PickIconIdFromSourceRefs(IEnumerable<MpISourceRef> refs) {
            if (refs == null || !refs.Any()) {
                return 0;
            }
            // find highest priority source with existing db icon
            var primary_source =
                refs
                // search sources with icons defined
                .Where(x => x.IconResourceObj is int iconId && iconId > 0)
                // sort this app and unknown to bottom
                .OrderByDescending(x => ((int)x.IconResourceObj) == MpDefaultDataModelTools.UnknownIconId || ((int)x.IconResourceObj) == MpDefaultDataModelTools.ThisAppIconId ? 0 : 1)
                // sub-sort highest by highest priority
                .ThenByDescending(x => x.Priority)
                .FirstOrDefault();
            if (primary_source == null) {
                return MpDefaultDataModelTools.UnknownIconId;
            }
            // TODO? when primary source is a copy item
            // this will returns its primary icon id which is fine 
            // but maybe it could be contextual, not sure
            return (int)primary_source.IconResourceObj;
        }
        private async Task<(MpCopyItem, string)> PerformDupCheckAsync(
            string compare_data,
            MpCopyItemType itemType,
            bool force_allow_dup,
            bool suppressWrite) {
            string pre_item_checksum = MpCopyItem.GetContentCheckSum(compare_data);
            if (!MpAvPrefViewModel.Instance.IsDuplicateCheckEnabled ||
                force_allow_dup ||
                suppressWrite) {
                return (null, pre_item_checksum);
            }
            var dups = await MpDataModelProvider.GetCopyItemByCheckSumAsync(pre_item_checksum);
            if (dups.Count > 1) {
                MpConsole.WriteLine($"Warning! multiple dups detected. There should only be 1 or dup check was disabled and later re-enabled. Returning most recent by cap datetime...");
            }
            if (dups.OrderByDescending(x => x.CopyDateTime).FirstOrDefault() is not MpCopyItem dupCheck) {
                return (null, pre_item_checksum);
            }

            MpConsole.WriteLine($"Duplicate item detected, returning original id:'{dupCheck.Id}'");
            dupCheck.WasDupOnCreate = true;
            return (dupCheck, null);
        }

        private string[] _CommonDataFormatsByPriority = new[] {
            //
            MpPortableDataFormats.Text3,
            MpPortableDataFormats.Text2,
            MpPortableDataFormats.Text,
            MpPortableDataFormats.Html,
            MpPortableDataFormats.Xhtml,
            MpPortableDataFormats.Image2,
            MpPortableDataFormats.Image,
            MpPortableDataFormats.Rtf,
            MpPortableDataFormats.Csv,
            MpPortableDataFormats.Files,
        };

        private int GetFormatPriority(MpAvDataObject avdo, string format) {
            int priority = _CommonDataFormatsByPriority.IndexOf(format);

            if (format == MpPortableDataFormats.Files &&
                avdo.TryGetData(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, out string internal_cit_str) &&
                internal_cit_str.ToEnum<MpCopyItemType>() is MpCopyItemType cit &&
                cit != MpCopyItemType.FileList) {
                // for pseudo files give low priority
                return -1;
            }
            if (format == MpPortableDataFormats.Rtf &&
                (
                avdo.ContainsData(MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT) ||
                avdo.ContainsData(MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT)) &&
                (avdo.ContainsData(MpPortableDataFormats.Html) || avdo.ContainsData(MpPortableDataFormats.Xhtml))) {
                // for html->rtf prefer original html
                return -1;
            }
            return priority;
        }
        private string GetItemDataByPriority(MpAvDataObject avdo, out MpDataFormatType inputFormatType) {
            inputFormatType = MpDataFormatType.None;
            if (!_CommonDataFormatsByPriority.Any(x => avdo.ContainsData(x))) {
                // no content formats
                return null;
            }

            string max_format =
                avdo
                .GetAllDataFormats()
                .OrderByDescending(x => GetFormatPriority(avdo, x))
                .FirstOrDefault();

            if (!avdo.TryGetData(max_format, out string itemData)) {
                return null;
            }

            switch (max_format) {
                case var _ when max_format == MpPortableDataFormats.Text3:
                case var _ when max_format == MpPortableDataFormats.Text2:
                case var _ when max_format == MpPortableDataFormats.Text:
                    inputFormatType = MpDataFormatType.PlainText;
                    break;
                case var _ when max_format == MpPortableDataFormats.Html:
                case var _ when max_format == MpPortableDataFormats.Xhtml:

                    // NOTE to avoid loosing rtf markup the converted html is 
                    // fully html special entities are fully encoded which will lead
                    // to parsing issues if left as is or double encoding if not considered
                    inputFormatType =
                        avdo.ContainsData(MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT) ?
                        MpDataFormatType.Rtf2Html :
                        MpDataFormatType.Html;
                    break;
                case var _ when max_format == MpPortableDataFormats.Rtf:
                    // NOTE should only happen if user has disabled (its default) convert rtf2html 
                    //MpDebug.Assert(!avdo.ContainsData(MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT), $"CopyItem builder error trying to use conversion data instead of source");
                    //MpDebug.Assert(
                    //    avdo.ContainsData(MpPortableDataFormats.Html) ||
                    //    avdo.ContainsData(MpPortableDataFormats.Xhtml),
                    //    $"CopyItem builder error should prefer html over rtf when available");

                    itemData = itemData.RtfToHtml();
                    inputFormatType = MpDataFormatType.Rtf2Html;
                    break;
                case var _ when max_format == MpPortableDataFormats.Csv:
                    itemData = itemData.CsvStrToRichHtmlTable();
                    inputFormatType = MpDataFormatType.Html;
                    //if (avdo.ContainsData(MpPortableDataFormats.AvRtf_bytes) && 
                    //    avdo.GetData(MpPortableDataFormats.AvRtf_bytes) is byte[] rtfCsvBytes) {
                    //    // NOTE this is assuming the content is a rich text table. But it may not be 
                    //    // depending on the source so may need to be careful handling these. 
                    //    itemType = MpCopyItemType.Text;
                    //    itemData = rtfCsvBytes.ToDecodedString().EscapeExtraOfficeRtfFormatting();
                    //    itemData = itemData.ToRichHtmlText(MpPortableDataFormats.AvRtf_bytes);
                    //} else {
                    //    string csvStr = avdo.GetData(MpPortableDataFormats.AvCsv).ToString();
                    //    //itemData = csvStr.ToRichText();
                    //    itemData = itemData.ToRichHtmlText(MpPortableDataFormats.AvCsv);
                    //}
                    break;
                case var _ when max_format == MpPortableDataFormats.Image:
                case var _ when max_format == MpPortableDataFormats.Image2:
                    // NOTE this is just a filler here, haven't had need to discern images
                    inputFormatType = MpDataFormatType.Bmp;
                    break;
                case var _ when max_format == MpPortableDataFormats.Files:
                    inputFormatType = MpDataFormatType.FileList;
                    break;
            }
            return itemData;
        }

        private async Task<(MpCopyItemType, string, string, string)> DecodeContentDataAsync(MpAvDataObject avdo) {
            string itemData = GetItemDataByPriority(avdo, out MpDataFormatType inputTextFormat);
            MpCopyItemType itemType = inputTextFormat.ToCopyItemType();

            if (itemType == MpCopyItemType.None) {
                MpConsole.WriteTraceLine("clipboard compare_data is not known format");
                return default;
            }

            string itemPlainText = null;
            if (avdo.TryGetData(MpPortableDataFormats.Text, out string pt)) {
                itemPlainText = pt;
            }

            string delta = null;

            // POST-PROCESS (TEXT ONLY)

            if (itemType == MpCopyItemType.Text) {
                MpRichHtmlContentConverterResult htmlClipboardData =
                    await MpAvPlainHtmlConverter.Instance.ConvertAsync(
                        itemData,
                        inputTextFormat,
                        itemPlainText);

                if (htmlClipboardData == null) {
                    itemData = null;
                } else {
                    if (!string.IsNullOrEmpty(htmlClipboardData.SourceUrl)) {
                        // add url ref if found so won't have to find again
                        avdo.AddOrUpdateUri(htmlClipboardData.SourceUrl);
                    }
                    itemData = htmlClipboardData.OutputData;
                    delta = htmlClipboardData.Delta;
                    if (htmlClipboardData.DeterminedFormat == MpPortableDataFormats.Image) {
                        // browser image copy handling
                        itemType = MpCopyItemType.Image;
                        delta = null;
                    } 
                    //else if (!MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                    //    //plain text mode, just use plain text for now
                    //    itemData = itemData.ToPlainText();
                    //}
                }
            }

            if (MpAvPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems &&
                itemType == MpCopyItemType.Text &&
                itemData.IsNullOrWhitespaceHtmlString()) {
                // if text is just whitespace and those ignored flag to ignore item
                return default;
            }
            if (itemPlainText == null) {
                itemPlainText = itemData;
            }

            return (itemType, itemData, delta, itemPlainText);
        }

        private async Task<string> GetDefaultItemTitleAsync(MpCopyItemType itemType, MpAvDataObject avdo) {
            if (_LastAddId < 0) {
                _LastAddId = await MpDataModelProvider.GetLastRowIdAsync<MpCopyItem>();
            }
            _LastAddId++;

            string default_title = null;
            if (avdo.ContainsData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT)) {
                default_title = avdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT) as string;
            }
            if (string.IsNullOrEmpty(default_title)) {
                string def_prefix =
                    itemType == MpCopyItemType.Text ?
                        UiStrings.ClipTileDefTitleTextPrefix :
                        itemType == MpCopyItemType.FileList ?
                            UiStrings.ClipTileDefTitleFilesPrefix :
                            UiStrings.ClipTileDefTitleImagePrefix;
                default_title = $"{def_prefix}{(_LastAddId.ToCommaSeperatedIntString())}";
            }
            return default_title;
        }
        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
