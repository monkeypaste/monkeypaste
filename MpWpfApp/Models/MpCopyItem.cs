using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpCopyItem : MpDbObject {
        #region Private Variables

        private static int _CopyItemCount = 0;

        #endregion

        #region Properties

        public List<MpSubTextToken> SubTextTokenList { get; set; } = new List<MpSubTextToken>();
        public int CopyItemId { get; set; }
        public object DataObject { get; set; }
        public int ColorId { get; private set; }
        public string Title { get; set; }
        public MpCopyItemType CopyItemType { get; private set; }
        public int ClientId { get; private set; }
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
        public string ItemPlainText { get; set; }
        public string ItemRichText { get; set; }
        public BitmapSource ItemImage { get; set; }
        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }

        #endregion

        #region Public Methods

        public MpCopyItem() {
        }

        public static MpCopyItem CreateFromClipboard(IntPtr processHandle) {
            IDataObject iData = Clipboard.GetDataObject();
            MpCopyItem newCopyItem = null;
            if (iData == null) {
                return newCopyItem;
            }
            string sourcePath = MpHelpers.GetProcessPath(processHandle);
            Color itemColor = MpHelpers.GetRandomColor();

            try {
                if (iData.GetDataPresent(DataFormats.FileDrop)) {
                    newCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.FileList, (string[])iData.GetData(DataFormats.FileDrop, true), sourcePath, itemColor);
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    newCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.RichText, (string)iData.GetData(DataFormats.Rtf), sourcePath, itemColor);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    newCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.Image, Clipboard.GetImage(), sourcePath, itemColor);
                } else if ((iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) && !string.IsNullOrEmpty((string)iData.GetData(DataFormats.Text))) {
                    newCopyItem = MpCopyItem.CreateCopyItem(MpCopyItemType.RichText, MpHelpers.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.Text)), sourcePath, itemColor);
                } else {
                    Console.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }
                return newCopyItem;
            }
            catch (Exception e) {
                //this catches intermittent COMExceptions (happened copy/pasting in Excel)
                Console.WriteLine("Caught exception creating copyitem: " + e.ToString());
                return null;
            }
        }
        
        public static MpCopyItem CreateCopyItem(MpCopyItemType itemType, object data, string sourcePath, Color tileColor) {
            MpCopyItem newItem = new MpCopyItem();
            newItem.SourcePath = sourcePath;
            newItem.CopyItemType = itemType;
            newItem.CopyDateTime = DateTime.Now;
            newItem.Title = Enum.GetName(typeof(MpCopyItemType), newItem.CopyItemType);
            newItem.CopyCount = 1;
            newItem.App = new MpApp(sourcePath, false);
            newItem.AppId = newItem.App.appId;
            newItem.Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            newItem.ItemColor = new MpColor((int)tileColor.R, (int)tileColor.G, (int)tileColor.B, 255);

            switch (itemType) {
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                    }
                    newItem.DataObject = paths;
                    newItem.ItemPlainText = (string)newItem.DataObject;
                    newItem.ItemRichText = MpHelpers.ConvertPlainTextToRichText(newItem.ItemPlainText);
                    newItem.ItemImage = MpHelpers.ConvertRichTextToImage(newItem.ItemRichText);
                    break;
                case MpCopyItemType.Image:
                    newItem.DataObject = MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)data);
                    newItem.ItemImage = (BitmapSource)data;
                    newItem.ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(newItem.ItemImage);
                    newItem.ItemRichText = MpHelpers.ConvertPlainTextToRichText(newItem.ItemPlainText);
                    break;
                case MpCopyItemType.RichText:
                    newItem.DataObject = newItem.ItemPlainText = MpHelpers.IsStringRichText((string)data) ? (string)data : MpHelpers.ConvertPlainTextToRichText((string)data);
                    newItem.ItemRichText = (string)newItem.DataObject;
                    newItem.ItemPlainText = MpHelpers.ConvertRichTextToPlainText(newItem.ItemRichText);
                    newItem.ItemImage = MpHelpers.ConvertRichTextToImage(newItem.ItemRichText);
                    newItem.SubTextTokenList = MpSubTextToken.GatherTokens((string)newItem.DataObject);
                    break;
            }
            var existingItem = newItem.GetExistingCopyItem();
            if (existingItem != null) {
                Console.WriteLine("MpCopyItem: ignoring duplicate and returning original item");
                return existingItem;
            }
            
            return newItem;
        }
        
        public static List<MpCopyItem> GetAllCopyItems() {
            List<MpCopyItem> clips = new List<MpCopyItem>();
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    clips.Add(new MpCopyItem(dr));
                }
            }
            return clips;
        }

        protected MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }

        //returns null if not found
        public MpCopyItem GetExistingCopyItem() {
            DataTable dt = null;
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                case MpCopyItemType.RichText:
                    dt = MpDb.Instance.Execute("select * from MpCopyItem where fk_MpCopyItemTypeId=" + (int)CopyItemType + " and ItemText='" + (string)DataObject + "'");
                    break;
                case MpCopyItemType.Image:
                    dt = MpDb.Instance.Execute("select * from MpCopyItem where fk_MpCopyItemTypeId=" + (int)CopyItemType + " and ItemImage=@0", new List<string>() { "@0" }, new List<object>() { (byte[])DataObject });
                    break;
            }
            if (dt != null && dt.Rows.Count > 0) {
                return new MpCopyItem(dt.Rows[0]);
            }
            return null;
        }

        public int GetPasteCount() {
            if (CopyItemId <= 0) {
                return 0;
            }
            DataTable dt = MpDb.Instance.Execute("select * from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
            if (dt == null) {
                return 0;
            }
            return dt.Rows.Count;
        }

        //public string GetPlainText() {
        //    switch (CopyItemType) {
        //        case MpCopyItemType.Image:
        //            return MpHelpers.ConvertBitmapSourceToPlainText(MpHelpers.ConvertByteArrayToBitmapSource((byte[])DataObject));
        //        case MpCopyItemType.RichText:
        //            return MpHelpers.ConvertRichTextToPlainText((string)DataObject);
        //        case MpCopyItemType.FileList:
        //            return (string)DataObject;
        //        default:
        //            Console.WriteLine("CopyItme GetPlainText error unknow type");
        //            return string.Empty;
        //    }
        //}

        //public string GetRichText() {
        //    switch (CopyItemType) {
        //        case MpCopyItemType.Image:
        //            return MpHelpers.ConvertPlainTextToRichText(MpHelpers.ConvertBitmapSourceToPlainText(MpHelpers.ConvertByteArrayToBitmapSource((byte[])DataObject)));
        //        case MpCopyItemType.RichText:
        //            return (string)DataObject;
        //        case MpCopyItemType.FileList:
        //            return MpHelpers.ConvertPlainTextToRichText((string)DataObject);
        //        default:
        //            Console.WriteLine("CopyItme GetRichText error unknow type");
        //            return string.Empty;
        //    }
        //}

        //public BitmapSource GetBitmapSource() {
        //    switch (CopyItemType) {
        //        case MpCopyItemType.Image:
        //            return MpHelpers.ConvertByteArrayToBitmapSource((byte[])DataObject);
        //        case MpCopyItemType.RichText:
        //        case MpCopyItemType.FileList:
        //            return MpHelpers.ConvertRichTextToImage(GetRichText());
        //        default:
        //            Console.WriteLine("CopyItme GetBitmapSource error unknow type");
        //            return null;
        //    }
        //}

        public List<string> GetFileList(string baseDir = "",MpCopyItemType forceType = MpCopyItemType.None) {
            //returns path of tmp file for rt or img and actual paths of filelist
            var fileList = new List<string>();
            if (CopyItemType == MpCopyItemType.FileList) {
                if(forceType == MpCopyItemType.Image) {
                    fileList.Add(MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemImage));
                } else if (forceType == MpCopyItemType.RichText) {
                    fileList.Add(MpHelpers.WriteTextToFile(Path.GetTempFileName(), ItemPlainText));
                } else {
                    var splitArray = ((string)DataObject).Split(Environment.NewLine.ToCharArray());
                    if (splitArray == null || splitArray.Length == 0) {
                        throw new Exception("CopyItem GetFileList error, file list should not be empty");
                    } else {
                        foreach (string p in splitArray) {
                            if (!string.IsNullOrEmpty(p.Trim())) {
                                fileList.Add(p);
                            }
                        }
                    }
                }
            } else {
                string fp = string.IsNullOrEmpty(baseDir) ? Path.GetTempPath() : baseDir;
                string fn = MpHelpers.RemoveSpecialCharacters(Title.Trim());
                if (string.IsNullOrEmpty(fn)) {
                    fn = Path.GetRandomFileName();
                }
                string fe = CopyItemType == MpCopyItemType.RichText ? ".txt" : ".png";
                fe = forceType == MpCopyItemType.RichText ? ".txt" : fe;
                fe = forceType == MpCopyItemType.Image ? ".png" : fe;
                string op = MpHelpers.GetUniqueFileName(fp + fn + fe);
                //file extension
                switch (CopyItemType) {
                    case MpCopyItemType.RichText:
                        if(forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.WriteBitmapSourceToFile(op, ItemImage));
                        } else {
                            fileList.Add(MpHelpers.WriteTextToFile(op, ItemPlainText));
                        }
                        break;
                    case MpCopyItemType.Image:
                        if (forceType == MpCopyItemType.RichText) {
                            fileList.Add(MpHelpers.WriteTextToFile(op, ItemPlainText));
                        } else {
                            fileList.Add(MpHelpers.WriteBitmapSourceToFile(op, ItemImage));
                        }
                        break;
                }
            }

            return fileList;
        }

        public void Combine(MpCopyItem otherItem) {
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                    var fileStr = string.Empty;
                    foreach(string f in otherItem.GetFileList()) {
                        fileStr += f + Environment.NewLine;
                    }
                    DataObject += Environment.NewLine + fileStr;
                    ItemPlainText = (string)DataObject;
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    ItemImage = MpHelpers.ConvertRichTextToImage(ItemRichText);
                    break;
                case MpCopyItemType.Image:
                    ItemImage = MpHelpers.CombineBitmap(new List<BitmapSource>() { ItemImage, otherItem.ItemImage });
                    DataObject = MpHelpers.ConvertBitmapSourceToByteArray(ItemImage);
                    ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemImage);
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    break;
                case MpCopyItemType.RichText:
                    DataObject = MpHelpers.CombineRichText(ItemRichText, otherItem.ItemRichText);
                    ItemRichText = (string)DataObject;
                    ItemPlainText = MpHelpers.ConvertRichTextToPlainText(ItemRichText); 
                    ItemImage = MpHelpers.ConvertRichTextToImage(ItemRichText);
                    SubTextTokenList = MpSubTextToken.GatherTokens(ItemRichText);
                    break;
            }
        }

        public void DeleteFromDatabase() {
            if (CopyItemId <= 0) {
                return;
            }
            MpDb.Instance.ExecuteNonQuery("delete from MpPasteHistory where fk_MpCopyItemId=" + CopyItemId);
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
            MpDb.Instance.ExecuteNonQuery("delete from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemTag where fk_MpCopyItemId=" + this.CopyItemId);
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemSortTypeOrder where fk_MpCopyItemId=" + this.CopyItemId);
        }

        #endregion

        #region Private Methods

        private void MapDataToColumns() {
            TableName = "MpCopyItem";
            columnData.Clear();
            columnData.Add("pk_MpCopyItemId", this.CopyItemId);
            columnData.Add("fk_MpCopyItemTypeId", this.CopyItemType);
            columnData.Add("fk_MpClientId", this.ClientId);
            columnData.Add("fk_MpAppId", this.AppId);
            columnData.Add("fk_MpColorId", this.ColorId);
            columnData.Add("CopyDateTime", this.CopyDateTime);
            //columnData.Add("SubItemId",this.SubItemId);
            columnData.Add("Title", this.Title);
            //columnData.Add("DataObject",this.DataObject);
            columnData.Add("CopyCount", this.CopyCount);
        }

        #endregion

        #region Overrides

        public override void LoadDataRow(DataRow dr) {
            this.CopyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            this.CopyItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            this.ClientId = Convert.ToInt32(dr["fk_MpClientId"].ToString());
            this.AppId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            this.ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            this.CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            this.Title = dr["Title"].ToString().Replace("''", "'");
            this.CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                    DataObject = (object)dr["ItemText"].ToString();
                    ItemPlainText = (string)dr["ItemText"].ToString();
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    ItemImage = MpHelpers.ConvertRichTextToImage(ItemRichText);
                    break;
                case MpCopyItemType.Image:
                    DataObject = (byte[])dr["ItemImage"];
                    ItemImage = MpHelpers.ConvertByteArrayToBitmapSource((byte[])DataObject);
                    ItemPlainText = (string)dr["ItemText"].ToString();
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    break;
                case MpCopyItemType.RichText:
                    DataObject = dr["ItemText"].ToString();
                    ItemRichText = (string)DataObject;
                    ItemPlainText = MpHelpers.ConvertRichTextToPlainText(ItemRichText);
                    ItemImage = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]);
                    SubTextTokenList = MpSubTextToken.GatherTokens(ItemRichText);
                    break;
            }
             
            //get app and icon obj
            DataTable dt = MpDb.Instance.Execute("select * from MpApp where pk_MpAppId=" + AppId);
            if (dt != null && dt.Rows.Count > 0) {
                this.App = new MpApp(dt.Rows[0]);
            } else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + AppId);
            }

            //get subtokens
            dt = MpDb.Instance.Execute("select * from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow row in dt.Rows) {
                    SubTextTokenList.Add(new MpSubTextToken(row));
                }
            } else {
                //copyitem not req'd to have subtokens
            }

            //get color
            dt = MpDb.Instance.Execute("select * from MpColor where pk_MpColorId=" + ColorId);
            if (dt != null && dt.Rows.Count > 0) {
                this.ItemColor = new MpColor(dt.Rows[0]);
            } else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpColor with id " + AppId);
            }
            MapDataToColumns();
        }
        
        // still req'd if NoDb=true
        public override void WriteToDatabase() {
            bool isNew = false;
            if (App == null) {
                App = new MpApp(SourcePath, false);
                this.AppId = App.appId;
            }
            if (this.AppId == 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpApp where pk_MpAppId=" + AppId);
                if (dt != null && dt.Rows.Count > 0) {
                    this.App = new MpApp(dt.Rows[0]);
                    this.App.appId = 0;
                } else {
                    //this case occur
                    //Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + appId);
                }
            }
            App.WriteToDatabase();
            this.AppId = this.App.appId;

            if (ItemColor == null) {
                throw new Exception("MpCopyItem exception writing without color created");
            } else {
                ItemColor.WriteToDatabase();
                ColorId = ItemColor.ColorId;
            }
            string itemText = this.CopyItemType == MpCopyItemType.RichText ? (string)DataObject : ItemPlainText;
            byte[] itemImage = this.CopyItemType == MpCopyItemType.Image ? (byte[])DataObject : MpHelpers.ConvertBitmapSourceToByteArray(ItemImage);
            //if copyitem already exists
            if (this.CopyItemId > 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + this.CopyItemId);
                if (dt.Rows.Count > 0) {
                    MpDb.Instance.ExecuteNonQuery("update MpCopyItem set fk_MpCopyItemTypeId=" + (int)this.CopyItemType + ", fk_MpClientId=" + this.ClientId + ", fk_MpAppId=" + this.AppId + ",fk_MpColorId=" + this.ColorId + ", Title='" + this.Title.Replace("'", "''") + "', CopyCount=" + this.CopyCount + ", ItemText='" + itemText.Replace("'", "''") + "', ItemImage=@0 where pk_MpCopyItemId=" + this.CopyItemId, new List<string>() { "@0" }, new List<object>() { itemImage });
                } else {
                    Console.WriteLine("MpCopyItem error cannot find pk of existing item");
                    return;
                }
                isNew = false;
            } else {
                if (MpDb.Instance.NoDb) {
                    CopyItemId = ++_CopyItemCount;
                    MapDataToColumns();
                    return;
                }
                MpDb.Instance.ExecuteNonQuery("insert into MpCopyItem(fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount,ItemText,ItemImage) values (" + (int)this.CopyItemType + "," + MpDb.Instance.Client.ClientId + "," + this.AppId + "," + this.ColorId + ",'" + this.Title + "','" + this.CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'," + this.CopyCount + ",'" + itemText.Replace("'", "''") + "',@0);", new List<string>() { "@0" }, new List<object>() { itemImage });
                this.CopyItemId = MpDb.Instance.GetLastRowId("MpCopyItem", "pk_MpCopyItemId");
                isNew = true;
            }
            foreach (MpSubTextToken subToken in SubTextTokenList) {
                subToken.CopyItemId = CopyItemId;
                subToken.WriteToDatabase();
            }

            MapDataToColumns();
            Console.WriteLine(isNew ? "Created " : "Updated " + " MpCopyItem");
            Console.WriteLine(ToString());
        }

        #endregion
    }

    public enum MpCopyItemType {
        None = 0,
        RichText,
        Image,
        FileList,
        Csv //this is only used during runtime
    }
}
