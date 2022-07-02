using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollection : MpIPlatformScreenInfoCollection {
        public IEnumerable<MpIPlatformScreenInfo> Screens { get; set; }

        public MpAvScreenInfoCollection() {
            Screens = new ObservableCollection<MpAvScreenInfo>();
        }
    }
}
