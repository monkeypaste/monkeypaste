using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;
using System.Linq;

namespace WebSearch {
    public class WebSearch : MpIAnalyzeComponent {
        const string URI_PARAM_ID = "1";
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            if (req != null &&
                req.GetParamValue<string>(URI_PARAM_ID) is string uri) {
                try {
                    using (var myProcess = new Process()) {
                        myProcess.StartInfo.UseShellExecute = true;
                        myProcess.StartInfo.FileName = uri;
                        myProcess.Start();
                    }
                }
                catch (Exception ex) {
                    return new MpAnalyzerPluginResponseFormat() {
                        userNotifications = new[] {
                            new MpPluginUserNotificationFormat() {
                                NotificationType = MpPluginNotificationType.PluginResponseError,
                                Title = "Web Search Error",
                                Body = ex.Message,
                                Detail = uri,
                                IconSourceObj = MpBase64Images.Error
                            }
                        }.ToList()
                    };
                }
            }
            return null;
        }
    }
}
