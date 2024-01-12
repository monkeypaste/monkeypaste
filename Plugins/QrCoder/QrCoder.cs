using MonkeyPaste.Common.Plugin;
using SkiaSharp;
using SkiaSharp.QrCode.Image;
using System;
using System.Collections.Generic;
using System.IO;

namespace QrCoder {
    public class QrCoder : MpIAnalyzeComponent {
        const string CONV_TEXT_PARAM_ID = "1";
        const string SOURCE_TITLE_PARAM_ID = "2";
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string textToConvert = req.GetParamValue<string>(CONV_TEXT_PARAM_ID);

            var qrCode = new QrCode(textToConvert, new Vector2Slim(256, 256), SKEncodedImageFormat.Png);

            using (MemoryStream memoryStream = new MemoryStream()) {
                qrCode.GenerateImage(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string outputQrCodeBase64 = Convert.ToBase64String(imageBytes);

                var resp = new MpAnalyzerPluginResponseFormat() {
                    dataObjectLookup = new Dictionary<string, object> {
                        { MpPortableDataFormats.Image, imageBytes },
                        { MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, $"{req.GetParamValue<string>(SOURCE_TITLE_PARAM_ID)} - Qr Code" }
                    }
                };
                return resp;
            }
        }
    }
}
