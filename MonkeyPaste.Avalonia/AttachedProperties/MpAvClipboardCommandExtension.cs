using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
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
using System.Windows.Input;
using Key = Avalonia.Input.Key;

namespace MonkeyPaste.Avalonia {
    public static class MpAvClipboardCommandExtension {
        static MpAvClipboardCommandExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #region Properties

        #region CopyCommand AvaloniaProperty
        public static ICommand GetCopyCommand(AvaloniaObject obj) {
            return obj.GetValue(CopyCommandProperty);
        }

        public static void SetCopyCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(CopyCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> CopyCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "CopyCommand",
                null);

        #endregion

        #region CopyCommandParameter AvaloniaProperty
        public static object GetCopyCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(CopyCommandParameterProperty);
        }

        public static void SetCopyCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(CopyCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> CopyCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "CopyCommandParameter",
                null);

        #endregion

        #region CutCommand AvaloniaProperty
        public static ICommand GetCutCommand(AvaloniaObject obj) {
            return obj.GetValue(CutCommandProperty);
        }

        public static void SetCutCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(CutCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> CutCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "CutCommand",
                null);

        #endregion

        #region CutCommandParameter AvaloniaProperty
        public static object GetCutCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(CutCommandParameterProperty);
        }

        public static void SetCutCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(CutCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> CutCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "CutCommandParameter",
                null);

        #endregion

        #region PasteCommand AvaloniaProperty
        public static ICommand GetPasteCommand(AvaloniaObject obj) {
            return obj.GetValue(PasteCommandProperty);
        }

        public static void SetPasteCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(PasteCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> PasteCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "PasteCommand",
                null);

        #endregion

        #region PasteCommandParameter AvaloniaProperty
        public static object GetPasteCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(PasteCommandParameterProperty);
        }

        public static void SetPasteCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(PasteCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object> PasteCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
                "PasteCommandParameter",
                null);

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
            if (sender is not Control control) {
                return;
            }
            control.DetachedFromVisualTree += DetachedFromVisualHandler;
            control.KeyUp += Control_KeyUp;
        }


        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is not Control control) {
                return;
            }
            control.AttachedToVisualTree -= Element_AttachedToVisualTree;
            control.DetachedFromVisualTree -= DetachedFromVisualHandler;
            control.KeyUp -= Control_KeyUp;
        }


        private static void Control_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            if (e.KeyModifiers != KeyModifiers.Control) {
                return;
            }
            if (e.Key == Key.C &&
                GetCopyCommand(control) is ICommand copyCmd) {
                copyCmd.Execute(GetCopyCommandParameter(control));
                return;
            }
            if (e.Key == Key.X &&
                GetCutCommand(control) is ICommand cutCmd) {
                cutCmd.Execute(GetCutCommandParameter(control));
                return;
            }
            if (e.Key == Key.V &&
                GetPasteCommand(control) is ICommand pasteCmd) {
                pasteCmd.Execute(GetPasteCommandParameter(control));
                return;
            }

        }
        #endregion


        #endregion
    }
}
