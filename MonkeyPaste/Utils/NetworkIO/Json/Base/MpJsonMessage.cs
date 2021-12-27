using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpJsonMessage {
        //public sealed string GetMessage(string[] args) {
        //    var jsonProps = this.GetType()
        //            .GetProperties()
        //            .Where(x => 
        //                x.GetCustomAttribute<JsonPropertyAttribute>() != null)
        //                 .OrderBy(x=>x.GetCustomAttribute<JsonPropertyAttribute>().Order);

        //    foreach(var jsonProp in jsonProps) {

        //    }
        //}

        public string Serialize() {
            return JsonConvert.SerializeObject(this);
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
}
