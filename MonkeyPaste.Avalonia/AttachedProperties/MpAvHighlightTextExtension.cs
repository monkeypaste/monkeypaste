﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvHighlightTextExtension {
        #region Private Variables
        private static double _DefaultOpacity = 0.5d;
        private static Dictionary<Control, MpAvTextHighlightAdorner> _AttachedControlAdornerLookup = new Dictionary<Control, MpAvTextHighlightAdorner>();

        private static IBrush _DefaultInactiveHighlightBrush =>
            Mp.Services == null || Mp.Services.PlatformResource == null ?
                null : Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent1BgColor.ToString()).AdjustAlpha(_DefaultOpacity).ToAvBrush();

        private static IBrush _DefaultActiveHighlightBrush =>
        Mp.Services == null || Mp.Services.PlatformResource == null ?
            null : Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent3Color.ToString()).AdjustAlpha(_DefaultOpacity).ToAvBrush();
        #endregion

        #region Statics
        static MpAvHighlightTextExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            RangesInfoViewModelProperty.Changed.AddClassHandler<Control>((x, y) => HandleRangesInfoViewModelChanged(x, y));
        }

        #endregion

        #region Properties

        #region RangesInfoViewModel AvaloniaProperty
        public static MpIHighlightTextRangesInfoViewModel GetRangesInfoViewModel(AvaloniaObject obj) {
            if (obj.GetValue(RangesInfoViewModelProperty) is not MpIHighlightTextRangesInfoViewModel bound_htrvm) {
                if (obj is not Control c ||
                        c.DataContext == null) {
                    return null;
                }
                if (c.DataContext is not MpIHighlightTextRangesInfoViewModel dc_htrvm) {
                    throw new NotImplementedException($"Highlight ext needs highlight vm by datacontext or property binding");
                }
                // manually set hlr to trigger prop change attach
                SetRangesInfoViewModel(obj, dc_htrvm);
                return dc_htrvm;
            }
            return bound_htrvm;
        }

        public static void SetRangesInfoViewModel(AvaloniaObject obj, MpIHighlightTextRangesInfoViewModel value) {
            obj.SetValue(RangesInfoViewModelProperty, value);
        }

        public static readonly AttachedProperty<MpIHighlightTextRangesInfoViewModel> RangesInfoViewModelProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpIHighlightTextRangesInfoViewModel>(
                "RangesInfoViewModel");

        private static void HandleRangesInfoViewModelChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            void HighlightRanges_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                UpdateHighlights(element);
            }

            void RangeInfoViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                if (!e.PropertyName.StartsWith(nameof(MpIHighlightTextRangesInfoViewModel))) {
                    return;
                }
                //MpConsole.WriteLine($"highlight prop changed: '{e.PropertyName}'");
                UpdateHighlights(element);
            }

            if (e.OldValue is MpIHighlightTextRangesInfoViewModel old_htrivm) {
                old_htrivm.PropertyChanged -= RangeInfoViewModel_PropertyChanged;
                old_htrivm.HighlightRanges.CollectionChanged -= HighlightRanges_CollectionChanged;
            }
            if (e.NewValue is MpIHighlightTextRangesInfoViewModel new_htrivm) {
                new_htrivm.PropertyChanged += RangeInfoViewModel_PropertyChanged;
                new_htrivm.HighlightRanges.CollectionChanged += HighlightRanges_CollectionChanged;
            }
            UpdateHighlights(element);
        }


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
                    attached_control.AttachedToLogicalTree += Attached_control_AttachedToLogicalTree;
                    attached_control.DetachedFromLogicalTree += Attached_control_DetachedFromLogicalTree;
                    attached_control.EffectiveViewportChanged += Attached_control_EffectiveViewportChanged;
                    if (attached_control.IsInitialized) {
                        Attached_control_AttachedToLogicalTree(attached_control, null);
                    }

                } else {
                    Attached_control_DetachedFromLogicalTree(attached_control, null);
                }
            }
        }

        private static void Attached_control_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            if (sender is not Control attached_control) {
                return;
            }
            UpdateHighlights(attached_control);
        }

        private static void Attached_control_AttachedToLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (sender is not Control attached_control ||
                _AttachedControlAdornerLookup.ContainsKey(attached_control)) {
                return;
            }
            var tha = new MpAvTextHighlightAdorner(attached_control);
            _AttachedControlAdornerLookup.Add(attached_control, tha);
            Dispatcher.UIThread.Post(async () => {

                //AddVisualAdorner(attached_control, tha, AdornerLayer.GetAdornerLayer(attached_control));
                await attached_control.AddOrReplaceAdornerAsync(tha, -1);

                UpdateHighlights(attached_control);
            });
        }

        private static void Attached_control_DetachedFromLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (sender is not Control attached_control) {
                return;
            }
            attached_control.AttachedToLogicalTree += Attached_control_AttachedToLogicalTree;
            attached_control.DetachedFromLogicalTree += Attached_control_DetachedFromLogicalTree;

            if (!_AttachedControlAdornerLookup.ContainsKey(attached_control)) {
                return;
            }
            RemoveVisualAdorner(attached_control, _AttachedControlAdornerLookup[attached_control], AdornerLayer.GetAdornerLayer(attached_control));
            _AttachedControlAdornerLookup.Remove(attached_control);
        }
        #endregion


        #endregion

        private static void UpdateHighlights(Control attached_control) {
            if (!_AttachedControlAdornerLookup.TryGetValue(attached_control, out var tha)) {
                return;
            }

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
                GetRangesInfoViewModel(attached_control) is not MpIHighlightTextRangesInfoViewModel hrivm ||
                hrivm.HighlightRanges.Where(x => x.Document == attached_control) is not IEnumerable<MpTextRange> hlrl ||
                !hlrl.Any()) {
                tha.DrawHighlights(null);
                return;
            }

            var all_brl =
                hrivm
                .HighlightRanges
                .Select((x, idx) =>
                    (
                        idx == hrivm.ActiveHighlightIdx ?
                            GetActiveHighlightBrush(attached_control) ?? _DefaultActiveHighlightBrush :
                            GetInactiveHighlightBrush(attached_control) ?? _DefaultInactiveHighlightBrush,
                        x));
            var gl =
                all_brl
                .Where(x => x.Item2.Document == attached_control)
                .Select(x => (
                    x.Item1.ToHex().ToAvBrush() as IBrush,
                    ft.BuildHighlightGeometry(new Point(), x.Item2.StartIdx, x.Item2.Count)));

            tha.DrawHighlights(gl);
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
            InvalidateVisual();
        }
        public void DrawHighlights(IEnumerable<(IBrush, Geometry)> gl) {
            _gl = gl;
            IsVisible = _gl != null && _gl.Any();
            Dispatcher.UIThread.Post(InvalidateVisual);
        }
        public override void Render(DrawingContext context) {
            if (IsVisible) {
                _gl.Where(x => x.Item2 != null).ForEach(x => context.DrawGeometry(x.Item1, null, x.Item2));
            }
            base.Render(context);
        }
    }
}