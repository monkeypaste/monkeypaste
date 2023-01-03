using System;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using System.Linq;
using System.IO;
using SkiaSharp.QrCode.Image;
using SkiaSharp;

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
                    //newContentItem = new MpPluginResponseNewContentFormat() {                       
                    //    content = new MpJsonPathProperty(outputQrCodeBase64),
                    //    label = new MpJsonPathProperty($"{req.GetRequestParamStringValue(2)} - Qr Code"),
                    //    format = "PNG"
                    //}
                    dataObject = new MpPortableDataObject(MpPortableDataFormats.AvPNG, imageBytes)
                };
                resp.dataObject.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_TITLE_FORMAT, $"{req.GetRequestParamStringValue(2)} - Qr Code");
                return resp;
            }
        }
    }
}
