using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Reflection;

namespace MonkeyPaste {
    public static class MpExtensions {
        public static void DoubleBuffered(this Control control, bool enabled) {
            var prop = control.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop.SetValue(control, enabled, null);
        }
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable) {
            return new ObservableCollection<T>(enumerable);
        }
    }
}
