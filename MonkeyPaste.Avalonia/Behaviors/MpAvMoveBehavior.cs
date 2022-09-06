using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {

    public class MpAvMoveBehavior : MpAvBehavior<Control> {
        #region Private Variables

        //private static List<MpIMovableViewModel> _allMovables = new List<MpIMovableViewModel>();

        //public static bool IsAnyMoving => _allMovables.Any(x => x.IsMoving);
        //public static bool CanAnyMove => _allMovables.Any(x => x.CanMove);


        public static bool IsAnyMoving { get; private set; }// => Application.Current.MainWindow.GetVisualDescendents<MpAvMoveBehavior>().Any(x => x.IsMoving);
        public static bool CanAnyMove { get; private set; }// => Application.Current.MainWindow.GetVisualDescendents<MpAvMoveBehavior>().Any(x => x.CanMove);

        private MpPoint _lastMousePosition;

        private MpPoint _mouseDownPosition;

        #endregion

        #region Properties

        #region Command Property

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly AttachedProperty<ICommand>
            CommandProperty =
            AvaloniaProperty.RegisterAttached<object, Control, ICommand>(
            "Command",
            null,
            false);

        #endregion

        #region CommandParameter Property
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public static readonly AttachedProperty<object>
            CommandParameterProperty =
            AvaloniaProperty.RegisterAttached<object, Control, object>(
            "CommandParameter",
            null,
            false);

        #endregion

        #region IsMoving Property
        public bool IsMoving {
            get { return (bool)GetValue(IsMovingProperty); }
            set { SetValue(IsMovingProperty, value); }
        }

        public static readonly AttachedProperty<bool>
            IsMovingProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "IsMoving",
            false,
            false,
            BindingMode.TwoWay);

        #endregion

        #region CanMove Property
        public bool CanMove {
            get { return (bool)GetValue(CanMoveProperty); }
            set { SetValue(CanMoveProperty, value); }
        }

        public static readonly AttachedProperty<bool>
            CanMoveProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
            "CanMove",
            false,
            false,
            BindingMode.TwoWay);

        #endregion

        #endregion

        protected override void OnLoad() {
            base.OnLoad();

            if (AssociatedObject == null) {
                return;
            }

            AssociatedObject.PointerPressed += AssociatedObject_MouseDown;
            AssociatedObject.PointerReleased += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PointerMoved += AssociatedObject_MouseMove;
            AssociatedObject.PointerEnter += AssociatedObject_MouseEnter;
            AssociatedObject.PointerLeave += AssociatedObject_MouseLeave;

            //if (AssociatedObject.DataContext is MpIMovableViewModel rvm) {
            //    //var dupCheck = _allMovables.FirstOrDefault(x => x.MovableId == rvm.MovableId);
            //    //if (dupCheck != null) {
            //    //    MpConsole.WriteLine("Duplicate movable detected while loading, swapping for new...");
            //    //    _allMovables.Remove(dupCheck);
            //    //}
            //    _allMovables.Add(rvm);
            //}
        }


        protected override void OnUnload() {
            base.OnUnload();

            if(AssociatedObject != null) {
                AssociatedObject.PointerPressed -= AssociatedObject_MouseDown;
                AssociatedObject.PointerReleased -= AssociatedObject_MouseLeftButtonUp;
                AssociatedObject.PointerMoved -= AssociatedObject_MouseMove;
                AssociatedObject.PointerEnter -= AssociatedObject_MouseEnter;
                AssociatedObject.PointerLeave -= AssociatedObject_MouseLeave;

                //if (AssociatedObject.DataContext is MpIMovableViewModel rvm) {
                //    var toRemove = _allMovables.FirstOrDefault(x => x.MovableId == rvm.MovableId);
                //    if (toRemove != null) {
                //        _allMovables.Remove(toRemove);
                //    }
                //}
            }
        }

        #region Public Methods

        public void Move(double dx, double dy) {
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }
            var adivm = AssociatedObject.DataContext as MpIBoxViewModel;
            var newLoc = new Point(adivm.X + dx, adivm.Y + dy);
            adivm.X = newLoc.X;
            adivm.Y = newLoc.Y;

            //MpConsole.WriteLine($"New Location: {adivm.X} {adivm.Y}");
        }

        #endregion

        #region Private Methods


        #region Move Event Handlers

        private void AssociatedObject_MouseEnter(object sender, PointerEventArgs e) {
            if(!IsAnyMoving && !MpAvDragDropManager.IsDragAndDrop) {
                CanMove = true;
            }
        }

        private void AssociatedObject_MouseLeave(object sender, PointerEventArgs e) {
            if(!IsMoving) {
                CanMove = false;
            }
            MpPlatformWrapper.Services.Cursor.UnsetCursor(AssociatedObject.DataContext);
        }

        private void AssociatedObject_MouseMove(object sender, PointerEventArgs e) {
            if(!IsMoving) {
                return;
            }

            MpPlatformWrapper.Services.Cursor.SetCursor(AssociatedObject, MpCursorType.ResizeAll);
            var mwmp = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();

            var delta = mwmp - _lastMousePosition;

            // NOTE must transform mouse delta from designer canvas scaling
            delta.X *= 1 / MpActionCollectionViewModel.Instance.ScaleX;
            delta.Y *= 1 / MpActionCollectionViewModel.Instance.ScaleY;

            Move(delta.X, delta.Y);

            _lastMousePosition = mwmp;
        }

        private void AssociatedObject_MouseDown(object sender, PointerPressedEventArgs e) {
            if(MpAvZoomBorder.IsTranslating) {
                return;
            }

            _mouseDownPosition = _lastMousePosition = e.GetPosition(MpAvMainWindow.Instance).ToPortablePoint();
            if (AssociatedObject.DataContext is MpISelectableViewModel svm) {
                svm.IsSelected = true;
            }
            e.Pointer.Capture(AssociatedObject);
            IsMoving = e.Pointer.Captured == AssociatedObject;
            e.Handled = true;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e) {
            FinishMove(e);
        }

        private void FinishMove(PointerReleasedEventArgs e) {
            e.Pointer.Capture(null);
            //AssociatedObject.ReleaseMouseCapture();

            if (IsMoving) {
                IsMoving = false;
                (AssociatedObject.DataContext as MpAvActionViewModelBase).HasModelChanged = true;
            }
            if ((_lastMousePosition - _mouseDownPosition).Length < 5) {
                Command?.Execute(CommandParameter);
            }
        }

        #endregion

        #endregion
    }
}
