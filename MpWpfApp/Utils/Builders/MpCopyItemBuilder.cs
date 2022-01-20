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
        
        public static async Task<MpCopyItem> CreateFromClipboard(MpDataObject mpdo) {
            try {                
                if (mpdo == null || mpdo.DataFormatLookup.Count == 0) {
                    return null;
                }
                var iData = mpdo.DataFormatLookup as Dictionary<string,string>;
                var processManager = MpProcessHelper.MpProcessManager.Instance;

                string processPath, appName, processIconImg64;

                var processHandle = processManager.LastHandle;
                if (processHandle == IntPtr.Zero) {
                    // since source is unknown set to this app

                    processPath = MpPreferences.Instance.ThisAppSource.App.AppPath;
                    appName = MpPreferences.Instance.ThisAppSource.App.AppName;
                    processIconImg64 = MpPreferences.Instance.ThisAppSource.App.Icon.IconImage.ImageBase64;
                } else {
                    processPath = MpHelpers.GetProcessPath(processHandle);
                    appName = MpHelpers.GetProcessApplicationName(processHandle);
                    processIconImg64 = MpHelpers.GetIconImage(processPath).ToBase64String();
                }
                
                string itemData = null;
                string htmlData = string.Empty;
                MpCopyItemType itemType = MpCopyItemType.None;

                if (iData.ContainsKey(DataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = iData[DataFormats.FileDrop];
                } else if (iData.ContainsKey(DataFormats.CommaSeparatedValue)) {
                    itemType = MpCopyItemType.Text;
                    string csvStr = iData[DataFormats.CommaSeparatedValue];
                    itemData = csvStr.ToRichTextTable();
                } else if (iData.ContainsKey(DataFormats.Rtf)) {
                    itemType = MpCopyItemType.Text;
                    itemData = iData[DataFormats.Rtf].EscapeExtraOfficeRtfFormatting();
                    //itemData = itemData.ToQuillText();
                } else if (iData.ContainsKey(DataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = iData[DataFormats.Bitmap];
                } else if(iData.ContainsKey(DataFormats.Text)) {                    
                    itemType = MpCopyItemType.Text;
                    itemData = MpHelpers.ConvertPlainTextToRichText(iData[DataFormats.UnicodeText]);
                    //itemData = itemData.ToQuillText();
                } else {
                    MonkeyPaste.MpConsole.WriteTraceLine("clipboard data is not known format");
                    return null;
                }

                if (MpPreferences.Instance.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.Text &&
                    string.IsNullOrWhiteSpace((itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }

                if (iData.ContainsKey(DataFormats.Html)) {
                    htmlData = iData[DataFormats.Html];
                }

                var dupCheck = await MpDataModelProvider.Instance.GetCopyItemByData(itemData);
                if(dupCheck != null) {
                    MpConsole.WriteLine("Duplicate item detected, flipping id and returning");
                    dupCheck = await MpDb.Instance.GetItemAsync<MpCopyItem>(dupCheck.Id);
                    dupCheck.Id *= -1;
                    return dupCheck;
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

                if (itemType == MpCopyItemType.Text && ((string)itemData).Length > MpPreferences.Instance.MaxRtfCharCount) {
                    itemData = MpHelpers.ConvertRichTextToPlainText((string)itemData);
                    if (((string)itemData).Length > MpPreferences.Instance.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        if (MpPreferences.Instance.NotificationShowCopyItemTooLargeToast) {
                            MpStandardBalloonViewModel.ShowBalloon(
                            "Item TOO LARGE",
                            $"Max Item Characters is {MpPreferences.Instance.MaxRtfCharCount} and copied item is {((string)itemData).Length} characters",
                            MpPreferences.Instance.AbsoluteResourcesPath + @"/Images/monkey (2).png");
                        }
                        return null;
                    }
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
