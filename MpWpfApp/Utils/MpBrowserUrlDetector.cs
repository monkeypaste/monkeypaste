using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
                foreach (var line in region.lines) {
                    foreach (var word in line.words) {
                        var bb = ParseStringForRect(word.boundingBox);
                        if(bb.Y > cutoffHeight) {
                            return string.Empty;
                        }
                        if(MpHelpers.Instance.IsValidUrl(word.text)) {
                            return MpHelpers.Instance.GetFullyFormattedUrl(word.text);
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
                    Console.WriteLine(@"Error parsing rect string: " + rectString + " with exception: " + ex);
                }
            }
            return parsedRect;
        } 
    }
}
