using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Linq;

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
            if (IsPointInTextBoxSelection(tb, e.GetPosition(tb)) &&
                tb.SelectionLength() > 0 &&
                (!e.IsLeftDown(attached_control) || GetIsDragging(attached_control))) {
                //MpConsole.WriteLine($"cursor OVER sel");
                tb.GetVisualDescendant<TextPresenter>().Cursor = new Cursor(StandardCursorType.SizeAll);
            } else {
                //MpConsole.WriteLine($"cursor NOT over sel");
                tb.GetVisualDescendant<TextPresenter>().Cursor = new Cursor(StandardCursorType.Ibeam);
            }
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is not Control attached_control ||
                FindTextBox(sender) is not TextBox tb) {
                return;
            }
            if (tb.SelectionLength() == 0) {
                return;
            }
            if (!IsPointInTextBoxSelection(tb, e.GetPosition(tb))) {
                // only start drag if down is over selection
                return;
            }

            tb.DragCheckAndStart(e,
                start: async (start_e) => {
                    MpAvDataObject avdo = null;
                    MpAvClipTileViewModel ctvm = tb.GetSelfOrAncestorDataContext<MpAvClipTileViewModel>();
                    if (ctvm != null) {
                        avdo = new MpAvDataObject(ctvm.CopyItem.ToPortableDataObject(true, true));
                        ctvm.IsTileDragging = true;
                    } else {
                        avdo = new MpAvDataObject(MpPortableDataFormats.Text, tb.Text);
                    }

                    if (tb.SelectionLength() > 0) {
                        avdo.SetData(MpPortableDataFormats.Text, tb.Text.Substring(tb.SelectionStart, tb.SelectionLength()));
                    }
                    SetIsDragging(attached_control, true);
                    var result = await DragDrop.DoDragDrop(e, avdo, DragDropEffects.Link | DragDropEffects.Copy);
                    SetIsDragging(attached_control, false);

                    if (ctvm != null) {
                        ctvm.IsTileDragging = false;
                    }
                });
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

        private static bool IsPointInTextBoxSelection(TextBox tb, Point p) {
            int mp_tb_idx = tb.GetTextIndexFromTextBoxPoint(p);
            int actual_start_idx = Math.Min(tb.SelectionStart, tb.SelectionEnd);
            int actual_end_idx = Math.Max(tb.SelectionStart, tb.SelectionEnd);
            if (!(mp_tb_idx >= actual_start_idx && mp_tb_idx <= actual_end_idx)) {
                // press not over selection
                return false;
            }
            return true;
        }
        #endregion
    }
}
