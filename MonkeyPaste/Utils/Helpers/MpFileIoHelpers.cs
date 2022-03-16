using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public static class MpFileIoHelpers {
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

        public static string GetUniqueFileOrDirectoryName(string dir, string fileOrDirectoryName) {
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
                newFullPath = Path.Combine(dir, fn + count + fe);
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

        public static async Task<byte[]> ReadBytesFromUriAsync(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                MpConsole.WriteTraceLine(@"Cannot read bytes, bad url: " + url);
                return null;
            }
            using (var httpClient = new HttpClient()) {
                try {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "C# App");
                    byte[] bytes = await httpClient.GetByteArrayAsync(url);
                    using (var fs = new FileStream("favicon.ico", FileMode.Create)) {                    
                        fs.Write(bytes, 0, bytes.Length);
                        return bytes;
                    
                    }
                } catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                }
            }
            return null;
        }

        public static async Task<byte[]> ReadBytesFromUri(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                MpConsole.WriteTraceLine(@"Cannot read bytes, bad url: " + url);
                return null;
            }
            using var httpClient = new HttpClient();
            //RunSync<object>(() => dpv.GetDataAsync(af).AsTask());
            byte[] bytes = await httpClient.GetByteArrayAsync(url);

            using var fs = new FileStream("favicon.ico", FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);

            return bytes;
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

        public static string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
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
                    if (isTemporary) {
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

        public static string WriteByteArrayToFile(string filePath, byte[] byteArray, bool isTemporary = false) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    filePath = filePath.ToLower().Replace(@".tmp", @".png");
                }
                File.WriteAllBytes(filePath, byteArray);
                return filePath;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path {filePath} for byte array " + (byteArray == null ? "which is null" : "which is NOT null"), ex);
                return null;
            }
        }
    }
}
