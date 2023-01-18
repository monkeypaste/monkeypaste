using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using Newtonsoft.Json;
using System.Linq;

namespace MonkeyPaste {
    public static class MpPluginRequestBuilder {
        public static async Task<MpAnalyzerPluginRequestFormat> BuildRequestAsync(
            List<MpParameterFormat> paramFormats,
            Dictionary<object,string> paramValues,
            MpCopyItem sourceContent) {
            
            List<MpPluginRequestItemFormat> requestItems = new List<MpPluginRequestItemFormat>();

            foreach(var paramFormat in paramFormats) {
                MpPluginRequestItemFormat requestItem = await BuildRequestItem(
                    paramFormat,
                    paramValues[paramFormat.paramId],
                    sourceContent);
                requestItems.Add(requestItem);
            }

            return new MpAnalyzerPluginRequestFormat() {
                items = requestItems.Cast<MpIParameterKeyValuePair>().ToList()
            };
        }

        private static async Task<MpPluginRequestItemFormat> BuildRequestItem(
            MpParameterFormat paramFormat, 
            string paramValue,
            MpCopyItem sourceContent) {
            return new MpPluginRequestItemFormat() { 
                paramId = paramFormat.paramId, 
                value = await MpPluginParameterValueEvaluator.GetParameterRequestValueAsync(
                    paramFormat.controlType,
                    paramFormat.unitType,
                    paramValue, 
                    sourceContent)
            };
        }

        
    }
}
