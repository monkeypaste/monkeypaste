using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpJsonExtensions {
        #region Serialize
        public static string SerializeObjectOmitNulls(this object obj) {
            return SerializeObject(obj, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }
        public static string SerializeObject(this object obj, JsonSerializerSettings settings = null) {
            if (obj == null) {
                return string.Empty;
            }
            try {
                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error serializing type '{obj.GetType()}'.", ex);
                return string.Empty;
            }
        }

        public static string SerializeObjectToBase64OmitNulls(this object obj, Encoding enc = null) {
            if (obj == null) {
                return string.Empty;
            }
            string jsonStr = SerializeObject(obj, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            string base64Str = jsonStr.ToBase64String(enc);
            return base64Str;
        }

        public static string SerializeObjectToBase64(this object obj, JsonSerializerSettings settings = null, Encoding enc = null) {
            if (obj == null) {
                return string.Empty;
            }
            string jsonStr = SerializeObject(obj, settings);
            string base64Str = jsonStr.ToBase64String(enc);
            return base64Str;
        }
        #endregion

        #region Deserialize
        public static MpPluginParameterRequestFormat ParseParamRequest(string json) {
            var req_lookup = json.DeserializeObject<Dictionary<string, object>>();
            if (req_lookup != null &&
                req_lookup.TryGetValue("items", out var itemsObj) && itemsObj is JArray items_jarray) {
                var param_lookup = new Dictionary<string, string>();
                foreach (var kvp_jtoken in items_jarray) {
                    if (kvp_jtoken.SelectToken("paramId", false) is JToken param_token &&
                        kvp_jtoken.SelectToken("paramValue", false) is JToken val_token) {

                        param_lookup.Add(param_token.Value<string>(), val_token.Value<string>());
                    }
                }
                return new MpPluginParameterRequestFormat() {
                    items = param_lookup.Select(x => new MpParameterRequestItemFormat(x.Key, x.Value)).ToList()
                };
            }
            return null;
        }

        public static T DeserializeObject<T>(this string obj, JsonSerializerSettings settings = null) where T : new() {
            if (obj is string objStr) {
                try {
                    return JsonConvert.DeserializeObject<T>(objStr, settings);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing str: " + objStr, ex);
                }

            }
            return new T();
        }

        public static T DeserializeBase64Object<T>(this string obj, Encoding enc = null) where T : new() {
            return DeserializeBase64Object_internal<T>(obj, enc, null);
        }
        private static T DeserializeBase64Object_internal<T>(object obj, Encoding enc = null, JsonSerializerSettings settings = null) where T : new() {
            if (obj is string objBase64Str && !string.IsNullOrWhiteSpace(objBase64Str)) {
                try {

                    //byte[] bytes = Convert.FromBase64String(objBase64Str);
                    //enc = enc == null ? Encoding.UTF8 : enc;
                    //string objStr = enc.GetString(bytes, 0, bytes.Length);

                    // NOTE ignoring encoding since string is base 64
                    string objStr = objBase64Str.ToStringFromBase64(enc);
                    return objStr.DeserializeObject<T>(settings);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing base64 str: " + objBase64Str, ex);
                }
            }
            return new T();
        }
        #endregion




    }
}
