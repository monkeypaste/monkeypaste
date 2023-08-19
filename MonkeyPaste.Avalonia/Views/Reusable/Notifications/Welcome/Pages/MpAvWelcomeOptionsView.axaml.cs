using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeOptionsView : MpAvUserControl<MpAvWelcomeOptionGroupViewModel> {
        public MpAvWelcomeOptionsView() : base() {
            AvaloniaXamlLoader.Load(this);
            new RadioButton().Click += MpAvWelcomeOptionsView_Click;
        }

        private void MpAvWelcomeOptionsView_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (sender is not Control c ||
                c.DataContext is not MpAvWelcomeOptionItemViewModel woivm) {
                return;
            }
            woivm.ToggleOptionCommand.Execute(sender);
        }
    }
}
