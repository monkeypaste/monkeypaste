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

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public static class MpAvMoveExtension {
        #region Private Variables
        #endregion

        #region Constants
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
            control.PointerMoved += Control_PointerMoved;
            control.PointerEntered += Control_PointerEnter;
            control.PointerExited += Control_PointerLeave;

            //if (Control.DataContext is MpIMovableViewModel rvm) {
            //    //var dupCheck = _allMovables.FirstOrDefault(x => x.MovableId == rvm.MovableId);
            //    //if (dupCheck != null) {
            //    //    MpConsole.WriteLine("Duplicate movable detected while loading, swapping for new...");
            //    //    _allMovables.Remove(dupCheck);
            //    //}
            //    _allMovables.Add(rvm);
            //}
        }

        private static void Control_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var control = sender as Control;

            if (control != null) {
                control.DetachedFromVisualTree -= Control_DetachedFromVisualTree;
                control.RemoveHandler(Control.PointerPressedEvent, Control_PointerPressed);
                control.RemoveHandler(Control.PointerReleasedEvent, Control_PointerReleased);
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

        public static void Move(Control Control, double dx, double dy) {
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }
            var adivm = Control.DataContext as MpIBoxViewModel;
            var newLoc = new MpPoint(adivm.X + dx, adivm.Y + dy);
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
                //MpPlatformWrapper.Services.Cursor.UnsetCursor(Control.DataContext);
            }
        }

        private static void Control_PointerEnter(object sender, PointerEventArgs e) {
            if (sender is Control control &&
                !IsAnyMoving && !MpAvMainWindowViewModel.Instance.IsAnyItemDragging) {
                //CanMove = true;
                SetCanMove(control, true);
            }
        }

        private static void Control_PointerMoved(object sender, PointerEventArgs e) {
            if (sender is Control control &&
                control.DataContext is MpAvActionViewModelBase avmb) {
                if (!GetIsMoving(control)) {
                    return;
                }
                if (!e.IsLeftDown(control)) {
                    FinishMove(control);
                }

                Control relativeTo = control.GetVisualAncestor<MpAvDesignerCanvas>(); //MpAvMainView.Instance;
                var mwmp = e.GetPosition(relativeTo).ToPortablePoint();

                MpPoint delta = mwmp - _lastMousePosition;

                // NOTE must transform mouse delta from designer canvas scaling
                //delta.X *= 1 / avmb.RootTriggerActionViewModel.Scale;
                //delta.Y *= 1 / avmb.RootTriggerActionViewModel.Scale;

                Move(control, delta.X, delta.Y);

                _lastMousePosition = mwmp;
            }
        }

        private static void Control_PointerReleased(object sender, PointerReleasedEventArgs e) {
            if (sender is Control control) {
                //e.Pointer.Capture(null);
                FinishMove(control);
            }
        }

        private static void Control_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is Control control) {
                if (MpAvZoomBorder.IsTranslating) {
                    return;
                }
                Control relativeTo = control.GetVisualAncestor<MpAvDesignerCanvas>(); //MpAvMainView.Instance;

                _mouseDownPosition = e.GetPosition(relativeTo).ToPortablePoint();
                _lastMousePosition = _mouseDownPosition;
                //if (GetBeginMoveCommand(control) is ICommand beginMoveCmd) {
                //    beginMoveCmd.Execute(GetBeginMoveCommandParameter(control));
                //} else if (control.DataContext is MpISelectableViewModel svm) {
                //    svm.IsSelected = true;
                //}
                //e.Pointer.Capture(control);
                SetIsMoving(control, true);
                //e.Handled = true;
            }
        }


        private static void FinishMove(Control control) {
            SetIsMoving(control, false);

            if (_lastMousePosition != null &&
                _mouseDownPosition != null &&
                (_lastMousePosition - _mouseDownPosition).Length < 5 &&
                GetFinishMoveCommand(control) is ICommand finishMoveCmd) {
                finishMoveCmd.Execute(GetFinishMoveCommandParameter(control));
            }
        }

        #endregion

        #endregion
    }

}
