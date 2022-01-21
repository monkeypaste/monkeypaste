using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using static MpWpfApp.MpWpfImagingHelper;

namespace MpWpfApp {
    public class MpWpfIconBuilder : MpIconBuilderBase {

        public BitmapSource CreateBorder(BitmapSource img, double scale, Color bgColor) {
            var borderBmpSrc = TintBitmapSource(img, bgColor, true);
            //var borderSize = new Size(borderBmpSrc.Width * scale, bordherBmpSrc.Height * scale);
            return ScaleBitmapSource(borderBmpSrc, new Size(scale, scale));
        }

        public override string CreateBorder(string iconBase64, double scale, string hexColor) {
            return CreateBorder(iconBase64.ToBitmapSource(), scale, hexColor.ToWinMediaColor()).ToBase64String();
        }


        public override List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            //var sw = new Stopwatch();
            //sw.Start();
            var bmpSource = iconBase64.ToBitmapSource();
            var primaryIconColorList = new List<string>();
            var hist = MpWpfImagingHelper.GetStatistics(bmpSource);
            foreach (var kvp in hist) {
                var c = Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

                //MonkeyPaste.MpConsole.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
                if (primaryIconColorList.Count == palleteSize) {
                    break;
                }
                //between 0-255 where 0 is black 255 is white
                var rgDiff = Math.Abs((int)c.R - (int)c.G);
                var rbDiff = Math.Abs((int)c.R - (int)c.B);
                var gbDiff = Math.Abs((int)c.G - (int)c.B);
                var totalDiff = rgDiff + rbDiff + gbDiff;

                //0-255 0 is black
                var grayScaleValue = 0.2126 * (int)c.R + 0.7152 * (int)c.G + 0.0722 * (int)c.B;
                var relativeDist = primaryIconColorList.Count == 0 ? 1 : ColorDistance(ConvertHexToColor(primaryIconColorList[primaryIconColorList.Count - 1]), c);
                if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
                    primaryIconColorList.Add(ConvertColorToHex(c));
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < palleteSize; i++) {
                primaryIconColorList.Add(ConvertColorToHex(GetRandomColor()));
            }
            //sw.Stop();
            //MonkeyPaste.MpConsole.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }
    }
}
