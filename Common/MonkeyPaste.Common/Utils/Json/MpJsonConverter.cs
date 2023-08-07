using Newtonsoft.Json;
using System;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpJsonConverter {
        public static T DeserializeObject<T>(object obj) where T : new() {
            if (obj is string objStr) {
                try {
                    return JsonConvert.DeserializeObject<T>(objStr);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing str: " + objStr, ex);
                }

            }
            return new T();
        }

        public static T DeserializeBase64Object<T>(object obj, Encoding enc = null, JsonSerializerSettings settings = null) where T : new() {
            if (obj is string objBase64Str && !string.IsNullOrWhiteSpace(objBase64Str)) {
                try {

                    //byte[] bytes = Convert.FromBase64String(objBase64Str);
                    //enc = enc == null ? Encoding.UTF8 : enc;
                    //string objStr = enc.GetString(bytes, 0, bytes.Length);

                    // NOTE ignoring encoding since string is base 64
                    string objStr = objBase64Str.ToStringFromBase64(enc);
                    return JsonConvert.DeserializeObject<T>(objStr, settings);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing base64 str: " + objBase64Str, ex);
                }
            }
            return new T();
        }


        public static string SerializeObject(object obj, JsonSerializerSettings settings = null) {
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

        public static string SerializeObjectToBase64JsonStr(object obj, JsonSerializerSettings settings = null, Encoding enc = null) {
            if (obj == null) {
                return string.Empty;
            }
            string jsonStr = SerializeObject(obj, settings);
            string base64Str = jsonStr.ToBase64String(enc);
            return base64Str;
        }
    }
}
