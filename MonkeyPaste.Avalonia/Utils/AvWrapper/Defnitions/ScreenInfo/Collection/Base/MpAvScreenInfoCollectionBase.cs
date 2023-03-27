
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvScreenInfoCollectionBase : MpIPlatformScreenInfoCollection {

        public ObservableCollection<MpIPlatformScreenInfo> Screens { get; protected set; }

        public MpAvScreenInfoCollectionBase(IEnumerable<MpIPlatformScreenInfo> sil) {
            Screens = new ObservableCollection<MpIPlatformScreenInfo>(sil);
        }

    }
}
