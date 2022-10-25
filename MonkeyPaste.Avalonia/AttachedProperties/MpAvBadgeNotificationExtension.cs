using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;

namespace MonkeyPaste.Avalonia {

    public static class MpAvBadgeNotificationExtension {
        static MpAvBadgeNotificationExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            NotificationCountProperty.Changed.AddClassHandler<Control>((x, y) => HandleNotificationCountChanged(x, y));
        }
        #region Properties

        #region NotificationCount AvaloniaProperty
        public static int GetNotificationCount(AvaloniaObject obj) {
            return obj.GetValue(NotificationCountProperty);
        }

        public static void SetNotificationCount(AvaloniaObject obj, int value) {
            obj.SetValue(NotificationCountProperty, value);
        }

        public static readonly AttachedProperty<int> NotificationCountProperty =
            AvaloniaProperty.RegisterAttached<object, Control, int>(
                "NotificationCount",
                0,
                false);

        private static void HandleNotificationCountChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is Control control && 
                e.NewValue is int notificationCount) {
                Dispatcher.UIThread.Post(async () => {
                    var badge_notifiers = (control as Panel).Children.Where(x => x is MpAvBadgeNotificationAdorner).Cast<MpAvBadgeNotificationAdorner>(); //await control.GetControlAdornersAsync();
                    //var badge_notifiers = notifiers.Where(x => x is MpAvBadgeNotificationAdorner).Cast<MpAvBadgeNotificationAdorner>();
                    foreach (var bna in badge_notifiers) {
                        bna.Draw();
                    }
                });
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

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                    control.DetachedFromVisualTree += DetachedToVisualHandler;
                    if(control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
                return;
            }

            
        }
        private static void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                var adorner = new MpAvBadgeNotificationAdorner(control);
                //control.AddOrReplaceAdornerAsync(adorner).FireAndForgetSafeAsync();
                if(control is Panel p) {
                    p.Children.Add(adorner);
                } else {
                    Debugger.Break();
                }
            }
        }

        private static void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
            if (s is Control control) {
                control.DetachedFromVisualTree -= DetachedToVisualHandler;
                control.AttachedToVisualTree -= AttachedToVisualHandler;
            }
        }
        #endregion

        #endregion
    }
}
