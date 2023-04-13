using Avalonia;
using Avalonia.Browser;
using Avalonia.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Avalonia.Web;
using MonkeyPaste.Common;
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

[assembly: SupportedOSPlatform("browser")]

internal partial class Program {
    private static void Main(string[] args) {
#if BROWSER
        new MpAvBrWrapper().CreateDeviceInstance(null);

        BuildAvaloniaApp()
            .AfterSetup((arg) => {
                MpAvNativeWebViewHost.Implementation = new MpAvBrWebViewBuilder();
                //var dw = new MpAvBrWrapper();
                //dw.CreateDeviceInstance(null);
                //await dw.JsImporter.ImportAllAsync();
            }).SetupBrowserApp("out");
#endif
    }
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}