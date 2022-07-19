using System;
using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvX11IconBuilder : MpAvIconBuildBase {
        public MpAvX11IconBuilder() {
        }

        public override string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            return MpAvX11PathIconHelper.GetIconBase64FromX11Path(appPath);
        }
    }
}

