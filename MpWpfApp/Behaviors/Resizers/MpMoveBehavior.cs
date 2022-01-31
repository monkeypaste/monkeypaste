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
using MonkeyPaste;

namespace MpWpfApp {

    public class MpMoveBehavior : MpBehavior<FrameworkElement> {
        #region Private Variables

        private static List<MpIMovableViewModel> _allMovables = new List<MpIMovableViewModel>();

        public static bool IsAnyMoving => _allMovables.Any(x => x.IsMoving);
        public static bool CanAnyMove => _allMovables.Any(x => x.CanMove);

        private Point _lastMousePosition;

        #endregion

        #region Properties

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

            AssociatedObject.PreviewMouseLeftButtonDown += AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;

            if (AssociatedObject.DataContext is MpIMovableViewModel rvm) {
                if (_allMovables.Contains(rvm)) {
                    MpConsole.WriteLine("Duplicate resizer detected while loading, swapping for new...");
                    _allMovables.Remove(rvm);
                }
                _allMovables.Add(rvm);
            }
        }


        protected override void OnUnload() {
            base.OnUnload();

            AssociatedObject.PreviewMouseLeftButtonDown -= AssociatedObject_MouseDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;

            if (AssociatedObject.DataContext is MpIMovableViewModel rvm) {
                if (_allMovables.Contains(rvm)) {
                    MpConsole.WriteLine("Duplicate resizer detected while loading, swapping for new...");
                    _allMovables.Remove(rvm);
                }
            }
        }

        #region Public Methods


        public void Move(double dx, double dy) {
            if (Math.Abs(dx + dy) < 0.1) {
                return;
            }
            var adivm = AssociatedObject.DataContext as MpIActionDesignerItemViewModel;
            adivm.X += dx;
            adivm.Y += dy;
            MpConsole.WriteLine("Moved: " + dx + " " + dy);
        }

        #endregion

        #region Private Methods


        #region Manual Resize Event Handlers

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e) {
            if(!IsAnyMoving) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.ResizeAll;
                CanMove = true;
            }
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e) {
            if(!IsMoving) {
                MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
                CanMove = false;
            }
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e) {
            if (!IsMoving) {
                return;
            }

            var mwmp = e.GetPosition(Application.Current.MainWindow);

            Vector delta = mwmp - _lastMousePosition;
            _lastMousePosition = mwmp;

            Move(delta.X, delta.Y);
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (!IsAnyMoving) {
                IsMoving = AssociatedObject.CaptureMouse();

                if (IsMoving) {
                    _lastMousePosition = e.GetPosition(Application.Current.MainWindow);
                    e.Handled = true;
                }
            }
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            AssociatedObject.ReleaseMouseCapture();

            if (IsMoving) {
                IsMoving = false;
            }
        }

        #endregion

        #endregion
    }
}
