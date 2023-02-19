using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvLostFocusUpdateBindingBehavior : Behavior<TextBox> {
        static MpAvLostFocusUpdateBindingBehavior() {
            TextProperty.Changed.Subscribe(e => {
                ((MpAvLostFocusUpdateBindingBehavior)e.Sender).OnBindingValueChanged();
            });
        }


        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<MpAvLostFocusUpdateBindingBehavior, string>(
            name: nameof(Text),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

        public string Text {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void OnAttached() {
            if (AssociatedObject != null) {
                AssociatedObject.LostFocus += OnLostFocus;
                AssociatedObject.DetachedFromLogicalTree += AssociatedObject_DetachedFromLogicalTree;
            }
            base.OnAttached();
        }

        private void AssociatedObject_DetachedFromLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            this.Detach();
        }

        protected override void OnDetaching() {
            if (AssociatedObject != null) {
                AssociatedObject.LostFocus -= OnLostFocus;
            }
            base.OnDetaching();
        }

        private void OnLostFocus(object? sender, RoutedEventArgs e) {
            if (AssociatedObject != null) {
                Text = AssociatedObject.Text;
            }
        }

        private void OnBindingValueChanged() {
            if (AssociatedObject != null) {
                AssociatedObject.Text = Text;
            }
        }
    }
}
