using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonkeyPaste.Common {
    public static class MpCommonHelpers {
        public static string GetExecutingDir() {
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }
        public static string GetSolutionDir() {
            string solution_path = Environment.CurrentDirectory.FindParentDir("MonkeyPaste");
            return solution_path;
        }

        public static string FindParentDir(this string curPath, string projectName) {
            string rootPath = Path.GetPathRoot(curPath);
            string curDirName = Path.GetFileName(curPath);
            while (curDirName != projectName) {
                if (curPath == rootPath) {
                    throw new DirectoryNotFoundException("Could not find the project directory.");
                }
                curPath = Directory.GetParent(curPath).FullName;
                curDirName = Path.GetFileName(curPath);
            }
            return curPath;
        }

        public static string NewLineByEnv(MpUserDeviceType deviceType) {
            switch(deviceType) {
                case MpUserDeviceType.Windows:
                    return "/r/n";
                default:
                    return "/n";
            }
        }
    }
}
