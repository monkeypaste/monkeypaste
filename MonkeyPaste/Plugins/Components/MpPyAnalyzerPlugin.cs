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
    public class MpPyAnalyzerPlugin : MpCommandLinePlugin {

        public MpPyAnalyzerPlugin(string targetPath) : base(targetPath) {
            Endpoint = Path.Combine(SCRIPTS_ROOT_DIR, "python", "pyrunner.bat");
        }

    }
}
