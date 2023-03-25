using Avalonia;
using Avalonia.Browser;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Avalonia.Web;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program {
    private static void Main(string[] args) {
        BuildAvaloniaApp()
            .AfterSetup(_ => {
                //new MpAvBrWrapper().CreateDeviceInstance(this);
                //MpAvNativeWebViewHost.Implementation = new MpAvAdWebViewBuilder();
                MpAvNativeWebViewHost.Implementation = new MpAvBrWebViewBuilder();
            }).SetupBrowserApp("out");
    }
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}