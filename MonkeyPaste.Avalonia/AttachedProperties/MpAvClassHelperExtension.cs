using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public static class MpAvClassHelperExtension {
        #region Private Variables

        #endregion

        #region Statics
        static MpAvClassHelperExtension() {
            ClassesToAddProperty.Changed.AddClassHandler<Control>((x, y) => HandleChange(x, y, true));
            ClassesToRemoveProperty.Changed.AddClassHandler<Control>((x, y) => HandleChange(x, y, false));
            ClassesToSetProperty.Changed.AddClassHandler<Control>((x, y) => HandleChange(x, y, null));
        }

        #endregion

        #region Properties

        #region ClassesToAdd AvaloniaProperty
        public static string GetClassesToAdd(AvaloniaObject obj) {
            return obj.GetValue(ClassesToAddProperty);
        }

        public static void SetClassesToAdd(AvaloniaObject obj, string value) {
            obj.SetValue(ClassesToAddProperty, value);
        }

        public static readonly AttachedProperty<string> ClassesToAddProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "ClassesToAdd",
                string.Empty);
        #endregion

        #region ClassesToRemove AvaloniaProperty
        public static string GetClassesToRemove(AvaloniaObject obj) {
            return obj.GetValue(ClassesToRemoveProperty);
        }

        public static void SetClassesToRemove(AvaloniaObject obj, string value) {
            obj.SetValue(ClassesToRemoveProperty, value);
        }

        public static readonly AttachedProperty<string> ClassesToRemoveProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "ClassesToRemove",
                string.Empty);
        #endregion

        #region ClassesToSet AvaloniaProperty
        public static string GetClassesToSet(AvaloniaObject obj) {
            return obj.GetValue(ClassesToSetProperty);
        }

        public static void SetClassesToSet(AvaloniaObject obj, string value) {
            obj.SetValue(ClassesToSetProperty, value);
        }

        public static readonly AttachedProperty<string> ClassesToSetProperty =
            AvaloniaProperty.RegisterAttached<object, Control, string>(
                "ClassesToSet",
                string.Empty);
        #endregion

        #endregion

        #region Helpers
        private static void HandleChange(Control attached_control, AvaloniaPropertyChangedEventArgs e, bool? is_add) {
            if (e.NewValue.ToStringOrEmpty().SplitNoEmpty(" ") is not { } new_classes) {
                return;
            }
            string op = is_add.IsTrue() ? "ADDED" : is_add.IsFalse() ? "REMOVED" : "SET";
            if (is_add.IsNull()) {
                // ClassesToSet
                attached_control.Classes.Clear();
            }
            foreach (string new_class in new_classes) {
                if (is_add.IsTrueOrNull()) {
                    // ClassesToAdd or ClassesToSet
                    attached_control.Classes.Add(new_class);
                } else if (is_add.IsFalse()) {
                    // ClassesToRemove
                    attached_control.Classes.Remove(new_class);
                }
                //MpConsole.WriteLine($"Mutable Class '{new_class}' {op} to '{attached_control}'");
            }
        }

        #endregion
    }
}
