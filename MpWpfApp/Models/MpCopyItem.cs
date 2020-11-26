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
        private object _itemData = null;
        #endregion

        #region Properties

        public int CopyItemId { get; set; } = 0;
        public int PreCopyItemId { get; set; } = 0;
        public int PostCopyItemId { get; set; } = 0;
        public int ColorId { get; private set; } = 1;
        public string Title { get; set; } = "Untitled";
        public MpCopyItemType CopyItemType { get; private set; } = MpCopyItemType.None;
        public int ClientId { get; private set; } = 0;
        public int AppId { get; set; } = 0;
        public int IconId { get; set; } = 0;
        public DateTime CopyDateTime { get; set; }
        public int CopyCount { get; set; } = 0;
        public int PasteCount {
            get {
                return GetPasteCount();
            }
        }

        //public List<MpSubTextToken> SubTextTokenList { get; set; } = new List<MpSubTextToken>();

        public int RelevanceScore {
            get {
                return CopyCount + PasteCount;
            }
        }

        public string SourcePath { get; set; } = string.Empty;

        public string ItemPlainText { 
            get {
                switch(CopyItemType) {
                    case MpCopyItemType.FileList:
                        return (string)_itemData;
                    case MpCopyItemType.Image:
                        return "[Image]";
                    case MpCopyItemType.RichText:
                        return MpHelpers.ConvertRichTextToPlainText((string)_itemData);
                }
                return (string)_itemData;
            }
        }
        public string ItemRichText {
            get {
                switch (CopyItemType) {
                    case MpCopyItemType.FileList:
                        return MpHelpers.ConvertPlainTextToRichText((string)_itemData);
                    case MpCopyItemType.Image:
                        return "[Image]";
                    case MpCopyItemType.RichText:
                        return (string)_itemData;
                }
                return (string)_itemData;
            }
        }
        //public string ItemXaml { get; set; } = string.Empty;
        public MpEventEnabledFlowDocument ItemFlowDocument {
            get {
                switch (CopyItemType) {
                    case MpCopyItemType.FileList:
                        return MpHelpers.ConvertRichTextToFlowDocument(MpHelpers.ConvertPlainTextToRichText((string)_itemData));
                    case MpCopyItemType.Image:
                        return new MpEventEnabledFlowDocument();
                    case MpCopyItemType.RichText:
                        return MpHelpers.ConvertRichTextToFlowDocument((string)_itemData);
                }
                return new MpEventEnabledFlowDocument();
            }
        }
        public BitmapSource ItemBitmapSource {
            get {
                switch (CopyItemType) {
                    case MpCopyItemType.FileList:
                        return MpHelpers.ConvertRichTextToBitmapSource(MpHelpers.ConvertPlainTextToRichText((string)_itemData));
                    case MpCopyItemType.Image:
                        return (BitmapSource)_itemData;
                    case MpCopyItemType.RichText:
                        return MpHelpers.ConvertRichTextToBitmapSource((string)_itemData);
                }
                return new BitmapImage();
            }
        }

        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }

        //this is only set wheen ci is created to name app
        public IntPtr SourceHandle { get; set; } = IntPtr.Zero;
        #endregion

        #region Public Methods

        public MpCopyItem() {
            ItemColor = new MpColor(MpHelpers.GetRandomColor());
            App = new MpApp();
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
                    newCopyItem = MpCopyItem.CreateCopyItem(
                        MpCopyItemType.FileList, 
                        (string[])iData.GetData(DataFormats.FileDrop, true), 
                        sourcePath, 
                        itemColor,
                        processHandle);
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    newCopyItem = MpCopyItem.CreateCopyItem(
                        MpCopyItemType.RichText, 
                        (string)iData.GetData(DataFormats.Rtf), 
                        sourcePath, 
                        itemColor,
                        processHandle);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    newCopyItem = MpCopyItem.CreateCopyItem(
                        MpCopyItemType.Image, 
                        Clipboard.GetImage(), 
                        sourcePath, 
                        itemColor,
                        processHandle);
                } else if ((iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) && !string.IsNullOrEmpty((string)iData.GetData(DataFormats.Text))) {
                    newCopyItem = MpCopyItem.CreateCopyItem(
                        MpCopyItemType.RichText, 
                        MpHelpers.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.Text)), 
                        sourcePath, 
                        itemColor,
                        processHandle);
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
        
        public static MpCopyItem CreateCopyItem(
            MpCopyItemType itemType, 
            object data, 
            string sourcePath, 
            Color tileColor, 
            IntPtr hwnd) {
            MpCopyItem newItem = new MpCopyItem();
            newItem.SourceHandle = hwnd;
            newItem.SourcePath = sourcePath;
            newItem.CopyItemType = itemType;
            newItem.CopyDateTime = DateTime.Now;
            newItem.Title = "Untitled"; //Enum.GetName(typeof(MpCopyItemType), newItem.CopyItemType);
            newItem.CopyCount = 1;
            newItem.App = new MpApp(sourcePath, false, hwnd);
            newItem.AppId = newItem.App.AppId;
            newItem.Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            newItem.ClientId = newItem.Client.ClientId;
            newItem.ItemColor = new MpColor((int)tileColor.R, (int)tileColor.G, (int)tileColor.B, 255);

            switch (itemType) {
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                    }
                    newItem.SetData(paths);
                    //newItem.ItemPlainText = paths;
                    //newItem.ItemRichText = MpHelpers.ConvertPlainTextToRichText(newItem.ItemPlainText);
                    ////newItem.ItemXaml = MpHelpers.ConvertRichTextToXaml(newItem.ItemRichText);
                    //newItem.ItemFlowDocument = MpHelpers.COnvertRi(newItem.ItemXaml);
                    //newItem.ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(newItem.ItemRichText);
                    //newItem.SubTextTokenList = MpSubTextToken.GatherTokens(newItem.ItemFlowDocument);
                    break;
                case MpCopyItemType.Image:
                    newItem.SetData((BitmapSource)data);
                    //newItem.ItemBitmapSource = (BitmapSource)data;
                    //newItem.ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(newItem.ItemBitmapSource);
                    //newItem.ItemRichText = MpHelpers.ConvertPlainTextToRichText(newItem.ItemPlainText);
                    //newItem.ItemXaml = MpHelpers.ConvertRichTextToXaml(newItem.ItemRichText);
                    //newItem.ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(newItem.ItemXaml);
                    break;
                case MpCopyItemType.RichText:
                    newItem.SetData((string)data);
                    //newItem.ItemPlainText = MpHelpers.IsStringRichText((string)data) ? MpHelpers.ConvertRichTextToPlainText((string)data) : MpHelpers.IsStringXaml((string)data) ? MpHelpers.ConvertXamlToPlainText((string)data) : (string)data;
                    //newItem.ItemRichText = MpHelpers.IsStringRichText((string)data) ? (string)data : MpHelpers.IsStringXaml((string)data) ? MpHelpers.ConvertXamlToRichText((string)data) : MpHelpers.ConvertPlainTextToRichText((string)data);
                    //newItem.ItemXaml = MpHelpers.ConvertRichTextToXaml((string)data);// MpHelpers.IsStringRichText((string)data) ? MpHelpers.ConvertRichTextToXaml((string)data) : MpHelpers.IsStringXaml((string)data) ? (string)data : MpHelpers.ConvertPlainTextToXaml((string)data);
                    //newItem.ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(newItem.ItemXaml);
                    //newItem.ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(newItem.ItemRichText);
                    //newItem.SubTextTokenList = MpSubTextToken.GatherTokens(newItem.ItemFlowDocument);
                    break;
            }

            return newItem;
        }
        
        public static List<MpCopyItem> GetAllCopyItems() {
            List<MpCopyItem> clips = new List<MpCopyItem>();
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem", null);
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

        public void SetData(object data) {
            _itemData = data;
        }
        public object GetData() {
            return _itemData;
        }
        public int GetPasteCount() {
            if (CopyItemId <= 0) {
                return 0;
            }
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpPasteHistory where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            if (dt == null) {
                return 0;
            }
            return dt.Rows.Count;
        }

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
                    foreach (string f in GetFileList()) {
                        fileStr += f + Environment.NewLine;
                    }
                    foreach (string f in otherItem.GetFileList()) {
                        fileStr += f + Environment.NewLine;
                    }
                    SetData(fileStr);
                    //ItemPlainText += Environment.NewLine + fileStr;
                    //ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    //ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
                    //ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
                    //ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                    break;
                case MpCopyItemType.Image:
                    SetData(MpHelpers.CombineBitmap(new List<BitmapSource> { ItemBitmapSource, otherItem.ItemBitmapSource}));
                    //ItemBitmapSource = MpHelpers.CombineBitmap(new List<BitmapSource>() { ItemBitmapSource, otherItem.ItemBitmapSource });
                    //ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemBitmapSource);
                    //ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    //ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
                    //ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemRichText);
                    break;
                case MpCopyItemType.RichText:
                    SetData(MpHelpers.ConvertFlowDocumentToRichText(MpHelpers.CombineFlowDocuments(otherItem.ItemFlowDocument, ItemFlowDocument)));
                    //switch (otherItem.CopyItemType) {
                    //    case MpCopyItemType.Image:
                    //        MpHelpers.AppendBitmapSourceToFlowDocument(ItemFlowDocument, otherItem.ItemBitmapSource);
                    //        break;
                    //    default:
                    //        //var thisRange = MpHelpers.FindStringRangeFromPosition(ItemFlowDocument.ContentStart, ItemPlainText);
                    //        //ItemFlowDocument = MpHelpers.CombineFlowDocuments(otherItem.ItemFlowDocument, ItemFlowDocument);
                    //        //ItemXaml = MpHelpers.ConvertFlowDocumentToXaml(ItemFlowDocument);
                    //        //ItemRichText = MpHelpers.ConvertFlowDocumentToRichText(ItemFlowDocument);
                    //        //ItemPlainText = MpHelpers.ConvertRichTextToPlainText(ItemRichText);
                    //        //ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
                    //        //SubTextTokenList = MpSubTextToken.GatherTokens(ItemFlowDocument);
                    //        break;
                    //}
                    
                    break;
            }
        }

        public void ConvertType(MpCopyItemType newType) {
            return;
            //if (newType == CopyItemType) {
            //    return;
            //}
            
            //switch (newType) {
            //    case MpCopyItemType.FileList:
            //        switch(CopyItemType) {
            //            case MpCopyItemType.RichText:
            //                ItemPlainText = MpHelpers.WriteTextToFile(Path.GetTempFileName(), ItemPlainText, false);
            //                ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //                ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //                ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //                ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //                break;
            //            case MpCopyItemType.Image:
            //                ItemPlainText = MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource, false);
            //                ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //                ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //                ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //                ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //                break;
            //        }
            //        break;
            //    case MpCopyItemType.Image:
            //        switch (CopyItemType) {
            //            case MpCopyItemType.RichText:
            //                ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemBitmapSource);
            //                ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //                ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //                ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //                ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //                break;
            //            case MpCopyItemType.FileList:
            //                ItemPlainText = MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource, false);
            //                ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //                ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //                ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //                ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //                break;
            //        }
            //        break;
            //    case MpCopyItemType.RichText:
            //        switch (CopyItemType) {
            //            case MpCopyItemType.Image:
            //                ItemPlainText = MpHelpers.ConvertBitmapSourceToPlainText(ItemBitmapSource);
            //                ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //                ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //                ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //                ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //                break;
            //            case MpCopyItemType.FileList:
            //                ItemPlainText = MpHelpers.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource, false);
            //                ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //                ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //                ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //                ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //                break;
            //        }
            //        break;
            //}
            //CopyItemType = newType;
        }

        public void DeleteFromDatabase() {
            if (CopyItemId <= 0) {
                return;
            }
            MpDb.Instance.ExecuteWrite(
                "delete from MpPasteHistory where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItem where pk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpSubTextToken where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTag where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemSortTypeOrder where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
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
            SetData(dr["ItemText"].ToString()); 
            if(CopyItemType == MpCopyItemType.Image) {
                SetData(MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
            }
            //switch (CopyItemType) {
            //    case MpCopyItemType.FileList:
            //        ItemPlainText = (string)dr["ItemText"].ToString();
            //        ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //        ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //        ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //        ItemBitmapSource = MpHelpers.ConvertRichTextToBitmapSource(ItemRichText);
            //        break;
            //    case MpCopyItemType.Image:
            //        ItemBitmapSource = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]);
            //        ItemPlainText = (string)dr["ItemText"].ToString();
            //        ItemRichText = MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
            //        ItemXaml = MpHelpers.ConvertRichTextToXaml(ItemRichText);
            //        ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //        break;
            //    case MpCopyItemType.RichText:
            //        ItemXaml = dr["ItemText"].ToString();
            //        ItemRichText = MpHelpers.ConvertXamlToRichText(ItemXaml);
            //        ItemFlowDocument = MpHelpers.ConvertXamlToFlowDocument(ItemXaml);
            //        ItemPlainText = MpHelpers.ConvertRichTextToPlainText(ItemRichText);
            //        ItemBitmapSource = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]);
            //        SubTextTokenList = MpSubTextToken.GatherTokens(ItemFlowDocument);
            //        break;
            //}
             
            //get app and icon obj
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpApp where pk_MpAppId=@aid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@aid", AppId }
                    });
            if (dt != null && dt.Rows.Count > 0) {
                this.App = new MpApp(dt.Rows[0]);
            } else {
                Console.WriteLine("MpCopyItem Error: error retrieving MpApp with id " + AppId);
            }

            //get subtokens
            dt = MpDb.Instance.Execute(
                "select * from MpSubTextToken where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            //if (dt != null && dt.Rows.Count > 0) {
            //    foreach (DataRow row in dt.Rows) {
            //        SubTextTokenList.Add(new MpSubTextToken(row));
            //    }
            //} else {
            //    //copyitem not req'd to have subtokens
            //}

            //get color
            dt = MpDb.Instance.Execute(
                "select * from MpColor where pk_MpColorId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@cid", ColorId }
                    });
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
                App = new MpApp(SourcePath, false, SourceHandle);
                this.AppId = App.AppId;
            }
            if (this.AppId == 0) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpApp where pk_MpAppId=@aid",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@aid", AppId }
                        });
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
                MpDb.Instance.ExecuteWrite(
                        "update MpCopyItem set fk_MpCopyItemTypeId=@citd, fk_MpClientId=@cid, fk_MpAppId=@aid, fk_MpColorId=@clrId, Title=@t, CopyCount=@cc, ItemText=@it, ItemImage=@ii where pk_MpCopyItemId=@ciid",
                        new Dictionary<string, object> {
                            { "@citd", (int)CopyItemType },
                            { "@cid", ClientId },
                            { "@aid", AppId },
                            { "@clrId", ColorId },
                            { "@t", Title },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", itemImage},
                            { "@ciid", CopyItemId},
                        });
                isNew = false;
            } else {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpCopyItem(fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount,ItemText,ItemImage) " + 
                    "values (@citd,@cid,@aid,@clrId,@t,@cdt,@cc,@it,@ii)",
                    new Dictionary<string, object> {
                            { "@citd", (int)CopyItemType },
                            { "@cid", ClientId },
                            { "@aid", AppId },
                            { "@clrId", ColorId },
                            { "@t", Title },
                            { "@cdt", CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", itemImage},
                            { "@ciid", CopyItemId},
                        });
                this.CopyItemId = MpDb.Instance.GetLastRowId("MpCopyItem", "pk_MpCopyItemId");
                isNew = true;
            }
            //foreach (MpSubTextToken subToken in SubTextTokenList) {
            //    subToken.CopyItemId = CopyItemId;
            //    subToken.WriteToDatabase();
            //}

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
