using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvDragToOpenGestureView : MpAvUserControl<MpAvPointerGestureWindowViewModel> {
        public MpAvDragToOpenGestureView() : base() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
