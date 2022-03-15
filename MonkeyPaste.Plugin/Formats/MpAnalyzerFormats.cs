
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {

    public enum MpAnalyticItemParameterControlType {
        None = 0,
        Button,
        TextBox,
        ComboBox,
        List,
        MultiSelectList,
        EditableList,
        CheckBox,
        Slider,
        FileChooser,
        DirectoryChooser
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
        FileSystemPath,
        ContentQuery
    }

    public class MpAnalyzerPluginRequestFormat : MpJsonObject {
        public List<MpAnalyzerPluginRequestItemFormat> items { get; set; } = new List<MpAnalyzerPluginRequestItemFormat>();
    }

    public class MpAnalyzerPluginRequestItemFormat : MpJsonObject {
        public int paramId { get; set; } = 0;
        public string value { get; set; } = string.Empty;
    }

    public class MpAnalyzerPluginFormat : MpJsonObject {
        public MpHttpTransactionFormat http { get; set; }

        public MpAnalyzerPluginInputFormat inputType { get; set; } = null;
        public MpAnalyzerPluginOutputFormat outputType { get; set; } = null;

        public List<MpAnalyticItemParameterFormat> parameters { get; set; } = null;
        public List<MpAnalyzerPresetFormat> presets { get; set; } = null;
    }

    public class MpAnalyzerPluginInputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
    }

    public class MpAnalyzerPluginOutputFormat : MpJsonObject {
        public bool text { get; set; } = false;
        public bool image { get; set; } = false;
        public bool file { get; set; } = false;
        public bool imageToken { get; set; } = false;
        public bool textToken { get; set; } = false;
    }

    public class MpAnalyticItemParameterFormat : MpJsonObject {
        public int paramId { get; set; } = 0;

        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public MpAnalyticItemParameterControlType controlType { get; set; } = MpAnalyticItemParameterControlType.None;
        public MpAnalyticItemParameterValueUnitType unitType { get; set; } = MpAnalyticItemParameterValueUnitType.PlainText;

        public bool isVisible { get; set; } = true;
        public bool isReadOnly { get; set; } = false;
        public bool isRequired { get; set; } = false;

        //TextBox
        public int maxLength { get; set; } = int.MaxValue;
        public int minLength { get; set; } = 0;
        public string illegalCharacters { get; set; } = null;

        //Slider
        public double minimum { get; set; } = double.MinValue;
        public double maximum { get; set; } = double.MaxValue;
        public int precision { get; set; } = 2;

        public List<MpAnalyticItemParameterValueFormat> values { get; set; } = new List<MpAnalyticItemParameterValueFormat>();
    }

    public class MpAnalyticItemParameterValueFormat : MpJsonObject {
        public string value { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public bool isDefault { get; set; } = false;
    }

    public class MpAnalyzerPresetFormat : MpJsonObject {
        public string guid { get; set; }

        public bool isDefault { get; set; } = false;

        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public List<MpAnalyzerPresetValueFormat> values { get; set; } = new List<MpAnalyzerPresetValueFormat>();
    }

    public class MpAnalyzerPresetValueFormat : MpJsonObject {
        public int paramId { get; set; } = 0;
        public string value { get; set; } = string.Empty;
    }


}
