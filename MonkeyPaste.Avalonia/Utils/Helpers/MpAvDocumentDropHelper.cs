
using Avalonia.Input;
using MonkeyPaste.Common;
using Avalonia.Threading;
using System.Threading.Tasks;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Controls;
using System.Diagnostics;
using Gtk;
using Avalonia.Interactivity;
using Xamarin.Essentials;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDocumentDropHelper {

        public static MpQuillHostDataItemsMessageFragment ToDataItemFragment(this IDataObject avdo, DragDropEffects dde = DragDropEffects.None) {
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

            return new MpQuillHostDataItemsMessageFragment() {
                dataItems = dil,
                effectAllowed = dde.ToJsDropEffects()
            };
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
