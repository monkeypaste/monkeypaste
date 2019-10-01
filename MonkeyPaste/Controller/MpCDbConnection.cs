 using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace MonkeyPaste {
    public sealed class MpCDbConnection {
        //private static readonly Lazy<MpCDbConnection> lazy = new Lazy<MpCDbConnection>(() => new MpCDbConnection());
        //public static MpCDbConnection Instance { get { return lazy.Value; } }
        
        private readonly string dbName = "mp.db";
        private MpClient _client = null;

        public MpCDbConnection(string dbFilePath,string dbName) {
            
        }
        public bool TestCDb() {
            return Execute("SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{MpCopyItem}';").Rows.Count > 0;
        }
        public void CreateCDb() {
            //test if tables exist

            if(!TestCDb()) {
                ExecuteNonQuery(GetDropString());
            }
            ExecuteNonQuery(GetCreateString());
        }
        /*public void ResetCDb(bool softReset = false) {
            ExecuteNonQuery(softReset ? GetClearString() : GetDropString());
            ExecuteNonQuery(GetCreateString());

            MessageBox.Show("Local DB successfully " + (softReset ? "reset":"created"));
        }*/
        public MpClient GetClient() {
            if(_client != null) {
                //Console.WriteLine("Client already created ignoring creation");
                return _client;
            }
            ExecuteNonQuery("insert into MpClient(fk_MpPlatformId,Ip4Address,LoginDateTime) values(3,'" + MpHelperFunctions.Instance.GetCurrentIPAddress().MapToIPv4().ToString() + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "');");
            DataTable dt = Execute("select * from MpClient ORDER BY pk_MpClientId DESC LIMIT 1;");

            if(dt.Rows.Count > 0) {
                _client = new MpClient() {
                    MpClientId = Convert.ToInt32(dt.Rows[0][0].ToString()),
                    MpPlatformId = Convert.ToInt32(dt.Rows[0][1].ToString()),
                    Ip4Address = dt.Rows[0][2].ToString(),
                    LoginDateTime = Convert.ToDateTime(dt.Rows[0][3].ToString())
                };
            }
            return _client;
        }
        public void SetClient(MpClient newMpClient) {
            _client = newMpClient;
        }

        private SQLiteConnection SetConnection() {
            return new SQLiteConnection("Data Source=" + MpSingletonController.Instance.GetMpRegistryController().GetDBCPath() + "\\" + dbName);
        }
        // insert / update / delete
        public void ExecuteNonQuery(string sql,List<string> paramList = null,List<object> paramValueList = null) {
            paramList = paramList == null ? new List<string>() : paramList;
            paramValueList = paramValueList == null ? new List<object>() : paramValueList;
            if(paramList.Count > 0 && paramValueList.Count > 0 && paramList.Count != paramValueList.Count) {
                Console.WriteLine("Param error! Param count: " + paramList.Count + " val count: " + paramValueList.Count);
                return;
            }
            SQLiteConnection sql_con = SetConnection();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = String.Format(sql);
            for(int i = 0;i < paramList.Count;i++) {
                // check p to conditionally set parameter type
                SQLiteParameter param = new SQLiteParameter(paramList[i],DbType.Binary);
                param.Value = (byte[])paramValueList[i];
                sql_cmd.Parameters.Add(param);
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
        /*public void InsertMpApp(string path,Image iconImage) {
            SQLiteConnection sql_con = SetConnection();
            sql_con.Open();
            SQLiteCommand sql_cmd = sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = "insert into MpImageItem(fk_MpCopyItemId,ItemImage) values (" + copyItemId + ",@img)";
            sql_cmd.Prepare();
            sql_cmd.Parameters.Add("@img",DbType.Binary,data.Length);
            sql_cmd.Parameters["@img"].Value = data;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }
        public void InsertMpImageItem(int copyItemId,byte[] data) {
            SQLiteConnection sql_con = SetConnection();
            sql_con.Open();
            SQLiteCommand sql_cmd = sql_cmd = sql_con.CreateCommand();
            sql_cmd.CommandText = "insert into MpImageItem(fk_MpCopyItemId,ItemImage) values ("+copyItemId+",@img)";
            sql_cmd.Prepare();
            sql_cmd.Parameters.Add("@img",DbType.Binary,data.Length);
            sql_cmd.Parameters["@img"].Value = data;
            sql_cmd.ExecuteNonQuery();
            sql_con.Close();
        }*/
        // select
        public DataTable Execute(string sql) {
            SQLiteConnection sql_con = SetConnection();
            sql_con.Open();
            SQLiteCommand sql_cmd = sql_con.CreateCommand();
            SQLiteDataAdapter DB = new SQLiteDataAdapter(sql,sql_con);
            DataSet DS = new DataSet();
            DS.Reset();
            DB.Fill(DS);
            DataTable DT = new DataTable();
            DT = DS.Tables[0];
            sql_con.Close();
            return DT;
        }
        public int GetLastRowId(string tableName,string pkName) {
            DataTable dt = Execute("select * from "+tableName+" ORDER BY "+pkName+" DESC LIMIT 1;");
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
                ItemText longtext not null,
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
            ";
        }
    }
}
