using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvSettingsFrameCollectionView : UserControl {
        public MpAvSettingsFrameCollectionView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
