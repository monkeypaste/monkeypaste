﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpAnalyticItemParameterType {
        None = 0,
        Button,
        Text,
        ComboBox,
        CheckBox,
        Slider,
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

    public class MpAnalyticItemParameter {
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
        public List<MpAnalyticItemParameter> ParameterFormats { get; set; }
    }
}