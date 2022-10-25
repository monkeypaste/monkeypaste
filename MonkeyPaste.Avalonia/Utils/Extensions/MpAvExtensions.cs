using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvExtensions {
        #region Adorners        
        public static async Task<IEnumerable<MpAvAdornerBase>> GetControlAdornersAsync(this Control control, int timeout_ms = 1000) {
            Dispatcher.UIThread.VerifyAccess();
            var al = await control.GetAdornerLayerAsync(timeout_ms);
            if (al == null) {
                Debugger.Break();
                return null;
            }
            return al.Children.Where(x => x is MpAvAdornerBase ab && ab.AdornedControl == control).Cast<MpAvAdornerBase>();
        }

        #endregion
    }
}
