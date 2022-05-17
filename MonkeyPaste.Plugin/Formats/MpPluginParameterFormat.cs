using System.Collections.Generic;

namespace MonkeyPaste.Plugin {

    public enum MpPluginParameterControlType {
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
        PasswordBox
    }

    public enum MpPluginParameterValueUnitType {
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
        ContentQuery,
        DelimitedPlainText,
        DelimitedContentQuery
    }

    public class MpPluginParameterFormat : MpJsonObject {
        public int paramId { get; set; } = 0;

        //paramName is intended to replace paramId and is a unique key for
        //a user supplied value passed into portable data object param of plugin
        //method
        public string paramName { get; set; }


        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;

        public MpPluginParameterControlType controlType { get; set; } = MpPluginParameterControlType.None;
        public MpPluginParameterValueUnitType unitType { get; set; } = MpPluginParameterValueUnitType.PlainText;

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

        public List<MpPluginParameterValueFormat> values { get; set; } = new List<MpPluginParameterValueFormat>();
    }


    public class MpPluginParameterValueFormat : MpJsonObject {
        public string value { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public bool isDefault { get; set; } = false;
    }

}
