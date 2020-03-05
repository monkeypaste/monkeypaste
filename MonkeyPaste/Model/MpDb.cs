using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpDb {
        public MpClient Client { get; set; }
        public MpUser User { get; set; }

        public string DbPath { get; set; } 
        public string DbPassword { get; set; }

        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }

        public bool NoDb { get; set; }

        private int _passwordAttempts = 0;
        private bool _isLoaded = false;

        public MpDb(string dbPath,string dbPassword,string identityToken,string accessToken) {
            NoDb = MpCompatibility.IsRunningOnMono();
            Console.WriteLine("Running mono: " + NoDb);

            DbPath = dbPath;
            DbPassword = dbPassword;
            IdentityToken = identityToken;
            AccessToken = accessToken;
            Init();
        }
        public void Init() {
            InitUser(IdentityToken);
            InitClient(AccessToken);
            InitDb();
        }
        public void InitDb() {
            if(NoDb) {
                Console.WriteLine("Skipping Db Init");
                return;
            }
            if(DbPath == null || DbPath == String.Empty || !Directory.Exists(Path.GetDirectoryName(DbPath)) || !File.Exists(DbPath)) {
                Console.WriteLine(DbPath + " does not exist...");
                DialogResult result = MessageBox.Show("No Database found would you like to load a file?","No DB Found",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                if(result == DialogResult.Yes) {
                    OpenFileDialog openFileDialog = new OpenFileDialog() {
                        FileName = "Select a db file",
                        Filter = "Db files (*.db)|*.db",
                        Title = "Open DB File"
                    };
                    DialogResult openResult = openFileDialog.ShowDialog();
                    if(openResult == DialogResult.OK) {
                        DbPath = openFileDialog.FileName;
                        DialogResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?","Remember Database?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                        if(autoLoadResult == DialogResult.Yes) {
                            MpRegistryHelper.Instance.SetValue("DBPath",DbPath);
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
                            DbPath = saveFileDialog.FileName;
                            DialogResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?","Remember Database?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                            if(autoLoadResult == DialogResult.Yes) {
                                MpRegistryHelper.Instance.SetValue("DBPath",DbPath);
                            }
                            SQLiteConnection.CreateFile(DbPath);
                            DialogResult newDbPasswordResult = MessageBox.Show("Would you like to encrypt database with a password?","Encrypt?",MessageBoxButtons.YesNo,MessageBoxIcon.Question,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                            if(newDbPasswordResult == DialogResult.Yes) {
                                MpSetDbPasswordForm setDbPasswordForm = new MpSetDbPasswordForm();
                                setDbPasswordForm.ShowDialog();
                                SetDbPassword(setDbPasswordForm.PasswordTextBox.Text);
                            }
                            ExecuteNonQuery(GetCreateString());
                        }
                        else {
                            NoDb = true;
                        }
                    }
                    else {
                        NoDb = true;
                    }
                }
            }
            Console.WriteLine("Database successfully initialized at " + DbPath);
            _isLoaded = true;
        }
        public void InitUser(string idToken) {
           // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            Client = new MpClient(0,3,MpHelperSingleton.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(),accessToken,DateTime.Now);
        }
        public List<MpCopyItem> MergeCopyItemLists(List<MpCopyItem> listA,List<MpCopyItem> listB) {
            //sorts merged list by copy datetime
            List<MpCopyItem> mergedList = new List<MpCopyItem>();
            if(listA != null) {
                foreach(MpCopyItem cia in listA) {
                    mergedList.Add(cia);
                }
            }
            if(listB != null) {
                foreach(MpCopyItem cib in listB) {
                    //clear merged copyitems db id so it gets a new one in current list

                    if(NoDb) {
                        cib.CopyItemId = ++MpCopyItem.TotalCopyItemCount;                        
                    }
                    else {
                        //cib = MpCopyItem.CreateCopyItem(0,(MpCopyItemType)cib.copyItemTypeId,cib.Client)
                        cib.WriteToDatabase();
                    }
                    mergedList.Add(cib);
                }
            }
            mergedList = mergedList.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x,"CopyDateTime")).ToList();
            return mergedList;
        }
        public List<MpCopyItem> GetCopyItems(string altPath = "",string altPassword = "") {
            if(NoDb) {
                return new List<MpCopyItem>();
            }
            string tempPath = "", tempPassword = "";
            if(altPath != "") {
                tempPath = DbPath;
                tempPassword = DbPassword;
                DbPath = altPath;
                DbPassword = altPassword;
                
            } else if(_isLoaded == false) {
                InitDb();
            }
            List<MpCopyItem> copyItemList = new List<MpCopyItem>();
            DataTable dt = Execute("select * from MpCopyItem");
            if(dt != null) {
                foreach(DataRow dr in dt.Rows) {
                    copyItemList.Add(new MpCopyItem(dr));
                }
            }
            if(altPath != "") {
                DbPath = tempPath;
                DbPassword = tempPassword;

            }
            Console.WriteLine(copyItemList.Count + " copyitems gathered");
            return copyItemList;
        }        
        public List<MpTag> GetTags() {
            if(NoDb) {
                return new List<MpTag>() { new MpTag("History",Color.Green,MpTagType.Default),new MpTag("Favorites",Color.Blue,MpTagType.Default) };
            }
            if(_isLoaded == false) {
                InitDb();
            }
            List<MpTag> tagList = new List<MpTag>();
            DataTable dt = Execute("select * from MpTag");
            if(dt != null) {
                foreach(DataRow dr in dt.Rows) {
                    tagList.Add(new MpTag(dr));
                }
            }
            Console.WriteLine("Init w/ " + tagList.Count + " tags added");
            return tagList;
        }
        public void SetDbPassword(string newPassword) {
            if(DbPassword != newPassword) {
                // if db is unpassword protected
                if(DbPassword == null || DbPassword == String.Empty) {
                    SQLiteConnection conn = new SQLiteConnection("Data Source=" + DbPath + ";Version=3;");
                    conn.SetPassword(newPassword);
                    conn.Open();
                    conn.Close();
                }
                else {
                    SQLiteConnection conn = SetConnection();
                    conn.Open();
                    conn.ChangePassword(newPassword);
                }
                DbPassword = newPassword;
            }
        }
        private SQLiteConnection SetConnection() {
            if(NoDb) {
                return null;
            }
            // see https://stackoverflow.com/questions/1381264/password-protect-a-sqlite-db-is-it-possible
            // about passwords
            string connStr = "Data Source=" + DbPath + ";Version=3;";
            if(DbPassword != null && DbPassword != String.Empty) {
                connStr += "Password=" + DbPassword + ";";
            }
            //Console.WriteLine("Connection String: " + connStr);
            SQLiteConnection conn = null;
            try {
                conn = new SQLiteConnection(connStr);
            }
            catch(Exception e) {
                Console.WriteLine("Error during SQL connection: " + connStr + "\n" + "With error: " + e.ToString());
                conn = null;
                DbPath = null;
                InitDb();
                SetConnection();
            }
            return conn;
        }
        public void ExecuteNonQuery(string sql,List<string> paramList = null,List<object> paramValueList = null) {
            if(NoDb || _passwordAttempts > Properties.Settings.Default.MaxDbPasswordAttempts) {
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
                Console.WriteLine("Error in executenonquery: " + ex.ToString());
                if(_isLoaded) {
                    DialogResult warnDbErrorResult = MessageBox.Show("Error writing data to " + DbPath + " and program terminating","IO Error",MessageBoxButtons.OK,MessageBoxIcon.Error,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                   // MpSingletonController.Instance.ExitApplication();
                } else {
                    MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                    DialogResult enterPasswordResult = enterPasswordForm.ShowDialog();
                    _passwordAttempts++;
                    if(_passwordAttempts < Properties.Settings.Default.MaxDbPasswordAttempts) {
                        Init();
                    }
                    else {
                        return;
                    }
                }              
            }
            if(!wasError) {
                _passwordAttempts = 0;
            }
            sql_con.Close();
            
        }
        public DataTable Execute(string sql,List<string> paramList = null,List<object> paramValueList = null) {
            if(NoDb || _passwordAttempts > Properties.Settings.Default.MaxDbPasswordAttempts) {
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
                wasError = true;
                Console.WriteLine("Error in sql execute  " + ex.ToString());
                if(_isLoaded) {
                    DialogResult warnDbErrorResult = MessageBox.Show("Error writing data to " + DbPath + " and program terminating","IO Error",MessageBoxButtons.OK,MessageBoxIcon.Error,MessageBoxDefaultButton.Button1,MessageBoxOptions.DefaultDesktopOnly);
                    MpSingletonController.Instance.ExitApplication();
                }
                else {
                    MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                    DialogResult enterPasswordResult = enterPasswordForm.ShowDialog();
                    _passwordAttempts++;
                    if(_passwordAttempts < Properties.Settings.Default.MaxDbPasswordAttempts) {
                        Init();
                    }
                    else {
                        return null;
                    }
                }
            }
            if(!wasError) {
                _passwordAttempts = 0;
            }
            return DT;
        }
        public void CloseDb() {
            SQLiteConnection sql_con = SetConnection();
            sql_con.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
                      pk_MpTagTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , TagTypeName text NOT NULL
                    );
                    INSERT INTO MpTagType(TagTypeName) VALUES('None'),('App'),('Device'),('Custom'),('Default');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpTag (
                      pk_MpTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpTagTypeId integer NOT NULL
                    , TagName text NOT NULL
                    , fk_MpColorId integer 
                    , CONSTRAINT FK_MpTag_0_0 FOREIGN KEY (fk_MpTagTypeId) REFERENCES MpTagType (pk_MpTagTypeId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId)
                    );
                    INSERT INTO MpTag(fk_MpTagTypeId,TagName,fk_MpColorId) VALUES (5,'History',3),(5,'Favorites',2);
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpSetting (
                      pk_MpSettingId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , SettingName text NOT NULL
                    , SettingValue text NULL
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpPlatformType (
                      pk_MpPlatformTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , PlatformName text NOT NULL 
                    );
                    INSERT INTO MpPlatformType(PlatformName) VALUES('ios'),('android'),('windows'),('mac'),('linux');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpIcon (
                      pk_MpIconId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , IconBlob image NOT NULL
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpHotKey (
                      pk_MpHotKeyId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , KeyList text NULL
                    , ModList text NULL
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpDeviceType (
                      pk_MpDeviceTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , DeviceTypeName text NULL 
                    );
                    INSERT INTO MpDeviceType(DeviceTypeName) VALUES('windows'),('mac'),('android'),('iphone'),('ipad'),('tablet');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpPlatform (
                      pk_MpPlatformId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpPlatformTypeId integer NOT NULL
                    , fk_MpDeviceTypeId integer NOT NULL
                    , Version text NULL 
                    , CONSTRAINT FK_MpPlatform_0_0 FOREIGN KEY (fk_MpDeviceTypeId) REFERENCES MpDeviceType (pk_MpDeviceTypeId)
                    , CONSTRAINT FK_MpPlatform_1_0 FOREIGN KEY (fk_MpPlatformTypeId) REFERENCES MpPlatformType (pk_MpPlatformTypeId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItemType (
                      pk_MpCopyItemTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , TypeName text NULL 
                    );
                    INSERT INTO MpCopyItemType(TypeName) VALUES('text'),('rich_text'),('html_text'),('image'),('file_list');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCommandType (
                      pk_MpCommandTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , CommandName text NOT NULL
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCommand (
                      pk_MpCommandId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCommandTypeId int NOT NULL
                    , fk_MpHotKey int NOT NULL
                    , CONSTRAINT FK_MpCommand_0_0 FOREIGN KEY (fk_MpHotKey) REFERENCES MpHotKey (pk_MpHotKeyId)
                    , CONSTRAINT FK_MpCommand_1_0 FOREIGN KEY (fk_MpCommandTypeId) REFERENCES MpCommandType (pk_MpCommandTypeId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpClient (
                      pk_MpClientId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpPlatformId integer NOT NULL
                    , Ip4Address text NULL 
                    , AccessToken text NULL 
                    , LoginDateTime datetime NOT NULL
                    , LogoutDateTime datetime NULL
                    , CONSTRAINT FK_MpClient_0_0 FOREIGN KEY (fk_MpPlatformId) REFERENCES MpPlatform (pk_MpPlatformId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpColor (
                       pk_MpColorId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    ,  R integer not null default 255
                    ,  G integer not null default 255
                    ,  B integer not null default 255
                    ,  A integer not null default 255
                    );
                    INSERT INTO MpColor(R,G,B,A) VALUES (255,0,0,255),(0,255,0,255),(0,0,255,255);
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpApp (
                      pk_MpAppId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpIconId integer NOT NULL
                    , SourcePath text NOT NULL 
                    , IsAppRejected integer NOT NULL
                    , fk_ColorId integer 
                    , CONSTRAINT FK_MpApp_0_0 FOREIGN KEY (fk_MpIconId) REFERENCES MpIcon (pk_MpIconId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_ColorId) REFERENCES MpColor (pk_MpColorId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItem (
                      pk_MpCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemTypeId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer NOT NULL
                    , fk_MpColorId integer 
                    , Title text NULL 
                    , CopyCount integer not null default 1
                    , CopyDateTime datetime DEFAULT (current_timestamp) NOT NULL
                    , CONSTRAINT FK_MpCopyItem_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpCopyItem_1_0 FOREIGN KEY (fk_MpClientId) REFERENCES MpClient (pk_MpClientId)
                    , CONSTRAINT FK_MpCopyItem_2_0 FOREIGN KEY (fk_MpCopyItemTypeId) REFERENCES MpCopyItemType (pk_MpCopyItemTypeId) 
                    , CONSTRAINT FK_MpCopyItem_3_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId) 
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItemTag (
                      pk_MpCopyItemTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpTagId integer NOT NULL
                    , CONSTRAINT FK_MpCopyItemTag_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpCopyItemTag_1_0 FOREIGN KEY (fk_MpTagId) REFERENCES MpTag (pk_MpTagId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
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
                    ---------------------------------------------------------------------------------------------------------------------
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
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpFileDropListItem (
                      pk_MpFileDropListItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , CONSTRAINT FK_MpFileDropListItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpFileDropListSubItem (
                      pk_MpFileDropListSubItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpFileDropListItemId integer NOT NULL
                    , ItemPath text NOT NULL 
                    , CONSTRAINT FK_MpFileDropListSubItem_0_0 FOREIGN KEY (fk_MpFileDropListItemId) REFERENCES MpFileDropListItem (pk_MpFileDropListItemId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpImageItem (
                      pk_MpImageItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , ItemImage longblob NOT NULL
                    , CONSTRAINT FK_MpImageItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpTextItem (
                      pk_MpTextItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , ItemText text NOT NULL 
                    , CONSTRAINT FK_MpTextItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
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
                delete from MpTagType where pk_MpTagTypeId > 0;
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
