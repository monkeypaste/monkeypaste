using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpFileIo {
        public const int MAX_WIN_PATH_LENGTH = 32_767;

        public static ReaderWriterLock locker = new ReaderWriterLock();

        public static string TouchFile(this string path) {
            if (path.IsFileOrDirectory()) {
                return path;
            }
            try {
                using (File.Create(path)) {
                    return path;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error touching file '{path}'.", ex);
            }
            return null;

        }
        public static string TouchDir(this string path) {
            if (path.IsFileOrDirectory()) {
                return path;
            }
            try {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error touching dir '{path}'.", ex);
            }
            return null;
        }

        public static string ToFileSystemPath(this Uri uri) {
            if (uri == null ||
                uri.Scheme != Uri.UriSchemeFile) {
                return string.Empty;
            }
            return uri.AbsolutePath;
        }

        public static string ToDirectory(this string path, bool replaceIfExists, bool createUniqueIfExists) {
            MpDebug.Assert(!(replaceIfExists && createUniqueIfExists), "Only 1 flag can be true", false, true);
            if (path.IsDirectory()) {
                if (replaceIfExists) {
                    Directory.Delete(path, true);
                } else if (createUniqueIfExists) {
                    path = GetUniqueFileOrDirectoryPath(Directory.GetParent(path).FullName, Path.GetDirectoryName(path));
                } else {
                    // nothing to do
                    return path;
                }
            }
            var result = Directory.CreateDirectory(path);
            return result.FullName;
        }

        public static string ToFile(
            this string fileData,
            string forcePath = "",
            string forceDir = "",
            string forceNamePrefix = "",
            string forceExt = "",
            bool overwrite = false) {

            try {
                if (!string.IsNullOrEmpty(forcePath)) {
                    // parse full path and override parts
                    forceDir = Path.GetDirectoryName(forcePath);
                    forceNamePrefix = Path.GetFileNameWithoutExtension(forcePath);
                    forceExt = Path.GetExtension(forcePath);
                }
                // NOTE overwrite msgbox only shows up if not temporary
                // NOTE2 csv is too annoying to discern (probably all are) need to check before and force/convert
                forceExt = string.IsNullOrEmpty(forceExt) ? forceExt : forceExt.Replace(".", string.Empty);

                if (string.IsNullOrEmpty(forceExt)) {
                    // when ext is not given infer from content
                    forceExt = MpCommonTools.Services.StringTools.DetectStringFileExt(fileData);
                } else {
                    if (forceExt.ToLowerInvariant().Equals("rtf")) {
                        fileData = MpCommonTools.Services.StringTools.ToRichText(fileData);
                    } else if (forceExt.ToLowerInvariant().Equals("txt")) {
                        fileData = MpCommonTools.Services.StringTools.ToPlainText(fileData);
                    } else if (forceExt.ToLowerInvariant().Equals("csv")) {
                        fileData = MpCommonTools.Services.StringTools.ToCsv(fileData);
                    } else if (forceExt.ToLowerInvariant().Equals("html")) {
                        fileData = MpCommonTools.Services.StringTools.ToHtml(fileData);
                    }
                }
                string tfp;
                if (fileData.IsFileOrDirectory()) {
                    tfp = fileData;
                } else if (forceExt == "png" ||
                           forceExt.ToLowerInvariant().Equals("bmp") ||
                           forceExt.ToLowerInvariant().Equals("jpg") ||
                           forceExt.ToLowerInvariant().Equals("jpeg")) {
                    string tmp_fp = Path.GetTempFileName().Replace(@".tmp", forceExt);
                    tfp = WriteByteArrayToFile(tmp_fp, fileData.ToBytesFromBase64String());
                } else {
                    tfp = WriteTextToFile(Path.GetTempFileName(), fileData);
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
                    if (string.IsNullOrEmpty(tfe)) {
                        ofp += $".{forceExt}";
                    } else {
                        ofp = ofp.Replace(tfe, "." + forceExt.Replace(".", string.Empty));
                    }

                }

                if (!string.IsNullOrEmpty(forceDir)) {
                    if (!forceDir.IsDirectory()) {
                        throw new Exception("Directory not found: " + forceDir);
                    }
                    string tfd = Path.GetDirectoryName(tfp);
                    ofp = ofp.Replace(tfd, forceDir);
                }
                if (ofp.ToLowerInvariant() != tfp.ToLowerInvariant()) {
                    if (ofp.IsFileOrDirectory() && !overwrite) {
                        if (string.IsNullOrEmpty(forceDir)) {
                            // this means file is going to write to temp folder and to avoid IO or name issues preserve name
                            // but put in random subdirectory of temp folder
                            string randomSubDirPath = Path.Combine(Path.GetDirectoryName(ofp), Path.GetRandomFileName());
                            try {
                                Directory.CreateDirectory(randomSubDirPath);

                                ofp = Path.Combine(randomSubDirPath, Path.GetFileName(ofp));
                            }
                            catch (Exception ex) {
                                MpConsole.WriteTraceLine("Error creating random temp subdirectory: " + ex);
                                ofp = MpFileIo.GetUniqueFileOrDirectoryPath(Path.GetDirectoryName(ofp), Path.GetFileName(ofp));
                            }
                        } else {
                            ofp = MpFileIo.GetUniqueFileOrDirectoryPath(Path.GetDirectoryName(ofp), Path.GetFileName(ofp));
                        }

                    }
                    // move temporary file to processed output file path and delete temporary
                    try {
                        // TODO figure out how to handle recrusive directory copy for this case for now don't do it
                        ofp = MpFileIo.CopyFileOrDirectory(tfp, ofp, false, overwrite);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error copying temp file '{tfp}' to '{ofp}', returning temporary. Exception: " + ex);
                        return tfp;
                    }
                }
                return ofp;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("ToFile exception", ex);
            }
            return string.Empty;
        }
        public static void OpenFileBrowser(string path, IEnumerable<string> selected_file_names = default) {
            if (!path.IsFileOrDirectory()) {
                MpCommonTools.Services.PlatformMessageBox.ShowOkCancelMessageBoxAsync($"Error", $"{path} not found");
                // give up on any selected paths
                selected_file_names = default;

                // fallback to dir
                path = Path.GetDirectoryName(path);
                if (!path.IsFileOrDirectory()) {
                    // no luck fallback to desktop dir
                    path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                }
            }

            string args = path;
#if WINDOWS
            if (selected_file_names != null) {
                ShowSelectedInExplorer.FilesOrFolders(path.IsFile() ? path.GetDir() : path, selected_file_names.ToArray());
                return;
            }
            if (path.IsFile()) {
                // when path is file and not folder add select arg to process start
                args = $"/select,\"{path}\"";
            }
#endif
            ProcessStartInfo startInfo = new ProcessStartInfo {
                Arguments = args,
                FileName = Path.GetFileName(MpCommonTools.Services.PlatformInfo.OsFileManagerPath)
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

        public static DateTime? GetDateTimeInfo(string path, bool modified = false, bool utc = true) {
            if (path.IsDirectory() &&
                new DirectoryInfo(path) is { } di) {
                if (modified) {
                    return utc ? di.LastWriteTimeUtc : di.LastWriteTime;
                }
                return utc ? di.CreationTimeUtc : di.CreationTime;
            }
            if (path.IsFile() &&
                new FileInfo(path) is { } fi) {
                if (modified) {
                    return utc ? fi.LastWriteTimeUtc : fi.LastWriteTime;
                }
                return utc ? fi.CreationTimeUtc : fi.CreationTime;
            }
            return default;
        }

        public static string GetAbsolutePath(string basePath, string path) {
            // from https://stackoverflow.com/a/35218619/105028
            if (path == null) {
                return null;
            }
            try {
                if (basePath == null) {
                    basePath = Path.GetFullPath("."); // quick way of getting current working directory
                } else {
                    basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
                }

                if (Path.Combine(basePath, path).IsFileOrDirectory()) {
                    return Path.Combine(basePath, path);
                }
                string finalPath;
                // specific for windows paths starting on \ - they need the drive added to them.
                // I constructed this piece like this for possible Mono support.
                if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path))) {
                    if (path.StartsWith(Path.DirectorySeparatorChar.ToString())) {
                        finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                    } else {
                        if (path.Contains("/") && Path.DirectorySeparatorChar == '\\') {
                            // relative linux style path in windows dir
                            path = path.Replace("/", Path.DirectorySeparatorChar.ToString());
                        } else if (path.Contains("\\") && Path.DirectorySeparatorChar != '\\') {
                            path = path.Replace("\\", Path.DirectorySeparatorChar.ToString());
                        }
                        finalPath = Path.Combine(basePath, path.TrimStart(Path.DirectorySeparatorChar));
                    }
                } else {

                    finalPath = Path.Combine(basePath, path.TrimStart(Path.DirectorySeparatorChar));
                }
                // resolves any internal "..\" to get the true full path.
                return Path.GetFullPath(finalPath);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error finding absolute path. BasePath: '{basePath}' Path: '{path}'", ex);
                return string.Empty;
            }

        }

        public static double ConvertBytesToMegabytes(long bytes, int precision = 2) {
            return Math.Round((bytes / 1024f) / 1024f, precision);
        }

        public static double ConvertMegaBytesToBytes(long megabytes, int precision = 2) {
            return Math.Round((megabytes * 1024f) * 1024f, precision);
        }
        public static void CopyContents(string sourceDir, string targetDir, bool recursive = true, bool overwrite = true) {
            CopyContents(new DirectoryInfo(sourceDir), new DirectoryInfo(targetDir), recursive, overwrite);
        }
        public static void CopyContents(this DirectoryInfo sourceDir, DirectoryInfo targetDir, bool recursive, bool overwrite) {
            foreach (var file in sourceDir.GetFiles()) {
                string fp = Path.Combine(targetDir.FullName, file.Name);
                if (overwrite || !fp.IsFileOrDirectory()) {
                    file.CopyTo(fp);
                }
            }
            if (!recursive) {
                return;
            }
            foreach (var directory in sourceDir.GetDirectories()) {
                directory.CopyContents(targetDir.CreateSubdirectory(directory.Name), recursive, overwrite);
            }
        }

        public static double GetPathsSizeInMegaBytes(IEnumerable<string> paths) {
            double total_bytes = paths.Select(x => GetPathSizeInBytes(x)).Sum();
            double total_mega_bytes = Math.Round(total_bytes / Math.Pow(1024.0, 2), 2);
            return total_mega_bytes;
        }
        private static double GetPathSizeInBytes(string path) {
            if (path == null) {
                return 0;
            }
            bool is_file = path.IsFile();
            bool is_dir = path.IsDirectory();
            if (!is_file && !is_dir) {
                return 0;
            }
            double bytes = is_file ?
                    GetFileSizeInBytes(path) : GetDirectorySizeInBytes(path);
            return 0;
        }
        private static double GetFileSizeInBytes(string filePath) {
            try {
                FileInfo fi = new FileInfo(filePath);
                return fi.Length;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error checking size of path {filePath}", ex);
            }
            return 0;
        }
        private static double GetDirectorySizeInBytes(string path) {
            // from https://stackoverflow.com/a/51942249/105028
            try {
                double total_bytes = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Select(d => new FileInfo(d))
                    .Select(d => new { Directory = d.DirectoryName, FileSize = d.Length })
                    .ToLookup(d => d.Directory)
                    .Select(d => d.Select(x => x.FileSize).Sum())
                    .Sum();
                return total_bytes;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error calculating dir size.", ex);
                return 0;
            }
        }

        public static string CopyFileOrDirectory(string sourcePath, string targetPath, bool recursive = true, bool forceOverwrite = false) {
            bool overwrite = forceOverwrite;
            if (!overwrite && targetPath.IsFileOrDirectory()) {
                return null;
            }
            if (File.Exists(sourcePath)) {
                File.Copy(sourcePath, targetPath, overwrite);
                return targetPath;
            }
            if (Directory.Exists(sourcePath)) {
                CopyDirectory(sourcePath, targetPath, recursive);
                return targetPath;
            }
            MpConsole.WriteTraceLine($"Source directory not found '{sourcePath}'");
            return null;
        }

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true) {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles()) {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive) {
                foreach (DirectoryInfo subDir in dirs) {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static string GetThisAppTempDir() {
            // NOTE this wraps calls to temp dir to a sub-folder for this app
            string internal_temp_dir = Path.Combine(Path.GetTempPath(), MpCommonTools.Services.ThisAppInfo.ThisAppProductName);
            if (!internal_temp_dir.IsDirectory()) {
                CreateDirectory(internal_temp_dir);
            }
            return internal_temp_dir;
        }

        public static string GetThisAppRandomTempDir() {
            string rand_temp_dir = Path.Combine(GetThisAppTempDir(), Path.GetRandomFileName());
            CreateDirectory(rand_temp_dir);
            return rand_temp_dir;
        }


        public static bool IsUnderTemporaryFolder(this string path) {
            if (string.IsNullOrWhiteSpace(path) || !path.IsFileOrDirectory()) {
                return false;
            }
            string tempDir = Path.GetDirectoryName(Path.GetTempPath());

            bool isTempPath = path.ToLowerInvariant().StartsWith(tempDir.ToLowerInvariant());
            if (isTempPath) {
                return true;
            }
            MpDebug.Break();
            return false;
        }

        public static string GetFileOrDirectoryName(this string path) {
            if (path.IsFile()) {
                return Path.GetFileName(path);
            }
            if (path.IsDirectory()) {
                return Path.GetDirectoryName(path);
            }
            // broken path
            return null;
        }

        public static string GetUniqueFileOrDirectoryPath(
            string force_dir = default,
            string force_name = default,
            string instanceSeparator = "_") {

            string fd = force_dir ?? GetThisAppRandomTempDir();
            string fn = string.IsNullOrEmpty(force_name) ? Path.GetRandomFileName() : force_name;
            if (!fd.IsDirectory()) {
                MpConsole.WriteLine($"Directory '{fd}' does not exist, creating..");
                fd = fd.RemoveSpecialCharacters();
                Directory.CreateDirectory(fd);
            }

            string fe = string.Empty;
            if (fn.Contains(".")) {
                //is file name                
                fe = Path.GetExtension(fn);
                fn = Path.GetFileNameWithoutExtension(force_name);
            }

            int count = 1;

            string newFullPath = Path.Combine(fd, fn + fe);

            while (newFullPath.IsFileOrDirectory()) {
                newFullPath = Path.Combine(fd, fn + instanceSeparator + count + fe);
                count++;
            }
            return newFullPath;
        }

        public static void AppendTextToFile(string path, string textToAppend) {
            try {
                locker.AcquireWriterLock(Timeout.Infinite);
                File.AppendAllLines(path, new[] { textToAppend });
                locker.ReleaseWriterLock();
            }
            catch (Exception ex) {
                locker.ReleaseWriterLock();
                throw ex;
            }
            finally {
            }
        }

        public static string ReadTextFromFileOrResource(string fileOrResourcePath, Assembly assembly = null) {
            if (fileOrResourcePath.IsFile()) {
                return ReadTextFromFile(fileOrResourcePath);
            }
            return ReadTextFromResource(fileOrResourcePath, assembly);
        }

        public static string ReadTextFromFile(string filePath) {
            if (!filePath.IsFile()) {
                return string.Empty;
            }
            try {
                locker.AcquireReaderLock(Timeout.Infinite);
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    locker.ReleaseReaderLock();
                    return outStr;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                locker.ReleaseReaderLock();
                return null;
            }
        }

        public static string ReadTextFromResource(string resourcePath, Assembly assembly) {
            try {
                assembly ??= Assembly.GetExecutingAssembly();
                locker.AcquireReaderLock(Timeout.Infinite);
                using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
                using (StreamReader reader = new StreamReader(stream)) {
                    string result = reader.ReadToEnd();
                    locker.ReleaseReaderLock();
                    return result;
                }

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("error for resource path: " + resourcePath, ex);
                locker.ReleaseReaderLock();
                return string.Empty;
            }
        }


        public static async Task<byte[]> ReadBytesFromUriAsync(string uriStr, string baseDir = "", int timeoutMs = 5000) {

            if (!Uri.IsWellFormedUriString(uriStr, UriKind.Absolute) ||
                new Uri(uriStr, UriKind.Absolute) is not Uri uri) {
                string fileSystemPath = uriStr;
                if (!fileSystemPath.IsFileOrDirectory()) {
                    fileSystemPath = GetAbsolutePath(baseDir, uriStr);
                }
                if (fileSystemPath.IsFileOrDirectory()) {
                    return ReadBytesFromFile(fileSystemPath);
                }

                MpConsole.WriteTraceLine(@"Cannot read bytes, bad url: " + uriStr + " baseDir: " + baseDir);
                return null;
            }
            if (uri.Scheme == "file") {
                return ReadBytesFromFile(uri.LocalPath);
            }
            if (uri.Scheme == "avares") {
                return MpCommonTools.Services.PlatformResource.GetResource<byte[]>(uriStr);
            }

            using (var httpClient = MpHttpClient.Client) {
                //if (timeoutMs > 0) {
                //    httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                //}
                try {
                    byte[] bytes = await httpClient.GetByteArrayAsync(uri);//.TimeoutAfter(TimeSpan.FromMilliseconds(timeoutMs));
                    return bytes;
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error reading bytes from '{uri}'. ", ex);
                }
            }
            return null;
        }

        public static async Task<string> ReadTextFromUriAsync(string uri, string baseDir = "", int timeoutMs = 5000, Encoding en = null) {
            var bytes = await ReadBytesFromUriAsync(uri, baseDir, timeoutMs);
            return bytes.ToDecodedString(en);
        }

        public static byte[] ReadBytesFromFile(string filePath) {
            if (!File.Exists(filePath)) {
                return null;
            }
            try {
                locker.AcquireReaderLock(Timeout.Infinite);
                using (var fs = new FileStream(filePath, FileMode.Open)) {
                    int c;
                    var bytes = new List<byte>();

                    while ((c = fs.ReadByte()) != -1) {
                        bytes.Add((byte)c);
                    }
                    locker.ReleaseReaderLock();
                    return bytes.ToArray();
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                locker.ReleaseReaderLock();
                return null;
            }
        }

        public static bool IsFileInUse(string path) {
            if (!path.IsFile()) {
                return false;
            }
            try {
                using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite)) {
                    fs.Close();
                }
            }
            catch {
                return true;
            }
            return false;
        }

        public static bool DeleteFile(string filePath) {
            if (filePath.IsFile()) {
                try {
                    File.Delete(filePath);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("DeleteFile error for filePath: " + filePath + ex.ToString());
                    return false;
                }
            }
            return true;
        }

        public static bool CreateDirectory(string path) {
            if (path.IsDirectory()) {
                MpConsole.WriteTraceLine($"Warning directory '{path}' already exists");
                return true;
            }
            if (path.IsFile()) {
                MpConsole.WriteTraceLine($"Error '{path}' is a file not a directory");
                return false;
            }
            if (string.IsNullOrEmpty(path)) {
                MpConsole.WriteTraceLine($"Error no directory path provided");
                return false;
            }
            try {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating Directory '{path}'", ex);
                return false;
            }
            return true;
        }
        public static bool DeleteDirectory(string path, bool recursive = true) {
            if (!path.IsFileOrDirectory()) {
                // if path doesn't exist treat as success
                return true;
            }
            if (path.IsDirectory()) {
                try {
                    Directory.Delete(path, recursive);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error deleting Directory '{path}'", ex);
                    return false;
                }
                return true;
            }
            return false;
        }
        public static bool DeleteFileOrDirectory(string path, bool recursive = true) {
            if (path.IsFile()) {
                return DeleteFile(path);
            }
            if (path.IsDirectory()) {
                return DeleteDirectory(path, recursive);
            }
            return false;
        }

        public static string WriteTextToFile(string filePath, string text, bool overwrite = false) {
            try {
                if (filePath.ToLowerInvariant().EndsWith(@".tmp")) {
                    string extension = MpCommonTools.Services.StringTools.DetectStringFileExt(text);
                    filePath = filePath.ToLowerInvariant().Replace(@".tmp", extension);
                }
                if (overwrite && filePath.IsFile()) {
                    DeleteFile(filePath);
                }
                locker.AcquireWriterLock(Timeout.Infinite);
                File.WriteAllText(filePath, text);
                locker.ReleaseWriterLock();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path '{filePath}' with text '{text}'", ex);
                locker.ReleaseWriterLock();
            }
            return filePath;
        }

        public static string WriteByteArrayToFile(string filePath, byte[] byteArray) {
            try {
                locker.AcquireWriterLock(Timeout.Infinite);
                File.WriteAllBytes(filePath, byteArray);
                locker.ReleaseWriterLock();

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path {filePath} for byte array " + (byteArray == null ? "which is null" : "which is NOT null"), ex);
                locker.ReleaseWriterLock();
            }
            return filePath;
        }

        #region WriteUrlToFile w/ progress
        public static async Task WriteUrlToFileAsync(
            this string url, string path,
            TimeSpan timeout = default, IProgress<double> progress = null, CancellationToken cancellationToken = default) {
            // from https://stackoverflow.com/a/46497896/105028
            using (var httpClient = MpHttpClient.Client) {
                //if (timeout != default) {
                //    httpClient.Timeout = timeout;
                //}

                // Create a file stream to store the downloaded data.
                // This really can be any type of writeable stream.
                using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    // Use the custom extension method below to download the data.
                    // The passed progress-instance will receive the download status updates.
                    await httpClient.DownloadAsync(url, file, progress, cancellationToken);
                }
            }
        }

        private static async Task DownloadAsync(
            this HttpClient client,
            string requestUri,
            Stream destination,
            IProgress<double> progress = null, CancellationToken cancellationToken = default) {
            // Get the http headers first to examine the content length
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead)) {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync()) {

                    // Ignore progress reporting when no progress reporter was 
                    // passed or when the content length is unknown
                    if (progress == null || !contentLength.HasValue) {
                        await download.CopyToAsync(destination);
                        return;
                    }

                    // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                    var relativeProgress = new Progress<long>(totalBytes => progress.Report((double)totalBytes / contentLength.Value));
                    // Use extension method to report progress while downloading
                    await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                    progress.Report(1);
                }
            }
        }

        private static async Task CopyToAsync(
            this Stream source,
            Stream destination,
            int bufferSize,
            IProgress<long> progress = null, CancellationToken cancellationToken = default) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }

        #endregion
        public static string GetLnkTargetPath(string filepath) {
            using (var br = new BinaryReader(System.IO.File.OpenRead(filepath))) {
                // skip the first 20 bytes (HeaderSize and LinkCLSID)
                br.ReadBytes(0x14);
                // read the LinkFlags structure (4 bytes)
                uint lflags = br.ReadUInt32();
                // if the HasLinkTargetIDList bit is set then skip the stored IDList 
                // structure and header
                if ((lflags & 0x01) == 1) {
                    br.ReadBytes(0x34);
                    var skip = br.ReadUInt16(); // this counts of how far we need to skip ahead
                    br.ReadBytes(skip);
                }
                // get the number of bytes the path contains
                var length = br.ReadUInt32();
                // skip 12 bytes (LinkInfoHeaderSize, LinkInfoFlgas, and VolumeIDOffset)
                br.ReadBytes(0x0C);
                // Find the location of the LocalBasePath position
                var lbpos = br.ReadUInt32();
                // Skip to the path position 
                // (subtract the length of the read (4 bytes), the length of the skip (12 bytes), and
                // the length of the lbpos read (4 bytes) from the lbpos)
                br.ReadBytes((int)lbpos - 0x14);
                var size = length - lbpos - 0x02;
                var bytePath = br.ReadBytes((int)size);
                var path = Encoding.UTF8.GetString(bytePath, 0, bytePath.Length);
                return path;
            }
        }

        public static string GetOsExecutableFileFilters() {
            switch (MpCommonTools.Services.PlatformInfo.OsType) {
                case MpUserDeviceType.Windows:
                    return "lnk,exe";
                default:
                    MpDebug.Break("Add here or deal with it");
                    return string.Empty;
            }
        }

        public static string GetDefaultUserAgent() {
            //string user_agent_str = $"Mozilla/5.0 (compatible; {MpCommonTools.Services.ThisAppInfo.ThisAppProductName}/{MpCommonTools.Services.ThisAppInfo.ThisAppProductVersion})";
            string user_agent_str = $"{MpCommonTools.Services.ThisAppInfo.ThisAppProductName})";
            return user_agent_str;
        }

        public static void SetDefaultUserAgent(this HttpClient httpClient) {
            //httpClient.DefaultRequestHeaders.Add("User-Agent", System.Guid.NewGuid().ToString());
            //httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(MpCommonTools.Services.ThisAppInfo.ThisAppProductName);
            //string ua = MpCommonTools.Services.UserAgentProvider.UserAgent;
            //httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ua);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
        }
    }


}
