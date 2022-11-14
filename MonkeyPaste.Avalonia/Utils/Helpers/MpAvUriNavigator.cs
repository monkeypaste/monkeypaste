using System;
using System.Diagnostics;
using System.Threading;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public static class MpAvUriNavigator {
        public static void NavigateToUri(Uri uri) {
            if(uri.Scheme == Uri.UriSchemeFile) {
                NavigateToPath(uri.LocalPath);
                return;
            }
            

            if (OperatingSystem.IsWindows()) {
                Process.Start("explorer.exe", uri.AbsoluteUri);
            } else {
                using (var myProcess = new Process()) {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = uri.AbsoluteUri;
                    myProcess.Start();
                }
            }
        }

        public static void NavigateToPath(string path, bool useFileBrowser = true) {
            if (path.IsFile() && useFileBrowser) {
                path = path.FindParentDirectory();
            } 
            path = path.Contains(" ") ? $"\"{path}\"" : path;
            
            if (OperatingSystem.IsWindows()) {
                //using (var myProcess = new Process()) {
                //    myProcess.StartInfo.UseShellExecute = false;
                //    myProcess.StartInfo.FileName = "explorer.exe";
                //    if (path.IsFile()) {
                //        myProcess.StartInfo.ArgumentList.Add(@"/select");
                //    } else if (path.IsDirectory()) {
                //        myProcess.StartInfo.ArgumentList.Add(@"/open");
                //    }
                //    myProcess.StartInfo.ArgumentList.Add(path);
                //    myProcess.Start();
                //}
                Process.Start("explorer.exe", path);
            } else {
                using (var myProcess = new Process()) {
                    myProcess.StartInfo.UseShellExecute = true;
                    myProcess.StartInfo.FileName = path;
                    myProcess.Start();
                }
            }
        }
    }
}
