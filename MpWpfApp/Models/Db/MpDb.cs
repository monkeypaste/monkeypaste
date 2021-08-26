using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
//using SQLite;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using SQLite;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using Microsoft.Win32;
using MonkeyPaste;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Data;
//using Xamarin.Forms;

namespace MpWpfApp {
    public class MpDb : MonkeyPaste.MpISync {
        private static readonly Lazy<MpDb> _Lazy = new Lazy<MpDb>(() => new MpDb());
        public static MpDb Instance { get { return _Lazy.Value; } }

        public MpClient Client { get; set; }
        public MpUser User { get; set; }

        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }

        private object _rdLock;
        //private int _passwordAttempts = 0;

        private bool _isLoaded = false;

        public event EventHandler<object> OnSyncableChange;

        public bool IsLoaded {
            get {
                return _isLoaded;
            }
        }
        public MpDb() {
            Init();
        }
        private void Init() {
            if(string.IsNullOrEmpty(Properties.Settings.Default.ThisClientGuid)) {
                Properties.Settings.Default.ThisClientGuid = Guid.NewGuid().ToString();
            }
            InitUser(IdentityToken);
            InitClient(AccessToken);
            InitDb();
            _isLoaded = true;
        }
        private void InitDb() {
            if(File.Exists(Properties.Settings.Default.DbPath)) {
                File.Delete(Properties.Settings.Default.DbPath);
            }

            //if db does not exist create it with a random password and set its path and password properties
            if (string.IsNullOrEmpty(Properties.Settings.Default.DbPath) || 
                !File.Exists(Properties.Settings.Default.DbPath)) {
                Console.WriteLine("Db does not exist in " + MpHelpers.Instance.GetApplicationDirectory());
                Properties.Settings.Default.DbPath = MpHelpers.Instance.GetApplicationDirectory() + Properties.Settings.Default.DbName;
                Properties.Settings.Default.DbPassword = string.Empty;
                Properties.Settings.Default.Save();
                System.Data.SQLite.SQLiteConnection.CreateFile(Properties.Settings.Default.DbPath);
                ExecuteWrite(GetCreateString(), null);
                //SetDbPassword(MpHelpers.Instance.GetRandomString());
            }
            InitPCL();
            Console.WriteLine("Database successfully initialized at " + Properties.Settings.Default.DbPath);
            _isLoaded = true;
        }
        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            Client = new MpClient(0, 3, MpHelpers.Instance.GetLocalIp4Address()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
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
                    using (var con = SetConnection()) {
                        con.Open();
                        var query = @"PRAGMA key=" + newPassword;
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con)) {
                            cmd.ExecuteNonQuery();
                        }
                    }
                } else {
                    using (var con = SetConnection()) {
                        con.Open();
                        var query = @"PRAGMA rekey=" + newPassword;
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con)) {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    
                }
                Properties.Settings.Default.DbPassword = newPassword;
                Properties.Settings.Default.Save();

                Console.WriteLine("DbPassword is: " + newPassword);
            }
        }

        private System.Data.SQLite.SQLiteConnection SetConnection(bool isInit = false) {
            // see https://stackoverflow.com/questions/1381264/password-protect-a-sqlite-db-is-it-possible
            // about passwords
            SQLiteConnectionStringBuilder connStr = new SQLiteConnectionStringBuilder();
            connStr.DataSource = Properties.Settings.Default.DbPath;
            connStr.Version = 3; 
            
            if (!string.IsNullOrEmpty(Properties.Settings.Default.DbPassword)) {
                //connStr.Password = Properties.Settings.Default.DbPassword;
            }
            //Console.WriteLine("Connection String: " + connStr);
            System.Data.SQLite.SQLiteConnection conn = null; 
            try {
                conn = new System.Data.SQLite.SQLiteConnection(connStr.ConnectionString);
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

        private SQLite.SQLiteConnection SetConnectionPCL(bool isInit = false) {
            // see https://stackoverflow.com/questions/1381264/password-protect-a-sqlite-db-is-it-possible
            // about passwords
            SQLiteConnectionString connStr = new SQLiteConnectionString(
                databasePath: Properties.Settings.Default.DbPath,
                storeDateTimeAsTicks: false,
                //key: MpPreferences.Instance.DbPassword,
                openFlags: SQLiteOpenFlags.ReadWrite |
                           SQLiteOpenFlags.Create |
                           SQLiteOpenFlags.SharedCache |
                           SQLiteOpenFlags.FullMutex
                );

            if (!string.IsNullOrEmpty(Properties.Settings.Default.DbPassword)) {
                //connStr.Password = Properties.Settings.Default.DbPassword;
            }
            //Console.WriteLine("Connection String: " + connStr);
            SQLite.SQLiteConnection conn = null;
            try {
                conn = new SQLite.SQLiteConnection(connStr);
            }
            catch (Exception e) {
                Console.WriteLine("Error during SQL connection: " + connStr + "\n" + "With error: " + e.ToString());
                conn = null;
                Properties.Settings.Default.DbPath = null;
                InitDb();
                SetConnection(isInit);
            }
            return conn;
        }

        private void InitPCL() {
            using (var con = SetConnectionPCL()) {
                //con.CreateTable<MpApp>();
                //con.CreateTable<MpClient>();
                //con.CreateTable<MpClientPlatform>();
                //con.CreateTable<MpClip>();
                //con.CreateTable<MpClipComposite>();
                //con.CreateTable<MpClipTag>();
                //con.CreateTable<MpClipTemplate>();
                con.CreateTable<MpColor>();
                //con.CreateTable<MpDbImage>();
                //con.CreateTable<MpIcon>();
                //con.CreateTable<MpPasteHistory>();
                //con.CreateTable<MpSource>();
                con.CreateTable<MpTag>();
                //con.CreateTable<MpUrl>();
                //con.CreateTable<MpUrlDomain>();
                //con.CreateTable<MpDbLog>();
                //con.CreateTable<MpSyncHistory>();
            }
        }

        public int ExecuteWrite(string query, Dictionary<string, object> args, string dbObjectGuid = "", string sourceClientGuid = "", object dbObject = null, bool ignoreTracking = false, bool ignoreSyncing = false) {
            MpDbLogActionType actionType = MpDbLogActionType.None;
            if(!string.IsNullOrEmpty(dbObjectGuid) && !ignoreTracking && dbObject != null && dbObject is MpISyncableDbObject) {
                //only track objects providing a guid
                actionType = MpDbLogTracker.TrackDbWrite(query, args, dbObjectGuid, sourceClientGuid, dbObject);                 
            }
            
            int numberOfRowsAffected;
            using (var con = SetConnection()) {   
                con.Open();
                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con)) {
                    if(args != null) {
                        foreach (var pair in args) {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                if (actionType != MpDbLogActionType.None && !ignoreSyncing && dbObject is MpISyncableDbObject) {
                    OnSyncableChange?.Invoke(this, dbObjectGuid);
                } else if(dbObject != null) {
                    (dbObject as MpDbModelBase).NotifyRemoteUpdate(actionType, dbObject, sourceClientGuid);
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
                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con)) {
                    if (args != null) {
                        foreach (KeyValuePair<string, object> entry in args) {
                            cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                        }
                    }
                    using (var da = new SQLiteDataAdapter(cmd)) {
                        using (var dt = new DataTable()) {
                            da.Fill(dt);
                            da.Dispose();
                            return dt;
                        }
                    }
                }
            }
        }

        public SQLite.TableMapping GetTableMapping(string tableName) {
            using (var con = SetConnectionPCL()) {
                return con.TableMappings.Where(x => x.TableName.ToLower() == tableName.ToLower()).FirstOrDefault();
            }            
        }

        public DataRow GetDbDataRowByTableGuid(string tableName, string objGuid) {            
            var dt = MpDb.Instance.Execute(
                "select * from " + tableName + " where " + tableName + "Guid='" + objGuid + "'", null);
            if (dt != null && dt.Rows.Count > 0) {
                return dt.Rows[0];
            }
            return null;
        }
        public object GetDbObjectByTableGuid(string tableName, string objGuid) {
            using (var con = SetConnectionPCL()) {
                TableMapping qtm = null;
                foreach (var tm in con.TableMappings) {
                    if (tm.TableName.ToLower() == tableName.ToLower()) {
                        qtm = tm;
                        break;
                    }
                }
                if (qtm == null) {
                    return null;
                }

                var dt = con.Query(
                    qtm,
                string.Format("select * from {0} where {1}=?", tableName, tableName + "Guid"),
                objGuid);

                if (dt != null && dt.Count > 0) {
                    return dt[0];
                }
                return null;
            }
            
        }

        public async Task<int> ExecuteWriteAsync(string query, Dictionary<string, object> args) {
            int numberOfRowsAffected;
            using (var con = SetConnection()) {
                con.Open();
                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con)) {
                    if (args != null) {
                        foreach (var pair in args) {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }
                    numberOfRowsAffected = await cmd.ExecuteNonQueryAsync();
                }
                return numberOfRowsAffected;
            }
        }
        
        public async Task<DataTable> ExecuteAsync(string query, Dictionary<string, object> args) {
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }
            using (var con = SetConnection()) {
                con.Open();
                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, con)) {
                    if (args != null) {
                        foreach (KeyValuePair<string, object> entry in args) {
                            cmd.Parameters.AddWithValue(entry.Key, entry.Value);
                        }
                    }
                    using (var da = await cmd.ExecuteReaderAsync()) {
                        //var da = new SQLiteDataAdapter(cmd);
                        using (var dt = new DataTable()) {
                            dt.Load(da);
                            //da.Fill(dt);
                            da.Dispose();
                            //await Task.Run(() => { Thread.Sleep(1); });
                            return dt;
                        }
                    }
                }
            }
        }
        
        public void WriteDatabaseToXmlFile(string out_filename) {
            System.Data.SQLite.SQLiteConnection sqc = SetConnection();
            sqc.Open();
            var sqd = new SqlDataAdapter(null, GetAllData());
            var ds = new DataSet();
            sqd.Fill(ds, "adv");
            System.Data.SQLite.SQLiteConnection sql_con = SetConnection();
            System.Data.SQLite.SQLiteCommand sql_cmd = sql_con.CreateCommand();
            ds.WriteXml(out_filename);
        }
        public int GetLastRowId(string tableName, string pkName) {
            DataTable dt = Execute("select * from " + tableName + " ORDER BY " + pkName + " DESC LIMIT 1;", null);
            if (dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0][0].ToString());
            }
            return -1;
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
                    CREATE TABLE MpSyncHistory (
                      pk_MpSyncHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , OtherClientGuid text
                    , SyncDateTime datetime
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpDbLog (
                      pk_MpDbLogId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , DbObjectGuid text
                    , DbTableName text
                    , AffectedColumnName text
                    , AffectedColumnValue text
                    , LogActionType integer default 0
                    , LogActionDateTime datetime
                    , SourceClientGuid text
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpDbImage (
                      pk_MpDbImageId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpDbImageGuid text
                    , ImageBase64 text
                    , ImageBlob image
                    );
                    ---------------------------------------------------------------------------------------------------------------------                    
                    CREATE TABLE MpTag (
                      pk_MpTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpTagGuid text
                    , fk_ParentTagId integer default 0
                    , TagName text
                    , SortIdx integer
                    , fk_MpColorId integer 
                    , CONSTRAINT FK_MpTag_0_0 FOREIGN KEY (fk_ParentTagId) REFERENCES MpTagType (pk_MpTagId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId)
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
                    , MpIconGuid text
                    , fk_IconDbImageId integer
                    , fk_IconBorderDbImageId integer
                    , fk_IconSelectedHighlightBorderDbImageId integer
                    , fk_IconHighlightBorderDbImageId integer
                    , fk_MpColorId1 integer default 0
                    , fk_MpColorId2 integer default 0
                    , fk_MpColorId3 integer default 0
                    , fk_MpColorId4 integer default 0
                    , fk_MpColorId5 integer default 0
                    , CONSTRAINT FK_MpIcon_0_0 FOREIGN KEY (fk_MpColorId1) REFERENCES MpColor (pk_MpColorId)
                    , CONSTRAINT FK_MpIcon_1_0 FOREIGN KEY (fk_MpColorId2) REFERENCES MpColor (pk_MpColorId)
                    , CONSTRAINT FK_MpIcon_2_0 FOREIGN KEY (fk_MpColorId3) REFERENCES MpColor (pk_MpColorId)
                    , CONSTRAINT FK_MpIcon_3_0 FOREIGN KEY (fk_MpColorId4) REFERENCES MpColor (pk_MpColorId)
                    , CONSTRAINT FK_MpIcon_4_0 FOREIGN KEY (fk_MpColorId5) REFERENCES MpColor (pk_MpColorId)
                    , CONSTRAINT FK_MpIcon_0_0 FOREIGN KEY (fk_IconDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)   
                    , CONSTRAINT FK_MpIcon_1_0 FOREIGN KEY (fk_IconBorderDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)                       
                    , CONSTRAINT FK_MpIcon_0_0 FOREIGN KEY (fk_IconSelectedHighlightBorderDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)   
                    , CONSTRAINT FK_MpIcon_1_0 FOREIGN KEY (fk_IconHighlightBorderDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)   
                    );
                    ---------------------------------------------------------------------------------------------------------------------                    
                    CREATE TABLE MpShortcut (
                      pk_MpShortcutId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId INTEGER DEFAULT 0
                    , fk_MpTagId INTEGER DEFAULT 0
                    , ShortcutName text NOT NULL                    
                    , KeyString text NULL       
                    , DefaultKeyString text NULL
                    , RoutingType integer NOT NULL DEFAULT 0 
                    , CONSTRAINT FK_MpShortcut_1_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpShortcutItemId)
                    , CONSTRAINT FK_MpShortcut_2_0 FOREIGN KEY (fk_MpTagId) REFERENCES MpTag (pk_MpTagId)
                    );
                    INSERT INTO MpShortcut(ShortcutName,RoutingType,KeyString,DefaultKeyString) VALUES
                    ('Show Window',2,'Control+Shift+D','Control+Shift+D')
                    ,('Hide Window',1,'Escape','Escape')
                    ,('Append Mode',2,'Control+Shift+A','Control+Shift+A')
                    ,('Auto-Copy Mode',2,'Control+Shift+C','Control+Shift+C')
                    ,('Right-Click Paste Mode',2,'Control+Shift+R','Control+Shift+R')
                    ,('Paste Selected Clip',1,'Enter','Enter')
                    ,('Delete Selected Clip',1,'Delete','Delete')
                    ,('Select Next',1,'Right','Right')
                    ,('Select Previous',1,'Left','Left')
                    ,('Select All',1,'Control+A','Control+A')
                    ,('Invert Selection',1,'Control+Shift+Alt+A','Control+Shift+Alt+A')
                    ,('Bring to front',1,'Control+Home','Control+Home')
                    ,('Send to back',1,'Control+End','Control+End')
                    ,('Assign Hotkey',1,'A','A')
                    ,('Change Color',1,'C','C')
                    ,('Say',1,'S','S')
                    ,('Merge',1,'M','M')
                    ,('Undo',1,'Control+Z','Control+Z')
                    ,('Redo',1,'Control+Y','Control+Y')
                    ,('Edit',1,'Control+E','Control+E')
                    ,('Rename',1,'F2','F2')
                    ,('Duplicate',1,'Control+D','Control+D')
                    ,('Email',1,'Control+E','Control+E')
                    ,('Qr Code',1,'Control+Shift+Q','Control+Shift+Q')
                    ,('Toggle Auto-Analyze Mode',2,'Control+Shift+B','Control+Shift+B')
                    ,('Toggle Is App Paused',2,'Control+Shift+P','Control+Shift+P')
                    ,('Copy Selection',1,'Control+C','Control+C');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpPasteToAppPath (
                      pk_MpPasteToAppPathId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , AppPath text NOT NULL
                    , AppName text
                    , Args text
                    , Label text
                    , fk_MpDbImageId integer default 0
                    , WindowState integer default 1
                    , IsSilent integer NOT NULL default 0
                    , IsAdmin integer NOT NULL default 0
                    , PressEnter integer NOT NULL default 0
                    , CONSTRAINT FK_MpPasteToAppPath_0_0 FOREIGN KEY (fk_MpDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)                    
                    );
                    INSERT INTO MpPasteToAppPath(AppPath,IsAdmin) VALUES ('%windir%\System32\cmd.exe',0);
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
                    INSERT INTO MpCopyItemType(TypeName) VALUES ('rich_text'),('image'),('file_list'),('composite');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpSortType (
                      pk_MpSortTypeId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , SortTypeName text NULL 
                    );
                    INSERT INTO MpSortType(SortTypeName) VALUES('Date'),('Application'),('Title'),('Content'),('Type'),('Usage');
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItemSortTypeOrder (
                      pk_MpCopyItemSortTypeOrderId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemSortTypeOrderGuid text
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpSortTypeId integer NOT NULL
                    , SortOrder integer NOT NULL 
                    , CONSTRAINT FK_MpCopyItemSortTypeOrder_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpCopyItemSortTypeOrder_1_0 FOREIGN KEY (fk_MpSortTypeId) REFERENCES MpSortType (pk_MpSortTypeId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpClient (
                      pk_MpClientId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpClientGuid text
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
                    ,  MpColorGuid text
                    ,  R integer not null default 255
                    ,  G integer not null default 255
                    ,  B integer not null default 255
                    ,  A integer not null default 255
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpApp (
                      pk_MpAppId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAppGuid text
                    , SourcePath text NOT NULL 
                    , AppName text 
                    , IsAppRejected integer NOT NULL   
                    , fk_MpIconId integer
                    , CONSTRAINT FK_MpApp_0_0 FOREIGN KEY (fk_MpIconId) REFERENCES MpIcon (pk_MpIconId)
                    );   
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpUrlDomain (
                      pk_MpUrlDomainId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlDomainGuid text
                    , UrlDomainPath text NOT NULL 
                    , UrlDomainTitle text
                    , IsUrlDomainRejected integer NOT NULL DEFAULT 0   
                    , fk_MpIconId integer default 0
                    , CONSTRAINT FK_MpUrlDomain_0_0 FOREIGN KEY (fk_MpIconId) REFERENCES MpIcon (pk_MpIconId)
                    );  
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpUrl (
                      pk_MpUrlId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlGuid text
                    , UrlPath text NOT NULL 
                    , UrlTitle text
                    , fk_MpUrlDomainId int NOT NULL
                    , CONSTRAINT FK_MpUrl_0_0 FOREIGN KEY (fk_MpUrlDomainId) REFERENCES MpUrlDomain (pk_MpUrlDomainId)
                    ); 
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpSource (
                      pk_MpSourceId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpSourceGuid text
                    , fk_MpUrlId integer
                    , fk_MpAppId integer NOT NULL
                    , CONSTRAINT FK_MpUrl_0_0 FOREIGN KEY (fk_MpUrlId) REFERENCES MpUrl (pk_MpUrlId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    ); 
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItem (
                      pk_MpCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemGuid text
                    , fk_MpCopyItemTypeId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer NOT NULL
                    , fk_MpUrlId integer
                    , fk_MpColorId integer
                    , Title text NULL 
                    , CopyCount integer not null default 1
                    , PasteCount integer not null default 0
                    , fk_MpDbImageId integer
                    , fk_SsMpDbImageId integer default 0
                    , ItemText text 
                    , ItemRtf text 
                    , ItemHtml text 
                    , ItemDescription text
                    , ItemCsv text
                    , CopyDateTime datetime DEFAULT (current_timestamp) NOT NULL          
                    
                    , CONSTRAINT FK_MpCopyItem_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpCopyItem_1_0 FOREIGN KEY (fk_MpClientId) REFERENCES MpClient (pk_MpClientId)
                    , CONSTRAINT FK_MpCopyItem_2_0 FOREIGN KEY (fk_MpCopyItemTypeId) REFERENCES MpCopyItemType (pk_MpCopyItemTypeId) 
                    , CONSTRAINT FK_MpCopyItem_3_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId) 
                    , CONSTRAINT FK_MpCopyItem_4_0 FOREIGN KEY (fk_MpUrlId) REFERENCES MpUrl (pk_MpUrlId) 
                    , CONSTRAINT FK_MpCopyItem_5_0 FOREIGN KEY (fk_MpDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)
                    , CONSTRAINT FK_MpCopyItem_6_0 FOREIGN KEY (fk_SsMpDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)    
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCompositeCopyItem (
                      pk_MpCompositeCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCompositeCopyItemGuid text
                    , fk_MpCopyItemId INTEGER NOT NULL
                    , fk_ParentMpCopyItemId INTEGER NOT NULL
                    , SortOrderIdx INTEGER NOT NULL DEFAULT 0
                    , CONSTRAINT FK_MpCompositeCopyItem_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpCompositeCopyItem_1_0 FOREIGN KEY (fk_ParentMpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpCopyItemTag (
                      pk_MpCopyItemTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemTagGuid text
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
                    , MpCopyItemTemplateGuid text
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpColorId integer default 2
                    , TemplateName text NOT NULL 
                    , CONSTRAINT FK_MpCopyItemTemplate_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)                    
                    , CONSTRAINT FK_MpCopyItemTemplate_0_0 FOREIGN KEY (fk_MpColorId) REFERENCES MpColor (pk_MpColorId)                    
                    );       
                    ---------------------------------------------------------------------------------------------------------------------
                    CREATE TABLE MpPasteHistory (
                      pk_MpPasteHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , SourceClientGuid text
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpClientId integer NOT NULL
                    , fk_MpAppId integer                     
                    , fk_MpUrlId integer 
                    , PasteDateTime datetime NOT NULL
                    , CONSTRAINT FK_MpPasteHistory_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpPasteHistory_1_0 FOREIGN KEY (fk_MpClientId) REFERENCES MpClient (pk_MpClientId)
                    , CONSTRAINT FK_MpPasteHistory_2_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpPasteHistory_3_0 FOREIGN KEY (fk_MpUrlId) REFERENCES MpUrl (pk_MpUrlId)
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

        #region Sync Data
        public bool IsWpf() {
            return true;
        }

        public bool IsConnectedToNetwork() {
            return MpHelpers.Instance.IsConnectedToNetwork();
        }

        public bool IsConnectedToInternet() {
            return MpHelpers.Instance.IsConnectedToInternet();
        }

        public int GetSyncPort() {
            return 44381;
        }
        public string GetThisClientGuid() {
            return Properties.Settings.Default.ThisClientGuid;
        }
        public string GetPrimaryLocalIp4Address() {
            if (!IsConnectedToNetwork()) {
                return "0.0.0.0";
            }
            return MpHelpers.Instance.GetLocalIp4Address();
        }

        public string[] GetAllLocalIp4Addresses() {
            if (!IsConnectedToNetwork()) {
                return new string[] { "0.0.0.0" };
            }
            return MonkeyPaste.MpHelpers.Instance.GetAllLocalIPv4();
        }

        public string GetExternalIp4Address() {
            if (!IsConnectedToInternet()) {
                return "0.0.0.0";
            }
            return MpHelpers.Instance.GetExternalIp4Address();
        }

        public async Task<List<MonkeyPaste.MpDbLog>> GetDbObjectLogs(string dboGuid, DateTime fromDtUtc) {
            var logs = MpDbLog.GetDbLogsByGuid(dboGuid, fromDtUtc);
            await Task.Delay(1);
            return logs;
        }

        public async Task<DateTime> GetLastSyncForRemoteDevice(string otherDeviceGuid) {
            await Task.Delay(1);
            var sh = MpSyncHistory.GetSyncHistoryByGuid(otherDeviceGuid);
            if (sh != null) {
                return sh.SyncDateTime;
            }
            return DateTime.MinValue;
        }

        public async Task<string> GetLocalLogFromSyncDate(DateTime fromDateTime, string ignoreGuid = "") {
            var logItems = MpDbLog.GetAllDbLogs().Where(x => x.LogActionDateTime > fromDateTime).ToList();
            var dbol = new List<MpISyncableDbObject>();
            foreach (var li in logItems) {
                if (li.SourceClientGuid.ToString() == ignoreGuid) {
                    continue;
                }
                dbol.Add(li as MpISyncableDbObject);
            }
            var dbMsgStr = MpDbMessage.Create(dbol);
            await Task.Delay(1);
            return dbMsgStr;
        }

        public async Task<Dictionary<Guid, List<MonkeyPaste.MpDbLog>>> PrepareRemoteLogForSyncing(string dbLogMessageStr) {
            var dbLogMessage = MpDbMessage.Parse(dbLogMessageStr, GetTypeConverter());

            var remoteDbLogs = new List<MonkeyPaste.MpDbLog>();
            var dbLogWorker = new MonkeyPaste.MpDbLog();

            //deserialize logs and put into guid buckets
            var remoteItemChangeLookup = new Dictionary<Guid, List<MonkeyPaste.MpDbLog>>();
            foreach (var remoteLogRow in dbLogMessage.DbObjects) {
                var logItem = await dbLogWorker.DeserializeDbObject(remoteLogRow.ObjStr) as MonkeyPaste.MpDbLog;
                if (remoteItemChangeLookup.ContainsKey(logItem.DbObjectGuid)) {
                    remoteItemChangeLookup[logItem.DbObjectGuid].Add(logItem);
                } else {
                    remoteItemChangeLookup.Add(logItem.DbObjectGuid, new List<MonkeyPaste.MpDbLog>() { logItem });
                }
            }

            return remoteItemChangeLookup;
        }

        public async Task PerformSync(
            Dictionary<Guid, List<MonkeyPaste.MpDbLog>> changeLookup,
            string remoteClientGuid) {
            var lastSyncDt = await MpDb.Instance.GetLastSyncForRemoteDevice(remoteClientGuid);
            //filter & separate remote logs w/ local updates after remote action dt 
            var addChanges = new Dictionary<Guid, List<MonkeyPaste.MpDbLog>>();
            var updateChanges = new Dictionary<Guid, List<MonkeyPaste.MpDbLog>>();
            var deleteChanges = new Dictionary<Guid, List<MonkeyPaste.MpDbLog>>();
            foreach (var ckvp in changeLookup) {
                if (ckvp.Value == null || ckvp.Value.Count == 0) {
                    continue;
                }
                //filter changes by > local action date time
                var rlogs = ckvp.Value;//MpDbLog.FilterOutdatedRemoteLogs(ckvp.Key.ToString(), ckvp.Value,lastSyncDt); //
                if (rlogs.Count > 0) {
                    //seperate changes into 3 types
                    foreach (var l in rlogs.OrderBy(x => x.LogActionDateTime).ToList()) {
                        switch (l.LogActionType) {
                            case MpDbLogActionType.Create:
                                if (!addChanges.ContainsKey(ckvp.Key)) {
                                    addChanges.Add(ckvp.Key, new List<MonkeyPaste.MpDbLog>() { l });
                                } else {
                                    addChanges[ckvp.Key].Add(l);
                                }
                                break;
                            case MpDbLogActionType.Modify:
                                if (!updateChanges.ContainsKey(ckvp.Key)) {
                                    updateChanges.Add(ckvp.Key, new List<MonkeyPaste.MpDbLog>() { l });
                                } else {
                                    updateChanges[ckvp.Key].Add(l);
                                }
                                break;
                            case MpDbLogActionType.Delete:
                                if (!deleteChanges.ContainsKey(ckvp.Key)) {
                                    deleteChanges.Add(ckvp.Key, new List<MonkeyPaste.MpDbLog>() { l });
                                } else {
                                    deleteChanges[ckvp.Key].Add(l);
                                }
                                break;
                        }
                    }
                }
                //ditch adds or modifies when a delete exists
                foreach (var dc in deleteChanges) {
                    if (addChanges.ContainsKey(dc.Key)) {
                        addChanges.Remove(dc.Key);
                    }
                    if (updateChanges.ContainsKey(dc.Key)) {
                        updateChanges.Remove(dc.Key);
                    }
                }

                //sort 3 types by key references
                addChanges = OrderByPrecedence(addChanges);
                deleteChanges = OrderByPrecedence(deleteChanges);
                updateChanges = OrderByPrecedence(updateChanges);
            }

            // in delete, add, update order
            foreach (var ckvp in deleteChanges) {
                var dbot = new MpWpfStringToDbObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var deleteMethod = dbot.GetMethod("DeleteFromDatabase", new Type[] { typeof(string), typeof(bool), typeof(bool) });
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                deleteMethod.Invoke(dbo, new object[] { remoteClientGuid, false, true });
            }

            foreach (var ckvp in addChanges) {
                var dbot = new MpWpfStringToDbObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var writeMethod = dbot.GetMethod("WriteToDatabase", new Type[] { typeof(string), typeof(bool), typeof(bool) });
                writeMethod.Invoke(dbo, new object[] { remoteClientGuid, false, true });
            }

            foreach (var ckvp in updateChanges) {
                var dbot = new MpWpfStringToDbObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var writeMethod = dbot.GetMethod("WriteToDatabase", new Type[] { typeof(string), typeof(bool), typeof(bool) });
                writeMethod.Invoke(dbo, new object[] { remoteClientGuid, false, true });
            }

            var newSyncHistory = new MpSyncHistory() {
                OtherClientGuid = System.Guid.Parse(remoteClientGuid),
                SyncDateTime = DateTime.UtcNow
            };
            newSyncHistory.WriteToDatabase();
        }

        private Dictionary<Guid, List<MonkeyPaste.MpDbLog>> OrderByPrecedence(Dictionary<Guid, List<MonkeyPaste.MpDbLog>> dict) {
            if (dict.Count == 0) {
                return dict;
            }
            var items = from pair in dict
                        orderby GetDbTableOrder(pair.Value[0]) ascending
                        select pair;
            var customSortedValues = new Dictionary<Guid, List<MonkeyPaste.MpDbLog>>();

            foreach (var i in items) {
                customSortedValues.Add(i.Key, i.Value);
            }
            return customSortedValues;
        }

        private int GetDbTableOrder(MonkeyPaste.MpDbLog log) {
            var orderedLogs = new List<string>() {
                          "MpColor",
                          "MpDbImage",
                          "MpIcon",
                          "MpUrl",
                          "MpUrlDomain",
                          "MpApp",
                          "MpSource",
                          "MpCompositeCopyItem",
                          "MpCopyItemTag",
                          "MpCopyItemTemplate",
                          "MpCopyItem",
                          "MpTag",
                          "MpClient" };
            var idx = orderedLogs.IndexOf(log.DbTableName);
            if (idx < 0) {
                throw new Exception(@"Unknown dblog table type: " + log.DbTableName);
            }
            return idx;
        }

        public object GetMainThreadObj() {
            return Application.Current.MainWindow;
        }

        public async Task<object> GetDbObjectByGuid(string guid) {
            var dbo = await MpDb.Instance.GetDbObjectByGuid(guid);
            return dbo;
        }

        public MpIStringToSyncObjectTypeConverter GetTypeConverter() {
            return new MpWpfStringToDbObjectTypeConverter();
        }
        public string GetDbFileAsBase64() {
            var bytes = File.ReadAllBytes(Properties.Settings.Default.DbPath);
            return Convert.ToBase64String(bytes);
        }

        public ObservableCollection<MpRemoteDevice> GetRemoteDevices() {
            _rdLock = new object();
            var rdoc = new ObservableCollection<MpRemoteDevice>();
            BindingOperations.EnableCollectionSynchronization(rdoc, _rdLock);
            return rdoc;
        }
        #endregion
    }
}
