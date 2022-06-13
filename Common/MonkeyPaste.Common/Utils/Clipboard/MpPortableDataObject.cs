using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public enum MpClipboardFormatType {
        None = 0,
        Text,
        Html,
        Rtf,
        Bitmap,
        FileDrop,
        Csv,
        InternalContent,
        UnicodeText,
        OemText,
        Custom //when format name doesn't resolve to any previous
    }

    public interface MpIPlatformDataObjectRegistrar {
        int RegisterFormat(string format);
    }
    
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

        //private static ObservableCollection<MpClipboardFormatType> _supportedFormats;
        //public static ObservableCollection<MpClipboardFormatType> SupportedFormats {
        //    get {
        //        if(_supportedFormats == null) {
        //            _supportedFormats = new ObservableCollection<MpClipboardFormatType>() {
        //                MpClipboardFormatType.Text,
        //                MpClipboardFormatType.Html,
        //                MpClipboardFormatType.Rtf,
        //                MpClipboardFormatType.Bitmap,
        //                MpClipboardFormatType.FileDrop,
        //                MpClipboardFormatType.Csv,
        //                MpClipboardFormatType.InternalContent,
        //                MpClipboardFormatType.UnicodeText,
        //                MpClipboardFormatType.OemText,
        //                MpClipboardFormatType.Custom
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

        //public Dictionary<MpClipboardFormatType,string> DataFormatLookup { get; set; } = new Dictionary<MpClipboardFormatType, string>();

        //public object GetCustomData(string customDataFormatName) {
        //    int cdfIdx = GetCustomDataFormatId(customDataFormatName);
        //    if(cdfIdx < 0) {
        //        return null;
        //    }
        //    return CustomDataLookup[cdfIdx];
        //}

        //public void SetCustomData(string customDataFormatName, object customData) {
        //    if (!DataFormatLookup.ContainsKey(MpClipboardFormatType.Custom)) {
        //        DataFormatLookup.Add(MpClipboardFormatType.Custom, customDataFormatName);
        //        CustomDataLookup.Add(customData);
        //        return;

        //    }
        //    int cdfIdx = GetCustomDataFormatId(customDataFormatName);
        //    if (cdfIdx < 0) {
        //        DataFormatLookup[MpClipboardFormatType.Custom] += "," + customDataFormatName;
        //        CustomDataLookup.Add(customData);
        //        return;
        //    }
        //    CustomDataLookup[cdfIdx] = customData;
        //}

        //private int GetCustomDataFormatId(string customDataFormatName) {
        //    if (!DataFormatLookup.ContainsKey(MpClipboardFormatType.Custom)) {
        //        return -1;
        //    }
        //    var availableCustomFormats = DataFormatLookup[MpClipboardFormatType.Custom].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //    return availableCustomFormats.IndexOf(customDataFormatName);
        //}
        #endregion

        public Dictionary<MpPortableDataFormat, object> DataFormatLookup { get; private set; } = new Dictionary<MpPortableDataFormat, object>();

        public bool ContainsData(string format) {
            return GetData(format) != null;
        }

        public object GetData(string format) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if(pdf == null) {
                return null;
            }
            DataFormatLookup.TryGetValue(pdf, out object data);
            return data;
        }

        public void SetData(string format, object data) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if(pdf == null) {
                throw new Exception($"Format {format} is not registered");
            }
            DataFormatLookup.AddOrReplace(pdf, data);
        }

        public MpPortableDataObject() {
            DataFormatLookup = new Dictionary<MpPortableDataFormat, object>();
        }
        public MpPortableDataObject(string format, object data) : this() {
            SetData(format, data);
        }

    }
    public static class MpPortableDataFormats {
        private static MpIPlatformDataObjectRegistrar _registrar;

        private static string[] _defaultFormatNames = new string[] {
            Text,
            Rtf,
            Bitmap,
            Html,
            FileDrop,
            Csv,
            Unicode,
            OemText,
            //InternalContent
        };

        private static Dictionary<int, MpPortableDataFormat> _formatLookup;

        public const string Text = "Text";
        public const string Rtf = "Rich Text Format";
        public const string Bitmap = "Bitmap";
        public const string Html = "HTML Format";
        public const string FileDrop = "FileDrop";
        public const string Csv = "CSV";
        public const string Unicode = "Unicode";
        public const string OemText = "OEMText";

        public const string InternalContent = "Mp Internal Content";


        public static string[] Formats => _formatLookup.Select(x => x.Value.Name).ToArray();

        public static void Init(MpIPlatformDataObjectRegistrar registrar) {
            _registrar = registrar;

            _formatLookup = new Dictionary<int, MpPortableDataFormat>();

            foreach(string formatName in _defaultFormatNames) {
                RegisterDataFormat(formatName);
            }
        }
        public static MpPortableDataFormat GetDataFormat(int id) {
            if (id < 0 || id >= _formatLookup.Count) {
                return null;
            }
            return _formatLookup.ToList()[id].Value;
        }

        public static MpPortableDataFormat GetDataFormat(string format) {
            int id = GetDataFormatId(format);
            if (id < 0) {
                return null;
            }
            _formatLookup.TryGetValue(id, out MpPortableDataFormat dataFormat);
            return dataFormat;
        }

        public static MpPortableDataFormat RegisterDataFormat(string format) {
            int id = GetDataFormatId(format);
            if (id >= 0) {
                return _formatLookup[id];
            }

            var pdf = new MpPortableDataFormat(format, _registrar.RegisterFormat(format));
            _formatLookup.Add(pdf.Id, pdf);
            return pdf;
        }

        public static int GetDataFormatId(string format) {
            foreach(var kvp in _formatLookup) {
                if(kvp.Value.Name == format) {
                    return kvp.Key;
                }
            }
            return -1;
        }
    }


    public class MpPortableDataFormat {
        public string Name { get; private set; }

        public int Id { get; private set; }

        public MpPortableDataFormat(string name, int id) {
            Name = name;
            Id = id;
        }
    }
}
