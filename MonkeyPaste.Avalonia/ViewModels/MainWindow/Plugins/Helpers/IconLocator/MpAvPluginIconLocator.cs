using Avalonia.Media.Imaging;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginIconLocator {
        public static async Task<int> LocatePluginIconIdAsync(MpIParameterHostViewModel pluginHost, string overrideUri = null) {
            string uri = overrideUri == null ? pluginHost.PluginFormat.iconUri : overrideUri;
            var bytes = await MpFileIo.ReadBytesFromUriAsync(uri, pluginHost.PluginFormat.RootDirectory);
            if (bytes == null || bytes.Length == 0) {
                // no icon provided or broken uri 
                if (MpPrefViewModel.Instance.DefaultPluginIconId == 0) {
                    // first need for default icon id, create and store in preferences so its not duplicated
                    var def_bmp = MpAvIconSourceObjToBitmapConverter.Instance.Convert("JigsawImage", null, null, null) as Bitmap;
                    var def_icon = await Mp.Services.IconBuilder.CreateAsync(iconBase64: def_bmp.ToBase64String());
                    MpPrefViewModel.Instance.DefaultPluginIconId = def_icon.Id;
                }
                return MpPrefViewModel.Instance.DefaultPluginIconId;
            }
            var icon = await Mp.Services.IconBuilder.CreateAsync(
                iconBase64: bytes.ToBase64String());

            return icon.Id;
        }
    }
}
