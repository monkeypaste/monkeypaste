using MonkeyPaste.Common;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpCopyItemExtensions {
        public static MpPortableDataObject ToPortableDataObject(
            this MpCopyItem ci,
            string[] formats = null,
            bool includeSelfRef = false,
            bool includeTitle = false) {
            if (ci == null) {
                return new MpPortableDataObject();
            }
            var pdo = new MpPortableDataObject();
            switch (ci.ItemType) {
                case MpCopyItemType.Text:
                    pdo.SetData(MpPortableDataFormats.CefHtml, ci.ItemData);
                    if (formats != null) {
                        if (formats.Any(x => x == MpPortableDataFormats.Text)) {
                            pdo.SetData(MpPortableDataFormats.Text, ci.ItemData.ToPlainText("html"));
                        }
                    }
                    break;
                case MpCopyItemType.Image:
                    pdo.SetData(MpPortableDataFormats.AvPNG, ci.ItemData.ToBytesFromBase64String());
                    break;
                case MpCopyItemType.FileList:
                    pdo.SetData(MpPortableDataFormats.AvFileNames, ci.ItemData.SplitNoEmpty(Environment.NewLine));
                    break;
            }
            if (includeSelfRef) {
                pdo.SetData(MpPortableDataFormats.CefAsciiUrl, Mp.Services.SourceRefTools.ToUrlAsciiBytes(ci));
            }
            if (includeTitle) {
                pdo.SetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, ci.Title);
            }
            return pdo;
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
            return new[] { MpFileIo.GetUniqueFileOrDirectoryName(dir, $"{ci.Title}{(isFragment ? "-Fragment" : string.Empty)}.{ext}") };
        }

    }
}
