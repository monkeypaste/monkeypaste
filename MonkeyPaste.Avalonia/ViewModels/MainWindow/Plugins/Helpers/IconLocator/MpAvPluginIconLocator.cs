﻿using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginIconLocator {
        public static async Task<int> LocatePluginIconIdAsync(MpRuntimePlugin pf, string overrideUri = null, int timeout_ms = 30_000) {
            string uri = overrideUri == null ? pf.iconUri : overrideUri;
            //var bytes = await MpFileIo.ReadBytesFromUriAsync(uri, pf.ManifestDir, timeoutMs: timeout_ms);
            var bytes = await MpFileIo.ReadBytesFromUriAsync("icon.png", pf.ManifestDir, timeoutMs: timeout_ms);
            if (bytes == null || bytes.Length == 0) {
                // no icon provided or broken uri 
                if (MpAvPrefViewModel.Instance.DefaultPluginIconId == 0) {
                    // first need for default icon id, create and store in preferences so its not duplicated
                    var def_bmp = MpAvIconSourceObjToBitmapConverter.Instance.Convert("JigsawImage", null, null, null) as Bitmap;
                    var def_icon = await Mp.Services.IconBuilder.CreateAsync(iconBase64: def_bmp.ToBase64String());
                    MpAvPrefViewModel.Instance.DefaultPluginIconId = def_icon.Id;
                }
                return MpAvPrefViewModel.Instance.DefaultPluginIconId;
            }
            var icon = await Mp.Services.IconBuilder.CreateAsync(
                iconBase64: bytes.ToBase64String());
            if (icon == null) {
                return MpAvPrefViewModel.Instance.DefaultPluginIconId;
            }
            return icon.Id;
        }
    }
}
