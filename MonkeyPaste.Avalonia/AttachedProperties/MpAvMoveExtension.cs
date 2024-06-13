using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Windows.Input;
using Key = Avalonia.Input.Key;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public static class MpAvMoveExtension {
        #region Private Variables
        #endregion

        #region Constants
        const double MIN_MOVE_DIST = 5d;
        #endregion

        #region Statics
        static MpAvMoveExtension() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
            IsMovingProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsMovingChanged(x, y));
        }

        #endregion

        #region Properties
        public static bool IsAnyMoving { get; private set; }
        #endregion

        #region Private Variables
        private static MpPoint _lastMousePosition;

        private static MpPoint _mouseDownPosition;

        #endregion

        #region Properties

        #region BeginMoveCommand Property
        public static ICommand GetBeginMoveCommand(AvaloniaObject obj) {
            return obj.GetValue(BeginMoveCommandProperty);
        }

        public static void SetBeginMoveCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(BeginMoveCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand>
            BeginMoveCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
            "BeginMoveCommand",
            null);

        #endregion

        #region BeginMoveCommandParameter Property
        public static object GetBeginMoveCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(BeginMoveCommandParameterProperty);
        }

        public static void SetBeginMoveCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(BeginMoveCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object>
            BeginMoveCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
            "BeginMoveCommandParameter",
            null);

        #endregion

        #region FinishMoveCommand Property
        public static ICommand GetFinishMoveCommand(AvaloniaObject obj) {
            return obj.GetValue(FinishMoveCommandProperty);
        }

        public static void SetFinishMoveCommand(AvaloniaObject obj, ICommand value) {
            obj.SetValue(FinishMoveCommandProperty, value);
        }

        public static readonly AttachedProperty<ICommand>
            FinishMoveCommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
            "FinishMoveCommand",
            null);

        #endregion

        #region FinishMoveCommandParameter Property
        public static object GetFinishMoveCommandParameter(AvaloniaObject obj) {
            return obj.GetValue(FinishMoveCommandParameterProperty);
        }

        public static void SetFinishMoveCommandParameter(AvaloniaObject obj, object value) {
            obj.SetValue(FinishMoveCommandParameterProperty, value);
        }

        public static readonly AttachedProperty<object>
            FinishMoveCommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
            "FinishMoveCommandParameter",
            null);

        #endregion

        #region IsMoving Property
        public static bool GetIsMoving(AvaloniaObject obj) {
            return obj.GetValue(IsMovingProperty);
        }

        public static void SetIsMoving(AvaloniaObject obj, bool value) {
            obj.SetValue(IsMovingProperty, value);
        }

        public static readonly AttachedProperty<bool>
            IsMovingProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "IsMoving",
            false,
            false,
            BindingMode.TwoWay);

        private static void HandleIsMovingChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isResizing) {
                IsAnyMoving = isResizing;
            }
        }
        #endregion

        #region CanMove Property
        public static bool GetCanMove(AvaloniaObject obj) {
            return obj.GetValue(CanMoveProperty);
        }

        public static void SetCanMove(AvaloniaObject obj, bool value) {
            obj.SetValue(CanMoveProperty, value);
        }

        public static readonly AttachedProperty<bool>
            CanMoveProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "CanMove",
            true,
            false, BindingMode.TwoWay);

        #endregion

        #region RelativeTo Property
        public static Visual GetRelativeTo(AvaloniaObject obj) {
            return obj.GetValue(RelativeToProperty);
        }

        public static void SetRelativeTo(AvaloniaObject obj, Visual value) {
            obj.SetValue(RelativeToProperty, value);
        }

        public static readonly AttachedProperty<Visual>
            RelativeToProperty =
            AvaloniaProperty.RegisterAttached<object, Control, Visual>(
            "RelativeTo",
            null,
            false, BindingMode.TwoWay);

        #endregion

        #region IsEnabled Property
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool>
            IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "IsEnabled",
            false);
        private static void HandleIsEnabledChanged(Control element, AvaloniaPropertyChangedEventArgs e) {
            if (e.NewValue is bool isEnabledVal && isEnabledVal) {
                if (element is Control control) {
                    if (control.IsInitialized) {
                        Control_AttachedToVisualTree(control, null);
                    } else {
                        control.AttachedToVisualTree += Control_AttachedToVisualTree;

                    }
                }
            } else {
                Control_DetachedFromVisualTree(element, null);
            }


        }

        private static void Control_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;

            if (control == null) {
                return;
            }

            control.DetachedFromVisualTree += Control_DetachedFromVisualTree;
            control.AddHandler(Control.PointerPressedEvent, Control_PointerPressed, RoutingStrategies.Tunnel);
            control.AddHandler(Control.PointerReleasedEvent, Control_PointerReleased, RoutingStrategies.Tunnel);
            control.AddHandler(Control.KeyDownEvent, Control_KeyDown, RoutingStrategies.Tunnel);
            control.AddHandler(Control.KeyUpEvent, Control_KeyUp, RoutingStrategies.Tunnel);

            control.PointerMoved += Control_PointerMoved;
            control.PointerEntered += Control_PointerEnter;
            control.PointerExited += Control_PointerLeave;
        }



        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;

            if (control != null) {
                control.DetachedFromVisualTree -= Control_DetachedFromVisualTree;
                control.RemoveHandler(Control.PointerPressedEvent, Control_PointerPressed);
                control.RemoveHandler(Control.PointerReleasedEvent, Control_PointerReleased);
                control.RemoveHandler(Control.KeyDownEvent, Control_KeyDown);
                control.RemoveHandler(Control.KeyUpEvent, Control_KeyUp);
                control.PointerMoved -= Control_PointerMoved;
                control.PointerEntered -= Control_PointerEnter;
                control.PointerExited -= Control_PointerLeave;
            }
        }

        #endregion

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public static void Move(Control control, double dx, double dy) {
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }
            if (control.DataContext is not MpIBoxViewModel adivm ||
                control.DataContext is not MpAvViewModelBase vmb ||
                vmb.ParentObj is not MpIDesignerSettingsViewModel dsvm) {
                return;
            }

            // NOTE must transform mouse delta from designer canvas scaling
            //delta.X *= 1 / avmb.Parent.Scale;
            //delta.Y *= 1 / avmb.Parent.Scale;
            var delta = new MpPoint(dx, dy);
            delta /= dsvm.ZoomFactor;
            var newLoc = new MpPoint(adivm.X, adivm.Y) + delta;
            adivm.X = newLoc.X;
            adivm.Y = newLoc.Y;

            //MpConsole.WriteLine($"New Location: {adivm.X} {adivm.Y}");
        }

        #endregion

        #region Private Methods

        #region Move Event Handlers

        private static void Control_PointerLeave(object sender, PointerEventArgs e) {
            if (sender is Control control) {
                if (!GetIsMoving(control)) {
                    SetCanMove(control, false);
                }
            }
        }

        private static void Control_PointerEnter(object sender, PointerEventArgs e) {
            if (sender is Control control &&
                !IsAnyMoving &&
                !e.IsLeftDown(control)
                    //!MpAvMainWindowViewModel.Instance.IsAnyItemDragging
                    ) {
                //CanMove = true;
                SetCanMove(control, true);
            }
        }

        private static void Control_PointerMoved(object sender, PointerEventArgs e) {
            if (sender is not Control control ||
                control.DataContext is not MpAvActionViewModelBase avmb) {
                return;                
            }
            if (_lastMousePosition == null ||
                    _mouseDownPosition == null) {
                // pointer not pressed, ignore
                return;
            }
            Visual relativeTo = GetRelativeTo(control) ?? control;
            var mw_mp = e.GetPosition(relativeTo).ToPortablePoint();
            MpPoint delta = mw_mp - _lastMousePosition;
            _lastMousePosition = mw_mp;
            if (GetIsMoving(control)) {
                if (!e.IsLeftDown(control)) {
                    FinishMove(control);
                    return;
                }
                Move(control, delta.X, delta.Y);
                return;
            }
            if (mw_mp.Distance(_mouseDownPosition) >= MIN_MOVE_DIST) {
                // NOTE only set as moving once its actually moving (to allow hold to execute)
                SetIsMoving(control, true);
                // bring position tracking to current state (will be next move signal)
                _lastMousePosition = _mouseDownPosition;
            }
        }

        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            if (sender is not Control control) {
                return;                
            }
            e.Pointer.Capture(null);
            FinishMove(control);
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is not Control control ||
                MpAvZoomBorder.IsTranslating) {
                return;
            }

            Visual relativeTo = GetRelativeTo(control) ?? control;

            _mouseDownPosition = e.GetPosition(relativeTo).ToPortablePoint();
            _lastMousePosition = _mouseDownPosition;
            if (GetBeginMoveCommand(control) is ICommand beginMoveCmd) {
                beginMoveCmd.Execute(GetBeginMoveCommandParameter(control));
            } else if (control.DataContext is MpISelectableViewModel svm) {
                svm.IsSelected = true;
            }
            e.Pointer.Capture(control);
            SetIsMoving(control, true);
            e.Handled = true;
        }

        private static void Control_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (sender is not Control control) {
                return;
            }
            double multiplier = 5.0d;
            if (e.KeyModifiers == KeyModifiers.Shift || e.KeyModifiers == KeyModifiers.Control) {
                multiplier = 1.0d;
            } else if (e.KeyModifiers != KeyModifiers.None) {
                // ignore if non-shift mod key
                return;
            }
            MpPoint delta = null;
            if (e.Key == Key.Left) {
                delta = new MpPoint(-1, 0);
            } else if (e.Key == Key.Right) {
                delta = new MpPoint(1, 0);
            } else if (e.Key == Key.Up) {
                delta = new MpPoint(0, -1);
            } else if (e.Key == Key.Down) {
                delta = new MpPoint(0, 1);
            }
            if (delta == null) {
                return;
            }
            delta *= multiplier;
            SetIsMoving(control, true);
            Move(control, delta.X, delta.Y);
            e.Handled = true;
        }
        private static void Control_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (sender is not Control control ||
                !GetIsMoving(control)) {
                return;
            }
            FinishMove(control);
        }

        private static void FinishMove(Control control) {
            SetIsMoving(control, false);

            if (_lastMousePosition != null &&
                _mouseDownPosition != null &&
                (_lastMousePosition - _mouseDownPosition).Length >= 5 &&
                GetFinishMoveCommand(control) is ICommand finishMoveCmd) {
                finishMoveCmd.Execute(GetFinishMoveCommandParameter(control));
            }
            _lastMousePosition = null;
            _mouseDownPosition = null;
        }

        #endregion

        #endregion
    }

}
