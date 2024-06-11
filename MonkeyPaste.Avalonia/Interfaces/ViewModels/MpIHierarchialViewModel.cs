using Newtonsoft.Json;

namespace MonkeyPaste.Avalonia {
    public interface MpIHierarchialViewModel : MpIViewModel {
        [JsonIgnore]
        object ParentObj { get; }
    }
}
