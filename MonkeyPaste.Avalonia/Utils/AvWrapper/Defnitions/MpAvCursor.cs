using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste;
using System;
using System.Collections.Generic;
namespace MonkeyPaste.Avalonia {
    public class MpAvCursor : MpICursor {

        private Dictionary<MpCursorType, Cursor> _cursorLookup =
            new Dictionary<MpCursorType, Cursor>() {
                {MpCursorType.None, new Cursor(StandardCursorType.Arrow) },
                {MpCursorType.Default, new Cursor(StandardCursorType.Arrow)},
                //{MpCursorType.OverDragItem, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandOpenCursor"] as string))  )},
                //{MpCursorType.ContentMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  )},
                //{MpCursorType.TileMove, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["HandClosedCursor"] as string))  )},
                //{MpCursorType.ContentCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string)) )},
                //{MpCursorType.TileCopy, new Cursor(Path.Combine(Environment.CurrentDirectory, Application.Current.Resources["CopyCursor"] as string))  )},
                {MpCursorType.OverDragItem, new Cursor(StandardCursorType.Hand)},
                {MpCursorType.ContentMove, new Cursor(StandardCursorType.Hand)},
                {MpCursorType.TileMove, new Cursor(StandardCursorType.Hand)},
                {MpCursorType.ContentCopy, new Cursor(StandardCursorType.Hand)},
                {MpCursorType.TileCopy, new Cursor(StandardCursorType.Hand)},
                {MpCursorType.Invalid, new Cursor(StandardCursorType.No )},
                {MpCursorType.Waiting, new Cursor(StandardCursorType.Wait )},
                {MpCursorType.IBeam, new Cursor(StandardCursorType.Ibeam )},
                {MpCursorType.ResizeNS, new Cursor(StandardCursorType.SizeNorthSouth )},
                {MpCursorType.ResizeWE, new Cursor(StandardCursorType.SizeWestEast )},
                {MpCursorType.ResizeNWSE, new Cursor(StandardCursorType.SizeAll )},
                {MpCursorType.ResizeNESW, new Cursor(StandardCursorType.SizeAll )},
                {MpCursorType.ResizeAll, new Cursor(StandardCursorType.SizeAll )},
                {MpCursorType.Hand, new Cursor(StandardCursorType.Hand )}
            };

        public void SetCursor(MpCursorType newCursor) {
            //if (MpClipTrayViewModel.Instance.HasScrollVelocity) {
            //    return;
            //}

            if (Dispatcher.UIThread.CheckAccess()) {
                Cursor cursor = _cursorLookup[newCursor];

                //Mouse.OverrideCursor = cursor;
                //Mouse.PrimaryDevice.OverrideCursor = cursor;

                if (MainWindow.Instance == null) {
                    // NOTE occurs on init
                    return;
                }
                //Application.Current.MainWindow.ForceCursor = true;
                //Application.Current.MainWindow.Cursor = cursor;
                MainWindow.Instance.Cursor = cursor;
            } else {
                MpPlatformWrapper.Services.MainThreadMarshal.RunOnMainThread(() => SetCursor(newCursor));
                //MpHelpers.RunOnMainThread(()=>SetCursor(newCursor));
            }
        }
    }
}
