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

namespace MonkeyPaste.Avalonia {
    public class MpAvCopyItemBuilder : MpICopyItemBuilder {
        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        
        public static async Task<MpCopyItem> CreateFromDataObject(MpPortableDataObject mpdo, bool fromInternalSource, bool suppressWrite = false) {
            try {
                if (mpdo == null || mpdo.DataFormatLookup.Count == 0) {
                    return null;
                }

                var actual_formats = await Application.Current.Clipboard.GetFormatsAsync();
                actual_formats.ForEach(x => MpConsole.WriteLine("Actual format: " + x));

                if(OperatingSystem.IsLinux()) {
                    // linux doesn't case non-html formats the same as windows so mapping them here
                    bool isLinuxFileList = mpdo.ContainsData(MpPortableDataFormats.CefText) &&
                                        actual_formats.Contains(MpPortableDataFormats.LinuxGnomeFiles);
                    if(isLinuxFileList) {
                        // NOTE avalonia doesn't acknowledge files (no 'FileNames' entry) on Ubuntu 22.04
                        // and is beyond support for the clipboard plugin right now so..
                        // TODO eventually should tidy up clipboard handling so plugins are clear example code
                        string files_text_base64 = mpdo.GetData(MpPortableDataFormats.CefText) as string;
                        if(!string.IsNullOrEmpty(files_text_base64)) {
                            string files_text = files_text_base64.ToStringFromBase64();
                            MpConsole.WriteLine("Got file text: " + files_text);
                            mpdo.SetData(MpPortableDataFormats.AvFileNames, files_text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                        }
                        
                    } else {
                        bool isLinuxAndNeedsCommonPlainText = mpdo.ContainsData(MpPortableDataFormats.CefText) &&
                                                                !mpdo.ContainsData(MpPortableDataFormats.Text);
                        if(isLinuxAndNeedsCommonPlainText) {
                            string plain_text = mpdo.GetData(MpPortableDataFormats.CefText) as string;
                            mpdo.SetData(MpPortableDataFormats.Text, plain_text);
                        }
                    }
                }
                

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

                mpdo.DataFormatLookup.ForEach(x => MpConsole.WriteLine("Creating copyItem w/ available format; " + x.Key.Name));
                
                var iData = mpdo.DataFormatLookup as Dictionary<MpPortableDataFormat, object>;


                string itemData = null;
                //string htmlData = string.Empty;
                MpAvHtmlClipboardData htmlClipboardData = new MpAvHtmlClipboardData();
                MpCopyItemType itemType = MpCopyItemType.None;

                if (mpdo.ContainsData(MpPortableDataFormats.AvFileNames)) {

                    // FILES

                    itemType = MpCopyItemType.FileList;
                    itemData = mpdo.GetData(MpPortableDataFormats.AvFileNames).ToString();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Csv)) {

                    // CSV

                    itemType = MpCopyItemType.Text;

                    if (mpdo.ContainsData(MpPortableDataFormats.AvRtf_bytes) && 
                        mpdo.GetData(MpPortableDataFormats.AvRtf_bytes) is byte[] rtfCsvBytes) {
                        // NOTE this is assuming the content is a rich text table. But it may not be 
                        // depending on the source so may need to be careful handling these. 
                        itemType = MpCopyItemType.Text;
                        itemData = rtfCsvBytes.ToDecodedString().EscapeExtraOfficeRtfFormatting();
                        itemData = itemData.ToRichHtmlText(MpPortableDataFormats.AvRtf_bytes);
                    } else {
                        string csvStr = mpdo.GetData(MpPortableDataFormats.Csv).ToString();
                        //itemData = csvStr.ToRichText();
                        itemData = itemData.ToRichHtmlText(MpPortableDataFormats.Csv);
                    }
                } else if (mpdo.ContainsData(MpPortableDataFormats.AvRtf_bytes) &&
                        mpdo.GetData(MpPortableDataFormats.AvRtf_bytes) is byte[] rtfBytes &&
                        rtfBytes.ToDecodedString() is string rtfStr) {

                    // RTF

                    // for now and simplicity there are no platform checks for rtf because it should only (by the DataFormat name at least) be on windows
                    itemType = MpCopyItemType.Text;
                    itemData = rtfStr.EscapeExtraOfficeRtfFormatting();
                    itemData = itemData.ToRichHtmlText(MpPortableDataFormats.AvRtf_bytes);
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

                    itemType = MpCopyItemType.Text;
                    htmlClipboardData = await MpAvHtmlClipboardData.ParseAsync(htmlStr);
                    if(htmlClipboardData == null) {
                        return null;
                    }
                    itemData = htmlClipboardData.Html;

                    if(mpdo.ContainsData(MpPortableDataFormats.LinuxSourceUrl) && 
                        mpdo.GetData(MpPortableDataFormats.LinuxSourceUrl) is byte[] url_bytes &&
                        url_bytes.ToDecodedString(Encoding.ASCII) is string source_url_str) {
                        // on linux html is not in fragment format like windows and firefox supports this format
                        // but chrome doesn't
                        //source_url_str = System.Web.HttpUtility.HtmlDecode(source_url_str);
                        htmlClipboardData.SourceUrl = source_url_str;
                    }
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Text)) {

                    // TEXT

                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.Text).ToString().ToContentRichText();
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Unicode)) {
                    
                    // UNICODE
                    
                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.Unicode).ToString().ToContentRichText();
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.OemText)) {

                    // OEM TEXT

                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.OemText).ToString().ToContentRichText();
                    //itemData = itemData.ToQuillText();
                } else {
                    MpConsole.WriteTraceLine("clipboard data is not known format");
                    return null;
                }

                if(itemData == null) {
                    MpConsole.WriteTraceLine("Warning! CopyItemBuilder could not create itemData");
                    return null;
                }


                if (MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.Text &&
                    string.IsNullOrWhiteSpace((itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }

                //if (mpdo.ContainsData(MpPortableDataFormats.AvHtml_bytes)) {
                //    string rawHtmlData = mpdo.GetData(MpPortableDataFormats.AvHtml_bytes).ToString();
                //    htmlClipboardData = MpHtmlClipboardDataConverter.Parse(rawHtmlData);
                //    //htmlData = mpdo.GetData(MpPortableDataFormats.AvHtml_bytes).ToString();
                //}

                if (itemType == MpCopyItemType.Text && ((string)itemData).Length > MpPrefViewModel.Instance.MaxRtfCharCount) {
                    itemData = itemData.ToPlainText();
                    if (((string)itemData).Length > MpPrefViewModel.Instance.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        if (MpPrefViewModel.Instance.NotificationShowCopyItemTooLargeToast) {
                            MpNotificationCollectionViewModel.Instance.ShowMessageAsync(
                                title: "Item TOO LARGE",
                                msg: $"Max Item Characters is {MpPrefViewModel.Instance.MaxRtfCharCount} and copied item is {((string)itemData).Length} characters",
                                msgType: MpNotificationDialogType.DbError)
                                    .FireAndForgetSafeAsync(MpAvClipTrayViewModel.Instance);
                        }
                        return null;
                    }
                }

                var dupCheck = await MpDataModelProvider.GetCopyItemByDataAsync(itemData);
                if (dupCheck != null) {
                    MpConsole.WriteLine("Duplicate item detected, flipping id and returning");
                    dupCheck = await MpDb.GetItemAsync<MpCopyItem>(dupCheck.Id);
                    dupCheck.Id *= -1;
                    return dupCheck;
                }

                MpApp app = null;
                MpUrl url = null;
                if(fromInternalSource) {
                    app = await MpDb.GetItemAsync<MpApp>(MpPrefViewModel.Instance.ThisAppSource.AppId);
                } else {
                    var last_pinfo = MpPlatformWrapper.Services.ProcessWatcher.LastProcessInfo;

                    //if(OperatingSystem.IsLinux()) {
                    //    // this maybe temporary but linux not following process watching convention because its SLOW
                    //    string exe_path = MpX11ShellHelpers.GetExeWithArgsToExePath(MpPlatformWrapper.Services.ProcessWatcher.LastProcessPath);
                    //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(exe_path);
                    //} else {
                    //    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(MpPlatformWrapper.Services.ProcessWatcher.LastHandle);
                    //}
                    if(last_pinfo == null) {
                        Debugger.Break();
                    }
                    app = await MpPlatformWrapper.Services.AppBuilder.CreateAsync(last_pinfo);

                    url = htmlClipboardData == null ?
                            null : await MpUrlBuilder.CreateUrl(htmlClipboardData.SourceUrl);

                    if (url != null) {
                        if (MpAvUrlCollectionViewModel.Instance.IsRejected(url.UrlDomainPath)) {
                            MpConsole.WriteLine("Clipboard Monitor: Ignoring url domain '" + url.UrlDomainPath);
                            return null;
                        }
                        if (MpAvUrlCollectionViewModel.Instance.IsUrlRejected(url.UrlPath)) {
                            MpConsole.WriteLine("Clipboard Monitor: Ignoring url domain '" + url.UrlPath);
                            return null;
                        }
                    }
                }

                if (app == null) {
                    throw new Exception("Error creating copy item no source discovered");
                }
                if(url != null) {
                    await MpDb.AddOrUpdateAsync<MpUrl>(url);
                }
                var source = await MpSource.Create(app.Id, url == null ? 0:url.Id);

                var dobj = await MpDataObject.CreateAsync(
                    pdo: mpdo);

                var ci = await MpCopyItem.Create(
                    sourceId: source.Id,
                    dataObjectId: dobj.Id,
                    //preferredFormatName: htmlClipboardData == null ? null : MpPortableDataFormats.AvHtml_bytes,
                    data: itemData,
                    itemType: itemType,
                    suppressWrite: suppressWrite);

                return ci;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return null;
            }
        }

        public async Task<MpCopyItem> CreateAsync(MpPortableDataObject pdo, bool fromInternalSource, bool suppressWrite = false) {
            var ci = await CreateFromDataObject(pdo,fromInternalSource,suppressWrite);
            return ci;
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
