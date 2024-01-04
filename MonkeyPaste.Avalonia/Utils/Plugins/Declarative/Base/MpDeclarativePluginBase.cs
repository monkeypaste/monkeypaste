using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public abstract class MpDeclarativePluginBase : MpIAnalyzeComponentAsync, MpIAnalyzeComponent {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIAnalyzeComponent Implementation
        public MpAnalyzerPluginResponseFormat Analyze(MpAnalyzerPluginRequestFormat req) {
            string resp = RunDeclarativeAnalyzer(req);
            var result = DecodeResponseOutput(resp);
            return result;
        }
        #endregion

        #region MpIAnalyzeAsyncComponent Implementation
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            string resp = await RunDeclarativeAnalyzerAsync(req);
            var result = DecodeResponseOutput(resp);
            return result;
        }
        #endregion
        #endregion

        #region Properties

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected abstract string RunDeclarativeAnalyzer(MpAnalyzerPluginRequestFormat req);
        protected abstract Task<string> RunDeclarativeAnalyzerAsync(MpAnalyzerPluginRequestFormat req);
        #endregion

        #region Private Methods

        protected virtual MpAnalyzerPluginResponseFormat DecodeResponseOutput(string output) {
            return MpJsonExtensions.DeserializeBase64Object<MpAnalyzerPluginResponseFormat>(output);
            // if (MpJsonConverter.DeserializeBase64Object<MpAnalyzerPluginResponseFormat>(output) is MpAnalyzerPluginResponseFormat aprf {
            //object result = JsonConvert.DeserializeObject(jsonStr);
            //if (result is JObject jobj) {
            //    return new MpAnalyzerPluginResponseFormat() {
            //        errorMessage = jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.errorMessage)).ToString(),
            //        retryMessage = jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.retryMessage)).ToString(),
            //        //dataObjectLookup = MpPortableDataObject.Parse(jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.dataObjectLookup)).ToString()),

            //        //dataObjectLookup = MpPortableDataObject.Parse(jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.dataObjectLookup)).ToString()),
            //        //annotations = MpJsonConverter.DeserializeObject<List<MpPluginResponseAnnotationFormat>>(
            //        //    jobj.SelectToken(nameof(MpAnalyzerPluginResponseFormat.annotations)).ToString()),
            //    };
            //}
            //}
            //return null;
        }
        #endregion

        #region Commands
        #endregion
    }
}
