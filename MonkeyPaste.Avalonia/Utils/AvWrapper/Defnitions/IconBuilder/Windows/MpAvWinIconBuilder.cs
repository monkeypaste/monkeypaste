using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvWinIconBuilder : MpAvIconBuildBase {
        #region Public Methods

        public override string GetApplicationIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            string base64 = MpWinPathIconHelper.GetIconBase64FromWindowsPath(appPath, (int)iconSize);
            if(base64.IsNullOrEmpty()) {
                return MpBase64Images.QuestionMark;
            }
            return base64;
        }

        #endregion

        

    }
}
