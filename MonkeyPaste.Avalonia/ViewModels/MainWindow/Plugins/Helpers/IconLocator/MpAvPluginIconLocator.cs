using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginIconLocator {
        public static async Task<int> LocatePluginIconIdAsync(MpIParameterHostViewModel pluginHost, string overrideUri = null) {
            string uri = overrideUri == null ? pluginHost.PluginFormat.iconUri : overrideUri;
            var bytes = await MpFileIo.ReadBytesFromUriAsync(uri, pluginHost.PluginFormat.RootDirectory); ;
            var icon = await MpPlatform.Services.IconBuilder.CreateAsync(
                iconBase64: bytes.ToBase64String());

            return icon.Id;
        }
    }
}
