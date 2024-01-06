using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;

namespace FileConverter {
    public class FileConverter : MpIAnalyzeComponent {
        const string FILE_PATH_PARAM_ID = "1";
        const string TARGET_TYPE_PARAM_ID = "2";
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string path = req.GetParamValue<string>(FILE_PATH_PARAM_ID);
            if (!path.IsFile()) {
                return null;
            }
            string target_type = req.GetParamValue<string>(TARGET_TYPE_PARAM_ID);
            string target_format;
            object target_data = null;
            if (target_type == "Image") {
                target_format = MpPortableDataFormats.Image;
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
                dataObjectLookup = new Dictionary<string, object> { { target_format, target_data } }
            };
        }
    }
}
