using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpCopyItem:MpDbObject {
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

        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }

        public MpCopyItem() {}

        public static MpCopyItem CreateCopyItem(MpCopyItemType itemType,object data,string sourcePath,Color tileColor) { 
            MpCopyItem newItem = new MpCopyItem();
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
                MpCommandManager.Instance.ClipboardCommander.IgnoreNextClipboardEvent = true;

                ((WebBrowser)_wb).Document.ExecCommand("SelectAll",false,null);
                ((WebBrowser)_wb).Document.ExecCommand("Copy",false,null);
                MpCommandManager.Instance.ClipboardCommander.IgnoreNextClipboardEvent = false;
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
                DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpTextItem where ItemText=@1",new List<string>(){ "@1"},new List<object>() { (string)newItem.DataObject });
                if(dt != null && dt.Rows.Count > 0) {
                    int cid = Convert.ToInt32(dt.Rows[0]["fk_MpCopyItemId"].ToString());
                    dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + cid);
                    int cc = Convert.ToInt32(dt.Rows[0]["CopyCount"].ToString()) + 1;
                    Console.WriteLine("MpCopyItem: ignoring duplicate");
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("update MpCopyItem set CopyCount="+cc+" where pk_MpCopyItemId="+cid);
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
            newItem.ItemColor = new MpColor((int)tileColor.R,(int)tileColor.G,(int)tileColor.B,255);
            return newItem;
        }
        public MpCopyItem(int copyItemId) {
            DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + copyItemId);
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
            this.ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            this.CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            this.Title = dr["Title"].ToString().Replace("''","'");
            this.CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());

            //get app and icon obj
            DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpApp where pk_MpAppId=" + AppId);
            if(dt != null && dt.Rows.Count > 0) {
                this.App = new MpApp(dt.Rows[0]);
            } else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + AppId);
            }

            //get color
            dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpColor where pk_MpColorId=" + ColorId);
            if(dt != null && dt.Rows.Count > 0) {
                this.ItemColor = new MpColor(dt.Rows[0]);
            }
            else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpColor with id " + AppId);
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
                    copyItemData = MpAppManager.Instance.DataModel.Db.Execute("select * from MpTextItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpTextItem " + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpTextItemId"].ToString());
                    this.DataObject = copyItemData.Rows[0]["ItemText"].ToString();
                    break;
                case MpCopyItemType.FileList:
                    copyItemData = MpAppManager.Instance.DataModel.Db.Execute("select * from MpFileDropListItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpFileDropListItem " + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpFileDropListItemId"].ToString());
                    copyItemData = MpAppManager.Instance.DataModel.Db.Execute("select * from MpFileDropListSubItem where fk_MpFileDropListItemId=" + this.SubItemId);
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
                    copyItemData = MpAppManager.Instance.DataModel.Db.Execute("select * from MpImageItem where fk_MpCopyItemId=" + this.CopyItemId);
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
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpTextItem where fk_MpCopyItemId=" + CopyItemId);
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    break;
                case MpCopyItemType.FileList:
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpFileDropListSubItem where fk_MpFileDropListItemId=" + SubItemId);
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpFileDropListItem where pk_MpFileDropListItemId=" + SubItemId);
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    break;
                case MpCopyItemType.Image:
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpImageItem where fk_MpCopyItemId=" + CopyItemId);
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
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
                DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpApp where pk_MpAppId=" + AppId);
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
                throw new Exception("MpCopyItem exception writing without color created");
            } else {
                ItemColor.WriteToDatabase();
                ColorId = ItemColor.ColorId;
            }
            //if copyitem already exists
            if(this.CopyItemId > 0) {
                DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + this.CopyItemId);
                if(dt.Rows.Count > 0) {
                    MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("update MpCopyItem set fk_MpCopyItemTypeId=" + (int)this.CopyItemType + ", fk_MpClientId=" + this.ClientId + ", fk_MpAppId=" + this.AppId + ",fk_MpColorId=" + this.ColorId + ", Title='"+this.Title.Replace("'","''")+"', CopyCount="+this.CopyCount+" where pk_MpCopyItemId=" + this.CopyItemId);
                }
                else {
                    Console.WriteLine("MpCopyItem error cannot find pk of existing item");
                    return;
                }
                isNew = false;
            }
            else {
                ++TotalCopyItemCount;
                if(MpAppManager.Instance.DataModel.Db.NoDb) {                    
                    CopyItemId = TotalCopyItemCount;
                    MapDataToColumns();
                    return;
                }
                MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("insert into MpCopyItem(fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount) values (" + (int)this.CopyItemType + "," + MpAppManager.Instance.DataModel.Db.Client.ClientId + "," + this.AppId + ","+this.ColorId+ ",'"+this.Title+"','" + this.CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "',"+this.CopyCount+");");
                this.CopyItemId = MpAppManager.Instance.DataModel.Db.GetLastRowId("MpCopyItem","pk_MpCopyItemId");
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
                        MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("insert into MpTextItem(fk_MpCopyItemId,ItemText) values (" + this.CopyItemId + ",@1)",new List<string>() { "@1" },new List<object>() { ((string)this.DataObject).Replace("'","''") });
                        this.SubItemId = MpAppManager.Instance.DataModel.Db.GetLastRowId("MpTextItem","pk_MpTextItemId");
                    } else {
                        MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("update MpTextItem set ItemText='" + ((string)GetData()).Replace("'","''") + "' where pk_MpTextItemId=" + this.SubItemId);
                    }                    
                    break;
                case MpCopyItemType.FileList:
                    if(isNew) {
                        MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("insert into MpFileDropListItem(fk_MpCopyItemId) values(" + this.CopyItemId + ")");
                        this.SubItemId = MpAppManager.Instance.DataModel.Db.GetLastRowId("MpFileDropListItem","pk_MpFileDropListItemId");
                        foreach(string fileOrPath in (string[])this.DataObject) {
                            MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("insert into MpFileDropListSubItem(fk_MpFileDropListItemId,ItemPath) values (" + this.SubItemId + ",'" + fileOrPath + "')");
                        }
                    } else {
                        //file lists are not editable
                    }                    
                    break;
                case MpCopyItemType.Image:
                    if(isNew) {
                        MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("insert into MpImageItem(fk_MpCopyItemId,ItemImage) values (" + this.CopyItemId + ",@0)",new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ConvertImageToByteArray((Image)this.DataObject) });
                    } else {
                        MpAppManager.Instance.DataModel.Db.ExecuteNonQuery("update MpImageItem set ItemImage=@0 where pk_MpImageItemId="+this.SubItemId,new List<string>() { "@0" },new List<object>() { this.DataObject });
                    }
                    
                    break;
            }
            MapDataToColumns();
            Console.WriteLine(isNew ? "Created ":"Updated " + " MpCopyItem");
            Console.WriteLine(ToString());
        }
        public string GetCurrentDetail(int detailId) {
            string info = "I dunno";// string.Empty;
            switch (detailId) {
                //created
                case 0:
                    info = CopyDateTime.ToString();
                    break;
                //chars/lines
                case 1:
                    if (CopyItemType == MpCopyItemType.Image) {
                        Image ciimg = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])GetData());
                        info = "(" + ciimg.Width + ") x (" + ciimg.Height + ")";
                    }
                    else if (CopyItemType == MpCopyItemType.Text) {
                        info = ((string)GetData()).Length + " chars | " + MpHelperSingleton.Instance.GetLineCount((string)GetData()) + " lines";
                    }
                    else if (CopyItemType == MpCopyItemType.FileList) {
                        info = ((string[])GetData()).Length + " files | " + MpHelperSingleton.Instance.FileListSize((string[])GetData()) + " bytes";
                    }
                    break;
                //# copies/# pastes
                case 2:
                    DataTable dt = MpAppManager.Instance.DataModel.Db.Execute("select * from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    info = CopyCount + " copies | " + dt.Rows.Count + " pastes";
                    break;
                default:
                    info = "Unknown detailId: " + detailId;
                    break;
            }

            return info;
        }
        private void MapDataToColumns() {
            TableName = "MpCopyItem";
            columnData.Clear();
            columnData.Add("pk_MpCopyItemId",this.CopyItemId);
            columnData.Add("fk_MpCopyItemTypeId",this.CopyItemType);
            columnData.Add("fk_MpClientId",this.ClientId);
            columnData.Add("fk_MpAppId",this.AppId);
            columnData.Add("fk_MpColorId",this.ColorId);
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
