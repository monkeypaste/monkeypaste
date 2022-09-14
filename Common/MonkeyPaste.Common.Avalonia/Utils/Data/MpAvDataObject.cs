using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public class MpAvDataFormat : MpPortableDataFormat {
        public MpAvDataFormat(string name, int id, string portable) : base(name,id) { }
    }

    public class MpAvDataObject : MpPortableDataObject, IDataObject {
        public override void SetData(string format, object data) {
            if(MpAvDataFormats.IsFormatOverride(format)) {
                if(format == MpPortableDataFormats.FileDrop) {
                    data = data.ToString().Split(new string[] {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries) as IEnumerable<string>;
                } else if(format == MpPortableDataFormats.Html || format == MpPortableDataFormats.Rtf) {
                    data = Encoding.UTF8.GetBytes(data.ToString());
                }
            }
            base.SetData(format, data);
        }

        #region Avalonia.Input.IDataObject Implementation

        IEnumerable<string> IDataObject.GetDataFormats() {
            return DataFormatLookup.Select(x => x.Key.Name);
        }

        bool IDataObject.Contains(string dataFormat) {
            return ContainsData(dataFormat);
        }

        string IDataObject.GetText() { 
            return GetData(MpPortableDataFormats.Text) as string;
        }

        IEnumerable<string> IDataObject.GetFileNames() {

            if(GetData(MpAvDataFormats.AvFileNames) is IEnumerable<string> files) {
                return files;
            }
            if(GetData(MpPortableDataFormats.FileDrop) is string filesStr) {
                return filesStr.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            return null;
        }

        object IDataObject.Get(string dataFormat) {
            return GetData(dataFormat);
        }

        #endregion

    }
}
