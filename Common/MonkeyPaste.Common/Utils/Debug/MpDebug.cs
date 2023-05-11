using System;
using System.Diagnostics;
using System.Threading;

namespace MonkeyPaste.Common {
    public static class MpDebug {
        public static void Break(object args = null, bool silent = false) {
            if (Debugger.IsAttached) {
                if (args != null) {
                    MpConsole.WriteLine(args.ToString());
                }
                if (silent) {
                    return;
                }
                Debugger.Break();
            }
            //else {
            //    throw new Exception(args?.ToString());
            //}
        }
        public static void Assert(bool test, string msg, bool silent = false) {
            if (test) {
                return;
            }
            Break(msg, silent);
        }

        public static void BreakAllThreads(object args = null) {
            MpCommonTools.Services.GlobalInputListener.StopInputListener();
            Thread.Sleep(1000);
            Break(args);
            MpCommonTools.Services.GlobalInputListener.StartInputListener();
        }
    }
}
