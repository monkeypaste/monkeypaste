using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using static MonkeyPaste.Common.Plugin.MpPortableDataFormats;

namespace MonkeyPaste.Common {
    public static class MpDataFormatRegistrar {
        #region Private Variables
        private static MpIPlatformDataObjectRegistrar _registrar;
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        public static string[] SortedTextFormats =>
            TextFormats.Distinct().ToArray();

        private static Dictionary<string, int> _regReaderLookup = new Dictionary<string, int>();
        public static IEnumerable<string> RegisteredReaderFormats =>
            _regReaderLookup.Where(x => x.Value > 0).Select(x => x.Key);

        private static Dictionary<string, int> _regWriterLookup = new Dictionary<string, int>();
        public static IEnumerable<string> RegisteredWriterFormats =>
            _regWriterLookup.Where(x => x.Value > 0).Select(x => x.Key);


        private static List<string> _regInternalFormats = [];
        public static IReadOnlyList<string> RegisteredInternalFormats =>
            _regInternalFormats;
        public static IEnumerable<string> RegisteredFormats =>
            RegisteredReaderFormats
            .Union(RegisteredWriterFormats)
            .Union(RegisteredInternalFormats)
            .Distinct();
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public static void Init(MpIPlatformDataObjectRegistrar registrar) {
            _registrar = registrar;

            _regWriterLookup.Clear();
            _regReaderLookup.Clear();

            foreach (string formatName in AllInternalFormats) {
                RegisterDataFormat(formatName, false, false);
            }
        }

        public static void RegisterDataFormat(string format, bool isReader, bool isWriter) {
            int result = -1;
            if (isReader) {
                result = Register_internal(_regReaderLookup, format);
            }
            if (isWriter) {
                result = Register_internal(_regWriterLookup, format);
            }

            if (!isReader && !isWriter) {
                // NOTE this should only happen for internal formats
                if (!AllInternalFormats.Contains(format)) {
#if DEBUG
                    throw new System.Exception($"Non-internal format registration mismatch for '{format}'");
#endif
                } else if (!_regInternalFormats.Contains(format)) {
                    _regInternalFormats.Add(format);
                }
            }
            MpConsole.WriteLine($"Format '{format}' IsReader: {isReader} IsWriter: {isWriter} REGISTERED ({result})");
        }
        public static void UnregisterDataFormat(string format, bool isReader, bool isWriter) {
            int result = -1;
            if (isReader) {
                result = UnregisterFromLookup(_regReaderLookup, format);
            }
            if (isWriter) {
                result = UnregisterFromLookup(_regWriterLookup, format);
            }
            MpConsole.WriteLine($"Format '{format}' IsReader: {isReader} IsWriter: {isWriter} UNREGISTERED ({result})");
        }

        public static bool IsTextFormat(string format) {
            return SortedTextFormats.Contains(format);
        }
        public static bool IsPlainTextFormat(string format) {
            return PlainTextFormats.Contains(format);
        }

        public static bool IsRtfFormat(string format) {
            return RtfFormats.Contains(format);
        }

        public static bool IsCsvFormat(string format) {
            return CsvFormats.Contains(format);
        }
        public static bool IsHtmlFormat(string format) {
            return HtmlFormats.Contains(format);
        }
        public static bool IsImageFormat(string format) {
            return ImageFormats.Contains(format);
        }

        public static bool IsFilesFormat(string format) {
            return FileFormats.Contains(format);
        }

        public static bool IsFormatStrBase64(string format) {
            if (IsImageFormat(format) is true ||
                format == CefAsciiUrl) {
                return true;
            }
            return false;
        }
        public static bool IsAvaloniaFormat(string format) {
            return
                format == AvFiles ||
                format == AvImage ||
                format == AvText;
        }
        public static string GetDefaultFileExt(string format) {
            if (IsPlainTextFormat(format) is true) {
                return "txt";
            }
            if (IsHtmlFormat(format) is true) {
                return "html";
            }
            if (IsRtfFormat(format) is true) {
                return "rtf";
            }
            if (IsCsvFormat(format) is true) {
                return "csv";
            }
            if (format == MacImage3) {
                return "tiff";
            }
            if (format == LinuxImage3) {
                return "bmp";
            }
            if (IsImageFormat(format) is true) {
                return "png";
            }
            // fallback to txt
            return "txt";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods


        private static int UnregisterFromLookup(Dictionary<string, int> lookup, string format) {
            if (!lookup.TryGetValue(format, out int reg_count)) {
                return 0;
            }
            reg_count--;
            if (reg_count <= 0) {
                lookup.Remove(format);
            } else {
                lookup[format] = reg_count;
            }
            return reg_count;
        }
        private static int Register_internal(Dictionary<string, int> lookup, string format) {
            if (!lookup.TryGetValue(format, out int reg_count)) {
                reg_count = 1;
                lookup.Add(format, 0);
                if (_registrar != null) {
                    // pretty sure registering is only needed for win32 c++ but just keeping it
                    int id = _registrar.RegisterFormat(format);
                }
            } else {
                reg_count++;
            }
            lookup[format] = reg_count;

            return reg_count;
        }
        #endregion

        #region Commands
        #endregion


    }
}
