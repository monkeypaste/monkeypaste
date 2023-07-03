using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Linq;
using System.Reactive.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvShowDisabledTooltipExtension {
        #region Constructors
        static MpAvShowDisabledTooltipExtension() {
            ShowOnDisabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleShowOnDisabledChanged(x, y));
        }

        #endregion

        #region Properties

        #region ShowOnDisabled AvaloniaProperty
        public static bool GetShowOnDisabled(AvaloniaObject obj) {
            return obj.GetValue(ShowOnDisabledProperty);
        }

        public static void SetShowOnDisabled(AvaloniaObject obj, bool value) {
            obj.SetValue(ShowOnDisabledProperty, value);
        }

        public static readonly AttachedProperty<bool> ShowOnDisabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "ShowOnDisabled",
                false, false);

        private static void HandleShowOnDisabledChanged(Control control, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                control.DetachedFromVisualTree += AttachedControl_DetachedFromVisualOrExtension;
                control.AttachedToVisualTree += AttachedControl_AttachedToVisualTree;
                if (control.IsInitialized) {
                    // enabled after visual attached
                    AttachedControl_AttachedToVisualTree(control, null);
                }
            } else {
                AttachedControl_DetachedFromVisualOrExtension(control, null);
            }

        }

        private static void AttachedControl_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            var tl = TopLevel.GetTopLevel(control);
            tl.AddHandler(TopLevel.PointerMovedEvent, TopLevel_PointerMoved, RoutingStrategies.Tunnel);
        }


        private static void AttachedControl_DetachedFromVisualOrExtension(object s, VisualTreeAttachmentEventArgs e) {
            if (s is not Control control) {
                return;
            }
            control.DetachedFromVisualTree -= AttachedControl_DetachedFromVisualOrExtension;
            control.AttachedToVisualTree -= AttachedControl_AttachedToVisualTree;
            if (TopLevel.GetTopLevel(control) is not TopLevel tl) {
                return;
            }
            tl.RemoveHandler(TopLevel.PointerMovedEvent, TopLevel_PointerMoved);
        }

        private static void TopLevel_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e) {
            if (sender is not Control tl) {
                return;
            }
            var attached_controls =
                tl.GetVisualDescendants().Where(x => GetShowOnDisabled(x)).Cast<Control>();

            // find disabled children under pointer w/ this extension enabled
            var disabled_child_under_pointer =
                attached_controls
                    .FirstOrDefault(x =>
                        x.Bounds.Contains(e.GetPosition(x.Parent as Visual)) &&
                        !x.IsEnabled);

            if (disabled_child_under_pointer == null) {
                // no disabled children under pointer, clear any tooltips shown w/ this extension
                var disabled_childre_showing_tooltip =
                    attached_controls
                        .Where(x =>
                            ToolTip.GetIsOpen(x) &&
                            !x.IsEnabled);
                foreach (var dcst in disabled_childre_showing_tooltip) {
                    ToolTip.SetIsOpen(dcst, false);
                }
                return;
            }
            // manually show tooltip
            ToolTip.SetIsOpen(disabled_child_under_pointer, true);
        }
        #endregion

        #endregion
    }
}
