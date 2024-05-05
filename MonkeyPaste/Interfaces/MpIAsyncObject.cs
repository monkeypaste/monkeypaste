using Newtonsoft.Json;

namespace MonkeyPaste {
    public interface MpIAsyncObject {
        [JsonIgnore]
        bool IsBusy { get; set; }
    }
    public interface MpIPassiveAsyncObject {
        bool IsBusy { get; set; }
    }
    public interface MpIAsyncCollectionObject {
        bool IsAnyBusy { get; }
    }
}
