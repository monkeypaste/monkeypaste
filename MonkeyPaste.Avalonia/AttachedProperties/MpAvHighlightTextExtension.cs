using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvHighlightTextExtension {
        #region Private Variables
        private static Dictionary<Control, MpAvTextHighlightAdorner> _AttachedControlAdornerLookup = new Dictionary<Control, MpAvTextHighlightAdorner>();
        const string HL_INACTIVE_CLASS = "highlight-inactive";
        const string HL_ACTIVE_CLASS = "highlight-active";
        const string HL_BASE_CLASS = "highlight";
        public static IBrush DefaultInactiveHighlightBrush =>
            Mp.Services == null ||
            Mp.Services.PlatformResource == null ?
                null :
        //Brushes.Yellow;
        Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeHighlightInactiveColor);

        public static IBrush DefaultActiveHighlightBrush =>
        Mp.Services == null || Mp.Services.PlatformResource == null ?
            null :
        //Brushes.Lime;
        Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeHighlightActiveColor);
        #endregion

        #region Statics
        static MpAvHighlightTextExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            HighlightRangesProperty.Changed.AddClassHandler<Control>((x, y) => UpdateHighlights(x, nameof(HighlightRangesProperty)));
            ActiveHighlightIdxProperty.Changed.AddClassHandler<Control>((x, y) => UpdateHighlights(x, nameof(ActiveHighlightIdxProperty)));
            CanHighlightProperty.Changed.AddClassHandler<Control>((x, y) => UpdateHighlights(x, nameof(CanHighlightProperty)));
        }

        #endregion

        #region Properties

        //#region RangesInfoViewModel AvaloniaProperty
        //public static MpIHighlightTextRangesInfoViewModel GetRangesInfoViewModel(AvaloniaObject obj) {
        //    if (obj.GetValue(RangesInfoViewModelProperty) is not MpIHighlightTextRangesInfoViewModel bound_htrvm) {
        //        if (obj is not Control c ||
        //                c.DataContext == null) {
        //            return null;
        //        }
        //        if (c.DataContext is not MpIHighlightTextRangesInfoViewModel dc_htrvm) {
        //            throw new NotImplementedException($"Highlight ext needs highlight vm by datacontext or property binding");
        //        }
        //        // manually set hlr to trigger prop change attach
        //        SetRangesInfoViewModel(obj, dc_htrvm);
        //        return dc_htrvm;
        //    }
        //    return bound_htrvm;
        //}

        //public static void SetRangesInfoViewModel(AvaloniaObject obj, MpIHighlightTextRangesInfoViewModel value) {
        //    obj.SetValue(RangesInfoViewModelProperty, value);
        //}

        //public static readonly AttachedProperty<MpIHighlightTextRangesInfoViewModel> RangesInfoViewModelProperty =
        //    AvaloniaProperty.RegisterAttached<object, Control, MpIHighlightTextRangesInfoViewModel>(
        //        "RangesInfoViewModel");

        //private static void HandleRangesInfoViewModelChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
        //    void HighlightRanges_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
        //        UpdateHighlights(element);
        //    }

        //    void RangeInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
        //        if (//e.PropertyName.Contains(nameof(MpIHighlightTextRangesInfoViewModel)) ||
        //            e.PropertyName.Contains(nameof(MpIHighlightTextRangesInfoViewModel.ActiveHighlightIdx)) ||
        //            e.PropertyName.Contains(nameof(MpIHighlightTextRangesInfoViewModel.HighlightRanges))) {
        //            UpdateHighlights(element);
        //            //MpConsole.WriteLine($"highlight prop changed: '{e.PropertyName}'");
        //        }
        //        //
        //    }

        //    if (e.OldValue is MpIHighlightTextRangesInfoViewModel old_htrivm) {
        //        old_htrivm.PropertyChanged -= RangeInfoViewModel_PropertyChanged;
        //        old_htrivm.HighlightRanges.CollectionChanged -= HighlightRanges_CollectionChanged;
        //    }
        //    if (e.NewValue is MpIHighlightTextRangesInfoViewModel new_htrivm) {
        //        new_htrivm.PropertyChanged += RangeInfoViewModel_PropertyChanged;
        //        new_htrivm.HighlightRanges.CollectionChanged += HighlightRanges_CollectionChanged;
        //    }
        //    UpdateHighlights(element);
        //}


        //#endregion

        #region CanHighlight AvaloniaProperty
        public static bool GetCanHighlight(AvaloniaObject obj) {
            return obj.GetValue(CanHighlightProperty);
        }

        public static void SetCanHighlight(AvaloniaObject obj, bool value) {
            obj.SetValue(CanHighlightProperty, value);
        }

        public static readonly AttachedProperty<bool> CanHighlightProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "CanHighlight",
                defaultValue: true,
                defaultBindingMode: BindingMode.TwoWay);

        #endregion

        #region HighlightRanges AvaloniaProperty
        public static object GetHighlightRanges(AvaloniaObject obj) {
            return obj.GetValue(HighlightRangesProperty);
        }

        public static void SetHighlightRanges(AvaloniaObject obj, object value) {
            obj.SetValue(HighlightRangesProperty, value);
        }

        public static readonly AttachedProperty<object> HighlightRangesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "HighlightRanges",
                defaultValue: null);

        #endregion

        #region ActiveHighlightIdx AvaloniaProperty
        public static int GetActiveHighlightIdx(AvaloniaObject obj) {
            return obj.GetValue(ActiveHighlightIdxProperty);
        }

        public static void SetActiveHighlightIdx(AvaloniaObject obj, int value) {
            obj.SetValue(ActiveHighlightIdxProperty, value);
        }

        public static readonly AttachedProperty<int> ActiveHighlightIdxProperty =
            AvaloniaProperty.RegisterAttached<int, Control, int>(
                "ActiveHighlightIdx",
                defaultValue: -1);

        #endregion

        #region ActiveHighlightBrush AvaloniaProperty
        public static object GetActiveHighlightBrush(AvaloniaObject obj) {
            return obj.GetValue(ActiveHighlightBrushProperty);
        }

        public static void SetActiveHighlightBrush(AvaloniaObject obj, object value) {
            obj.SetValue(ActiveHighlightBrushProperty, value);
        }

        public static readonly AttachedProperty<object> ActiveHighlightBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "ActiveHighlightBrush",
                defaultValue: null);// 

        #endregion

        #region InactiveHighlightBrush AvaloniaProperty
        public static object GetInactiveHighlightBrush(AvaloniaObject obj) {
            return obj.GetValue(InactiveHighlightBrushProperty);
        }

        public static void SetInactiveHighlightBrush(AvaloniaObject obj, object value) {
            obj.SetValue(InactiveHighlightBrushProperty, value);
        }

        public static readonly AttachedProperty<object> InactiveHighlightBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "InactiveHighlightBrush",
                defaultValue: null);// 

        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control attached_control, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal) {
                if (isEnabledVal) {
                    attached_control.Loaded += Attached_control_Loaded;
                    attached_control.Unloaded += Attached_control_Unloaded;
                    attached_control.EffectiveViewportChanged += Attached_control_EffectiveViewportChanged;
                    if (attached_control.IsInitialized) {
                        Attached_control_Loaded(attached_control, null);
                    }

                } else {
                    Attached_control_Unloaded(attached_control, null);
                }
            }
        }


        private static void Attached_control_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control attached_control) {
                return;
            }

            UpdateHighlights(attached_control, null);
        }

        private static void Attached_control_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            if (sender is not Control attached_control ||
                sender is HtmlPanel) {
                return;
            }
            UpdateHighlights(attached_control, null);
        }

        private static void Attached_control_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control attached_control) {
                return;
            }
            attached_control.Loaded += Attached_control_Loaded;
            attached_control.Unloaded += Attached_control_Unloaded;
            if (!_AttachedControlAdornerLookup.ContainsKey(attached_control)) {
                return;
            }
            RemoveVisualAdorner(attached_control, _AttachedControlAdornerLookup[attached_control], AdornerLayer.GetAdornerLayer(attached_control));
            _AttachedControlAdornerLookup.Remove(attached_control);
        }
        #endregion


        #endregion

        private static void UpdateHighlights(Control attached_control, string sourcePropertyName) {
            if (!GetIsEnabled(attached_control)) {
                return;
            }
            if (!GetCanHighlight(attached_control) &&
                sourcePropertyName != nameof(CanHighlightProperty)) {
                return;
            }
            if (attached_control is MpAvMarqueeTextBox mtb) {
                mtb.HighlightRanges = GetHighlightRanges(attached_control) as IEnumerable<MpTextRange>;
                mtb.ActiveHighlightIdx = GetActiveHighlightIdx(attached_control);
                mtb.Redraw();
                return;
            }
            HighlightTextControl(attached_control);
        }

        private static void HighlightTextControl(Control attached_control) {
            if (GetHighlightRanges(attached_control) is not IEnumerable<MpTextRange> trl ||
                trl.ToArray() is not { } HighlightRanges) {
                return;
            }
            int ActiveHighlightIdx = GetActiveHighlightIdx(attached_control);

            async Task FinishUpdate(IEnumerable<(IBrush, Geometry)> gl) {
                if (!_AttachedControlAdornerLookup.TryGetValue(attached_control, out var tha)) {
                    tha = new MpAvTextHighlightAdorner(attached_control);
                    _AttachedControlAdornerLookup.Add(attached_control, tha);
                    await attached_control.AddOrReplaceAdornerAsync(tha);
                }
                tha.DrawHighlights(gl);
                if (attached_control is not TextBlock tb ||
                    string.IsNullOrEmpty(tb.Text)) {
                    return;
                }
                string text = tb.Text;
                tb.SetCurrentValue(TextBox.TextProperty, string.Empty);
                tb.Inlines.Clear();
                if (gl == null || !HighlightRanges.Any()) {
                    // reset inlines
                    tb.Text = text;
                    tb.SetCurrentValue(TextBox.TextProperty, text);
                    return;
                }
                var gll = gl.ToList();
                foreach (var (tr, idx) in HighlightRanges.WithIndex()) {
                    IBrush fg = tb.Foreground;
                    IBrush bg = gll[idx].Item1;
                    if (bg is SolidColorBrush scb) {
                        fg = scb.ToHex().ToContrastForegoundColor().ToAvBrush();
                    }
                    if (idx == 0 && tr.StartIdx > 0) {
                        tb.Inlines.Add(new Run(text.Substring(0, tr.StartIdx)));
                    }
                    tb.Inlines.Add(new Run(text.Substring(tr.StartIdx, tr.Count)) { Foreground = fg, Background = bg });

                    int end_idx = tr.StartIdx + tr.Count;
                    if (idx == HighlightRanges.Length - 1) {
                        tb.Inlines.Add(new Run(text.Substring(end_idx, text.Length - end_idx)));
                    } else if (HighlightRanges[idx + 1].StartIdx - end_idx > 1) {
                        tb.Inlines.Add(new Run(text.Substring(end_idx + 1, HighlightRanges[idx + 1].StartIdx - end_idx)));
                    }
                }
            }
            // create formatted text
            FormattedText ft = null;
            bool is_empty = false;
            if (attached_control.TryGetVisualDescendant<TextBox>(out var tb)) {
                ft = tb.ToFormattedText(true);
                is_empty = string.IsNullOrEmpty(tb.Text);
            } else if (attached_control.TryGetVisualDescendant<TextBlock>(out var tbl)) {
                ft = tbl.ToFormattedText(true);
                is_empty = string.IsNullOrEmpty(tbl.Text);
            } else {
                MpDebug.Break($"unknown control type '{attached_control.GetType()}', need formatted text to highlight");
                return;
            }

            if (is_empty ||
                HighlightRanges.Where(x => x.Document == null || x.Document == attached_control) is not IEnumerable<MpTextRange> hlrl ||
                !hlrl.Any()) {
                // no text or no highlights
                FinishUpdate(null).FireAndForgetSafeAsync();
                return;
            }

            // create brushes/geometry
            var all_brl =
                HighlightRanges
                .Select((x, idx) =>
                    (
                        idx == ActiveHighlightIdx ?
                            GetActiveHighlightBrush(attached_control) ?? DefaultActiveHighlightBrush :
                            GetInactiveHighlightBrush(attached_control) ?? DefaultInactiveHighlightBrush,
                        x));
            var gl =
                all_brl
                .Where(x => x.Item2.Document == null || x.Item2.Document == attached_control)
                .Select(x => (
                    x.Item1.ToHex().ToAvBrush() as IBrush,
                    ft.BuildHighlightGeometry(new Point(), x.Item2.StartIdx, x.Item2.Count)));

            FinishUpdate(gl).FireAndForgetSafeAsync();
        }

        private static void RemoveVisualAdorner(Visual visual, Control? adorner, AdornerLayer? layer) {
            if (adorner is null || layer is null || !layer.Children.Contains(adorner)) {
                return;
            }

            layer.Children.Remove(adorner);
            ((ISetLogicalParent)adorner).SetParent(null);
        }
    }

    internal class MpAvTextHighlightAdorner : MpAvAdornerBase {
        private IEnumerable<(IBrush, Geometry)> _gl;
        public MpAvTextHighlightAdorner(Control adornedControl) : base(adornedControl) { }

        public void Clear() {
            IsVisible = false;
            this.Redraw();
        }
        public void DrawHighlights(IEnumerable<(IBrush, Geometry)> gl) {
            _gl = gl;
            IsVisible = _gl != null && _gl.Any();
            this.Redraw();
        }
        public override void Render(DrawingContext context) {
            if (IsVisible) {
                _gl.Where(x => x.Item2 != null).ForEach(x => context.DrawGeometry(x.Item1, null, x.Item2));
            }
            base.Render(context);
        }
    }
}
