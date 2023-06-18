using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        #region Controls

        public static bool TryGetSelfOrAncestorDataContext<T>(this Control c, out T dc) where T : MpViewModelBase {
            dc = c.GetSelfOrAncestorDataContext<T>();
            return dc != default;
        }
        public static T GetSelfOrAncestorDataContext<T>(this Control c) where T : MpViewModelBase {
            if (c == null || c.DataContext is not MpViewModelBase cur_vm) {
                return default;
            }
            while (cur_vm != null) {
                if (cur_vm.GetType() == typeof(T)) {
                    return cur_vm as T;
                }
                if (cur_vm.ParentObj is MpViewModelBase par_vm) {
                    cur_vm = par_vm;
                } else {
                    cur_vm = null;
                }
            }
            return default;
        }
        #endregion

        #region Resources

        public static string ToPathFromAvResourceString(this string res_str) {
            // NOTE resource must be in same module (i think)
            List<string> path_parts = new List<string>() { AppDomain.CurrentDomain.BaseDirectory };
            path_parts.AddRange(new Uri(res_str).LocalPath.Split(@"/"));
            return Path.Combine(path_parts.ToArray());
        }

        #endregion
    }
}
