using Newtonsoft.Json;
using System.Text;

namespace MonkeyPaste.Common {
    public abstract class MpJsonObject : MpIJsonObject, MpIJsonBase64Object {
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
    }
    public abstract class MpOmitNullJsonObject : MpJsonObject {
        public override string SerializeJsonObject() {

            return base.SerializeJsonObject(new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
        public override string SerializeJsonObjectToBase64(Encoding enc = null) {
            return base.SerializeJsonObjectToBase64(new JsonSerializerSettings() {
                NullValueHandling = NullValueHandling.Ignore
            }, enc);
        }
    }
}
