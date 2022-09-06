using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvCursor : MpICursor, MpIBootstrappedItem {
        #region Private Variables

        private static Dictionary<MpCursorType, string> _cursorLookupData =
            new Dictionary<MpCursorType, string>() {
                {MpCursorType.None, StandardCursorType.Arrow.ToString() },
                {MpCursorType.Default, StandardCursorType.Arrow.ToString() },
                {MpCursorType.OverDragItem, "HandOpenCursor" },
                {MpCursorType.ContentMove, "HandClosedCursor"},
                {MpCursorType.TileMove, "HandClosedCursor"},
                {MpCursorType.ContentCopy, "CopyCursor" },
                {MpCursorType.TileCopy, "CopyCursor"},


                //{MpCursorType.OverDragItem, StandardCursorType.Hand)},
                //{MpCursorType.ContentMove, StandardCursorType.Hand)},
                //{MpCursorType.TileMove, StandardCursorType.Cross)},
                //{MpCursorType.ContentCopy, StandardCursorType.Hand)},
                //{MpCursorType.TileCopy, StandardCursorType.Hand)},
                {MpCursorType.Invalid, StandardCursorType.No.ToString() },
                {MpCursorType.Waiting, StandardCursorType.Wait.ToString() },
                {MpCursorType.IBeam, StandardCursorType.Ibeam.ToString() },
                {MpCursorType.ResizeNS, StandardCursorType.SizeNorthSouth.ToString() },
                {MpCursorType.ResizeWE, StandardCursorType.SizeWestEast.ToString() },
                {MpCursorType.ResizeNWSE, StandardCursorType.SizeAll.ToString() },
                {MpCursorType.ResizeNESW, StandardCursorType.SizeAll.ToString() },
                {MpCursorType.ResizeAll, StandardCursorType.SizeAll.ToString() },
                {MpCursorType.Hand, StandardCursorType.Hand.ToString() },
                {MpCursorType.Arrow, StandardCursorType.Arrow.ToString() }
            };

        private static Dictionary<MpCursorType, Cursor> _cursorLookup;

        #endregion

        #region Statics
        public static Cursor ConvertCursorTypeToAvCursor(MpCursorType ct, Cursor fallback = null) {
            if(_cursorLookup.TryGetValue(ct, out Cursor avCursor)) {
                return avCursor;
            }
            Debugger.Break();
            return fallback == null ? new Cursor(StandardCursorType.Arrow) : fallback;
        }
        #endregion

        #region Properties
        private MpCursorType _currentCursor = MpCursorType.Default;
        MpCursorType MpICursor.CurrentCursor => _currentCursor;
        #endregion

        #region Constructors
        public MpAvCursor(MpAvPlatformResource resLoader) {
            if(_cursorLookup != null) {
                Debugger.Break();
                return;
            }

            _cursorLookup = new Dictionary<MpCursorType, Cursor>();
            foreach (var cdkvp in _cursorLookupData) {
                Cursor c = null;
                if (Enum.TryParse(cdkvp.Value, false, out StandardCursorType ct)) {
                    c = new Cursor(ct);
                } else {
                    string cursor_resource_path = resLoader.GetResource(cdkvp.Value) as string;
                    var cbmp = MpAvStringResourceToBitmapConverter.Instance.Convert(cursor_resource_path, null, null, null) as IBitmap;
                    c = new Cursor(cbmp, PixelPoint.Origin);
                }
                if (c == null) {
                    Debugger.Break();
                }
                _cursorLookup.Add(cdkvp.Key, c);
            }
        }
        #endregion

        #region Public Methods


        void MpICursor.SetCursor(object targetObj, MpCursorType newCursor) {
            //if (MpAvClipTrayViewModel.Instance.HasScrollVelocity) {
            //    return;
            //}
            if (targetObj != null && !targetObj.GetType().IsSubclassOf(typeof(InputElement)) && !targetObj.GetType().IsSubclassOf(typeof(MpViewModelBase))) {
                Debugger.Break();
                return;
            }

            Dispatcher.UIThread.Post(() => {
                Cursor cursor = _cursorLookup[newCursor];
                InputElement targetElm = null;
                if(targetObj is InputElement) {
                    targetElm = targetObj as InputElement;
                } else {
                    targetElm = App.Desktop.MainWindow;
                }


                if (targetElm == null) {
                    return;
                }
                targetElm.Cursor = cursor;
                _currentCursor = newCursor;

                string logStr = $"Type: '{targetObj}' set Cursor to: '{_currentCursor}'";
                MpConsole.WriteLogLine(logStr);
            });
        }

        void MpICursor.UnsetCursor(object targetObj) {
            // TODO? if necessary setup cursor stack and remove entry here or ignore
            (this as MpICursor).SetCursor(targetObj, MpCursorType.Default);
        }

        public string Label => "Cursors";

        #endregion
    }
}
