using System;
using System.Collections.Generic;
using System.Text;
//using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MonkeyPaste {
    [TypeConversion(typeof(MpPackageNameSource))]
    public sealed class MpPackageNameSourceConverter : TypeConverter {
        public override object ConvertFromInvariantString(string value) {
            if (value != null)
                return MpPackageNameSource.FromPackageName(value);

            throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(MpPackageNameSource)));
        }
    }
}
