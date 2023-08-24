using Avalonia.Controls;
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeWindow : MpAvWindow<MpAvWelcomeNotificationViewModel> {

        public MpAvWelcomeWindow() {
            InitializeComponent();
            var mb = this.FindControl<Button>("MinimizeButton");
            mb.Click += (s, e) => {
                if (TopLevel.GetTopLevel(this) is Window w) {
                    w.WindowState = WindowState.Minimized;
                }
            };
        }


        private async void DragImage_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            _ = await MpAvDoDragDropWrapper.DoDragDropAsync(sender as Control, e, new MpAvDataObject(MpPortableDataFormats.Text, "Mmm, freshly dragged bananas"), DragDropEffects.Copy | DragDropEffects.Move);
        }

    }

}
