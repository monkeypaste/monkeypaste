using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpDb {
        private string _dbPath { get; set; }
        public string DbPath { get { return _dbPath; } set { _dbPath = value; } }

        private string _dbPassword { get; set; }
        public string DbPassword { get { return _dbPassword; } set { _dbPassword = value; } }

        private bool _noDb { get; set; }
        public bool NoDb { get { return _noDb; } set { _noDb = value; } }

        private int _passwordAttempts = 0;

        public MpDb(string dbPath,string dbPassword) {
            _noDb = false;
            DbPath = dbPath;
            DbPassword = dbPassword;
        }
        public void InitDb() {
            if(_dbPath == null || _dbPath == String.Empty || !Directory.Exists(Path.GetDirectoryName(_dbPath)) || !File.Exists(_dbPath)) {
                Console.WriteLine(_dbPath + " does not exist...");
                DialogResult result = MessageBox.Show("No Database found would you like to load a file?","No DB Found",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                if(result == DialogResult.Yes) {
                    OpenFileDialog openFileDialog = new OpenFileDialog() {
                        FileName = "Select a db file",
                        Filter = "Db files (*.db)|*.db",
                        Title = "Open DB File"
                    };
                    DialogResult openResult = openFileDialog.ShowDialog();
                    if(openResult == DialogResult.OK) {
                        _dbPath = openFileDialog.FileName;
                        DialogResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?","Remember Database?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                        if(autoLoadResult == DialogResult.Yes) {
                            MpRegistryHelper.Instance.SetValue("DBPath",_dbPath);
                        }
                    }
                }
                else {
                    DialogResult newDbResult = MessageBox.Show("Would you like to create a new database and store your history?","New Database?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                    if(newDbResult == DialogResult.Yes) {
                        SaveFileDialog saveFileDialog = new SaveFileDialog() {
                            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,

                            FileName = "mp.db",
                            Filter = "Db files (*.db)|*.db",
                            Title = "Create new DB File"
                        };

                        DialogResult saveResult = saveFileDialog.ShowDialog();
                        if(saveResult == DialogResult.OK) {
                            _dbPath = saveFileDialog.FileName;
                            DialogResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?","Remember Database?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                            if(autoLoadResult == DialogResult.Yes) {
                                MpRegistryHelper.Instance.SetValue("DBPath",_dbPath);
                            }
                            SQLiteConnection.CreateFile(_dbPath);
                            DialogResult newDbPasswordResult = MessageBox.Show("Would you like to encrypt database with a password?","Encrypt?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                            if(newDbPasswordResult == DialogResult.Yes) {
                                MpSetDbPasswordForm setDbPasswordForm = new MpSetDbPasswordForm();
                                setDbPasswordForm.ShowDialog();
                                SetDbPassword(setDbPasswordForm.PasswordTextBox.Text);
                            }
                            ExecuteNonQuery(GetCreateString());
                        }
                        else {
                            _noDb = true;
                        }
                    }
                    else {
                        _noDb = true;
                    }
                }
            }
            Console.WriteLine("Database successfully initialized at " + _dbPath);
        }
        public void SetDbPassword(string newPassword) {
            if(_dbPassword != newPassword) {
                // if db is unpassword protected
                if(_dbPassword == null || _dbPassword == String.Empty) {
                    SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbPath + ";Version=3;");
                    conn.SetPassword(newPassword);
                    conn.Open();
                    conn.Close();
                }
                else {
                    SQLiteConnection conn = SetConnection();
                    conn.Open();
                    conn.ChangePassword(newPassword);
                }
                _dbPassword = newPassword;
            }
        }
        private SQLiteConnection SetConnection() {
            if(_noDb) {
                return null;
            }
            // see https://stackoverflow.com/questions/1381264/password-protect-a-sqlite-db-is-it-possible
            // about passwords
            string connStr = "Data Source=" + _dbPath + ";Version=3;";
            if(_dbPassword != null && _dbPassword != String.Empty) {
                connStr += "Password=" + _dbPassword + ";";
            }
            Console.WriteLine("Connection String: " + connStr);
            SQLiteConnection conn = null;
            try {
                conn = new SQLiteConnection(connStr);
            }
            catch(Exception e) {
                Console.WriteLine("Error during SQL connection: " + connStr + "\n" + "With error: " + e.ToString());
                conn = null;
                _dbPath = null;
                InitDb();
                SetConnection();
            }
            return conn;
        }
        public void ExecuteNonQuery(string sql,List<string> paramList = null,List<object> paramValueList = null) {
            if(_noDb || _passwordAttempts > (int)MpSingletonController.Instance.GetSetting("MaxDbPasswordAttempts")) {
                return;
            }
            if((paramList != null && paramValueList != null) && (paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count)) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return;
            }
            bool wasError = false;
            SQLiteConnection sql_con = SetConnection();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = sql;

            if(paramList != null) {
                for(int i = 0;i < paramList.Count;i++) {
                    // check p to conditionally set parameter type
                    SQLiteParameter param = null;
                    if(paramList[i] == "@0") {
                        param = new SQLiteParameter(paramList[i],DbType.Binary);
                        param.Value = (byte[])paramValueList[i];
                    }
                    else if(paramList[i] == "@1") {
                        param = new SQLiteParameter(paramList[i],DbType.String);
                        param.Value = (string)paramValueList[i];
                    }
                    sql_cmd.Parameters.Add(param);
                }
            }

            sql_con.Open();
            try {
                sql_cmd.ExecuteNonQuery();
            }
            catch(SQLiteException ex) { 
                wasError = true;
                Console.WriteLine("Error in executenonquery w/ parameter list: " + ex.ToString());
                MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                DialogResult enterPasswordResult = enterPasswordForm.ShowDialog();
                _passwordAttempts++;
                if(_passwordAttempts < (int)MpSingletonController.Instance.GetSetting("MaxDbPasswordAttempts")) {
                    MpSingletonController.Instance.GetMpData().Init();
                }
                else {
                    return;
                }
            }
            if(!wasError) {
                _passwordAttempts = 0;
            }
            sql_con.Close();
        }
        public DataTable Execute(string sql,List<string> paramList = null,List<object> paramValueList = null) {
            if(_noDb || _passwordAttempts > (int)MpSingletonController.Instance.GetSetting("MaxDbPasswordAttempts")) {
                return null;
            }
            if((paramList != null && paramValueList != null) && (paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count)) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return null;
            }
            bool wasError = false;
            SQLiteConnection sql_con = SetConnection();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = sql;
            DataSet DS = new DataSet();
            DataTable DT = new DataTable();

            if(paramList != null) {
                for(int i = 0;i < paramList.Count;i++) {
                    // check p to conditionally set parameter type
                    SQLiteParameter param = null;
                    if(paramList[i] == "@0") {
                        param = new SQLiteParameter(paramList[i],DbType.Binary);
                        param.Value = (byte[])paramValueList[i];
                    }
                    else if(paramList[i] == "@1") {
                        param = new SQLiteParameter(paramList[i],DbType.String);
                        param.Value = (string)paramValueList[i];
                    }
                    sql_cmd.Parameters.Add(param);
                }
            }
            try {
                sql_con.Open();
                SQLiteDataAdapter DB = new SQLiteDataAdapter(sql_cmd);
                DS.Reset();
                DB.Fill(DS);
                DT = DS.Tables[0];
                sql_con.Close();
            }
            catch(SQLiteException ex) {
                Console.WriteLine("Error during executing sql: " + sql + "\n" + "With error: " + ex.ToString());
                wasError = true;
                MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                DialogResult enterPasswordResult = enterPasswordForm.ShowDialog();
                _passwordAttempts++;
                if(_passwordAttempts < (int)MpSingletonController.Instance.GetSetting("MaxDbPasswordAttempts")) {
                    MpSingletonController.Instance.GetMpData().Init();
                }
                else {
                    return null;
                }
            }
            if(!wasError) {
                _passwordAttempts = 0;
            }
            return DT;
        }
        public int GetLastRowId(string tableName,string pkName) {
            DataTable dt = Execute("select * from " + tableName + " ORDER BY " + pkName + " DESC LIMIT 1;");
            if(dt.Rows.Count > 0)
                return Convert.ToInt32(dt.Rows[0][0].ToString());
            return -1;
            /*SQLiteConnection sql_con = SetConnection();
            sql_con.Open();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = "select last_insert_rowid()";
            // The row ID is a 64-bit value - cast the Command result to an Int64.
            Int64 lastRowID64 = (Int64)sql_cmd.ExecuteScalar();

            // Then grab the bottom 32-bits as the unique ID of the row.
            return (int)lastRowID64;*/
        }
        public void ResetDb() {
            // File.Delete(DbPath);
            //InitDb();
            ExecuteNonQuery(GetClearString());
        }
        public void DeleteDb() {
            File.Delete(DbPath);
        }
        private string GetCreateString() {
            return @"
                    CREATE TABLE MpTagType (
                        pk_MpTagTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                        TagTypeName text NOT NULL
                    );
                    CREATE TABLE MpTag (
                        pk_MpTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpTagTypeId integer NOT NULL
                    , TagName text NOT NULL
                    , fk_ColorId integer 
                    , CONSTRAINT FK_MpTag_0_0 FOREIGN KEY (fk_MpTagTypeId) REFERENCES MpTagType (pk_MpTagTypeId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_ColorId) REFERENCES MpColor (pk_MpColorId)
                    );
                    CREATE TABLE MpSetting (
                        pk_MpSettingId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , SettingName text NOT NULL
                    , SettingValueType text NOT NULL
                    , SettingValue text NULL
                    , SettingDefaultValue text NULL
                    );
                    CREATE TABLE MpPlatformType (
                        pk_MpPlatformTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , PlatformName text NOT NULL 
                    );
                    INSERT INTO MpPlatformType(PlatformName) VALUES('ios'),('android'),('windows'),('mac');
                    CREATE TABLE MpIcon (
                        pk_MpIconId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , IconBlob image NOT NULL
                    );
                    CREATE TABLE MpHotKey (
                        pk_MpHotKeyId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , KeyList text NULL
                    , ModList text NULL
                    );
                    CREATE TABLE MpDeviceType (
                        pk_MpDeviceTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , DeviceTypeName text NULL 
                    );
                    INSERT INTO MpDeviceType(DeviceTypeName) VALUES('windows'),('mac'),('android'),('iphone'),('ipad'),('tablet');

                    CREATE TABLE MpPlatform (
                        pk_MpPlatformId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpPlatformTypeId integer NOT NULL
                    , fk_MpDeviceTypeId integer NOT NULL
                    , Version text NULL 
                    , CONSTRAINT FK_MpPlatform_0_0 FOREIGN KEY (fk_MpDeviceTypeId) REFERENCES MpDeviceType (pk_MpDeviceTypeId)
                    , CONSTRAINT FK_MpPlatform_1_0 FOREIGN KEY (fk_MpPlatformTypeId) REFERENCES MpPlatformType (pk_MpPlatformTypeId)
                    );
                    CREATE TABLE MpCopyItemType (
                        pk_MpCopyItemTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , TypeName text NULL 
                    );
                    INSERT INTO MpCopyItemType(TypeName) VALUES('text'),('rich_text'),('html_text'),('image'),('file_list');
                    CREATE TABLE MpCommandType (
                        pk_MpCommandTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , CommandName text NOT NULL
                    );
                    CREATE TABLE MpCommand (
                        pk_MpCommandId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCommandTypeId int NOT NULL
                    , fk_MpHotKey int NOT NULL
                    , CONSTRAINT FK_MpCommand_0_0 FOREIGN KEY (fk_MpHotKey) REFERENCES MpHotKey (pk_MpHotKeyId)
                    , CONSTRAINT FK_MpCommand_1_0 FOREIGN KEY (fk_MpCommandTypeId) REFERENCES MpCommandType (pk_MpCommandTypeId)
                    );
                    CREATE TABLE MpClient (
                        pk_MpClientId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpPlatformId integer NOT NULL
                    , Ip4Address text NULL 
                    , AccessToken text NULL 
                    , LoginDateTime datetime NOT NULL
                    , LogoutDateTime datetime NULL
                    , CONSTRAINT FK_MpClient_0_0 FOREIGN KEY (fk_MpPlatformId) REFERENCES MpPlatform (pk_MpPlatformId)
                    );
                    CREATE TABLE MpColor (
                        pk_MpColorId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    ,  R integer not null default 255
                    ,  G integer not null default 255
                    ,  B integer not null default 255
                    ,  A integer not null default 255
                    );
                    CREATE TABLE MpApp (
                        pk_MpAppId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpIconId integer NOT NULL
                    , SourcePath text NOT NULL 
                    , IsAppRejected integer NOT NULL
                    , fk_ColorId integer 
                    , CONSTRAINT FK_MpApp_0_0 FOREIGN KEY (fk_MpIconId) REFERENCES MpIcon (pk_MpIconId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_ColorId) REFERENCES MpColor (pk_MpColorId)
                    );
                    CREATE TABLE MpCopyItem (
                        pk_MpCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemTypeId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer NOT NULL
                    , fk_ColorId integer 
                    , Title text NULL 
                    , CopyCount integer not null default 1
                    , PasteCount integer not null default 0
                    , CopyDateTime datetime DEFAULT (current_timestamp) NOT NULL
                    , CONSTRAINT FK_MpCopyItem_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpCopyItem_1_0 FOREIGN KEY (fk_MpClientId) REFERENCES MpClient (pk_MpClientId)
                    , CONSTRAINT FK_MpCopyItem_2_0 FOREIGN KEY (fk_MpCopyItemTypeId) REFERENCES MpCopyItemType (pk_MpCopyItemTypeId) 
                    , CONSTRAINT FK_MpCopyItem_3_0 FOREIGN KEY (fk_ColorId) REFERENCES MpColor (pk_MpColorId) 
                    );
                    CREATE TABLE MpSubTextToken (
                        pk_MpSubTextTokenId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpCopyItemTypeId integer NOT NULL
                    , StartIdx integer NOT NULL
                    , EndIdx integer NOT NULL
                    , InstanceIdx int NOT NULL
                    , CONSTRAINT FK_MpSubTextToken_0_0 FOREIGN KEY (fk_MpCopyItemTypeId) REFERENCES MpCopyItemType (pk_MpCopyItemTypeId) 
                    , CONSTRAINT FK_MpSubTextToken_1_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    CREATE TABLE MpPasteHistory (
                        pk_MpPasteHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer NOT NULL
                    , PasteDateTime datetime NOT NULL
                    , CONSTRAINT FK_MpPasteHistory_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpPasteHistory_1_0 FOREIGN KEY (fk_MpClientId) REFERENCES MpClient (pk_MpClientId)
                    , CONSTRAINT FK_MpPasteHistory_2_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    CREATE TABLE MpFileDropListItem (
                        pk_MpFileDropListItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , CONSTRAINT FK_MpFileDropListItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    CREATE TABLE MpFileDropListSubItem (
                        pk_MpFileDropListSubItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpFileDropListItemId integer NOT NULL
                    , ItemPath text NOT NULL 
                    , CONSTRAINT FK_MpFileDropListSubItem_0_0 FOREIGN KEY (fk_MpFileDropListItemId) REFERENCES MpFileDropListItem (pk_MpFileDropListItemId)
                    );
                    CREATE TABLE MpImageItem (
                        pk_MpImageItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , ItemImage longblob NOT NULL
                    , CONSTRAINT FK_MpImageItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    CREATE TABLE MpTextItem (
                        pk_MpTextItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , ItemText text NOT NULL 
                    , CONSTRAINT FK_MpTextItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
            ";            
        }
        private string GetClearString() {
            return @"   
                delete from MpPlatform where pk_MpPlatformId > 0;
                delete from MpClient where pk_MpClientId > 0;
                delete from MpIcon where pk_MpIconId > 0;
                delete from MpApp where pk_MpAppId > 0;
                delete from MpCopyItem where pk_MpCopyItemId > 0;
                delete from MpTextItem where pk_MpTextItemId > 0;
                delete from MpFileDropListItem where pk_MpFileDropListItemId > 0;
                delete from MpFileDropListSubItem where pk_MpFileDropListSubItemId > 0;
                delete from MpPasteHistory where pk_MpPasteHistoryId > 0;
                delete from MpSubTextToken where pk_MpSubTextTokenId > 0;
                delete from MpTagType where pk_MpTagType > 0;
                delete from MpTag where pk_MpTag > 0; 
                delete from MpSetting where pk_MpSetting > 0; 
                delete from MpHotKey where pk_MpHotKey > 0; 
                delete from MpCommand where pk_MpCommand > 0; 
                delete from MpColor where pk_MpColor > 0
            ";
        }
        private string GetDropString() {
            return @"   
                drop table if exists MpPlatform;
                drop table if exists MpClient;
                drop table if exists MpIcon;
                drop table if exists MpApp;
                drop table if exists MpCopyItem;
                drop table if exists MpTextItem;
                drop table if exists MpFileDropListItem;
                drop table if exists MpFileDropListSubItem;
                drop table if exists MpPasteHistory;
                drop table if exists MpSubTextToken;
                drop table if exists MpTagType;
                drop table if exists MpTag; 
                drop table if exists MpSetting; 
                drop table if exists MpHotKey; 
                drop table if exists MpCommand; 
                drop table if exists MpColor;
            ";
        }
    }
}
