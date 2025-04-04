﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpCopyItemExtensions {
        public static MpAvDataObject ToAvDataObject(
            this MpCopyItem ci,
            bool includeSelfRef = false,
            bool includeTitle = false,
            string[] forceFormats = null) {
            var avdo = new MpAvDataObject();
            avdo.SetDataObjectSourceType(MpDataObjectSourceType.ClipTileClone);
            if (ci == null) {
                return avdo;
            }
            if (forceFormats == null) {
                switch (ci.ItemType) {
                    case MpCopyItemType.Text:
                        avdo.SetData(MpPortableDataFormats.Html, ci.ItemData);
                        avdo.SetData(MpPortableDataFormats.Text, ci.ItemData.ToPlainText("html"));
                        break;
                    case MpCopyItemType.Image:
                        avdo.SetData(MpPortableDataFormats.Image, ci.ItemData.ToBytesFromBase64String());
                        break;
                    case MpCopyItemType.FileList:
                        avdo.SetData(MpPortableDataFormats.Files, ci.ItemData.SplitNoEmpty(Environment.NewLine));
                        break;
                }
            } else {
                foreach (var format in forceFormats) {
                    object data = null;
                    switch (ci.ItemType) {
                        case MpCopyItemType.Text:
                            switch (format) {
                                case var _ when format == MpPortableDataFormats.Xhtml:
                                    data = ci.ItemData.ToBase64String();
                                    break;
                                case var _ when format == MpPortableDataFormats.Html:
                                    data = ci.ItemData;
                                    break;
                                case var _ when format == MpPortableDataFormats.Text:
                                    data = ci.ItemData.ToPlainText("html");
                                    break;
                                case var _ when format == MpPortableDataFormats.Image:
                                    data = ci.ItemData.ToHtmlImageDoc();
                                    break;
                                case var _ when format == MpPortableDataFormats.Files:
                                    data = ci.ItemData.ToFile(forcePath: ci.GetDefaultFilePaths().FirstOrDefault());
                                    break;
                            }
                            break;
                        case MpCopyItemType.Image:
                            switch (format) {
                                case var _ when format == MpPortableDataFormats.Html:
                                    data = ci.ItemData.ToHtmlImageDoc();
                                    break;
                                //case MpPortableDataFormats.Text:
                                //    data = ci.ItemData.ToAvBitmap().ToAsciiImage();
                                //    break;
                                case var _ when format == MpPortableDataFormats.Files:
                                    data = ci.ItemData.ToFile(forcePath: ci.GetDefaultFilePaths().FirstOrDefault());
                                    break;
                            }
                            break;
                        case MpCopyItemType.FileList:
                            switch (format) {
                                case var _ when format == MpPortableDataFormats.Text:
                                    data = ci.ItemData;
                                    break;
                                case var _ when format == MpPortableDataFormats.Files:
                                    data = ci.ItemData.SplitNoEmpty(Environment.NewLine);
                                    break;

                                case var _ when format == MpPortableDataFormats.Image:
                                    data = ci.ItemData.ToHtmlImageDoc();
                                    break;
                            }
                            break;
                    }
                    if (data == null) {
                        continue;
                    }
                    avdo.SetData(format, data);
                }
            }
            if (includeSelfRef) {
                avdo.AddContentReferences(ci, true);
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
            dir = string.IsNullOrEmpty(dir) ? MpFileIo.GetThisAppRandomTempDir() : dir;
            return new[] { MpFileIo.GetUniqueFileOrDirectoryPath(dir, $"{ci.Title}{(isFragment ? "-Part" : string.Empty)}.{ext}") };
        }

        public static MpQuillFileListDataFragment ToFileListDataFragmentMessage(this MpCopyItem ci) {
            MpDebug.Assert(ci.ItemType == MpCopyItemType.FileList, $"FileListDataFragment error. Item type should be '{MpCopyItemType.FileList}' but was '{ci.ItemType}'");
            if (ci.ItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter) is not string[] fpl) {
                return new MpQuillFileListDataFragment();
            }
            var msg = new MpQuillFileListDataFragment() {
                fileItems = fpl.Select(x => new MpQuillFileListItemDataFragmentMessage() {
                    filePath = x,
                    fileIconBase64 =
                        MpAvStringFileOrFolderPathToBitmapConverter.Instance
                        .Convert(x, typeof(string), null, CultureInfo.CurrentCulture) as string
                }).ToList()
            };
            return msg;
        }
        public static MpQuillDelta ToDelta(this MpCopyItem ci) {
            // DO NOT DELETE! part of omitted transaction change-tracking 
            MpQuillDelta deltaObj = new MpQuillDelta() {
                ops = new List<MpQuillOp>()
            };

            switch (ci.ItemType) {
                case MpCopyItemType.Text:
                    // NOTE no formatting here, could re-use html->rtf converter strutcutre to go delta
                    deltaObj.ops.Add(new MpQuillOp() { insert = ci.ItemData.ToPlainText() });
                    break;
                case MpCopyItemType.Image:
                    deltaObj.ops.Add(new MpQuillOp() {
                        insert = new MpQuillImageInsert() { image = $"data:image/png;base64,{ci.ItemData}" },
                        attributes = new MpQuillAttributes() { align = "center" }
                    });
                    break;
                case MpCopyItemType.FileList:
                    deltaObj.ops.Add(new MpQuillOp() {
                        insert = ci.ItemData
                    });

                    break;
            }
            return deltaObj;
        }

        public static string ToCompatibilityDataFormat(this MpCopyItemType itemType) {
            switch (itemType) {
                case MpCopyItemType.Text:
                    return MpPortableDataFormats.Text;
                case MpCopyItemType.Image:
                    return MpPortableDataFormats.Image;
                case MpCopyItemType.FileList:
                    return MpPortableDataFormats.Files;
            }
            return string.Empty;
        }

        public static MpCopyItemType ToCopyItemType(this MpDataFormatType dft) {
            switch (dft) {
                case MpDataFormatType.PlainText:
                case MpDataFormatType.Html:
                case MpDataFormatType.HtmlFragment:
                case MpDataFormatType.Rtf:
                case MpDataFormatType.Csv:
                case MpDataFormatType.DeltaJson:
                case MpDataFormatType.Rtf2Html:
                case MpDataFormatType.VsRtf2Html:
                    return MpCopyItemType.Text;
                case MpDataFormatType.Png:
                case MpDataFormatType.Tiff:
                case MpDataFormatType.Jpg:
                case MpDataFormatType.Gif:
                case MpDataFormatType.Bmp:
                    return MpCopyItemType.Image;
                case MpDataFormatType.FileList:
                    return MpCopyItemType.FileList;
                default:
                    return MpCopyItemType.None;
            }
        }
    }
}
