
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvDataFormats {
        #region Private Variables

        private static string[] _defaultFormatNames = new string[] {
            //AvRtf,
            //AvHtml,
            AvFileNames,
            CefHtml,
            CefText,
            CefQuillDeltaJson
        };

        private static Dictionary<int, MpAvDataFormat> _formatLookup;

        private static string[] _overrideFormatNames = new string[] {
            MpPortableDataFormats.Html,
            MpPortableDataFormats.FileDrop,
            MpPortableDataFormats.Rtf
        };

        #endregion

        #region Properties

        #endregion

        #region Constants


        //public const string AvRtf = MpPortableDataFormats.Rtf;
        //public const string AvHtml = MpPortableDataFormats.Html;
        public const string AvFileNames = "FileNames";
        public const string CefHtml = "text/html";
        public const string CefText = "text/plain";
        public const string CefQuillDeltaJson = "application/json/quill-delta";
        #endregion

        #region Public Methods

        public static void Init(MpIPlatformDataObjectRegistrar registrar) {
            MpPortableDataFormats.Init(registrar);

            _formatLookup = new Dictionary<int, MpAvDataFormat>();

            foreach (string formatName in _defaultFormatNames) {
                MpPortableDataFormats.RegisterDataFormat(formatName);
            }
        }

        public static bool IsFormatOverride(string format) {
            return _overrideFormatNames.Contains(format);
        }

        #endregion
    }
}
