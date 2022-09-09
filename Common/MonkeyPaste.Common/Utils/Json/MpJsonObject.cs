using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {    
    public abstract class MpJsonObject : MpIJsonObject {
        public static T DeserializeObject<T>(object obj) where T: class{
            if(obj is string objStr) {
                return JsonConvert.DeserializeObject<T>(objStr);
            }
            return null;
        }

        public static T DeserializeBase64Object<T>(object obj) where T : class {
            if (obj is string objBase64Str) {
                byte[] bytes = Convert.FromBase64String(objBase64Str);
                string objStr = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                var outObj = JsonConvert.DeserializeObject<T>(objStr);
                return outObj;
            }
            return null;
        }

        public static string SerializeObject(object obj) {
            if(obj == null) {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj);
        }

        public string Serialize() {
            return JsonConvert.SerializeObject(this);
        }

        public string SerializeToByteString() {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Serialize()));
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
            
            return Serialize();
        }
    }

    public abstract class MpJsonObject<T> where T:class {
        [JsonIgnore]
        public T Parent { get; set; }

    }
}
