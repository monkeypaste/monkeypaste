using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpData {
        private string _dbPath;
        private string _dbPassword;
        private MpClient _client;
        private MpUser _user;

        private ObservableCollection<MpIcon> _iconList = new ObservableCollection<MpIcon>();
        private ObservableCollection<MpApp> _appList = new ObservableCollection<MpApp>();
        private ObservableCollection<MpCopyItem> _copyItemList = new ObservableCollection<MpCopyItem>();

        public MpData(string dbPath,string dbPassword,string identityToken,string accessToken) {
            _dbPath = dbPath;
            _dbPassword = dbPassword;
            InitUser(identityToken);
            InitClient(accessToken);
        }
        public void Init() {            
            InitDb();
            InitMpIcon();
            InitMpApp();
            InitMpCopyItem();
        }
        public void SetDbPassword(string newPassword) {
            if(_dbPassword != newPassword) {
                _dbPassword = newPassword;
                // if db is unpassword protected
                if(_dbPassword == null || _dbPassword == String.Empty) {
                    SQLiteConnection conn = new SQLiteConnection("Data Source=" + _dbPath + ";Version=3;");
                    conn.SetPassword(newPassword);
                    conn.Open();
                    conn.Close();
                } else {
                    SQLiteConnection conn = SetConnection();
                    conn.Open();
                    conn.ChangePassword(newPassword);
                }                
            }
        }
        public string GetDbPassword() {
            return _dbPassword;
        }
        public void AddOnDataListChangeListener(MpCopyItemTileChooserPanelController lf) {
            //_mpIconList.CollectionChanged += lf.CopyItemCollection_CollectionChanged;
            //_mpAppList.CollectionChanged += lf.CopyItemCollection_CollectionChanged;
            _copyItemList.CollectionChanged += lf.CopyItemCollection_CollectionChanged;
        }
        public MpClient GetMpClient() {
            return _client;
        }
        private void InitUser(string idToken) {
            _user = new MpUser() { IdentityToken = idToken };
        }
        private void InitClient(string accessToken) {
            _client = new MpClient() { AccessToken = accessToken };
        }
        private void InitDb() {
            if(!Directory.Exists(Path.GetDirectoryName(_dbPath))) {
                Console.WriteLine(_dbPath + " does not exist, creating...");
                Directory.CreateDirectory(Path.GetDirectoryName(_dbPath));
                Console.WriteLine("Successfully created directory" + Path.GetDirectoryName(_dbPath));
            }
            //File.Delete(_dbPath);
            if(!File.Exists(_dbPath)) {
                Console.WriteLine("Database does not exist at " + _dbPath + " creating a new file...");
                SQLiteConnection.CreateFile(_dbPath);
                ExecuteNonQuery(GetCreateString());
            }
            Console.WriteLine("Database successfully initialized at " + _dbPath);
        }
        private void InitMpIcon() {
            _iconList = new ObservableCollection<MpIcon>();

            DataTable dt = Execute("select * from MpIcon");
            foreach(DataRow dr in dt.Rows) {
                _iconList.Add(new MpIcon(dr));
            }
            Console.WriteLine("Init w/ " + _iconList.Count + " icons.");
        }
        public void InitMpApp() {
            _appList = new ObservableCollection<MpApp>();

            DataTable dt = Execute("select * from MpApp");
            foreach(DataRow dr in dt.Rows) {
                _appList.Add(new MpApp(dr));
            }
            Console.WriteLine("Init w/ " + _appList.Count + " apps.");
        }
        public void InitMpCopyItem() {
            _copyItemList = new ObservableCollection<MpCopyItem>();

            DataTable dt = Execute("select * from MpCopyItem");
            foreach(DataRow dr in dt.Rows) {
                _copyItemList.Add(new MpCopyItem(dr));
            }
            
            Console.WriteLine("Init w/ "+_copyItemList.Count + " copyitems added");
        }
       public string GetCreateDbFilePath() {
            return @"C:\Users\tkefauver\Dropbox\Dev\MonkeyPaste\MonkeyPaste\db\CreateMpDb.sql";
        }
        public void ResetDb() {
            ExecuteNonQuery(GetDropString());
            ExecuteNonQuery(GetCreateString());
            Init();
        }
        public void DeleteDb() {
            //ExecuteNonQuery(GetDropString());
            //ExecuteNonQuery(GetCreateString());
            File.Delete(_dbPath);
            Init();
        }
        public void AddMpIcon(IntPtr sourceHandle) {
            int lastId = 0;
            if(_iconList.Count > 0) {
                lastId = _iconList[_iconList.Count - 1].iconId;
            }
            MpIcon newMpIcon = new MpIcon(0,sourceHandle);
            if(newMpIcon.iconId > lastId) {
                _iconList.Add(newMpIcon);
            }
        }
        public void AddMpIcon(MpIcon newIcon) {
            foreach(MpIcon i in _iconList) {
                if(i.iconId == newIcon.iconId) {
                    _iconList[_iconList.IndexOf(i)] = newIcon;
                    return;
                }
            }
            _iconList.Add(newIcon);
        }
        public void AddMpApp(IntPtr sourceHandle) {
            int lastId = 0;
            if(_appList.Count > 0) {
                lastId = _appList[_appList.Count - 1].appId;
            }
            MpApp newMpApp = new MpApp(0,0,sourceHandle,false);
            if(newMpApp.appId > lastId) {
                _appList.Add(newMpApp);
            }
        }
        public void AddMpApp(MpApp newApp) {
            foreach(MpApp a in _appList) {
                if(a.appId == newApp.appId) {
                    _appList[_appList.IndexOf(a)] = newApp;
                    return;
                }
            }
            _appList.Add(newApp);
        }
        public void AddMpCopyItem(IDataObject iData,IntPtr sourceHandle) {
            MpCopyItem ci = null;
           if(iData.GetDataPresent(DataFormats.Bitmap)) {
                ci = new MpCopyItem((Image)iData.GetData(DataFormats.Bitmap,true),sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.FileDrop)) {
                ci = new MpCopyItem((string[])iData.GetData(DataFormats.FileDrop,true),sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.Rtf)) {
                ci = new MpCopyItem((string)iData.GetData(DataFormats.Rtf),MpCopyItemType.RichText,sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.Html)) {
                ci = new MpCopyItem((string)iData.GetData(DataFormats.Html),MpCopyItemType.HTMLText,sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.Text)) {
                ci = new MpCopyItem((string)iData.GetData(DataFormats.Text),MpCopyItemType.Text,sourceHandle);
            }
            else {
                Console.WriteLine("MpData error clipboard data is not known format");
                return;
            }
            foreach(MpCopyItem mpci in _copyItemList) {
                if(ci.copyItemId == mpci.copyItemId)
                    return;
            }
            _copyItemList.Add(ci);
        }
        public void UpdateMpIcon(MpIcon updatedMpIcon) {
            for(int i = 0;i < _iconList.Count;i++) {
                if(_iconList[i].iconId == updatedMpIcon.iconId) {
                    _iconList[i] = updatedMpIcon;
                    break;
                }
            }
        }
        public void UpdateMpApp(MpApp updatedMpApp) {
            for(int i = 0;i < _appList.Count;i++) {
                if(_appList[i].appId == updatedMpApp.appId) {
                    _appList[i] = updatedMpApp;
                    break;
                }
            }
        }
        public void UpdateMpCopyItem(MpCopyItem updatedMpCopyItem) {
            for(int i = 0;i < _copyItemList.Count;i++) {
                if(_copyItemList[i].copyItemId == updatedMpCopyItem.copyItemId) {
                    _copyItemList[i] = updatedMpCopyItem;
                    break;
                }
            }
        }
        public MpIcon GetMpIcon(int MpIconId) {
            foreach(MpIcon mpi in _iconList) {
                if(mpi.iconId == MpIconId) {
                    return mpi;
                }
            }
            return null;
        }
        public MpIcon GetMpIcon(Image iconImage) {
            foreach(MpIcon mpi in _iconList) {
                if(mpi.IconImage == iconImage) {
                    return mpi;
                }
            }
            return null;
        }
        public MpApp GetMpApp(int MpAppId) {
            foreach(MpApp mpa in _appList) {
                if(mpa.appId == MpAppId) {
                    return mpa;
                }
            }
            return null;
        }
        public MpApp GetMpApp(string path) {
            foreach(MpApp mpa in _appList) {
                if(mpa.SourcePath == path) {
                    return mpa;
                }
            }
            return null;
        }
        public MpCopyItem[] GetMpCopyItemList() {
            MpCopyItem[] tempList = new MpCopyItem[_copyItemList.Count];
            _copyItemList.CopyTo(tempList,0);
            return tempList;
        }
        public MpCopyItem GetMpCopyItem(int MpCopyItemId) {
            foreach(MpCopyItem mpci in _copyItemList) {
                if(mpci.copyItemId == MpCopyItemId) {
                    return mpci;
                }
            }
            return null;
        }
        
        private SQLiteConnection SetConnection() {
            // see https://stackoverflow.com/questions/1381264/password-protect-a-sqlite-db-is-it-possible
            // about passwords
            string connStr = "Data Source=" + _dbPath + ";Version=3;";
            if(_dbPassword != null && _dbPassword != String.Empty) {
                connStr += "Password=" + _dbPassword+";";
            }
            Console.WriteLine("Connection String: " + connStr);
            SQLiteConnection conn = null;
            while(conn == null) {
                try {
                    conn = new SQLiteConnection(connStr);
                }
                catch(Exception e) {
                    conn = null;
                    MpEnterDbPasswordForm enterPasswordForm = new MpEnterDbPasswordForm();
                    DialogResult enterPasswordResult = enterPasswordForm.ShowDialog();
                    connStr = "Data Source=" + _dbPath + ";Version=3;";
                    if(_dbPassword != null && _dbPassword != String.Empty) {
                        connStr += "Password=" + _dbPassword + ";";
                    }
                    conn = new SQLiteConnection(connStr);
                }
            }
            return conn;
        }
        public void ExecuteNonQuery(string sql,List<string> paramList = null, List<object> paramValueList = null) {
            if((paramList != null && paramValueList != null) && (paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count)) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return;
            }
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
            catch(Exception ex) {
                Console.WriteLine("Error in executenonquery w/ parameter list: " + ex.ToString());
            }
            sql_con.Close();
        }
        public DataTable Execute(string sql,List<string> paramList = null,List<object> paramValueList = null) {
            if((paramList != null && paramValueList != null) && (paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count)) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return null;
            }
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
            SQLiteDataAdapter DB = new SQLiteDataAdapter(sql_cmd);
            DataSet DS = new DataSet();
            DS.Reset();
            DB.Fill(DS);
            DataTable DT = new DataTable();
            DT = DS.Tables[0];
            sql_con.Close();
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
            /*return @"
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpDeviceType (SERVER)                                                                                               |
                -------------------------------------------------------------------------------------------------------------------------
               --DeviceType is iphone,ipad,android,tablet,pc,mac
                create table if not exists MpDeviceType(
                pk_MpDeviceTypeId integer not null primary key autoincrement, 
                DeviceTypeName varchar(15)
                );
                insert into MpDeviceType(DeviceTypeName) values('windows'),('mac'),('android'),('iphone'),('ipad'),('tablet');
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpPlatformType (SERVER)                                                                                             |
                -------------------------------------------------------------------------------------------------------------------------
                --patformtype: ios,android,windows,macMpClientId
                create table if not exists MpPlatformType(
                pk_MpPlatformTypeId integer not null primary key autoincrement, 
                PlatformName varchar(30) not null
                );
                insert into MpPlatformType(PlatformName) values('ios'),('android'),('windows'),('mac');
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpPlatform (SERVER)                                                                                                 |
                -------------------------------------------------------------------------------------------------------------------------
                --platform windows(type) version 7,9,10,etc.
                create table if not exists MpPlatform(
                pk_MpPlatformId integer not null primary key autoincrement,
                fk_MpPlatformTypeId integer not null,
                fk_MpDeviceTypeId integer not null,
                Version varchar(30),
                constraint fk_MpPlatform_MpPlatformType foreign key(fk_MpPlatformTypeId) references MpPlatformType(pk_MpPlatformTypeId),
                constraint fk_MpPlatform_MpDeviceType foreign key(fk_MpDeviceTypeId) references MpDeviceType(pk_MpDeviceTypeId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpClient (CLIENT)                                                                                                   |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpClient(
                pk_MpClientId integer not null primary key autoincrement,
                fk_MpPlatformId integer not null,
                Ip4Address varchar(30),
                AccessToken varchar(255),
                LoginDateTime datetime not null,
                LogoutDateTime datetime,
                constraint fk_MpClient_MpPlatform foreign key(fk_MpPlatformId) references MpPlatform(pk_MpPlatformId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpCopyItemType (CLIENT)                                                                                             |
                -------------------------------------------------------------------------------------------------------------------------
                --should be text, rich text, html, image, file list
                create table if not exists MpCopyItemType(
                pk_MpCopyItemTypeId integer not null primary key autoincrement,
                TypeName varchar(20)
                );
                insert into MpCopyItemType(TypeName) values('text'),('rich_text'),('html_text'),('image'),('file_list');
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpIcon (CLIENT)                                                                                                     |
                -------------------------------------------------------------------------------------------------------------------------
                --represents an application on users machine calling copy command(Urlpath may store webbrowser tab at some pointeger)
                create table if not exists MpIcon(
                pk_MpIconId integer not null primary key autoincrement,
                IconBlob blob not null
                );
                -------------------------------------------------------------------------------------------------------------------------
                ----|   MpApp (CLIENT)                                                                                                      |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpApp(
                pk_MpAppId integer not null primary key autoincrement,
                fk_MpIconId integer not null,
                SourcePath varchar(255) not null,
                IsAppRejected integer not null,
                constraint fk_MpApp_MpIcon foreign key(fk_MpIconId) references MpIcon(pk_MpIconId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpCopyItem (CLIENT)                                                                                                 --|
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpCopyItem(
                pk_MpCopyItemId integer not null primary key autoincrement,
                fk_MpCopyItemTypeId integer not null,
                fk_MpClientId integer not null,
                fk_MpAppId integer not null,
                Title varchar,
                CopyDateTime datetime not null default current_timestamp,
                constraint fk_MpCopyItem_MpCopyItemType foreign key(fk_MpCopyItemTypeId) references MpCopyItemType(pk_MpCopyItemTypeId),
                constraint fk_MpCopyItem_MpClient foreign key(fk_MpClientId) references MpClient(pk_MpClientId),
                constraint fk_MpCopyItem_MpApp foreign key(fk_MpAppId) references MpApp(pk_MpAppId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpTextItem (CLIENT)                                                                                                 |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpTextItem(
                pk_MpTextItemId integer not null primary key autoincrement,
                fk_MpCopyItemId integer not null,
                ItemText varchar not null,
                constraint fk_MpTextItem_MpCopyItem foreign key(fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpImageItem (CLIENT)                                                                                                |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpImageItem(
                pk_MpImageItemId integer not null primary key autoincrement,
                fk_MpCopyItemId integer not null,
                ItemImage longblob not null,
                constraint fk_MpImageItem_MpCopyItem foreign key(fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpFileDropListItemType (CLIENT)                                                                                     |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpFileDropListItem(
                pk_MpFileDropListItemId integer not null primary key autoincrement,
                fk_MpCopyItemId integer not null,
                constraint fk_MpFileDropListItem_MpCopyItem foreign key(fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpFileDropListSubItemType (CLIENT)                                                                                  |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpFileDropListSubItem(
                pk_MpFileDropListSubItemId integer not null primary key autoincrement,
                fk_MpFileDropListItemId integer not null,
                ItemPath varchar(255) not null,
                constraint fk_MpFileDropListSubItem_MpFileDropListItem foreign key(fk_MpFileDropListItemId) references MpFileDropListItem(pk_MpFileDropListItemId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpPasteHistory (CLIENT)                                                                                             |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpPasteHistory(
                pk_MpPasteHistoryId integer not null primary key autoincrement,
                fk_MpCopyItemId integer not null,
                fk_MpClientId integer not null,
                fk_MpAppId integer not null,
                PasteDateTime datetime not null,
                constraint fk_MpPasteHistory_MpCopyItem foreign key(fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId),
                constraint fk_MpPasteHistory_MpClient foreign key(fk_MpClientId) references MpClient(pk_MpClientId),
                constraint fk_MpPasteHistory_MpApp foreign key(fk_MpAppId) references MpApp(pk_MpAppId)
                );
                -------------------------------------------------------------------------------------------------------------------------
                --|   MpSubTextToken (CLIENT)                                                                                          |
                -------------------------------------------------------------------------------------------------------------------------
                create table if not exists MpSubTextToken(
                pk_MpSubTextTokenId integer not null primary key autoincrement,
                fk_MpCopyItemId integer not null,
                fk_MpCopyItemTypeId integer not null,
                StartIdx integer not null,
                EndIdx integer not null,
                InstanceIdx int not null,
                constraint fk_MpSubTextToken_MpCopyItem foreign key(fk_MpCopyItemId) references MpCopyItem(pk_MpCopyItemId),
                constraint fk_MpSubTextToken_MpCopyItemType foreign key(fk_MpCopyItemTypeId) references MpCopyItemType(pk_MpCopyItemTypeId)
                );
                ";*/
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
