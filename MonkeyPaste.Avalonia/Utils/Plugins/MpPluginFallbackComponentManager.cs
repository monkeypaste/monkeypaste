using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpPluginFallbackComponentManager : MpIManagePluginComponents {
        public async Task<bool> InstallAsync(string pluginGuid, string packageUrl) {
            bool result = await MpPluginLoader.InstallPluginAsync(pluginGuid, packageUrl);
            return result;
        }

        public async Task<bool> BeginUpdateAsync(string pluginGuid, string packageUrl) {
            bool result = await MpPluginLoader.BeginUpdatePluginAsync(pluginGuid, packageUrl);
            return result;
        }

        public async Task<bool> UninstallAsync(string pluginGuid) {
            bool result = await MpPluginLoader.DeletePluginByGuidAsync(pluginGuid);
            return result;
        }
    }
}
