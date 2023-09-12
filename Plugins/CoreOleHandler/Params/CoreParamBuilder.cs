using MonkeyPaste.Common;

using MonkeyPaste.Common.Plugin;

namespace CoreOleHandler {
    public class CoreParamBuilder : MpISupportHeadlessClipboardComponentFormat {
        const int DEF_MAX_TEXT = 100_000;
        const int MAX_MAX_TEXT = 1_000_000;
        string PluginGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        string IconDir => @".\Resources\Images";

        (string, string, int, string)[] _formats = new (string, string, int, string)[] {
                (MpPortableDataFormats.Text,"Text",DEF_MAX_TEXT,"text.png"),
                (MpPortableDataFormats.CefText,"Text (web)",DEF_MAX_TEXT,"text.png"),
                (MpPortableDataFormats.AvRtf_bytes,"Rtf",DEF_MAX_TEXT,"rtf.png"),
                (MpPortableDataFormats.AvHtml_bytes,"Html",DEF_MAX_TEXT,"html.png"),
                (MpPortableDataFormats.CefHtml,"Html (web)",DEF_MAX_TEXT,"html.png"),
                (MpPortableDataFormats.LinuxSourceUrl,"Uri",-1,"html.png"),
                (MpPortableDataFormats.AvPNG,"Png",-1,"bitmap.png"),
                (MpPortableDataFormats.AvCsv,"Csv",DEF_MAX_TEXT,"csv.png"),
                (MpPortableDataFormats.AvFiles,"Files",-1,"files.png"),
                //("x-special/gnome-copied-files","Files (Linux)"),
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
                    maximum = MAX_MAX_TEXT,
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