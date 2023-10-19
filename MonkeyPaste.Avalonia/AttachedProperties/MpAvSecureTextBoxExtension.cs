using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSecureTextBoxExtension {
        static MpAvSecureTextBoxExtension() {
            IsEnabledProperty.Changed.AddClassHandler<TextBox>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region IsClipboardCopyEnabled AvaloniaProperty
        public static bool GetIsClipboardCopyEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsClipboardCopyEnabledProperty);
        }

        public static void SetIsClipboardCopyEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsClipboardCopyEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsClipboardCopyEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, bool>(
                "IsClipboardCopyEnabled",
                false);
        #endregion

        #region IsClipboardCutEnabled AvaloniaProperty
        public static bool GetIsClipboardCutEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsClipboardCutEnabledProperty);
        }

        public static void SetIsClipboardCutEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsClipboardCutEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsClipboardCutEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, bool>(
                "IsClipboardCutEnabled",
                false);
        #endregion

        #region IsContextMenuEnabled AvaloniaProperty
        public static bool GetIsContextMenuEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsContextMenuEnabledProperty);
        }

        public static void SetIsContextMenuEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsContextMenuEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsContextMenuEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, TextBox, bool>(
                "IsContextMenuEnabled",
                false);
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

        private static void HandleIsEnabledChanged(TextBox tb, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                tb.Loaded += Tb_Loaded;
                if (tb.IsLoaded) {
                    Tb_Loaded(tb, null);
                }
            } else {
                Tb_Unloaded(tb, null);
            }
        }

        private static void Tb_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not TextBox tb) {
                return;
            }
            tb.Unloaded += Tb_Unloaded;
            tb.AddHandler(TextBox.PointerPressedEvent, Tb_Reject_Context_PointerPressed_Handler, RoutingStrategies.Tunnel);
            tb.AddHandler(TextBox.CuttingToClipboardEvent, Tb_Reject_Cut_Handler, RoutingStrategies.Tunnel);
            tb.AddHandler(TextBox.CopyingToClipboardEvent, Tb_Reject_Copy_Handler, RoutingStrategies.Tunnel);
            if (tb.ContextMenu != null) {
                // is it non-null? use closing event instead of pointer press
                MpDebug.BreakAll();
                tb.ContextMenu.Opening += Tb_Reject_ContextMenu_Opening_Handler;
            }
        }


        private static void Tb_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not TextBox tb) {
                return;
            }
            tb.Loaded -= Tb_Loaded;
            tb.Unloaded -= Tb_Unloaded;
            tb.RemoveHandler(TextBox.PointerPressedEvent, Tb_Reject_Context_PointerPressed_Handler);
            tb.RemoveHandler(TextBox.CuttingToClipboardEvent, Tb_Reject_Cut_Handler);
            tb.RemoveHandler(TextBox.CopyingToClipboardEvent, Tb_Reject_Copy_Handler);

            if (tb.ContextMenu != null) {
                tb.ContextMenu.Opening -= Tb_Reject_ContextMenu_Opening_Handler;
            }
        }

        #endregion

        private static void Tb_Reject_ContextMenu_Opening_Handler(object sender, System.ComponentModel.CancelEventArgs e) {
            if (sender is not TextBox tb ||
                GetIsContextMenuEnabled(tb)) {
                return;
            }
            e.Cancel = true;
        }
        private static void Tb_Reject_Context_PointerPressed_Handler(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (sender is not TextBox tb ||
                !e.IsRightPress(tb) ||
                GetIsContextMenuEnabled(tb)) {
                return;
            }
            e.Handled = true;
        }

        private static void Tb_Reject_Cut_Handler(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not TextBox tb ||
                GetIsClipboardCutEnabled(tb)) {
                return;
            }
            e.Handled = true;
        }
        private static void Tb_Reject_Copy_Handler(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not TextBox tb ||
                GetIsClipboardCopyEnabled(tb)) {
                return;
            }
            e.Handled = true;
        }

        #endregion
    }
}
