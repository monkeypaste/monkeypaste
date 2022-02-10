
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public enum MpAnalyticItemParameterType {
        None = 0,
        Button,
        Text,
        ComboBox,
        CheckBox,
        Slider,
        Content
        //RuntimeMinOffset,//below are only runtime types        
    }

    public enum MpAnalyticItemParameterValueUnitType {
        None = 0,
        Bool,
        Integer,
        Decimal,
        PlainText,
        RichText,
        Html,
        Image,
        Base64Text,
        Custom
    }
    //adult,brands,categories,description,faces,objects,tags
    public class MpAnalyticItemParameterValue {
        [JsonProperty("value")]
        public string Value { get; set; } = string.Empty;

        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; } = false;

        [JsonProperty("isMinimum")]
        public bool IsMinimum { get; set; } = false;

        [JsonProperty("isMaximum")]
        public bool IsMaximum { get; set; } = false;
    }

    public class MpAnalyticItemParameterFormat {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("parameterType")]
        public MpAnalyticItemParameterType ParameterType { get; set; } = MpAnalyticItemParameterType.None;

        [JsonProperty("valueType")]
        public MpAnalyticItemParameterValueUnitType ValueType { get; set; } = MpAnalyticItemParameterValueUnitType.None;

        [JsonProperty("enumId")]
        public int EnumId { get; set; }

        [JsonProperty("sortOrderIdx")]
        public int SortOrderIdx { get; set; } = 0;

        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; } = false;

        [JsonProperty("isRequired")]
        public bool IsRequired { get; set; } = false;

        [JsonProperty("isMultiValue")]
        public bool IsMultiValue { get; set; } = false;

        [JsonProperty("formatInfo")]
        public string FormatInfo { get; set; } = string.Empty;

        [JsonProperty("isValueDeferred")]
        public bool IsValueDeferred { get; set; } = false;

        [JsonProperty("isVisible")]
        public bool IsVisible { get; set; } = true;

        [JsonProperty("values")]
        public List<MpAnalyticItemParameterValue> Values { get; set; } = new List<MpAnalyticItemParameterValue>();
    }

    public class MpAnalyticItemFormat {
        [JsonProperty("parameters")]
        public List<MpAnalyticItemParameterFormat> ParameterFormats { get; set; }
    }

    public class MpAnalyzerPluginFormat {
        public string guid { get; set; }
        public MpAnalyzerPluginInputFormat inputType { get; set; }
        public MpAnalyzerPluginOutputFormat outputType { get; set; }

        public string endpoint { get; set; }

        public List<MpAnalyticItemParameterFormat> parameters { get; set; }
        public List<MpAnalyzerPresetFormat> presets { get; set; }
    }    

    public class MpAnalyzerPluginBoxResponseValueFormat {
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }
    }

    public class MpAnalyzerPluginInputFormat {
        public bool plaintext { get; set; }
        public bool richtext { get; set; }
        public bool html { get; set; }
        public bool image { get; set; }
        public bool file { get; set; }
    }

    public class MpAnalyzerPluginOutputFormat {
        public bool text { get; set; }
        public bool image { get; set; }
        public bool box { get; set; }
        public bool file { get; set; }
    }

    public class MpAnalyzerPresetFormat {
        public string label { get; set; }
        public string description { get; set; }
        public List<MpAnalyzerPresetValueFormat> values { get; set; }
    }

    public class MpAnalyzerPresetValueFormat {
        public int enumId { get; set; }
        public string label { get; set; }
        public string value { get; set; }
    }

    public class MpAnalyzerPluginRequestValueFormat {
        public int enumId { get; set; }
        public string value { get; set; }
    }

    public class MpAnalyzerPluginResponseValueFormat {
        public string text { get; set; }
        public string imageBase64 { get; set; }
        public MpAnalyzerPluginBoxResponseValueFormat box { get; set; }
        public double decimalVal { get; set; }
        public int intVal { get; set; }
    }
}
