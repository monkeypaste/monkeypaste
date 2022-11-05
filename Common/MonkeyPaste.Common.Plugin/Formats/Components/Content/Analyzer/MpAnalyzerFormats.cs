
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using MonkeyPaste.Common;


namespace MonkeyPaste.Common.Plugin {
    public class MpAnalyzerPluginFormat : MpPluginContentComponentBaseFormat {
    }
        
    public class MpAnalyzerPluginRequestFormat : MpPluginRequestFormatBase {

        public MpPortableDataObject data { get; set; }
    }

    public class MpAnalyzerPluginResponseFormat : MpPluginResponseFormatBase { }

}
