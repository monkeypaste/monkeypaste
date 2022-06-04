using Newtonsoft.Json;
using System;
using System.Diagnostics;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpJsonEnumConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            if (!objectType.IsEnum) {
                var underlyingType = Nullable.GetUnderlyingType(objectType);
                if (underlyingType != null && underlyingType.IsEnum) {
                    objectType = underlyingType;
                }
            }

            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (!objectType.IsEnum) {
                var underlyingType = Nullable.GetUnderlyingType(objectType);
                if (underlyingType != null && underlyingType.IsEnum) {
                    objectType = underlyingType;
                }
            } 

            var value = reader.Value;

            string strValue;
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) {
                if (existingValue == null || Nullable.GetUnderlyingType(existingValue.GetType()) != null) {
                    return null;
                }
                strValue = "0";
            } else {
                strValue = value.ToString();
            }


            int intValue;
            if (int.TryParse(strValue, out intValue)) {
                return Enum.ToObject(objectType, intValue);
            } 

            return Enum.Parse(objectType, strValue);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

    public class MpJsonPathPropertyConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(MpJsonPathProperty);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {                        
            if (reader.Value == null) {
                return null;
            }
            
            return Activator.CreateInstance(objectType, reader.Value);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
