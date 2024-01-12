using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Avalonia {
    public static class MpAvWindowPositionExtension {
        static MpAvWindowPositionExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Window>((x, y) => HandleIsEnabledChanged(x, y));
            WindowPositionProperty.Changed.AddClassHandler<Window>((x, y) => HandleWindowPositionChanged(x, y));
        }
        #region Properties


        #region WindowPosition AvaloniaProperty
        public static PixelPoint GetWindowPosition(AvaloniaObject obj) {
            return obj.GetValue(WindowPositionProperty);
        }

        public static void SetWindowPosition(AvaloniaObject obj, PixelPoint value) {
            obj.SetValue(WindowPositionProperty, value);
        }

        public static readonly AttachedProperty<PixelPoint> WindowPositionProperty =
            AvaloniaProperty.RegisterAttached<object, Window, PixelPoint>(
                "WindowPosition",
                default);
        private static void HandleWindowPositionChanged(Window w, AvaloniaPropertyChangedEventArgs e) {
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
                w.PositionChanged += W_PositionChanged;
                w.Opened += W_Opened;
                if (w.IsInitialized) {
                    W_Opened(w, null);
                }
            } else {
                W_Closed(w, null);
            }
        }

        private static void W_PositionChanged(object sender, PixelPointEventArgs e) {
            if (sender is not Window w) {
                return;
            }
            //SetWindowX(w, w.Position.X);
            //SetWindowPosition(w, w.Position);
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
            w.PositionChanged -= W_PositionChanged;
        }

        #endregion

        private static void OnWindowPositionChanged(Window w) {
            w.Position = GetWindowPosition(w);
            MpConsole.WriteLine($"Window position changed to {w.Position}");
        }

        #endregion
    }
}
