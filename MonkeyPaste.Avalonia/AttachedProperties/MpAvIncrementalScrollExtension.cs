using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvIncrementalScrollExtension {
        #region Private Variables

        #endregion

        #region Constructors

        static MpAvIncrementalScrollExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion


        #region ScrollOffsetX AvaloniaProperty
        public static double GetScrollOffsetX(AvaloniaObject obj) {
            return obj.GetValue(ScrollOffsetXProperty);
        }

        public static void SetScrollOffsetX(AvaloniaObject obj, double value) {
            obj.SetValue(ScrollOffsetXProperty, value);
        }

        public static readonly AttachedProperty<double> ScrollOffsetXProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ScrollOffsetX",
                0.0d,
                false);

        #endregion

        #region ScrollOffsetY AvaloniaProperty
        public static double GetScrollOffsetY(AvaloniaObject obj) {
            return obj.GetValue(ScrollOffsetYProperty);
        }

        public static void SetScrollOffsetY(AvaloniaObject obj, double value) {
            obj.SetValue(ScrollOffsetYProperty, value);
        }

        public static readonly AttachedProperty<double> ScrollOffsetYProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "ScrollOffsetY",
                0.0d,
                false);

        #endregion

        #region MaxScrollOffsetX AvaloniaProperty
        public static double GetMaxScrollOffsetX(AvaloniaObject obj) {
            return obj.GetValue(MaxScrollOffsetXProperty);
        }

        public static void SetMaxScrollOffsetX(AvaloniaObject obj, double value) {
            obj.SetValue(MaxScrollOffsetXProperty, value);
        }

        public static readonly AttachedProperty<double> MaxScrollOffsetXProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MaxScrollOffsetX",
                0.0d,
                false);

        #endregion

        #region MaxScrollOffsetY AvaloniaProperty
        public static double GetMaxScrollOffsetY(AvaloniaObject obj) {
            return obj.GetValue(MaxScrollOffsetYProperty);
        }

        public static void SetMaxScrollOffsetY(AvaloniaObject obj, double value) {
            obj.SetValue(MaxScrollOffsetYProperty, value);
        }

        public static readonly AttachedProperty<double> MaxScrollOffsetYProperty =
            AvaloniaProperty.RegisterAttached<object, Control, double>(
                "MaxScrollOffsetY",
                0.0d,
                false);

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
                    if (control.IsInitialized) {
                        AttachedToVisualHandler(control, null);
                    } else {
                        control.AttachedToVisualTree += AttachedToVisualHandler;

                    }
                }
            } else {
                DetachedToVisualHandler(element, null);
            }

            void AttachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is ScrollViewer sv) {
                    Dispatcher.UIThread.Post(async () => {
                        var tracks = sv.GetVisualDescendants<Track>();
                        while (tracks.Count() == 0) {
                            tracks = sv.GetVisualDescendants<Track>();
                            await Task.Delay(100);
                        }

                        foreach (var track in tracks) {
                            track.IsThumbDragHandled = false;
                            // MpDebuggerHelper.Break();
                            track.Bind(
                                    Track.ValueProperty,
                                    new Binding() {
                                        Source = sv.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(MpAvClipTrayViewModel.Instance.ScrollOffsetX) :
                                                nameof(MpAvClipTrayViewModel.Instance.ScrollOffsetY),
                                        //Mode = BindingMode.TwoWay,
                                        Priority = BindingPriority.Style
                                    });
                            //MpDebuggerHelper.Break();

                            track.Bind(
                                    Track.MaximumProperty,
                                    new Binding() {
                                        Source = sv.DataContext,
                                        Path = track.Orientation == Orientation.Horizontal ?
                                                nameof(MpAvClipTrayViewModel.Instance.MaxScrollOffsetX) :
                                                nameof(MpAvClipTrayViewModel.Instance.MaxScrollOffsetY),
                                        Mode = BindingMode.TwoWay
                                    });

                            track.Minimum = 0;

                        }
                    });
                }
            }

            void DetachedToVisualHandler(object? s, VisualTreeAttachmentEventArgs? e) {
                if (s is ScrollViewer sv) {
                    sv.AttachedToVisualTree -= AttachedToVisualHandler;
                    sv.DetachedFromVisualTree -= DetachedToVisualHandler;
                }
            }
        }

        #endregion
    }

}
