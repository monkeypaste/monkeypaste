using Avalonia;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformResource : MpIPlatformResource {
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
            if (result is string resultStr) {
                string trimmed = resultStr.Trim();
                return trimmed;
            }
            return result;
        }

        public T GetResource<T>(string resourceKey) {
            object valObj = GetResource(resourceKey);
            if (valObj is T valT) {
                return valT;
            }
            if (typeof(T) == typeof(string)) {
                if (valObj is Color color) {
                    return (T)((object)color.ToPortableColor().ToHex());
                }
            }
            if (typeof(T) == typeof(IBrush)) {
                if (valObj is Color color) {
                    return (T)((object)new SolidColorBrush(color));
                }
            }
            MpDebug.Break($"Unimplemented resource converting from '{valObj.GetType}' to '{typeof(T)}'");
            return default;
        }

        public void SetResource(string resourceKey, object resourceValue) {
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
            // whats the key? (should it be added?
            Debugger.Break();
            return;
        }
    }
}
