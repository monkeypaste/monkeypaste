using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpWpfCursor : MpICursor {

        private Dictionary<MpCursorType, Cursor> _cursorLookup =
            new Dictionary<MpCursorType, Cursor>() {
                {MpCursorType.None, Cursors.Arrow },
                {MpCursorType.Default, Cursors.Arrow },
                {MpCursorType.OverDragItem, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandOpenCursor"] as string))  },
                {MpCursorType.ContentMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  },
                {MpCursorType.TileMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  },
                {MpCursorType.ContentCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string)) },
                {MpCursorType.TileCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string))  },
                {MpCursorType.Invalid, Cursors.No },
                {MpCursorType.Waiting, Cursors.Wait },
                {MpCursorType.IBeam, Cursors.IBeam },
                {MpCursorType.ResizeNS, Cursors.SizeNS },
                {MpCursorType.ResizeWE, Cursors.SizeWE },
                {MpCursorType.ResizeNWSE, Cursors.SizeNWSE },
                {MpCursorType.ResizeNESW, Cursors.SizeNESW },
                {MpCursorType.ResizeAll, Cursors.SizeAll },
                {MpCursorType.Link, Cursors.Hand },
            };

        public void SetCursor(MpCursorType newCursor) {
            if (MpClipTrayViewModel.Instance.IsScrolling ||
               MpClipTrayViewModel.Instance.IsLoadingMore) {
                return;
            }
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
                MpHelpers.RunOnMainThread(()=>SetCursor(newCursor));
            }
        }
    }
}
