using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpIconBuilder : MonkeyPaste.MpIIconBuilder {
        public async Task<string> CreateBorder(string iconBase64, double scale, string hexColor) {
            var iconBmpSrc = iconBase64.ToBitmapSource();
            var bmpSrc = await MpHelpers.Instance.CreateBorderAsync(iconBmpSrc, scale, MpHelpers.Instance.ConvertHexToColor(hexColor));
            
            return bmpSrc.ToBase64String();
        }

        public async Task<List<string>> CreatePrimaryColorList(string iconBase64, int palleteSize = 5) {
            var iconBmpSrc = iconBase64.ToBitmapSource();
            var colorList = await MpHelpers.Instance.CreatePrimaryColorListAsync(iconBmpSrc, palleteSize);

            return colorList;
        }
    }
}
