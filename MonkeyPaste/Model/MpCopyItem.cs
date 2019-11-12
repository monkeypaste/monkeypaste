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

        public int copyItemId { get; set; }
        public int SubItemId { get; set; }
        public string Title { get; set; }
        public MpCopyItemType copyItemTypeId { get; set; }
        public int clientId { get; set; }
        public int appId { get; set; }
        public DateTime CopyDateTime { get; set; }
        public int CopyCount { get; set; }
        private IntPtr sourceHandle;

        public MpApp App { get; set; }

        public MpCopyItem() { }

        private WebBrowser _wb;

        public MpCopyItem(int itemId, MpCopyItemType itemType,int clientId,int appId,object data,IntPtr sourceHandle) {
            this.copyItemTypeId = itemType;
            if(copyItemTypeId == MpCopyItemType.RichText) {
                RichTextBox rtb = new RichTextBox() { Rtf = (string)data };
                DataObject = rtb.Text;
                copyItemTypeId = MpCopyItemType.Text;
            }
            else if(copyItemTypeId == MpCopyItemType.HTMLText) {
                string dataStr = (string)data;
                int idx0 = dataStr.IndexOf("<html>") < 0 ? 0 : dataStr.IndexOf("<html>");
                int idx1 = dataStr.IndexOf("/<html>") < 0 ? dataStr.Length - 1 : dataStr.IndexOf("/<html>");
                dataStr = dataStr.Substring(idx0,idx1 - idx0);
                dataStr.Insert(dataStr.IndexOf("<html>") + 4," style='border:none;'>");
                _wb = new WebBrowser() {
                    AutoSize = true,
                    DocumentText = dataStr
                };
                MpSingletonController.Instance.SetIgnoreNextClipboardEvent(true);
                ((WebBrowser)_wb).Document.ExecCommand("SelectAll",false,null);
                ((WebBrowser)_wb).Document.ExecCommand("Copy",false,null);
                MpSingletonController.Instance.SetIgnoreNextClipboardEvent(false);
                TextBox temp = new TextBox();
                temp.Paste();
                DataObject = (string)temp.Text;
                ((WebBrowser)_wb).Document.ExecCommand("UNSELECT",false,Type.Missing);
                copyItemTypeId = MpCopyItemType.Text;
            }
            else if(copyItemTypeId == MpCopyItemType.Text) {
                DataObject = (string)data;
            }
            else {
                DataObject = data;
            }

            if(MpSingletonController.Instance.InAppendMode && copyItemTypeId == MpCopyItemType.Text) {
                if(MpSingletonController.Instance.AppendItem == null) {
                    MpSingletonController.Instance.AppendItem = this;
                } else {
                    string appendedStr = (string)MpSingletonController.Instance.AppendItem.GetData() + Environment.NewLine + (string)DataObject;
                    MpSingletonController.Instance.AppendItem.SetData((object)appendedStr);
                    MpSingletonController.Instance.GetMpData().UpdateMpCopyItem(MpSingletonController.Instance.AppendItem);
                    copyItemId = -2;
                    return;
                }
            } 
            this.copyItemId = itemId;   
            this.CopyDateTime = DateTime.Now;
            this.sourceHandle = sourceHandle;
            this.Title = Enum.GetName(typeof(MpCopyItemType),copyItemId).ToString();
            this.CopyCount = 1;
            App = new MpApp(0,0,sourceHandle,false);
            this.appId = App.appId;
            MpSingletonController.Instance.GetMpData().AddMpApp(App);

            //WriteToDatabase();
        }
        public MpCopyItem(object data,MpCopyItemType mpType,IntPtr sourceHandle) : this(0,mpType,MpSingletonController.Instance.GetMpData().GetMpClient().MpClientId,0,data,sourceHandle) { }
        public MpCopyItem(string copyText,MpCopyItemType textType,IntPtr sourceHandle) :this((object)copyText,textType,sourceHandle) {}
        public MpCopyItem(Image img,IntPtr sourceHandle) :this((object)MpHelperSingleton.Instance.ConvertImageToByteArray(img),MpCopyItemType.Image,sourceHandle) {}
        public MpCopyItem(string[] fileDropList,IntPtr sourceHandle) : this((object)fileDropList,MpCopyItemType.FileList,sourceHandle) {}

        public void SetData(object newData) {
            DataObject = newData;
            WriteToDatabase();
        }
        public object GetData() {
            switch(this.copyItemTypeId) {
                //case MpCopyItemType.PhoneNumber:
                //case MpCopyItemType.StreetAddress:
                //case MpCopyItemType.WebLink:
                //case MpCopyItemType.Email:
                case MpCopyItemType.Text:
                case MpCopyItemType.RichText:
                case MpCopyItemType.HTMLText:
                    //case MpCopyItemType.RichText:
                    return DataObject;
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
                    return (byte[])DataObject;           
            }
            return "Error unknown copyitem format!";
        }

        public MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }
        
        public override void LoadDataRow(DataRow dr) {
            this.copyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            this.copyItemTypeId = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            this.clientId = Convert.ToInt32(dr["fk_MpClientId"].ToString());
            this.appId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            this.CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            this.Title = dr["Title"].ToString();
            this.CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());

            App = new MpApp(MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpApp where pk_MpAppId=" + appId).Rows[0]);

            DataTable copyItemData = null;
            switch(this.copyItemTypeId) {
                //case MpCopyItemType.PhoneNumber:
                //case MpCopyItemType.StreetAddress:
                //case MpCopyItemType.WebLink:
                //case MpCopyItemType.Email:
                case MpCopyItemType.Text:
                case MpCopyItemType.HTMLText:
                case MpCopyItemType.RichText:
                    copyItemData = MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpTextItem where fk_MpCopyItemId=" + this.copyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpTextItem " + this.copyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpTextItemId"].ToString());
                    this.DataObject = copyItemData.Rows[0]["ItemText"].ToString();
                    break;
                case MpCopyItemType.FileList:
                    copyItemData = MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpFileDropListItem where fk_MpCopyItemId=" + this.copyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpFileDropListItem " + this.copyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpFileDropListItemId"].ToString());
                    copyItemData = MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpFileDropListSubItem where fk_MpFileDropListItemId=" + this.SubItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpFileDropListSubItem for  MpFileDropListItemId=" + this.copyItemId);
                        break;
                    }
                    this.DataObject = new string[copyItemData.Rows.Count];
                    for(int i = 0;i < copyItemData.Rows.Count;i++) {
                        string fileOrPath = copyItemData.Rows[i]["ItemPath"].ToString();
                        ((string[])this.DataObject)[i] = fileOrPath;
                    }
                    break;
                case MpCopyItemType.Image:
                    copyItemData = MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpImageItem where fk_MpCopyItemId=" + this.copyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpImageItem for Id=" + this.copyItemId);
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
        // still req'd if NoDb=true
        public override void WriteToDatabase() {            
            bool isNew = false;

            if(this.appId == 0) {
                App = new MpApp(0,0,sourceHandle,false);
                this.appId = App.appId;
                MpSingletonController.Instance.GetMpData().AddMpApp(App);
            }
            //if copyitem already exists
            if(this.copyItemId > 0) {
                DataTable dt = MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + this.copyItemId);
                if(dt.Rows.Count > 0) {
                    MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("update MpCopyItem set fk_MpCopyItemTypeId=" + (int)this.copyItemTypeId + ", fk_MpClientId=" + this.clientId + ", fk_MpAppId=" + this.appId + ", Title='"+this.Title+"', CopyCount="+this.CopyCount+" where pk_MpCopyItemId=" + this.copyItemId);
                }
                else {
                    Console.WriteLine("MpCopyItem error cannot find pk of existing item");
                    return;
                }
                isNew = false;
            }
            else {
                //ensure not a dup
                foreach(MpCopyItem ci in MpSingletonController.Instance.GetMpData().GetMpCopyItemList()) {
                    if(ci.copyItemTypeId != copyItemTypeId) {
                        continue;
                    }
                    //if this new item is a duplicate update original and return
                    if(ci.GetData() == GetData() && ci.appId == appId) {
                        ci.CopyCount++;
                        if(MpSingletonController.Instance.GetMpData().Db.NoDb) {
                            ci.WriteToDatabase();
                        }
                        MpSingletonController.Instance.GetMpData().UpdateMpCopyItem(ci);
                        copyItemId = -1;
                        return;
                    }
                }
                ++TotalCopyItemCount;
                if(MpSingletonController.Instance.GetMpData().Db.NoDb) {                    
                    copyItemId = TotalCopyItemCount;
                    MapDataToColumns();
                    return;
                }
                MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("insert into MpCopyItem(fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,Title,CopyDateTime,CopyCount) values (" + (int)this.copyItemTypeId + "," + MpSingletonController.Instance.GetMpData().GetMpClient().MpClientId + "," + this.appId + ",'"+this.Title+"','" + this.CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "',"+this.CopyCount+");");
                this.copyItemId = MpSingletonController.Instance.GetMpData().Db.GetLastRowId("MpCopyItem","pk_MpCopyItemId");
                isNew = true;
            }

            switch(this.copyItemTypeId) {
                case MpCopyItemType.RichText:
                case MpCopyItemType.HTMLText:
                //case MpCopyItemType.PhoneNumber:
                //case MpCopyItemType.StreetAddress:
                //case MpCopyItemType.WebLink:
                //case MpCopyItemType.Email:
                case MpCopyItemType.Text:
                    if(isNew) {
                        MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("insert into MpTextItem(fk_MpCopyItemId,ItemText) values (" + this.copyItemId + ",@1)",new List<string>() { "@1" },new List<object>() { this.DataObject });
                        this.SubItemId = MpSingletonController.Instance.GetMpData().Db.GetLastRowId("MpTextItem","pk_MpTextItemId");
                    } else {
                        MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("update MpTextItem set ItemText='" + (string)GetData() + "' where pk_MpTextItemId=" + this.SubItemId);
                    }                    
                    break;
                case MpCopyItemType.FileList:
                    if(isNew) {
                        MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("insert into MpFileDropListItem(fk_MpCopyItemId) values(" + this.copyItemId + ")");
                        this.SubItemId = MpSingletonController.Instance.GetMpData().Db.GetLastRowId("MpFileDropListItem","pk_MpFileDropListItemId");
                        foreach(string fileOrPath in (string[])this.DataObject) {
                            MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("insert into MpFileDropListSubItem(fk_MpFileDropListItemId,ItemPath) values (" + this.SubItemId + ",'" + fileOrPath + "')");
                        }
                    } else {
                        //file lists are not editable
                    }                    
                    break;
                case MpCopyItemType.Image:
                    if(isNew) {
                        MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("insert into MpImageItem(fk_MpCopyItemId,ItemImage) values (" + this.copyItemId + ",@0)",new List<string>() { "@0" },new List<object>() { this.DataObject });
                    } else {
                        MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("update MpImageItem set ItemImage=@0 where pk_MpImageItemId="+this.SubItemId,new List<string>() { "@0" },new List<object>() { this.DataObject });
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
            columnData.Add("pk_MpCopyItemId",this.copyItemId);
            columnData.Add("fk_MpCopyItemTypeId",this.copyItemTypeId);
            columnData.Add("fk_MpClientId",this.clientId);
            columnData.Add("fk_MpAppId",this.appId);
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
