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
            MpAnalyzerTransaction trans,
            MpCopyItem sourceCopyItem,
            int citid,
            bool suppressWrite) {
            MpCopyItem target_ci = await ProcessDataObjectAsync(trans, sourceCopyItem, citid, suppressWrite);
            return target_ci;
        }

        private static async Task<MpCopyItem> ProcessDataObjectAsync(MpAnalyzerTransaction trans, MpCopyItem sourceCopyItem, int citid, bool suppressWrite = false) {
            if (trans == null || 
                trans.Response == null || 
                trans.Response.dataObject == null || 
                trans.Response.dataObject.DataFormatLookup.Count == 0) {
                return null;
            }
            var mpdo = trans.Response.dataObject;
            string source_url_ref = MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(sourceCopyItem);

            List<MpISourceRef> target_source_refs = new List<MpISourceRef>();
            if (citid > 0) {
                var cit_ref = await MpDataModelProvider.GetSourceRefByCopyItemTransactionIdAsync(citid);
                if (cit_ref != null) {
                    target_source_refs.Add(cit_ref);
                }
            }
            target_source_refs.Add(sourceCopyItem);
            bool isNewContentResult = true;

            if (mpdo.ContainsData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT) &&
                mpdo.GetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT) is IEnumerable<string> urls) {
                var urlList = urls.ToList();
                if(urlList.Any(x=>x == source_url_ref)) {
                    // data references source item so any content formats should replace source item content
                    isNewContentResult = false;
                    // remove self reference
                    urlList.Remove(source_url_ref);
                }

                foreach (var url in urls) {
                   var url_ref = await MpPlatformWrapper.Services.SourceRefBuilder.FetchOrCreateSourceAsync(url);
                    if(url_ref == null) {
                        // whats the url format?
                        Debugger.Break();
                        continue;
                    }
                    target_source_refs.Add(url_ref);
                }
            }

            if(isNewContentResult) {
                // new content item     
                MpCopyItem target_ci = null;
                if (target_source_refs.All(x=> MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(x) != source_url_ref)) {
                    // ensure source content is ref'd in new item (and not duplicated)
                    target_source_refs.Add(sourceCopyItem);
                }
                var new_contnet_source_urls = target_source_refs.Select(x => MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(x));
                // substitute any provided urls so they have internal formatting
                mpdo.SetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, new_contnet_source_urls);

                // create new item
                target_ci = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(mpdo);
                return target_ci;
            }  else {
                // update to host content and/or annotations
                if (mpdo.ContainsData(MpPortableDataFormats.CefHtml) &&
                    mpdo.GetData(MpPortableDataFormats.CefHtml) is string updatedItemData) {
                    await Task.WhenAll(
                        target_source_refs.Select(x =>
                            MpCopyItemSource.CreateAsync(
                            copyItemId: sourceCopyItem.Id,
                            sourceObjId: x.SourceObjId,
                            sourceType: x.SourceType)));

                    sourceCopyItem.ItemData = updatedItemData;
                    await sourceCopyItem.WriteToDatabaseAsync();
                }
                if(mpdo.ContainsData(MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT) &&
                    mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT) is string 
                    annotation_node_json_str) {
                    await MpCopyItemAnnotation.CreateAsync(
                        copyItemId: sourceCopyItem.Id,
                        jsonStr: annotation_node_json_str);

                }
            }

            

            return null;
        }
    }
}
