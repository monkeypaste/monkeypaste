
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvAdScreenInfoCollection : MpAvScreenInfoCollectionBase {
        public MpAvAdScreenInfoCollection(IEnumerable<MpIPlatformScreenInfo> sil) : base(sil) {
        }
    }
}
