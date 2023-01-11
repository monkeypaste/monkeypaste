using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvLostFocusUpdateBindingBehavior : Behavior<TextBox> {
        static MpAvLostFocusUpdateBindingBehavior() {
            TextProperty.Changed.Subscribe(e => {
                ((MpAvLostFocusUpdateBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<MpAvLostFocusUpdateBindingBehavior, string>(
            "Text", defaultBindingMode: BindingMode.TwoWay);

        public string Text {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached() {
            AssociatedObject.LostFocus += OnLostFocus;
            base.OnAttached();
        }

        protected override void OnDetaching() {
            AssociatedObject.LostFocus -= OnLostFocus;
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e) {
            if (AssociatedObject != null)
                Text = AssociatedObject.Text;
        }

        private void OnBindingValueChanged() {
            if (AssociatedObject != null)
                AssociatedObject.Text = Text;
        }
    }
}
