using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Linq;
using PropertyChanged;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvTagView : MpAvUserControl<MpAvTagTileViewModel> {
        public bool IsTreeTag { get; set; } = false;

        public MpAvTagView() {
            InitializeComponent();
            var tagNameBorder = this.FindControl<MpAvClipBorder>("TagNameBorder");
            tagNameBorder.PointerPressed += TagNameBorder_PointerPressed;
        }

        private void TagNameTextBox_KeyDown(object sender, global::Avalonia.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                BindingContext.FinishRenameTagCommand.Execute(null);

            } else if (e.Key == Key.Escape) {
                e.Handled = true;
                BindingContext.CancelRenameTagCommand.Execute(null);
            }
        }


        private void TagNameBorder_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) {
                return;
            }
            if(e.ClickCount > 1) {
                BindingContext.RenameTagCommand.Execute(null);
            } else if (BindingContext.IsSelected) {
                MpDataModelProvider.QueryInfo.NotifyQueryChanged();
            }
            //MpDragDropManager.StartDragCheck(BindingContext);
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }        
    }
}
