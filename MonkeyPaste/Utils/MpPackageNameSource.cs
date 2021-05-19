using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    [TypeConverter(typeof(MpPackageNameSourceConverter))]
    public sealed class MpPackageNameSource : ImageSource {
        public static readonly BindableProperty PackageNameProperty = BindableProperty.Create(nameof(PackageName), typeof(string), typeof(MpPackageNameSource), default(string));

        public static ImageSource FromPackageName(string packageName) {
            return new MpPackageNameSource { PackageName = packageName };
        }
                
        public string PackageName {
            get { return (string)GetValue(PackageNameProperty); }
            set { SetValue(PackageNameProperty, value); }
        }

        public override Task<bool> Cancel() {
            return Task.FromResult(false);
        }

        public override string ToString() {
            return $"PackageName: {PackageName}";
        }

        public static implicit operator MpPackageNameSource(string packageName) {
            return (MpPackageNameSource)FromPackageName(packageName);
        }

        public static implicit operator string(MpPackageNameSource packageNameSource) {
            return packageNameSource != null ? packageNameSource.PackageName : null;
        }

        protected override void OnPropertyChanged(string propertyName = null) {
            if (propertyName == PackageNameProperty.PropertyName)
                OnSourceChanged();
            base.OnPropertyChanged(propertyName);
        }
    }
}
