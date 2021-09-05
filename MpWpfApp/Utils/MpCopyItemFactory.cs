using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpCopyItemFactory {
        #region Statics
        #endregion

        #region Private Variables
        #endregion

        #region Properties
        #endregion

        #region Public Methods
        public static MpCopyItem CreateFromClipboard(int remainingRetryCount = 5) {            
            if (remainingRetryCount < 0) {
                Console.WriteLine("Retry count exceeded ignoring copy item");
                return null;
            }
            try {
                var processHandle = MpClipboardManager.Instance.LastWindowWatcher.LastHandle;
                if (processHandle == IntPtr.Zero) {
                    // since source is unknown set to this app
                    processHandle = MpClipboardManager.Instance.LastWindowWatcher.ThisAppHandle;
                }
                string processPath = MpHelpers.Instance.GetProcessPath(processHandle);
                string appName = MpHelpers.Instance.GetProcessApplicationName(processHandle);
                string processIconImg64 = MpHelpers.Instance.GetIconImage(processPath).ToBase64String();

                MpApp app = MpApp.GetAppByPath(processPath);
                if (app == null) {
                    var icon = MpIcon.GetIconByImageStr(processIconImg64);
                    if (icon == null) {
                        icon = MpIcon.Create(processIconImg64);
                    }
                    app = MpApp.Create(processPath, appName, icon);
                }

                MpUrl url = null;

                IDataObject iData = Clipboard.GetDataObject();
                if (iData == null) {
                    return null;
                }
                string itemData = null;
                MpCopyItemType itemType = MpCopyItemType.None;

                if (iData.GetDataPresent(DataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    var paths = (string[])iData.GetData(DataFormats.FileDrop, true);
                    var sb = new StringBuilder();
                    foreach(var path in paths) {
                        sb.AppendLine(path);
                    }
                    itemData = sb.ToString();
                } else if (iData.GetDataPresent(DataFormats.CommaSeparatedValue)) {
                    itemType = MpCopyItemType.Csv;
                    itemData = (string)iData.GetData(DataFormats.CommaSeparatedValue);
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    itemType = MpCopyItemType.RichText;
                    itemData = (string)iData.GetData(DataFormats.Rtf);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = ((BitmapSource)Clipboard.GetImage()).ToBase64String();
                } else if ((iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) && !string.IsNullOrEmpty((string)iData.GetData(DataFormats.Text))) {
                    itemType = MpCopyItemType.RichText;
                    if (iData.GetDataPresent(DataFormats.Html)) {
                        // TODO parse html to get source url and html content
                        url = MpUrl.Create("https://www.google.com", "Parse Html source here");
                        var test = (string)iData.GetData(DataFormats.Html);
                        MonkeyPaste.MpConsole.WriteLine("Html clipboard data: ");
                        MonkeyPaste.MpConsole.WriteLine(test);
                    }
                    itemData = MpHelpers.Instance.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.UnicodeText));
                } else {
                    Console.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }
                if (itemType == MpCopyItemType.RichText && ((string)itemData).Length > Properties.Settings.Default.MaxRtfCharCount) {
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

                if (Properties.Settings.Default.IgnoreWhiteSpaceCopyItems &&
                    itemType == MpCopyItemType.RichText &&
                    string.IsNullOrWhiteSpace(((string)itemData).ToPlainText().Replace(Environment.NewLine, ""))) {
                    return null;
                }
                if (Properties.Settings.Default.IgnoreNewDuplicates) {
                    var dupItem = MpCopyItem.GetCopyItemByData(itemData);
                    if (dupItem != null) {
                        return dupItem;
                    }
                }

                if(app == null) {
                    throw new Exception("Error creating copy item no source discovered");
                }
                var source = MpSource.Create(app, url);
                return MpCopyItem.Create(source, itemData, itemType);
            }
            catch (Exception e) {
                //this catches intermittent COMExceptions (happened copy/pasting in Excel)
                Console.WriteLine("Caught exception creating copyitem (will reattempt to open clipboard): " + e.ToString());
                return CreateFromClipboard(remainingRetryCount - 1);
            }
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
