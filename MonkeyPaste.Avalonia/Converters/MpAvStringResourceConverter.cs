using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Platform;
using Avalonia;
using System.Reflection;
using Avalonia.Media.Imaging;
using Avalonia.Media;
using System.IO;
using Google.Apis.Logging;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringResourceConverter : IValueConverter {
        public static MpAvStringResourceConverter Instance = new MpAvStringResourceConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string rawUri;
            if (value == null) {
                if(parameter == null) {
                    throw new NotSupportedException();
                } else if(parameter is string) {
                    rawUri = parameter as string;
                } else {
                    throw new NotSupportedException();
                }
            } else if(value is string) {
                rawUri = value as string;
            } else {
                return null;
            }

            Uri uri;

            // Allow for assembly overrides
            if (rawUri.StartsWith("avares://")) {
                uri = new Uri(rawUri);
            } else {
                string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                uri = new Uri($"avares://{assemblyName}{rawUri}");
            }

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var asset = assets.Open(uri);

            if (targetType != null && (targetType == typeof(Geometry) ||
                targetType.IsSubclassOf(typeof(Geometry)))) {
                using (var sr = new StreamReader(asset)) {
                    return StreamGeometry.Parse(sr.ReadToEnd());
                }
            }
            if (targetType == null || typeof(IImage).IsAssignableFrom(targetType)) { 
                return new Bitmap(asset);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

    public class MpAvStringResourceToGeometryConverter : IValueConverter {
        public static MpAvStringResourceToGeometryConverter Instance = new MpAvStringResourceToGeometryConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            return MpAvStringResourceConverter.Instance.Convert(value, typeof(Geometry), parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }

}
