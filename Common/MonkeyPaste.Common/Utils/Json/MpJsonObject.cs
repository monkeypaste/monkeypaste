using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpJsonObject : MpIJsonObject, MpIJsonBase64Object {
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

        public static T DeserializeBase64Object<T>(object obj) where T : new() {
            if (obj is string objBase64Str) {
                try {
                    byte[] bytes = Convert.FromBase64String(objBase64Str);
                    string objStr = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                    return JsonConvert.DeserializeObject<T>(objStr);
                }catch(Exception ex) {
                    MpConsole.WriteTraceLine("Error deserializing base64 str: "+objBase64Str, ex);
                }
            }
            return new T();
        }


        public static string SerializeObject(object obj) {
            if(obj == null) {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj);
        }

        public string SerializeJsonObject() {
            return JsonConvert.SerializeObject(this);
        }

        public string SerializeJsonObjectToBase64() {
            string jsonStr = SerializeJsonObject();
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonStr);
            string jsonBase64 = Convert.ToBase64String(jsonBytes);
            return jsonBase64;
        }

        public object Deserialize(string jsonMsgStr) {
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
    }

    public abstract class MpJsonObject<T> where T:class {
        [JsonIgnore]
        public T Parent { get; set; }

    }
}
