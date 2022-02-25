using MonkeyPaste;
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
using MonkeyPaste;

namespace MonkeyPaste {

    public static class MpCursorStack {
        #region private static Variables

        private static MpICursor _cursor;
        //private static int _isBusyCount = 0;

        private static List<object> _pendingPops = new List<object>();

        #endregion

        #region Properties

        #region State

        //public static bool IsAppBusy => _isBusyCount > 0;

        public static MpCursorType CurrentCursor => CursorStack.Count == 0 ? MpCursorType.Default : CursorStack.Peek().Value;

        public static Stack<KeyValuePair<object,MpCursorType>> CursorStack { get; private set; } = new Stack<KeyValuePair<object, MpCursorType>>();

        #endregion

        #endregion

        #region Constructors

        public static async Task Init(MpICursor cursor) {
            await Task.Delay(1);
            _cursor = cursor;
        }

        #endregion

        #region public static Methods

        public static void PushCursor(object sender, MpCursorType cursor) {
            if(_pendingPops.Contains(sender)) {
                //if elelement set cursor but was overridden by another and element unset but
                //is setting again remove previous pop
                _pendingPops.Remove(sender);
            }
            if(CursorStack.Count > 0 && CursorStack.Peek().Key == sender) {
                //when current cursor changes its cursor don't add another stack level
                CursorStack.Pop();
            }
            CursorStack.Push(new KeyValuePair<object, MpCursorType>(sender, cursor));

            _cursor.SetCursor(CurrentCursor);
        }

        public static bool PopCursor(object sender) {
            if(CursorStack.Count > 0 && CursorStack.Peek().Key == sender) {
                //only let the most recent element pop cursor
                CursorStack.Pop();
                if(CursorStack.Count > 0) {
                    var popsToRemove = new List<object>();
                    while (_pendingPops.Contains(CursorStack.Peek().Key)) {
                        // cleanup any pending 
                        var popKvp = CursorStack.Pop();
                        popsToRemove.Add(popKvp.Key);
                        if (CursorStack.Count == 0) {
                            break;
                        }
                    }
                    foreach (var popToRemove in popsToRemove) {
                        _pendingPops.Remove(popsToRemove);
                    }
                }

                _cursor.SetCursor(CurrentCursor);
                return true;
            }
            if(CursorStack.ToArray().Any(x=>x.Key == sender)) {
                //only store pop if element has pushed

                //if pop is from prior element note pop once later lets it go
                _pendingPops.Add(sender);
            }
            return false;
        }

        #endregion

        #region private static Methods


        #endregion
    }
}
