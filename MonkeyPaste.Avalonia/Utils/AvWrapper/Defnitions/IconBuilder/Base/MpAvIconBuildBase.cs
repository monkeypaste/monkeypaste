using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvIconBuildBase : MpIIconBuilder {
        public async Task<MpIcon> CreateAsync(string iconBase64, bool createBorder = true) {
            var icon = await MpIcon.Create(
                iconImgBase64: iconBase64,
                createBorder: createBorder);

            return icon;
        }

        //public BitmapSource CreateBorder(BitmapSource img, double scale, Color bgColor) {
        //    var borderBmpSrc = img.Tint(bgColor, true);
        //    //var borderSize = new Size(borderBmpSrc.Width * scale, bordherBmpSrc.Height * scale);
        //    return borderBmpSrc.Scale(new Size(scale, scale));
        //}

        public string CreateBorder(string iconBase64, double scale, string hexColor) {
            // return CreateBorder(iconBase64.ToBitmapSource(), scale, hexColor.ToWinMediaColor()).ToBase64String();
            return null;
        }

        public List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            ////var sw = new Stopwatch();
            ////sw.Start();
            //var bmpSource = iconBase64.ToBitmapSource();
            //var primaryIconColorList = new List<string>();
            //var hist = MpWpfImagingHelper.GetStatistics(bmpSource);
            //foreach (var kvp in hist) {
            //    var c = Color.FromArgb(255, kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue);

            //    //MpConsole.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
            //    if (primaryIconColorList.Count == palleteSize) {
            //        break;
            //    }
            //    //between 0-255 where 0 is black 255 is white
            //    var rgDiff = Math.Abs((int)c.R - (int)c.G);
            //    var rbDiff = Math.Abs((int)c.R - (int)c.B);
            //    var gbDiff = Math.Abs((int)c.G - (int)c.B);
            //    var totalDiff = rgDiff + rbDiff + gbDiff;

            //    //0-255 0 is black
            //    var grayScaleValue = 0.2126 * (int)c.R + 0.7152 * (int)c.G + 0.0722 * (int)c.B;
            //    var relativeDist = primaryIconColorList.Count == 0 ? 1 : primaryIconColorList[primaryIconColorList.Count - 1].ToWinMediaColor().ColorDistance(c);
            //    if (totalDiff > 50 && grayScaleValue < 200 && relativeDist > 0.15) {
            //        primaryIconColorList.Add(c.ToHex());
            //    }
            //}

            ////if only 1 color found within threshold make random list
            //for (int i = primaryIconColorList.Count; i < palleteSize; i++) {
            //    primaryIconColorList.Add(MpColorHelpers.GetRandomHexColor());
            //}
            ////sw.Stop();
            ////MpConsole.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            //return primaryIconColorList;
            return null;
        }

        //public string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
        //    var bmpSrc = MpShellEx.GetBitmapFromPath(appPath, iconSize);
        //    if (bmpSrc == null) {
        //        return MpBase64Images.QuestionMark;
        //    }
        //    return bmpSrc.ToBase64String();
        //}
        public abstract string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32);

    }
}

