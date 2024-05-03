using System.Text.Json.Serialization;

namespace MonkeyPaste {
    public interface MpIAsyncObject {
        [JsonIgnore]
        bool IsBusy { get; }
    }
    public interface MpIPassiveAsyncObject {
        bool IsBusy { get; set; }
    }
    public interface MpIAsyncCollectionObject {
        bool IsAnyBusy { get; }
    }
}
