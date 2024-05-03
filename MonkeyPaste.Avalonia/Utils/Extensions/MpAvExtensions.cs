using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvExtensions {
        #region Private Variables
        private static Dictionary<AvaloniaObject, List<IDisposable>> _disposableLookup = [];
        #endregion

        #region Plugins

        public static MpIManagePluginComponents GetComponentManager(this MpManifestFormat mf) {
            if (mf == null) {
                return null;
            }
            switch (mf.pluginType) {
                case MpPluginType.Clipboard:
                    return MpAvClipboardHandlerCollectionViewModel.Instance;
                case MpPluginType.Analyzer:
                case MpPluginType.Fetcher:
                    return MpAvAnalyticItemCollectionViewModel.Instance;
                default:
                    return null;

            }
        }
        #endregion

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

        #region Screens

        public static Screen AsScreen(this IRenderRoot rr) {
            return new Screen(
                            1,
                            new PixelRect(rr.ClientSize.ToAvPixelSize(1)),
                            new PixelRect(rr.ClientSize.ToAvPixelSize(1)),
                            true);
        }
        public static MpPoint GetScreenMousePoint(this PointerEventArgs e) {
            //var c_mp = e.GetPosition(control).ToPortablePoint();
            //var c_origin = control.PointToScreen(new Point()).ToPortablePoint(control.VisualPixelDensity());
            //return c_mp + c_origin;
#if MOBILE
            return e.GetPosition(App.MainView).ToPortablePoint();
#else
            if (e.Source is not Control c ||
                c.GetScreen() is not Screen scr) {
                return MpPoint.Zero;
            }
            var c_mp = e.GetPosition(c);
            var scr_mp = c.PointToScreen(c_mp);
            var result = scr_mp.ToPortablePoint(scr.Scaling);
            return result;
#endif
        }
        public static Screen GetScreen(this Visual v) {
            if (MpAvWindowManager.GetTopLevel(v) is not WindowBase tl) {
                return null;
            }
            return tl.Screens.ScreenFromVisual(v);
        }
        public static double VisualPixelDensity(this Visual visual, Window w = null) {

            if (w == null &&
                MpAvWindowManager.GetTopLevel(visual) is Window tlw) {
                //Application.Current.GetMainTopLevel() is Window mw) {
                w = tlw;
            }

            if (w == null) {
                return 1;
            }
            if (visual == null) {
                //return w.Screens.Primary.PixelDensity;
                return w.Screens.Primary.Scaling;
            }
            var scr = w.Screens.ScreenFromVisual(visual);
            if (scr == null && Application.Current.GetMainTopLevel() is Window mw2) {
                scr = mw2.Screens.Primary;
                if (scr == null) {
                    if (mw2.Screens.All.FirstOrDefault() is { } mw3) {
                        scr = mw3;
                    } else {
                        MpDebug.Break();
                        return 1;
                    }
                }
            }
            //return scr.PixelDensity;
            return scr.Scaling;
        }
        public static Screen Primary(this Screens screens) {
            return screens.All.FirstOrDefault(x => x.IsPrimary);
        }
        public static Screen ScreenFromPoint_WORKS(this Screens screens, PixelPoint pp) {
            return screens.All.FirstOrDefault(x => x.Bounds.Contains(pp));
        }
        public static MpIPlatformScreenInfo ToScreenInfo(this Screen screen) {
            return new MpAvDesktopScreenInfo(screen);
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
        public static bool TryGetSelfOrAncestorDataContext<T>(this Control c, out T dc) where T : MpIViewModel {
            dc = c.GetSelfOrAncestorDataContext<T>();
            return dc != null;
        }
        public static T GetSelfOrAncestorDataContext<T>(this Control c) where T : MpIViewModel {
            if (c == null || c.DataContext is not MpIViewModel cur_vm) {
                return default;
            }
            while (cur_vm != null) {
                if (cur_vm is T tvm) {
                    return tvm;
                }
                if (cur_vm is MpIHierarchialViewModel hvm &&
                    hvm.ParentObj is MpIViewModel par_vm) {
                    cur_vm = par_vm;
                } else {
                    cur_vm = null;
                }
            }
            return default;
        }

        public static void SetHtml(this HtmlControl hc, string html) {
            hc.SetCurrentValue(HtmlControl.TextProperty, html);
        }

        public static void AddDisposable(this IDisposable disp, AvaloniaObject hostObject) {
            if (!_disposableLookup.TryGetValue(hostObject, out var displ)) {
                displ = [];
            }
            if (!displ.Contains(disp)) {
                displ.Add(disp);
            }
        }

        public static void ClearDisposables(this AvaloniaObject hostObject) {
            if (!_disposableLookup.TryGetValue(hostObject, out var displ)) {
                // none found
                return;
            }
            foreach (var disp in displ) {
                disp.Dispose();
            }
            displ.Clear();
            _disposableLookup.Remove(hostObject);
        }
        #endregion

        #region Strings

        public static string ToReadableTimeSpan(this DateTime dt) {
            TimeSpan ts = DateTime.Now - dt;
            int totalYears = (int)(ts.TotalDays / 365);
            int totalMonths = DateTime.Now.MonthDifference(dt);
            int totalWeeks = DateTime.Now.WeekDifference(dt);
            int totalDays = (int)ts.TotalDays;
            int totalHours = (int)ts.TotalHours;
            int totalMinutes = (int)ts.TotalMinutes;

            if (totalYears > 1) {
                return string.Format(UiStrings.TimeSpanYears, totalYears);
            }
            if (totalMonths >= 1 && totalWeeks >= 4) {
                if (totalMonths == 1) {
                    return UiStrings.TimeSpanMonthLast;
                }
                return string.Format(UiStrings.TimeSpanMonths, totalMonths);
            }
            if (totalWeeks >= 1) {
                if (totalWeeks == 1) {
                    return UiStrings.TimeSpanWeekLast;
                }
                return string.Format(UiStrings.TimeSpanWeeks, totalWeeks);
            }
            if (totalDays >= 1) {
                if (totalDays == 1) {
                    return UiStrings.TimeSpanDayLast;
                }
                return string.Format(UiStrings.TimeSpanDays, totalDays);
            }
            if (totalHours >= 1) {
                if (totalHours == 1) {
                    return UiStrings.TimeSpanHourLast;
                }
                return string.Format(UiStrings.TimeSpanHours, totalHours);
            }
            if (totalMinutes >= 1) {
                if (totalMinutes == 1) {
                    return UiStrings.TimeSpanMinute;
                }
                return string.Format(UiStrings.TimeSpanMinutes, totalMinutes);
            }
            return UiStrings.TimeSpanJustNow;
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
