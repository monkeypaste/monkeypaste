using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {

    public static class MpAvBadgeNotificationExtension {
        static MpAvBadgeNotificationExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }
        #region Properties

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
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }

            void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
                if (s is Control control) {
                    if (e == null) {
                        control.AttachedToVisualTree += AttachedToVisualHandler;
                    }
                    control.DetachedFromVisualTree += DetachedToVisualHandler;

                    var adornerLayer = AdornerLayer.GetAdornerLayer(control);
                    if (adornerLayer != null) {
                        var adorner = new MpAvBadgeNotificationAdorner(control);
                        adornerLayer.Children.Add(adorner); 
                        AdornerLayer.SetAdornedElement(adorner, control);
                    }

                    if(control.DataContext is MpIBadgeNotificationViewModel bnvm) {
                        bnvm.PropertyChanged += Bnvm_PropertyChanged;
                    }
                }
            }

            void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
                if (s is Control control) {
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                }
            }

            void Bnvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                if (sender is MpIBadgeNotificationViewModel bnvm) {
                    switch (e.PropertyName) {
                        case nameof(bnvm.HasBadgeNotification):
                            if(element is Control control) {
                                var adornerLayer = AdornerLayer.GetAdornerLayer(control);
                                if(adornerLayer != null) {
                                    var notificationAdorner = adornerLayer.Children.FirstOrDefault(x => x is MpAvBadgeNotificationAdorner);
                                    if(notificationAdorner != default) {
                                        notificationAdorner.InvalidateVisual();
                                    }
                                }
                                
                            }
                            break;
                    }
                }
            }
        }



        #endregion

        #endregion
    }
}
