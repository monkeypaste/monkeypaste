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
        Xaml,
        XamlPackage,
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
        Task<MpPortableDataObject> ConvertToPortableDataObject(bool fillTemplates);
    }
    public interface MpIExternalPasteHandler {
        Task PasteDataObject(MpPortableDataObject mpdo, object handleOrProcessInfo, bool finishWithEnterKey = false);
    }

    public interface MpIPortableDataObject {
        Dictionary<MpPortableDataFormat, object> DataFormatLookup { get; }

        bool ContainsData(string format);
        object GetData(string format);
        void SetData(string format, object data);
    }

    public class MpPortableDataObject : MpIPortableDataObject {
        #region Properties

        public Dictionary<MpPortableDataFormat, object> DataFormatLookup { get; private set; } = new Dictionary<MpPortableDataFormat, object>();
        
        #endregion

        public bool ContainsData(string format) {
            return GetData(format) != null;
        }

        public object GetData(string format) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                return null;
            }
            DataFormatLookup.TryGetValue(pdf, out object data);
            return data;
        }

        public void SetData(string format, object data) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                throw new MpUnregisteredDataFormatException($"Format {format} is not registered");
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
        #region Private Variables

        private static MpIPlatformDataObjectRegistrar _registrar;

        private static string[] _defaultFormatNames = new string[] {
            Text,
            Rtf,
            Xaml,
            XamlPackage,
            Html,
            Csv,
            Unicode,
            OemText,
            FileDrop,
            Bitmap,
            INTERNAL_CLIP_TILE_DATA_FORMAT
        };

        private static Dictionary<int, MpPortableDataFormat> _formatLookup;

        #endregion

        #region Constants

        public const string Text = "Text";
        public const string Rtf = "Rich Text Format";
        public const string Xaml = "Xaml";
        public const string XamlPackage = "XamlPackage";
        public const string Bitmap = "Bitmap";
        public const string Html = "HTML Format";
        public const string FileDrop = "FileDrop";
        public const string Csv = "CSV";
        public const string Unicode = "Unicode";
        public const string OemText = "OEMText";

        public const string INTERNAL_CLIP_TILE_DATA_FORMAT = "Mp Internal Content";

        #endregion

        #region Properties

        public static IEnumerable<string> RegisteredFormats => _formatLookup.Select(x => x.Value.Name);

        #endregion

        #region Public Methods

        public static void Init(MpIPlatformDataObjectRegistrar registrar) {
            _registrar = registrar;

            _formatLookup = new Dictionary<int, MpPortableDataFormat>();

            foreach (string formatName in _defaultFormatNames) {
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
                MpConsole.WriteTraceLine($"Warning attempted to register already registered format name:'{format}' id:{id}");
                return _formatLookup[id];
            }
            id = _registrar.RegisterFormat(format);
            var pdf = new MpPortableDataFormat(format, id);
            _formatLookup.Add(pdf.Id, pdf);
            MpConsole.WriteTraceLine($"Successfully registered format name:'{format}' id:{id}");
            return pdf;
        }

        public static void UnregisterDataFormat(string format) {
            int id = GetDataFormatId(format);
            if (id == 0) {
                // format doesn't exist so pretend it was successfull but log 
                MpConsole.WriteTraceLine($"Warning attempted to unregister a non-registered format named '{format}'");
                return;
            }
            if(_formatLookup.Remove(id)) {
                MpConsole.WriteTraceLine($"Successfully unregistered format name:'{format}' id:{id}");
            }
        }

        public static int GetDataFormatId(string format) {
            foreach(var kvp in _formatLookup) {
                if(kvp.Value.Name == format) {
                    return kvp.Key;
                }
            }
            return -1;
        }

        #endregion
    }


    public class MpPortableDataFormat {
        public string Name { get; private set; }

        public int Id { get; private set; }

        public MpPortableDataFormat(string name, int id) {
            Name = name;
            Id = id;
        }

        public override string ToString() {
            return $"{Id}-{Name}";
        }
    }
}
