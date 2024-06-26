﻿using Avalonia.Controls;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvDesktopScreenInfoCollection : MpAvScreenInfoCollectionBase {

        public MpAvDesktopScreenInfoCollection(MpAvWindow w) :
            base(w.Screens.All.Select(x => new MpAvDesktopScreenInfo(x))) {
        }
    }
}
