using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpCopyItemExtensions {
        public static MpAvDataObject ToAvDataObject(
            this MpCopyItem ci,
            bool includeSelfRef = false,
            bool includeTitle = false) {
            if (ci == null) {
                return new MpAvDataObject();
            }
            var avdo = new MpAvDataObject();
            switch (ci.ItemType) {
                case MpCopyItemType.Text:
                    avdo.SetData(MpPortableDataFormats.CefHtml, ci.ItemData);
                    avdo.SetData(MpPortableDataFormats.Text, ci.ItemData.ToPlainText("html"));
                    break;
                case MpCopyItemType.Image:
                    avdo.SetData(MpPortableDataFormats.AvPNG, ci.ItemData.ToBytesFromBase64String());
                    break;
                case MpCopyItemType.FileList:
                    avdo.SetData(MpPortableDataFormats.AvFileNames, ci.ItemData.SplitNoEmpty(Environment.NewLine));
                    break;
            }
            if (includeSelfRef) {
                avdo.SetData(MpPortableDataFormats.CefAsciiUrl, Mp.Services.SourceRefTools.ToUrlAsciiBytes(ci));
            }
            if (includeTitle) {
                avdo.SetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, ci.Title);
            }
            return avdo;
        }

        public static string[] GetDefaultFilePaths(this MpCopyItem ci, string dir = "", string ext = "", bool isFragment = false) {
            switch (ci.ItemType) {
                case MpCopyItemType.Text:
                    ext = string.IsNullOrEmpty(ext) ? "txt" : ext;
                    break;
                case MpCopyItemType.Image:
                    ext = string.IsNullOrEmpty(ext) ? "png" : ext;
                    break;

                default:
                case MpCopyItemType.FileList:
                    return ci.ItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter);
            }
            dir = string.IsNullOrEmpty(dir) ? Path.GetTempPath() : dir;
            return new[] { MpFileIo.GetUniqueFileOrDirectoryName(dir, $"{ci.Title}{(isFragment ? "-Part" : string.Empty)}.{ext}") };
        }

        public static MpQuillDelta ToDelta(this MpCopyItem ci) {
            // DO NOT DELETE! part of omitted transaction change-tracking 
            MpQuillDelta deltaObj = new MpQuillDelta() {
                ops = new List<Op>()
            };

            switch (ci.ItemType) {
                case MpCopyItemType.Text:
                    // NOTE no formatting here, could re-use html->rtf converter strutcutre to go delta
                    deltaObj.ops.Add(new Op() { insert = ci.ItemData.ToPlainText() });
                    break;
                case MpCopyItemType.Image:
                    deltaObj.ops.Add(new Op() {
                        insert = new ImageInsert() { image = $"data:image/png;base64,{ci.ItemData}" },
                        attributes = new Attributes() { align = "center" }
                    });
                    break;
                case MpCopyItemType.FileList:
                    deltaObj.ops.Add(new Op() {
                        insert = ci.ItemData
                    });

                    break;
            }
            return deltaObj;
        }

        public static string ToDefaultDataFormat(this MpCopyItemType itemType) {
            switch (itemType) {
                case MpCopyItemType.Text:
                    return MpPortableDataFormats.Text;
                case MpCopyItemType.Image:
                    return MpPortableDataFormats.AvPNG;
                case MpCopyItemType.FileList:
                    return MpPortableDataFormats.AvFileNames;
            }
            return string.Empty;
        }
    }
}
