using MonkeyPaste.Common.Plugin;

namespace CoreOleHandler {
    public class CoreOleParamBuilder : MpISupportHeadlessClipboardComponentFormat {
        const int DEF_MAX_TEXT = -1;//10_000_000;
        const int MAX_MAX_TEXT = 1_000_000;
        string PluginGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        string IconDir => @".\Resources\Images";

        (string, string, int, string)[] _formats;
        public MpClipboardComponent GetFormats(MpHeadlessComponentFormatRequest request) {
            CoreOleHelpers.SetCulture(request);
            // create format models AFTER culture set 
            _formats = GetFormatModels();

            return new MpClipboardComponent() {
                readers = _formats.Select(x => GetFormat(x.Item1, true)).ToList(),
                writers = _formats.Select(x => GetFormat(x.Item1, false)).ToList()
            };
        }

        private (string, string, int, string)[] GetFormatModels() {
            return [
                (MpPortableDataFormats.Text,Resources.TextFormatLabel,DEF_MAX_TEXT,"text.png"),
                (MpPortableDataFormats.MimeText,Resources.MimeTextFormatLabel,DEF_MAX_TEXT,"text.png"),
                (MpPortableDataFormats.Rtf,Resources.RtfFormatLabel,DEF_MAX_TEXT,"rtf.png"),
                (MpPortableDataFormats.Xhtml,Resources.HtmlFormatLabel,-1,"html.png"),
                (MpPortableDataFormats.Html,Resources.MimeHtmlFormatLabel,-1,"html.png"),
                (MpPortableDataFormats.MimeMozUrl,Resources.MozUrlFormatLabel,-1,"html.png"),
                (MpPortableDataFormats.Image,Resources.PngFormatLabel,-1,"png.png"),
#if MAC
		        (MpPortableDataFormats.Image2,"TIFF",-1,"png.png"),
#endif
                (MpPortableDataFormats.Csv,Resources.CsvFormatLabel,DEF_MAX_TEXT,"csv.png"),
                (MpPortableDataFormats.Files,Resources.FilesFormatLabel,-1,"files.png"),
                //("x-special/gnome-copied-files","Files (Linux)"),
        ];
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
                label = Resources.CommonIgnoreLabel,
                //description = $"When checked {format} format will not be {(isReader ? "read from" : "written to")} the clipboard or drag-and-drop data.",
                description = string.Format(isReader ? Resources.CommonIgnoreReaderHint : Resources.CommonIgnoreWriterHint, format),
                controlType = MpParameterControlType.CheckBox,
                unitType = MpParameterValueUnitType.Bool,
                value = new MpParameterValueFormat(false.ToString()),
                paramId = GetParamId(format, isReader, Resources.CommonIgnoreLabel)
            });
            if (tup.Item3 >= 0) {
                pfl.Add(new MpParameterFormat() {
                    label = Resources.MaxCharsLabel,
                    description = Resources.MaxCharsHint,
                    controlType = MpParameterControlType.Slider,
                    unitType = MpParameterValueUnitType.Integer,
                    minimum = 1,
                    maximum = MAX_MAX_TEXT,
                    value = new MpParameterValueFormat(tup.Item3.ToString(), true),
                    paramId = GetParamId(format, isReader, "maxcharcount")
                });
            }

            switch (format) {
                case MpPortableDataFormats.Files:
                    if (isReader) {
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.IgnoredDirsLabel,
                            description = Resources.IgnoredDirsHint,
                            controlType = MpParameterControlType.EditableList,
                            unitType = MpParameterValueUnitType.PlainText,
                            paramId = GetParamId(format, isReader, "ignoredirs")
                        });
                    }
                    pfl.Add(new MpParameterFormat() {
                        label = Resources.IgnoredFileExtLabel,
                        description = isReader ? Resources.IgnoredFileExtReaderHint : Resources.IgnoredFileExtWriterHint,
                        controlType = MpParameterControlType.EditableList,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = GetParamId(format, isReader, "ignoreexts")
                    });
                    break;
                case MpPortableDataFormats.Rtf:
                    pfl.Add(new MpParameterFormat() {
                        label = Resources.Rtf2HtmlLabel,
                        description = Resources.Rtf2HtmlHint,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        value = new MpParameterValueFormat(true.ToString(), true),
                        paramId = GetParamId(format, isReader, "tohtml")
                    });
                    break;
                case MpPortableDataFormats.Html:
                case MpPortableDataFormats.Xhtml:
                    pfl.Add(new MpParameterFormat() {
                        label = Resources.Html2RtfLabel,
                        description = Resources.Html2RtfHint,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        value = new MpParameterValueFormat(true.ToString(), true),
                        paramId = GetParamId(format, isReader, "tortf")
                    });
                    break;
                case MpPortableDataFormats.Image:
                    if (isReader) {
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.ImgIgnoreTransparentImgLabel,
                            description = Resources.ImgIgnoreTransparentImgHint,
                            controlType = MpParameterControlType.CheckBox,
                            unitType = MpParameterValueUnitType.Bool,
                            value = new MpParameterValueFormat(true.ToString(), true),
                            paramId = GetParamId(format, isReader, "ignore_empty")
                        });
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.ImgMaxWLabel,
                            description = Resources.ImgMaxHint,
                            controlType = MpParameterControlType.Slider,
                            unitType = MpParameterValueUnitType.Integer,
                            paramId = GetParamId(format, isReader, "maxw"),
                            minimum = 0,
                            maximum = 30_000,
                            value = new MpParameterValueFormat(3000.ToString(), true)
                        });
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.ImgMaxHLabel,
                            description = Resources.ImgMaxHint,
                            controlType = MpParameterControlType.Slider,
                            unitType = MpParameterValueUnitType.Integer,
                            paramId = GetParamId(format, isReader, "maxh"),
                            minimum = 0,
                            maximum = 30_000,
                            value = new MpParameterValueFormat(3000.ToString(), true)
                        });
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.ImgDoScaleLabel,
                            description = Resources.ImgDoScaleHint,
                            controlType = MpParameterControlType.CheckBox,
                            unitType = MpParameterValueUnitType.Bool,
                            paramId = GetParamId(format, isReader, "scaleoversized"),
                            value = new MpParameterValueFormat(true.ToString(), true)
                        });
                    } else {
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.ImgExportTypeLabel,
                            description = Resources.ImgExportTypeHint,
                            controlType = MpParameterControlType.ComboBox,
                            unitType = MpParameterValueUnitType.PlainText,
                            paramId = GetParamId(format, isReader, "exporttype"),
                            values =
                                new[] { "bmp", "png", "jpg" }
                                .Select(x => new MpParameterValueFormat(x, x == "png"))
                                .ToList()
                        });
                    }
                    break;
            }

            return pfl;
        }
        public static string GetParamId(string format, bool isReader, string detail) {
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