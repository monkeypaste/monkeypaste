using MonkeyPaste.Common.Plugin;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Common {
    public static class MpDebug {
        public static bool IsDebug =>
#if DEBUG
            true;
#else
            false;
#endif
        public static void Break(object args = null, bool silent = false, bool asException = false, MpLogLevel level = MpLogLevel.Debug) {
            if (args != null) {
                MpConsole.WriteLine(
                    line: args.ToStringOrEmpty(),
                    level: level);
            }
            if (silent) {
                return;
            }
            if (asException) {
                throw new Exception(args.ToStringOrEmpty());
            }
            BreakAll(true, true, args.ToStringOrEmpty());
        }
        public static void BreakAll(bool pre = true, bool post = false, string msg = default) {
#if DEBUG
            bool can_handle = MpCommonTools.Services != null && MpCommonTools.Services.DebugBreakHelper != null;

            if (Debugger.IsAttached) {
                if (pre && can_handle) {
                    MpCommonTools.Services.DebugBreakHelper.HandlePreBreak();
                }

                Debugger.Break();
                if (post && can_handle) {
                    MpCommonTools.Services.DebugBreakHelper.HandlePostBreak();
                }
            }
#endif
        }
        public static void Assert(bool test, string msg, bool silent = false, bool failAsException = false, MpLogLevel level = MpLogLevel.Debug) {
            if (test) {
                return;
            }
            Break(
                args: msg,
                silent: silent,
                asException: failAsException,
                level: level);
        }
    }
}
