using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpJsonObject : MpIJsonObject, MpIJsonBase64Object {
        #region Statics

        public static T DeserializeObject<T>(object obj) where T: new(){
            if(obj is string objStr) {
                try {
                    return JsonConvert.DeserializeObject<T>(objStr);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing str: "+objStr, ex);
                }

            }
            return new T();
        }

        public static T DeserializeBase64Object<T>(object obj, Encoding enc = null) where T : new() {
            if (obj is string objBase64Str && !string.IsNullOrWhiteSpace(objBase64Str)) {
                try {

                    //byte[] bytes = Convert.FromBase64String(objBase64Str);
                    //enc = enc == null ? Encoding.UTF8 : enc;
                    //string objStr = enc.GetString(bytes, 0, bytes.Length);

                    // NOTE ignoring encoding since string is base 64
                    string objStr = objBase64Str.ToStringFromBase64(enc);
                    return JsonConvert.DeserializeObject<T>(objStr);
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing base64 str: "+objBase64Str, ex);
                }
            }
            return new T();
        }


        public static string SerializeObject(object obj, JsonSerializerSettings settings = null) {
            if(obj == null) {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string SerializeObjectToBase64JsonStr(object obj, JsonSerializerSettings settings = null, Encoding enc = null) {
            if (obj == null) {
                return string.Empty;
            }
            string jsonStr = SerializeObject(obj,settings);
            string base64Str = jsonStr.ToBase64String(enc);
            return base64Str;
        }
        #endregion

        public virtual string SerializeJsonObject() {
            return this.SerializeJsonObject(null);
        }
        public virtual string SerializeJsonObject(JsonSerializerSettings settings) {
            return SerializeObject(this,settings);
        }

        public virtual string SerializeJsonObjectToBase64(Encoding enc = null) {
            return this.SerializeJsonObjectToBase64(null, enc);
        }
        public virtual string SerializeJsonObjectToBase64(JsonSerializerSettings settings, Encoding enc = null) {
            return SerializeObjectToBase64JsonStr(this, settings, enc);
        }

        public virtual object Deserialize(string jsonMsgStr) {
            var JSONCovert = typeof(JsonConvert);
            var parameterTypes = new[] { typeof(string) };
            var deserializer = JSONCovert.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(i => i.Name.Equals("DeserializeObject", StringComparison.InvariantCulture))
                .Where(i => i.IsGenericMethod)
                .Where(i => i.GetParameters().Select(a => a.ParameterType).SequenceEqual(parameterTypes))
                .Single();

            return deserializer.Invoke(null, new object[] { jsonMsgStr });
        }

        public override string ToString() {            
            return SerializeJsonObject();
        }
        public string ToPrettyPrintJsonString() {
            return SerializeJsonObject().ToPrettyPrintJson();
        }

        public virtual object Clone() {
            string this_json = this.SerializeJsonObject();
            return JsonConvert.DeserializeObject(this_json);
        }
    }

    public abstract class MpJsonObject<T> where T:class {
        [JsonIgnore]
        public T Parent { get; set; }

    }
}
