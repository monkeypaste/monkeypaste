using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAvActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpAvActionPropertyView : MpAvUserControl<MpAvActionViewModelBase> {
        public MpAvActionPropertyView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
