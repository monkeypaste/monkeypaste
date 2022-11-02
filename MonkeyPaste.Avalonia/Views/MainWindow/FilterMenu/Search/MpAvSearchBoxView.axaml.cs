using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSearchBoxView : MpAvUserControl<MpAvSearchBoxViewModel> {
        private ContextMenu _searchByContextMenu;

        public MpAvSearchBoxView() {
            InitializeComponent();

            var sb = this.FindControl<AutoCompleteBox>("SearchBox");
            sb.AttachedToVisualTree += Sb_AttachedToVisualTree;
            sb.AddHandler(Control.KeyUpEvent, SearchBox_KeyUp, RoutingStrategies.Tunnel);
        }

        #region Drop
        private object _dropLock = System.Guid.NewGuid().ToString();

        private void Sb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var sb = sender as AutoCompleteBox;
            
            sb.AddHandler(DragDrop.DragOverEvent, DragOver);
            sb.AddHandler(DragDrop.DropEvent, Drop);
        }
        private void DragEnter(object sender, DragEventArgs e) {
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
        }

        private async void DragOver(object sender, DragEventArgs e) {
            //e.DragEffects = DragDropEffects.Default;
            var formats = await e.Data.GetDataFormats_safe(_dropLock);
            if(!formats.Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
            }
        }
        private async void Drop(object sender, DragEventArgs e) {
            var formats = await e.Data.GetDataFormats_safe(_dropLock);
            if (!formats.Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
                return;
            }
            BindingContext.SearchText = e.Data.Get(MpPortableDataFormats.Text) as string;
        }

        #endregion

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void SearchBox_KeyUp(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if(sender is Control control && 
                    control.GetVisualDescendant<TextBox>() is TextBox tb) {
                    tb.SelectAll();
                }
                e.Handled = true;
                BindingContext.PerformSearchCommand.Execute(null);
                
            }
        }

    }
}
