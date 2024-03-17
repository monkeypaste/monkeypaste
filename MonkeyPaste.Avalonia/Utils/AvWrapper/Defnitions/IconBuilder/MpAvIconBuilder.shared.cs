using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder : MpIIconBuilder {

        public async Task<MpIcon> CreateAsync(
            string iconBase64,
            bool allowDup = false,
            bool suppressWrite = false) {
            var icon = await MpIcon.CreateAsync(
                iconImgBase64: iconBase64,
                allowDup: allowDup,
                suppressWrite: suppressWrite);

            return icon;
        }

        public List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            ////var sw = new Stopwatch();
            ////sw.Start();
            //var bmp = iconBase64.ToAvBitmap();
            MpConsole.WriteLine($"Icon Str: '{iconBase64}'");
            var primaryIconColorList = new List<string>();
            var hist = MpAvImageExtensions.GetStatistics(iconBase64);
            //var hist = bmp.GetStatistics();
            foreach (var kvp in hist) {
                var c = kvp.Item1; //Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

                //MpConsole.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
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


                double curMinRelativeDist = 1;
                for (int i = 0; i < primaryIconColorList.Count; i++) {
                    // 
                    double curDist = primaryIconColorList[i].ToPortableColor().ColorDistance(c);
                    curMinRelativeDist = Math.Min(curMinRelativeDist, curDist);
                }

                bool isSignificantlyDifferent_ByValue = curMinRelativeDist >= 0.15;
                bool isColorful = grayScaleValue < 200;
                bool isSignificantlyDifferent_ByChannels = totalDiff > 50;
                //primaryIconColorList.Count == 0 ? 1 : primaryIconColorList[primaryIconColorList.Count - 1].ToPortableColor().ColorDistance(c);
                if (isSignificantlyDifferent_ByChannels && isSignificantlyDifferent_ByValue && isColorful) {
                    primaryIconColorList.Add(c.ToHex());
                }
            }

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < palleteSize; i++) {
                primaryIconColorList.Add(MpColorHelpers.GetRandomHexColor());
            }
            //sw.Stop();
            //MpConsole.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }


        public bool IsStringBase64Image(string base64Str) {
            return base64Str.ToAvBitmap() != null;
        }


    }
}

