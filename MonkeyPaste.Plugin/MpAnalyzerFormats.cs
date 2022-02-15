
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public interface MpIDescriptor {
        string label { get; set; }
        string description { get; set; }

        double score { get; set; }
    }

    public interface MpIImageDescriptorBox : MpIDescriptor {
        double x { get; set; }
        double y { get; set; }
        double width { get; set; }
        double height { get; set; }
    }

    public interface MpITextTokenDescriptorRange : MpIDescriptor {
        int rangeStart { get; set; }
        int rangeEnd { get; set; }
    }

    public interface MpITextDescriptor : MpIDescriptor {
        string content { get; set; }
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
        Base64Text,
        FilePath
    }

    public class MpAnalyzerPluginRequestItemFormat {
        public int enumId { get; set; } = 0;
        public string value { get; set; } = string.Empty;
    }

    public class MpAnalyzerPluginFormat {
        public MpHttpTransactionFormat http { get; set; }

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
        public bool imageToken { get; set; } = false;
        public bool textToken { get; set; } = false;
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

        public MpHttpTransactionFormat deferredValueTransaction { get; set; }
    }

    public class MpAnalyticItemParameterValue {
        public string value { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public bool isDefault { get; set; } = false;
        public bool isMinimum { get; set; } = false;
        public bool isMaximum { get; set; } = false;
    }

    public class MpComboBoxControlFormat {
        public string displayPath { get; set; }
        public string valuePath { get; set; }
    }

    public class MpSliderControlFormat {
        public string minimum { get; set; }
        public string maximum { get; set; }
        public string tickFrequency { get; set; }
    }

    public abstract class MpAnalyzerResponseValueFormatBase : MpIDescriptor {
        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public double score { get; set; } = 0;
    }

    public class MpAnalyzerPluginImageTokenResponseValueFormat : 
        MpAnalyzerResponseValueFormatBase, 
        MpIImageDescriptorBox {
        public double x { get; set; } = 0;
        public double y { get; set; } = 0;
        public double width { get; set; } = 0;
        public double height { get; set; } = 0;
    }

    public class MpAnalyzerPluginTextResponseValueFormat : 
        MpAnalyzerResponseValueFormatBase, MpITextDescriptor {
        //if JSONPath returns null value is constant string
        public string contentPath { get; set; }
        public string titlePath { get; set; }
        public string descriptionPath { get; set; }
        public string content { get; set; }
    }

    public class MpAnalyzerPluginTextTokenResponseValueFormat :
        MpAnalyzerResponseValueFormatBase, 
        MpITextTokenDescriptorRange {
        public int rangeStart { get; set; }
        public int rangeEnd { get; set; }
    }

    public class MpAnalyzerPresetFormat {
        public bool isDefault { get; set; } = false;

        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public List<MpAnalyzerPresetValueFormat> values { get; set; } = new List<MpAnalyzerPresetValueFormat>();
    }

    public class MpAnalyzerPresetValueFormat {
        public int enumId { get; set; } = 0;

        //public string label { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
