using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeOptionsView : MpAvUserControl<MpAvWelcomeOptionGroupViewModel> {
        public MpAvWelcomeOptionsView() : base() {
            InitializeComponent();
            this.Loaded += OptionButton_Loaded;
        }

        private async void Title_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control c) {
                return;
            }
            // BUG this delay should match grow animation to fix initial center alignment
            await Task.Delay(1_500);
            c.InvalidateAll();

            await Task.Delay(1_400);
            c.InvalidateAll();
        }

        private void OptionButton_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not RadioButton rb) {
                return;
            }
            rb.AddHandler(PointerPressedEvent, Rb_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void Rb_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (sender is not Control c ||
                c.DataContext is not MpAvWelcomeOptionItemViewModel woivm) {
                return;
            }
            e.Handled = true;
            woivm.CheckOptionCommand.Execute(null);
        }

        private async void DragImage_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (BindingContext != null &&
                BindingContext.Parent != null &&
                BindingContext.Parent.CurPointerGestureWindowViewModel != null &&
                BindingContext.Parent.CurPointerGestureWindowViewModel.FakeWindowViewModel != null) {
                BindingContext.Parent.CurPointerGestureWindowViewModel.FakeWindowViewModel.ResetDropState();
            }
            var ido = new DataObject();
            ido.Set(MpPortableDataFormats.Text, MpAvFakeWindowView.DRAG_TEXT);
            _ = await MpAvDoDragDropWrapper.DoDragDropAsync(sender as Control, e, ido, DragDropEffects.Copy | DragDropEffects.Move);
        }
    }
}
