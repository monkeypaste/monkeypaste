using Avalonia;
using MonkeyPaste.Avalonia;
//using Avalonia.ReactiveUI;
using System;

namespace MonkeyPaste.xplat.Desktop {
    internal class Program {
        public const string RESET_DATA_ARG = "resetdata";
        public const string BACKUP_DATA_ARG = "backupdata";
        public static string[] Args { get; private set; }
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        //public static void Main(string[] args) => BuildAvaloniaApp()
        //    .StartWithClassicDesktopLifetime(args);
        public static void Main(string[] args) {
            //#if DEBUG
            // if(args.Length > 0 && args[0] == "waitfordebugger") {
            //     MpConsole.WriteLine("Waiting for debugger...");
            //     Thread.Sleep(10000); // Wait 10 Seconds
            // }
            //#endif
            Args = args;
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
            //.StartWithCefNetApplicationLifetime(args);
        }
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        // .UseReactiveUI();
    }
}