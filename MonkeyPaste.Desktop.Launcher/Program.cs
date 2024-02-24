using System.Diagnostics;
using System.Reflection;

namespace MonkeyPaste.Desktop.Launcher {
    public static class Program {
        static void Main(string[] args) {
            string app_path =
                Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)),
                    "MonkeyPaste.Desktop",
                    "MonkeyPaste.Desktop.exe");
            Process.Start(app_path, "--loginload");
        }
    }
}
