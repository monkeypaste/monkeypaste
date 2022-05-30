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
        Csv,
        InternalContent,
        UnicodeText,
        OemText,
        Custom //when format name doesn't resolve to any previous
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

        public static string InternalContentFormat = "MpInternalContentFormat";

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
                        MpClipboardFormatType.Csv,
                        MpClipboardFormatType.InternalContent,
                        MpClipboardFormatType.UnicodeText,
                        MpClipboardFormatType.OemText,
                        MpClipboardFormatType.Custom
                    };
                }
                return _supportedFormats;
            }
            set {
                _supportedFormats = value;
            }
        }

        private static List<object> _customDataLookup;
        public static List<object> CustomDataLookup {
            get {
                if (_customDataLookup == null) {
                    _customDataLookup = new List<object>();
                }
                return _customDataLookup;
            }
        }        

        public Dictionary<MpClipboardFormatType,string> DataFormatLookup { get; set; } = new Dictionary<MpClipboardFormatType, string>();

        public object GetCustomData(string customDataFormatName) {
            int cdfIdx = GetCustomDataFormatId(customDataFormatName);
            if(cdfIdx < 0) {
                return null;
            }
            return CustomDataLookup[cdfIdx];
        }

        public void SetCustomData(string customDataFormatName, object customData) {
            if (!DataFormatLookup.ContainsKey(MpClipboardFormatType.Custom)) {
                DataFormatLookup.Add(MpClipboardFormatType.Custom, customDataFormatName);
                CustomDataLookup.Add(customData);
                return;

            }
            int cdfIdx = GetCustomDataFormatId(customDataFormatName);
            if (cdfIdx < 0) {
                DataFormatLookup[MpClipboardFormatType.Custom] += "," + customDataFormatName;
                CustomDataLookup.Add(customData);
                return;
            }
            CustomDataLookup[cdfIdx] = customData;
        }

        private int GetCustomDataFormatId(string customDataFormatName) {
            if (!DataFormatLookup.ContainsKey(MpClipboardFormatType.Custom)) {
                return -1;
            }
            var availableCustomFormats = DataFormatLookup[MpClipboardFormatType.Custom].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return availableCustomFormats.IndexOf(customDataFormatName);
        }
        #endregion

    }

    public class MpPortableDataFormats {
        private static Dictionary<int, MpPortableDataFormat> _formatLookup;

        public static readonly string Text = "Text";
        public static readonly string Rtf = "Rich Text Format";
        public static readonly string Bitmap = "Bitmap";

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



}
