
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common {
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
            //FileNames,
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
       //public const string FileNames = "FileNames";
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
}
