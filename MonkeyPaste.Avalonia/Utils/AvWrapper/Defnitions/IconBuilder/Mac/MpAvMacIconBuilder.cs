using System;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvMacIconBuilder : MpAvIconBuildBase {
        public MpAvMacIconBuilder() {
        }

        public override string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            return MacHelper.GetIconBase64FromPath(appPath);
        }
    }
}

