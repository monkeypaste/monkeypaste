using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfResourceFetcher : MpINativeResource {
        public object GetResource(string resourceKey) {
            return Application.Current.Resources[resourceKey];
        }
    }
}
