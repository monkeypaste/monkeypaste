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
            // NOTE this wrapper just ensures formats are saved properly 
            // mapping is done after obj created so nothing is overwritten if it was populated
            // this ensures: 
            // 1. 'FileNames' is list of strings
            // 2. 'HTML Format' is stored as byte[] and 'text/html' 
            // 3. 'text/html' is stored as string

            if(data == null) {
                // will cause error for sometypes
                return;
            }
            if (format == MpPortableDataFormats.AvFileNames && 
                data is string portablePathStr) {
                // convert portable single line-separated string to enumerable of strings for avalonia
                data = portablePathStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries) as IEnumerable<string>;
            } else if ((format == MpPortableDataFormats.AvHtml_bytes || format == MpPortableDataFormats.AvRtf_bytes) && data is string portableDecodedFormattedTextStr) {
                // avalona like rtf and html to be stored as bytes
                data = portableDecodedFormattedTextStr.ToEncodedBytes();
            } else if(format == MpPortableDataFormats.CefHtml && data is byte[] html_bytes) {
                data = html_bytes.ToDecodedString();
            }
            base.SetData(format, data);
        }

        public void MapAllPseudoFormats() {
            // called after all available formats created to map cef types to avalonia and/or vice versa
            var html_bytes_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvHtml_bytes);
            var cefHtml_str_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefHtml);

            if(DataFormatLookup.ContainsKey(html_bytes_f) &&
                !DataFormatLookup.ContainsKey(cefHtml_str_f) &&
                GetData(html_bytes_f.Name) is byte[] html_bytes) {
                // convert html bytes to string and map to cef html
                string htmlStr = html_bytes.ToDecodedString();
                SetData(cefHtml_str_f.Name,htmlStr);
            }
            if (DataFormatLookup.ContainsKey(cefHtml_str_f) &&
                !DataFormatLookup.ContainsKey(html_bytes_f) &&
                GetData(cefHtml_str_f.Name) is string cef_html_str) {
                // convert html sring to to bytes
                byte[] htmlBytes = cef_html_str.ToEncodedBytes();
                SetData(html_bytes_f.Name, htmlBytes);
            }
            
            var text_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.Text);
            var cefText_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefText);

            if (DataFormatLookup.ContainsKey(text_f) &&
                !DataFormatLookup.ContainsKey(cefText_f)) {
                // ensure cef style text is in formats
                SetData(cefText_f.Name, GetData(text_f.Name));
            }
            if (DataFormatLookup.ContainsKey(cefHtml_str_f) &&
                !DataFormatLookup.ContainsKey(text_f)) {
                // ensure avalonia style text is in formats
                SetData(text_f.Name, GetData(cefText_f.Name));
            }

            if(OperatingSystem.IsLinux()) {
                // TODO this should only be for gnome based linux

                var av_fileNames_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvFileNames);
                var gnomeFiles_f = MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.LinuxGnomeFiles);

                if (DataFormatLookup.ContainsKey(av_fileNames_f) &&
                    !DataFormatLookup.ContainsKey(gnomeFiles_f) && 
                    GetData(av_fileNames_f.Name) is IEnumerable<string> files &&
                    string.Join(Environment.NewLine,files) is string av_files_str) {
                    // ensure cef style text is in formats
                    SetData(gnomeFiles_f.Name, av_files_str);
                }
                if (DataFormatLookup.ContainsKey(gnomeFiles_f) &&
                    !DataFormatLookup.ContainsKey(av_fileNames_f) &&
                    GetData(gnomeFiles_f.Name) is string gn_files_str &&
                    gn_files_str.Split(new string[]{Environment.NewLine},StringSplitOptions.RemoveEmptyEntries) is IEnumerable<string> gn_files
                    ) {
                    // ensure avalonia style text is in formats
                    SetData(av_fileNames_f.Name, gn_files);
                }
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
