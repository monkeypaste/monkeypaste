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
        public object Analyze(object args) {
            //await Task.Delay(1);

            var reqParts = MpJsonObject.DeserializeObject<MpAnalyzerPluginRequestFormat>(args.ToString()).items;
            string textToConvert = reqParts.FirstOrDefault(x => x.paramId == 1).value;

            var qrCode = new QrCode(textToConvert, new Vector2Slim(256, 256), SKEncodedImageFormat.Png);

            using (MemoryStream memoryStream = new MemoryStream()) {
                qrCode.GenerateImage(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();
                string outputQrCodeBase64 = Convert.ToBase64String(imageBytes);

                return new MpPluginResponseFormat() {
                    //newContentItem = new MpPluginResponseNewContentFormat() {
                    //    content = new MpJsonPathProperty(outputQrCodeBase64),
                    //    label = new MpJsonPathProperty("QR Code")
                    //}
                    dataObject = new MpPortableDataObject(MpPortableDataFormats.Bitmap,outputQrCodeBase64)
                };
            }
        }
    }
}
