using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public static class MpAvMutableClassesExtension {
        #region Private Variables

        #endregion

        #region Statics
        static MpAvMutableClassesExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region MutableClasses AvaloniaProperty
        public static Classes GetMutableClasses(AvaloniaObject obj) {
            return obj.GetValue(MutableClassesProperty);
        }

        public static void SetMutableClasses(AvaloniaObject obj, Classes value) {
            obj.SetValue(MutableClassesProperty, value);
        }

        public static readonly AttachedProperty<Classes> MutableClassesProperty =
            AvaloniaProperty.RegisterAttached<object, Control, Classes>(
                "MutableClasses",
                new Classes());
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
                element is not Control attached_control ||
                GetMutableClasses(attached_control) is not Classes mutable_classes) {
                return;
            }

            void Mutable_classes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                if (GetMutableClasses(attached_control) is not Classes mutable_classes) {
                    return;
                }
                if (e.NewItems != null) {
                    foreach (string new_class in e.NewItems) {
                        attached_control.Classes.Add(new_class);
                        MpConsole.WriteLine($"Mutable Class '{new_class}' ADDED to '{attached_control}'");
                    }
                }
                if (e.OldItems != null) {
                    foreach (string old_class in e.OldItems) {
                        attached_control.Classes.Add(old_class);
                        MpConsole.WriteLine($"Mutable Class '{old_class}' REMOVED to '{attached_control}'");
                    }
                }
            }

            void AttachedControl_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
                if (sender is not Control w ||
                    GetMutableClasses(w) is not Classes mutable_classes) {
                    return;
                }
                mutable_classes.CollectionChanged -= Mutable_classes_CollectionChanged;
                w.Unloaded -= AttachedControl_Unloaded;
            }

            if (isEnabledVal) {
                mutable_classes.CollectionChanged += Mutable_classes_CollectionChanged;
                attached_control.Unloaded += AttachedControl_Unloaded;
            } else {
                mutable_classes.CollectionChanged -= Mutable_classes_CollectionChanged;
                attached_control.Unloaded -= AttachedControl_Unloaded;
            }
        }


        #endregion


        #endregion
    }
}
