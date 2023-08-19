using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvScrollToOpenGestureView : MpAvUserControl<MpAvScrollToOpenGestureViewModel> {
        public MpAvScrollToOpenGestureView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
