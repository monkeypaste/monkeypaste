using Avalonia.Controls;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvEditableListBoxParameterView : MpAvUserControl<MpAvEditableEnumerableParameterViewModel> {
        public MpAvEditableListBoxParameterView() {
            InitializeComponent();
            var el = this.FindControl<ListBox>("EditableList");
            el.AttachedToVisualTree += El_AttachedToVisualTree;
        }

        private void El_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            if (BindingContext.Items.Count == 0) {
                BindingContext.AddValueCommand.Execute(null);
            }
        }
    }
}
