using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpCopyItemBuilder : MpICopyItemBuilder {
        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        
        public static async Task<MpCopyItem> CreateFromClipboard(Dictionary<string, string> iData) {
            try {
                if (iData == null || iData.Count == 0) {
                    return null;
                }

                var processHandle = MpClipboardManager.Instance.LastWindowWatcher.LastHandle;
                if (processHandle == IntPtr.Zero) {
                    // since source is unknown set to this app
                    processHandle = MpClipboardManager.Instance.LastWindowWatcher.ThisAppHandle;
                }
                string processPath = MpHelpers.Instance.GetProcessPath(processHandle);
                string appName = MpHelpers.Instance.GetProcessApplicationName(processHandle);
                string processIconImg64 = MpHelpers.Instance.GetIconImage(processPath).ToBase64String();
                
                string itemData;
                string htmlData = string.Empty;
                MpCopyItemType itemType;

                if (iData.ContainsKey(DataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = iData[DataFormats.FileDrop];
                } else if (iData.ContainsKey(DataFormats.CommaSeparatedValue)) {
                    itemType = MpCopyItemType.RichText;
                    string csvStr = iData[DataFormats.CommaSeparatedValue];
                    itemData = csvStr.ToRichTextTable();
                } else if (iData.ContainsKey(DataFormats.Rtf)) {
                    itemType = MpCopyItemType.RichText;
                    itemData = iData[DataFormats.Rtf].EscapeExtraOfficeRtfFormatting();
                    //itemData = itemData.ToQuillText();
                } else if (iData.ContainsKey(DataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = iData[DataFormats.Bitmap];
                } else if ((iData.ContainsKey(DataFormats.Html) || iData.ContainsKey(DataFormats.Text)) && !string.IsNullOrEmpty(iData[DataFormats.Text])) {
                    itemType = MpCopyItemType.RichText;
                    if (iData.ContainsKey(DataFormats.Html)) {
                        htmlData = iData[DataFormats.Html];                        
                    }
                    itemData = MpHelpers.Instance.ConvertPlainTextToRichText(iData[DataFormats.UnicodeText]);
                    //itemData = itemData.ToQuillText();
                } else {
                    MonkeyPaste.MpConsole.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }

                var dupCheck = await MpDataModelProvider.Instance.GetCopyItemByData(itemData);
                if(dupCheck != null) {
                    MpConsole.WriteLine("Duplicate item detected, ignoring");
                    return null;
                }

                MpApp app = await MpDataModelProvider.Instance.GetAppByPath(processPath);
                if (app == null) {
                    var icon = await MpDataModelProvider.Instance.GetIconByImageStr(processIconImg64);
                    if (icon == null) {
                        icon = await MpIcon.Create(processIconImg64);
                    } else {
                        icon = await MpDb.Instance.GetItemAsync<MpIcon>(icon.Id);
                    }
                    app = await MpApp.Create(processPath, appName, icon);
                } else {
                    app = await MpDb.Instance.GetItemAsync<MpApp>(app.Id);
                }

                MpUrl url = null;

                if(!string.IsNullOrEmpty(htmlData)) {
                    try {
                        url = await MpUrlBuilder.CreateFromHtmlData(htmlData, app);
                        if(url != null) {                            
                            if (MpUrlCollectionViewModel.Instance.IsDomainRejected(url.UrlDomainPath)) {
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

                if (itemType == MpCopyItemType.RichText && ((string)itemData).Length > MpPreferences.Instance.MaxRtfCharCount) {
                    itemData = MpHelpers.Instance.ConvertRichTextToPlainText((string)itemData);
                    if (((string)itemData).Length > Properties.Settings.Default.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        if (Properties.Settings.Default.NotificationShowCopyItemTooLargeToast) {
                            MpStandardBalloonViewModel.ShowBalloon(
                            "Item TOO LARGE",
                            $"Max Item Characters is {Properties.Settings.Default.MaxRtfCharCount} and copied item is {((string)itemData).Length} characters",
                            Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                        }
                        return null;
                    }
                }

                if (MpPreferences.Instance.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.RichText &&
                    string.IsNullOrWhiteSpace(((string)itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }

                if (app == null) {
                    throw new Exception("Error creating copy item no source discovered");
                }
                if(url != null) {
                    await MpDb.Instance.AddOrUpdateAsync<MpUrl>(url);
                }
                var source = await MpSource.Create(app, url);
                var ci = await MpCopyItem.Create(source, itemData, itemType);
                return ci;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return null;
            }
        }

        public async Task<MpCopyItem> Create() {
            var ci = await MpCopyItemBuilder.CreateFromClipboard(null);
            return ci;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
