using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public class MpHttpTransactionFormat {
        public string name { get; set; }

        public MpHttpRequestFormat request { get; set; }

        public MpPluginResponseFormat response { get; set; }
    }

    #region Request

    public class MpHttpRequestFormat {        
        public string method { get; set; }

        public List<MpHttpHeaderItemFormat> header { get; set; } = new List<MpHttpHeaderItemFormat>();

        public MpHttpUrlFormat url { get; set; }

        public MpHttpBodyFormat body { get; set; }

        public string description { get; set; }
    }

    public class MpHttpHeaderItemFormat {
        public string key { get; set; }
        public string value { get; set; }
        public string type { get; set; }
    }

    public class MpHttpBodyFormat {
        public string mode { get; set; }
        public string raw { get; set; }
        public string encoding { get; set; } = "UTF8";
        public string mediaType { get; set; }
    }

    public class MpHttpUrlFormat {
        public string raw { get; set; }
        public string protocol { get; set; }

        // joined w/ '.' seperator and terminated w/ "/" or "" if no path
        public List<string> host { get; set; } = new List<string>();

        // joined w/ '/' seperator and terminated with '?' or "" if no query
        public List<string> path { get; set; } = new List<string>();

        public List<MpHttpQueryArgument> query { get; set; }
    }

    public class MpHttpQueryArgument {
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
