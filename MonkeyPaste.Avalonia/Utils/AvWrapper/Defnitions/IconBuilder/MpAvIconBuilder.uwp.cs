﻿#if WINDOWS

using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder {

        public string GetPathIconBase64(string path, MpIconSize iconSize) =>
            GetPathIconBase64(path, IntPtr.Zero, iconSize);

        public string GetPathIconBase64(string path, nint handle, MpIconSize iconSize) {
            if (string.IsNullOrEmpty(path)) {
                return null;
            }
            string base64 = null;
            if (path.ToLowerInvariant().EndsWith("bin") &&
                handle != IntPtr.Zero) {
                // handling weird case of libre office pointing process path to .bin not actual app so using handle here
                base64 = MpAvWinPathIconHelper.GetIconBase64FromHandle(handle);

            }
            if (string.IsNullOrEmpty(base64)) {
                base64 = MpAvWinPathIconHelper.GetIconBase64FromWindowsPath(path, (int)iconSize);
            }
            if (string.IsNullOrEmpty(base64)) {
                return MpBase64Images.QuestionMark;
            }
            return base64;
        }
    }
}

#endif