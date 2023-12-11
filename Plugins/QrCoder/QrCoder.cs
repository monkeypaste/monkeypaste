using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SkiaSharp;
using SkiaSharp.QrCode.Image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QrCoder {
    public class QrCoder : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string textToConvert = req.GetRequestParamStringValue(1);

            var qrCode = new QrCode(textToConvert, new Vector2Slim(256, 256), SKEncodedImageFormat.Png);

            using (MemoryStream memoryStream = new MemoryStream()) {
                qrCode.GenerateImage(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string outputQrCodeBase64 = Convert.ToBase64String(imageBytes);

                var resp = new MpAnalyzerPluginResponseFormat() {
                    //dataObjectLookup = new MpPortableDataObject(MpPortableDataFormats.AvPNG, imageBytes)
                    dataObjectLookup = new Dictionary<string, object> {
                        {MpPortableDataFormats.Image, imageBytes },
                        { MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, $"{req.GetRequestParamStringValue(2)} - Qr Code" }
                    }
                };
                return resp;
            }
        }
    }

    public class ExamplePlugin : MpIAnalyzeComponent {
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string textToConvert = req.GetRequestParamStringValue(1);

            var qrCode = new QrCode(textToConvert, new Vector2Slim(256, 256), SKEncodedImageFormat.Png);

            using (MemoryStream memoryStream = new MemoryStream()) {
                qrCode.GenerateImage(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string outputQrCodeBase64 = Convert.ToBase64String(imageBytes);

                var resp = new MpAnalyzerPluginResponseFormat() {
                    //dataObjectLookup = new MpPortableDataObject(MpPortableDataFormats.AvPNG, imageBytes)
                    dataObjectLookup = new Dictionary<string, object> {
                        {MpPortableDataFormats.Image, imageBytes },
                        { MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, $"{req.GetRequestParamStringValue(2)} - Qr Code" }
                    }
                };
                return resp;
            }
            return new MpAnalyzerPluginResponseFormat() {

            }
        }
    }
}
