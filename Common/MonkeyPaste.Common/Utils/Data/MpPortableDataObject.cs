
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Common {

    public class MpPortableDataObject : MpIPortableDataObject {
        #region Properties

        public virtual Dictionary<MpPortableDataFormat, object> DataFormatLookup { get; private set; } = new Dictionary<MpPortableDataFormat, object>();
        
        #endregion

        public bool ContainsData(string format) {
            return GetData(format) != null;
        }

        public object GetData(string format) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                return null;
            }
            DataFormatLookup.TryGetValue(pdf, out object data);
            return data;
        }

        public virtual void SetData(string format, object data) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                throw new MpUnregisteredDataFormatException($"Format {format} is not registered");
            }
            DataFormatLookup.AddOrReplace(pdf, data);
        }

        public MpPortableDataObject() {
            DataFormatLookup = new Dictionary<MpPortableDataFormat, object>();
        }
        public MpPortableDataObject(string format, object data) : this() {
            SetData(format, data);
        }

    }
}
