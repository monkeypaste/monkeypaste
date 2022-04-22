using MonkeyPaste.Plugin;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpListBox : ListBox {

        #region Methods

        #region Overrides

        protected override void OnDragEnter(DragEventArgs e) {
            //MpConsole.WriteLine("Tray drag enter");
            OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            this.GetVisualAncestor<MpClipTrayView>().ClipTrayDropBehavior.OnDragOver(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.GetVisualAncestor<MpClipTrayView>().ClipTrayDropBehavior.OnDragLeave(this, e);
        }

        protected override void OnDrop(DragEventArgs e) {            
            this.GetVisualAncestor<MpClipTrayView>().ClipTrayDropBehavior.OnDrop(this, e);
        }

        #endregion

        #endregion
    }
}
