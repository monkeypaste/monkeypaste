using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    public static class MpAvWindowPositionExtension {
        static MpAvWindowPositionExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Window>((x, y) => HandleIsEnabledChanged(x, y));
            WindowXProperty.Changed.AddClassHandler<Window>((x, y) => HandleWindowXChanged(x, y));
            WindowYProperty.Changed.AddClassHandler<Window>((x, y) => HandleWindowYChanged(x, y));
        }
        #region Properties

        #region WindowX AvaloniaProperty
        public static int? GetWindowX(AvaloniaObject obj) {
            return obj.GetValue(WindowXProperty);
        }

        public static void SetWindowX(AvaloniaObject obj, int? value) {
            obj.SetValue(WindowXProperty, value);
        }

        public static readonly AttachedProperty<int?> WindowXProperty =
            AvaloniaProperty.RegisterAttached<object, Window, int?>(
                "WindowX",
                null);
        private static void HandleWindowXChanged(Window w, AvaloniaPropertyChangedEventArgs e) {
            OnWindowPositionChanged(w);
        }
        #endregion

        #region WindowY AvaloniaProperty
        public static int? GetWindowY(AvaloniaObject obj) {
            return obj.GetValue(WindowYProperty);
        }

        public static void SetWindowY(AvaloniaObject obj, int? value) {
            obj.SetValue(WindowYProperty, value);
        }

        public static readonly AttachedProperty<int?> WindowYProperty =
            AvaloniaProperty.RegisterAttached<object, Window, int?>(
                "WindowY",
                null);
        private static void HandleWindowYChanged(Window w, AvaloniaPropertyChangedEventArgs e) {
            OnWindowPositionChanged(w);
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

        private static void HandleIsEnabledChanged(Window w, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                w.Opened += W_Opened;
                if (w.IsInitialized) {
                    W_Opened(w, null);
                }
            } else {
                W_Closed(w, null);
            }
        }

        private static void W_Opened(object sender, EventArgs e) {
            if (sender is Window w) {
                w.Closed += W_Closed;
            }
        }

        private static void W_Closed(object sender, EventArgs e) {
            var w = sender as Window;
            if (w == null) {
                return;
            }
            w.Opened -= W_Opened;
            w.Closed -= W_Closed;
        }

        #endregion

        private static void OnWindowPositionChanged(Window w) {
            int new_x = GetWindowX(w).HasValue ? GetWindowX(w).Value : w.Position.X;
            int new_y = GetWindowY(w).HasValue ? GetWindowY(w).Value : w.Position.Y;

            if (w.Position.X != new_x || w.Position.Y != new_y) {
                w.Position = new PixelPoint(new_x, new_y);
            }
        }

        #endregion
    }
}
