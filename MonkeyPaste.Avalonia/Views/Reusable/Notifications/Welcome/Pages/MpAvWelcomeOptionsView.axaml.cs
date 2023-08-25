using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeOptionsView : MpAvUserControl<MpAvWelcomeOptionGroupViewModel> {
        public MpAvWelcomeOptionsView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

        private async void DragImage_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            var ido = new DataObject();
            ido.Set(MpPortableDataFormats.Text, "Mmm, freshly dragged bananas");
            _ = await MpAvDoDragDropWrapper.DoDragDropAsync(sender as Control, e, ido, DragDropEffects.Copy | DragDropEffects.Move);
        }
    }
}
