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
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        #region Drop

        #region Drop Events

        private void DragEnter(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragEnter] ClipboardFormat: "+BindingContext);

            Dispatcher.UIThread.Post(async () => {
                BindingContext.TogglePresetIsEnabledCommand.Execute(null);

                //var lastCursor = MpAvExternalDropWindow.Instance.Cursor;
                //MpAvExternalDropWindow.Instance.Cursor = new Cursor(StandardCursorType.Wait);
                await MpPlatformWrapper.Services.DataObjectHelperAsync.UpdateDragDropDataObjectAsync(
                    MpAvCefNetWebViewGlue.SourceDataObject,
                    MpAvCefNetWebViewGlue.DragDataObject);
                //MpAvExternalDropWindow.Instance.Cursor = lastCursor;
            });
        }

        private void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragOver] ClipboardFormat: " + BindingContext);
            if(BindingContext.IsEnabled) {
                e.DragEffects = DragDropEffects.Link;
            } else {
                e.DragEffects = DragDropEffects.None;
            }

           this.GetVisualAncestor<MpAvExternalDropWindow>().AutoScrollListBox(e);

        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            MpConsole.WriteLine("[DragLeave] ClipboardFormat: " + BindingContext);
            BindingContext.IsDropItemHovering = false;
        }

        private void Drop(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[Drop] ClipboardFormat: " + BindingContext);
            e.DragEffects = DragDropEffects.None;
        }

        #endregion

        #endregion
    }
}
