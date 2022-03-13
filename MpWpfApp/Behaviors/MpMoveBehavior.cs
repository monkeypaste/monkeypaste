using MonkeyPaste;
using System;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Core;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MpWpfApp {

    public class MpMoveBehavior : MpBehavior<FrameworkElement> {
        #region Private Variables

        private static List<MpIMovableViewModel> _allMovables = new List<MpIMovableViewModel>();

        public static bool IsAnyMoving => _allMovables.Any(x => x.IsMoving);
        public static bool CanAnyMove => _allMovables.Any(x => x.CanMove);

        private Point _lastMousePosition;

        private Point _mouseDownPosition;

        #endregion

        #region Properties

        #region Command DependencyProperty

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                "Command", typeof(ICommand),
                typeof(MpMoveBehavior),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region CommandParameter DependencyProperty

        public object CommandParameter {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(
                "CommandParameter", typeof(object),
                typeof(MpMoveBehavior),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region IsMoving DependencyProperty

        public bool IsMoving {
            get { return (bool)GetValue(IsMovingProperty); }
            set { SetValue(IsMovingProperty, value); }
        }

        public static readonly DependencyProperty IsMovingProperty =
            DependencyProperty.Register(
                "IsMoving", typeof(bool),
                typeof(MpMoveBehavior),
                new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region CanMove DependencyProperty

        public bool CanMove {
            get { return (bool)GetValue(CanMoveProperty); }
            set { SetValue(CanMoveProperty, value); }
        }

        public static readonly DependencyProperty CanMoveProperty =
            DependencyProperty.Register(
                "CanMove", typeof(bool),
                typeof(MpMoveBehavior),
                new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #endregion

        protected override void OnLoad() {
            base.OnLoad();

            if (AssociatedObject == null) {
                return;
            }

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;

            if (AssociatedObject.DataContext is MpIMovableViewModel rvm) {
                if (_allMovables.Contains(rvm)) {
                    MpConsole.WriteLine("Duplicate movable detected while loading, swapping for new...");
                    _allMovables.Remove(rvm);
                }
                _allMovables.Add(rvm);
            }
        }


        protected override void OnUnload() {
            base.OnUnload();

            if(AssociatedObject != null) {
                AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
                AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
                AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
                AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
                AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;

                if (AssociatedObject.DataContext is MpIMovableViewModel rvm) {
                    if (_allMovables.Contains(rvm)) {
                        MpConsole.WriteLine("Duplicate movable detected while loading, swapping for new...");
                        _allMovables.Remove(rvm);
                    }
                }
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


        #region Manual Resize Event Handlers

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            if(!IsAnyMoving && !MpDragDropManager.IsDragAndDrop) {
                CanMove = true;
            }
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if(!IsMoving) {
                CanMove = false;
            }
            MpCursor.UnsetCursor(AssociatedObject.DataContext);
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if(!IsMoving) {
                return;
            }

            MpCursor.SetCursor(AssociatedObject.DataContext, MpCursorType.ResizeAll);
            var mwmp = e.GetPosition(Application.Current.MainWindow);

            Vector delta = mwmp - _lastMousePosition;
            _lastMousePosition = mwmp;

            Move(delta.X, delta.Y);
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            _mouseDownPosition = _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
            if (AssociatedObject.DataContext is MpISelectableViewModel svm) {
                svm.IsSelected = true;
            }
            IsMoving = AssociatedObject.CaptureMouse();
            e.Handled = true;
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            FinishMove();
        }

        private void FinishMove() {
            AssociatedObject.ReleaseMouseCapture();

            if (IsMoving) {
                IsMoving = false;
                (AssociatedObject.DataContext as MpActionViewModelBase).HasModelChanged = true;
            }
            if ((_lastMousePosition - _mouseDownPosition).Length < 5) {
                Command?.Execute(CommandParameter);
            }
        }

        #endregion

        #endregion
    }
}
