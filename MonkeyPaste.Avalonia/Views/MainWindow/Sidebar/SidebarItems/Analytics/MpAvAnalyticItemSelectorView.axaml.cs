using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAnalyticItemSelectorView : MpAvUserControl<MpAvAnalyticItemCollectionViewModel> {

        public MpAvAnalyticItemSelectorView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
