using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpPluginResponseConverter {
        public static async Task<MpCopyItem> Convert(
            MpAnalyzerTransaction trans,
            MpCopyItem sourceCopyItem,
            int transSourceId,
            bool suppressWrite) {
            MpCopyItem targetCopyItem = null;

            if (trans.Response is MpPluginResponseFormat prf) {
                targetCopyItem = await ProcessNewContentItem(prf, sourceCopyItem, transSourceId, suppressWrite);
            } else {
                targetCopyItem = sourceCopyItem;
            }
            await ProcessAnnotations(trans, targetCopyItem, transSourceId, suppressWrite);

            if (suppressWrite == false && targetCopyItem != null) {
                //create is suppressed when its part of a match expression
                if (sourceCopyItem.Id != targetCopyItem.Id) {
                    //var pci = await MpDb.GetItemAsync<MpCopyItem>(sourceCopyItem.Id);

                    //int parentSortOrderIdx = pci.CompositeSortOrderIdx;
                    //List<MpCopyItem> ppccil = null;

                    //if (pci.CompositeParentCopyItemId > 0) {
                    //    //when this items parent is a composite child, adjust fk/sort so theres single parent
                    //    var ppci = await MpDb.GetItemAsync<MpCopyItem>(pci.CompositeParentCopyItemId);
                    //    ppccil = await MpDataModelProvider.GetCompositeChildrenAsync(pci.CompositeParentCopyItemId);
                    //    ppccil.Insert(0, ppci);
                    //} else {
                    //    ppccil = await MpDataModelProvider.GetCompositeChildrenAsync(pci.Id);
                    //    ppccil.Insert(0, pci);
                    //}
                    //ppccil = ppccil.OrderBy(x => x.CompositeSortOrderIdx).ToList();
                    //for (int i = 0; i < ppccil.Count; i++) {
                    //    var cci = ppccil[i];
                    //    if (cci.Id == sourceCopyItem.Id) {
                    //        targetCopyItem.CompositeParentCopyItemId = sourceCopyItem.Id;
                    //        targetCopyItem.CompositeSortOrderIdx = i + 1;
                    //        await targetCopyItem.WriteToDatabaseAsync();
                    //    } else if (i > parentSortOrderIdx) {
                    //        ppccil[i].CompositeSortOrderIdx += 1;
                    //        await ppccil[i].WriteToDatabaseAsync();
                    //    }
                    //}
                }

                //var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(sourceCopyItem.Id);
                //if (scivm != null) {
                //    await MpHelpers.RunOnMainThreadAsync(async () => {
                //        //analysis content is  linked with visible item in tray
                //        await scivm.Parent.InitializeAsync(scivm.Parent.HeadItem.CopyItem, scivm.Parent.QueryOffsetIdx);
                //        MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
                //    });
                //}
                MpDataModelProvider.QueryInfo.NotifyQueryChanged(false);
            }

            if (targetCopyItem == null) {
                //this should only occur during an action sequece
                targetCopyItem = sourceCopyItem;
            }

            return targetCopyItem;
        }


        private static async Task<MpCopyItem> ProcessNewContentItem(
            MpPluginResponseFormat prf,
            MpCopyItem sourceCopyItem,
            int transSourceId,
            bool suppressWrite = false) {
            if (prf == null || prf.newContentItem == null) {
                return sourceCopyItem;
            }
            var source = await MpDb.GetItemAsync<MpSource>(transSourceId);

            string title = sourceCopyItem.Title + " Analysis";
            if(prf.newContentItem.label != null) {
                title = prf.newContentItem.label.value;
            }

            var targetCopyItem = await MpCopyItem.Create(
                sourceId: source.Id,
                title: title,
                data: prf.newContentItem.content.value,
                copyItemSourceGuid: sourceCopyItem.Guid,
                //itemType: MpCopyItemType.Text,
                suppressWrite: suppressWrite);

            return targetCopyItem;
        }

        private static async Task ProcessAnnotations(MpAnalyzerTransaction trans, MpCopyItem sourceCopyItem, int transSourceId, bool suppressWrite = false) {
            if (trans == null || trans.Response == null) {
                return;
            }
            if (trans.Response is MpPluginResponseFormat prf && prf.annotations != null) {

                await Task.WhenAll(prf.annotations.Select(x => ProcesseAnnotation(x, sourceCopyItem.Id, sourceCopyItem.ItemType, trans.RequestContent, transSourceId, suppressWrite)));
            }

        }

        private static async Task ProcesseAnnotation(
            MpPluginResponseAnnotationFormat a,
            int copyItemId,
            MpCopyItemType copyItemType,
            object reqContent,
            int transSourceId,
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
                        sourceId: transSourceId,
                        matchValue: reqContent.ToString().Substring(sIdx, Math.Max(0, eIdx - sIdx - 1)),
                        label: label,
                        score: score,
                        suppressWrite: suppressWrite);
            }

            if (a.children != null) {
                await Task.WhenAll(a.children.Select(x => ProcesseAnnotation(x, copyItemId, copyItemType, reqContent, transSourceId, suppressWrite)));
            }
        }
    }
}
