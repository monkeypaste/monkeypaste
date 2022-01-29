using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonkeyPaste {
    public class MpDataObject  {
        #region Private Variables

        private static readonly string[] _defaultDataFormats = {
                //"UnicodeText",
                "Text",
                "Html",
                "Rtf",
                "Bitmap",
                "FileDrop",
                "Csv"
            };

        #endregion

        #region Properties

        public static ObservableCollection<string> SupportedFormats { get; private set; } = new ObservableCollection<string>(_defaultDataFormats);

        public Dictionary<string,string> DataFormatLookup { get; set; } = new Dictionary<string, string>();

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
            foreach (string format in SupportedFormats) {
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
            var dfl = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonDict);
            if(dfl == null) {
                MpConsole.WriteTraceLine(@"Warning parsed empty data object");
                dfl = new Dictionary<string, string>();
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
