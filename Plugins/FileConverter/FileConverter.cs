using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;

namespace FileConverter {
    public class FileConverter : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string path = req.GetRequestParamStringValue(1);
            if (!path.IsFile()) {
                return null;
            }
            string target_type = req.GetRequestParamStringValue(2);
            string target_format;
            object target_data = null;
            if (target_type == "Image") {
                target_format = MpPortableDataFormats.AvPNG;
                if (MpFileIo.ReadBytesFromFile(path) is byte[] bytes &&
                    bytes.Length > 0) {
                    target_data = bytes.ToBase64String();
                }
            } else {
                target_format = MpPortableDataFormats.Text;
                target_data = MpFileIo.ReadTextFromFile(path);
            }
            if (target_data == null) {
                return null;
            }

            return new MpAnalyzerPluginResponseFormat() {
                dataObject = new Dictionary<string, object> { { target_format, target_data } }
            };
        }
    }
}
