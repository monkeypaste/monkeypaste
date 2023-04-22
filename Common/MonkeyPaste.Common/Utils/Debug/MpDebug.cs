using System;
using System.Diagnostics;
using System.Threading;

namespace MonkeyPaste.Common {
    public static class MpDebug {
        public static void Break(object args = null) {
            if (Debugger.IsAttached) {
                if (args != null) {
                    MpConsole.WriteLine(args.ToString());
                }

                Debugger.Break();
            }
            //else {
            //    throw new Exception(args?.ToString());
            //}
        }
        public static void Assert(bool test, string msg) {
            if (test) {
                return;
            }
            Break(msg);
        }

        public static void BreakAllThreads(object args = null) {
            MpCommonTools.Services.GlobalInputListener.StopInputListener();
            Thread.Sleep(1000);
            Break(args);
            MpCommonTools.Services.GlobalInputListener.StartInputListener();
        }
    }
}
