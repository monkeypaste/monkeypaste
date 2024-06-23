using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformResource : MpIPlatformResource {
        string[] _binaryResourceExtensions = [
            "zip",
            "wav"
            ];
        public object GetResource(string resourceKey) {
            if (string.IsNullOrEmpty(resourceKey)) {
                return null;
            }
            object result = null;
            if (Application.Current.Resources.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out object value)) {
                result = value;
            } else if (Application.Current.Styles.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out object styleValue)) {
                result = styleValue;
            }
            if (result == null && _binaryResourceExtensions.Contains(resourceKey.SplitNoEmpty(".").LastOrDefault())) {
                // handle dats
                using (var stream = AssetLoader.Open(new Uri(resourceKey, UriKind.RelativeOrAbsolute))) {
                    stream.Seek(0, SeekOrigin.Begin);

                    using (var ms = new MemoryStream()) {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            if (result is string resultStr) {
                string trimmed = resultStr.Trim();
                return trimmed;
            }
            return result;
        }

        public T GetResource<T>(MpThemeResourceKey resourceKey) =>
            GetResource<T>(resourceKey.ToString());

        public T GetResource<T>(string resourceKey) {
            object valObj = GetResource(resourceKey);
            if (valObj is T valT) {
                return valT;
            }

            if (typeof(T) == typeof(string)) {
                if (valObj is Color color) {
                    return (T)((object)color.ToPortableColor().ToHex(true));
                }
                if (valObj is SolidColorBrush scb) {
                    return (T)(object)scb.Color.ToPortableColor().ToHex().AdjustAlpha(scb.Opacity);
                }
            }
            if (typeof(T) == typeof(IBrush)) {
                if (valObj is Color color) {
                    return (T)((object)new SolidColorBrush(color));
                }
            }
            if (typeof(T) == typeof(Color)) {
                if (valObj is Color color) {
                    return (T)((object)new SolidColorBrush(color));
                }
            }
            if (typeof(T) == typeof(byte[])) {
                if (valObj is string valStr) {
                    object result = MpAvStringResourceConverter.Instance.Convert(valStr, null, null, null);
                    if (result is Bitmap bmp) {
                        return (T)((object)bmp.ToByteArray());
                    }
                }
            }
            if (valObj != null) {
                MpDebug.Break($"Unimplemented resource conversion from '{valObj.GetType()}' to '{typeof(T)}'");
            } else {
                MpDebug.Break($"Cannot find resource w/ key '{resourceKey}'");
            }
            return default;
        }

        public void SetResource(string resourceKey, object resourceValue, bool addIfMissing) {
            if (string.IsNullOrEmpty(resourceKey)) {
                return;
            }
            if (Application.Current.Resources.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out var oldVal)) {
                Application.Current.Resources[resourceKey] = resourceValue;
                return;
            }
            if (Application.Current.Styles.TryGetResource(resourceKey, Application.Current.ActualThemeVariant, out var oldStyleVal)) {
                Application.Current.Styles.Resources[resourceKey] = resourceValue;
                return;
            }
            if(!addIfMissing) {
                // whats the key? (should it be added?
                MpDebug.Break();
                return;
            }
            Application.Current.Resources.Add(new(resourceKey, resourceValue));
            MpConsole.WriteLine($"Resource '{resourceKey}' added with value '{resourceValue}'");
        }
    }
}
