using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSelectionChangedCommandExtension {
        static MpAvSelectionChangedCommandExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region SelectionChangedCommand AvaloniaProperty
        public static ICommand GetSelectionChangedCommand(AvaloniaObject obj) {
            return obj.GetValue(SelectionChangedCommandProperty);
        }

        public static void SetSelectionChangedCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(SelectionChangedCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand> SelectionChangedCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
                "SelectionChangedCommand", null);

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
                    control.AttachedToLogicalTree += Control_AttachedToLogicalTree;
                    control.DetachedFromLogicalTree += Control_DetachedFromLogicalTree;
                    Control_AttachedToLogicalTree(control, null);

                } else {
                    Control_DetachedFromLogicalTree(control, null);
                }
            }
        }

        private static void Control_DetachedFromLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (sender is Control control) {
                if (control is SelectingItemsControl sic) {
                    sic.SelectionChanged -= Sic_SelectionChanged;
                }
                if (control is TreeView tv) {
                    tv.SelectionChanged -= Tv_SelectionChanged;
                }
            }
        }

        private static void Control_AttachedToLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (sender is Control control) {
                if (control is SelectingItemsControl sic) {
                    sic.SelectionChanged += Sic_SelectionChanged;
                }
                if (control is TreeView tv) {
                    tv.SelectionChanged += Tv_SelectionChanged;
                }
            }
        }

        private static void Tv_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is Control c &&
                GetSelectionChangedCommand(c) is ICommand cmd) {
                cmd.Execute(c);
            }
        }

        private static void Sic_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is Control c &&
                GetSelectionChangedCommand(c) is ICommand cmd) {
                cmd.Execute(c);
            }
        }

        private static void DetachedFromVisualHandler(object s, VisualTreeAttachmentEventArgs? e) {
            if (s is Control control) {
            }
        }

        #endregion

        #endregion
    }
}
