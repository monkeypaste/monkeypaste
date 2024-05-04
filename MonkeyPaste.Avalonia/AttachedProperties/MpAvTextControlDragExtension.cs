using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public static class MpAvTextControlDragExtension {
        static MpAvTextControlDragExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region IsDragOverHandled AvaloniaProperty
        public static bool GetIsDragOverHandled(AvaloniaObject obj) {
            return obj.GetValue(IsDragOverHandledProperty);
        }

        public static void SetIsDragOverHandled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsDragOverHandledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsDragOverHandledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsDragOverHandled",
                false,
                false);

        #endregion

        #region IsDropHandled AvaloniaProperty
        public static bool GetIsDropHandled(AvaloniaObject obj) {
            return obj.GetValue(IsDropHandledProperty);
        }

        public static void SetIsDropHandled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsDropHandledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsDropHandledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsDropHandled", true);

        #endregion

        #region IsDragging AvaloniaProperty
        public static bool GetIsDragging(AvaloniaObject obj) {
            return obj.GetValue(IsDraggingProperty);
        }

        public static void SetIsDragging(AvaloniaObject obj, bool value) {
            obj.SetValue(IsDraggingProperty, value);
        }

        public static readonly AttachedProperty<bool> IsDraggingProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsDragging",
                defaultValue: false,
                defaultBindingMode: BindingMode.TwoWay);

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

        private static void HandleIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal) {
                if (isEnabledVal) {
                    control.AttachedToVisualTree += Control_AttachedToVisualTree;
                    if (control.IsAttachedToVisualTree()) {
                        Control_AttachedToVisualTree(control, null);
                    }
                } else {
                    Control_DetachedFromVisualTree(control, null);
                }
            }
        }

        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            control.RemoveHandler(Control.PointerPressedEvent, Control_PointerPressed);
            control.RemoveHandler(Control.PointerMovedEvent, Control_PointerMoved);
            control.DetachedFromVisualTree -= Control_DetachedFromVisualTree;
        }

        private static void Control_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
            control.AddHandler(Control.PointerMovedEvent, Control_PointerMoved, RoutingStrategies.Tunnel);
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }
        private static void Control_PointerMoved(object sender, PointerEventArgs e) {
            if (sender is not Control attached_control ||
                FindTextBox(sender) is not TextBox tb) {
                return;
            }
            if (tb.IsPointInTextBoxSelection(e.GetPosition(tb)) &&
                tb.SelectionLength() > 0 &&
                (!e.IsLeftDown(attached_control) || GetIsDragging(attached_control))) {
                //MpConsole.WriteLine($"cursor OVER sel");
                tb.GetVisualDescendant<TextPresenter>().Cursor = new Cursor(StandardCursorType.SizeAll);
            } else {
                //MpConsole.WriteLine($"cursor NOT over sel");
                tb.GetVisualDescendant<TextPresenter>().Cursor = new Cursor(StandardCursorType.Ibeam);
            }
        }

        private static async void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is not Control attached_control ||
                FindTextBox(sender) is not TextBox tb) {
                return;
            }
            if (tb.SelectionLength() == 0) {
                return;
            }
            if (!tb.IsPointInTextBoxSelection(e.GetPosition(tb))) {
                // only start drag if down is over selection
                return;
            }

            MpAvDataObject avdo = new MpAvDataObject();
            //e.Handled = true;

            if (tb.SelectionLength() > 0) {
                avdo.SetData(MpPortableDataFormats.Text, tb.Text.Substring(tb.LiteralSelectionStart(), tb.SelectionLength()));
            } else {
                avdo.SetData(MpPortableDataFormats.Text, tb.Text);
            }
            SetIsDragging(attached_control, true);
            var result = await MpAvDoDragDropWrapper.DoDragDropAsync(attached_control, e, avdo, DragDropEffects.Copy);
            SetIsDragging(attached_control, false);
        }


        #endregion

        private static TextBox FindTextBox(object obj) {
            if (obj is TextBox tb) {
                return tb;
            }
            if (obj is AutoCompleteBox acb &&
                acb.FindNameScope().Find("PART_TextBox") is TextBox acb_tb) {
                return acb_tb;
            }
            return null;
        }

        #endregion
    }
}
