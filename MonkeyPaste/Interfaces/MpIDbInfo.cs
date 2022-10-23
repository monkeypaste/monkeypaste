using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIDbInfo {
        //public string DbName => "mp.db";

        //public string DbPath => Path.Combine(Environment.CurrentDirectory, DbName);

        //public string DbMediaFolderPath => Path.Combine(LocalStoragePath, "media");

        //public int MaxDbPasswordAttempts => 3;
        string DbExtension { get; }
        string DbName { get; }
        string DbPath { get; }

        //string GetDbFilePath();
        //string GetDbPassword();
        //string GetDbName();
    }

}
