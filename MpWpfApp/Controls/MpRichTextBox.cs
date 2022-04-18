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
    public class MpRichTextBox : RichTextBox {
        private bool _isDragging;
        private Point? _mouseDownPoint;

        #region Methods

        #region Overrides

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            //e.Handled = false;
            //if (!IsReadOnly) {
            //    MpCursor.SetCursor(this, MpCursorType.IBeam);
            //}
            //if (Mouse.LeftButton == MouseButtonState.Released) {
            //    if (_isDragging) {

            //    }
            //    _mouseDownPoint = null;
            //    _isDragging = false;
            //} else {
            //    if (!_isDragging) {
            //        if (!_mouseDownPoint.HasValue) {
            //            _mouseDownPoint = e.GetPosition(this);
            //        } else {
            //            if (_mouseDownPoint.Value.Distance(e.GetPosition(this)) > 5) {
            //                _isDragging = true;

            //                MpHelpers.RunOnMainThread((Action)(async () => {
            //                    DataObject data = new DataObject();
            //                    var ci = (DataContext as MpClipTileViewModel).HeadItem.CopyItem;
            //                    data = await MpWpfDataObjectHelper.Instance.ConvertToWpfDataObject(
            //                                    ci, true, null);

            //                    //Debugger.Break();

            //                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move | DragDropEffects.Copy);
            //                }));
            //            }
            //        }

            //    }
            //}
        }

        protected override void OnDragEnter(DragEventArgs e) {
            OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {

            this.GetVisualAncestor<MpContentView>().ContentViewDropBehavior.Rtb_DragOver(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.GetVisualAncestor<MpContentView>().ContentViewDropBehavior.Rtb_DragLeave(this, e);
        }

        protected override void OnDrop(DragEventArgs e) {
            
            this.GetVisualAncestor<MpContentView>().ContentViewDropBehavior.Rtb_Drop(this, e);
        }

        #endregion

        #endregion
    }
}
