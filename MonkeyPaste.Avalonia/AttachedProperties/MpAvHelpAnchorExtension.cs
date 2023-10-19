using Avalonia;
using Avalonia.Controls;

namespace MonkeyPaste.Avalonia {
    public static class MpAvHelpAnchorExtension {
        static MpAvHelpAnchorExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

        #region LinkType AvaloniaProperty
        public static MpHelpLinkType GetLinkType(AvaloniaObject obj) {
            return obj.GetValue(LinkTypeProperty);
        }

        public static void SetLinkType(AvaloniaObject obj, MpHelpLinkType value) {
            obj.SetValue(LinkTypeProperty, value);
        }

        public static readonly AttachedProperty<MpHelpLinkType> LinkTypeProperty =
            AvaloniaProperty.RegisterAttached<object, Control, MpHelpLinkType>(
                "LinkType",
                MpHelpLinkType.None);
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

        private static void HandleIsEnabledChanged(Control tb, AvaloniaPropertyChangedEventArgs e) {
            // just needs to exist
        }



        #endregion

        #endregion
    }
}
