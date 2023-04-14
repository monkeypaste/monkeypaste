using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvTransactionNodeDetailView : MpAvUserControl<MpAvITransactionNodeViewModel> {
        public MpAvTransactionNodeDetailView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
