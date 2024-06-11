using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonkeyPaste.Avalonia {
    public interface MpIViewModel : MpIAsyncObject {

        [JsonIgnore]
        bool HasModelChanged { get; set; }

        void OnPropertyChanged(
            [CallerMemberName] string propertyName = null,
            [CallerFilePath] string path = null,
            [CallerMemberName] string memName = null,
            [CallerLineNumber] int line = 0);

        event PropertyChangedEventHandler PropertyChanged;
    }
}
