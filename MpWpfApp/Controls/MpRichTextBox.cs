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
            e.Handled = false;
            if (!IsReadOnly) {
                MpCursor.SetCursor(this, MpCursorType.IBeam);
            }
            if (Mouse.LeftButton == MouseButtonState.Released) {
                if (_isDragging) {

                }
                _mouseDownPoint = null;
                _isDragging = false;
            } else {
                if (!_isDragging) {
                    if (!_mouseDownPoint.HasValue) {
                        _mouseDownPoint = e.GetPosition(this);
                    } else {
                        if (_mouseDownPoint.Value.Distance(e.GetPosition(this)) > 5) {
                            _isDragging = true;

                            MpHelpers.RunOnMainThread((Action)(async () => {
                                DataObject data = new DataObject();
                                var ci = (DataContext as MpClipTileViewModel).HeadItem.CopyItem;
                                data = await MpWpfDataObjectHelper.Instance.ConvertToWpfDataObject(
                                                ci, true, null);

                                //Debugger.Break();

                                DragDrop.DoDragDrop(this, data, DragDropEffects.Move | DragDropEffects.Copy);
                            }));
                        }
                    }

                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e) {
            OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            e.Effects = DragDropEffects.None;

            bool isValid = MpWpfDataObjectHelper.Instance.IsContentDropDragDataValid((DataObject)e.Data);
            if (isValid) {
                if (e.KeyStates == DragDropKeyStates.ControlKey ||
                   e.KeyStates == DragDropKeyStates.AltKey ||
                   e.KeyStates == DragDropKeyStates.ShiftKey) {
                    e.Effects = DragDropEffects.Copy;
                } else {
                    e.Effects = DragDropEffects.Move;
                }
            }
            e.Handled = true;
        }

        protected override void OnDragLeave(DragEventArgs e) {
            // base.OnDragLeave(e);
        }

        protected override void OnDrop(DragEventArgs e) {
            if (e.Handled) {
                return;
            }
            if (e.Data.GetDataPresent(MpDataObject.InternalContentFormat)) {
                var dci = e.Data.GetData(MpDataObject.InternalContentFormat) as MpCopyItem;

                Selection.Text = string.Format(
                    @"{0}{1}{2}",
                    "{c{",
                    dci.Guid,
                    "}c}");
            }
        }

        #endregion

        #endregion
    }
}
