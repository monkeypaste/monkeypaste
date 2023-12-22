using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpPluginRequestBuilder {
        public static async Task<MpAnalyzerPluginRequestFormat> BuildRequestAsync(
            List<MpParameterFormat> paramFormats,
            Dictionary<object, string> paramValues,
            MpCopyItem sourceContent,
            bool evaluateSourceRefs) {
            // evaluate all content queries and map params to req kvps

            List<MpParameterRequestItemFormat> requestItems = new List<MpParameterRequestItemFormat>();

            foreach (var paramFormat in paramFormats) {
                MpParameterRequestItemFormat requestItem = await BuildRequestItem(
                    paramFormat,
                    paramValues[paramFormat.paramId],
                    sourceContent,
                    evaluateSourceRefs);
                requestItems.Add(requestItem);
            }

            return new MpAnalyzerPluginRequestFormat() {
                items = requestItems
            };
        }

        private static async Task<MpParameterRequestItemFormat> BuildRequestItem(
            MpParameterFormat paramFormat,
            string paramValue,
            MpCopyItem sourceContent,
            bool evaluateSourceRefs) {

            // NOTE for logging, return paramValue expression not result
            string req_item_value =
                evaluateSourceRefs ?
                await MpPluginParameterValueEvaluator.GetParameterRequestValueAsync(
                    paramFormat.controlType,
                    paramFormat.unitType,
                    paramValue,
                    sourceContent) :
                    paramValue;
            return new MpParameterRequestItemFormat() {
                paramId = paramFormat.paramId,
                value = req_item_value
            };
        }


    }
}
