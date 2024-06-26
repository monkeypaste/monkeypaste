﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpPluginResponseConverter {
        public static async Task<MpCopyItem> ConvertAsync(
            MpRuntimePlugin pluginFormat,
            MpAnalyzerTransaction trans,
            Dictionary<string, string> paramValues,
            MpCopyItem sourceCopyItem,
            object sourceHandler,
            bool suppressWrite) {

            if (trans != null &&
                trans.Response != null &&
                !string.IsNullOrEmpty(trans.Response.errorMessage)) {
                // on error throw exception to error handler
                throw new MpUserNotifiedException(trans.Response.errorMessage);
            }

            if (sourceHandler == null ||
                trans == null ||
                trans.Response == null ||
                trans.Response.dataObjectLookup == null ||
                trans.Response.dataObjectLookup.Count == 0) {
                return null;
            }

            var mpdo = new MpAvDataObject(trans.Response.dataObjectLookup);
            var outputType = pluginFormat.analyzer.outputType;
            List<string> ref_urls = new List<string>();

            string source_url_ref = Mp.Services.SourceRefTools.ConvertToInternalUrl(sourceCopyItem);

            var plugin_source_ref = sourceHandler as MpISourceRef;
            string plugin_param_req_ref_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(
                plugin_source_ref);//, trans.Request.SerializeObjectToBase64());


            // add reference to plugin

            if (plugin_source_ref == null) {
                MpDebug.Break();
            }

            if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> mpdo_urls)) {
                // retain any references response may have included
                // (remove cur plugin and source item since it will be added, probably shouldn't be there)
                ref_urls = mpdo_urls
                    .Where(x => x != source_url_ref && x != plugin_param_req_ref_url).ToList();
            }

            // add reference to preset with args ie 'https://<preset endpoint>/<preset request>'
            ref_urls.Add(plugin_param_req_ref_url);

            if (outputType.IsOutputNewContent() &&
                sourceCopyItem != null) {
                // when new content is created reference source content
                // (I think by design sourceItem will never be null but maybe that should be ok?)

                ref_urls.Add(source_url_ref);

                mpdo.SetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, ref_urls);

                // create new item
                //var target_ci = await
                //    Mp.Services.CopyItemBuilder
                //    .BuildAsync(
                //        pdo: mpdo,
                //        transType: MpTransactionType.Created);
                var target_ci = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(mpdo, false, MpDataObjectSourceType.PluginResponse);
                return target_ci;
            }

            // NOTE for existing content, plugins should by convention always return a dataobject
            // NOTE 2 after transaction tile picks up db event, reloads, finds transaction and applies deltas (at least thats the plan)

            var unevaluated_req = await MpPluginRequestBuilder.BuildRequestAsync(
                                        pluginFormat.analyzer.parameters,
                                        paramValues,
                                        sourceCopyItem,
                                        false);

            await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                        copyItemId: sourceCopyItem.Id,
                        reqType: MpJsonMessageFormatType.ParameterRequest,
                        req: unevaluated_req.SerializeObject(),
                        respType: MpJsonMessageFormatType.DataObject,
                        resp: mpdo.SerializeData(),
                        ref_uris: ref_urls,
                        transType: MpTransactionType.Analyzed);

            return null;
        }
    }
}
