using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpCommonHelpers {
        public static string GetExecutingDir() {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        public static string GetSolutionDir() {
            string solution_path = Environment.CurrentDirectory.FindParentDirectory("MonkeyPaste");
            return solution_path;
        }


        public static string NewLineByEnv(MpUserDeviceType deviceType) {
            switch(deviceType) {
                case MpUserDeviceType.Windows:
                    return "\r\n";
                default:
                    return "\n";
            }
        }
    }
}
