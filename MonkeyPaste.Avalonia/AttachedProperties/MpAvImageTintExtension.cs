using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvImageTintExtension {
        private static Dictionary<int, string> _tintLookup = new Dictionary<int, string>();
        private static Dictionary<int, bool> _isTintingLookup = new Dictionary<int, bool>();
        static MpAvImageTintExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            TintProperty.Changed.AddClassHandler<Control>((x, y) => HandleTintChanged(x, y));
        }
        #region Properties

        #region Tint AvaloniaProperty
        public static object GetTint(AvaloniaObject obj) {
            return obj.GetValue(TintProperty);
        }

        public static void SetTint(AvaloniaObject obj, object value) {
            obj.SetValue(TintProperty, value);
        }

        public static readonly AttachedProperty<object> TintProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "Tint",
                null);
        private static void HandleTintChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            UpdateTint(element);
        }
        #endregion

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, DataGrid, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                element.AttachedToVisualTree += Element_AttachedToVisualTree;
                if (element.IsInitialized) {
                    Element_AttachedToVisualTree(element, null);
                }
            } else {
                DetachedFromVisualHandler(element, null);
            }
        }

        private static void Element_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }
            control.DetachedFromVisualTree += DetachedFromVisualHandler;

            if (control.GetLogicalDescendants().OfType<Image>() is IEnumerable<Image> imgl) {
                imgl.ForEach(x => x.GetObservable(Image.SourceProperty).Subscribe(value => UpdateTint(control)));
            }
        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            var control = s as Control;
            if (control == null) {
                return;
            }
            control.AttachedToVisualTree -= Element_AttachedToVisualTree;
            control.DetachedFromVisualTree -= DetachedFromVisualHandler;
        }


        #endregion

        private static void UpdateTint(object element) {
            if (element is Control c &&
                c.GetVisualDescendants<Image>() is IEnumerable<Image> imgl &&
                imgl.Any() &&
                GetTint(c) is object tintObj) {
                int c_hash = c.GetHashCode();
                if (!_isTintingLookup.ContainsKey(c_hash)) {
                    _isTintingLookup.Add(c_hash, true);
                } else if (_isTintingLookup[c_hash]) {
                    return;
                } else {
                    _isTintingLookup[c_hash] = true;
                }

                string hex_or_named_color_str = tintObj as string;
                if (string.IsNullOrEmpty(hex_or_named_color_str) &&
                   tintObj is ISolidColorBrush scb) {
                    hex_or_named_color_str = scb.Color.ToPortableColor().ToHex();
                }
                if (hex_or_named_color_str == Brushes.DimGray.ToPortableColor().ToHex()) {

                }
                if (hex_or_named_color_str == Brushes.Red.ToPortableColor().ToHex()) {

                }
                if (!_tintLookup.ContainsKey(c_hash)) {
                    _tintLookup.Add(c_hash, hex_or_named_color_str);
                } else if (_tintLookup[c_hash] != hex_or_named_color_str) {
                    _tintLookup[c_hash] = hex_or_named_color_str;
                } else {
                    // no change needed
                    _isTintingLookup[c_hash] = false;
                    return;
                }
                foreach (var img in imgl) {

                    if (img.Source is Bitmap bmp) {
                        if (hex_or_named_color_str.IsHexStringTransparent()) {
                            // how handle this?

                        }
                        img.Source = bmp.Tint(hex_or_named_color_str);
                    }
                }
                _isTintingLookup[c_hash] = false;
            }
        }

        #endregion
    }
}
