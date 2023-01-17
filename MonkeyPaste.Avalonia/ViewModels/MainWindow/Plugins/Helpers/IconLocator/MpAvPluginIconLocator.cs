using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public static class MpAvPluginIconLocator {
        public static async Task<int> LocatePluginIconIdAsync(MpIParameterHostViewModel pluginHost, string overrideUri = null) {
            string uri = overrideUri == null ? pluginHost.PluginFormat.iconUri : overrideUri;
            var bytes = await MpFileIo.ReadBytesFromUriAsync(uri, pluginHost.PluginFormat.RootDirectory); ;
            var icon = await MpPlatformWrapper.Services.IconBuilder.CreateAsync(
                iconBase64: bytes.ToBase64String());

            return icon.Id;
        }
    }
}
