using Avalonia;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvParameterCollectionView : MpAvUserControl<MpAvIParameterCollectionViewModel> {


        public MpAvParameterCollectionView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
