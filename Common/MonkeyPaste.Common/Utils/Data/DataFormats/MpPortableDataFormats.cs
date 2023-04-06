
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpPortableDataFormats {
        #region Private Variables

        private static MpIPlatformDataObjectRegistrar _registrar;

        private static string[] _defaultFormatNames = new string[] {

            // windows

            Text,
            //Rtf,
            Xaml,
            XamlPackage,
            //Html,
            AvCsv,
            Unicode,
            OemText,
            //FileDrop,
            WinBitmap,
            WinDib,

            // linux

            LinuxSourceUrl,
            LinuxGnomeFiles,
            LinuxUriList, 

            // avalonia

            AvRtf_bytes,
            AvHtml_bytes,
            AvFileNames,
            AvPNG,

            // cef

            CefHtml,
            CefText,
            CefJson,
            CefAsciiUrl,
            //CefUnicodeUrl,

            // internal
            INTERNAL_SOURCE_URI_LIST_FORMAT, // maps to LinuxUriList
            
            INTERNAL_CONTENT_HANDLE_FORMAT,
            INTERNAL_CONTENT_TITLE_FORMAT,
            INTERNAL_CONTENT_ROI_FORMAT,
            INTERNAL_CONTENT_ANNOTATION_FORMAT,
            INTERNAL_CONTENT_DELTA_FORMAT,
            INTERNAL_PARAMETER_REQUEST_FORMAT,
            INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT,
            INTERNAL_TAG_ITEM_FORMAT,
            INTERNAL_CONTENT_TYPE_FORMAT
        };

        private static Dictionary<int, MpPortableDataFormat> _formatLookup = new Dictionary<int, MpPortableDataFormat>();

        #endregion

        #region Constants

        // Windows Formats
        public const string Text = "Text";
        public const string Xaml = "Xaml";
        public const string XamlPackage = "XamlPackage";
        public const string Unicode = "Unicode";
        public const string OemText = "OEMText";


        public const string WinCsv = "CSV";
        public const string WinBitmap = "Bitmap";
        public const string WinDib = "DeviceIndependentBitmap";
        public const string WinRtf = "Rich Text Format";
        public const string WinHtml = "HTML Format";
        public const string WinFileDrop = "FileDrop";

        // Linux Formats
        public const string LinuxSourceUrl = "text/x-moz-url-priv";

        public const string LinuxUriList = "text/uri-list";
        public const string LinuxGnomeFiles = "x-special/gnome-copied-files";

        // Avalonia Formats

        public const string AvCsv = "Csv";
        public const string AvRtf_bytes = "Rich Text Format";
        public const string AvHtml_bytes = "HTML Format";
        public const string AvFileNames = "FileNames";
        public const string AvPNG = "PNG";

        // Cef Formats

        //public const string CefDragData = "CefDragData";
        //public const string CefUnicodeUrl = "UniformResourceLocatorW";
        public const string CefAsciiUrl = "UniformResourceLocator";

        public const string CefHtml = "text/html";
        public const string CefText = "text/plain";
        public const string CefJson = "application/json";

        public const string INTERNAL_SOURCE_URI_LIST_FORMAT = LinuxUriList;

        public const string INTERNAL_CONTENT_HANDLE_FORMAT = "Mp Internal Content";
        public const string INTERNAL_CONTENT_TYPE_FORMAT = "Mp Internal Content Type";
        public const string INTERNAL_CONTENT_TITLE_FORMAT = "Mp Internal Content Title";

        public const string INTERNAL_CONTENT_ROI_FORMAT = "Mp Internal Content Roi";
        public const string INTERNAL_CONTENT_ANNOTATION_FORMAT = "Mp Internal Content Annotation";
        public const string INTERNAL_CONTENT_DELTA_FORMAT = "Mp Internal Quill Delta Json";
        public const string INTERNAL_PARAMETER_REQUEST_FORMAT = "Mp Internal Parameter Request Format";
        public const string INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT = "Mp Internal Search Criteria Item";
        public const string INTERNAL_TAG_ITEM_FORMAT = "Mp Internal Tag Tile Item";

        // NOTE data object is not registered and only used to merge data objects
        public const string INTERNAL_DATA_OBJECT_FORMAT = "Mp Internal Data Object Format";

        public const string PLACEHOLDER_DATAOBJECT_TEXT = "3acaaed7-862d-47f5-8614-3259d40fce4d";
        #endregion

        #region Statics

        public static string[] InternalFormats = new string[] {
            INTERNAL_SOURCE_URI_LIST_FORMAT,
            INTERNAL_CONTENT_HANDLE_FORMAT,
            INTERNAL_CONTENT_TITLE_FORMAT,
            INTERNAL_CONTENT_ROI_FORMAT,
            INTERNAL_CONTENT_ANNOTATION_FORMAT,
            INTERNAL_CONTENT_DELTA_FORMAT,
            INTERNAL_PARAMETER_REQUEST_FORMAT,
            INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT,
            INTERNAL_TAG_ITEM_FORMAT,
            INTERNAL_DATA_OBJECT_FORMAT,
            INTERNAL_CONTENT_TYPE_FORMAT
        };
        #endregion

        #region Properties

        public static IEnumerable<string> RegisteredFormats =>
            _formatLookup.Select(x => x.Value.Name);

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
            MpConsole.WriteLine($"Successfully registered format name:'{format}' id:{id}");
            return pdf;
        }

        public static void UnregisterDataFormat(string format) {
            int id = GetDataFormatId(format);
            if (id == 0) {
                // format doesn't exist so pretend it was successfull but log 
                MpConsole.WriteTraceLine($"Warning attempted to unregister a non-registered format named '{format}'");
                return;
            }
            if (_formatLookup.Remove(id)) {
                MpConsole.WriteLine($"Successfully unregistered format name:'{format}' id:{id}");
            }
        }

        public static int GetDataFormatId(string format) {
            var kvpl = _formatLookup.Where(x => x.Value.Name.ToLower() == format.ToLower());
            if (kvpl.Count() == 0) {
                return -1;
            }
            if (kvpl.Count() > 1) {
                // multiple formats w/ same name but different case detected
                Debugger.Break();
                var match_kvp = kvpl.FirstOrDefault(x => x.Value.Name == format);
                if (!match_kvp.Equals(default(KeyValuePair<int, MpPortableDataFormat>))) {
                    // when exact match found return that one...
                    return match_kvp.Key;
                }
            }
            return kvpl.First().Key;
        }

        #endregion
    }
}
