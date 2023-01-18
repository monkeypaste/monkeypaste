using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginResponseConverter {
        public static async Task<MpCopyItem> ConvertAsync(
            MpPluginFormat pluginFormat,
            MpAnalyzerTransaction trans,
            MpCopyItem sourceCopyItem,
            object sourceHandler,
            bool suppressWrite) {
            MpCopyItem target_ci = await ProcessDataObjectAsync(pluginFormat, trans, sourceCopyItem, sourceHandler, suppressWrite);
            return target_ci;
        }

        private static async Task<MpCopyItem> ProcessDataObjectAsync(
            MpPluginFormat pluginFormat,
            MpAnalyzerTransaction trans,
            MpCopyItem sourceCopyItem,
            object sourceHandler, bool suppressWrite = false) {
            var pp = sourceHandler as MpPluginPreset;
            if (pp == null ||
                trans == null || 
                trans.Response == null || 
                trans.Response.dataObject == null || 
                trans.Response.dataObject.DataFormatLookup.Count == 0) {
                return null;
            }

            var mpdo = trans.Response.dataObject;
            var outputType = pluginFormat.analyzer.outputType;
            List<string> ref_urls = new List<string>();

            string source_url_ref = MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(sourceCopyItem);

            var plugin_source_ref = await MpDataModelProvider.GetSourceRefBySourceypeAndSourceIdAsync(
                MpTransactionSourceType.AnalyzerPreset, pp.Id);
            string plugin_ref_url = MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(
                plugin_source_ref, trans.Request.SerializeJsonObjectToBase64());


            // add reference to plugin
            
            if (plugin_source_ref == null) {
                Debugger.Break();
            }

            if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> mpdo_urls)) {
                // retain any references response may have included (remove cur plugin and source item since it will be added, probably shouldn't be there)
                ref_urls = mpdo_urls
                    .Where(x => x != source_url_ref && x != plugin_ref_url).ToList();
            }

            // add reference to preset with args ie 'https://<preset endpoint>/<preset request>'
            ref_urls.Add(plugin_ref_url);

            if (outputType.IsOutputNewContent() &&
                sourceCopyItem != null) {
                // when new content is created reference source content
                // (I think by design sourceItem will never be null but maybe that should be ok?)

                ref_urls.Add(source_url_ref);

                mpdo.SetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, ref_urls);

                // create new item
                var target_ci = await MpPlatformWrapper.Services.CopyItemBuilder.BuildAsync(mpdo);
                return target_ci;
            }

            // NOTE for existing content, plugins should by convention always return a dataobject
            // NOTE 2 after transaction tile picks up db event, reloads, finds transaction and applies deltas (at least thats the plan)
            await MpPlatformWrapper.Services.TransactionBuilder.PerformTransactionAsync(
                        copyItemId: sourceCopyItem.Id,
                        reqType: MpJsonMessageFormatType.ParameterRequest,
                        req: trans.Request.SerializeJsonObject(),
                        respType: MpJsonMessageFormatType.DataObject,
                        resp: mpdo.Serialize(),
                        ref_urls: ref_urls,
                        label: "Analyzed");

            return null;
        }
    }
}
