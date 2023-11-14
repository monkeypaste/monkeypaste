using Avalonia;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace OutSysTest {
    internal class Program {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) {
            //if (args.Contains("--wait-for-attach")) {
            //    Console.WriteLine("Attach debugger and use 'Set next statement'");
            //    while (true) {
            //        Thread.Sleep(100);
            //        if (Debugger.IsAttached)
            //            break;
            //    }
            //}
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}