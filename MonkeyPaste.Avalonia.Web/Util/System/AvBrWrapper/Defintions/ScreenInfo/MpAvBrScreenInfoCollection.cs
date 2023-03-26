
using Avalonia.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public class MpAvBrScreenInfoCollection : MpAvScreenInfoCollectionBase {
        public MpAvBrScreenInfoCollection(IEnumerable<MpIPlatformScreenInfo> sil) : base(sil) {
        }
    }
}
