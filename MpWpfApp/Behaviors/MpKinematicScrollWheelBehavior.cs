using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;

namespace MpWpfApp {
    public class MpKinematicScrollWheelBehavior : Behavior<ListBox>{
        #region Private Variables

        private double _lastWheelDelta = 0;
        private double _velocity = 0;
        private double _scrollTarget = 0;

        private DispatcherTimer _timer;

        #endregion

        #region Properties

        public double Friction { get; set; } = 0;
        public double WheelDampening { get; set; } = 0;

        #endregion

        protected override void OnAttached() {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(async () => {
                ScrollViewer sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
                while (sv == null) {
                    sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
                    await Task.Delay(100);
                }
                sv.PreviewMouseWheel += Sv_PreviewMouseWheel;
                
                _timer = new DispatcherTimer();
                _timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                _timer.Tick += HandleWorldTimerTick;
                _timer.Start();

            });
        }

        private void HandleWorldTimerTick(object sender, EventArgs e) {
            ScrollViewer sv = AssociatedObject.GetVisualDescendent<ScrollViewer>();
            if (sv == null) {
                return;
            }

            double lbw = AssociatedObject.ActualWidth;
            double sbo = sv.ContentHorizontalOffset;
            if(_lastWheelDelta < 0) {
                if (lbw + sbo >= sv.ExtentWidth) {
                    //when scrolling down and scrollViewer is at end of list do not let velocity accumulate
                    _velocity = _lastWheelDelta = 0;
                }
            } else if(_lastWheelDelta > 0) {
                if(sbo == 0) {
                    //when scrolling up and scrollViewer is at beginning of list do not let velocity accumulate
                    _velocity = _lastWheelDelta = 0;
                }
            }

            //if (MpClipTrayViewModel.Instance.IsLoadingMore ||
            //    MpClipTrayViewModel.Instance.IsLastItemVisible) {
            //    _scrollTarget = sv.HorizontalOffset;
            //    _velocity = _lastWheelDelta = 0;
            //}

            if (Math.Abs(_velocity) > 0.1) {
                sv.ScrollToHorizontalOffset(_scrollTarget);
                _scrollTarget += _velocity;
                _velocity *= Friction;
                _lastWheelDelta = 0;
            }
        }

        private void Sv_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            // e.Delta < 0 : scrolling down into list
            // e.Delta > 0 : scrolling up to beginning

            if (MpClipTrayViewModel.Instance.IsAnyTileFlipped ||
                MpClipTrayViewModel.Instance.IsAnyTileExpanded ||
                MpMainWindowViewModel.Instance.IsMainWindowOpening) {
                return;
            }
            _velocity -= e.Delta * WheelDampening;
            _lastWheelDelta = e.Delta;

            e.Handled = false;
            //wheel event is passed to the loadMoreExtentsion which marks handled=true if there's a wheel delta
        }
    }
}
