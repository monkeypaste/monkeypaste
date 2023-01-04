using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    #region Request

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
