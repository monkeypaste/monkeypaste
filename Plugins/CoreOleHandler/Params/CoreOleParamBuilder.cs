using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CoreOleHandler {
    public class CoreOleParamBuilder : MpISupportHeadlessClipboardComponentFormat {
        const int DEF_MAX_TEXT = -1;//10_000_000;
        const int MAX_MAX_TEXT = 1_000_000;
        string PluginGuid => "cf2ec03f-9edd-45e9-a605-2a2df71e03bd";
        string IconDir => @".\Resources\Images";

        (string formatName, string label, int max_len, string icon_path)[] _formatModels;
        public MpClipboardComponent GetFormats(MpHeadlessComponentFormatRequest request) {
            CoreOleHelpers.SetCulture(request);
            // create format models AFTER culture set 
            _formatModels = GetFormatModels();

            return new MpClipboardComponent() {
                readers = _formatModels.Select(x => GetFormat(x.formatName, true)).ToList(),
                writers = _formatModels.Select(x => GetFormat(x.formatName, false)).ToList()
            };
        }

        private (string formatName, string label, int max_len, string icon_path)[] GetFormatModels() {
            var fml = new List<(string formatName, string label, int max_len, string icon_path)>() {
                (MpPortableDataFormats.Text,Resources.TextFormatLabel,DEF_MAX_TEXT,"text.png"),
                (MpPortableDataFormats.MimeText,Resources.MimeTextFormatLabel,DEF_MAX_TEXT,"text.png"),
                (MpPortableDataFormats.Rtf,Resources.RtfFormatLabel,DEF_MAX_TEXT,"rtf.png"),
                (MpPortableDataFormats.Xhtml,Resources.HtmlFormatLabel,-1,"html.png"),
                (MpPortableDataFormats.Html,Resources.MimeHtmlFormatLabel,-1,"html.png"),
                (MpPortableDataFormats.MimeMozUrl,Resources.MozUrlFormatLabel,-1,"html.png"),
                (MpPortableDataFormats.Image,Resources.PngFormatLabel,-1,"png.png"),
                (MpPortableDataFormats.Files,Resources.FilesFormatLabel,-1,"files.png"),
            };
            if (OperatingSystem.IsMacOS()) {
                fml.Add((MpPortableDataFormats.MacImage2, "PNG (platform)", -1, "png.png"));
                fml.Add((MpPortableDataFormats.MacImage3, "TIFF", -1, "png.png"));
            } else if (OperatingSystem.IsLinux()) {
                fml.AddRange([
                    (MpPortableDataFormats.LinuxImage2,Resources.PngFormatLabel + "2",-1,"png.png"),
                    (MpPortableDataFormats.LinuxImage3,Resources.PngFormatLabel + "3",-1,"png.png"),
                    (MpPortableDataFormats.LinuxFiles2,Resources.FilesFormatLabel + " (platform)",-1,"files.png"),
                    (MpPortableDataFormats.LinuxFiles3,Resources.FilesFormatLabel + " (platform2)",-1,"files.png"),
                    ]);
            } else if (OperatingSystem.IsWindows()) {
                fml.Add((MpPortableDataFormats.Csv, Resources.CsvFormatLabel, DEF_MAX_TEXT, "csv.png"));
            } 
            if (fml.Where(x => fml.Any(y => y != x && y.formatName == x.formatName)) is { } dups) {
                MpDebug.Assert(!dups.Any(), $"Error, dup formats found for {string.Join(",", dups.Select(x => x.formatName))}");
            }
            return fml.ToArray();
        }

        private MpClipboardHandlerFormat GetFormat(string format, bool isReader) {
            var hf = new MpClipboardHandlerFormat() {
                formatGuid = $"{PluginGuid}-{format}-{(isReader ? "READ" : "WRITE")}",
                iconUri = Path.Combine(IconDir, $"{_formatModels.FirstOrDefault(x => x.formatName == format).icon_path}"),
                formatName = format,
                displayName = _formatModels.FirstOrDefault(x => x.formatName == format).label,
                parameters = GetFormatParameters(format, isReader),

            };
            return hf;
        }

        private List<MpParameterFormat> GetFormatParameters(string format, bool isReader) {
            var tup = _formatModels.FirstOrDefault(x => x.formatName == format);
            var pfl = new List<MpParameterFormat>();
            // DEFAULT PARAMS

            // IGNORE PARAM
            pfl.Add(new MpParameterFormat() {
                label = Resources.CommonIgnoreLabel,
                description = string.Format(isReader ? Resources.CommonIgnoreReaderHint : Resources.CommonIgnoreWriterHint, format),
                controlType = MpParameterControlType.CheckBox,
                unitType = MpParameterValueUnitType.Bool,
                value = new MpParameterValueFormat(false.ToString()),
                paramId = GetParamId(format, isReader, Resources.CommonIgnoreLabel)
            });
            if (tup.max_len >= 0) {
                pfl.Add(new MpParameterFormat() {
                    label = Resources.MaxCharsLabel,
                    description = Resources.MaxCharsHint,
                    controlType = MpParameterControlType.Slider,
                    unitType = MpParameterValueUnitType.Integer,
                    minimum = 1,
                    maximum = MAX_MAX_TEXT,
                    value = new MpParameterValueFormat(tup.max_len.ToString(), true),
                    paramId = GetParamId(format, isReader, "maxcharcount")
                });
            }
            if (!isReader && IsPseudoFileFormat(format)) {
                pfl.Add(new MpParameterFormat() {
                    label = Resources.ToFilePriorityLabel,
                    description = string.Format(Resources.ToFilePriorityHint, format),
                    controlType = MpParameterControlType.NumberTicker,
                    unitType = MpParameterValueUnitType.Integer,
                    minimum = 0,
                    maximum = int.MaxValue,
                    precision = 0,
                    value = new MpParameterValueFormat(1.ToString(), true),
                    paramId = GetParamId(format, isReader, "filepriority")
                });
                pfl.Add(new MpParameterFormat() {
                    label = Resources.ToFileExtLabel,
                    description = string.Format(Resources.ToFileExtHint, format),
                    controlType = MpParameterControlType.TextBox,
                    unitType = MpParameterValueUnitType.PlainText,
                    value = new MpParameterValueFormat(MpDataFormatRegistrar.GetDefaultFileExt(format), true),
                    paramId = GetParamId(format, isReader, "fileext")
                });
            }

            switch (format) {
                case var _ when format == MpPortableDataFormats.Files:
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
                case var _ when format == MpPortableDataFormats.Rtf:
                    pfl.Add(new MpParameterFormat() {
                        label = Resources.Rtf2HtmlLabel,
                        description = Resources.Rtf2HtmlHint,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        value = new MpParameterValueFormat(true.ToString(), true),
                        paramId = GetParamId(format, isReader, "tohtml")
                    });
                    break;
                case var _ when format == MpPortableDataFormats.Html:
                case var _ when format == MpPortableDataFormats.Xhtml:
                    pfl.Add(new MpParameterFormat() {
                        label = Resources.Html2RtfLabel,
                        description = Resources.Html2RtfHint,
                        controlType = MpParameterControlType.CheckBox,
                        unitType = MpParameterValueUnitType.Bool,
                        value = new MpParameterValueFormat(true.ToString(), true),
                        paramId = GetParamId(format, isReader, "tortf")
                    });
                    break;
                case var _ when format == MpPortableDataFormats.Image:
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
                            label = Resources.TextToImageLabel,
                            description = Resources.TextToImageHint,
                            controlType = MpParameterControlType.MultiSelectList,
                            unitType = MpParameterValueUnitType.PlainText,
                            paramId = GetParamId(format, isReader, "fromtextformats"),
                            values =
                                _formatModels
                                .Where(x => MpDataFormatRegistrar.IsTextFormat(x.formatName) is true)
                                .Select(x => new MpParameterValueFormat(x.formatName, x.label, false))
                                .ToList()
                        });
                        pfl.Add(new MpParameterFormat() {
                            label = Resources.ImageToTextLabel,
                            description = Resources.ImageToTextHint,
                            controlType = MpParameterControlType.CheckBox,
                            unitType = MpParameterValueUnitType.Bool,
                            paramId = GetParamId(format, isReader, "asciiart"),
                            value = new MpParameterValueFormat(true.ToString(), true)
                        });
                    }
                    break;
            }

            return pfl;
        }
        public static bool IsPseudoFileFormat(string format) {
            return
                MpDataFormatRegistrar.IsFilesFormat(format) is not true &&
                (MpDataFormatRegistrar.IsTextFormat(format) is true ||
                 MpDataFormatRegistrar.IsImageFormat(format) is true);
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