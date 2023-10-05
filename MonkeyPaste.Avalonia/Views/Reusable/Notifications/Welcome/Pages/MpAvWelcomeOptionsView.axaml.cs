using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeOptionsView : MpAvUserControl<MpAvWelcomeOptionGroupViewModel> {
        public MpAvWelcomeOptionsView() : base() {
            InitializeComponent();
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
