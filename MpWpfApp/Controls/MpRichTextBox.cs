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
using System.Windows.Media;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpRichTextBox : RichTextBox {
        private bool _isDragging;
        private Point? _mouseDownPoint;

        #region Methods

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            if (DataContext is MpClipTileViewModel ctvm &&
               IsReadOnly &&
               ctvm.IsSubSelectionEnabled &&
               Selection.IsEmpty) {
                var caret_rect = Selection.Start.GetCharacterRect(LogicalDirection.Forward);

                drawingContext.DrawLine(
                    new Pen(Brushes.Black, 1),
                    caret_rect.TopLeft,
                    caret_rect.BottomLeft);
            }
        }

        protected override void OnSelectionChanged(RoutedEventArgs e) {
            base.OnSelectionChanged(e);
            InvalidateVisual();
        }
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

            this.GetVisualAncestor<MpContentView>().ContentViewDropBehavior.OnDragOver(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.GetVisualAncestor<MpContentView>().ContentViewDropBehavior.OnDragLeave(this, e);
        }

        protected override void OnDrop(DragEventArgs e) {
            
            this.GetVisualAncestor<MpContentView>().ContentViewDropBehavior.OnDrop(this, e);
        }

        #endregion

        #endregion
    }
}
