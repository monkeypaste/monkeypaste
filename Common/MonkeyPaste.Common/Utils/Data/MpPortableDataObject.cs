using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {

    public class MpPortableDataObject : MpJsonObject, MpIPortableDataObject {
        #region Statics

        public static MpPortableDataObject Parse(string json) {
            var mpdo = new MpPortableDataObject();
            var req_lookup = MpJsonConverter.DeserializeObject<Dictionary<string, object>>(json);
            foreach (var kvp in req_lookup) {
                mpdo.SetData(kvp.Key, kvp.Value);
            }
            return mpdo;
        }
        public static bool IsDataNotEqual(MpPortableDataObject dbo1, MpPortableDataObject dbo2) {
            if (dbo1 == null && dbo2 != null) {
                return true;
            }
            if (dbo1 != null && dbo2 == null) {
                return true;
            }
            if (dbo1.DataFormatLookup.Count != dbo2.DataFormatLookup.Count) {
                return true;
            }
            foreach (var nce in dbo2.DataFormatLookup) {
                try {
                    if (!dbo1.DataFormatLookup.ContainsKey(nce.Key)) {
                        return true;
                    }
                    if (nce.Value is byte[] newBytes &&
                        dbo1.DataFormatLookup[nce.Key] is byte[] oldBytes) {
                        if (!newBytes.SequenceEqual(oldBytes)) {
                            return true;
                        }
                    } else if (nce.Value is IEnumerable<string> valStrs &&
                                dbo1.DataFormatLookup[nce.Key] is IEnumerable<string> lastStrs) {
                        // must check actual string entries since the ref is always different 
                        if (valStrs.Count() != lastStrs.Count()) {
                            return true;
                        }
                        return valStrs.Any(x => !lastStrs.Contains(x));
                    } else {
                        if (!dbo1.DataFormatLookup[nce.Key].Equals(nce.Value)) {
                            return true;
                        }
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error comparing clipbaord data. ", ex);
                }


            }
            return false;
        }
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
        public MpPortableDataObject(Dictionary<string, object> formatDataLookup, bool caseSensitive = true) : this() {
            if (formatDataLookup != null) {
                formatDataLookup.ForEach(x => SetData(x.Key, x.Value));
            }
        }
        #endregion

        #region Public Methods

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

        public bool TryGetData(string format, out object data) {
            data = null;
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                return false;
            }
            if (DataFormatLookup.TryGetValue(pdf, out object tmp)) {
                data = tmp;
                return data != null;
            }
            return false;
        }
        public bool TryGetData<T>(string format, out T data) where T : class {
            if (TryGetData(format, out object dataObj)) {
                if (dataObj is T) {
                    data = dataObj as T;
                    return data != default(T);
                }
            }
            data = default;
            return false;
        }

        public virtual void SetData(string format, object data) {
            var pdf = MpPortableDataFormats.GetDataFormat(format);
            if (pdf == null) {
                throw new MpUnregisteredDataFormatException($"Format {format} is not registered");
                //MpConsole.WriteLine($"Warning! '{format}' data object format not registered at startup deteced. Attempting to add now..");
                //pdf = MpPortableDataFormats.RegisterDataFormat(format);
                //if (pdf == null) {
                //    throw new MpUnregisteredDataFormatException($"Format {format} cannot be registered");
                //}
            }

            if (data is string[] newStrArr &&
                TryGetData(format, out string[] curStrArr) &&
                    newStrArr.Any(x => !curStrArr.Contains(x))) {
                MpConsole.WriteLine($"String list ({format}) updated", true);
                MpConsole.WriteLine($"Old: {string.Join(Environment.NewLine, curStrArr)}");
                MpConsole.WriteLine($"New: {string.Join(Environment.NewLine, newStrArr)}", false, true);

                // update string list IN PLACE
                Array.Resize(ref curStrArr, newStrArr.Length);
                //newStrArr.CopyTo(curStrArr, 0);
                for (int i = 0; i < curStrArr.Length; i++) {
                    curStrArr[i] = newStrArr[i];
                }
                return;
            } else if (data is byte[] newBytes &&
                       TryGetData(format, out byte[] curBytes) &&
                        !curBytes.SequenceEqual(newBytes)) {
                // update image IN PLACE
                if (curBytes.ToDecodedString() == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT) {
                    MpConsole.WriteLine($"place holder for {format} replaced with '{newBytes.ToDecodedString()}'");
                }
                Array.Resize(ref curBytes, newBytes.Length);
                newBytes.CopyTo(curBytes, 0);
                DataFormatLookup[pdf] = curBytes;
                return;
            }
            DataFormatLookup.AddOrReplace(pdf, data);
        }


        public string SerializeData() {
            return MpJsonConverter.SerializeObject(DataFormatLookup.ToDictionary(x => x.Key.Name, x => (object)x.Value));
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine("-------------------------------------------------");
            foreach (var kvp in DataFormatLookup) {
                sb.AppendLine($"Format '{kvp.Key.Name}':");
                if (kvp.Value is IEnumerable<object> objl) {
                    objl.ForEach(x => sb.AppendLine(x.ToString()));
                } else {
                    sb.AppendLine($"'{kvp.Value.ToString()}'");
                }

            }
            sb.AppendLine("-------------------------------------------------");
            return sb.ToString();
        }

        #endregion
    }
}
