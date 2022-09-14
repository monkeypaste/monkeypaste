using System.Collections.Generic;

namespace MonkeyPaste.Common {
    public interface MpIPortableDataObject {
        Dictionary<MpPortableDataFormat, object> DataFormatLookup { get; }

        bool ContainsData(string format);
        object GetData(string format);
        void SetData(string format, object data);
    }
}
