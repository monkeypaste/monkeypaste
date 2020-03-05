using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItem:MpDBObject {
        public static int TotalCopyItemCount = 0;

        public List<MpSubTextToken> subTextTokenList = new List<MpSubTextToken>();

        private Object DataObject { get; set; }
        
        public int ColorId { get; set; }
        public int CopyItemId { get; set; }
        public int SubItemId { get; set; }
        public string Title { get; set; }
        public MpCopyItemType CopyItemType { get; set; }
        public int ClientId { get; set; }
        public int AppId { get; set; }
        public int IconId { get; set; }
        public DateTime CopyDateTime { get; set; }
        public int CopyCount { get; set; }
        public string SourcePath { get; set; }
        //private IntPtr sourceHandle;

        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }

        public MpCopyItem() {}

        public static MpCopyItem CreateCopyItem(MpCopyItemType itemType,object data,string sourcePath,Color tileColor) { 
            MpCopyItem newItem = new MpCopyItem();
            newItem.ItemColor = new MpColor(tileColor.R,tileColor.G,tileColor.B,255);
            newItem.SourcePath = sourcePath;
            newItem.CopyItemType = itemType;
            if(newItem.CopyItemType == MpCopyItemType.RichText) {
                RichTextBox rtb = new RichTextBox() { Rtf = (string)data };
                newItem.DataObject = rtb.Text;
                newItem.CopyItemType = MpCopyItemType.Text;
            }
            else if(newItem.CopyItemType == MpCopyItemType.HTMLText) {
                string dataStr = (string)data;
                int idx0 = dataStr.IndexOf("<html>") < 0 ? 0 : dataStr.IndexOf("<html>");
                int idx1 = dataStr.IndexOf("/<html>") < 0 ? dataStr.Length - 1 : dataStr.IndexOf("/<html>");
                dataStr = dataStr.Substring(idx0,idx1 - idx0);
                dataStr.Insert(dataStr.IndexOf("<html>") + 4," style='border:none;'>");
                WebBrowser _wb = new WebBrowser() {
                    AutoSize = true,
                    DocumentText = dataStr
                };
                MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
                ((WebBrowser)_wb).Document.ExecCommand("SelectAll",false,null);
                ((WebBrowser)_wb).Document.ExecCommand("Copy",false,null);
                MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
                TextBox temp = new TextBox();
                temp.Paste();
                newItem.DataObject = (string)temp.Text;
                ((WebBrowser)_wb).Document.ExecCommand("UNSELECT",false,Type.Missing);
                newItem.CopyItemType = MpCopyItemType.Text;
            }
            else if(newItem.CopyItemType == MpCopyItemType.Text) {
                newItem.DataObject = (string)data;
            }
            else {
                newItem.DataObject = data;
            }
            if(newItem.CopyItemType == MpCopyItemType.Text) {
                DataTable dt = MpLogFormController.Db.Execute("select * from MpTextItem where ItemText=@1",new List<string>(){ "@1"},new List<object>() { (string)newItem.DataObject });
                if(dt != null && dt.Rows.Count > 0) {
                    int cid = Convert.ToInt32(dt.Rows[0]["fk_MpCopyItemId"].ToString());
                    dt = MpLogFormController.Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + cid);
                    int cc = Convert.ToInt32(dt.Rows[0]["CopyCount"].ToString()) + 1;
                    Console.WriteLine("MpCopyItem: ignoring duplicate");
                    MpLogFormController.Db.ExecuteNonQuery("update MpCopyItem set CopyCount="+cc+" where pk_MpCopyItemId="+cid);
                    return null;
                }
            }
            //newItem.CopyItemId = itemId;
            newItem.CopyDateTime = DateTime.Now;
            //newItem.sourceHandle = sourceHandle;
            newItem.Title = Enum.GetName(typeof(MpCopyItemType),newItem.CopyItemId).ToString();
            newItem.CopyCount = 1;
            newItem.App = new MpApp(sourcePath,false);
            newItem.AppId = newItem.App.appId;
            newItem.Client = new MpClient(0,0,MpHelperSingleton.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(),"unknown",DateTime.Now);
            
            return newItem;
        }
        public MpCopyItem(int copyItemId) {
            DataTable dt = MpLogFormController.Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + copyItemId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
       
        public void SetData(object newData) {
            DataObject = newData;
            WriteToDatabase();
        }
        public object GetData() {
            switch(this.CopyItemType) {
                //case MpCopyItemType.PhoneNumber:
                //case MpCopyItemType.StreetAddress:
                //case MpCopyItemType.WebLink:
                //case MpCopyItemType.Email:
                case MpCopyItemType.Text:
                case MpCopyItemType.RichText:
                case MpCopyItemType.HTMLText:
                    //case MpCopyItemType.RichText:
                    return ((string)DataObject).Replace("''","'");
                //case MpCopyItemType.HTMLText:
                //    return ((string)DataObject).Trim();
                //case MpCopyItemType.RichText:
                //    TextBox rtb = new TextBox() { Rtf = (string)this.DataObject };
                   // DataObject = (object)rtb.Text;
                 //   return rtb.Text.Trim();
                case MpCopyItemType.FileList:
                    if(DataObject.GetType() == typeof(string[])) {
                        string fileListStr = "";
                        foreach(string fileOrPathStr in (string[])DataObject) {
                            fileListStr += fileOrPathStr + Environment.NewLine;
                        }
                        return fileListStr;
                    }
                    return new string[1] { "Misreferenced" };
                case MpCopyItemType.Image:
                    return DataObject;           
            }
            return "Error unknown copyitem format!";
        }

        public MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            this.CopyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            this.CopyItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            this.ClientId = Convert.ToInt32(dr["fk_MpClientId"].ToString());
            this.AppId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            this.ColorId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            this.CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            this.Title = dr["Title"].ToString().Replace("''","'");
            this.CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());

            //get app and icon obj
            DataTable dt = MpLogFormController.Db.Execute("select * from MpApp where pk_MpAppId=" + AppId);
            if(dt != null && dt.Rows.Count > 0) {
                this.App = new MpApp(dt.Rows[0]);
            } else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + AppId);
            }

            //get color
            dt = MpLogFormController.Db.Execute("select * from MpColor where pk_MpColorId=" + ColorId);
            if(dt != null && dt.Rows.Count > 0) {
                this.ItemColor = new MpColor(dt.Rows[0]);
            }
            else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + AppId);
            }

            DataTable copyItemData = null;
            switch(this.CopyItemType) {
                //case MpCopyItemType.PhoneNumber:
                //case MpCopyItemType.StreetAddress:
                //case MpCopyItemType.WebLink:
                //case MpCopyItemType.Email:
                case MpCopyItemType.Text:
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                    copyItemData = MpLogFormController.Db.Execute("select * from MpTextItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpTextItem " + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpTextItemId"].ToString());
                    this.DataObject = copyItemData.Rows[0]["ItemText"].ToString();
                    break;
                case MpCopyItemType.FileList:
                    copyItemData = MpLogFormController.Db.Execute("select * from MpFileDropListItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpFileDropListItem " + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpFileDropListItemId"].ToString());
                    copyItemData = MpLogFormController.Db.Execute("select * from MpFileDropListSubItem where fk_MpFileDropListItemId=" + this.SubItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpFileDropListSubItem for  MpFileDropListItemId=" + this.CopyItemId);
                        break;
                    }
                    this.DataObject = new string[copyItemData.Rows.Count];
                    for(int i = 0;i < copyItemData.Rows.Count;i++) {
                        string fileOrPath = copyItemData.Rows[i]["ItemPath"].ToString();
                        ((string[])this.DataObject)[i] = fileOrPath;
                    }
                    break;
                case MpCopyItemType.Image:
                    copyItemData = MpLogFormController.Db.Execute("select * from MpImageItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpImageItem for Id=" + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpImageItemId"].ToString());
                    this.DataObject = (byte[])copyItemData.Rows[0]["ItemImage"];
                    break;
            }
            MapDataToColumns();
            Console.WriteLine("Loaded MpCopyItem");
            Console.WriteLine(ToString());
        }
        public void DeleteFromDatabase() {
            if(CopyItemId <= 0) {
                return;
            }
            switch(CopyItemType) {
                case MpCopyItemType.Text:
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpTextItem where fk_MpCopyItemId=" + CopyItemId);
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    break;
                case MpCopyItemType.FileList:
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpFileDropListSubItem where fk_MpFileDropListItemId=" + SubItemId);
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpFileDropListItem where pk_MpFileDropListItemId=" + SubItemId);
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    break;
                case MpCopyItemType.Image:
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpImageItem where fk_MpCopyItemId=" + CopyItemId);
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    MpLogFormController.Db.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    break;
            }
        }
        // still req'd if NoDb=true
        public override void WriteToDatabase() {            
            bool isNew = false;
            if(App == null) {
                App = new MpApp(SourcePath,false);
                this.AppId = App.appId;
            }
            if(this.AppId == 0) {
                DataTable dt = MpLogFormController.Db.Execute("select * from MpApp where pk_MpAppId=" + AppId);
                if(dt != null && dt.Rows.Count > 0) {
                    this.App = new MpApp(dt.Rows[0]);
                    this.App.appId = 0;
                }
                else {
                    //this case occur
                    //Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + appId);
                }
            }
            App.WriteToDatabase();
            this.AppId = this.App.appId;
            
            if(ItemColor == null) {
                throw new Exception("CopyItem exception writing without color created");
            } else {
                ItemColor.WriteToDatabase();
            }
            //if copyitem already exists
            if(this.CopyItemId > 0) {
                DataTable dt = MpLogFormController.Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + this.CopyItemId);
                if(dt.Rows.Count > 0) {
                    MpLogFormController.Db.ExecuteNonQuery("update MpCopyItem set fk_MpCopyItemTypeId=" + (int)this.CopyItemType + ", fk_MpClientId=" + this.ClientId + ", fk_MpAppId=" + this.AppId + ", Title='"+this.Title.Replace("'","''")+"', CopyCount="+this.CopyCount+" where pk_MpCopyItemId=" + this.CopyItemId);
                }
                else {
                    Console.WriteLine("MpCopyItem error cannot find pk of existing item");
                    return;
                }
                isNew = false;
            }
            else {
                ++TotalCopyItemCount;
                if(MpLogFormController.Db.NoDb) {                    
                    CopyItemId = TotalCopyItemCount;
                    MapDataToColumns();
                    return;
                }
                MpLogFormController.Db.ExecuteNonQuery("insert into MpCopyItem(fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,Title,CopyDateTime,CopyCount) values (" + (int)this.CopyItemType + "," + MpLogFormController.Db.Client.ClientId + "," + this.AppId + ",'"+this.Title+"','" + this.CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "',"+this.CopyCount+");");
                this.CopyItemId = MpLogFormController.Db.GetLastRowId("MpCopyItem","pk_MpCopyItemId");
                isNew = true;
            }

            switch(this.CopyItemType) {
                case MpCopyItemType.RichText:
                case MpCopyItemType.HTMLText:
                //case MpCopyItemType.PhoneNumber:
                //case MpCopyItemType.StreetAddress:
                //case MpCopyItemType.WebLink:
                //case MpCopyItemType.Email:
                case MpCopyItemType.Text:
                    if(isNew) {
                        MpLogFormController.Db.ExecuteNonQuery("insert into MpTextItem(fk_MpCopyItemId,ItemText) values (" + this.CopyItemId + ",@1)",new List<string>() { "@1" },new List<object>() { ((string)this.DataObject).Replace("'","''") });
                        this.SubItemId = MpLogFormController.Db.GetLastRowId("MpTextItem","pk_MpTextItemId");
                    } else {
                        MpLogFormController.Db.ExecuteNonQuery("update MpTextItem set ItemText='" + ((string)GetData()).Replace("'","''") + "' where pk_MpTextItemId=" + this.SubItemId);
                    }                    
                    break;
                case MpCopyItemType.FileList:
                    if(isNew) {
                        MpLogFormController.Db.ExecuteNonQuery("insert into MpFileDropListItem(fk_MpCopyItemId) values(" + this.CopyItemId + ")");
                        this.SubItemId = MpLogFormController.Db.GetLastRowId("MpFileDropListItem","pk_MpFileDropListItemId");
                        foreach(string fileOrPath in (string[])this.DataObject) {
                            MpLogFormController.Db.ExecuteNonQuery("insert into MpFileDropListSubItem(fk_MpFileDropListItemId,ItemPath) values (" + this.SubItemId + ",'" + fileOrPath + "')");
                        }
                    } else {
                        //file lists are not editable
                    }                    
                    break;
                case MpCopyItemType.Image:
                    if(isNew) {
                        MpLogFormController.Db.ExecuteNonQuery("insert into MpImageItem(fk_MpCopyItemId,ItemImage) values (" + this.CopyItemId + ",@0)",new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ConvertImageToByteArray((Image)this.DataObject) });
                    } else {
                        MpLogFormController.Db.ExecuteNonQuery("update MpImageItem set ItemImage=@0 where pk_MpImageItemId="+this.SubItemId,new List<string>() { "@0" },new List<object>() { this.DataObject });
                    }
                    
                    break;
            }
            MapDataToColumns();
            Console.WriteLine(isNew ? "Created ":"Updated " + " MpCopyItem");
            Console.WriteLine(ToString());
        }
        private void MapDataToColumns() {
            tableName = "MpCopyItem";
            columnData.Clear();
            columnData.Add("pk_MpCopyItemId",this.CopyItemId);
            columnData.Add("fk_MpCopyItemTypeId",this.CopyItemType);
            columnData.Add("fk_MpClientId",this.ClientId);
            columnData.Add("fk_MpAppId",this.AppId);
            columnData.Add("CopyDateTime",this.CopyDateTime);
            columnData.Add("SubItemId",this.SubItemId);
            columnData.Add("Title",this.Title);
            columnData.Add("DataObject",this.DataObject);
            columnData.Add("CopyCount",this.CopyCount);
        }
    }

    public enum MpCopyItemType {
        None=0,
        Text,
        RichText,
        HTMLText,
        WebLink,
        Image,
        FileList,
        StreetAddress,
        Email,
        PhoneNumber
    }

    public class MpSubTextToken {
        public string Token { get; set; }
        public MpCopyItemType TokenType { get; set; }
        public int StartIdx { get; set; }
        public int EndIdx { get; set; }
        public int InstanceCount { get; set; }
        public int InstanceId { get; set; }

        public MpSubTextToken(string token,MpCopyItemType mpType,int s,int e,int ic,int iid) {
            this.Token = token;
            this.TokenType = mpType;
            this.StartIdx = s;
            this.EndIdx = e;
            this.InstanceCount = ic;
            this.InstanceId = iid;
        }
    }

}
