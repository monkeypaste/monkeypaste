using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Plugin {

    public enum MpParameterControlType {
        None = 0,
        TextBox,
        ComboBox,
        List,
        MultiSelectList,
        EditableList,
        CheckBox,
        Slider,
        FileChooser,
        DirectoryChooser,
        PasswordBox,
        Radio,
        ComponentPicker,
        ShortcutRecorder,
        Button
    }

    public enum MpParameterValueUnitType {
        None = 0,
        Bool,
        Integer,
        Decimal,
        PlainText,
        RichText,
        //CredentialText,
        Html,
        Image,
        Base64Text,
        FileSystemPath,
        PlainTextContentQuery,
        RawDataContentQuery,
        DelimitedPlainText,
        CollectionComponentId,
        ActionComponentId,
        AnalyzerComponentId,
        ContentPropertyPathTypeComponentId
    }

    public interface MpIParamterValueProvider {
        string ParamId { get; }
        string Value { get; }
    }
    public class MpParameterFormat : MpJsonObject, MpIParamterValueProvider {
        #region Interfaces

        #region MpIParameterValueProvider Implementation
        [JsonIgnore]
        string MpIParamterValueProvider.ParamId => paramId;
        [JsonIgnore]
        string MpIParamterValueProvider.Value {
            get {
                if (values == null || values.Count == 0) {
                    return string.Empty;
                }
                if (values.Where(x => x.isDefault).Count() > 0) {
                    return values.Select(x => x.value).ToCsv(CsvProps);
                }
                return values.First().value;
            }
        }
        #endregion

        #endregion
        // NOTE paramId can be empty in manifest and will fall back to index of parameter in manifest
        // internally this doesn't matter but plugin needs to either name them or be aware of the order
        // or request args will be mismatched
        // NOTE! important when using manifest index that readers are counted before writers no matter 
        // order in file so readers should be defined before writers as a convention, not mandated

        private string _paramId = null;
        public string paramId {
            get {
                if (string.IsNullOrEmpty(_paramId)) {
                    // fallback
                    _paramId = label;
                }
                return _paramId;
            }
            set {
                if (!string.IsNullOrEmpty(value) && paramId != value) {
                    // don't let omitted/empty name become value
                    _paramId = value;
                }
            }
        }

        public string label { get; set; } = string.Empty;
        public string placeholder { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;

        public MpParameterControlType controlType { get; set; } = MpParameterControlType.None;
        public MpParameterValueUnitType unitType { get; set; } = MpParameterValueUnitType.PlainText;

        public bool isExecuteParameter { get; set; }
        public bool isSharedValue { get; set; }
        public bool isValueDeferred { get; set; }
        public bool isVisible { get; set; } = true;
        public bool isReadOnly { get; set; } = false;
        public bool isRequired { get; set; } = false;
        public bool confirmToRemember { get; set; } = false;

        //TextBox
        public int maxLength { get; set; } = int.MaxValue;
        public int minLength { get; set; } = 0;
        public string pattern { get; set; } = string.Empty;
        public string patternInfo { get; set; } = string.Empty;

        //Slider
        public double minimum { get; set; } = 0;
        public double maximum { get; set; } = 1;
        public int precision { get; set; } = 2;

        public List<MpPluginParameterValueFormat> values { get; set; } = new List<MpPluginParameterValueFormat>();

        public static MpCsvFormatProperties GetControlCsvProps(MpParameterControlType controlType) {
            return IsControlTypeMultiValue(controlType) ?
                MpCsvFormatProperties.DefaultBase64Value :
                MpCsvFormatProperties.Default;
        }

        public static bool IsControlTypeMultiValue(MpParameterControlType controlType) {
            return
                controlType == MpParameterControlType.MultiSelectList ||
                controlType == MpParameterControlType.EditableList;
        }
        public static bool IsControlCsvValue(MpParameterControlType controlType) {
            return GetControlCsvProps(controlType).IsValueBase64;
        }

        [JsonIgnore]
        public bool IsCsvValue => IsControlCsvValue(controlType);

        [JsonIgnore]
        public MpCsvFormatProperties CsvProps => GetControlCsvProps(controlType);

        [JsonIgnore]
        public bool IsMultiValue =>
            IsControlTypeMultiValue(controlType);
    }


    public class MpPluginParameterValueFormat : MpJsonObject {
        public string value { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public bool isDefault { get; set; } = false;
    }

}
