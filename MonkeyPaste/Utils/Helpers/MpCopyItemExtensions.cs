using MonkeyPaste.Common;
using System;

namespace MonkeyPaste {
    public static class MpCopyItemExtensions {
        public static MpPortableDataObject ToPortableDataObject(this MpCopyItem ci, bool includeRef = false, bool includeTitle = false) {
            if (ci == null) {
                return new MpPortableDataObject();
            }
            var pdo = new MpPortableDataObject();
            switch (ci.ItemType) {
                case MpCopyItemType.Text:
                    pdo.SetData(MpPortableDataFormats.CefHtml, ci.ItemData);
                    break;
                case MpCopyItemType.Image:
                    pdo.SetData(MpPortableDataFormats.AvPNG, ci.ItemData.ToBytesFromBase64String());
                    break;
                case MpCopyItemType.FileList:
                    pdo.SetData(MpPortableDataFormats.AvFileNames, ci.ItemData.SplitNoEmpty(Environment.NewLine));
                    break;
            }
            if (includeRef) {
                pdo.SetData(MpPortableDataFormats.CefAsciiUrl, Mp.Services.SourceRefBuilder.ToUrlAsciiBytes(ci));
            }
            if (includeTitle) {
                pdo.SetData(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, ci.Title);
            }
            return pdo;
        }
    }
}
