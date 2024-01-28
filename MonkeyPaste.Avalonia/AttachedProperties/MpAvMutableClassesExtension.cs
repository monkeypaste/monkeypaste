using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMutableClassesExtension {
        #region Private Variables

        #endregion

        #region Statics
        static MpAvMutableClassesExtension() {
            MutableClassesProperty.Changed.AddClassHandler<Control>((x, y) => HandleMutableClassesChanged(x, y));
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region MutableClasses AvaloniaProperty
        public static string GetMutableClasses(AvaloniaObject obj) {
            return obj.GetValue(MutableClassesProperty);
        }

        public static void SetMutableClasses(AvaloniaObject obj, string value) {
            obj.SetValue(MutableClassesProperty, value);
        }

        public static readonly AttachedProperty<string> MutableClassesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "MutableClasses",
                string.Empty);
        private static void HandleMutableClassesChanged(Control attached_control, AvaloniaPropertyChangedEventArgs e) {
            if (!GetIsEnabled(attached_control) ||
                e.OldValue.ToStringOrEmpty().SplitNoEmpty(" ") is not { } old_classes ||
                    e.NewValue.ToStringOrEmpty().SplitNoEmpty(" ") is not { } new_classes) {
                return;
            }
            foreach (string old_class in old_classes) {
                attached_control.Classes.Add(old_class);
                MpConsole.WriteLine($"Mutable Class '{old_class}' REMOVED to '{attached_control}'");
            }
            foreach (string new_class in new_classes) {
                attached_control.Classes.Add(new_class);
                MpConsole.WriteLine($"Mutable Class '{new_class}' ADDED to '{attached_control}'");
            }
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
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is not bool isEnabledVal ||
                element is not Control attached_control) {
                return;
            }
        }


        #endregion


        #endregion
    }
}
