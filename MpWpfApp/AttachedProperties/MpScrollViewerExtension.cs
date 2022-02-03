using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MpWpfApp {
    public class MpScrollViewerExtension : DependencyObject {
        #region VerticalOffset attached property

        public static double GetVerticalOffset(DependencyObject depObj) {
            return (double)depObj.GetValue(VerticalOffsetProperty);
        }

        public static void SetVerticalOffset(DependencyObject depObj, double value) {
            depObj.SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalOffset",
                typeof(double),
                typeof(MpScrollViewerExtension),
                new FrameworkPropertyMetadata {
                    DefaultValue = default(double),
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = (obj, e) => {
                        var sv = obj as ScrollViewer;
                        if (sv == null) {
                            return;
                        }
                        SetVerticalOffset(sv, (double)e.NewValue);
                        sv.ScrollToVerticalOffset((double)e.NewValue);
                    }
                });


        #endregion

        #region HorizontalOffset attached property

        public static double GetHorizontalOffset(DependencyObject depObj) {
            return (double)depObj.GetValue(HorizontalOffsetProperty);
        }

        public static void SetHorizontalOffset(DependencyObject depObj, double value) {
            depObj.SetValue(HorizontalOffsetProperty, value);
        }
        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalOffset",
                typeof(double),
                typeof(MpScrollViewerExtension),
                new FrameworkPropertyMetadata {
                    DefaultValue = default(double),
                    BindsTwoWayByDefault = true,
                    PropertyChangedCallback = (obj, e) => {
                        var sv = obj as ScrollViewer;
                        if(sv == null) {
                            return;
                        }
                        SetHorizontalOffset(sv, (double)e.NewValue);
                        sv.ScrollToHorizontalOffset((double)e.NewValue);
                    }
                });

        #endregion

        #region IsEnabled DependencyProperty

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
            typeof(MpScrollViewerExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if (e.NewValue is bool isEnabled) {
                        var sv = obj as ScrollViewer;
                        if(sv == null) {
                            return;
                        }
                        if (isEnabled) {
                            sv.ScrollChanged += Sv_ScrollChanged;
                            sv.Unloaded += Sv_Unloaded;
                        } else {
                            Sv_Unloaded(sv, null);
                        }
                    }
                }
            });

        private static void Sv_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            var sv = sender as ScrollViewer;
            if(GetHorizontalOffset(sv) != e.HorizontalChange) {
                SetHorizontalOffset(sv, e.HorizontalOffset);
            }
            if (GetVerticalOffset(sv) != e.VerticalOffset) {
                SetVerticalOffset(sv, e.VerticalOffset);
            }
        }

        private static void Sv_Unloaded(object sender, RoutedEventArgs e) {
            var sv = sender as ScrollViewer;

            sv.Unloaded -= Sv_Unloaded;
            sv.ScrollChanged -= Sv_ScrollChanged;
        }


        #endregion
    }
}
