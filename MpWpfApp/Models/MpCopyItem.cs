using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Documents;
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
        //private object DataObject { get; set; }
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
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public string SourcePath { get; set; }

        public string ItemPlainText { get; set; }
        public string ItemRichText { get; set; }
        public FlowDocument ItemFlowDocument { get; set; }
        public BitmapSource ItemBitmapSource { get; set; }

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
            newItem.Title = "Untitled"; //Enum.GetName(typeof(MpCopyItemType), newItem.CopyItemType);
            newItem.CopyCount = 1;
            newItem.App = new MpApp(sourcePath, false);
            newItem.AppId = newItem.App.AppId;
            newItem.Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            newItem.ItemColor = new MpColor((int)tileColor.R, (int)tileColor.G, (int)tileColor.B, 255);

            switch (itemType) {
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                        newItem.Width = Math.Max(newItem.Width, str.Length);
                    }
                    newItem.ItemPlainText = paths;
                    newItem.ItemRichText = MpHelpers.ConvertPlainTextToRichText(newItem.ItemPlainText);
                    newItem.ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(newItem.ItemRichText);
                    newItem.ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(newItem.ItemRichText);
                    newItem.SubTextTokenList = MpSubTextToken.GatherTokens((string)newItem.ItemRichText);
                    newItem.Height = ((string[])data).Length;
                    break;
                case MpCopyItemType.Image:
                    newItem.ItemBitmapSource = (BitmapSource)data;
                    newItem.ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(newItem.ItemBitmapSource);
                    newItem.ItemRichText = MpHelpers.ConvertPlainTextToRichText(newItem.ItemPlainText);
                    newItem.ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(newItem.ItemRichText);
                    newItem.Height = newItem.ItemBitmapSource.PixelHeight;
                    newItem.Width = newItem.ItemBitmapSource.PixelWidth;
                    break;
                case MpCopyItemType.RichText:
                    newItem.ItemPlainText = MpHelpers.IsStringRichText((string)data) ? MpHelpers.ConvertRichTextToPlainText((string)data) : (string)data;
                    newItem.ItemRichText = MpHelpers.IsStringRichText((string)data) ? (string)data : MpHelpers.ConvertPlainTextToRichText((string)data);
                    newItem.ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(newItem.ItemRichText);
                    newItem.ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(newItem.ItemRichText);
                    newItem.SubTextTokenList = MpSubTextToken.GatherTokens((string)newItem.ItemRichText);
                    newItem.Height = MpHelpers.GetRowCount((string)data);
                    break;
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
                    fileList.Add(MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource));
                } else if (forceType == MpCopyItemType.RichText) {
                    fileList.Add(MpHelpers.WriteTextToFile(Path.GetTempFileName(), ItemPlainText));
                } else {
                    var splitArray = ItemPlainText.Split(Environment.NewLine.ToCharArray());
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
                            fileList.Add(MpHelpers.WriteBitmapSourceToFile(op, ItemBitmapSource));
                        } else {
                            fileList.Add(MpHelpers.WriteTextToFile(op, ItemPlainText));
                        }
                        break;
                    case MpCopyItemType.Image:
                        if (forceType == MpCopyItemType.RichText) {
                            fileList.Add(MpHelpers.WriteTextToFile(op, ItemPlainText));
                        } else {
                            fileList.Add(MpHelpers.WriteBitmapSourceToFile(op, ItemBitmapSource));
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
                    ItemPlainText += Environment.NewLine + fileStr;
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                    ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                    break;
                case MpCopyItemType.Image:
                    ItemBitmapSource = MpHelpers.CombineBitmap(new List<BitmapSource>() { ItemBitmapSource, otherItem.ItemBitmapSource });
                    ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemBitmapSource);
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                    break;
                case MpCopyItemType.RichText:
                    switch(otherItem.CopyItemType) {
                        case MpCopyItemType.Image:
                            MpHelpers.AppendBitmapSourceToFlowDocument(ItemFlowDocument, otherItem.ItemBitmapSource);
                            break;
                        default:
                            MpHelpers.CombineFlowDocuments(otherItem.ItemFlowDocument, ItemFlowDocument);
                            ItemRichText = MpHelpers.ConvertFlowDocumentToRichText(ItemFlowDocument);
                            ItemPlainText = MpHelpers.ConvertRichTextToPlainText(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            SubTextTokenList = MpSubTextToken.GatherTokens(ItemRichText);
                            break;
                    }
                    
                    break;
            }
        }

        public void ConvertType(MpCopyItemType newType) {
            if (newType == CopyItemType) {
                return;
            }
            
            switch (newType) {
                case MpCopyItemType.FileList:
                    switch(CopyItemType) {
                        case MpCopyItemType.RichText:
                            ItemPlainText = MpHelpers.WriteTextToFile(Path.GetTempFileName(), ItemPlainText, false);
                            ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                            ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            break;
                        case MpCopyItemType.Image:
                            ItemPlainText = MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource, false);
                            ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                            ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            break;
                    }
                    break;
                case MpCopyItemType.Image:
                    switch (CopyItemType) {
                        case MpCopyItemType.RichText:
                            ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemBitmapSource);
                            ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                            ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            break;
                        case MpCopyItemType.FileList:
                            ItemPlainText = MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource, false);
                            ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                            ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            break;
                    }
                    break;
                case MpCopyItemType.RichText:
                    switch (CopyItemType) {
                        case MpCopyItemType.Image:
                            ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemBitmapSource);
                            ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                            ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            break;
                        case MpCopyItemType.FileList:
                            ItemPlainText = MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource, false);
                            ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                            ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                            ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                            break;
                    }
                    break;
            }
            CopyItemType = newType;
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
            CopyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            this.CopyItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            this.ClientId = Convert.ToInt32(dr["fk_MpClientId"].ToString());
            this.AppId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            this.ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            this.CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            this.Title = dr["Title"].ToString().Replace("''", "'");
            this.CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                    ItemPlainText = (string)dr["ItemText"].ToString();
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                    ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                    break;
                case MpCopyItemType.Image:
                    ItemBitmapSource = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]);
                    ItemPlainText = (string)dr["ItemText"].ToString();
                    ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                    break;
                case MpCopyItemType.RichText:
                    ItemRichText = dr["ItemText"].ToString();
                    ItemFlowDocument = MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                    ItemPlainText = MpHelpers.ConvertRichTextToPlainText(ItemRichText);
                    ItemBitmapSource = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]);
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
                this.AppId = App.AppId;
            }
            if (this.AppId == 0) {
                DataTable dt = MpDb.Instance.Execute("select * from MpApp where pk_MpAppId=" + AppId);
                if (dt != null && dt.Rows.Count > 0) {
                    this.App = new MpApp(dt.Rows[0]);
                    this.App.AppId = 0;
                } else {
                    //this case occur
                    //Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + appId);
                }
            }
            App.WriteToDatabase();
            this.AppId = this.App.AppId;

            if (ItemColor == null) {
                throw new Exception("MpCopyItem exception writing without color created");
            } else {
                ItemColor.WriteToDatabase();
                ColorId = ItemColor.ColorId;
            }
            string itemText = this.CopyItemType == MpCopyItemType.RichText ? ItemRichText : ItemPlainText;
            byte[] itemImage = MpHelpers.ConvertBitmapSourceToByteArray(ItemBitmapSource);
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
