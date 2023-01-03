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
            MpCopyItem target_ci;

            //if (trans.Response is MpPluginResponseFormatBase prf) {
            //    target_ci = await ProcessNewContentItemAsync(prf, sourceCopyItem, citid, suppressWrite);
            //} else {
            //    target_ci = sourceCopyItem;
            //}
            //await ProcessAnnotationsAsync(trans, target_ci, citid, suppressWrite);

            target_ci = await ProcessDataObjectAsync(trans, sourceCopyItem, citid, suppressWrite);

            //if (suppressWrite == false && target_ci != null) {
            //    // NOTE when target is in tray it will get notified from db update and re-initialize
            //    await target_ci.WriteToDatabaseAsync();

            //    if(sourceCopyItem.Id != target_ci.Id) {
            //        //MpAvQueryInfoViewModel.Current.NotifyQueryChanged(false);
            //        MpPlatformWrapper.Services.QueryInfo.NotifyQueryChanged();
            //    }
            //}

            //if (target_ci == null) {
            //    //this should only occur during an action sequece
            //    target_ci = sourceCopyItem;
            //}

            return target_ci;
        }


        private static async Task<MpCopyItem> ProcessNewContentItemAsync(
            MpPluginResponseFormatBase prf,
            MpCopyItem sourceCopyItem, 
            int citid,
            bool suppressWrite = false) {

            if (prf == null || prf.newContentItem == null) {
                return sourceCopyItem;
            }
            //var source = await MpDataModelProvider.GetItemAsync<MpSource>(transSourceId);\
            //var targetCopyItem = await MpCopyItem.Create(
            //    //sourceId: source.Id,
            //    title: title,
            //    data: prf.newContentItem.content.value,
            //    //copyItemSourceGuid: sourceCopyItem.Guid,
            //    itemType: targetCopyItemType,
            //    suppressWrite: suppressWrite);

            List<MpISourceRef> target_source_refs = new List<MpISourceRef>();
            if(citid > 0) {
                var cit_ref = await MpDataModelProvider.GetSourceRefByCopyItemTransactionIdAsync(citid);
                if (cit_ref != null) {
                    target_source_refs.Add(cit_ref);
                }
            }
            target_source_refs.Add(sourceCopyItem);
            
            string title = sourceCopyItem.Title + " Analysis";
            if (prf.newContentItem.label != null) {
                title = prf.newContentItem.label.value;
            }
            var nc_pdo = new MpPortableDataObject();
            nc_pdo.SetData(prf.newContentItem.format, prf.newContentItem.content.value);
            nc_pdo.SetData(MpPortableDataFormats.INTERNAL_CLIP_TILE_TITLE_FORMAT, title);
            var source_urls = target_source_refs.Select(x => MpPlatformWrapper.Services.SourceRefBuilder.ConvertToRefUrl(x));
            nc_pdo.SetData(MpPortableDataFormats.LinuxUriList, source_urls);
            //nc_pdo.SetData(MpPortableDataFormats.CefAsciiUrl, MpSourceRefHelper.ToUrlAsciiBytes(sourceCopyItem));

            var target_ci = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(nc_pdo);

            return target_ci;
        }

        private static async Task ProcessAnnotationsAsync(MpAnalyzerTransaction trans, MpCopyItem sourceCopyItem, int citid, bool suppressWrite = false) {
            if (trans == null || trans.Response == null) {
                return;
            }
            if (trans.Response is MpPluginResponseFormatBase prf && prf.annotations != null) {

                await Task.WhenAll(prf.annotations.Select(x => ProcesseAnnotationAsync(x, sourceCopyItem.Id, sourceCopyItem.ItemType, trans.RequestContent, citid, suppressWrite)));
            }

        }

        private static async Task ProcesseAnnotationAsync(
            MpPluginResponseAnnotationFormat a,
            int copyItemId,
            MpCopyItemType copyItemType,
            object reqContent,
            int citid,
            bool suppressWrite = false) {
            if (a == null) {
                return;
            }

            double score = 1;

            if (a.score != null) {
                score = a.score.value;
                if (a.minScore != 0 || a.maxScore != 1) {
                    //normalize scoring from 0-1
                    score = (a.maxScore - a.minScore) / a.score.value;
                }
            }

            string label = a.label == null ? string.Empty : a.label.value;

            if (copyItemType == MpCopyItemType.Image && a.box != null) {
                MpSize boxSize = new MpSize();
                if (a.box != null) {
                    //    var bmpSrc = reqContent.ToString().ToBitmapSource();
                    //    boxSize = new MpSize(bmpSrc.PixelWidth, bmpSrc.PixelHeight);
                    //} else {
                    boxSize.Width = a.box.width.value;
                    boxSize.Height = a.box.height.value;
                }

                MpImageAnnotation contentBox = await MpImageAnnotation.Create(
                   cid: copyItemId,
                   x: a.box == null ? 0 : a.box.x.value,
                   y: a.box == null ? 0 : a.box.y.value,
                   w: a.box == null ? 0 : a.box.width.value,
                   h: a.box == null ? 0 : a.box.height.value,
                   label: label,
                   c: score,
                   hexColor: a.appearance == null || a.appearance.foregroundColor == null ? null : a.appearance.foregroundColor.value,
                   suppressWrite: suppressWrite);
            } else if (copyItemType == MpCopyItemType.Text) {
                int sIdx = 0;
                int eIdx = reqContent.ToString().Length;

                if (a.range != null &&
                    a.range.rangeStart.value >= 0 &&
                    a.range.rangeStart.value < reqContent.ToString().Length &&
                    a.range.rangeStart.value + a.range.rangeLength.value >= 0 &&
                    a.range.rangeStart.value + a.range.rangeLength.value < reqContent.ToString().Length) {
                    // NOTE these range checks are more to cover up issue w/ reqContent not always having data...
                    sIdx = a.range.rangeStart.value;
                    eIdx = a.range.rangeLength.value;
                }
                var ta = await MpTextAnnotation.Create(
                        copyItemId: copyItemId,
                        copyItemTransId: citid,
                        matchValue: reqContent.ToString().Substring(sIdx, Math.Max(0, eIdx - sIdx - 1)),
                        label: label,
                        score: score,
                        suppressWrite: suppressWrite);
            }

            if (a.children != null) {
                await Task.WhenAll(a.children.Select(x => ProcesseAnnotationAsync(x, copyItemId, copyItemType, reqContent, citid, suppressWrite)));
            }
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

            if (mpdo.ContainsData(MpPortableDataFormats.LinuxUriList) &&
                mpdo.GetData(MpPortableDataFormats.LinuxUriList) is IEnumerable<string> urls) {
                var urlList = urls.ToList();
                if(urlList.Any(x=>x == source_url_ref)) {
                    // data references source item so any content formats should replace source item content
                    //target_ci = sourceCopyItem;
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
                mpdo.SetData(MpPortableDataFormats.LinuxUriList, new_contnet_source_urls);

                // create new item
                target_ci = await MpPlatformWrapper.Services.CopyItemBuilder.CreateAsync(mpdo);
                return target_ci;
            }  

            if (mpdo.ContainsData(MpPortableDataFormats.CefHtml) &&
                mpdo.GetData(MpPortableDataFormats.CefHtml) is string updatedItemData) {
                sourceCopyItem.ItemData = updatedItemData;
                await sourceCopyItem.WriteToDatabaseAsync();
            }

            return null;
        }
    }
}
