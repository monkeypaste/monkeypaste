using System.Windows;
using MonkeyPaste;

namespace MpWpfApp {
    public static class MpSizeObserverExtension {
        #region Properties

        #region ObservedWidth

        public static readonly DependencyProperty ObservedWidthProperty = DependencyProperty.RegisterAttached(
            "ObservedWidth",
            typeof(double),
            typeof(MpSizeObserverExtension));

        public static double GetObservedWidth(FrameworkElement frameworkElement) {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (double)frameworkElement.GetValue(ObservedWidthProperty);
        }

        public static void SetObservedWidth(FrameworkElement frameworkElement, double observedWidth) {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObservedWidthProperty, observedWidth);
        }

        #endregion

        #region ObservedHeight

        public static readonly DependencyProperty ObservedHeightProperty = DependencyProperty.RegisterAttached(
            "ObservedHeight",
            typeof(double),
            typeof(MpSizeObserverExtension));

        public static double GetObservedHeight(FrameworkElement frameworkElement) {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (double)frameworkElement.GetValue(ObservedHeightProperty);
        }

        public static void SetObservedHeight(FrameworkElement frameworkElement, double observedHeight) {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(ObservedHeightProperty, observedHeight);
        }

        #endregion

        #region IsEnabled Property

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpSizeObserverExtension),
            new FrameworkPropertyMetadata(OnIsEnabledChanged));

        public static bool GetIsEnabled(FrameworkElement frameworkElement) {
            //frameworkElement.AssertNotNull("frameworkElement");
            return (bool)frameworkElement.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(FrameworkElement frameworkElement, bool observe) {
            //frameworkElement.AssertNotNull("frameworkElement");
            frameworkElement.SetValue(IsEnabledProperty, observe);
        }


        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            var frameworkElement = (FrameworkElement)dependencyObject;

            if ((bool)e.NewValue) {
                frameworkElement.SizeChanged += OnFrameworkElementSizeChanged;
                UpdateObservedSizesForFrameworkElement(frameworkElement);
            } else {
                frameworkElement.SizeChanged -= OnFrameworkElementSizeChanged;
            }
        }

        private static void OnFrameworkElementSizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateObservedSizesForFrameworkElement((FrameworkElement)sender);
        }

        private static void UpdateObservedSizesForFrameworkElement(FrameworkElement frameworkElement) {
            // WPF 4.0 onwards
            frameworkElement.SetCurrentValue(ObservedWidthProperty, frameworkElement.ActualWidth);
            frameworkElement.SetCurrentValue(ObservedHeightProperty, frameworkElement.ActualHeight);
        }

        #endregion

        #endregion


    }
}