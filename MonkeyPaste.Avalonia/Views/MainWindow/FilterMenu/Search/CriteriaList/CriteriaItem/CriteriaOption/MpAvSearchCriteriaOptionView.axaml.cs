
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    /// <summary>
    /// Interaction logic for MpCriteriaItemOptionView.xaml
    /// </summary>
    public partial class MpAvSearchCriteriaOptionView : MpAvUserControl<MpAvSearchCriteriaOptionViewModel> {
        public MpAvSearchCriteriaOptionView() {
            AvaloniaXamlLoader.Load(this);
        }


        private void TextBox_AttachedToLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            if (sender is TextBox tb) {
                InitDragDrop(tb);
            }
        }

        #region Drop
        private void InitDragDrop(TextBox tb) {
            DragDrop.SetAllowDrop(tb, true);
            tb.AddHandler(DragDrop.DragOverEvent, DragOver);
            tb.AddHandler(DragDrop.DropEvent, Drop);
        }

        private void DragOver(object sender, DragEventArgs e) {
            //e.DragEffects = DragDropEffects.Default;
            if (!e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
            } else {
                // override criteria sorting
                e.Handled = true;
            }
        }
        private void Drop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
                return;
            }
            if (sender is TextBox tb) {
                tb.Text = e.Data.Get(MpPortableDataFormats.Text) as string;
            }
        }

        #endregion
    }
}
