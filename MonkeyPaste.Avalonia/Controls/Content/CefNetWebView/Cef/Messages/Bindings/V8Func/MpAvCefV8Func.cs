using CefNet;
using System.Diagnostics;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace MonkeyPaste.Avalonia {
    class MpAvCefV8Func : CefV8Handler {
        private string _dbPath;

        public MpAvCefV8Func(string dbPath) : base() {
            _dbPath = dbPath;
        }

        protected override bool Execute(string name, CefV8Value @object, CefV8Value[] arguments, ref CefV8Value retval, ref string exception) {
            CefProcessMessage browserProcMsg = new CefProcessMessage("WindowBindingResponse");
            browserProcMsg.ArgumentList.SetString(0, name);
            if (arguments != null) {
                for (int i = 0; i < arguments.Length; i++) {
                    browserProcMsg.ArgumentList.SetString(i + 1, arguments[i].GetStringValue());
                }
            } else {
                browserProcMsg.ArgumentList.SetString(1, String.Empty);
            }

            CefV8Context.GetCurrentContext().Frame.SendProcessMessage(CefProcessId.Browser, browserProcMsg);

            exception = null;
            retval = null;
            return true;
        }
    }
}
