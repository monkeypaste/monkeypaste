using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
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
                MpDebug.Break();
                return null;
            }
            return al.Children.Where(x => x is MpAvAdornerBase ab && ab.AdornedControl == control).Cast<MpAvAdornerBase>();
        }

        #endregion

        #region Screen

        public static PixelPoint GetSystemTrayWindowPosition(this Screen s, PixelSize ws, int pad = 10) {
            //ws = (ws.ToPortablePoint() * s.Scaling).ToPortableSize();
            int x = s.Bounds.Right - ws.Width - pad;
#if MAC
            int y = s.WorkingArea.TopRight.Y + pad;
#else
            int y = s.WorkingArea.Bottom - ws.Height - pad;
#endif
            //x /= s.Scaling;
            //y /= s.Scaling;

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);
            return new PixelPoint(x, y);
        }

        public static Screen GetScreen(this Visual v) {
            if (TopLevel.GetTopLevel(v) is not WindowBase tl) {
                return null;
            }
            return tl.Screens.ScreenFromVisual(v);
        }
        #endregion

        #region Controls

        public static void Redraw(this Control control) {
            try {
                // NOTE trying to fix egl context bug, thinking this is executing

                Dispatcher.UIThread.Post(control.InvalidateVisual);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error rendering control. ", ex);
            }
        }

        public static bool IsTextInputControl(this Control control) {
            // NOTE only returns true if control is in a state that would receive text

            if (control == null) {
                return false;
            }

            if (control is TextBox tb) {
                return !tb.IsReadOnly;
            }

            if (control is MpAvContentWebView wv) {
                return !wv.IsContentReadOnly;
            }
            if (control is AutoCompleteBox) {
                return true;
            }
            return false;
        }
        public static bool TryGetSelfOrAncestorDataContext<T>(this Control c, out T dc) where T : MpAvViewModelBase {
            dc = c.GetSelfOrAncestorDataContext<T>();
            return dc != default;
        }
        public static T GetSelfOrAncestorDataContext<T>(this Control c) where T : MpAvViewModelBase {
            if (c == null || c.DataContext is not MpAvViewModelBase cur_vm) {
                return default;
            }
            while (cur_vm != null) {
                if (cur_vm.GetType() == typeof(T)) {
                    return cur_vm as T;
                }
                if (cur_vm.ParentObj is MpAvViewModelBase par_vm) {
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
            if (!res_str.IsStringResourcePath()) {
                MpDebug.Break($"invalid resource uri '{res_str}'");
                return null;
            }

            List<string> path_parts = new List<string>() { AppDomain.CurrentDomain.BaseDirectory };
            path_parts.AddRange(new Uri(res_str).LocalPath.Split(@"/"));
            return Path.Combine(path_parts.ToArray());
        }


        #endregion

        #region Text Search

        public static IEnumerable<(int, int)> QueryText(this string search_text, MpITextMatchInfo tmi) {
            return search_text.QueryText(tmi.MatchValue, tmi.CaseSensitive, tmi.WholeWord, tmi.UseRegex);
        }
        #endregion
    }
}
