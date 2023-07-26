using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Crypto.Signers;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvTextControlDropExtension {
        static MpAvTextControlDropExtension() {
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
            control.AddHandler(DragDrop.DragOverEvent, DragEnter);
            control.AddHandler(DragDrop.DragOverEvent, DragOver);
            control.AddHandler(DragDrop.DropEvent, Drop);
            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
        }
        private static void DisableDnd(Control control) {
            DragDrop.SetAllowDrop(control, false);
            control.RemoveHandler(DragDrop.DragOverEvent, DragEnter);
            control.RemoveHandler(DragDrop.DragOverEvent, DragOver);
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
            e.DragEffects = GetDropEffects(e);
            if (FindTextBox(sender) is not TextBox tb ||
                e.DragEffects == DragDropEffects.None) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                bool success = await tb.TrySetFocusAsync();
                MpConsole.WriteLine($"DragEnter focus success: {success}");
            });
        }
        private static void DragOver(object sender, DragEventArgs e) {
            e.DragEffects = GetDropEffects(e);
            if (FindTextBox(sender) is not TextBox tb ||
                e.DragEffects == DragDropEffects.None) {
                return;
            }

            // override criteria sorting
            e.Handled = GetIsDragOverHandled(tb);

            UpdateDropPosition(tb, e);
        }
        private static async void Drop(object sender, DragEventArgs e) {
            e.DragEffects = GetDropEffects(e);
            if (sender is not Control c ||
                FindTextBox(c) is not TextBox tb ||
                e.DragEffects == DragDropEffects.None) {
                return;
            }
            e.Handled = GetIsDropHandled(tb);
            var processed_drag_avdo = await Mp.Services
                       .DataObjectHelperAsync.ReadDragDropDataObjectAsync(e.Data) as MpAvDataObject;
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

        private static DragDropEffects GetDropEffects(DragEventArgs e) {
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
        private static void UpdateDropPosition(TextBox tb, DragEventArgs e) {
            var mp = e.GetPosition(tb);
            TextLayout tl = tb.ToTextLayout();
            TextHitTestResult htt = tl.HitTestPoint(mp);
            //if (!htt.IsInside) {
            //    // ignore
            //    return;
            //}
            int caret_idx = htt.TextPosition + (htt.IsTrailing ? 1 : 0);
            MpConsole.WriteLine($"Drop caret idx: {caret_idx}");
            tb.CaretIndex = caret_idx;
            tb.SelectionStart = caret_idx;
            tb.SelectionEnd = caret_idx;
        }

        private static TextBox FindTextBox(object obj) {
            if (obj is not Control c) {
                return null;
            }
            if (c is TextBox tb) {
                return tb;
            }
            if (c is AutoCompleteBox acb &&
                acb.FindNameScope().Find("PART_TextBox") is TextBox acb_tb) {
                return acb_tb;
            }
            return c.GetVisualDescendant<TextBox>();
        }
        #endregion
    }
}
