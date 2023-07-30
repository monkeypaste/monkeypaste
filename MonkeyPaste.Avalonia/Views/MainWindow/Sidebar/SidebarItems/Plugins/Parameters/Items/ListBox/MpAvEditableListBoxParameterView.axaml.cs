using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpListBoxParameterView.xaml
    /// </summary>
    public partial class MpAvEditableListBoxParameterView : MpAvUserControl<MpAvEditableEnumerableParameterViewModel> {
        public MpAvEditableListBoxParameterView() {
            AvaloniaXamlLoader.Load(this);
            var el = this.FindControl<ListBox>("EditableList");
            el.AttachedToVisualTree += El_AttachedToVisualTree;

        }


        private void El_AttachedToVisualTree(object sender, global::Avalonia.VisualTreeAttachmentEventArgs e) {
            if (BindingContext.Items.Count == 0) {
                BindingContext.AddValueCommand.Execute(null);
            }
        }


        public async void MoveRowButton_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (sender is not Control mb ||
                mb.DataContext is not MpAvEnumerableParameterValueViewModel epvvm) {
                return;
            }
            var avdo = new MpAvDataObject(MpPortableDataFormats.INTERNAL_PARAMETER_VALUE_FORMAT, epvvm);

            var result = await DragDrop.DoDragDrop(e, avdo, DragDropEffects.Move | DragDropEffects.Copy);
        }
    }
}
