using MonkeyPaste;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MpWpfApp {
    public class MpFileContentItemViewModelToImageSourceConverter : IValueConverter {
        //returns primary source by default but secondary w/ parameter of 'SecondarySource' 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is MpContentItemViewModel civm) {
                int iconId = civm.IconId;
                string path = civm.CopyItemData;
                string iconBase64 = string.Empty;
                if (iconId > 0 && path.IsFileOrDirectory()) {
                    var ivm = MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == iconId);
                    if (ivm == default) {
                        iconBase64 = MpBase64Images.Warning;
                    } else {
                        iconBase64 = ivm.IconBase64;
                    }
                } else if (path.IsFileOrDirectory()) {
                    iconBase64 = MpShellEx.GetBitmapFromPath(path, MpIconSize.SmallIcon16).ToBase64String();
                }
                if (string.IsNullOrEmpty(iconBase64)) {
                    iconBase64 = MpBase64Images.Warning;
}

                return new MpBase64StringToBitmapSourceConverter().Convert(iconBase64, null, null, CultureInfo.CurrentCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}
