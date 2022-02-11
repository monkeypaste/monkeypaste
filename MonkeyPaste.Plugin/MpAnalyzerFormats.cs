
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public interface MpIDescriptor {
        string Label { get; set; }
        string Description { get; set; }

        double Score { get; set; }
    }

    public interface MpIImageDescriptorBox : MpIDescriptor {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
    }

    public interface MpITextDescriptorRange : MpIDescriptor {
        int RangeStart { get; set; }
        int RangeEnd { get; set; }
    }

    public enum MpAnalyticItemParameterControlType {
        None = 0,
        Button,
        Text,
        ComboBox,
        CheckBox,
        Slider,
        Hidden
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
        Base64Text
    }

    public class MpAnalyzerPluginRequestItemFormat {
        public int enumId { get; set; } = 0;
        public string value { get; set; } = string.Empty;
    }

    public class MpAnalyzerPluginFormat {
        public string endpoint { get; set; } = string.Empty;
        public string apiKey { get; set; } = string.Empty;

        public MpAnalyzerPluginInputFormat inputType { get; set; } = null;
        public MpAnalyzerPluginOutputFormat outputType { get; set; } = null;

        public List<MpAnalyticItemParameterFormat> parameters { get; set; } = null;
        public List<MpAnalyzerPresetFormat> presets { get; set; } = null;
    }

    public class MpAnalyzerPluginInputFormat {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
    }

    public class MpAnalyzerPluginOutputFormat {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool box { get; set; } = false;
        public bool token { get; set; } = false;
        public bool file { get; set; } = false;
    }

    public class MpAnalyticItemParameterFormat {
        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public MpAnalyticItemParameterControlType parameterControlType { get; set; } = MpAnalyticItemParameterControlType.None;

        public MpAnalyticItemParameterValueUnitType parameterValueType { get; set; } = MpAnalyticItemParameterValueUnitType.None;

        public int enumId { get; set; } = 0;
        public int sortOrderIdx { get; set; } = 0;
        public bool isReadOnly { get; set; } = false;
        public bool isRequired { get; set; } = false;
        public bool isMultiValue { get; set; } = false;
        public string formatInfo { get; set; } = string.Empty; // may be used for additional validation
        public bool isValueDeferred { get; set; } = false; // TODO isValueDeferred is a placeholder and should be a seperate nullable json object for pulling values from http
        public bool isVisible { get; set; } = true;

        public List<MpAnalyticItemParameterValue> values { get; set; } = new List<MpAnalyticItemParameterValue>();
    }

    public class MpAnalyticItemParameterValue {
        public string value { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public bool isDefault { get; set; } = false;
        public bool isMinimum { get; set; } = false;
        public bool isMaximum { get; set; } = false;
    }

    public abstract class MpAnalyzerResponseValueFormatBase : MpIDescriptor {
        [JsonProperty("label")]
        public string Label { get; set; } = string.Empty;
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;
        [JsonProperty("score")]
        public double Score { get; set; } = 0;
    }

    public class MpAnalyzerPluginBoxResponseValueFormat : 
        MpAnalyzerResponseValueFormatBase, 
        MpIImageDescriptorBox {
        [JsonProperty("x")]
        public double X { get; set; } = 0;
        [JsonProperty("y")]
        public double Y { get; set; } = 0;
        [JsonProperty("width")]
        public double Width { get; set; } = 0;
        [JsonProperty("height")]
        public double Height { get; set; } = 0;
    }

    public class MpAnalyzerPluginTextResponseValueFormat :
        MpAnalyzerResponseValueFormatBase, 
        MpITextDescriptorRange {
        [JsonProperty("start")]
        public int RangeStart { get; set; }
        [JsonProperty("end")]
        public int RangeEnd { get; set; }
    }

    public class MpAnalyzerPresetFormat {
        public bool isDefault { get; set; } = false;

        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public List<MpAnalyzerPresetValueFormat> values { get; set; } = new List<MpAnalyzerPresetValueFormat>();
    }

    public class MpAnalyzerPresetValueFormat {
        public int enumId { get; set; } = 0;

        public string label { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
