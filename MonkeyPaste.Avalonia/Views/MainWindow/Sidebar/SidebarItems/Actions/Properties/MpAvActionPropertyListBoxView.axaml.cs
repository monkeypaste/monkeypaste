using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpAvActionPropertyListBoxView.xaml
    /// </summary>
    public partial class MpAvActionPropertyListBoxView : MpAvUserControl<MpAvActionCollectionViewModel> {
        public MpAvActionPropertyListBoxView() {
            InitializeComponent();
            var aplb = this.FindControl<ListBox>("ActionPropertyListBox");
            aplb.SelectionChanged += Aplb_SelectionChanged;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void Aplb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lb = sender as ListBox;
            lb.ScrollIntoView(MpAvActionCollectionViewModel.Instance.PrimaryAction);
        }
    }
}
