
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {

    public class MpPortableDataObject : MpIPortableDataObject {
        #region Statics


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

        public override string ToString() {
            var sb = new StringBuilder();
            foreach(var kvp in DataFormatLookup) {
                sb.AppendLine($"Format '{kvp.Key.Name}':");
                sb.AppendLine($"'{kvp.Value.ToString()}'");
            }
            return sb.ToString();
        }
    }
}
