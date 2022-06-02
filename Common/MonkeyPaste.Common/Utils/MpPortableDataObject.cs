using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Plugin {

    public interface MpIClipboardMonitor {
        event EventHandler<MpPortableDataObject> OnClipboardChanged;
        
        bool IgnoreNextClipboardChangeEvent { get; set; }

        void StartMonitor();
        void StopMonitor();
    }

    public interface MpIPlatformDataObjectHelper {
        MpPortableDataObject ConvertToSupportedPortableFormats(object nativeDataObj, int retryCount = 5);
        object ConvertToPlatformClipboardDataObject(MpPortableDataObject portableObj);
        void SetPlatformClipboard(MpPortableDataObject portableObj, bool ignoreClipboardChange);
        MpPortableDataObject GetPlatformClipboardDataObject();
    }

    public interface MpIPortableContentDataObject {
        Task<MpPortableDataObject> ConvertToPortableDataObject(
            bool isDragDrop,
            object targetHandleOrProcessInfo,
            bool ignoreSubSelection = false,
            bool isDropping = false);
    }
    public interface MpIExternalPasteHandler {
        Task PasteDataObject(MpPortableDataObject mpdo, object handleOrProcessInfo, bool finishWithEnterKey = false);
    }


    public class MpPortableDataObject {
        #region Properties
        public Dictionary<MpPortableDataFormat, string> DataFormatLookup { get; set; } = new Dictionary<MpPortableDataFormat, string>();


        //public static string InternalContentFormat = "MpInternalContentFormat";

        //private static ObservableCollection<MpPortableDataFormat> _supportedFormats;
        //public static ObservableCollection<MpPortableDataFormat> SupportedFormats {
        //    get {
        //        if(_supportedFormats == null) {
        //            _supportedFormats = new ObservableCollection<MpPortableDataFormat>() {
        //                MpPortableDataFormat.Text,
        //                MpPortableDataFormat.Html,
        //                MpPortableDataFormat.Rtf,
        //                MpPortableDataFormat.Bitmap,
        //                MpPortableDataFormat.FileDrop,
        //                MpPortableDataFormat.Csv,
        //                MpPortableDataFormat.InternalContent,
        //                MpPortableDataFormat.UnicodeText,
        //                MpPortableDataFormat.OemText,
        //                MpPortableDataFormat.Custom
        //            };
        //        }
        //        return _supportedFormats;
        //    }
        //    set {
        //        _supportedFormats = value;
        //    }
        //}

        //private static List<object> _customDataLookup;
        //public static List<object> CustomDataLookup {
        //    get {
        //        if (_customDataLookup == null) {
        //            _customDataLookup = new List<object>();
        //        }
        //        return _customDataLookup;
        //    }
        //}        

        //public object GetCustomData(string customDataFormatName) {
        //    int cdfIdx = GetCustomDataFormatId(customDataFormatName);
        //    if(cdfIdx < 0) {
        //        return null;
        //    }
        //    return CustomDataLookup[cdfIdx];
        //}

        //public void SetCustomData(string customDataFormatName, object customData) {
        //    if (!DataFormatLookup.ContainsKey(MpPortableDataFormat.Custom)) {
        //        DataFormatLookup.Add(MpPortableDataFormat.Custom, customDataFormatName);
        //        CustomDataLookup.Add(customData);
        //        return;

        //    }
        //    int cdfIdx = GetCustomDataFormatId(customDataFormatName);
        //    if (cdfIdx < 0) {
        //        DataFormatLookup[MpPortableDataFormat.Custom] += "," + customDataFormatName;
        //        CustomDataLookup.Add(customData);
        //        return;
        //    }
        //    CustomDataLookup[cdfIdx] = customData;
        //}

        //private int GetCustomDataFormatId(string customDataFormatName) {
        //    if (!DataFormatLookup.ContainsKey(MpPortableDataFormat.Custom)) {
        //        return -1;
        //    }
        //    var availableCustomFormats = DataFormatLookup[MpPortableDataFormat.Custom].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    return availableCustomFormats.IndexOf(customDataFormatName);
        //}
        #endregion

    }

    public static class MpPortableDataFormats {
        private static Dictionary<int, MpPortableDataFormat> _formatLookup;

        public static readonly string Text = "Text";
        public static readonly string Rtf = "Rich Text Format";
        public static readonly string Bitmap = "Bitmap";
        public static readonly string Html = "HTML Format";
        public static readonly string FileDrop = "FileDrop";
        public static readonly string Csv = "CSV";
        public static readonly string Unicode = "Unicode";
        public static readonly string OemText = "OEMText";

        public static void Init() {
            _formatLookup = new Dictionary<int, MpPortableDataFormat>();
            _formatLookup.Add(0, new MpPortableDataFormat(Text, 0));
            _formatLookup.Add(1, new MpPortableDataFormat(Rtf, 1));
            _formatLookup.Add(2, new MpPortableDataFormat(Bitmap, 2));
            _formatLookup.Add(3, new MpPortableDataFormat(Html, 3));
            _formatLookup.Add(4, new MpPortableDataFormat(FileDrop, 4));
            _formatLookup.Add(5, new MpPortableDataFormat(Csv, 5));
            _formatLookup.Add(6, new MpPortableDataFormat(Unicode, 6));
            _formatLookup.Add(7, new MpPortableDataFormat(OemText, 7));
        }
        public static MpPortableDataFormat GetDataFormat(int id) {
            if(id < 0 || id >= _formatLookup.Count) {
                return null;
            }
            return _formatLookup.ToList()[id].Value;
        }

        public static MpPortableDataFormat GetDataFormat(string format) {
            int id = GetDataFormatId(format);
            if(id < 0) {
                return null;
            }
            _formatLookup.TryGetValue(id, out MpPortableDataFormat dataFormat);
            return dataFormat;
        }

        public static MpPortableDataFormat RegisterDataFormat(string format) {
            int id = GetDataFormatId(format);
            if(id >= 0) {
                return _formatLookup[id];
            }
            _formatLookup.Add(_formatLookup.Count, new MpPortableDataFormat(format, _formatLookup.Count));
            id = GetDataFormatId(format);
            if (id >= 0) {
                return _formatLookup[id];
            }
            return null;
        }

        private static int GetDataFormatId(string format) {
            var format_kvp = _formatLookup.FirstOrDefault(x => x.Value.Name == format);
            return format_kvp.Key;
        }
    }


    public class MpPortableDataFormat {
        private string _name;
        public string Name => _name;

        private int _id;
        public int Id => _id;
        public MpPortableDataFormat(string name, int id) {
            _name = name;
            _id = id;
        }
    }

}
