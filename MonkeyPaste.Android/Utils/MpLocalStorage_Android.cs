using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonkeyPaste;
using System.Threading.Tasks;
using Java.IO;
using System.IO;

namespace MonkeyPaste.Droid {
    public class MpLocalStorage_Android : MpILocalStorage {
        public bool CreateFile(string fileName, byte[] bytes, string fileType) {
            
            //try {
            //    fileType = fileType.Contains(".") ? fileType : "." + fileType;
            //    string outPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath; 
                
            //    //using(var fos = new FileOutputStream(outPath + "/db_dump.db") {
            //    //    fos.Write()
            //    //}
            //    //string path = //Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS).AbsolutePath;
            //    string filePath = outPath + Java.IO.File.Separator + fileName + fileType;// System.IO.Path.Combine(path, );
            //    if (System.IO.File.Exists(filePath)) {
            //        System.IO.File.Delete(filePath);                    
            //    }
            //    System.IO.File.Create(filePath);
            //    using (var fsStream = new FileStream(filePath, FileMode.Create))
            //    using (var writer = new BinaryWriter(fsStream, Encoding.UTF8)) {
            //        writer.Write(bytes);
            //    }
            //    MpConsole.WriteLine(@"File written to " + filePath);
            //}
            //catch {
            //    return false;
            //}
            return true;
        }
    }
}