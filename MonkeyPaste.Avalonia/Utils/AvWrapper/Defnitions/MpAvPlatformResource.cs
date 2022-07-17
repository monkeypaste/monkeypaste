using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using MonkeyPaste;
namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformResource : MpIPlatformResource {
        public object GetResource(string resourceKey) {
            if(Application.Current.Resources.TryGetResource(resourceKey,out object value)) {
                return value;
            }
            return null;
        }
    }
}
