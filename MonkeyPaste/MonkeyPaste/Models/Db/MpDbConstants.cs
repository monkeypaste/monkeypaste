using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public static class MpDbConstants {
        public const string DbName = "Mp.db";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DbPath {
            get {                
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return System.IO.Path.Combine(basePath, DbName);
            }
        }
    }
}
