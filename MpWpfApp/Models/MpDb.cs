using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace MpWpfApp {
    public class MpDb {
        private static readonly Lazy<MpDb> _Lazy = new Lazy<MpDb>(() => new MpDb());
        public static MpDb Instance { get { return _Lazy.Value; } }

        public MpClient Client { get; set; }
        public MpUser User { get; set; }

        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }

        private bool _noDb = false;
        public bool NoDb {
            get {
                return _noDb;
            }
            set {
                if (_noDb != value) {
                    _noDb = value;
                    if (_noDb == true) {
                        _isLoaded = true;
                    }
                }
            }
        }

        private int _passwordAttempts = 0;
        private bool _isLoaded = false;
        public bool IsLoaded {
            get {
                return _isLoaded;
            }
        }
        public MpDb() {
            //NoDb = true;
            Init();
        }
        private void Init() {
            InitUser(IdentityToken);
            InitClient(AccessToken);
            InitDb();
            _isLoaded = true;
        }
        private void InitDb() {
            if (NoDb) {
                Console.WriteLine("Database exists, skipping Db creation");
                return;
            }
            if (string.IsNullOrEmpty(Properties.Settings.Default.DbPath) || 
                !Directory.Exists(Path.GetDirectoryName(Properties.Settings.Default.DbPath)) || 
                !File.Exists(Properties.Settings.Default.DbPath)) {
                Console.WriteLine(Properties.Settings.Default.DbPath + " does not exist...");
                MessageBoxResult result = MessageBox.Show("No Database found would you like to load a file?", "No DB Found", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                if (result == MessageBoxResult.Yes) {
                    OpenFileDialog openFileDialog = new OpenFileDialog() {
                        FileName = "Select a db file",
                        Filter = "Db files (*.db)|*.db",
                        Title = "Open DB File"
                    };
                    bool? openResult = openFileDialog.ShowDialog();
                    if (openResult != null && openResult.Value) {
                        Properties.Settings.Default.DbPath = openFileDialog.FileName;
                        MessageBoxResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?", "Remember Database?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (autoLoadResult == MessageBoxResult.Yes) {
                            Properties.Settings.Default.Save();
                        }
                    }
                } else {
                    MessageBoxResult newDbResult = MessageBox.Show("Would you like to create a new database and store your history?", "New Database?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                    if (newDbResult == MessageBoxResult.Yes) {
                        SaveFileDialog saveFileDialog = new SaveFileDialog() {
                            InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                            FileName = "mp.db",
                            Filter = "Db files (*.db)|*.db",
                            Title = "Create new DB File"
                        };

                        bool? saveResult = saveFileDialog.ShowDialog();
                        if (saveResult != null && saveResult.Value) {
                            Properties.Settings.Default.DbPath = saveFileDialog.FileName;
                            MessageBoxResult autoLoadResult = MessageBox.Show("Would you like to remember this next time?", "Remember Database?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                            if (autoLoadResult == MessageBoxResult.Yes) {
                                Properties.Settings.Default.Save();
                            }
                            SQLiteConnection.CreateFile(Properties.Settings.Default.DbPath);
                            MessageBoxResult newDbPasswordResult = MessageBox.Show("Would you like to encrypt database with a password?", "Encrypt?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                            if (newDbPasswordResult == MessageBoxResult.Yes) {
                                throw new Exception("Create WPF password form and show here");
                                //MpSetDbPasswordForm setDbPasswordForm = new MpSetDbPasswordForm();
                                //setDbPasswordForm.ShowDialog();
                                //SetDbPassword(setDbPasswordForm.PasswordTextBox.Text);
                            }
                            // TODO Add last msgbox to ask if password should be remembered next time
                            ExecuteNonQuery(GetCreateString());
                        } else {
                            NoDb = true;
                        }
                    } else {
                        NoDb = true;
                    }
                }
            }
            Console.WriteLine("Database successfully initialized at " + Properties.Settings.Default.DbPath);
            _isLoaded = true;
        }
        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            Client = new MpClient(0, 3, MpHelpers.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
        }
        public List<MpCopyItem> MergeCopyItemLists(List<MpCopyItem> listA, List<MpCopyItem> listB) {
            //sorts merged list by copy datetime
            List<MpCopyItem> mergedList = new List<MpCopyItem>();
            if (listA != null) {
                foreach (MpCopyItem cia in listA) {
                    mergedList.Add(cia);
                }
            }
            if (listB != null) {
                foreach (MpCopyItem cib in listB) {
                    //clear merged copyitems db id so it gets a new one in current list

                    if (NoDb) {
                        cib.CopyItemId = mergedList.Count + 1;
                    } else {
                        //cib = MpCopyItem.CreateCopyItem(0,(MpCopyItemType)cib.copyItemTypeId,cib.Client)
                        cib.WriteToDatabase();
                    }
                    mergedList.Add(cib);
                }
            }
            mergedList = mergedList.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x, "CopyDateTime")).ToList();
            return mergedList;
        }
        public List<MpSetting> GetAppSettingList() {
            List<MpSetting> appSettingList = new List<MpSetting>();
            DataTable dt = Execute("select * from MpSetting where pk_MpSettingId >= 0");
            if (dt != null) {
                foreach (DataRow dr in dt.Rows) {
                    appSettingList.Add(new MpSetting(dr));
                }
            }
            Console.WriteLine(appSettingList.Count + " settings gathered from db: " + Properties.Settings.Default.DbPath);
            return appSettingList;
        }
        public List<MpApp> GetExcludedAppList() {
            List<MpApp> excludedAppList = new List<MpApp>();
            DataTable dt = Execute("select * from MpApp where IsAppRejected > 0");
            if (dt != null) {
                foreach (DataRow dr in dt.Rows) {
                    excludedAppList.Add(new MpApp(dr));
                }
            }
            Console.WriteLine(excludedAppList.Count + " exxcluded gathered from db: " + Properties.Settings.Default.DbPath);
            return excludedAppList;
        }
        public List<MpTag> GetTags() {
            if (NoDb) {
                return new List<MpTag>() { new MpTag(Properties.Settings.Default.HistoryTagTitle, Colors.Green), new MpTag("Favorites", Colors.Blue) };
            }
            //if(_isLoaded == false) {
            //    InitDb();
            //}
            List<MpTag> tagList = new List<MpTag>();
            DataTable dt = Execute("select * from MpTag");
            if (dt != null) {
                foreach (DataRow dr in dt.Rows) {
                    tagList.Add(new MpTag(dr));
                }
            }
            Console.WriteLine("Init w/ " + tagList.Count + " tags added");
            return tagList;
        }
        public void SetDbPassword(string newPassword) {
            if (Properties.Settings.Default.DbPassword != newPassword) {
                // if db is unpassword protected
                if (string.IsNullOrEmpty(Properties.Settings.Default.DbPassword)) {
                    ExecuteNonQuery("PRAGMA key='" + newPassword + "';");
                } else {
                    ExecuteNonQuery("PRAGMA rekey='" + newPassword + "';");
                }
                Properties.Settings.Default.DbPassword = newPassword;
            }
        }
        private SQLiteConnection SetConnection() {
            if (NoDb) {
                return null;
            }
            // see https://stackoverflow.com/questions/1381264/password-protect-a-sqlite-db-is-it-possible
            // about passwords
            SQLiteConnectionStringBuilder connStr = new SQLiteConnectionStringBuilder();
            connStr.DataSource = Properties.Settings.Default.DbPath;
            connStr.Version = 3;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DbPassword)) {
                connStr.Password = Properties.Settings.Default.DbPassword;
            }
            //Console.WriteLine("Connection String: " + connStr);
            SQLiteConnection conn = null;
            try {
                conn = new SQLiteConnection(connStr.ConnectionString);
            }
            catch (Exception e) {
                Console.WriteLine("Error during SQL connection: " + connStr + "\n" + "With error: " + e.ToString());
                conn = null;
                Properties.Settings.Default.DbPath = null;
                InitDb();
                SetConnection();
            }
            return conn;
        }
        public void ExecuteNonQuery(string sql, List<string> paramList = null, List<object> paramValueList = null) {
            if (NoDb || _passwordAttempts > Properties.Settings.Default.MaxDbPasswordAttempts) {
                return;
            }
            if ((paramList != null && paramValueList != null) && (paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count)) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return;
            }
            bool wasError = false;
            SQLiteConnection sql_con = SetConnection();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = sql;

            if (paramList != null) {
                for (int i = 0; i < paramList.Count; i++) {
                    // check p to conditionally set parameter type
                    SQLiteParameter param = null;
                    if (paramList[i] == "@0") {
                        param = new SQLiteParameter(paramList[i], DbType.Binary);
                        param.Value = (byte[])paramValueList[i];
                    } else if (paramList[i] == "@1") {
                        param = new SQLiteParameter(paramList[i], DbType.String);
                        param.Value = (string)paramValueList[i];
                    }
                    sql_cmd.Parameters.Add(param);
                }
            }

            sql_con.Open();
            try {
                sql_cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex) {
                wasError = true;
                Console.WriteLine("Error in executenonquery: " + ex.ToString());
                if (_isLoaded) {
                    MessageBoxResult warnDbErrorResult = MessageBox.Show("Error writing data to " + Properties.Settings.Default.DbPath + " and program terminating", "IO Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                    // MpSingletonController.Instance.ExitApplication();
                } else {
                    //MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                    //MessageBoxResult enterPasswordResult = enterPasswordForm.ShowDialog();

                    _passwordAttempts++;
                    if (_passwordAttempts < Properties.Settings.Default.MaxDbPasswordAttempts) {
                        Init();
                    } else {
                        return;
                    }

                    throw new Exception("Create WPF password form and show here");
                }
            }
            if (!wasError) {
                _passwordAttempts = 0;
            }
            sql_con.Close();
            //sql_con.Dispose();
            //CloseDb();
        }
        public DataTable Execute(string sql, List<string> paramList = null, List<object> paramValueList = null) {
            if (NoDb || _passwordAttempts > Properties.Settings.Default.MaxDbPasswordAttempts) {
                return null;
            }
            if ((paramList != null && paramValueList != null) && (paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count)) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return null;
            }
            bool wasError = false;
            SQLiteConnection sql_con = SetConnection();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = sql;
            DataSet DS = new DataSet();
            DataTable DT = new DataTable();

            if (paramList != null) {
                for (int i = 0; i < paramList.Count; i++) {
                    // check p to conditionally set parameter type
                    SQLiteParameter param = null;
                    if (paramList[i] == "@0") {
                        param = new SQLiteParameter(paramList[i], DbType.Binary);
                        param.Value = (byte[])paramValueList[i];
                    } else if (paramList[i] == "@1") {
                        param = new SQLiteParameter(paramList[i], DbType.String);
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
                //sql_con.Dispose();
                //DB.Dispose();
                //DS.Dispose();
                //CloseDb();
            }
            catch (SQLiteException ex) {
                wasError = true;
                Console.WriteLine("Error in sql execute  " + ex.ToString());
                if (_isLoaded) {
                    MessageBoxResult warnDbErrorResult = MessageBox.Show("Error writing data to " + Properties.Settings.Default.DbPath + " and program terminating", "IO Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);

                    throw new Exception("Add exit app call here");
                } else {
                    //MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                    //MessageBoxResult enterPasswordResult = enterPasswordForm.ShowDialog();

                    _passwordAttempts++;
                    if (_passwordAttempts < Properties.Settings.Default.MaxDbPasswordAttempts) {
                        Init();
                    } else {
                        return null;
                    }

                    throw new Exception("Create WPF password form and show here");
                }
            }
            if (!wasError) {
                _passwordAttempts = 0;
            }
            return DT;
        }
        public void CloseDb() {
            SQLiteConnection sql_con = SetConnection();
            sql_con.Close();
            sql_con.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        public void WriteDatabaseToXmlFile(string out_filename) {
            SQLiteConnection sqc = SetConnection();
            sqc.Open();
            var sqd = new SqlDataAdapter(null, GetAllData());
            var ds = new DataSet();
            sqd.Fill(ds, "adv");
            SQLiteConnection sql_con = SetConnection();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            ds.WriteXml(out_filename);
        }
        public int GetLastRowId(string tableName, string pkName) {
            DataTable dt = Execute("select * from " + tableName + " ORDER BY " + pkName + " DESC LIMIT 1;");
            if (dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0][0].ToString());
            }
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
            // File.Delete(Properties.Settings.Default.DbPath);
            //InitDb();
            ExecuteNonQuery(GetClearString());
        }
        public void DeleteDb() {
            File.Delete(Properties.Settings.Default.DbPath);
        }
        private string GetCreateString() {
            return @"
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpTag (
                      pk_MpTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_ParentTagId integer
                    , TagName text
                    , fk_MpColorId integer 
                    , CONSTRAINT FK_MpTag_0_0 FOREIGN KEY (fk_ParentTagId) REFERENCES MpTagType (pk_MpTagId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId)
                    );
                    INSERT INTO MpTag(TagName,fk_MpColorId) VALUES ('All',3),('Favorites',2);
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
                    INSERT INTO MpPlatformType(PlatformName) VALUES('ios'),('android'),('windows'),('mac'),('linux'),('web');
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
                    INSERT INTO MpDeviceType(DeviceTypeName) VALUES('pc'),('mac'),('android'),('iphone'),('ipad'),('tablet'),('web');
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
                    INSERT INTO MpCopyItemType(TypeName) VALUES ('rich_text'),('image'),('file_list');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpSortType (
                      pk_MpSortTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , SortTypeName text NULL 
                    );
                    INSERT INTO MpSortType(SortTypeName) VALUES('Date'),('Application'),('Title'),('Content'),('Type'),('Usage');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItemSortTypeOrder (
                      pk_MpCopyItemSortTypeOrderId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpSortTypeId integer NOT NULL
                    , SortOrder integer NOT NULL 
                    , CONSTRAINT FK_MpCopyItemSortTypeOrder_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpCopyItemSortTypeOrder_1_0 FOREIGN KEY (fk_MpSortTypeId) REFERENCES MpSortType (pk_MpSortTypeId)
                    );
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
                    , PasteCount integer not null default 0
                    , ItemImage longblob NOT NULL
                    , ItemText text NOT NULL 
                    , Screenshot longblob
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
                    CREATE TABLE MpSubTextTokenType (
                      pk_MpSubTextTokenTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , TokenTypeName text NOT NULL 
                    );
                    INSERT INTO MpSubTextTokenType(TokenTypeName) VALUES('URI'),('Email'),('Phone Number'),('Currency'),('Hex Color'),('Street Address');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpSubTextToken (
                      pk_MpSubTextTokenId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpSubTextTokenTypeId integer NOT NULL
                    , StartIdx integer NOT NULL
                    , EndIdx integer NOT NULL
                    , BlockIdx int NOT NULL
                    , InlineIdx int NOT NULL
                    , TokenText text NOT NULL
                    , CONSTRAINT FK_MpSubTextToken_0_0 FOREIGN KEY (fk_MpSubTextTokenTypeId) REFERENCES MpSubTextTokenType (pk_MpSubTextTokenTypeId) 
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
            ";
        }
        private string GetAllData() {
            return @"   
                select * from MpPlatform where pk_MpPlatformId > 0;
                select * from MpClient where pk_MpClientId > 0;
                select * from MpIcon where pk_MpIconId > 0;
                select * from MpApp where pk_MpAppId > 0;
                select * from MpCopyItem where pk_MpCopyItemId > 0;
                select * from MpTextItem where pk_MpTextItemId > 0;
                select * from MpFileDropListItem where pk_MpFileDropListItemId > 0;
                select * from MpFileDropListSubItem where pk_MpFileDropListSubItemId > 0;
                select * from MpPasteHistory where pk_MpPasteHistoryId > 0;
                select * from MpSubTextToken where pk_MpSubTextTokenId > 0;
                select * from MpTagType where pk_MpTagTypeId > 0;
                select * from MpTag where pk_MpTag > 0; 
                select * from MpSetting where pk_MpSetting > 0; 
                select * from MpHotKey where pk_MpHotKey > 0; 
                select * from MpCommand where pk_MpCommand > 0; 
                select * from MpColor where pk_MpColor > 0
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
