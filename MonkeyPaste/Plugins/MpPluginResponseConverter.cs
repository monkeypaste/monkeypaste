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
            int citid,
            object sourceHandler,
            bool suppressWrite) {
            MpCopyItem target_ci = await ProcessDataObjectAsync(pluginFormat, trans, sourceCopyItem, citid, sourceHandler, suppressWrite);
            return target_ci;
        }

        private static async Task<MpCopyItem> ProcessDataObjectAsync(
            MpPluginFormat pluginFormat,
            MpAnalyzerTransaction trans, 
            MpCopyItem sourceCopyItem, 
            int citid,
            object sourceHandler, bool suppressWrite = false) {
            if (trans == null || 
                trans.Response == null || 
                trans.Response.dataObject == null || 
                trans.Response.dataObject.DataFormatLookup.Count == 0) {
                return null;
            }
            var mpdo = trans.Response.dataObject;
            var outputType = pluginFormat.analyzer.outputType;
            List<string> ref_urls = new List<string>();
            if(mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out IEnumerable<string> mpdo_urls)) {
                // retain any references response may have included
                ref_urls = mpdo_urls.ToList();
            }

            if(sourceHandler is MpPluginPreset pp) {
                // add reference to plugin
                var plugin_source_ref = await MpDataModelProvider.GetSourceRefByTransactionTypeAndSourceIdAsync(
                    MpCopyItemSourceType.AnalyzerPreset, pp.Id);
                if(plugin_source_ref == null) {
                    Debugger.Break();
                }
                string plugin_ref_url = MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(plugin_source_ref);
                if (!ref_urls.All(x => x.ToLower() != plugin_ref_url.ToLower())) {
                    ref_urls.Add(plugin_ref_url);
                }

            }

            MpCopyItem target_ci = null;
            if(outputType.IsOutputNewContent() &&
                sourceCopyItem != null) {
                // when new content is created reference source content
                // (I think by design sourceItem will never be null but maybe that should be ok?)
                string source_url_ref = MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(sourceCopyItem);
                if(!ref_urls.All(x=>x.ToLower() != source_url_ref.ToLower())) {
                    ref_urls.Add(source_url_ref);
                }

                mpdo.SetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, ref_urls);

                // create new item
                target_ci = await MpPlatformWrapper.Services.CopyItemBuilder.BuildAsync(mpdo);
            }  else {
                var ref_sources = 
                    await Task.WhenAll(
                        ref_urls.Select(x => 
                        MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(x)));

                // ClipTileTransaction VM will pickup transaction/sources to update view
                await MpPlatformWrapper.Services.SourceRefBuilder.AddTransactionSourcesAsync(
                    copyItemTransactionId: citid,
                    transactionSources: ref_sources);
            }

            

            return null;
        }
    }
}
