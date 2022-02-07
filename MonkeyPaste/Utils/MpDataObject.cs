using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste {
    public enum MpClipboardFormat {
        None = 0,
        Text,
        Html,
        Rtf,
        Bitmap,
        FileDrop,
        Csv,
        UnicodeText
    }

    public class MpDataObject  {
        #region Private Variables


        #endregion

        #region Properties

        private static ObservableCollection<MpClipboardFormat> _supportedFormats;
        public static ObservableCollection<MpClipboardFormat> SupportedFormats {
            get {
                if(_supportedFormats == null) {
                    _supportedFormats = new ObservableCollection<MpClipboardFormat>() {
                        MpClipboardFormat.Text,
                        MpClipboardFormat.Html,
                        MpClipboardFormat.Rtf,
                        MpClipboardFormat.Bitmap,
                        MpClipboardFormat.FileDrop,
                        MpClipboardFormat.Csv,
                        MpClipboardFormat.UnicodeText
                    };
                }
                return _supportedFormats;
            }
            set {
                _supportedFormats = value;
            }
        }

        public Dictionary<MpClipboardFormat,string> DataFormatLookup { get; set; } = new Dictionary<MpClipboardFormat, string>();

        #endregion

        #region Builders

        public static MpDataObject Create(
            IList<MpCopyItem> cil, 
            MpIPasteObjectBuilder pasteBuilder,
            string[] fileNameWithoutExtension = null,
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false,
            bool isCopy = false) {
            var dobj = new MpDataObject();
            foreach (var format in SupportedFormats) {
                dobj.DataFormatLookup.Add(format,
                    pasteBuilder.GetFormat(
                    format: format,
                    data: cil.Select(x=>x.ItemData).ToArray(),
                    fileNameWithoutExtension: fileNameWithoutExtension == null ?
                                                cil.Select(x => x.Title).ToArray() : fileNameWithoutExtension,
                    directory: directory,
                    textFormat: textFormat,
                    imageFormat: imageFormat,
                    isTemporary: isTemporary));
            }
            return dobj;
        }

        public static MpDataObject Parse(string jsonDict) {
            var dfl = JsonConvert.DeserializeObject<Dictionary<MpClipboardFormat, string>>(jsonDict);
            if(dfl == null) {
                MpConsole.WriteTraceLine(@"Warning parsed empty data object");
                dfl = new Dictionary<MpClipboardFormat, string>();
            }
            return new MpDataObject() {
                DataFormatLookup = dfl
            };
        }

        #endregion

        #region Public Methods

        public string ToJson() {
            return JsonConvert.SerializeObject(DataFormatLookup);
        }

        #endregion
    }
}
