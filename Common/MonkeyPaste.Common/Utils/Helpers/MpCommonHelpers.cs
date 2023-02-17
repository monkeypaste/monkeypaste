using System;
using System.IO;

namespace MonkeyPaste.Common {
    public static class MpCommonHelpers {
        public static string GetExecutingDir() {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        public static string GetExecutingPath() {
            return Path.Combine(GetExecutingDir(), System.Reflection.Assembly.GetExecutingAssembly().FullName);
        }
        public static string GetSolutionDir() {
            string solution_path = Environment.CurrentDirectory.FindParentDirectory("MonkeyPaste");
            return solution_path;
        }


        public static string NewLineByEnv(MpUserDeviceType deviceType) {
            switch (deviceType) {
                case MpUserDeviceType.Windows:
                    return "\r\n";
                default:
                    return "\n";
            }
        }
    }
}
