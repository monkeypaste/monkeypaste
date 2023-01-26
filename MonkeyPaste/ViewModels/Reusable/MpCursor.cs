using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonkeyPaste {

    public static class MpCursor {
        #region private static Variables

        private static MpICursor _cursor;
        #endregion

        #region Properties

        #region State

        public static MpCursorType CurrentCursor { get; set; } = MpCursorType.Default;

        //public static bool IsCursorFrozen { get; set; } = false;
        #endregion

        #endregion

        #region Constructors

        public static void Init() {
            _cursor = MpPlatform.Services.Cursor;
        }

        #endregion

        #region public static Methods

        public static void SetCursor(object targetObj, MpCursorType cursor) {
            if(!MpBootstrapperViewModelBase.IsCoreLoaded) { // || IsCursorFrozen) {
                return;
            }

            CurrentCursor = cursor;
            _cursor.SetCursor(targetObj,CurrentCursor);

            string logStr = $"Type: '{targetObj}' set Cursor to: '{cursor}'";
            MpConsole.WriteLogLine(logStr);
        }

        public static void UnsetCursor(object sender) {
            SetCursor(sender, MpCursorType.Default);
        }

        #endregion

        #region private static Methods


        #endregion
    }
}
