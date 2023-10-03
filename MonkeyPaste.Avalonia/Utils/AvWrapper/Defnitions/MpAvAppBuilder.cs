using MonkeyPaste.Common;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppBuilder : MpIAppBuilder {
        public async Task<MpApp> CreateAsync(MpPortableProcessInfo pi) {
            if (pi == null) {
                // prob
                MpDebug.Break();
                return null;
            }

            string processPath = pi.ProcessPath;
            string appName = pi.ApplicationName;
            string iconBase64 = pi.MainWindowIconBase64;
            string args =
                pi.ArgumentList != null && pi.ArgumentList.Any() ?
                    string.Join(" ", pi.ArgumentList) :
                    null;

            if (!processPath.IsFile()) {
                MpDebug.Break($"Invalid process path detected '{processPath}'");
                return null;
            }
            var dupApp = await MpDataModelProvider.GetAppByMembersAsync(processPath, args, MpDefaultDataModelTools.ThisUserDeviceId);
            if (dupApp != null) {
                dupApp.WasDupOnCreate = true;
                return dupApp;
            }

            // GET APP ICON
            if (string.IsNullOrEmpty(iconBase64)) {
                iconBase64 = Mp.Services.IconBuilder.GetPathIconBase64(processPath, pi.Handle);
            }

            if (string.IsNullOrEmpty(iconBase64)) {
                MpConsole.WriteLine($" could not find icon for info (using question mark): ");
                MpConsole.WriteLine(pi.ToString());
                iconBase64 = MpBase64Images.QuestionMark;
            }

            var icon = await Mp.Services.IconBuilder.CreateAsync(iconBase64);

            var app = await MpApp.CreateAsync(
                appPath: processPath,
                appName: appName,
                arguments: args,
                iconId: icon.Id);

            return app;
        }
    }
}

