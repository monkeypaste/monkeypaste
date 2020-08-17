using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MpWpfApp {
    public class MpCopyItem:MpDbObject,IComparable {
        private static int _CopyItemCount = 0;

        public List<MpSubTextToken> SubTextTokenList = new List<MpSubTextToken>();

        public Object DataObject { get; set; }
        
        public int ColorId { get;  set; }

        
        public int CopyItemId { get; set; }

        public int SubItemId { get; set; }
        public string Title { get; set; }

        public MpCopyItemType CopyItemType { get; set; }
        public int ClientId { get; set; }
        public int AppId { get; set; }
        public int IconId { get; set; }
        public DateTime CopyDateTime { get; set; }
        public int CopyCount { get; set; }
        public int PasteCount {
            get {
                return GetPasteCount();
            }
        }
        public int RelevanceScore {
            get {
                return CopyCount + PasteCount;
            }
        }
        public string SourcePath { get; set; }

        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }

        public MpCopyItem() {
        }

        public static MpCopyItem CreateFromClipboard(IntPtr processHandle) {
            IDataObject iData = Clipboard.GetDataObject();
            var iDataFormats = Clipboard.GetDataObject().GetFormats();
            MpCopyItem ci = null;
            if(iData == null) {
                return ci;
            }
            string sourcePath = MpHelpers.GetProcessPath(processHandle);
            Color itemColor = MpHelpers.GetRandomColor();
            
            try {
                if (iData.GetDataPresent(DataFormats.FileDrop)) {
                    ci = MpCopyItem.CreateCopyItem(MpCopyItemType.FileList, (string[])iData.GetData(DataFormats.FileDrop, true), sourcePath, itemColor);
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    ci = MpCopyItem.CreateCopyItem(MpCopyItemType.RichText, (string)iData.GetData(DataFormats.Rtf), sourcePath, itemColor);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    ci = MpCopyItem.CreateCopyItem(MpCopyItemType.Image, Clipboard.GetImage(), sourcePath, itemColor);
                } else if (iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) {
                    ci = MpCopyItem.CreateCopyItem(MpCopyItemType.RichText, MpHelpers.PlainTextToRtf((string)iData.GetData(DataFormats.Text)), sourcePath, itemColor);
                } else {
                    Console.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }
                return ci;
            }
            catch(Exception e) {
                //this catches intermittent COMExceptions (happened copy/pasting in Excel)
                return null;
            }
        }
        public static MpCopyItem CreateCopyItem(MpCopyItemType itemType,object data,string sourcePath,Color tileColor) { 
            MpCopyItem newItem = new MpCopyItem();
            newItem.SourcePath = sourcePath;
            newItem.CopyItemType = itemType;
            if (newItem.CopyItemType == MpCopyItemType.RichText) {
                newItem.DataObject = ((string)data);//.Trim();// Regex.Replace(((string)data).Trim(), @"^\s+(?!\B)|\s*(?>[\r\n]+)$", string.Empty, RegexOptions.Multiline).TrimEnd();
                
                //if item is a duplicate update copycount and return a reference to it
                DataTable dt = MpDb.Instance.Execute("select * from MpTextItem where ItemText=@1", new List<string>() { "@1" }, new List<object>() { (string)newItem.DataObject });
                if (dt != null && dt.Rows.Count > 0) {
                    int cid = Convert.ToInt32(dt.Rows[0]["fk_MpCopyItemId"].ToString());
                    MpCopyItem orgCopyItem = new MpCopyItem(cid);
                    //orgCopyItem.CopyCount++;
                    //orgCopyItem.CopyDateTime = DateTime.Now;
                    Console.WriteLine("MpCopyItem: ignoring duplicate and returning original item");
                    return orgCopyItem;
                }

                //parse text for tokens
                newItem.SubTextTokenList = MpSubTextToken.GatherTokens((string)newItem.DataObject);
            } 
            else {
                newItem.DataObject = data;
            }
            //newItem.CopyItemId = itemId;
            newItem.CopyDateTime = DateTime.Now;
            //newItem.sourceHandle = sourceHandle;
            newItem.Title = Enum.GetName(typeof(MpCopyItemType),newItem.CopyItemType);
            newItem.CopyCount = 1;
            newItem.App = new MpApp(sourcePath,false);
            newItem.AppId = newItem.App.appId;
            newItem.Client = new MpClient(0,0,MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(),"unknown",DateTime.Now);
            newItem.ItemColor = new MpColor((int)tileColor.R,(int)tileColor.G,(int)tileColor.B,255);
            return newItem;
        }
        public MpCopyItem(int copyItemId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + copyItemId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public static List<MpCopyItem> GetAllCopyItems() {
            List<MpCopyItem> clips = new List<MpCopyItem>();
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem");
            if(dt != null && dt.Rows.Count > 0) {
                foreach(DataRow r in dt.Rows) {
                    clips.Add(new MpCopyItem(r));
                }
            }
            return clips;
        }

        public MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }
       
        public int GetPasteCount() {
            if(CopyItemId <= 0) {
                return 0;
            }
            DataTable dt = MpDb.Instance.Execute("select * from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
            if(dt == null) {
                return 0;
            }
            return dt.Rows.Count;
        }
        public string GetPlainText() {
            switch(CopyItemType) {
                case MpCopyItemType.Image:
                    return @"[IMAGE]";
                case MpCopyItemType.RichText:
                    RichTextBox rtb = new RichTextBox();
                    rtb.SetRtf((string)DataObject);
                    return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Replace("''","'");
                case MpCopyItemType.FileList:
                    string outStr = string.Empty;
                    foreach(string str in (string[])DataObject) {
                        outStr += str + Environment.NewLine;
                    }
                    return outStr;
                default:
                    Console.WriteLine("CopyItme GetPlainText error unknow type");
                    return string.Empty;
            }
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
            DataTable dt = MpDb.Instance.Execute("select * from MpApp where pk_MpAppId=" + AppId);
            if(dt != null && dt.Rows.Count > 0) {
                this.App = new MpApp(dt.Rows[0]);
            } else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + AppId);
            }
            //get subtokens
            dt = MpDb.Instance.Execute("select * from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
            if (dt != null && dt.Rows.Count > 0) {
                foreach(DataRow row in dt.Rows) {
                    SubTextTokenList.Add(new MpSubTextToken(row));
                }
            } else {
                //copyitem not req'd to have subtokens
            }

            //get color
            dt = MpDb.Instance.Execute("select * from MpColor where pk_MpColorId=" + ColorId);
            if(dt != null && dt.Rows.Count > 0) {
                this.ItemColor = new MpColor(dt.Rows[0]);
            }
            else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpColor with id " + AppId);
            }

            DataTable copyItemData = null;
            switch(this.CopyItemType) {
                case MpCopyItemType.RichText:
                    copyItemData = MpDb.Instance.Execute("select * from MpTextItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpTextItem " + this.CopyItemId +" text is null giving it placeholder value");
                        this.DataObject = "null text placeholder";
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpTextItemId"].ToString());
                    this.DataObject = copyItemData.Rows[0]["ItemText"].ToString();
                    break;
                case MpCopyItemType.FileList:
                    copyItemData = MpDb.Instance.Execute("select * from MpFileDropListItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpFileDropListItem " + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpFileDropListItemId"].ToString());
                    copyItemData = MpDb.Instance.Execute("select * from MpFileDropListSubItem where fk_MpFileDropListItemId=" + this.SubItemId);
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
                    copyItemData = MpDb.Instance.Execute("select * from MpImageItem where fk_MpCopyItemId=" + this.CopyItemId);
                    if(copyItemData == null || copyItemData.Rows.Count == 0) {
                        Console.WriteLine("Error reading MpImageItem for Id=" + this.CopyItemId);
                        break;
                    }
                    this.SubItemId = Convert.ToInt32(copyItemData.Rows[0]["pk_MpImageItemId"].ToString());
                    this.DataObject = MpHelpers.ConvertByteArrayToBitmapSource((byte[])copyItemData.Rows[0]["ItemImage"]);
                    break;
            }
            MapDataToColumns();
            //Console.WriteLine("Loaded MpCopyItem");
            //Console.WriteLine(ToString());
        }
        public void DeleteFromDatabase() {
            if(CopyItemId <= 0) {
                return;
            }
            switch(CopyItemType) {
                case MpCopyItemType.RichText:
                    MpDb.Instance.ExecuteNonQuery("delete from MpTextItem where fk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
                    break;
                case MpCopyItemType.FileList:
                    MpDb.Instance.ExecuteNonQuery("delete from MpFileDropListSubItem where fk_MpFileDropListItemId=" + SubItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpFileDropListItem where pk_MpFileDropListItemId=" + SubItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
                    break;
                case MpCopyItemType.Image:
                    MpDb.Instance.ExecuteNonQuery("delete from MpImageItem where fk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
                    MpDb.Instance.ExecuteNonQuery("delete from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
                    break;
            }
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemTag where fk_MpCopyItemId=" + this.CopyItemId);
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemSortTypeOrder where fk_MpCopyItemId=" + this.CopyItemId);
        }
        // still req'd if NoDb=true
        public override void WriteToDatabase() {            
            bool isNew = false;
            if(App == null) {
                App = new MpApp(SourcePath,false);
                this.AppId = App.appId;
            }
            if(this.AppId == 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpApp where pk_MpAppId=" + AppId);
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
                DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + this.CopyItemId);
                if(dt.Rows.Count > 0) {
                    MpDb.Instance.ExecuteNonQuery("update MpCopyItem set fk_MpCopyItemTypeId=" + (int)this.CopyItemType + ", fk_MpClientId=" + this.ClientId + ", fk_MpAppId=" + this.AppId + ",fk_MpColorId=" + this.ColorId + ", Title='"+this.Title.Replace("'","''")+"', CopyCount="+this.CopyCount+" where pk_MpCopyItemId=" + this.CopyItemId);
                }
                else {
                    Console.WriteLine("MpCopyItem error cannot find pk of existing item");
                    return;
                }
                isNew = false;
            }
            else {
                if(MpDb.Instance.NoDb) {                    
                    CopyItemId = ++_CopyItemCount;
                    MapDataToColumns();
                    return;
                }
                MpDb.Instance.ExecuteNonQuery("insert into MpCopyItem(fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount) values (" + (int)this.CopyItemType + "," + MpDb.Instance.Client.ClientId + "," + this.AppId + ","+this.ColorId+ ",'"+this.Title+"','" + this.CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "',"+this.CopyCount+");");
                this.CopyItemId = MpDb.Instance.GetLastRowId("MpCopyItem","pk_MpCopyItemId");
                isNew = true;
            }

            switch(this.CopyItemType) {
                case MpCopyItemType.RichText:
                    if(isNew) {
                        MpDb.Instance.ExecuteNonQuery("insert into MpTextItem(fk_MpCopyItemId,ItemText) values (" + this.CopyItemId + ",@1)",new List<string>() { "@1" },new List<object>() { ((string)this.DataObject).Replace("'","''") });
                        this.SubItemId = MpDb.Instance.GetLastRowId("MpTextItem","pk_MpTextItemId");
                    } else {
                        MpDb.Instance.ExecuteNonQuery("update MpTextItem set ItemText='" + ((string)DataObject).Replace("'","''") + "' where pk_MpTextItemId=" + this.SubItemId);
                    }                    
                    break;
                case MpCopyItemType.FileList:
                    if(isNew) {
                        MpDb.Instance.ExecuteNonQuery("insert into MpFileDropListItem(fk_MpCopyItemId) values(" + this.CopyItemId + ")");
                        this.SubItemId = MpDb.Instance.GetLastRowId("MpFileDropListItem","pk_MpFileDropListItemId");
                        foreach(string fileOrPath in (string[])this.DataObject) {
                            MpDb.Instance.ExecuteNonQuery("insert into MpFileDropListSubItem(fk_MpFileDropListItemId,ItemPath) values (" + this.SubItemId + ",'" + fileOrPath + "')");
                        }
                    } else {
                        //file lists are not editable
                    }                    
                    break;
                case MpCopyItemType.Image:
                    if(isNew) {
                        MpDb.Instance.ExecuteNonQuery("insert into MpImageItem(fk_MpCopyItemId,ItemImage) values (" + this.CopyItemId + ",@0)",new List<string>() { "@0" },new List<object>() { MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.DataObject) });
                    } else {
                        MpDb.Instance.ExecuteNonQuery("update MpImageItem set ItemImage=@0 where pk_MpImageItemId="+this.SubItemId,new List<string>() { "@0" },new List<object>() { MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)this.DataObject) });
                    }
                    
                    break;
            }
            foreach (MpSubTextToken subToken in SubTextTokenList) {
                subToken.CopyItemId = CopyItemId;
                subToken.WriteToDatabase();
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
                        //System.Drawing.Image ciimg = MpHelpers.ImageConverter.ConvertByteArrayToImage((byte[])GetData());
                        
                        info = "(" + ((BitmapSource)DataObject).Width + ") x (" + ((BitmapSource)DataObject).Height + ")";
                    }
                    else if (CopyItemType == MpCopyItemType.RichText) {
                        info = GetPlainText().Length + " chars | " + MpHelpers.GetLineCount(GetPlainText()) + " lines";
                    }
                    else if (CopyItemType == MpCopyItemType.FileList) {
                        info = ((string[])DataObject).Length + " files | " + MpHelpers.FileListSize((string[])DataObject) + " bytes";
                    }
                    break;
                //# copies/# pastes
                case 2:
                    DataTable dt = MpDb.Instance.Execute("select * from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
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

        public int CompareTo(object obj) {
            if(((MpCopyItem)obj).DataObject == DataObject && ((MpCopyItem)obj).AppId == AppId) {
                return 0;
            }
            if (((MpCopyItem)obj).CopyDateTime > CopyDateTime && ((MpCopyItem)obj).AppId == AppId) {
                return -1;
            }
            return 1;
        }
    }

    public enum MpCopyItemType {
        None=0,
        RichText,
        Image,
        FileList
    }    
}
