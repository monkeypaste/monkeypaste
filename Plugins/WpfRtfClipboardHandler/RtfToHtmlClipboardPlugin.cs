using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace WpfRtfClipboardHandler {
    public class RtfToHtmlClipboardPlugin : MpIClipboardReaderComponent {
        public static int OLE_RETRY_DELAY_MS {
            get {
                return MpRandom.Rand.Next(20, 100);
            }
        }
        #region MpIClipboardReaderComponentAsync Implementation

        public async Task<MpClipboardReaderResponse> ReadClipboardDataAsync(MpClipboardReaderRequest request) {
            if (!request.readFormats.Contains(MpPortableDataFormats.WinRtf)) {
                // i don't think this should happen, clipboard collection should prune this out...
                return null;
            }
            string rtf = null;
            if (request.forcedClipboardDataObject == null) {
                // clipboard read
                bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(OLE_RETRY_DELAY_MS);
                    canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
                WinApi.OpenClipboard(IntPtr.Zero);
                if (!Clipboard.ContainsData(MpPortableDataFormats.WinRtf)) {
                    return null;
                }
                rtf = Clipboard.GetData(MpPortableDataFormats.WinRtf) as string;
            } else if (request.forcedClipboardDataObject is MpPortableDataObject mpdo &&
                    mpdo.TryGetData(MpPortableDataFormats.WinRtf, out var rtfObj)) {
                if (rtfObj is string) {
                    rtf = rtfObj as string;
                } else if (rtfObj is byte[] rtfBytes) {
                    rtf = rtfBytes.ToDecodedString();
                }
            }
            if (string.IsNullOrEmpty(rtf)) {
                return null;
            }

            List<MpPluginUserNotificationFormat> nfl = new List<MpPluginUserNotificationFormat>();
            List<Exception> exl = new List<Exception>();
            string html = null;
            try {
                html = MpWpfRtfToHtmlConverter2.ConvertFormatToHtml(rtf, MpPortableDataFormats.WinRtf);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error converting rtf '{rtf}'", ex);
                return null;
            }
            if (string.IsNullOrEmpty(html)) {
                return null;
            }

            var read_output = new MpPortableDataObject(MpPortableDataFormats.CefHtml, html);

            return new MpClipboardReaderResponse() {
                dataObject = read_output,
                userNotifications = nfl,
                errorMessage = string.Join(Environment.NewLine, exl)
            };
        }

        #endregion
    }
}
