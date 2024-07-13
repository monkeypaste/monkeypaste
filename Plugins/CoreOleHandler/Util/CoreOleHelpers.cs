using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;

namespace CoreOleHandler {
    public static class CoreOleHelpers {

        public static void SetCulture(MpMessageRequestFormatBase req, List<MpUserNotification> ntfl = default) {
            if (req == null) {
                return;
            }
            try {
                if (Resources.Culture != null &&
                    Resources.Culture.Name == req.culture) {
                    return;
                }
                Resources.Culture = new System.Globalization.CultureInfo(req.culture);
            }
            catch (Exception ex) {

                if (ntfl != default) {
                    ntfl.Add(
                        new MpUserNotification() {
                            NotificationType = MpPluginNotificationType.PluginResponseWarning,
                            Title = Resources.NtfFormatIgnoredTitle,
                            Body = ex.ToString(),
                            Detail = req.culture,
                            IconSourceObj = MpBase64Images.ClipboardIcon
                        });
                }
                MpConsole.WriteTraceLine($"Error settings culture code to '{req.culture}'");
            }
        }


    }
}