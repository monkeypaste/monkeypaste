using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPluginSelectorItemView : MpAvUserControl<MpIAsyncComboBoxItemViewModel> {
        public MpAvPluginSelectorItemView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
