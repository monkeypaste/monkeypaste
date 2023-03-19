using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvExternalDropView : MpAvUserControl<MpAvExternalDropWindowViewModel> {
        #region Private Variables
        #endregion

        #region Constructors

        public MpAvExternalDropView() : base() {
            AvaloniaXamlLoader.Load(this);
            var hdmb = this.FindControl<Border>("HideDropMenuBorder");
            hdmb.AddHandler(DragDrop.DragOverEvent, OnHideOver);

            var dilb = this.FindControl<ListBox>("DropItemListBox");
            dilb.EnableItemsControlAutoScroll();
        }
        #endregion

        private void OnHideOver(object sender, DragEventArgs e) {
            BindingContext.CancelDropWidgetCommand.Execute(null);
        }
    }
}
