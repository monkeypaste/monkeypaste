using Newtonsoft.Json;
using System;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpJsonObject : MpIJsonObject, MpIJsonBase64Object, ICloneable {
        #region Statics


        #endregion

        public virtual string SerializeJsonObject() {
            return this.SerializeJsonObject(null);
        }
        public virtual string SerializeJsonObject(JsonSerializerSettings settings) {
            return MpJsonConverter.SerializeObject(this, settings);
        }

        public virtual string SerializeJsonObjectToBase64(Encoding enc = null) {
            return this.SerializeJsonObjectToBase64(null, enc);
        }
        public virtual string SerializeJsonObjectToBase64(JsonSerializerSettings settings, Encoding enc = null) {
            return MpJsonConverter.SerializeObjectToBase64JsonStr(this, settings, enc);
        }


        public override string ToString() {
            return SerializeJsonObject();
        }

        public virtual object Clone() {
            string this_json = this.SerializeJsonObject();
            return JsonConvert.DeserializeObject(this_json);
        }
    }

    public abstract class MpJsonObject<T> where T : class {
        [JsonIgnore]
        public T Parent { get; set; }

    }
}
