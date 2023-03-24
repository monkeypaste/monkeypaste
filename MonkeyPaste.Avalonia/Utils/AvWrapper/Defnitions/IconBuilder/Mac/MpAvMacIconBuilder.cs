#if MAC
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvMacIconBuilder : MpAvIconBuildBase {
        public MpAvMacIconBuilder() {
        }

        public override string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            if (string.IsNullOrEmpty(appPath)) {
                return null;
            }
            return MpAvMacPathIconHelper.GetIconBase64FromMacPath(appPath);
        }
    }
}

#endif