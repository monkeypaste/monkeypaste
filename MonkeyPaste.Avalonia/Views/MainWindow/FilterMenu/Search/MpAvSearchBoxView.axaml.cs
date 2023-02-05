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
        public static Button SearchIconButton { get; private set; }
        public MpAvSearchBoxView() {
            InitializeComponent();

            var sb = this.FindControl<AutoCompleteBox>("SearchBox");
            sb.AttachedToVisualTree += Sb_AttachedToVisualTree;
            sb.AddHandler(Control.KeyUpEvent, SearchBox_KeyUp, RoutingStrategies.Tunnel);

            SearchIconButton = this.FindControl<Button>("SearchDropDownButton");
        }

        #region Drop

        private void Sb_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            var sb = sender as AutoCompleteBox;
            
            sb.AddHandler(DragDrop.DragOverEvent, DragOver);
            sb.AddHandler(DragDrop.DropEvent, Drop);
        }
        private void DragEnter(object sender, DragEventArgs e) {
            e.DragEffects = DragDropEffects.Copy | DragDropEffects.Move;
        }

        private void DragOver(object sender, DragEventArgs e) {
            //e.DragEffects = DragDropEffects.Default;
            if(!e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
                e.DragEffects = DragDropEffects.None;
            }
        }
        private void Drop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataFormats().Contains(MpPortableDataFormats.Text)) {
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
