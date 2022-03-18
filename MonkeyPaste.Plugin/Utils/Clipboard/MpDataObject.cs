using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {
    public enum MpClipboardFormatType {
        None = 0,
        Text,
        Html,
        Rtf,
        Bitmap,
        FileDrop,
        Csv
    }

    public interface MpIPasteObjectBuilder {
        string GetFormat(
            MpClipboardFormatType format,
            string data,
            string fileNameWithoutExtension = "",
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false);

        string GetFormat(
            MpClipboardFormatType format,
            string[] data,
            string[] fileNameWithoutExtension = null,
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false,
            bool isCopy = false);

    }
    
    public interface MpIClipboardMonitor {
        event EventHandler<MpDataObject> OnClipboardChanged;
        
        bool IgnoreClipboardChangeEvent { get; set; }

        void StartMonitor();
        void StopMonitor();
    }

    public interface MpIClipboardInterop {
        MpDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5);
        object ConvertToNativeFormat(MpDataObject portableObj);
        void SetDataObjectWrapper(MpDataObject portableObj);
    }

    public interface MpIExternalPasteHandler {
        Task PasteDataObject(MpDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false);
    }


    public class MpDataObject  {
        #region Private Variables
        private static MpIPasteObjectBuilder _pasteObjectBuilder;
        #endregion

        #region Properties

        private static ObservableCollection<MpClipboardFormatType> _supportedFormats;
        public static ObservableCollection<MpClipboardFormatType> SupportedFormats {
            get {
                if(_supportedFormats == null) {
                    _supportedFormats = new ObservableCollection<MpClipboardFormatType>() {
                        MpClipboardFormatType.Text,
                        MpClipboardFormatType.Html,
                        MpClipboardFormatType.Rtf,
                        MpClipboardFormatType.Bitmap,
                        MpClipboardFormatType.FileDrop,
                        MpClipboardFormatType.Csv
                    };
                }
                return _supportedFormats;
            }
            set {
                _supportedFormats = value;
            }
        }

        public Dictionary<MpClipboardFormatType,string> DataFormatLookup { get; set; } = new Dictionary<MpClipboardFormatType, string>();

        #endregion

        #region Constructors

        public static void Init(MpIPasteObjectBuilder pob) {
            _pasteObjectBuilder = pob;
        }
        #endregion

        #region Builders

        public static MpDataObject Create(
            string data = "",
            string fileNameWithoutExtension = null,
            string directory = "",
            string textFormat = ".rtf",
            string imageFormat = ".png",
            bool isTemporary = false,
            bool isCopy = false,
            List<MpClipboardFormatType> formats = null) {
            formats = formats == null ? SupportedFormats.ToList() : formats;
            var dobj = new MpDataObject();
            foreach (var format in formats) {
                dobj.DataFormatLookup.Add(format,
                    _pasteObjectBuilder.GetFormat(
                        format: format,
                        data: data,
                        fileNameWithoutExtension: fileNameWithoutExtension,
                        directory: directory,
                        textFormat: textFormat,
                        imageFormat: imageFormat,
                        isTemporary: isTemporary));
            }
            return dobj;
        }

        //public static MpDataObject Create(
        //    IList<string> datas,
        //    IList<string> titles,
        //    MpIPasteObjectBuilder pasteBuilder,
        //    string[] fileNamesWithoutExtension = null,
        //    string directory = "",
        //    string textFormat = ".rtf",
        //    string imageFormat = ".png",
        //    bool isTemporary = false,
        //    bool isCopy = false) {
        //    var dobj = new MpDataObject();
        //    foreach (var format in SupportedFormats) {
        //        dobj.DataFormatLookup.Add(format,
        //            pasteBuilder.GetFormat(
        //            format: format,
        //            data: datas.ToArray(),
        //            fileNameWithoutExtension: fileNamesWithoutExtension == null ?
        //                                        titles.ToArray() : fileNamesWithoutExtension,
        //            directory: directory,
        //            textFormat: textFormat,
        //            imageFormat: imageFormat,
        //            isTemporary: isTemporary));
        //    }
        //    return dobj;
        //}

        public static MpDataObject Parse(string jsonDict) {
            var dfl = JsonConvert.DeserializeObject<Dictionary<MpClipboardFormatType, string>>(jsonDict);
            if(dfl == null) {
                MpConsole.WriteTraceLine(@"Warning parsed empty data object");
                dfl = new Dictionary<MpClipboardFormatType, string>();
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
