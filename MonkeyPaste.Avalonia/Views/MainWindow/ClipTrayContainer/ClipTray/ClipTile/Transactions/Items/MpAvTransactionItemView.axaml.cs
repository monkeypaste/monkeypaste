using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTransactionItemView : MpAvUserControl<MpAvTransactionItemViewModel> {
        public MpAvTransactionItemView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
