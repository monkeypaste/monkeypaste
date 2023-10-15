using MonkeyPaste.Common.Plugin;
//using Newtonsoft.Json;
//using SQLite;

namespace MonkeyPaste.Avalonia {
    public class MpAvMissingParameterViewModel : MpAvParameterViewModelBase {
        public MpAvMissingParameterViewModel() : this(null) { }
        public MpAvMissingParameterViewModel(MpAvViewModelBase parent) : base(null) {
            ParameterFormat = new MpParameterFormat() {
                isVisible = false
            };
        }
    }
}
