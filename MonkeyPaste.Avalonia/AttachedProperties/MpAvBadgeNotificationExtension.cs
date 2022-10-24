using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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
                    control.AttachedToVisualTree += AttachedToVisualHandler;
                    control.DetachedFromVisualTree += DetachedToVisualHandler;
                    control.DataContextChanged += Control_DataContextChanged;
                    if(control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    }
                }
            } else {

                DetachedToVisualHandler(element, null);
                return;
            }

            void AttachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
                if (s is Control control) {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(control);
                    if (adornerLayer != null) {
                        var adorner = new MpAvBadgeNotificationAdorner(control);
                        adornerLayer.Children.Add(adorner);
                        AdornerLayer.SetAdornedElement(adorner, control);
                    }

                    if (control.DataContext is MpIBadgeNotificationViewModel bnvm) {
                        bnvm.PropertyChanged += Bnvm_PropertyChanged;
                    }
                }
            }

            void Control_DataContextChanged(object sender, System.EventArgs e) {
                if (sender is Control control &&
                    control.DataContext is MpIBadgeNotificationViewModel bnvm) {

                    bnvm.PropertyChanged += Bnvm_PropertyChanged;
                }
            }


            void DetachedToVisualHandler(object s, VisualTreeAttachmentEventArgs e) {
                if (s is Control control) {
                    control.DataContextChanged -= Control_DataContextChanged;
                    control.DetachedFromVisualTree -= DetachedToVisualHandler;
                    control.AttachedToVisualTree -= AttachedToVisualHandler;
                    if (control.DataContext is MpIBadgeNotificationViewModel bnvm) {
                        bnvm.PropertyChanged -= Bnvm_PropertyChanged;
                    }
                }
            }

            void Bnvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
                if (sender is MpIBadgeNotificationViewModel bnvm) {
                    switch (e.PropertyName) {
                        case nameof(bnvm.HasBadgeNotification):
                            if (element is Control control) {
                                var adornerLayer = AdornerLayer.GetAdornerLayer(control);
                                if (adornerLayer == null) {
                                    // where is it?
                                    Debugger.Break();
                                    return;
                                }
                                var notificationAdorners = adornerLayer.Children
                                    .Where(x => x is MpAvBadgeNotificationAdorner bna && bna.AdornedControl == control)
                                    .Cast<MpAvBadgeNotificationAdorner>();

                                if (notificationAdorners == null || notificationAdorners.Count() == 0) {
                                    // shouldn't happen
                                    Debugger.Break();
                                    // add it lazy
                                    //notificationAdorners = new MpAvBadgeNotificationAdorner(control);
                                    //adornerLayer.Children.Add(notificationAdorners);
                                }

                                notificationAdorners.ForEach(x => x.InvalidateAll());
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
