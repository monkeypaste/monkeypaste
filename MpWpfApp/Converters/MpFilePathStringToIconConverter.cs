using MonkeyPaste;
using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {

    public class MpFilePathStringToIconConverter : IValueConverter {
        private static MpProcessHelper.MpProcessIconBuilder _pib = null;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is string pathStr) {
                if(string.IsNullOrEmpty(pathStr)) {
                    return new Image();
                }
                if(_pib == null) {
                    _pib = new MpProcessHelper.MpProcessIconBuilder(new MpWpfIconBuilder());
                }
                try {
                    return _pib.GetBase64BitmapFromFilePath(pathStr);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    return System.Drawing.SystemIcons.Question.ToBitmap().ToBitmapSource();
                }

            }
            return new Image();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
