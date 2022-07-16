using System;
using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvMacIconBuilder : MpAvIconBuildBase {
        public MpAvMacIconBuilder() {
        }

        public override string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            return MpAvMacPathIconHelper.GetIconBase64FromMacPath(appPath);
        }
    }
}

