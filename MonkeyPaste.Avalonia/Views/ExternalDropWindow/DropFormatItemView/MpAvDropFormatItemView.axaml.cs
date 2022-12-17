using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using System.Diagnostics;
using System;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvDropFormatItemView : MpAvUserControl<MpAvClipboardFormatPresetViewModel> {

        public MpAvDropFormatItemView() {
            InitializeComponent();

            DragDrop.SetAllowDrop(this, true);
            this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            this.AddHandler(DragDrop.DragOverEvent, DragOver);
            this.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            this.AddHandler(DragDrop.DropEvent, Drop);

            this.GetVisualDescendants<Control>(false).ForEach(x => DragDrop.SetAllowDrop(x, false));
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            // no matter what flag that this dnd has a unique state so 
            // any config's should be ignored, too bad (i think)
            if(!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => DragEnter(sender, e));
                return;
            }

            MpAvExternalDropWindowViewModel.Instance.HasUserToggledAnyHandlers = true;

            MpConsole.WriteLine("[DragEnter] Source: " + e.Source);
            if (BindingContext.IsDropItemHovering || e.Source is not Border) {
                // false drag enter, ignore
                return;
            }
            BindingContext.IsDropItemHovering = true;

            MpConsole.WriteLine("[DragEnter] ClipboardFormat: "+BindingContext);
            BindingContext.TogglePresetIsEnabledCommand.Execute(null);
        }

        private void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragOver] ClipboardFormat: " + BindingContext);

            e.DragEffects = DragDropEffects.Link;
            //this.GetVisualAncestor<MpAvExternalDropWindow>().AutoScrollListBox(e);

        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            MpConsole.WriteLine("[DragLeave] Source: " + e.Source);
            // if (e.Source is Border ) {
            // assume its the container border, false readings from internal textblock

            //    }
            if(e.Source is Border) {
                BindingContext.IsDropItemHovering = false;
            }
            
        }

        private void Drop(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[Drop] ClipboardFormat: " + BindingContext);
            e.DragEffects = DragDropEffects.None;
        }

        #endregion

        #endregion
    }
}
