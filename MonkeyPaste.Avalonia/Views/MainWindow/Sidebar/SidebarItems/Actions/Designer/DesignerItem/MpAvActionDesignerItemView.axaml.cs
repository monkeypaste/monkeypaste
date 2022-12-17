
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpActionDesignerItemView.xaml
    /// </summary>
    public partial class MpAvActionDesignerItemView : MpAvUserControl<MpAvActionViewModelBase> {
        public MpAvActionDesignerItemView() {
            InitializeComponent();

            var dicc = this.FindControl<ContentControl>("DesignerItemContentControl");
            dicc.AddHandler(ContentControl.KeyDownEvent, Dicc_KeyDown, RoutingStrategies.Tunnel);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void Dicc_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if(e.Key == Key.Delete) {
                if (BindingContext.IsSelected) {
                    e.Handled = true;
                    BindingContext.DeleteThisActionCommand.Execute(null);
                }
            }
        }


    }
}
