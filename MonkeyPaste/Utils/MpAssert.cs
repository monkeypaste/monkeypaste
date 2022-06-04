using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public static class MpAssert {
        public static void Assert(
            object boolOrObj, 
            string failStr, 
            bool breakOnFail = false, bool exceptionOnFail = false, 
            [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            bool failed = false;
            if(boolOrObj == null) {
                failed = true;
            } else if(boolOrObj is bool boolParam && !boolParam) {
                failed = true;
            } 
            if(failed) {
                if(breakOnFail) {
                    Debugger.Break();
                } 
                if(exceptionOnFail) {
                    throw new Exception(failStr);
                }
                MpConsole.WriteTraceLine(failStr, null, callerName, callerFilePath, lineNum);
            }
        }

    }
}
