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

        public async Task<MpCopyItem> CreateAsync(MpPortableDataObject mpdo, bool suppressWrite = false) {
            if (mpdo == null || mpdo.DataFormatLookup.Count == 0) {
                return null;
            }
            await NormalizePlatformFormatsAsync(mpdo);

            var refs = await GatherSourceRefsAsync(mpdo);

            if (IsAnySourceRejected(refs)) {
                return null;
            }
            Tuple<MpCopyItemType, string> data_tuple = await DecodeContentDataAsync(mpdo);

            if (data_tuple == null ||
                data_tuple.Item1 == MpCopyItemType.None ||
                data_tuple.Item2 == null) {
                MpConsole.WriteLine("Warning! CopyItemBuilder could not create itemData");
                return null;
            }

            var dobj = await MpDataObject.CreateAsync(pdo: mpdo);

            //MpSource original_source = null;
            //if (refs != null && refs.Count() > 0) {
            //    // create 'originial' source ref (used to keep full data search working/fast and avoid many-to-many CopyItemSource table)
            //    var primary_app_ref = refs.FirstOrDefault(x => x.SourceType == MpCopyItemSourceType.App);
            //    var primary_url_ref = refs.FirstOrDefault(x => x.SourceType == MpCopyItemSourceType.Url);
            //    var primary_ci_ref = refs.FirstOrDefault(x => x.SourceType == MpCopyItemSourceType.CopyItem);

            //    if (primary_app_ref != null ||
            //        primary_url_ref != null ||
            //        primary_ci_ref != null) {

            //        original_source = await MpSource.CreateAsync(
            //            appId: primary_app_ref == null ? 0 : primary_app_ref.SourceObjId,
            //            urlId: primary_url_ref == null ? 0 : primary_url_ref.SourceObjId,
            //            copyItemId: primary_ci_ref == null ? 0 : primary_ci_ref.SourceObjId);
            //    }
            //}

            MpCopyItemType itemType = data_tuple.Item1;
            string itemData = data_tuple.Item2;
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

            if (refs != null && refs.Count() > 0) {
                // create base of item's source tree
                var ref_models = await Task.WhenAll(
                    refs.Select(x =>
                    MpCopyItemSource.CreateAsync(
                        copyItemId: ci.Id,
                        sourceObjId: x.SourceObjId,
                        sourceType: x.SourceType)));
            }
            return ci;
        }

        #endregion

        #region Private Methods

        #region Source Helpers

        private async Task<IEnumerable<MpISourceRef>> GatherSourceRefsAsync(MpPortableDataObject mpdo) {
            // TODO should probably try to limit usage of CefAsciiUrl to giving Editor dataTransfer and pass UriList data as param here

            List<MpISourceRef> refs = new List<MpISourceRef>();
            //if (internalSourceCopyItemId == 0) {
            //    throw new Exception("Invalid internalSourceCopyItemId, if not -1 needs to be greater than zero. Value was " + internalSourceCopyItemId);
            //}
            if (//internalSourceCopyItemId < 0 &&
                mpdo.ContainsData(MpPortableDataFormats.CefAsciiUrl) &&
                mpdo.GetData(MpPortableDataFormats.CefAsciiUrl) is byte[] urlBytes &&
                urlBytes.ToDecodedString() is string urlRef) {
                MpISourceRef sr = await MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(urlRef);
                if (sr != null) {
                    // occurs on sub-selection drop onto pintray or tag
                    refs.Add(sr);
                }
            }
            if (mpdo.ContainsData(MpPortableDataFormats.LinuxUriList) &&
                mpdo.GetData(MpPortableDataFormats.LinuxUriList) is IEnumerable<string> uril) {
                var list_refs = await
                    Task.WhenAll(uril.Select(x => MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(x)));
                refs.AddRange(list_refs);
            }

            if (refs.FirstOrDefault(x => x.SourceType == MpCopyItemSourceType.CopyItem) is MpCopyItem source_ci) {
                // when creating an item from an internal source
                // get source item type and remove higher priority formats that aren't of source type
                // (so partial drop of text isn't inferred as files for example)

                if (source_ci != null) {
                    if (source_ci.ItemType != MpCopyItemType.FileList) {
                        mpdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvFileNames));
                    }
                    if (source_ci.ItemType != MpCopyItemType.Image) {
                        mpdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvPNG));
                    }
                    mpdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvRtf_bytes));
                    mpdo.DataFormatLookup.Remove(MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvCsv));
                }
            }
            if(refs == null || refs.Count == 0) {
                // external ole create
                var ext_refs = await GatherExternalSourceRefsAsync(mpdo);
                if(ext_refs != null && ext_refs.Count() > 0) {
                    refs = ext_refs.ToList();
                }
            }
            if (refs == null || refs.Count == 0) {
                // fallback
                var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
                refs = new List<MpISourceRef>() { this_app };
            }
            return refs;
        }

        private async Task<IEnumerable<MpISourceRef>> GatherExternalSourceRefsAsync(MpPortableDataObject mpdo) {
            var last_pinfo = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;

            //if(OperatingSystem.IsLinux()) {
            //    // this maybe temporary but linux not following process watching convention because its SLOW
            //    string exe_path = MpX11ShellHelpers.GetExeWithArgsToExePath(MpPlatformWrapper.Services.ProcessWatcher.LastProcessPath);
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(exe_path);
            //} else {
            //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastHandle);
            //}
            if (last_pinfo == null) {
                Debugger.Break();
                return null;
            }
            var app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(last_pinfo);

            MpUrl url = null;
            string source_url = MpAvHtmlClipboardData.FindSourceUrl(mpdo);
            if(!string.IsNullOrWhiteSpace(source_url)) {
                url = await MpUrlBuilder.CreateUrlAsync(source_url);
            }

            List<MpISourceRef> ext_refs = new List<MpISourceRef>();
            if (url != null) {
                // NOTE url added first
                ext_refs.Add(url);
            }
            if (app != null) {
                ext_refs.Add(app);
            }
            return ext_refs;
        }

        private bool IsAnySourceRejected(IEnumerable<MpISourceRef> refs) {
            foreach (var source_ref in refs) {
                if (source_ref is MpUrl url &&
                    (url.IsDomainRejected || url.IsDomainRejected)) {
                    MpConsole.WriteLine($"Rejected url detected. Url: '{url}'");
                    return true;
                } else if (source_ref is MpApp app &&
                    app.IsAppRejected) {
                    MpConsole.WriteLine($"Rejected app detected. App: '{app}'");
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Content Helpers

        private async Task<Tuple<MpCopyItemType,string>> DecodeContentDataAsync(MpPortableDataObject mpdo) {
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
                itemData = csvStr.ToRichHtmlTable();

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

                // BITMAP
                itemType = MpCopyItemType.Image;
                itemData = pngBase64Str;
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
                }                
            }
            return new Tuple<MpCopyItemType, string>(itemType, itemData);
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
            string default_title = mpdo.GetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_TITLE_FORMAT) as string;
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
            var actual_formats = await Application.Current.Clipboard.GetFormatsAsync();
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
