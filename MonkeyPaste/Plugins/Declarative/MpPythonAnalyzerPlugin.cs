using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
//using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpPythonAnalyzerPlugin : MpCommandLinePlugin {

        public MpPythonAnalyzerPlugin(string bundlePath) : base(bundlePath) {
            // TODO this should be resolved at runtime using pycheck.py which should
            // return python.exe path or empty which will invalidte/filter python plugins
            CommandPath = @"C:\Users\tkefauver\AppData\Local\Programs\Python\Python310\python.exe";
        }

    }
}
