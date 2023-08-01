using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Globalization;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvStringResourceConverter : IValueConverter {
        public static MpAvStringResourceConverter Instance = new MpAvStringResourceConverter();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            string rawUri;
            if (value == null) {
                if (parameter == null) {
                    throw new NotSupportedException();
                } else if (parameter is string) {
                    rawUri = parameter as string;
                } else {
                    throw new NotSupportedException();
                }
            } else if (value is string) {
                rawUri = value as string;
                rawUri = rawUri.Trim();
            } else {
                return null;
            }

            Uri uri;

            // Allow for assembly overrides
            if (rawUri.StartsWith("avares://")) {
                uri = new Uri(rawUri);
            } else {
                string resource_val = Mp.Services.PlatformResource.GetResource(rawUri) as string;
                if (string.IsNullOrWhiteSpace(resource_val)) {
                    return null;
                }
                uri = new Uri(resource_val);
            }

            try {
                //var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                using (var asset = AssetLoader.OpenAndGetAssembly(uri).stream) {
                    asset.Seek(0, SeekOrigin.Begin);

                    if (targetType != null && (targetType == typeof(Geometry) ||
                        targetType.IsSubclassOf(typeof(Geometry)))) {
                        using (var sr = new StreamReader(asset)) {
                            return StreamGeometry.Parse(sr.ReadToEnd());
                        }
                    }
                    if (targetType == typeof(string)) {
                        using (var sr = new StreamReader(asset)) {
                            return sr.ReadToEnd();
                        }
                    }
                    if (targetType == null || typeof(IImage).IsAssignableFrom(targetType)) {
                        return new Bitmap(asset);
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error accessing resource from '{uri}'", ex);
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
