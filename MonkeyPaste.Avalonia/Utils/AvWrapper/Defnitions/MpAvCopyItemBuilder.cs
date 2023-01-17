using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using MonkeyPaste.Common.Avalonia;
using System.Reflection;
using Avalonia;
using MonkeyPaste.Common.Wpf;

namespace MonkeyPaste.Avalonia {
    public class MpAvCopyItemBuilder : MpICopyItemBuilder {
        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public async Task<MpCopyItem> BuildAsync(MpPortableDataObject mpdo, bool suppressWrite = false) {
            if (mpdo == null || mpdo.DataFormatLookup.Count == 0) {
                return null;
            }
            await NormalizePlatformFormatsAsync(mpdo);

            var refs = await MpPlatformWrapper.Services.SourceRefBuilder.GatherSourceRefsAsync(mpdo, true);

            if (MpPlatformWrapper.Services.SourceRefBuilder.IsAnySourceRejected(refs)) {
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
            string data_format = GetPreferredContentType(itemType);
            var ci = await MpCopyItem.CreateAsync(
                //sourceId: original_source == null ? MpDefaultDataModelTools.ThisSourceId : original_source.Id,
                dataObjectId: dobj.Id,
                title: default_title,
                dataFormat: data_format,
                data: itemData,
                itemType: itemType,
                suppressWrite: suppressWrite);

            List<string> ref_urls = refs.Select(x=>MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(x)).ToList();
            if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> urls)) { 
                var urlList = urls.ToList();
                for (int i = 0; i < ref_urls.Count; i++) {
                    var provided_url = urlList.FirstOrDefault(x => x.ToLower().StartsWith(ref_urls[i].ToLower()));
                    if(provided_url != null) {
                        // prefer provided url in case it has args and remove so not added later
                        ref_urls[i] = provided_url;
                        urlList.Remove(provided_url);
                    }
                }
                // add remaining urls for transaction
                if(urlList.Count > 0) {
                    ref_urls.AddRange(urlList);
                }
            }

            await MpPlatformWrapper.Services.TransactionBuilder.PerformTransactionAsync(
                            copyItemId: ci.Id,
                            reqType: MpJsonMessageFormatType.DataObject,
                            req: mpdo.Serialize(),
                            respType: MpJsonMessageFormatType.Delta,
                            resp: itemDelta,
                            ref_urls: ref_urls,
                                //refs.Select(x => MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(x)),
                            label: "Created");

            return ci;
        }

        #endregion

        #region Private Methods

        #region Source Helpers

        

        #endregion

        #region Content Helpers

        private async Task<Tuple<MpCopyItemType,string,string>> DecodeContentDataAsync(MpPortableDataObject mpdo) {
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
                        pngBytes.ToBase64String() is string pngBase64Str) {

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

                // HTML
                inputTextFormat = "html";
                itemType = MpCopyItemType.Text;
                itemData = htmlStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.Text) &&
                        mpdo.GetData(MpPortableDataFormats.Text) is string textStr) {

                // TEXT
                inputTextFormat = "text";
                itemType = MpCopyItemType.Text;
                itemData = textStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.Unicode) &&
                        mpdo.GetData(MpPortableDataFormats.Text) is string unicodeStr) {

                // UNICODE
                inputTextFormat = "text";
                itemType = MpCopyItemType.Text;
                itemData = unicodeStr;
            } else if (mpdo.ContainsData(MpPortableDataFormats.OemText) &&
                        mpdo.GetData(MpPortableDataFormats.Text) is string oemStr) {

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

                var htmlClipboardData = await MpAvPlainHtmlConverter.Instance.ParseAsync(itemData, inputTextFormat);
                if (htmlClipboardData == null) {
                    itemData = null;
                } else {
                    itemData = htmlClipboardData.Html;
                    delta = htmlClipboardData.Delta;
                }                
            }
            if(string.IsNullOrEmpty(delta)) {
                MpQuillDelta deltaObj = new MpQuillDelta() {
                    ops = new List<Op>()
                };

                switch(itemType) {
                    case MpCopyItemType.Text:
                        deltaObj.ops.Add(new Op() { insert = itemData });
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
            return new Tuple<MpCopyItemType, string,string>(itemType, itemData,delta);
        }

        private string GetPreferredContentType(MpCopyItemType itemType) {
            switch (itemType) {
                case MpCopyItemType.Text:
                    return MpPortableDataFormats.AvHtml_bytes;
                case MpCopyItemType.Image:
                    return MpPortableDataFormats.AvPNG;
                case MpCopyItemType.FileList:
                    return MpPortableDataFormats.AvFileNames;
            }
            return MpPortableDataFormats.Text.ToString();
        }

        private string GetDefaultItemTitle(MpCopyItemType itemType, MpPortableDataObject mpdo) {
            string default_title = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT) as string;
            if(string.IsNullOrEmpty(default_title)) {
                default_title = $"{itemType} {(++MpPrefViewModel.Instance.UniqueContentItemIdx)}";
            }
            return default_title;
        }
        #endregion

        #region Platform Handling
        private async Task<MpPortableDataObject> NormalizePlatformFormatsAsync(MpPortableDataObject mpdo) {
            if(OperatingSystem.IsWindows()) {
                bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(50);
                    canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
            }
            var actual_formats = await Application.Current.Clipboard.GetFormatsSafeAsync();
            actual_formats.ForEach(x => MpConsole.WriteLine("Actual format: " + x));

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

            mpdo.DataFormatLookup.ForEach(x => MpConsole.WriteLine("Creating copyItem w/ available format; " + x.Key.Name));
            return mpdo;
        }

        #endregion
        
        #endregion

        #region Commands
        #endregion
    }
}
