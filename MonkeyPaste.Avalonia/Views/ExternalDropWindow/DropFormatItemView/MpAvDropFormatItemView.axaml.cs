using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvDropFormatItemView : MpAvUserControl<MpAvClipboardFormatPresetViewModel> {

        public MpAvDropFormatItemView() {
            AvaloniaXamlLoader.Load(this);

            InitDnd();
        }

        #region Drop

        private void InitDnd() {
            DragDrop.SetAllowDrop(this, true);
            this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            this.AddHandler(DragDrop.DragOverEvent, DragOver);
            this.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            this.AddHandler(DragDrop.DropEvent, Drop);

            this.GetVisualDescendants<Control>(false).ForEach(x => DragDrop.SetAllowDrop(x, false));
        }

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            // no matter what flag that this dnd has a unique state so 
            // any config's should be ignored, too bad (i think)
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(() => DragEnter(sender, e));
                return;
            }
            if (!IsDragValid(sender, e)) {
                return;
            }

            MpAvExternalDropWindowViewModel.Instance.HasUserToggledAnyHandlers = true;

            if (BindingContext.IsDropItemHovering || e.Source is not Border) {
                // false drag enter, ignore
                return;
            }
            BindingContext.IsDropItemHovering = true;

            BindingContext.TogglePresetIsEnabledCommand.Execute(null);
        }

        private void DragOver(object sender, DragEventArgs e) {
            e.DragEffects = DragDropEffects.None;
            if (!IsDragValid(sender, e)) {
                return;
            }
            e.DragEffects = DragDropEffects.Copy;
        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            // assume its the container border, false readings from internal textblock
            if (e.Source is Border) {
                BindingContext.IsDropItemHovering = false;
            }

        }

        private void Drop(object sender, DragEventArgs e) {
            //MpConsole.WriteLine("[Drop] ClipboardFormat: " + BindingContext);
            e.DragEffects = DragDropEffects.None;
        }

        private bool IsDragValid(object sender, DragEventArgs e) {
            //if (BindingContext == null ||
            //    !BindingContext.IsFormatOnSourceDragObject) {
            //    // ignore dnd events when format not on drag object
            //    return false;
            //}
            return true;
        }
        #endregion

        #endregion
    }
}
