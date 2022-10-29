using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;

namespace MonkeyPaste {
    public static class MpFileIo {
        public static void OpenFileBrowser(string path, string browserFileName = null) {
            if(!path.IsFileOrDirectory()) {
                MpPlatformWrapper.Services.NativeMessageBox.ShowOkCancelMessageBox($"Error", $"{path} not found");
                path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }

            if(string.IsNullOrEmpty(browserFileName)) {
                if(MpPlatformWrapper.Services.OsInfo.OsType == MpUserDeviceType.Windows) {
                    browserFileName = "explorer.exe";
                } else {
                    throw new Exception("need file browser paths for non-windows os here");
                }
            }

            string args = path;
            if(File.Exists(path)) {
                // when path is file and not folder add select arg to process start
                args = string.Format("/select,\"{0}\"", path);
            }
            ProcessStartInfo startInfo = new ProcessStartInfo {
                Arguments = args,
                FileName = browserFileName
            };

            Process.Start(startInfo);
        }

        public static double FileListSize(string[] paths) {
            long total = 0;
            foreach (string path in paths) {
                if (Directory.Exists(path)) {
                    total += CalcDirSize(path, true);
                } else if (File.Exists(path)) {
                    total += new FileInfo(path).Length;
                }
            }
            return ConvertBytesToMegabytes(total);
        }

        private static long CalcDirSize(string sourceDir, bool recurse = true) {
            return CalcDirSizeHelper(new DirectoryInfo(sourceDir), recurse);
        }

        private static long CalcDirSizeHelper(DirectoryInfo di, bool recurse = true) {
            long size = 0;
            FileInfo[] fiEntries = di.GetFiles();
            foreach (var fiEntry in fiEntries) {
                Interlocked.Add(ref size, fiEntry.Length);
            }

            if (recurse) {
                DirectoryInfo[] diEntries = di.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                System.Threading.Tasks.Parallel.For<long>(
                    0,
                    diEntries.Length,
                    () => 0,
                    (i, loop, subtotal) => {
                        if ((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) {
                            return 0;
                        }
                        subtotal += CalcDirSizeHelper(diEntries[i], true);
                        return subtotal;
                    },
                    (x) => Interlocked.Add(ref size, x));
            }
            return size;
        }
        public static string GetAbsolutePath(string path) {
            return GetAbsolutePath(null, path);
        }

        public static string GetAbsolutePath(string basePath, string path) {
            if (path == null) {
                return null;
            }
            if (basePath == null) {
                basePath = Path.GetFullPath("."); // quick way of getting current working directory
            } else {
                basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
            }

            string finalPath;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path))) {
                if (path.StartsWith(Path.DirectorySeparatorChar.ToString())) {
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                } else {
                    finalPath = Path.Combine(basePath, path);
                }
            } else {
                finalPath = path;
            }
            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath(finalPath);
        }

        public static string ToFile(this string fileData, string forceDir = "", string forceNamePrefix = "", string forceExt = "", bool overwrite = false, bool isTemporary = true) {
            // NOTE csv is too annoying to discern (probably all are) need to check before and force/convert
            forceExt = string.IsNullOrEmpty(forceExt) ? forceExt : forceExt.Replace(".", string.Empty);

            if (string.IsNullOrEmpty(forceExt)) {
                // when ext is not given infer from content
                if (fileData.IsStringRichText()) {
                    forceExt = "rtf";
                } else if (fileData.IsStringBase64()) {
                    forceExt = "png";
                } /*else if (fileData.IsStringCsv()) {
                    forceExt = "csv";
                }*/ else if (fileData.IsStringRichHtmlText()) {
                    forceExt = "html";
                } else if (!fileData.IsFileOrDirectory()) {
                    forceExt = "txt";
                }
            } else {
                if (forceExt.ToLower().Equals("rtf")) {
                    fileData = MpPlatformWrapper.Services.StringTools.ToRichText(fileData);
                } else if (forceExt.ToLower().Equals("txt")) {
                    fileData = MpPlatformWrapper.Services.StringTools.ToPlainText(fileData);
                } else if (forceExt.ToLower().Equals("csv")) {
                    fileData = MpPlatformWrapper.Services.StringTools.ToCsv(fileData);
                } else if (forceExt.ToLower().Equals("html")) {
                    // add to string tools
                    Debugger.Break();
                }
            }

            string tfp;
            if (fileData.IsFileOrDirectory()) {
                tfp = fileData;
            } else if (forceExt == "png" ||
                       forceExt.ToLower().Equals("bmp") ||
                       forceExt.ToLower().Equals("jpg") ||
                       forceExt.ToLower().Equals("jpeg")) {
                tfp = WriteByteArrayToFile(Path.GetTempFileName(), fileData.ToByteArray(), isTemporary);
            } else {
                tfp = WriteTextToFile(Path.GetTempFileName(), fileData, isTemporary);
            }
            string ofp = tfp;

            if (!string.IsNullOrEmpty(forceNamePrefix)) {
                forceNamePrefix = forceNamePrefix.RemoveInvalidFileNameChars();
                string tfnwe = Path.GetFileName(tfp);
                string ofnwe = forceNamePrefix + Path.GetExtension(tfp);
                ofp = ofp.Replace(tfnwe, ofnwe);
            }

            if (!string.IsNullOrEmpty(forceExt)) {
                string tfe = Path.GetExtension(tfp);
                ofp = ofp.Replace(tfe, "." + forceExt.Replace(".",string.Empty));
            }

            if (!string.IsNullOrEmpty(forceDir)) {
                if (!Directory.Exists(forceDir)) {
                    throw new Exception("Directory not found: " + forceDir);
                }
                string tfd = Path.GetDirectoryName(tfp);
                ofp = ofp.Replace(tfd, forceDir);
            }
            if (ofp.ToLower() != tfp.ToLower()) {
                if (ofp.IsFileOrDirectory() && !overwrite) {
                    if (string.IsNullOrEmpty(forceDir)) {
                        // this means file is going to write to temp folder and to avoid IO or name issues preserve name
                        // but put in random subdirectory of temp folder
                        string randomSubDirPath = Path.Combine(Path.GetDirectoryName(ofp), Path.GetRandomFileName());
                        try {
                            Directory.CreateDirectory(randomSubDirPath);
                            if (isTemporary && randomSubDirPath.IsUnderTemporaryFolder()) {
                                MpTempFileManager.AddTempFilePath(randomSubDirPath);
                            }

                            ofp = Path.Combine(randomSubDirPath, Path.GetFileName(ofp));
                        }
                        catch (Exception ex) {
                            MpConsole.WriteTraceLine("Error creating random temp subdirectory: " + ex);
                            ofp = GetUniqueFileOrDirectoryName(Path.GetDirectoryName(ofp), Path.GetFileName(ofp));
                        }
                    } else {
                        ofp = GetUniqueFileOrDirectoryName(Path.GetDirectoryName(ofp), Path.GetFileName(ofp));
                    }

                }
                // move temporary file to processed output file path and delete temporary
                try {
                    // TODO figure out how to handle recrusive directory copy for this case for now don't do it
                    ofp = CopyFileOrDirectory(tfp, ofp,false,isTemporary,overwrite);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error copying temp file '{tfp}' to '{ofp}', returning temporary. Exception: " + ex);
                    return tfp;
                }
                if (tfp.IsUnderTemporaryFolder()) {
                    MpTempFileManager.AddTempFilePath(tfp);
                }
            }
            return ofp;
        }
        public static double ConvertBytesToMegabytes(long bytes, int precision = 2) {
            return Math.Round((bytes / 1024f) / 1024f, precision);
        }

        public static double ConvertMegaBytesToBytes(long megabytes, int precision = 2) {
            return Math.Round((megabytes * 1024f) * 1024f, precision);
        }

        public static double GetFileSizeInBytes(string filePath) {
            try {
                if (File.Exists(filePath)) {
                    FileInfo fi = new FileInfo(filePath);
                    return fi.Length;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error checking size of path {filePath}", ex);
            }
            return -1;
        }

        public static string CopyFileOrDirectory(string sourcePath, string targetPath, bool recursive = true, bool isTemporary = false, bool forceOverwrite = false) {
            bool overwrite = forceOverwrite;
            if(!overwrite && targetPath.IsFileOrDirectory()) {
                var result = MpPlatformWrapper.Services.NativeMessageBox.ShowYesNoCancelMessageBox("Overwrite?", $"Destination '{targetPath}' already exists, would you like to overwrite it?");
                if(result.HasValue) {
                    if(result.Value) {
                        overwrite = true;
                    }
                } else {
                    return null;
                }
            }
            if(File.Exists(sourcePath)) {
                File.Copy(sourcePath, targetPath, overwrite);
                if(isTemporary && targetPath.IsUnderTemporaryFolder()) {
                    MpTempFileManager.AddTempFilePath(targetPath);
                }
                return targetPath;
            }
            if(Directory.Exists(sourcePath)) {
                CopyDirectory(sourcePath, targetPath, recursive,isTemporary);
                return targetPath;
            }
            MpConsole.WriteTraceLine($"Source directory not found '{sourcePath}'");
            return null;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, bool isTemporary = false) {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);
            if (isTemporary && destinationDir.IsUnderTemporaryFolder()) {
                MpTempFileManager.AddTempFilePath(destinationDir);
            }

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles()) {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
                if (isTemporary && targetFilePath.IsUnderTemporaryFolder()) {
                    MpTempFileManager.AddTempFilePath(targetFilePath);
                }
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive) {
                foreach (DirectoryInfo subDir in dirs) {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true,isTemporary);
                }
            }
        }

        public static bool DeleteFileOrDirectory(string path, bool recursive = true) {
            if(File.Exists(path)) {
                try {
                    File.Delete(path);
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"Error deleting '{path}'",ex);
                    return false;
                }
                return true;
            }
            if(Directory.Exists(path)) {                
                try {
                    Directory.Delete(path, recursive);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error deleting '{path}'", ex);
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool IsUnderTemporaryFolder(this string path) { 
            if(string.IsNullOrWhiteSpace(path) || !path.IsFileOrDirectory()) {
                return false;
            }
            string tempDir = Path.GetDirectoryName(Path.GetTempPath());

            bool isTempPath = path.ToLower().StartsWith(tempDir.ToLower());
            if(isTempPath) {
                return true;
            }
            Debugger.Break();
            return false;
        }
        public static string GetUniqueFileOrDirectoryName(string dir, string fileOrDirectoryName, string instanceSeparator = "_") {
            //only support Image and RichText fileTypes
            string fp = string.IsNullOrEmpty(dir) ? Path.GetTempPath() : dir;
            string fn = string.IsNullOrEmpty(fileOrDirectoryName) ? Path.GetRandomFileName() : fileOrDirectoryName;
            if (string.IsNullOrEmpty(fn)) {
                fn = Path.GetRandomFileName();
            }
            if (!Directory.Exists(dir)) {
                MpConsole.WriteLine($"Directory '{dir}' does not exist, creating..");
                dir = dir.RemoveSpecialCharacters();
                Directory.CreateDirectory(dir);               
            }

            string fe = string.Empty;
            if(fileOrDirectoryName.Contains(".")) {
                //is file name                
                fe = Path.GetExtension(fn);
                fn = Path.GetFileNameWithoutExtension(fileOrDirectoryName);
            }            

            int count = 1;

            string newFullPath = Path.Combine(dir, fn + fe);

            while (File.Exists(newFullPath) || Directory.Exists(newFullPath)) {
                newFullPath = Path.Combine(dir, fn + instanceSeparator + count + fe);
                count++;
            }
            return newFullPath;
        }

        public static void AppendTextToFile(string path, string textToAppend) {
            try {
                if (!File.Exists(path)) {
                    // Create a file to write to.
                    using (var sw = File.CreateText(path)) {
                        sw.WriteLine(textToAppend);
                    }
                } else {
                    // This text is always added, making the file longer over time
                    // if it is not deleted.
                    using (StreamWriter sw = File.AppendText(path)) {
                        sw.WriteLine(textToAppend);
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error appending text '{textToAppend}' to path '{path}'");
                MpConsole.WriteTraceLine($"With exception: {ex}");
            }
        }

        public static string ReadTextFromFileOrResource(string fileOrResourcePath, Assembly assembly = null) {
            if(File.Exists(fileOrResourcePath)) {
                return ReadTextFromFile(fileOrResourcePath);
            }
            return ReadTextFromResource(fileOrResourcePath, assembly);
        }

        public static string ReadTextFromFile(string filePath) {
            try {
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    return outStr;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public static string ReadTextFromResource(string resourcePath, Assembly assembly = null) {
            try {
                assembly = assembly == null ? Assembly.GetExecutingAssembly() : assembly;
                //var resourceName = "MyCompany.MyProduct.MyFile.txt";

                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                using (StreamReader reader = new StreamReader(stream)) {
                    string result = reader.ReadToEnd();
                    return result;
                }

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("error for resource path: " + resourcePath, ex);
                return string.Empty;
            }
        }

        public static async Task<byte[]> ReadBytesFromUriAsync(string uri,string baseDir = "", int timeoutMs = 5000) {
            if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute)) {
                string fileSystemPath = uri;
                if (!fileSystemPath.IsFileOrDirectory()) {
                    fileSystemPath = Path.Combine(baseDir, uri);
                    if(!fileSystemPath.IsFileOrDirectory()) {
                        fileSystemPath = GetAbsolutePath(baseDir, uri);
                    }                    
                }
                if(fileSystemPath.IsFileOrDirectory()) {
                    return ReadBytesFromFile(fileSystemPath);
                }

                MpConsole.WriteTraceLine(@"Cannot read bytes, bad url: " + uri+ " baseDir: "+baseDir);
                return null;
            }
            using (var httpClient = new HttpClient()) {
                try {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", System.Guid.NewGuid().ToString());
                    byte[] bytes = await httpClient.GetByteArrayAsync(uri).TimeoutAfter(TimeSpan.FromMilliseconds(timeoutMs));
                    return bytes;
                    //using (var fs = new FileStream("favicon.ico", FileMode.Create)) {
                    //    await fs.WriteAsync(bytes, 0, bytes.Length);
                    //    return bytes;                    
                    //}
                } catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                }
            }
            return null;
        }

        public static byte[] ReadBytesFromFile(string filePath) {
            if (!File.Exists(filePath)) {
                return null;
            }
            try {
                using var fs = new FileStream(filePath, FileMode.Open);

                int c;
                var bytes = new List<byte>();

                while ((c = fs.ReadByte()) != -1) {
                    bytes.Add((byte)c);
                }

                return bytes.ToArray();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public static Xamarin.Forms.ImageSource ReadImageFromFile(string filePath) {
            try {
                var bytes = ReadBytesFromFile(filePath);
                return new MpImageConverter().Convert(bytes.ToArray(), typeof(Xamarin.Forms.ImageSource)) as Xamarin.Forms.ImageSource;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public static bool DeleteFile(string filePath) {
            if (File.Exists(filePath)) {
                try {
                    File.Delete(filePath);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                    return false;
                }
            }
            return true;
        }

        public static string WriteTextToFile(string filePath, string text, bool isTemporary) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    string extension = string.Empty;
                    if (text.IsStringRichText()) {
                        extension = @".rtf";
                    } else if (text.IsStringCsv()) {
                        extension = @".csv";
                    } else {
                        extension = @".txt";
                    }
                    filePath = filePath.ToLower().Replace(@".tmp", extension);
                }
                using (var of = new StreamWriter(filePath)) {
                    of.Write(text);
                    of.Close();
                    if (isTemporary && filePath.IsUnderTemporaryFolder()) {
                        MpTempFileManager.AddTempFilePath(filePath);
                    }
                    return filePath;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path '{filePath}' with text '{text}'", ex);
                return null;
            }
        }

        public static string WriteByteArrayToFile(string filePath, byte[] byteArray, bool isTemporary) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    filePath = filePath.ToLower().Replace(@".tmp", @".png");
                }
                File.WriteAllBytes(filePath, byteArray);
                if (isTemporary && filePath.IsUnderTemporaryFolder()) {
                    MpTempFileManager.AddTempFilePath(filePath);
                }
                return filePath;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path {filePath} for byte array " + (byteArray == null ? "which is null" : "which is NOT null"), ex);
                return null;
            }
        }

    }
}
