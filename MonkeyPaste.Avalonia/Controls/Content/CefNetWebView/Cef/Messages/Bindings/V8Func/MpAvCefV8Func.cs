using CefNet;
using System.Diagnostics;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    class MpAvCefV8Func : CefV8Handler {
        private string _dbPath;


        public MpAvCefV8Func(string dbPath) : base() {
            _dbPath = dbPath;
        }

        protected override bool Execute(string name, CefV8Value @object, CefV8Value[] arguments, ref CefV8Value retval, ref string exception) {
            //MpConsole.WriteLine("Received window binding msg name: " + name);
            // if(arguments != null) {
            //     arguments.ForEach((x, i) => MpConsole.WriteLine("Arg " + i + ": " + x.ToString()));
            // }
            if(name.StartsWith("get")) {
                // js is accessing data from cs...
                // is it accessible?
                //Debugger.Break();

                if (name == "getAllTemplatesFromDb") {
                    List<MpTextTemplate> citl = MpDataModelProvider.GetItems<MpTextTemplate>();

                    exception = null;
                    retval = CefV8Value.CreateString(JsonConvert.SerializeObject(citl));
                    return true;
                }
                if(name == "getDragData") {

                }
            } else if(name.StartsWith("notify")) {
                // js is setting cs data..

                CefProcessMessage browserProcMsg = new CefProcessMessage("WindowBindingResponse");
                browserProcMsg.ArgumentList.SetString(0, name);
                if(arguments != null) {
                    for (int i = 0; i < arguments.Length; i++) {
                        browserProcMsg.ArgumentList.SetString(i+1, arguments[i].GetStringValue());
                    }
                }else {
                    browserProcMsg.ArgumentList.SetString(1, "<NO PARAM>");
                }
                
                CefV8Context.GetCurrentContext().Frame.SendProcessMessage(CefProcessId.Browser, browserProcMsg);

                exception = null;
                retval = null;
                return true;
            }

            // unknown msg name
            MpConsole.WriteTraceLine("Uknown msg name: " + name);

            Debugger.Break();

            return false;
        }
    }
}
