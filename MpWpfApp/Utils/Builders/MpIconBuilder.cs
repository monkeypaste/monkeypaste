using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIconBuilder : MpIIconBuilder {
        public string CreateBorder(string iconBase64, double scale, string hexColor) {
            var iconBmpSrc = iconBase64.ToBitmapSource();
            var bmpSrc = MpWpfApp.MpHelpers.Instance.CreateBorder(iconBmpSrc, scale, MpHelpers.Instance.ConvertHexToColor(hexColor));
            return bmpSrc.ToBase64String();
        }

        public List<string> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            var iconBmpSrc = iconBase64.ToBitmapSource();
            return MpWpfApp.MpHelpers.Instance.CreatePrimaryColorList(iconBmpSrc, palleteSize);
        }
    }
}
