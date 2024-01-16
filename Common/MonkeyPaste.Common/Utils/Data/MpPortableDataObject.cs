using MonkeyPaste.Common.Plugin;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpPortableDataObject : MpIPortableDataObject {
        #region Statics


        #endregion

        #region MpJsonObject Overrides

        #endregion

        #region Properties

        public virtual ConcurrentDictionary<string, object> DataFormatLookup { get; set; } = new ConcurrentDictionary<string, object>();

        #endregion


        #region Constructors

        public MpPortableDataObject() {
        }
        public MpPortableDataObject(string format, object data) : this() {
            SetData(format, data);
        }
        public MpPortableDataObject(Dictionary<string, object> formatDataLookup) : this() {
            if (formatDataLookup != null) {
                formatDataLookup.ForEach(x => SetData(x.Key, x.Value));
            }
        }
        #endregion

        #region Public Methods

        public bool ContainsData(string format) {
            return DataFormatLookup.ContainsKey(format);
        }

        public virtual object GetData(string format) {
            DataFormatLookup.TryGetValue(format, out object data);
            return data;
        }

        //public bool TryGetData(string format, out object data) {
        //    data = null;
        //    var pdf = MpPortableDataFormats.GetDataFormat(format);
        //    if (pdf == null) {
        //        return false;
        //    }
        //    if (DataFormatLookup.TryGetValue(pdf, out object tmp)) {
        //        data = tmp;
        //        return data != null;
        //    }
        //    return false;
        //}
        //public virtual bool TryGetData<T>(string format, out T data) where T : class {
        //    if (GetData(format) is object dataObj) {
        //        if (dataObj is T) {
        //            data = dataObj as T;
        //            return data != default(T);
        //        }
        //        if (dataObj is byte[] bytes) {
        //            if (typeof(T) == typeof(string)) {
        //                data = (T)(object)bytes.ToDecodedString();
        //                return true;
        //            }
        //        }
        //    }
        //    data = default;
        //    return false;
        //}

        public virtual void SetData(string format, object data) {
            if (!DataFormatLookup.TryAdd(format, data)) {
                DataFormatLookup[format] = data;
            }
        }
        public virtual bool Remove(string format) {
            return DataFormatLookup.TryRemove(format, out _);
        }


        public string SerializeData() {
            return MpJsonExtensions.SerializeObject(DataFormatLookup.ToDictionary(x => x.Key, x => (object)x.Value));
        }

        public override string ToString() {
            var sb = new StringBuilder();
            DataFormatLookup.ForEach(x => sb.Append(x.Key + " | "));
            return sb.ToString();
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
