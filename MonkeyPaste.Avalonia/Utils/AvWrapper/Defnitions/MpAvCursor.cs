using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvCursor : MpICursor {

        private Dictionary<MpCursorType, Cursor> _cursorLookup =
            new Dictionary<MpCursorType, Cursor>() {
                {MpCursorType.None, Cursors.Arrow },
                {MpCursorType.Default, Cursors.Arrow },
                //{MpCursorType.OverDragItem, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandOpenCursor"] as string))  },
                //{MpCursorType.ContentMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  },
                //{MpCursorType.TileMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  },
                //{MpCursorType.ContentCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string)) },
                //{MpCursorType.TileCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string))  },
                {MpCursorType.OverDragItem, Cursors.Hand},
                {MpCursorType.ContentMove, Cursors.Hand},
                {MpCursorType.TileMove, Cursors.Hand},
                {MpCursorType.ContentCopy, Cursors.Hand},
                {MpCursorType.TileCopy, Cursors.Hand},
                {MpCursorType.Invalid, Cursors.No },
                {MpCursorType.Waiting, Cursors.Wait },
                {MpCursorType.IBeam, Cursors.IBeam },
                {MpCursorType.ResizeNS, Cursors.SizeNS },
                {MpCursorType.ResizeWE, Cursors.SizeWE },
                {MpCursorType.ResizeNWSE, Cursors.SizeNWSE },
                {MpCursorType.ResizeNESW, Cursors.SizeNESW },
                {MpCursorType.ResizeAll, Cursors.SizeAll },
                {MpCursorType.Hand, Cursors.Hand },
            };

        public void SetCursor(MpCursorType newCursor) {
            //if (MpClipTrayViewModel.Instance.HasScrollVelocity) {
            //    return;
            //}

            if (Application.Current.Dispatcher.CheckAccess()) {
                Cursor cursor = _cursorLookup[newCursor];

                Mouse.OverrideCursor = cursor;
                Mouse.PrimaryDevice.OverrideCursor = cursor;

                if (Application.Current.MainWindow == null) {
                    // NOTE occurs on init
                    return;
                }
                Application.Current.MainWindow.ForceCursor = true;
                Application.Current.MainWindow.Cursor = cursor;
            } else {
                MpPlatformWrapper.Services.MainThreadMarshal.RunOnMainThread(() => SetCursor(newCursor));
                //MpHelpers.RunOnMainThread(()=>SetCursor(newCursor));
            }
        }
    }
}
