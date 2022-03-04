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

        #endregion

        #endregion

        #region Constructors

        public static void Init(MpICursor cursor) {
            _cursor = cursor;
        }

        #endregion

        #region public static Methods

        public static void SetCursor(object sender, MpCursorType cursor) {
            if(MpNotificationCollectionViewModel.Instance.IsInitialLoad) {
                return;
            }

            CurrentCursor = cursor;
            _cursor.SetCursor(CurrentCursor);

            string logStr = $"Type: '{sender.GetType()}' set Cursor to: '{cursor.ToString()}'";
            if(sender is MpDbModelBase dbo) {
                logStr = $"Guid: '{dbo.Guid}' " + logStr;
            }
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
