using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpPortableDataObject : MpJsonObject, MpIPortableDataObject {
        #region Statics


        #endregion

        #region MpJsonObject Overrides

        #endregion

        #region Properties

        public virtual Dictionary<MpPortableDataFormat, object> DataFormatLookup { get; set; } = new Dictionary<MpPortableDataFormat, object>();

        #endregion


        #region Constructors

        public MpPortableDataObject() {
            DataFormatLookup = new Dictionary<MpPortableDataFormat, object>();
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
            return GetData(format) != null;
        }

        public virtual object GetData(string format) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                return null;
            }
            DataFormatLookup.TryGetValue(pdf, out object data);
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
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            MpDebug.Assert(pdf != null, $"Shouldn't ever be null anymore");

            //if (data is string[] newStrArr &&
            //    TryGetData(format, out string[] curStrArr) &&
            //        newStrArr.Any(x => !curStrArr.Contains(x))) {
            //    MpConsole.WriteLine($"String list ({format}) updated", true);
            //    MpConsole.WriteLine($"Old: {string.Join(Environment.NewLine, curStrArr)}");
            //    MpConsole.WriteLine($"New: {string.Join(Environment.NewLine, newStrArr)}", false, true);

            //    // update string list IN PLACE
            //    Array.Resize(ref curStrArr, newStrArr.Length);
            //    //newStrArr.CopyTo(curStrArr, 0);
            //    for (int i = 0; i < curStrArr.Length; i++) {
            //        curStrArr[i] = newStrArr[i];
            //    }
            //    return;
            //} 
            //else if (data is byte[] newBytes &&
            //           TryGetData(format, out byte[] curBytes) &&
            //            !curBytes.SequenceEqual(newBytes)) {
            //    // update image IN PLACE
            //    if (curBytes.ToDecodedString() == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT) {
            //        MpConsole.WriteLine($"place holder for {format} replaced with '{newBytes.ToDecodedString()}'");
            //    }
            //    Array.Resize(ref curBytes, newBytes.Length);
            //    newBytes.CopyTo(curBytes, 0);
            //    DataFormatLookup[pdf] = curBytes;
            //    return;
            //}
            DataFormatLookup.AddOrReplace(pdf, data);
        }


        public string SerializeData() {
            return MpJsonConverter.SerializeObject(DataFormatLookup.ToDictionary(x => x.Key.Name, x => (object)x.Value));
        }

        public override string ToString() {
            var sb = new StringBuilder();
            DataFormatLookup.ForEach(x => sb.Append(x.Key.Name + "|"));
            return sb.ToString();
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
