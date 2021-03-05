using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace MpWpfApp {
    public class MpAnimatedVisibilityFadeBehavior : Behavior<Border> {
        public Duration AnimationDuration { get; set; }
        public Visibility InitialState { get; set; }

        DoubleAnimation m_animationOut;
        DoubleAnimation m_animationIn;

        protected override void OnAttached() {
            base.OnAttached();

            m_animationIn = new DoubleAnimation(1, AnimationDuration, FillBehavior.HoldEnd);
            m_animationOut = new DoubleAnimation(0, AnimationDuration, FillBehavior.HoldEnd);
            m_animationOut.Completed += (sender, args) => {
                AssociatedObject.SetCurrentValue(Border.VisibilityProperty, Visibility.Collapsed);
            };

            AssociatedObject.SetCurrentValue(Border.VisibilityProperty,
                                             InitialState == Visibility.Collapsed
                                                ? Visibility.Collapsed
                                                : Visibility.Visible);

            Binding.AddTargetUpdatedHandler(AssociatedObject, Updated);
        }

        private void Updated(object sender, DataTransferEventArgs e) {
            var value = (Visibility)AssociatedObject.GetValue(Border.VisibilityProperty);
            switch (value) {
                case Visibility.Collapsed:
                    AssociatedObject.SetCurrentValue(Border.VisibilityProperty, Visibility.Visible);
                    AssociatedObject.BeginAnimation(Border.OpacityProperty, m_animationOut);
                    break;
                case Visibility.Visible:
                    AssociatedObject.BeginAnimation(Border.OpacityProperty, m_animationIn);
                    break;
            }
        }
    }
}
