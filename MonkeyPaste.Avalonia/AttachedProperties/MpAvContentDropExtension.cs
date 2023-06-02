using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using MonkeyPaste.Common;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvContentDropExtension {
        static MpAvContentDropExtension() {
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
                    EnableDnd(control);
                } else {
                    DisableDnd(control);
                }
            }
        }
        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
                DisableDnd(control);
            }
        }

        #endregion

        #endregion

        #region Drop
        private static void EnableDnd(Control control) {
            if (control is not TextBox && control is not AutoCompleteBox) {
                MpDebug.Break("DropExt only supports textbox and autocompletebox");
                return;
            }
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragOverEvent, DragOver);
            control.AddHandler(DragDrop.DropEvent, Drop);
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }

        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragOverEvent, DragOver);
            control.AddHandler(DragDrop.DropEvent, Drop);
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }

        private static void DisableDnd(Control control) {
            DragDrop.SetAllowDrop(control, false);
            control.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            control.RemoveHandler(DragDrop.DropEvent, Drop);
        }
        private static void DragOver(object sender, DragEventArgs e) {
            //e.DragEffects = DragDropEffects.Default;
            if (!e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
            } else {
                // override criteria sorting
                e.Handled = GetIsDragOverHandled(sender as Control);
            }
        }
        private static void Drop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
                return;
            }
            if (sender is TextBox tb) {
                tb.Text = e.Data.Get(MpPortableDataFormats.Text) as string;
            } else if (sender is AutoCompleteBox acb) {
                acb.Text = e.Data.Get(MpPortableDataFormats.Text) as string;
            }
        }

        #endregion
    }
}
