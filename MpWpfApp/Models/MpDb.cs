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

        private int _passwordAttempts = 0;

        private bool _isLoaded = false;
        public bool IsLoaded {
            get {
                return _isLoaded;
            }
        }
        public MpDb() {
            Init();
        }
        private void Init() {
            InitUser(IdentityToken);
            InitClient(AccessToken);
            InitDb();
            _isLoaded = true;
        }
        private void InitDb() {
            //if db does not exist create it with a random password and set its path and password properties
            if (string.IsNullOrEmpty(Properties.Settings.Default.DbPath) || !File.Exists(Properties.Settings.Default.DbPath)) {
                Console.WriteLine("Db does not exist in " + MpHelpers.GetApplicationDirectory());
                Properties.Settings.Default.DbPath = MpHelpers.GetApplicationDirectory() + Properties.Settings.Default.DbName;
                Properties.Settings.Default.DbPassword = string.Empty;
                Properties.Settings.Default.Save();
                SQLiteConnection.CreateFile(Properties.Settings.Default.DbPath);
                ExecuteWrite(GetCreateString(), null);
                //SetDbPassword(MpHelpers.GetRandomString());
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

                    cib.WriteToDatabase();
                    mergedList.Add(cib);
                }
            }
            mergedList = mergedList.OrderByDescending(x => MpTypeHelper.GetPropertyValue(x, "CopyDateTime")).ToList();
            return mergedList;
        }
        
        public void SetDbPassword(string newPassword) {
            if (Properties.Settings.Default.DbPassword != newPassword) {
                // if db is unpassword protected
                if (string.IsNullOrEmpty(Properties.Settings.Default.DbPassword)) {
                    ExecuteWrite(
                        "PRAGMA key=@np;",
                        new Dictionary<string, object> {
                        { "@np", newPassword }
                        });
                } else {
                    ExecuteWrite(
                        "PRAGMA rekey=@np;",
                        new Dictionary<string, object> {
                        { "@np", newPassword }
                        });
                }
                Properties.Settings.Default.DbPassword = newPassword;
                Properties.Settings.Default.Save();
            }
        }
        private SQLiteConnection SetConnection(bool isInit = false) {
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
        public int ExecuteWrite(string query, Dictionary<string, object> args) {
            int numberOfRowsAffected;
            using (var con = SetConnection()) {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con)) {
                    if(args != null) {
                        foreach (var pair in args) {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                return numberOfRowsAffected;
            }
        }
        public DataTable Execute(string query, Dictionary<string, object> args) {
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }
            using (var con = SetConnection()) {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con)) {
                    if(args != null) {
                        foreach (KeyValuePair<string, object> entry in args) {
                            cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                        }
                    }
                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    return dt;
                }
            }
        }
        /*public void ExecuteWrite(string sql, List<string> paramList = null, List<object> paramValueList = null) {
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
                    if (paramList[i] == Properties.Settings.Default.DbParameterBinary) {
                        param = new SQLiteParameter(paramList[i], DbType.Binary);
                        param.Value = (byte[])paramValueList[i];
                    } else if (paramList[i] == Properties.Settings.Default.DbParameterString) {
                        param = new SQLiteParameter(paramList[i], DbType.String);
                        param.Value = (string)paramValueList[i];
                    } else if (paramList[i] == Properties.Settings.Default.DbParameterInt) {
                        param = new SQLiteParameter(paramList[i], DbType.Int32);
                        param.Value = (int)paramValueList[i];
                    } else if (paramList[i] == Properties.Settings.Default.DbParameterDateTime) {
                        param = new SQLiteParameter(paramList[i], DbType.DateTime);
                        param.Value = (DateTime)paramValueList[i];
                    }
                    sql_cmd.Parameters.Add(param);
                }
            }

            sql_con.Open();
            try {
                sql_cmd.ExecuteWrite();
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
                    if (paramList[i] == Properties.Settings.Default.DbParameterBinary) {
                        param = new SQLiteParameter(paramList[i], DbType.Binary);
                        param.Value = (byte[])paramValueList[i];
                    } else if (paramList[i] == Properties.Settings.Default.DbParameterString) {
                        param = new SQLiteParameter(paramList[i], DbType.String);
                        param.Value = (string)paramValueList[i];
                    } else if (paramList[i] == Properties.Settings.Default.DbParameterInt) {
                        param = new SQLiteParameter(paramList[i], DbType.Int32);
                        param.Value = (int)paramValueList[i];
                    } else if (paramList[i] == Properties.Settings.Default.DbParameterDateTime) {
                        param = new SQLiteParameter(paramList[i], DbType.DateTime);
                        param.Value = (DateTime)paramValueList[i];
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
        }*/
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
            DataTable dt = Execute("select * from " + tableName + " ORDER BY " + pkName + " DESC LIMIT 1;", null);
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
            ExecuteWrite(GetClearString(), null);
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
                    , SortIdx integer
                    , fk_MpColorId integer 
                    , CONSTRAINT FK_MpTag_0_0 FOREIGN KEY (fk_ParentTagId) REFERENCES MpTagType (pk_MpTagId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId)
                    );
                    INSERT INTO MpTag(TagName,fk_MpColorId,SortIdx) VALUES ('All',3,0),('Favorites',2,1);
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
                    CREATE TABLE MpShortcut (
                      pk_MpShortcutId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId INTEGER DEFAULT 0
                    , fk_MpTagId INTEGER DEFAULT 0
                    , ShortcutName text NOT NULL                    
                    , KeyList text NULL       
                    , DefaultKeyList text NULL
                    , RoutingType integer NOT NULL DEFAULT 0 
                    , CONSTRAINT FK_MpShortcut_1_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpShortcutItemId)
                    , CONSTRAINT FK_MpShortcut_2_0 FOREIGN KEY (fk_MpTagId) REFERENCES MpTag (pk_MpTagId)
                    );
                    INSERT INTO MpShortcut(ShortcutName,RoutingType,DefaultKeyList) VALUES
                    ('Show Window',2,'Control+Shift+D')
                    ,('Hide Window',1,'Escape')
                    ,('Append Mode',2,'Control+Shift+A')
                    ,('Auto-Copy Mode',2,'Control+Shift+C')
                    ,('Right-Click Paste Mode',2,'Control+Shift+R')
                    ,('Paste Selected Clip',1,'Enter')
                    ,('Delete Selected Clip',1,'Delete')
                    ,('Search',1,'S')
                    ,('Select Next',1,'Right')
                    ,('Select Previous',1,'Left')
                    ,('Select All',1,'Control+A')
                    ,('Invert Selection',1,'Control+Shift+Alt+A')
                    ,('Bring to front',1,'Home')
                    ,('Send to back',1,'End')
                    ,('Assign Hotkey',1,'')
                    ,('Change Color',1,'')
                    ,('Share',1,'')
                    ,('Say',1,'')
                    ,('Merge',1,'');
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
                    INSERT INTO MpCopyItemType(TypeName) VALUES ('rich_text'),('image'),('file_list'),('xaml');
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
                    , SourcePath text NOT NULL 
                    , AppName text 
                    , IsAppRejected integer NOT NULL                    
                    , IconBlob image NOT NULL
                    );                
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItem (
                      pk_MpCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemTypeId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer NOT NULL
                    , fk_MpPreCopyItemId integer
                    , fk_MpPostCopyItemId integer
                    , fk_MpColorId integer
                    , Title text NULL 
                    , CopyCount integer not null default 1
                    , PasteCount integer not null default 0
                    , ItemImage longblob
                    , ItemText text NOT NULL                     
                    , TitleSwirl longblob NOT NULL
                    , Screenshot longblob
                    , CopyDateTime datetime DEFAULT (current_timestamp) NOT NULL
                    , CONSTRAINT FK_MpCopyItem_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpCopyItem_1_0 FOREIGN KEY (fk_MpClientId) REFERENCES MpClient (pk_MpClientId)
                    , CONSTRAINT FK_MpCopyItem_2_0 FOREIGN KEY (fk_MpCopyItemTypeId) REFERENCES MpCopyItemType (pk_MpCopyItemTypeId) 
                    , CONSTRAINT FK_MpCopyItem_3_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId) 
                    , CONSTRAINT FK_MpCopyItem_4_0 FOREIGN KEY (fk_MpPreCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpCopyItem_5_0 FOREIGN KEY (fk_MpPostCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
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
                    CREATE TABLE MpDetectedImageObject (
                      pk_MpDetectedImageObjectId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , Confidence real NOT NULL
                    , X real NOT NULL
                    , Y real NOT NULL
                    , Width real NOT NULL
                    , Height real NOT NULL                    
                    , ObjectTypeName text
                    , CONSTRAINT FK_MpDetectedImageObject_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItemTemplate (
                      pk_MpCopyItemTemplateId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , HexColor text NOT NULL
                    , TemplateName text NOT NULL 
                    , CONSTRAINT FK_MpCopyItemTemplate_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)                    
                    );                    
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpRichTextStyle (
                      pk_MpRichTextStyleId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpColorId1 integer NOT NULL
                    , fk_MpColorId2 integer NOT NULL
                    , FontFamily text NOT NULL
                    , FontStyle integer NOT NULL
                    , FontSize real NOT NULL
                    , CONSTRAINT FK_MpRichTextStyle_0_0 FOREIGN KEY (fk_MpColorId1) REFERENCES MpColor (pk_MpColorId) 
                    , CONSTRAINT FK_MpRichTextStyle_1_0 FOREIGN KEY (fk_MpColorId2) REFERENCES MpColor (pk_MpColorId) 
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpPasteHistory (
                      pk_MpPasteHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer 
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
                select * from MpTagType where pk_MpTagTypeId > 0;
                select * from MpTag where pk_MpTag > 0; 
                select * from MpSetting where pk_MpSetting > 0; 
                select * from MpShortcut where pk_MpShortcut > 0; 
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
                delete from MpShortcut where pk_MpShortcut > 0; 
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
                drop table if exists MpShortcut; 
                drop table if exists MpColor;
            ";
        }
    }
}
