using MonkeyPaste.Common.Plugin;
using System;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Common {
    public static class MpX11ShellHelpers {
        private static string _shellTestString_cmdstr;

        public static string GetLauncherProperty(string appPath,string propertyName) {
            try {
                string exe_name = Path.GetFileName(appPath);
                string exec_match_val = $"Exec={exe_name}";

                string desktop_dir = "/usr/share/applications";
                var dpl = new DirectoryInfo(desktop_dir)
                    .GetFiles("*.desktop").Select(x => x.FullName);

                string prop_val = null;
                foreach (string dp in dpl) {
                    // read desktop file
                    string dp_pt = MpFileIo.ReadTextFromFile(dp);
                    if (dp_pt.Contains(exec_match_val)) {
                        // desktop file contains exec match
                        if (dp_pt.SplitNoEmpty($"{propertyName}=") is { } propParts &&
                            propParts.Length > 1 &&
                            propParts.Skip(1).FirstOrDefault() is { } propValPart1 &&
                            propValPart1.SplitByLineBreak().FirstOrDefault() is { } propValPart2 &&
                            !propValPart2.IsNullOrEmpty()) {
                            // found "Icon=<icon name>" 
                            prop_val = propValPart2;
                            break;
                        }
                    }
                }
                return prop_val;
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error reading launcher prop '{propertyName}' for app path '{appPath}'.", ex);
            }
            return null;
        }

        public static string GetExeWithArgsToExePath(string exeWithArgsStr) {
            if (string.IsNullOrEmpty(exeWithArgsStr)) {
                return string.Empty;
            }
            return $"which {exeWithArgsStr}".ShellExec().Trim();
        }

        public static string GetCleanShellStr(string shellStr) {
            string shell_str_type = TestShellString(shellStr);
            if (shell_str_type == "UNKNOWN") {
                throw new System.Exception("Unknown shell string: " + shellStr);
            }
            if (shell_str_type == "EXECUTABLE") {
                return GetExeWithArgsToExePath(shellStr);
            }
            return shellStr;
        }

        private static string TestShellString(string strToTest) {
            if (_shellTestString_cmdstr == null) {
                // from https://stackoverflow.com/a/45899525/105028
                _shellTestString_cmdstr = Environment.NewLine;
                _shellTestString_cmdstr += $"F_NAME=\"${1}\"" + Environment.NewLine;
                _shellTestString_cmdstr += "if test -f \"${F_NAME}\"" + Environment.NewLine;
                _shellTestString_cmdstr += "then" + Environment.NewLine;
                _shellTestString_cmdstr += "echo \"FILE\"" + Environment.NewLine;
                _shellTestString_cmdstr += "elif test -d \"${F_NAME}\"" + Environment.NewLine;
                _shellTestString_cmdstr += "then" + Environment.NewLine;
                _shellTestString_cmdstr += "echo \"DIRECTORY\"" + Environment.NewLine;
                _shellTestString_cmdstr += "else" + Environment.NewLine;
                _shellTestString_cmdstr += "echo \"UNKNOWN\"" + Environment.NewLine;
                _shellTestString_cmdstr += "fi" + Environment.NewLine;
            }
            string result = $"sh -c `{_shellTestString_cmdstr}` {strToTest}".ShellExec();
            if (result == "FILE") {
                bool isExecutable = !string.IsNullOrEmpty(GetExeWithArgsToExePath(strToTest));
                if (isExecutable) {
                    return "EXECUTABLE";
                }
            }
            return result;
        }
    }
}