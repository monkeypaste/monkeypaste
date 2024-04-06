using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MonkeyPaste.Desktop.Launcher {
    public static class Program {
        static void Main(string[] args) {
            string app_path =
                Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                    "MonkeyPaste.Desktop",
                    "MonkeyPaste.Desktop.exe");

            string launch_args = args == null || args.Length == 0 ?
                "--loginload" : string.Join(" ", args);
            if (launch_args == "--restarted") {
                Thread.Sleep(3_000);
            }
            Process.Start(app_path, launch_args);
        }
    }
}
