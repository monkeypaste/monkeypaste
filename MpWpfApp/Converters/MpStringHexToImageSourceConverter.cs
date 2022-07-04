using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpStringHexToImageSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string hexStr && hexStr.IsStringHexColor()) {
                Brush brush = hexStr.ToWpfBrush();
                var bgBmp = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Images/texture.png"));
                bgBmp = bgBmp.Tint(((SolidColorBrush)brush).Color, false);
                var borderBmp = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Images/textureborder.png"));
                if (!MpWpfColorHelpers.IsBright((brush as SolidColorBrush).Color)) {
                    borderBmp = borderBmp.Tint(Colors.White, false);
                }

                var outputBmpSrc = MpWpfImagingHelper.MergeImages(new List<BitmapSource> { bgBmp, borderBmp });
                if (parameter is string paramStr) {
                    string checkPath = string.Empty;
                    if (paramStr.ToString().ToLower() == "checked") {
                        checkPath = @"/Images/check.png";
                    } else if (paramStr.ToLower() == "partial_checked") {
                        checkPath = @"/Images/check_partial.png";
                    }
                    if (!string.IsNullOrEmpty(checkPath)) {
                        var checkBmp = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + checkPath));
                        if (!MpWpfColorHelpers.IsBright((brush as SolidColorBrush).Color)) {
                            //on dark backgrounds convert black check to white check for better visualization
                            checkBmp = checkBmp.Tint(Colors.White, false);
                        }
                        outputBmpSrc = MpWpfImagingHelper.MergeImages(
                            new List<BitmapSource> { outputBmpSrc, checkBmp });
                    }
                }

                return outputBmpSrc;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
