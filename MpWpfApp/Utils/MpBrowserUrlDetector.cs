using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpBrowserUrlDetector {
        private static readonly Lazy<MpBrowserUrlDetector> _Lazy = new Lazy<MpBrowserUrlDetector>(() => new MpBrowserUrlDetector());
        public static MpBrowserUrlDetector Instance { get { return _Lazy.Value; } }


        public async Task<string> FindUrlAddressFromScreenshot(BitmapSource ssbmp) {
            if(ssbmp == null || ssbmp.Height <= 0) {
                return string.Empty;
            }
            

            var ocr = await MpImageOcr.Instance.OcrImage(ssbmp.ToByteArray());
            //var ocrText = await MpImageOcr.Instance.OcrImageForText(ssbmp.ToByteArray());
            if (ocr == null) {
                return string.Empty;
            }
            double cutoffHeight = ssbmp.Height / 4;

            foreach (var region in ocr.regions) {
                var rbb = ParseStringForRect(region.boundingBox);
                foreach (var line in region.lines) {
                    var lbb = ParseStringForRect(line.boundingBox);
                    foreach (var word in line.words) {
                        var wbb = ParseStringForRect(word.boundingBox);
                        if(rbb.Y > cutoffHeight && 
                           lbb.Y > cutoffHeight && 
                           wbb.Y > cutoffHeight) {
                            return string.Empty;
                        }
                        if(MpUrlHelpers.IsValidUrl(word.text)) {
                            return MonkeyPaste.MpUrlHelpers.GetFullyFormattedUrl(word.text);
                        }
                    }
                }
            }

            return string.Empty;
        }       

        private Rect ParseStringForRect(string rectString) {
            if(string.IsNullOrEmpty(rectString)) {
                return new Rect();
            }
            var parsedRect = new Rect();
            var rectValStrList = rectString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (int i = 0; i < rectValStrList.Count; i++) {
                try {
                    double val = Convert.ToDouble(rectValStrList[i]);
                    if (i == 0) {
                        parsedRect.X = val;
                    } else if (i == 1) {
                        parsedRect.Y = val;
                    } else if (i == 2) {
                        parsedRect.Width = val;
                    } else if (i == 3) {
                        parsedRect.Height = val;
                    }
                } catch(Exception ex) {
                    MpConsole.WriteLine(@"Error parsing rect string: " + rectString + " with exception: " + ex);
                }
            }
            return parsedRect;
        } 
    }
}
