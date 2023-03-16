using Avalonia.Markup.Xaml;


namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpActionDesignerView.xaml
    /// </summary>
    public partial class MpAvTriggerOptionsView : MpAvUserControl<MpAvTriggerCollectionViewModel> {
        public MpAvTriggerOptionsView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
