using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Markup.Xaml.Templates;
using MonkeyPaste;
namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformResource : MpIPlatformResource {
        public object GetResource(string resourceKey) {
            if(string.IsNullOrEmpty(resourceKey)) {
                return null;
            }
            if(Application.Current.Resources.TryGetResource(resourceKey,out object value)) {
                return value;
            }
            if(Application.Current.Styles.TryGetResource(resourceKey, out object styleValue)) {
                return styleValue;
            }
            return null;
        }

        public void SetResource(string resourceKey, object resourceValue) {
            if (string.IsNullOrEmpty(resourceKey)) {
                return;
            }
            if(Application.Current.Resources.TryGetResource(resourceKey, out var oldVal)) {
                Application.Current.Resources[resourceKey] = resourceValue;
                return;
            }
            if (Application.Current.Styles.TryGetResource(resourceKey, out var oldStyleVal)) {
                Application.Current.Styles.Resources[resourceKey] = resourceValue;
                return;
            }
            // whats the key? (should it be added?
            Debugger.Break();
            return;
        }
    }
}
