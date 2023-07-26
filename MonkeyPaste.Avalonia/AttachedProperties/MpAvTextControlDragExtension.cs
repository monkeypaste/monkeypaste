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
                    control.PointerPressed += Control_PointerPressed;
                } else {
                    control.PointerPressed -= Control_PointerPressed;
                }
            }
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (FindTextBox(sender) is not TextBox tb) {
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
                    var result = await DragDrop.DoDragDrop(e, avdo, DragDropEffects.Link | DragDropEffects.Copy);

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
        #endregion
    }
}
