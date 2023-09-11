using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia.Plugin;
using MonkeyPaste.Common.Plugin;

namespace AltOleHandler {
    public class AltOleParamBuilder : MpISupportHeadlessClipboardComponentFormat {
        const int DEF_MAX_TEXT = 1_000_000;
        string PluginGuid => "c10af19f-a40f-4a37-920c-e502c2b74d68";
        string IconDir => @".\Resources\Images";

        (string, string, int, string)[] _formats = new (string, string, int, string)[] {
                ("SUPeR FUNkY FoRmATZZz","Dat funky format",DEF_MAX_TEXT,"funky.png")
        };
        public MpClipboardHandlerFormats GetFormats() {
            return new MpClipboardHandlerFormats() {
                readers = _formats.Select(x => GetFormat(x.Item1, true)).ToList(),
                writers = _formats.Select(x => GetFormat(x.Item1, false)).ToList()
            };
        }

        private MpClipboardHandlerFormat GetFormat(string format, bool isReader) {
            var hf = new MpClipboardHandlerFormat() {
                formatGuid = $"{PluginGuid}-{format}-{(isReader ? "READ" : "WRITE")}",
                iconUri = Path.Combine(IconDir, $"{_formats.FirstOrDefault(x => x.Item1 == format).Item4}"),
                formatName = format,
                displayName = _formats.FirstOrDefault(x => x.Item1 == format).Item2,
                parameters = GetFormatParameters(format, isReader),

            };
            return hf;
        }

        private List<MpParameterFormat> GetFormatParameters(string format, bool isReader) {
            var tup = _formats.FirstOrDefault(x => x.Item1 == format);
            var pfl = new List<MpParameterFormat>();
            // DEFAULT PARAMS

            pfl.Add(new MpParameterFormat() {
                label = "Ignore",
                description = $"When checked {format} format will not be {(isReader ? "read from" : "written to")} the clipboard or drag-and-drop data.",
                controlType = MpParameterControlType.CheckBox,
                unitType = MpParameterValueUnitType.Bool,
                value = new MpPluginParameterValueFormat(false.ToString()),
                paramId = GetParamId(format, isReader, "ignore")
            });
            if (tup.Item3 >= 0) {
                pfl.Add(new MpParameterFormat() {
                    label = "Max Characters",
                    description = $"For performance concerns, a maximum amount characters is needed :(",
                    controlType = MpParameterControlType.Slider,
                    unitType = MpParameterValueUnitType.Integer,
                    minimum = 1,
                    maximum = int.MaxValue,
                    value = new MpPluginParameterValueFormat(tup.Item3.ToString(), true),
                    paramId = GetParamId(format, isReader, "maxcharcount")
                });
            }

            switch (format) {
                case MpPortableDataFormats.AvFiles:
                    if (isReader) {
                        pfl.Add(new MpParameterFormat() {
                            label = "Ignored Directories",
                            description = $"These directories will not be read from clipboard or drag-and-drop data.",
                            controlType = MpParameterControlType.EditableList,
                            unitType = MpParameterValueUnitType.PlainText,
                            paramId = GetParamId(format, isReader, "ignoredirs")
                        });
                    }
                    pfl.Add(new MpParameterFormat() {
                        label = "Ignored File Extensions",
                        description = $"These file types will not be {(isReader ? "read from" : "written to")} the clipboard or drag-and-drop data.",
                        controlType = MpParameterControlType.EditableList,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = GetParamId(format, isReader, "ignoreexts")
                    });
                    break;
                case MpPortableDataFormats.AvRtf_bytes:
                    pfl.Add(new MpParameterFormat() {
                        label = "Convert to Html",
                        description = $"When html is not already present, rtf will be converted to html. This may be useful between web browsers and/or word processing applications. (only for supported platforms)",
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        value = new MpPluginParameterValueFormat(true.ToString(), true),
                        paramId = GetParamId(format, isReader, "tohtml")
                    });
                    break;
                case MpPortableDataFormats.CefHtml:
                case MpPortableDataFormats.AvHtml_bytes:
                    pfl.Add(new MpParameterFormat() {
                        label = "Convert to Rtf",
                        description = $"When rtf is not already present, html will be converted to rtf. This may be useful between web browsers and/or word processing applications. (only for supported platforms)",
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        value = new MpPluginParameterValueFormat(true.ToString(), true),
                        paramId = GetParamId(format, isReader, "tortf")
                    });
                    break;
                case MpPortableDataFormats.AvPNG:
                    if (!isReader) {
                        pfl.Add(new MpParameterFormat() {
                            label = "Default Export Type",
                            description = $"When pasting or dropping a bitmap this will be the default file type",
                            controlType = MpParameterControlType.ComboBox,
                            unitType = MpParameterValueUnitType.PlainText,
                            paramId = GetParamId(format, isReader, "exporttype"),
                            values =
                                new[] { "bmp", "png", "jpg" }
                                .Select(x => new MpPluginParameterValueFormat(x, x == "png"))
                                .ToList()
                        });
                    }
                    break;
            }

            return pfl;
        }
        private string GetParamId(string format, bool isReader, string detail) {
            format =
                format
                .Replace(" ", string.Empty)
                .Replace("/", string.Empty)
                .Replace("-", string.Empty)
                .ToUpper();
            string result = $"{format}_{(isReader ? "R" : "W")}_{detail.ToUpper()}";
            return result;
        }
    }
}