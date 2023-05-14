using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCopyItemBuilder : MpICopyItemBuilder {
        #region Private Variables
        #endregion

        #region Statics
        private static int _UniqueItemCount = -1;
        public static int UniqueItemCount {
            get {
                if (_UniqueItemCount < 0) {
                    _UniqueItemCount = MpPrefViewModel.Instance.UniqueContentItemIdx;
                }
                return _UniqueItemCount;
            }
            set {
                if (UniqueItemCount != value) {
                    _UniqueItemCount = value;
                    Task.Run(() => {
                        // update pref in bg thread since duplicates are popping up
                        MpPrefViewModel.Instance.UniqueContentItemIdx = _UniqueItemCount;
                    });
                }
            }
        }
        #endregion



        #region Properties
        #endregion

        #region Public Methods

        public async Task<MpCopyItem> BuildAsync(
            MpPortableDataObject mpdo,
            bool suppressWrite = false,
            MpTransactionType transType = MpTransactionType.None,
            bool force_ext_sources = true) {
            if (mpdo == null || mpdo.DataFormatLookup.Count == 0) {
                return null;
            }
            if (transType == MpTransactionType.None) {
                throw new Exception("Must have transacion type");
            }

            await NormalizePlatformFormatsAsync(mpdo);

            var refs = await Mp.Services.SourceRefTools.GatherSourceRefsAsync(mpdo, force_ext_sources);

            if (Mp.Services.SourceRefTools.IsAnySourceRejected(refs)) {
                return null;
            }
            Tuple<MpCopyItemType, string, string> data_tuple = await DecodeContentDataAsync(mpdo);

            if (data_tuple == null ||
                data_tuple.Item1 == MpCopyItemType.None ||
                data_tuple.Item2 == null) {
                MpConsole.WriteLine("Warning! CopyItemBuilder could not create itemData");
                return null;
            }

            var dobj = await MpDataObject.CreateAsync(pdo: mpdo);

            MpCopyItemType itemType = data_tuple.Item1;
            string itemData = data_tuple.Item2;
            string itemDelta = data_tuple.Item3;
            string default_title = GetDefaultItemTitle(itemType, mpdo);

            var ci = await MpCopyItem.CreateAsync(
                dataObjectId: dobj.Id,
                title: default_title,
                data: itemData,
                itemType: itemType,
                suppressWrite: suppressWrite);
            if (ci == null) {
                // probably null data, clean up pre-create
                await dobj.DeleteFromDatabaseAsync();
                return null;
            }

            if (ci.WasDupOnCreate) {
                // remove new data object
                await dobj.DeleteFromDatabaseAsync();
                MpConsole.WriteLine($"Duplicate (current) data object w/ id '{dobj.Id}' deleted. Using org instance w/ id '{ci.DataObjectId}'");
                if (transType == MpTransactionType.Created) {
                    // try to prevent multiple 'create' transactions, 'Recreate' will imply dup ref
                    transType = MpTransactionType.Recreated;
                }
            }
            List<string> ref_urls = refs.Select(x => Mp.Services.SourceRefTools.ConvertToRefUrl(x)).ToList();
            if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> urls)) {
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
                            //req: mpdo.SerializeData(),
                            respType: MpJsonMessageFormatType.Delta,
                            //resp: itemDelta,
                            ref_uris: ref_urls,
                            transType: transType);

            return ci;
        }

        #endregion

        #region Private Methods

        #region Source Helpers



        #endregion

        #region Content Helpers

        private async Task<Tuple<MpCopyItemType, string, string>> DecodeContentDataAsync(MpPortableDataObject mpdo) {
            string inputTextFormat = null;
            string itemData = null;
            MpCopyItemType itemType = MpCopyItemType.None;

            if (mpdo.ContainsData(MpPortableDataFormats.AvFileNames)) {

                // FILES

                string fl_str = null;
                if (mpdo.GetData(MpPortableDataFormats.AvFileNames) is byte[] fileBytes) {
                    fl_str = fileBytes.ToDecodedString();
                } else if (mpdo.GetData(MpPortableDataFormats.AvFileNames) is string fileStr) {
                    fl_str = fileStr;
                } else if (mpdo.GetData(MpPortableDataFormats.AvFileNames) is IEnumerable<string> paths) {
                    fl_str = string.Join(Environment.NewLine, paths);
                } else if (mpdo.GetData(MpPortableDataFormats.AvFileNames) is IEnumerable<IStorageItem> sil) {
                    fl_str = string.Join(Environment.NewLine, sil.Select(x => x.Path));
                } else {
                    var fl_data = mpdo.GetData(MpPortableDataFormats.AvFileNames);
                    // what type is it? string[]?
                    Debugger.Break();
                }
                if (string.IsNullOrWhiteSpace(fl_str)) {
                    // conversion error
                    Debugger.Break();
                    return null;
                }
                itemType = MpCopyItemType.FileList;
                itemData = fl_str;
            } else if (mpdo.ContainsData(MpPortableDataFormats.AvCsv) &&
                        mpdo.GetData(MpPortableDataFormats.AvCsv) is byte[] csvBytes &&
                        csvBytes.ToDecodedString() is string csvStr) {

                // CSV

                inputTextFormat = "html";
                itemType = MpCopyItemType.Text;
                //itemData = csvStr.ToRichText();
                itemData = csvStr.CsvStrToRichHtmlTable();

                //if (mpdo.ContainsData(MpPortableDataFormats.AvRtf_bytes) && 
                //    mpdo.GetData(MpPortableDataFormats.AvRtf_bytes) is byte[] rtfCsvBytes) {
                //    // NOTE this is assuming the content is a rich text table. But it may not be 
                //    // depending on the source so may need to be careful handling these. 
                //    itemType = MpCopyItemType.Text;
                //    itemData = rtfCsvBytes.ToDecodedString().EscapeExtraOfficeRtfFormatting();
                //    itemData = itemData.ToRichHtmlText(MpPortableDataFormats.AvRtf_bytes);
                //} else {
                //    string csvStr = mpdo.GetData(MpPortableDataFormats.AvCsv).ToString();
                //    //itemData = csvStr.ToRichText();
                //    itemData = itemData.ToRichHtmlText(MpPortableDataFormats.AvCsv);
                //}
            } else if (mpdo.ContainsData(MpPortableDataFormats.AvRtf_bytes) &&
                        !mpdo.ContainsData(MpPortableDataFormats.AvHtml_bytes) &&
                        mpdo.GetData(MpPortableDataFormats.AvRtf_bytes) is byte[] rtfBytes &&
                    rtfBytes.ToDecodedString() is string rtfStr) {

                // RTF (HTML will be preferred)

                inputTextFormat = "rtf";
                itemType = MpCopyItemType.Text;
                itemData = rtfStr.EscapeExtraOfficeRtfFormatting();
            } else if (mpdo.ContainsData(MpPortableDataFormats.AvPNG) &&
                        mpdo.GetData(MpPortableDataFormats.AvPNG) is byte[] pngBytes &&
                        //pngBytes.ToBase64String() is string pngBase64Str) {
                        Convert.ToBase64String(pngBytes) is string pngBase64Str) {

                // BITMAP (bytes)
                itemType = MpCopyItemType.Image;
                itemData = pngBase64Str;
            } else if (mpdo.ContainsData(MpPortableDataFormats.AvPNG) &&
                        mpdo.GetData(MpPortableDataFormats.AvPNG) is string pngBytesStr) {

                // BITMAP (base64)
                itemType = MpCopyItemType.Image;
                itemData = pngBytesStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.AvHtml_bytes) &&
                        mpdo.GetData(MpPortableDataFormats.AvHtml_bytes) is byte[] htmlBytes &&
                        htmlBytes.ToDecodedString() is string htmlStr) {

                // HTML (bytes)
                inputTextFormat = "html";
                itemType = MpCopyItemType.Text;
                itemData = htmlStr;
            } else if (mpdo.TryGetData(MpPortableDataFormats.CefHtml, out string cefHtmlStr)) {

                // HTML (xml)
                inputTextFormat = "html";
                itemType = MpCopyItemType.Text;
                itemData = cefHtmlStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.Text) &&
                        mpdo.GetData(MpPortableDataFormats.Text) is string textStr) {

                // TEXT
                inputTextFormat = "text";
                itemType = MpCopyItemType.Text;
                itemData = textStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.Unicode) &&
                        mpdo.GetData(MpPortableDataFormats.Unicode) is string unicodeStr) {

                // UNICODE
                inputTextFormat = "text";
                itemType = MpCopyItemType.Text;
                itemData = unicodeStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.OemText) &&
                        mpdo.GetData(MpPortableDataFormats.OemText) is string oemStr) {

                // OEM TEXT
                inputTextFormat = "text";
                itemType = MpCopyItemType.Text;
                itemData = oemStr;
            } else {
                MpConsole.WriteTraceLine("clipboard data is not known format");
            }

            string delta = null;

            // POST-PROCESS (TEXT ONLY)

            if (itemType == MpCopyItemType.Text) {
                if (string.IsNullOrEmpty(inputTextFormat)) {
                    // should be set
                    Debugger.Break();
                    inputTextFormat = "text";
                }

                MpAvHtmlClipboardData htmlClipboardData = null;

                if (MpPrefViewModel.Instance.IsRichHtmlContentEnabled) {
                    htmlClipboardData = await MpAvPlainHtmlConverter.Instance.ParseAsync(itemData, inputTextFormat);
                    if (htmlClipboardData == null) {
                        itemData = null;
                    } else {
                        itemData = htmlClipboardData.RichHtml;
                        delta = htmlClipboardData.Delta;
                        if (!string.IsNullOrEmpty(htmlClipboardData.Html) &&
                            htmlClipboardData.Html.StartsWith("<img")) {
                            try {
                                var img_parts = htmlClipboardData.Html.Split("src=\"");
                                if (img_parts.Length > 1) {
                                    var img_parts2 = img_parts[1].Split("\"");
                                    if (img_parts2.Length > 0) {
                                        string img_src_uri = img_parts2[0];
                                        var img_bytes = await MpFileIo.ReadBytesFromUriAsync(img_src_uri);
                                        if (img_bytes != null && img_bytes.Length > 0 &&
                                            Convert.ToBase64String(img_bytes) is string img_base64) {
                                            // update item type to image and clear delta (it references img uri not bytes)
                                            itemType = MpCopyItemType.Image;
                                            itemData = img_base64;
                                            delta = null;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) {
                                MpConsole.WriteTraceLine($"Error converting img html to img content. Img html: '{htmlClipboardData.Html}'", ex);
                            }
                            // handle special case that item is an image drop from browser (tested on chrome in windows)

                        }
                    }
                } else {
                    if (!string.IsNullOrEmpty(itemData)) {
                        if (inputTextFormat == "html") {
                            itemData = itemData.ToPlainText();
                        }
                    }
                }
            }

            if (itemType == MpCopyItemType.Text && !string.IsNullOrEmpty(itemData) &&
                string.IsNullOrWhiteSpace(MpRichHtmlToPlainTextConverter.Convert(itemData)) &&
                MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems) {
                // if text is just whitespace and those ignored flag to ignore item
                return null;
            }

            if (string.IsNullOrEmpty(delta)) {
                MpQuillDelta deltaObj = new MpQuillDelta() {
                    ops = new List<Op>()
                };

                switch (itemType) {
                    case MpCopyItemType.Text:
                        deltaObj.ops.Add(new Op() { insert = itemData.ToPlainText(inputTextFormat) });
                        break;
                    case MpCopyItemType.Image:
                        deltaObj.ops.Add(new Op() {
                            insert = new ImageInsert() { image = $"data:image/png;base64,{itemData}" },
                            attributes = new Attributes() { align = "center" }
                        });
                        break;
                    case MpCopyItemType.FileList:
                        deltaObj.ops.Add(new Op() {
                            insert = itemData
                        });

                        break;
                }
                delta = deltaObj.SerializeJsonObject();
            }
            return new Tuple<MpCopyItemType, string, string>(itemType, itemData, delta);
        }

        private string GetDefaultItemTitle(MpCopyItemType itemType, MpPortableDataObject mpdo) {

            string default_title = null;
            if (mpdo.ContainsData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT)) {
                default_title = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT) as string;
            }
            if (string.IsNullOrEmpty(default_title)) {
                UniqueItemCount = UniqueItemCount + 1;
                default_title = $"{itemType} {(UniqueItemCount)}";
            }
            return default_title;
        }
        #endregion

        #region Platform Handling
        private async Task<MpPortableDataObject> NormalizePlatformFormatsAsync(MpPortableDataObject mpdo) {
            if (OperatingSystem.IsAndroid()) {
                return mpdo;
            }
            var actual_formats = await TopLevel.GetTopLevel(Application.Current.GetMainWindow()).Clipboard.GetFormatsSafeAsync();
            MpConsole.WriteLine($"Normalizing actual dataobject formats:  {string.Join(",", actual_formats.Select(x => x))}");

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
                // linux doesn't case non-html formats the same as windows so mapping them here
                bool isLinuxFileList = mpdo.ContainsData(MpPortableDataFormats.CefText) &&
                                    actual_formats.Contains(MpPortableDataFormats.LinuxGnomeFiles);
                if (isLinuxFileList) {
                    // NOTE avalonia doesn't acknowledge files (no 'FileNames' entry) on Ubuntu 22.04
                    // and is beyond support for the clipboard plugin right now so..
                    // TODO eventually should tidy up clipboard handling so plugins are clear example code
                    string files_text_base64 = mpdo.GetData(MpPortableDataFormats.CefText) as string;
                    if (!string.IsNullOrEmpty(files_text_base64)) {
                        string files_text = files_text_base64.ToStringFromBase64();
                        MpConsole.WriteLine("Got file text: " + files_text);
                        mpdo.SetData(MpPortableDataFormats.AvFileNames, files_text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                    }

                } else {
                    bool isLinuxAndNeedsCommonPlainText = mpdo.ContainsData(MpPortableDataFormats.CefText) &&
                                                            !mpdo.ContainsData(MpPortableDataFormats.Text);
                    if (isLinuxAndNeedsCommonPlainText) {
                        string plain_text = mpdo.GetData(MpPortableDataFormats.CefText) as string;
                        mpdo.SetData(MpPortableDataFormats.Text, plain_text);
                    }
                }
            }
            MpConsole.WriteLine($"DataObject format normalization complete. Available dataobject formats: {string.Join(",", mpdo.DataFormatLookup.Select(x => x.Key.Name))}");
            return mpdo;
        }

        #endregion

        #endregion

        #region Commands
        #endregion
    }
}
