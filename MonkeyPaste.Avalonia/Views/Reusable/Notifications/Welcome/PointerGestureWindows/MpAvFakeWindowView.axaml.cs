using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvFakeWindowView : MpAvUserControl<MpAvFakeWindowViewModel> {
        public MpAvFakeWindowView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
