using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvBusySpinnerView : MpAvUserControl<object> {
        public MpAvBusySpinnerView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
