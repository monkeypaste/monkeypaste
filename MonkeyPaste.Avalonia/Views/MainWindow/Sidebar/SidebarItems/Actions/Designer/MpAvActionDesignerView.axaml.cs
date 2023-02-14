using Avalonia.Markup.Xaml;


namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpActionDesignerView.xaml
    /// </summary>
    public partial class MpAvActionDesignerView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvActionDesignerView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
