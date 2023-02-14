//using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public class MpAvWinIconBuilder : MpAvIconBuildBase {
        #region Public Methods

        public override string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            string base64 = MpAvWinPathIconHelper.GetIconBase64FromWindowsPath(appPath, (int)iconSize);
            if (base64.IsNullOrEmpty()) {
                return MpBase64Images.QuestionMark;
            }
            return base64;
        }

        #endregion
    }
}
