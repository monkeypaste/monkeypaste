using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using MpProcessHelper;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System.Reflection;
using System.Web.UI.HtmlControls;

namespace MpWpfApp {
    public class MpWpfCopyItemBuilder : MpICopyItemBuilder {
        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        
        public static async Task<MpCopyItem> CreateFromDataObject(MpPortableDataObject mpdo, bool suppressWrite = false) {
            try {
                if (mpdo == null || mpdo.DataFormatLookup.Count == 0) {
                    return null;
                }
                var iData = mpdo.DataFormatLookup as Dictionary<MpPortableDataFormat, object>;


                string itemData = null;
                //string htmlData = string.Empty;
                MpHtmlClipboardDataConverter htmlClipboardData = new MpHtmlClipboardDataConverter();
                MpCopyItemType itemType = MpCopyItemType.None;

                if (mpdo.ContainsData(MpPortableDataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = mpdo.GetData(MpPortableDataFormats.FileDrop).ToString();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Csv)) {
                    itemType = MpCopyItemType.Text;

                    if (mpdo.ContainsData(MpPortableDataFormats.Rtf)) {
                        // NOTE this is assuming the content is a rich text table. But it may not be 
                        // depending on the source so may need to be careful handling these. 
                        itemType = MpCopyItemType.Text;
                        itemData = mpdo.GetData(MpPortableDataFormats.Rtf).ToString().EscapeExtraOfficeRtfFormatting();
                    } else {
                        string csvStr = mpdo.GetData(MpPortableDataFormats.Csv).ToString();
                        itemData = csvStr.ToRichTextTable();
                        //itemData = itemData.ToQuillText();
                    }
                } else if (mpdo.ContainsData(MpPortableDataFormats.Rtf)) {
                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.Rtf).ToString().EscapeExtraOfficeRtfFormatting();
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = mpdo.GetData(MpPortableDataFormats.Bitmap).ToString();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Html)) {
                    itemType = MpCopyItemType.Text;
                    string rawHtmlData = mpdo.GetData(MpPortableDataFormats.Html).ToString();
                    htmlClipboardData = await MpHtmlClipboardDataConverter.Parse(rawHtmlData);
                    itemData = htmlClipboardData.Rtf;
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Text)) {
                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.Text).ToString().ToContentRichText();
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.Unicode)) {
                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.Unicode).ToString().ToContentRichText();
                    //itemData = itemData.ToQuillText();
                } else if (mpdo.ContainsData(MpPortableDataFormats.OemText)) {
                    itemType = MpCopyItemType.Text;
                    itemData = mpdo.GetData(MpPortableDataFormats.OemText).ToString().ToContentRichText();
                    //itemData = itemData.ToQuillText();
                } else {
                    MpConsole.WriteTraceLine("clipboard data is not known format");
                    return null;
                }

                if (MpPrefViewModel.Instance.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.Text &&
                    string.IsNullOrWhiteSpace((itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }

                //if (mpdo.ContainsData(MpPortableDataFormats.Html)) {
                //    string rawHtmlData = mpdo.GetData(MpPortableDataFormats.Html).ToString();
                //    htmlClipboardData = MpHtmlClipboardDataConverter.Parse(rawHtmlData);
                //    //htmlData = mpdo.GetData(MpPortableDataFormats.Html).ToString();
                //}

                if (itemType == MpCopyItemType.Text && ((string)itemData).Length > MpPrefViewModel.Instance.MaxRtfCharCount) {
                    itemData = itemData.ToPlainText();
                    if (((string)itemData).Length > MpPrefViewModel.Instance.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        if (MpPrefViewModel.Instance.NotificationShowCopyItemTooLargeToast) {
                            MpNotificationCollectionViewModel.Instance.ShowMessage(
                                title: "Item TOO LARGE",
                                msg: $"Max Item Characters is {MpPrefViewModel.Instance.MaxRtfCharCount} and copied item is {((string)itemData).Length} characters",
                                msgType: MpNotificationDialogType.DbError)
                                    .FireAndForgetSafeAsync(MpClipTrayViewModel.Instance);
                        }
                        return null;
                    }
                }

                var dupCheck = await MpDataModelProvider.GetCopyItemByData(itemData);
                if (dupCheck != null) {
                    MpConsole.WriteLine("Duplicate item detected, flipping id and returning");
                    dupCheck = await MpDb.GetItemAsync<MpCopyItem>(dupCheck.Id);
                    dupCheck.Id *= -1;
                    return dupCheck;
                }

                var app = await MpPlatformWrapper.Services.AppBuilder.Create(MpProcessManager.LastHandle);

                //string processPath, appName, processIconImg64;

                //var processHandle = MpProcessManager.LastHandle;
                //if (processHandle == IntPtr.Zero) {
                //    // since source is unknown set to this app

                //    processPath = Assembly.GetExecutingAssembly().Location;
                //    appName = MpJsonPreferenceIO.Instance.ThisAppName;
                //    processIconImg64 = MpBase64Images.AppIcon;
                //} else {
                //    processPath = MpProcessManager.GetProcessPath(processHandle);
                //    appName = MpProcessManager.GetProcessApplicationName(processHandle);
                //    processIconImg64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(processPath);
                //}

                //MpApp app = await MpDataModelProvider.GetAppByPath(processPath);
                //if (app == null) {
                //    var icon = await MpDataModelProvider.GetIconByImageStr(processIconImg64);
                //    if (icon == null) {
                //        icon = await MpIcon.Create(processIconImg64);
                //    } else {
                //        icon = await MpDb.GetItemAsync<MpIcon>(icon.Id);
                //    }
                //    app = await MpApp.Create(processPath, appName, icon.Id);
                //} else {
                //    app = await MpDb.GetItemAsync<MpApp>(app.Id);
                //}

                MpUrl url = htmlClipboardData == null ?
                    null : await MpUrlBuilder.CreateUrl(htmlClipboardData.SourceUrl);

                if (url != null) {
                    if (MpUrlCollectionViewModel.Instance.IsRejected(url.UrlDomainPath)) {
                        MpConsole.WriteLine("Clipboard Monitor: Ignoring url domain '" + url.UrlDomainPath);
                        return null;
                    }
                    if (MpUrlCollectionViewModel.Instance.IsUrlRejected(url.UrlPath)) {
                        MpConsole.WriteLine("Clipboard Monitor: Ignoring url domain '" + url.UrlPath);
                        return null;
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
                    //preferredFormatName: htmlClipboardData == null ? null : MpPortableDataFormats.Html,
                    data: itemData,
                    itemType: itemType,
                    suppressWrite: suppressWrite);



                return ci;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return null;
            }
        }

        public async Task<MpCopyItem> Create(MpPortableDataObject pdo, bool suppressWrite = false) {
            var ci = await CreateFromDataObject(pdo,suppressWrite);
            return ci;
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
