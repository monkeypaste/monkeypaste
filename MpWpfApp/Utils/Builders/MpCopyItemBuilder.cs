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
using MonkeyPaste.Plugin;
using System.Reflection;

namespace MpWpfApp {
    public class MpCopyItemBuilder : MpICopyItemBuilder {
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
                var iData = mpdo.DataFormatLookup as Dictionary<MpClipboardFormatType,string>;
                                
                
                string itemData = null;
                string htmlData = string.Empty;
                MpCopyItemType itemType = MpCopyItemType.None;

                if (iData.ContainsKey(MpClipboardFormatType.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = iData[MpClipboardFormatType.FileDrop];
                } else if (iData.ContainsKey(MpClipboardFormatType.Csv)) {
                    itemType = MpCopyItemType.Text;
                    string csvStr = iData[MpClipboardFormatType.Csv];
                    itemData = csvStr.ToRichTextTable();
                    //itemData = itemData.ToQuillText();
                } else if (iData.ContainsKey(MpClipboardFormatType.Rtf)) {
                    itemType = MpCopyItemType.Text;
                    itemData = iData[MpClipboardFormatType.Rtf].EscapeExtraOfficeRtfFormatting();
                    //itemData = itemData.ToQuillText();

                } else if (iData.ContainsKey(MpClipboardFormatType.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = iData[MpClipboardFormatType.Bitmap];
                } else if(iData.ContainsKey(MpClipboardFormatType.Text)) {                    
                    itemType = MpCopyItemType.Text;
                    itemData = iData[MpClipboardFormatType.Text].ToRichText();
                    //itemData = itemData.ToQuillText();
                } else if (iData.ContainsKey(MpClipboardFormatType.UnicodeText)) {
                    itemType = MpCopyItemType.Text;
                    itemData = iData[MpClipboardFormatType.UnicodeText].ToRichText();
                    //itemData = itemData.ToQuillText();
                } else if (iData.ContainsKey(MpClipboardFormatType.OemText)) {
                    itemType = MpCopyItemType.Text;
                    itemData = iData[MpClipboardFormatType.OemText].ToRichText();
                    //itemData = itemData.ToQuillText();
                } else {
                    MpConsole.WriteTraceLine("clipboard data is not known format");
                    return null;
                }

                if (MpPreferences.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.Text &&
                    string.IsNullOrWhiteSpace((itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }

                if (iData.ContainsKey(MpClipboardFormatType.Html)) {
                    htmlData = iData[MpClipboardFormatType.Html];
                }

                if (itemType == MpCopyItemType.Text && ((string)itemData).Length > MpPreferences.MaxRtfCharCount) {
                    itemData = itemData.ToPlainText();
                    if (((string)itemData).Length > MpPreferences.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        if (MpPreferences.NotificationShowCopyItemTooLargeToast) {
                            MpNotificationCollectionViewModel.Instance.ShowMessage(
                                title: "Item TOO LARGE",
                                msg: $"Max Item Characters is {MpPreferences.MaxRtfCharCount} and copied item is {((string)itemData).Length} characters",
                                msgType: MpNotificationDialogType.DbError)
                                    .FireAndForgetSafeAsync(MpClipTrayViewModel.Instance);
                        }
                        return null;
                    }
                }

                var dupCheck = await MpDataModelProvider.GetCopyItemByData(itemData);
                if(dupCheck != null) {
                    MpConsole.WriteLine("Duplicate item detected, flipping id and returning");
                    dupCheck = await MpDb.GetItemAsync<MpCopyItem>(dupCheck.Id);
                    dupCheck.Id *= -1;
                    return dupCheck;
                }

                string processPath, appName, processIconImg64;

                var processHandle = MpProcessManager.LastHandle;
                if (processHandle == IntPtr.Zero) {
                    // since source is unknown set to this app

                    processPath = MpPreferences.ThisAppSource.App.AppPath;
                    appName = MpPreferences.ThisAppSource.App.AppName;
                    processIconImg64 = MpBase64Images.AppIcon;
                } else {
                    processPath = MpProcessManager.GetProcessPath(processHandle);
                    appName = MpProcessManager.GetProcessApplicationName(processHandle);
                    processIconImg64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(processPath);
                }

                MpApp app = await MpDataModelProvider.GetAppByPath(processPath);
                if (app == null) {
                    var icon = await MpDataModelProvider.GetIconByImageStr(processIconImg64);
                    if (icon == null) {
                        icon = await MpIcon.Create(processIconImg64);
                    } else {
                        icon = await MpDb.GetItemAsync<MpIcon>(icon.Id);
                    }
                    app = await MpApp.Create(processPath, appName, icon);
                } else {
                    app = await MpDb.GetItemAsync<MpApp>(app.Id);
                }

                MpUrl url = null;

                if(!string.IsNullOrEmpty(htmlData)) {
                    try {
                        url = await MpUrlBuilder.CreateFromHtmlData(htmlData);
                        if(url != null) {                            
                            if (MpUrlCollectionViewModel.Instance.IsRejected(url.UrlDomainPath)) {
                                MpConsole.WriteLine("Clipboard Monitor: Ignoring url domain '" + url.UrlDomainPath);
                                return null;
                            }
                            if (MpUrlCollectionViewModel.Instance.IsUrlRejected(url.UrlPath)) {
                                MpConsole.WriteLine("Clipboard Monitor: Ignoring url domain '" + url.UrlPath);
                                return null;
                            }
                        }
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine(@"Error parsing url from htmlData: " + htmlData, ex);
                    }
                }


                if (app == null) {
                    throw new Exception("Error creating copy item no source discovered");
                }
                if(url != null) {
                    await MpDb.AddOrUpdateAsync<MpUrl>(url);
                }
                var source = await MpSource.Create(app, url);
                var ci = await MpCopyItem.Create(
                    sourceId: source.Id,
                    data: itemData,
                    itemType: itemType,
                    suppressWrite: suppressWrite);

                return ci;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return null;
            }
        }

        public async Task<MpCopyItem> Create() {
            var ci = await MpCopyItemBuilder.CreateFromDataObject(null);
            return ci;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
