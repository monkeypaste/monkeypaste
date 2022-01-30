using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using MonkeyPaste;

namespace MpWpfApp {

    public class MpResizerExtension : DependencyObject {
        #region MinWidth

        public static double GetMinWidth(DependencyObject obj) {
            return (double)obj.GetValue(MinWidthProperty);
        }
        public static void SetMinWidth(DependencyObject obj, double value) {
            obj.SetValue(MinWidthProperty, value);
        }
        public static readonly DependencyProperty MinWidthProperty =
          DependencyProperty.RegisterAttached(
            "MinWidth",
            typeof(double),
            typeof(MpResizerExtension),
            new FrameworkPropertyMetadata(default(double)));

        #endregion

        #region MinHeight

        public static double GetMinHeight(DependencyObject obj) {
            return (double)obj.GetValue(MinHeightProperty);
        }
        public static void SetMinHeight(DependencyObject obj, double value) {
            obj.SetValue(MinHeightProperty, value);
        }
        public static readonly DependencyProperty MinHeightProperty =
          DependencyProperty.RegisterAttached(
            "MinHeight",
            typeof(double),
            typeof(MpResizerExtension),
            new FrameworkPropertyMetadata(default(double)));

        #endregion

        #region MaxWidth

        public static double GetMaxWidth(DependencyObject obj) {
            return (double)obj.GetValue(MaxWidthProperty);
        }
        public static void SetMaxWidth(DependencyObject obj, double value) {
            obj.SetValue(MaxWidthProperty, value);
        }
        public static readonly DependencyProperty MaxWidthProperty =
          DependencyProperty.RegisterAttached(
            "MaxWidth",
            typeof(double),
            typeof(MpResizerExtension),
            new FrameworkPropertyMetadata(double.MaxValue));

        #endregion

        #region MaxHeight

        public static double GetMaxHeight(DependencyObject obj) {
            return (double)obj.GetValue(MaxHeightProperty);
        }
        public static void SetMaxHeight(DependencyObject obj, double value) {
            obj.SetValue(MaxHeightProperty, value);
        }
        public static readonly DependencyProperty MaxHeightProperty =
          DependencyProperty.RegisterAttached(
            "MaxHeight",
            typeof(double),
            typeof(MpResizerExtension),
            new FrameworkPropertyMetadata(double.MaxValue));

        #endregion

        #region ResizeAdorner

        public static MpResizeAdorner GetResizeAdorner(DependencyObject obj) {
            return (MpResizeAdorner)obj.GetValue(ResizeAdornerProperty);
        }
        public static void SetResizeAdorner(DependencyObject obj, MpResizeAdorner value) {
            obj.SetValue(ResizeAdornerProperty, value);
        }
        public static readonly DependencyProperty ResizeAdornerProperty =
          DependencyProperty.RegisterAttached(
            "ResizeAdorner",
            typeof(MpResizeAdorner),
            typeof(MpResizerExtension),
            new FrameworkPropertyMetadata(null));

        #endregion

        #region IsEnabled

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
            typeof(MpResizerExtension),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if (e.NewValue == null || !obj.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                        return;
                    }
                    var fe = obj as FrameworkElement;
                    bool isEnabled = (bool)e.NewValue;
                    if (isEnabled) {
                        if (fe.IsLoaded) {
                            SetResizeAdorner(fe, new MpResizeAdorner(fe, GetMinWidth(fe), GetMinHeight(fe), GetMaxWidth(fe), GetMaxHeight(fe)));
                        } else {
                            fe.Loaded += Fe_Loaded;
                        }
                    } else {
                        fe.Loaded -= Fe_Loaded;
                        var ra = GetResizeAdorner(fe);
                        if(ra == null) {
                            return;
                        }
                        ra.Disable();
                    }
                }
            });

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = sender as FrameworkElement;
            fe.Loaded -= Fe_Loaded;
            if (GetResizeAdorner(fe) == null) {
                var ra = new MpResizeAdorner(fe, GetMinWidth(fe), GetMinHeight(fe), GetMaxWidth(fe), GetMaxHeight(fe));
                SetResizeAdorner(fe, ra);
            }           
        }

        #endregion
    }
}