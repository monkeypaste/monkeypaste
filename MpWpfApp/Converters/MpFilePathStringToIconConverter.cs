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
using MonkeyPaste.Plugin;

namespace MpWpfApp {

    public class MpFilePathStringToIconConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            BitmapSource bmpSrc = null;
            int s = parameter != null ? System.Convert.ToInt32(parameter.ToString()) : 22;
            if (value is string pathStr) {
                if(!string.IsNullOrEmpty(pathStr)) {
                    try {
                        string filePath = pathStr;
                        if (File.Exists(pathStr) || Directory.Exists(pathStr)) {
                            using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(pathStr)) {
                                bmpSrc = Imaging.CreateBitmapSourceFromHIcon(
                                                    icon.Handle,
                                                    new Int32Rect(0, 0, s, s),
                                                    BitmapSizeOptions.FromEmptyOptions());
                            }

                        }
                        //string iconBase64Str = MpProcessHelper.MpProcessIconBuilder.GetBase64BitmapFromPath(pathStr);
                        //return new MpBase64StringToBitmapSourceConverter().Convert(iconBase64Str,targetType,parameter,culture);
                    }
                    catch (Exception ex) {
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
