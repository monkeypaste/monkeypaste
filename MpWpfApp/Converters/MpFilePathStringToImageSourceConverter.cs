using MonkeyPaste;
using Org.BouncyCastle.Asn1.Tsp;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Web.Profile;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.System;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {

    public class MpFilePathStringToImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            BitmapSource bmpSrc = null;
            int s = parameter != null ? System.Convert.ToInt32(parameter.ToString()) : 22;
            if (value is string pathStr) {
                if(!string.IsNullOrEmpty(pathStr)) {
                    try {
                        if (pathStr.IsFileOrDirectory()) {
                            bmpSrc = MpShellEx.GetBitmapFromPath(pathStr, MpIconSize.SmallIcon16);

                        }
                        //string iconBase64Str = MpProcessHelper.MpProcessIconBuilder.GetBase64BitmapFromPath(pathStr);
                        //return new MpBase64StringToBitmapSourceConverter().Convert(iconBase64Str,targetType,parameter,culture);
                    }
                    catch (Exception ex) {
                        bmpSrc = null;
                        MpConsole.WriteTraceLine(ex);
                    }
                }
            }
            if(bmpSrc == null) {
                bmpSrc = System.Drawing.SystemIcons.Question.ToBitmap().ToBitmapSource();
            }
            return bmpSrc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
