using System.Collections.Concurrent;

namespace MonkeyPaste.Common {
    public interface MpIPortableDataObject {
        ConcurrentDictionary<string, object> DataFormatLookup { get; }

        bool ContainsData(string format);
        object GetData(string format);
        void SetData(string format, object data);
    }
}
