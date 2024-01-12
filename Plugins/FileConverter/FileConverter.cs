using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileConverter {
    public class FileConverter : MpIAnalyzeComponent {
        const string FILE_PATH_PARAM_ID = "1";
        const string TARGET_TYPE_PARAM_ID = "2";
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string path = req.GetParamValue<string>(FILE_PATH_PARAM_ID);
            if (!File.Exists(path)) {
                return null;
            }
            string target_type = req.GetParamValue<string>(TARGET_TYPE_PARAM_ID);
            string target_format;
            object target_data = null;
            if (target_type == "Image") {
                target_format = MpPortableDataFormats.Image;
                if (ReadBytesFromFile(path) is byte[] bytes &&
                    bytes.Length > 0) {
                    target_data = Convert.ToBase64String(bytes);
                }
            } else {
                target_format = MpPortableDataFormats.Text;
                target_data = ReadTextFromFile(path);
            }
            if (target_data == null) {
                return null;
            }

            return new MpAnalyzerPluginResponseFormat() {
                dataObjectLookup = new Dictionary<string, object> { { target_format, target_data } }
            };
        }

        private byte[] ReadBytesFromFile(string filePath) {
            if (!File.Exists(filePath)) {
                return null;
            }
            try {
                using (var fs = new FileStream(filePath, FileMode.Open)) {
                    int c;
                    var bytes = new List<byte>();

                    while ((c = fs.ReadByte()) != -1) {
                        bytes.Add((byte)c);
                    }
                    return bytes.ToArray();
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }
        private string ReadTextFromFile(string filePath) {
            if (ReadBytesFromFile(filePath) is not byte[] bytes) {
                return string.Empty;
            }
            return bytes.ToDecodedString();
        }
    }
}
