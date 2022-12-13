using System;
using System.Collections.Generic;
using System.Text;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Plugin {
    public class MpHttpTransactionFormatBase : MpJsonObject {
        public string name { get; set; }

        public MpHttpRequestFormat request { get; set; }

        public MpPluginResponseFormatBase response { get; set; }
    }

    public class MpHttpAnalyzerTransactionFormat : MpHttpTransactionFormatBase {
        public new MpAnalyzerPluginResponseFormat response { get; set; }
    }

    #region Request

    public class MpHttpRequestFormat : MpJsonObject {
        public string method { get; set; }
        public List<MpHttpHeaderItemFormat> header { get; set; } = new List<MpHttpHeaderItemFormat>();
        public MpHttpUrlFormat url { get; set; }
        public MpHttpBodyFormat body { get; set; }
        public string description { get; set; }
    }

    public class MpHttpHeaderItemFormat : MpJsonObject {
        public string key { get; set; }
        public string value { get; set; }
        public MpJsonPathProperty valuePath { get; set; }
        public string type { get; set; }
    }

    public class MpHttpBodyFormat : MpJsonObject {
        public string mode { get; set; }
        public string raw { get; set; }
        public string encoding { get; set; } = "UTF8";
        public string mediaType { get; set; }
    }

    public class MpHttpUrlFormat : MpJsonObject {
        public string raw { get; set; }
        public string protocol { get; set; }

        // joined w/ '.' seperator and terminated w/ "/" or "" if no path
        public List<string> host { get; set; } = new List<string>();

        // joined w/ '/' seperator and terminated with '?' or "" if no query
        public List<string> path { get; set; } = new List<string>();
        
        public List<MpJsonPathProperty> dynamicPath { get; set; }

        public List<MpHttpQueryArgument> query { get; set; }
    }

    public class MpHttpQueryArgument : MpJsonObject {
        public string key { get; set; }
        public string value { get; set; }
        public bool isEnumId { get; set; }
        public bool omitIfNullOrEmpty { get; set; }
    }

    #endregion

    #region Response 

    //public class MpHttpResponseFormat {
        

    //    public MpPluginResponseContentMap responseToContentMap { get; set; }
    //    public List<MpAnalyzerPluginTextResponseFormat> text { get; set; }
    //    public List<MpAnalyzerPluginTextTokenResponseValueFormat> textTokens { get; set; }
    //    public List<MpAnalyzerPluginImageTokenResponseValueFormat> imageTokens { get; set; }
    //}

    #endregion
}
