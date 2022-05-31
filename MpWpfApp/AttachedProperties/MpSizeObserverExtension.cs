using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {
    public static class MpSizeObserverExtension {
        #region Properties

        #region ObservedWidth


        public static double GetObservedWidth(DependencyObject dpo) {
            return (double)dpo.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(DependencyObject dpo, double observedWidth) {
            dpo.SetValue(ObservedWidthProperty, observedWidth);
        }

        public static readonly DependencyProperty ObservedWidthProperty =
            DependencyProperty.RegisterAttached(
                "ObservedWidth",
                typeof(double),
                typeof(MpSizeObserverExtension));

        #endregion

        #region ViewModel

        public static object GetViewModel(DependencyObject dpo) {
            return dpo.GetValue(ViewModelProperty);
        }

        public static void SetViewModel(DependencyObject dpo, object ViewModel) {
            dpo.SetValue(ViewModelProperty, ViewModel);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.RegisterAttached(
                "ViewModel",
                typeof(object),
                typeof(MpSizeObserverExtension),
                new FrameworkPropertyMetadata(default(object)));

        #endregion

        #region ObservedHeight

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(MpSizeObserverExtension));

        public static double GetObservedHeight(FrameworkElement fe) {
            //fe.AssertNotNull("fe");
            return (double)fe.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement fe, double observedHeight) {
            //fe.AssertNotNull("fe");
            fe.SetValue(ObservedHeightProperty, observedHeight);
        }

        #endregion

        #region IsEnabled Property

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpSizeObserverExtension),
            new FrameworkPropertyMetadata(OnIsEnabledChanged));

        public static bool GetIsEnabled(FrameworkElement fe) {
            //fe.AssertNotNull("fe");
            return (bool)fe.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(FrameworkElement fe, bool observe) {
            //fe.AssertNotNull("fe");
            fe.SetValue(IsEnabledProperty, observe);
        }


        private static void OnIsEnabledChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e) {
            var fe = (FrameworkElement)dpo;
            
            if ((bool)e.NewValue) {                
                fe.SizeChanged += OnFrameworkElementSizeChanged;
                if (!fe.IsLoaded) {
                    fe.Loaded += Fe_Loaded;
                } else {
                    UpdateObservedSizesForFrameworkElement(fe);
                }                
            } else {
                fe.SizeChanged -= OnFrameworkElementSizeChanged;
                fe.Loaded -= Fe_Loaded;
            }
        }

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            var fe = (FrameworkElement)sender;
            if(fe == null) {
                return;
            }
            UpdateObservedSizesForFrameworkElement(fe);
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement fe) {
            // WPF 4.0 onwards
            fe.SetCurrentValue(ObservedWidthProperty, fe.ActualWidth);
            fe.SetCurrentValue(ObservedHeightProperty, fe.ActualHeight);
        }

        #endregion

        #endregion

        public static void SetWidth(DependencyObject dpo, double width) {
            if(dpo is FrameworkElement fe && GetIsEnabled(fe)) {
                fe.Width = width;
            }
        }


    }
}