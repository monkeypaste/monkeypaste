using MonkeyPaste.Common.Plugin;

namespace AltOleHandlerTest {
    public static class PluginFormatBuilder {
        static string PluginGuid => "c10af19f-a40f-4a37-920c-e502c2b74d68";
        static string IconDir => @"Resources\Images";
        static string[] _readers = new string[] {
                "SUPER FUNKY FORMAT",
                "Text"
            };

        static string[] _writers = new string[] {
                "Text"
        };
        public static MpClipboardHandlerFormats Build() {
            return new MpClipboardHandlerFormats() {
                readers = _readers.Select(x => GetFormat(x, true)).ToList(),
                writers = _readers.Select(x => GetFormat(x, false)).ToList()
            };
        }

        private static MpClipboardHandlerFormat GetFormat(string format, bool isReader) {
            var hf = new MpClipboardHandlerFormat() {
                formatGuid = GetFormatGuid(format, isReader),
                iconUri = Path.Combine(IconDir, $"{format.ToLower()}.png"),
                formatName = format,
                parameters = GetFormatParameters(format, isReader)
            };
            return hf;
        }

        private static string GetFormatGuid(string format, bool isReader) {
            return $"{PluginGuid}-{format}-{(isReader ? "READ" : "WRITE")}";
        }

        private static List<MpParameterFormat> GetFormatParameters(string format, bool isReader) {
            var pfl = new List<MpParameterFormat>();
            // DEFAULT PARAMS

            // ignore
            pfl.Add(new MpParameterFormat() {
                label = "Ignore",
                description = $"When checked {format} format will be completely ignored",
                controlType = MpParameterControlType.CheckBox,
                unitType = MpParameterValueUnitType.Bool,
                value = new MpPluginParameterValueFormat(false.ToString())
            });

            return pfl;
        }
    }
}