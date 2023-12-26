using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDropExtension {
        static MpAvDropExtension() {
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
                true);

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

        #region DragEnterCommand AvaloniaProperty
        public static ICommand GetDragEnterCommand(AvaloniaObject obj) {
            return obj.GetValue(DragEnterCommandProperty);
        }

        public static void SetDragEnterCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DragEnterCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DragEnterCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DragEnterCommand",
                null,
                false);

        #endregion

        #region DragEnterCommandParameter AvaloniaProperty
        public static object GetDragEnterCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DragEnterCommandParameterProperty);
        }

        public static void SetDragEnterCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DragEnterCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DragEnterCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DragEnterCommandParameter",
                null,
                false);

        #endregion

        #region DragLeaveCommand AvaloniaProperty
        public static ICommand GetDragLeaveCommand(AvaloniaObject obj) {
            return obj.GetValue(DragLeaveCommandProperty);
        }

        public static void SetDragLeaveCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DragLeaveCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DragLeaveCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DragLeaveCommand",
                null,
                false);

        #endregion

        #region DragLeaveCommandParameter AvaloniaProperty
        public static object GetDragLeaveCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(DragLeaveCommandParameterProperty);
        }

        public static void SetDragLeaveCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(DragLeaveCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> DragLeaveCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "DragLeaveCommandParameter",
                null,
                false);

        #endregion

        #region DropCommand AvaloniaProperty
        public static ICommand GetDropCommand(AvaloniaObject obj) {
            return obj.GetValue(DropCommandProperty);
        }

        public static void SetDropCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(DropCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> DropCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "DropCommand", null);

        #endregion


        #region DropEffects AvaloniaProperty
        public static DragDropEffects GetDropEffects(AvaloniaObject obj) {
            return obj.GetValue(DropEffectsProperty);
        }

        public static void SetDropEffects(AvaloniaObject obj, DragDropEffects value) {
            obj.SetValue(DropEffectsProperty, value);
        }

        public static readonly AttachedProperty<DragDropEffects> DropEffectsProperty =
            AvaloniaProperty.RegisterAttached<object, Control, DragDropEffects>(
                "DropEffects", DragDropEffects.None);

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
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            control.AddHandler(DragDrop.DragOverEvent, DragOver);
            control.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            control.AddHandler(DragDrop.DropEvent, Drop);
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }
        private static void DisableDnd(Control control) {
            DragDrop.SetAllowDrop(control, false);
            control.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
            control.RemoveHandler(DragDrop.DragOverEvent, DragOver);
            control.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
            control.RemoveHandler(DragDrop.DropEvent, Drop);
        }

        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;
            if (control == null) {
                return;
            }
            DisableDnd(control);
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }


        private static void DragEnter(object sender, DragEventArgs e) {
            if (sender is not Control c) {
                return;
            }

            if (GetDragEnterCommand(c) is ICommand cmd) {
                cmd.Execute(GetDragEnterCommandParameter(c));
            }
            e.DragEffects = e.DragEffects & GetAttachedControlDropEffects(c, e);
            if (FindTextBox(c) is not TextBox tb) {
                return;
            }
            if (e.DragEffects == DragDropEffects.None) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                bool success = await tb.TrySetFocusAsync();
                MpConsole.WriteLine($"DragEnter focus success: {success}");
            });
        }
        private static void DragOver(object sender, DragEventArgs e) {
            if (sender is not Control c) {
                return;
            }

            e.DragEffects = e.DragEffects & GetAttachedControlDropEffects(c, e);
            if (e.DragEffects == DragDropEffects.None) {
                return;
            }

            // override criteria sorting
            if (FindTextBox(c) is not TextBox tb) {
                return;
            }
            e.Handled = GetIsDragOverHandled(tb);

            UpdateTextControlDropPosition(tb, e);
        }
        private static void DragLeave(object sender, DragEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            if (GetDragLeaveCommand(c) is ICommand cmd) {
                cmd.Execute(GetDragLeaveCommandParameter(c));
            }
        }
        private static async void Drop(object sender, DragEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            if (FindTextBox(c) is not TextBox tb) {
                return;
            }
            e.DragEffects = GetTextControlDropEffects(c, e);
            if (e.DragEffects == DragDropEffects.None) {
                return;
            }
            e.Handled = GetIsDropHandled(tb);
            var processed_drag_avdo = await Mp.Services
                       .DataObjectTools.ReadDragDropDataObjectAsync(e.Data) as MpAvDataObject;
            Dispatcher.UIThread.Post(() => {
                string drop_text = processed_drag_avdo.GetData(MpPortableDataFormats.Text) as string;
                int drop_idx = tb.CaretIndex;
                tb.Text =
                    tb.Text.Substring(0, drop_idx) +
                    drop_text +
                    tb.Text.Substring(drop_idx);

                tb.SelectionStart = drop_idx;
                tb.SelectionEnd = drop_idx + drop_text.Length;

                if (GetDropCommand(c) is ICommand drop_cmd) {
                    drop_cmd.Execute(new object[] { MpTransactionType.Dropped, processed_drag_avdo });
                }
            });
        }

        #endregion

        #region Drop Helpers

        private static DragDropEffects GetAttachedControlDropEffects(Control attached_control, DragEventArgs e) {
            if (GetDropEffects(attached_control) is DragDropEffects dde &&
                dde != DragDropEffects.None) {
                return dde;
            }
            return GetTextControlDropEffects(attached_control, e);
        }
        private static DragDropEffects GetTextControlDropEffects(Control attached_control, DragEventArgs e) {
            if (MpAvTextControlDragExtension.GetIsDragging(attached_control)) {
                return DragDropEffects.None;
            }
            DragDropEffects dde = DragDropEffects.None;
            if (e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                    dde |= DragDropEffects.Copy;
                } else {
                    dde |= DragDropEffects.Move;
                }
            }
            return dde;
        }
        private static void UpdateTextControlDropPosition(TextBox tb, DragEventArgs e) {
            var mp = e.GetPosition(tb);
            int mp_caret_idx = tb.GetTextIndexFromTextBoxPoint(mp);
            MpConsole.WriteLine($"Drop caret idx: {mp_caret_idx}");
            tb.CaretIndex = mp_caret_idx;
            tb.SelectionStart = mp_caret_idx;
            tb.SelectionEnd = mp_caret_idx;
        }

        private static TextBox FindTextBox(object obj) {
            if (obj is not Control c) {
                return null;
            }
            if (c is TextBox tb) {
                return tb;
            }
            //if (c is MpAvAutoCompleteBox acb &&
            //    acb.TextBox is TextBox acb_tb) {
            //    return acb_tb;
            //}
            if (c is AutoCompleteBox acb &&
                acb.GetVisualDescendant<TextBox>() is TextBox acb_tb) {
                return acb_tb;
            }
            return c.GetVisualDescendant<TextBox>();
        }
        #endregion

        #region Drop Extensions
        public static DragDropEffects ToValidDropEffect(this DragEventArgs e) {
            return e.DragEffects & (e.KeyModifiers.HasFlag(KeyModifiers.Control) ? DragDropEffects.Copy : e.KeyModifiers.HasFlag(KeyModifiers.Alt) ? DragDropEffects.Link : DragDropEffects.Move);
        }
        #endregion
    }
}
