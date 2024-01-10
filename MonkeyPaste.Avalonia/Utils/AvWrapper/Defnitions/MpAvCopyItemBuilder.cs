using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #region Public Methods

        public async Task<MpCopyItem> BuildAsync(
            MpAvDataObject avdo,
            bool suppressWrite = false,
            MpTransactionType transType = MpTransactionType.None,
            bool force_allow_dup = false) {
            if (avdo == null || avdo.DataFormatLookup.Count == 0) {
                return null;
            }
            if (transType == MpTransactionType.None) {
                throw new Exception("Must have transacion type");
            }

            await NormalizePlatformFormatsAsync(avdo);

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
                force_allow_dup, suppressWrite);

            IEnumerable<MpISourceRef> refs = await Mp.Services.SourceRefTools.GatherSourceRefsAsync(avdo);

            if (Mp.Services.SourceRefTools.IsAnySourceRejected(refs)) {
                return null;
            }
            if (ci == null) {
                // new, non-duplicate or don't care
                var dobj = await MpDataObject.CreateAsync(pdo: avdo);

                string default_title = await GetDefaultItemTitleAsync(itemType, avdo);
                int itemIconId = PickIconIdFromSourceRefs(refs);

                ci = await MpCopyItem.CreateAsync(
                    dataObjectId: dobj.Id,
                    title: default_title,
                    data: itemData,
                    itemType: itemType,
                    iconId: itemIconId,
                    checksum: checksum,
                    suppressWrite: suppressWrite);
                if (ci == null) {
                    // probably null data, clean up pre-create
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

            if (!suppressWrite) {
                await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                            copyItemId: ci.Id,
                            reqType: MpJsonMessageFormatType.DataObject,
                            //req: avdo.SerializeData(),
                            respType: MpJsonMessageFormatType.Delta,
                            //resp: string.IsNullOrEmpty(itemDelta) ? ci.ToDelta():itemDelta,
                            ref_uris: ref_urls,
                            transType: transType);
            }

            return ci;
        }

        #endregion

        #region Private Methods

        #region Source Helpers
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
                case MpPortableDataFormats.Text3:
                case MpPortableDataFormats.Text2:
                case MpPortableDataFormats.Text:
                    inputFormatType = MpDataFormatType.PlainText;
                    break;
                case MpPortableDataFormats.Html:
                case MpPortableDataFormats.Xhtml:

                    // NOTE to avoid loosing rtf markup the converted html is 
                    // fully html special entities are fully encoded which will lead
                    // to parsing issues if left as is or double encoding if not considered
                    inputFormatType =
                        avdo.ContainsData(MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT) ?
                        MpDataFormatType.Rtf2Html :
                        MpDataFormatType.Html;
                    break;
                case MpPortableDataFormats.Rtf:
                    // NOTE should only happen if user has disabled (its default) convert rtf2html 
                    //MpDebug.Assert(!avdo.ContainsData(MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT), $"CopyItem builder error trying to use conversion data instead of source");
                    //MpDebug.Assert(
                    //    avdo.ContainsData(MpPortableDataFormats.Html) ||
                    //    avdo.ContainsData(MpPortableDataFormats.Xhtml),
                    //    $"CopyItem builder error should prefer html over rtf when available");

                    itemData = itemData.RtfToHtml();
                    inputFormatType = MpDataFormatType.Rtf2Html;
                    break;
                case MpPortableDataFormats.Csv:
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
                case MpPortableDataFormats.Image:
                case MpPortableDataFormats.Image2:
                    // NOTE this is just a filler here, haven't had need to discern images
                    inputFormatType = MpDataFormatType.Bmp;
                    break;
                case MpPortableDataFormats.Files:
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
                    } else if (!MpAvPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                        //plain text mode, just use plain text for now
                        itemData = itemData.ToPlainText();
                    }
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

        #region Platform Handling
        private async Task<MpAvDataObject> NormalizePlatformFormatsAsync(MpAvDataObject avdo) {
            if (OperatingSystem.IsAndroid()) {
                return avdo;
            }
            MpConsole.WriteLine($"Normalizing actual dataobject formats:  {string.Join(",", avdo.GetAllDataFormats().Select(x => x))}");

            // foreach(var af in actual_formats) {
            //     MpConsole.WriteLine("Actual available format: " + af);
            //     object af_data = await Application.Current.Clipboard.GetDataAsync(af);
            //     if(af_data == null) {
            //         MpConsole.WriteLine("data null");
            //         continue;
            //     }
            //     if(af_data is string af_data_str) {
            //         MpConsole.WriteLine("(string)");
            //         MpConsole.WriteLine(af_data_str);
            //     } else if(af_data is IEnumerable<string> strl) {
            //         MpConsole.WriteLine("(string[]");
            //         strl.ForEach(x => MpConsole.WriteLine(x));
            //     } else if(af_data is byte[] bytes && bytes.ToDecodedString() is string bytes_str) {
            //         MpConsole.WriteLine("(bytes)");
            //         MpConsole.WriteLine(bytes_str);
            //     } else {
            //         MpConsole.WriteLine("(unknown): " + af_data.GetType());
            //     }
            // }

            if (OperatingSystem.IsLinux()) {
                var actual_formats = await MpAvCommonTools.Services.DeviceClipboard.GetFormatsSafeAsync();
                // linux doesn't case non-html formats the same as windows so mapping them here
                bool isLinuxFileList = avdo.ContainsData(MpPortableDataFormats.MimeText) &&
                                    actual_formats.Contains(MpPortableDataFormats.LinuxGnomeFiles);
                if (isLinuxFileList) {
                    // NOTE avalonia doesn't acknowledge files (no 'FileNames' entry) on Ubuntu 22.04
                    // and is beyond support for the clipboard plugin right now so..
                    // TODO eventually should tidy up clipboard handling so plugins are clear example code
                    string files_text_base64 = avdo.GetData(MpPortableDataFormats.MimeText) as string;
                    if (!string.IsNullOrEmpty(files_text_base64)) {
                        string files_text = files_text_base64.ToStringFromBase64();
                        MpConsole.WriteLine("Got file text: " + files_text);
                        avdo.SetData(MpPortableDataFormats.Files, files_text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                    }

                } else {
                    bool isLinuxAndNeedsCommonPlainText = avdo.ContainsData(MpPortableDataFormats.MimeText) &&
                                                            !avdo.ContainsData(MpPortableDataFormats.Text);
                    if (isLinuxAndNeedsCommonPlainText) {
                        string plain_text = avdo.GetData(MpPortableDataFormats.MimeText) as string;
                        avdo.SetData(MpPortableDataFormats.Text, plain_text);
                    }
                }
            }
            MpConsole.WriteLine($"DataObject format normalization complete. Available dataobject formats: {string.Join(",", avdo.DataFormatLookup.Select(x => x.Key.Name))}");
            return avdo;
        }

        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
