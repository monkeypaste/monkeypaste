
using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
//using Xamarin.Essentials;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvContentDropHelper {

        public static MpQuillHostDataItemsMessage ToQuillDataItemsMessage(this IDataObject avdo, DragDropEffects dde = DragDropEffects.None) {
            if (avdo == null) {
                avdo = new MpAvDataObject();
            }
            var dil = new List<MpQuillHostDataItemFragment>();
            foreach (var format in avdo.GetAllDataFormats()) {
                string data = null;
                if (avdo.Get(format) is byte[] bytes &&
                    bytes.ToBase64String() is string bytesStr) {
                    data = bytesStr;
                } else if (avdo.Get(format) is IEnumerable<string> strs &&
                    string.Join(Environment.NewLine, strs) is string strSet) {
                    data = strSet;
                } else {
                    data = avdo.Get(format).ToString();
                }
                if (data == null) {
                    continue;
                }
                dil.Add(new MpQuillHostDataItemFragment() { format = format, data = data });
            }

            return new MpQuillHostDataItemsMessage() {
                dataItems = dil,
                effectAllowed = dde.ToJsDropEffects()
            };
        }

        public static MpAvDataObject ToAvDataObject(this MpQuillHostDataItemsMessage hdim) {
            MpAvDataObject req_mpdo = null;
            if (hdim != null) {
                req_mpdo = new MpAvDataObject(hdim.dataItems.ToDictionary(x => x.format, x => (object)x.data));
            }
            return req_mpdo;
        }

        public static string ToDataFormat(this MpJsonMessageFormatType jmft) {
            switch (jmft) {
                case MpJsonMessageFormatType.Annotation:
                    return MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT;
                case MpJsonMessageFormatType.ParameterRequest:
                    return MpPortableDataFormats.INTERNAL_PARAMETER_REQUEST_FORMAT;
                case MpJsonMessageFormatType.Delta:
                    return MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT;
                case MpJsonMessageFormatType.DataObject:
                    return MpPortableDataFormats.INTERNAL_DATA_OBJECT_FORMAT;
                case MpJsonMessageFormatType.Error:
                default:
                    return MpPortableDataFormats.Text;
            }
        }
        public static string ToJsDropEffects(this DragDropEffects dde) {
            if (dde.HasFlag(DragDropEffects.Copy)) {
                return DragDropEffects.Copy.ToString().ToLower();
            }
            if (dde.HasFlag(DragDropEffects.Move)) {
                return DragDropEffects.Move.ToString().ToLower();
            }
            if (dde.HasFlag(DragDropEffects.Link)) {
                return DragDropEffects.Link.ToString().ToLower();
            }
            return DragDropEffects.None.ToString().ToLower();
        }

        public static MpQuillDragDropEventMessage SetJsModKeys(this KeyModifiers km, MpQuillDragDropEventMessage msg) {
            msg.ctrlKey = km.HasFlag(KeyModifiers.Control);
            msg.altKey = km.HasFlag(KeyModifiers.Alt);
            msg.shiftKey = km.HasFlag(KeyModifiers.Shift);
            //msg.escKey = km.HasFlag(KeyModifiers.);
            msg.metaKey = km.HasFlag(KeyModifiers.Meta);
            return msg;
        }
    }
}
