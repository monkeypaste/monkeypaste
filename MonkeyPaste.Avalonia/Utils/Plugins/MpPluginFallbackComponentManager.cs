using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpPluginFallbackComponentManager : MpIManagePluginComponents {
        public async Task<bool> InstallAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm) {
            bool result = await MpPluginLoader.InstallPluginAsync(pluginGuid, packageUrl, false, cpivm);
            return result;
        }

        public async Task<bool> BeginUpdateAsync(string pluginGuid, string packageUrl, MpICancelableProgressIndicatorViewModel cpivm) {
            bool result = await MpPluginLoader.BeginUpdatePluginAsync(pluginGuid, packageUrl, cpivm);
            return result;
        }

        public async Task<bool> UninstallAsync(string pluginGuid) {
            bool result = await MpPluginLoader.DeletePluginByGuidAsync(pluginGuid);
            return result;
        }
    }
}
