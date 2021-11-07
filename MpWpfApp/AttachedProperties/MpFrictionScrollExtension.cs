using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ListBox = System.Windows.Controls.ListBox;

namespace MpWpfApp {
    public class MpFrictionScrollExtension : DependencyObject {

        #region IsEnabled Property

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }
        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpFrictionScrollExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                    bool isEnabled = (bool)e.NewValue;
                    if (isEnabled) {
                        var fe = obj as FrameworkElement;
                        if (fe != null && fe.IsLoaded) {
                            var sv = fe.GetVisualDescendent<ScrollViewer>();
                            if (sv != null) {
                                sv.PreviewMouseWheel += SvOnPreviewMouseWheel;
                            }
                        } else if(fe != null) {
                            fe.Loaded += FeOnLoaded;
                        }
                    } else {
                        var fe = obj as FrameworkElement;
                        fe.Loaded -= FeOnLoaded;

                        var animationTimer = GetWorldTimer(fe);
                        animationTimer.Stop();
                        animationTimer.Tick -= HandleWorldTimerTick;
                        if (fe.IsLoaded) {
                            var sv = fe.GetVisualDescendent<ScrollViewer>();
                            if (sv != null) {
                                sv.PreviewMouseWheel -= SvOnPreviewMouseWheel;
                            }
                        } else {
                            fe.Loaded += FeOnLoaded;
                        }
                    }
                }
            });

        #endregion

        #region IsHorizontal Property

        public static bool GetIsHorizontal(DependencyObject obj) {
            return (bool)obj.GetValue(IsHorizontalProperty);
        }
        public static void SetIsHorizontal(DependencyObject obj, bool value) {
            obj.SetValue(IsHorizontalProperty, value);
        }
        public static readonly DependencyProperty IsHorizontalProperty =
            DependencyProperty.RegisterAttached(
                "IsHorizontal",
                typeof(bool),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(true));

        #endregion

        #region Friction Property

        public static double GetFriction(DependencyObject obj) {
            return (double)obj.GetValue(FrictionProperty);
        }
        public static void SetFriction(DependencyObject obj, double value) {
            obj.SetValue(FrictionProperty, value);
        }
        public static readonly DependencyProperty FrictionProperty =
            DependencyProperty.RegisterAttached(
                "Friction",
                typeof(double),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(0.0));

        #endregion

        #region LastWheelDelta Property

        public static double GetLastWheelDelta(DependencyObject obj) {
            return (double)obj.GetValue(LastWheelDeltaProperty);
        }
        public static void SetLastWheelDelta(DependencyObject obj, double value) {
            obj.SetValue(LastWheelDeltaProperty, value);
        }
        public static readonly DependencyProperty LastWheelDeltaProperty =
            DependencyProperty.RegisterAttached(
                "LastWheelDelta",
                typeof(double),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(0.0));

        #endregion

        #region Velocity Property

        public static Vector GetVelocity(DependencyObject obj) {
            return (Vector)obj.GetValue(VelocityProperty);
        }
        public static void SetVelocity(DependencyObject obj, Vector value) {
            obj.SetValue(VelocityProperty, value);
        }
        public static readonly DependencyProperty VelocityProperty =
            DependencyProperty.RegisterAttached(
                "Velocity",
                typeof(Vector),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(new Vector()));

        #endregion

        #region ScrollTarget Property

        public static Vector GetScrollTarget(DependencyObject obj) {
            return (Vector)obj.GetValue(ScrollTargetProperty);
        }
        public static void SetScrollTarget(DependencyObject obj, Vector value) {
            obj.SetValue(ScrollTargetProperty, value);
        }
        public static readonly DependencyProperty ScrollTargetProperty =
            DependencyProperty.RegisterAttached(
                "ScrollTarget",
                typeof(Vector),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(new Vector()));

        #endregion

        #region WheelDampening Property

        public static double GetWheelDampening(DependencyObject obj) {
            return (double)obj.GetValue(WheelDampeningProperty);
        }
        public static void SetWheelDampening(DependencyObject obj, double value) {
            obj.SetValue(WheelDampeningProperty, value);
        }
        public static readonly DependencyProperty WheelDampeningProperty =
            DependencyProperty.RegisterAttached(
                "WheelDampening",
                typeof(double),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(0.0));

        #endregion

        #region ScrollViewer Property

        public static ScrollViewer GetScrollViewer(DependencyObject obj) {
            return (ScrollViewer)obj.GetValue(ScrollViewerProperty);
        }
        public static void SetScrollViewer(DependencyObject obj, ScrollViewer value) {
            obj.SetValue(ScrollViewerProperty, value);
        }
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.RegisterAttached(
                "ScrollViewer",
                typeof(ScrollViewer),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        #region WorldTimer Property

        public static DispatcherTimer GetWorldTimer(DependencyObject obj) {
            return (DispatcherTimer)obj.GetValue(WorldTimerProperty);
        }
        public static void SetWorldTimer(DependencyObject obj, DispatcherTimer value) {
            obj.SetValue(WorldTimerProperty, value);
        }
        public static readonly DependencyProperty WorldTimerProperty =
            DependencyProperty.RegisterAttached(
                "WorldTimer",
                typeof(DispatcherTimer),
                typeof(MpFrictionScrollExtension),
                new FrameworkPropertyMetadata(null));

        #endregion

        private static void FeOnLoaded(object sender, RoutedEventArgs e) {
            var dpo = sender as DependencyObject;
            var fe = sender as FrameworkElement;
            ScrollViewer sv = null;

            MpHelpers.Instance.RunOnMainThread(async () => {
                while (sv == null) {
                    sv = fe.GetVisualDescendent<ScrollViewer>();
                    await Task.Delay(100);
                }
                SetScrollViewer(dpo, sv);

                sv.PreviewMouseWheel += SvOnPreviewMouseWheel;
                
                var animationTimer = new DispatcherTimer();
                animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
                animationTimer.Tick += HandleWorldTimerTick;
                animationTimer.Start();
                animationTimer.Tag = dpo;

                SetWorldTimer(dpo, animationTimer);
            }, DispatcherPriority.Background);
        }

        private static void HandleWorldTimerTick(object sender, EventArgs e) {
            DispatcherTimer worldTimer = sender as DispatcherTimer;
            if (worldTimer?.Tag == null) {
                return;
            }
            DependencyObject dpo = worldTimer.Tag as DependencyObject;
            ;
            ScrollViewer sv = GetScrollViewer(dpo);
            if (sv == null) {
                return;
            }
            Vector velocity = GetVelocity(dpo);

            if (velocity.Length > 1) {
                Vector scrollTarget = GetScrollTarget(dpo);

                sv.ScrollToHorizontalOffset(scrollTarget.X);
                sv.ScrollToVerticalOffset(scrollTarget.Y);
                scrollTarget.X += velocity.X;
                scrollTarget.Y += velocity.Y;

                SetScrollTarget(dpo, scrollTarget);
                double friction = GetFriction(dpo);
                velocity *= friction;
                SetVelocity(dpo, velocity);
                SetLastWheelDelta(dpo, 0);
            }
        }

        private static void SvOnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (MpClipTrayViewModel.Instance.IsAnyTileFlipped || 
                MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
                return;
            }

            var dpo = (sender as ScrollViewer).GetVisualAncestor<ListBox>() as DependencyObject;
            bool isHorizontal = GetIsHorizontal(dpo);
            double lastWheelDelta = GetLastWheelDelta(dpo);
            Vector velocity = GetVelocity(dpo);

            double dampening = GetWheelDampening(dpo);
            //if (lastWheelDelta == 0 || e.Delta == 0) {
            //    //ignore
            //} else if ((lastWheelDelta < 0 && e.Delta > 0) ||
            //           (lastWheelDelta > 0 && e.Delta < 0)) {
            //    //when wheel direction changes clear velocity
            //    //for immediate feedback

            //    if (isHorizontal) {
            //        velocity.X = 0;
            //        dampening = 1.0;
            //    } else {
            //        velocity.Y = 0;
            //    }
            //}
            
            if (isHorizontal) {
                velocity.X -= (e.Delta * dampening);
            } else {
                velocity.Y -= (e.Delta * dampening);
            }
            
            lastWheelDelta = e.Delta;

            SetVelocity(dpo, velocity);
            SetLastWheelDelta(dpo,lastWheelDelta);

            e.Handled = true;
            //wheel event is passed to the loadMoreExtentsion which marks handled=true if there's a wheel delta
        }
    }
}