using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace WebSearch {
    public class WebSearch : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            if (req != null &&
                req.GetRequestParamStringValue(1) is string uri) {
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
