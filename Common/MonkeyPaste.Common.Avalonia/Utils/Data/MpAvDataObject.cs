using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public class MpAvDataFormat : MpPortableDataFormat {
        public MpAvDataFormat(string name, int id, string portable) : base(name,id) { }
    }

    public class MpAvDataObject : MpPortableDataObject, IDataObject {
        public override void SetData(string format, object data) {
            if(data == null) {
                // will cause error for sometypes
                return;
            }
            if (format == MpPortableDataFormats.AvFileNames && data is string portablePathStr) {
                // convert portable single line-separated string to enumerable of strings for avalonia
                data = portablePathStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) as IEnumerable<string>;
            } else if ((format == MpPortableDataFormats.AvHtml_bytes || format == MpPortableDataFormats.AvRtf_bytes) && data is string portableDecodedFormattedTextStr) {
                // avalona like rtf and html to be stored as bytes
                data = portableDecodedFormattedTextStr.ToEncodedBytes();
            }
            base.SetData(format, data);
        }

        public void MapAllPseudoFormats() {
            // called after all available formats created to map cef types to avalonia and/or vice versa
            var html_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvHtml_bytes);
            var cefHtml_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefHtml);

            if(DataFormatLookup.ContainsKey(html_f) &&
                !DataFormatLookup.ContainsKey(cefHtml_f)) {
                // convert html bytes to string and map to cef html
                string htmlStr = (GetData(html_f.Name) as byte[]).ToDecodedString();
                SetData(cefHtml_f.Name,htmlStr);
            }
            if (DataFormatLookup.ContainsKey(cefHtml_f) &&
                !DataFormatLookup.ContainsKey(html_f)) {
                // convert html sring to to bytes
                byte[] htmlBytes = (GetData(cefHtml_f.Name) as string).ToEncodedBytes();
                SetData(html_f.Name, htmlBytes);
            }

            var text_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.Text);
            var cefText_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefText);

            if (DataFormatLookup.ContainsKey(text_f) &&
                !DataFormatLookup.ContainsKey(cefText_f)) {
                // ensure cef style text is in formats
                SetData(cefText_f.Name, GetData(text_f.Name));
            }
            if (DataFormatLookup.ContainsKey(cefHtml_f) &&
                !DataFormatLookup.ContainsKey(text_f)) {
                // ensure avalonia style text is in formats
                SetData(text_f.Name, GetData(cefText_f.Name));
            }

            // TODO should add unicode, oem, etc. here for greater compatibility
        }

        #region Avalonia.Input.IDataObject Implementation

        IEnumerable<string> IDataObject.GetDataFormats() {
            return DataFormatLookup.Select(x => x.Key.Name);
        }

        bool IDataObject.Contains(string dataFormat) {
            return ContainsData(dataFormat);
        }

        string IDataObject.GetText() { 
            return GetData(MpPortableDataFormats.Text) as string;
        }

        IEnumerable<string> IDataObject.GetFileNames() {

            if(GetData(MpPortableDataFormats.AvFileNames) is IEnumerable<string> files) {
                return files;
            }
           
            return null;
        }

        object IDataObject.Get(string dataFormat) {
            return GetData(dataFormat);
        }

        #endregion

    }
}
