using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
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
            var ctdb = this.GetVisualAncestor<MpClipTrayView>();
            if(ctdb != null) {
                ctdb.ClipTrayDropBehavior.OnDragOver(this, e);
                return;
            }
            var ptdb = this.GetVisualAncestor<MpPinTrayView>();
            if (ptdb != null) {
                ptdb.PinTrayDropBehavior.OnDragOver(this, e);
                return;
            }
        }

        protected override void OnDragLeave(DragEventArgs e) {
            var ctdb = this.GetVisualAncestor<MpClipTrayView>();
            if (ctdb != null) {
                ctdb.ClipTrayDropBehavior.OnDragLeave(this, e);
                return;
            }
            var ptdb = this.GetVisualAncestor<MpPinTrayView>();
            if (ptdb != null) {
                ptdb.PinTrayDropBehavior.OnDragLeave(this, e);
                return;
            }
        }

        protected override void OnDrop(DragEventArgs e) {            
            var ctdb = this.GetVisualAncestor<MpClipTrayView>();
            if (ctdb != null) {
                ctdb.ClipTrayDropBehavior.OnDrop(this, e);
                return;
            }
            var ptdb = this.GetVisualAncestor<MpPinTrayView>();
            if (ptdb != null) {
                ptdb.PinTrayDropBehavior.OnDrop(this, e);
                return;
            }
        }

        #endregion

        #endregion
    }
}
