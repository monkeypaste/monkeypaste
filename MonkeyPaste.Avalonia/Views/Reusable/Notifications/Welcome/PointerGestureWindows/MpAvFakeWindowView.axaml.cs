using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvFakeWindowView : MpAvUserControl<MpAvPointerGestureWindowViewModel> {
        public MpAvFakeWindowView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
