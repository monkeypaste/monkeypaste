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
        object GetDataObjectWrapper();
    }

    public interface MpIExternalPasteHandler {
        Task PasteDataObject(MpPortableDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false);
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
                        MpClipboardFormatType.OemText
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

    }
}
