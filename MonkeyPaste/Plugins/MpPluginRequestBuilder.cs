using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginRequestBuilder {
        public static async Task<MpAnalyzerPluginRequestFormat> BuildRequestAsync(
            List<MpParameterFormat> paramFormats,
            Dictionary<object, string> paramValues,
            MpCopyItem sourceContent) {

            List<MpParameterRequestItemFormat> requestItems = new List<MpParameterRequestItemFormat>();

            foreach (var paramFormat in paramFormats) {
                MpParameterRequestItemFormat requestItem = await BuildRequestItem(
                    paramFormat,
                    paramValues[paramFormat.paramId],
                    sourceContent);
                requestItems.Add(requestItem);
            }

            return new MpAnalyzerPluginRequestFormat() {
                items = requestItems
            };
        }

        private static async Task<MpParameterRequestItemFormat> BuildRequestItem(
            MpParameterFormat paramFormat,
            string paramValue,
            MpCopyItem sourceContent) {
            return new MpParameterRequestItemFormat() {
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
