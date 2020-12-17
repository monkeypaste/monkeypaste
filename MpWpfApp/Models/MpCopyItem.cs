using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpCopyItem : MpDbObject, ICloneable {
        #region Private Variables
        private static int _CopyItemCount = 0;
        private object _itemData = null;
        private string _imageItemObjectSeperator = @"?";
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

        public int RelevanceScore {
            get {
                return CopyCount + PasteCount;
            }
        }

        public string ImageObjectTypeCsv {
            get {
                var outStr = string.Empty;
                foreach (var obj in ImageItemObjectList) {
                    outStr += obj.TypeCsv + ",";
                }
                return string.IsNullOrEmpty(outStr) ? outStr : outStr.Remove(0, 1);
            }
        }

        public string SourcePath { get; set; } = string.Empty;

        //public string ItemMetaCsv { get; set; } = string.Empty;

        public string ItemPlainText { 
            get {
                switch(CopyItemType) {
                    case MpCopyItemType.FileList:
                        return (string)_itemData;
                    case MpCopyItemType.Image:
                        return ImageObjectTypeCsv;
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
                        return MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    case MpCopyItemType.Image:
                        return MpHelpers.ConvertPlainTextToRichText(ItemPlainText);
                    case MpCopyItemType.RichText:
                        return (string)_itemData;
                }
                return (string)_itemData;
            }
        }

        public MpEventEnabledFlowDocument ItemFlowDocument {
            get {
                switch (CopyItemType) {
                    case MpCopyItemType.FileList:
                        return MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
                    case MpCopyItemType.Image:
                        return MpHelpers.ConvertRichTextToFlowDocument(ItemRichText);
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

        public BitmapSource ItemTitleSwirl { get; set; }

        public List<MpDetectedImageObject> ImageItemObjectList = new List<MpDetectedImageObject>();

        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }

        //this is only set wheen ci is created to name app
        public IntPtr SourceHandle { get; set; } = IntPtr.Zero;
        #endregion

        #region Public Methods

        public MpCopyItem() {
            ItemColor = new MpColor(MpHelpers.GetRandomColor());
            ItemTitleSwirl = InitSwirl();
            App = new MpApp();
            _itemData = "Default";
            CopyItemType = MpCopyItemType.RichText;
            ImageItemObjectList = new List<MpDetectedImageObject>();
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
                        MpHelpers.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.UnicodeText)), 
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
            IntPtr hwnd,
            string appName = "") {
            MpCopyItem newItem = new MpCopyItem();
            newItem.SourceHandle = hwnd;
            newItem.SourcePath = sourcePath;
            newItem.CopyItemType = itemType;
            newItem.CopyDateTime = DateTime.Now;
            newItem.Title = "Untitled"; 
            newItem.CopyCount = 1;
            newItem.App = hwnd == IntPtr.Zero ? new MpApp(sourcePath,false,appName) : new MpApp(sourcePath, false, hwnd);
            newItem.AppId = newItem.App.AppId;
            newItem.Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            newItem.ClientId = newItem.Client.ClientId;
            newItem.ItemColor = new MpColor((int)tileColor.R, (int)tileColor.G, (int)tileColor.B, 255);
            newItem.ItemTitleSwirl = newItem.InitSwirl();
            newItem.ImageItemObjectList = new List<MpDetectedImageObject>();

            switch (itemType) {
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                    }
                    newItem.SetData(paths);
                    break;
                case MpCopyItemType.Image:
                    newItem.SetData((BitmapSource)data);
                    newItem.ImageItemObjectList = MpHelpers.DetectObjects(MpHelpers.ConvertBitmapSourceToByteArray(newItem.ItemBitmapSource));
                    
                    Console.WriteLine("Image metadata: ");
                    foreach(var iio in newItem.ImageItemObjectList) {
                        Console.WriteLine(iio);
                    }
                    break;
                case MpCopyItemType.RichText:
                    newItem.SetData((string)data);
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
            WriteToDatabase();
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
                    break;
                case MpCopyItemType.Image:
                    SetData(
                        MpHelpers.CombineBitmap(
                            new List<BitmapSource> { 
                                ItemBitmapSource, 
                                otherItem.ItemBitmapSource}));
                    break;
                case MpCopyItemType.RichText:
                    SetData(
                        MpHelpers.ConvertFlowDocumentToRichText(
                            MpHelpers.CombineFlowDocuments(
                                otherItem.ItemFlowDocument, 
                                ItemFlowDocument)));
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
                "delete from MpDetectedImageObject where fk_MpCopyItemId=@ciid",
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
        
        public BitmapSource InitSwirl(BitmapSource sharedSwirl = null) {
            if (sharedSwirl == null) {
                var itemBrush = new SolidColorBrush() { Color = ItemColor.Color };
                SolidColorBrush lighterColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness(itemBrush, -0.5f), 100);
                SolidColorBrush darkerColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness(itemBrush, -0.4f), 50);
                SolidColorBrush accentColor = MpHelpers.ChangeBrushAlpha(
                                MpHelpers.ChangeBrushBrightness(itemBrush, -0.0f), 100);
                var path = @"pack://application:,,,/Resources/Images/";
                var swirl1 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0001.png"));
                swirl1 = MpHelpers.TintBitmapSource(swirl1, (itemBrush).Color);

                var swirl2 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0002.png"));
                swirl2 = MpHelpers.TintBitmapSource(swirl2, lighterColor.Color);

                var swirl3 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0003.png"));
                swirl3 = MpHelpers.TintBitmapSource(swirl3, darkerColor.Color);

                var swirl4 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0004.png"));
                swirl4 = MpHelpers.TintBitmapSource(swirl4, accentColor.Color);

                return MpHelpers.MergeImages(new List<BitmapSource>() { swirl1, swirl2, swirl3, swirl4 });
            } else {
                return sharedSwirl;
            }
        }

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
            columnData.Add("ItemText",this.ItemRichText);
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
            this.ItemTitleSwirl = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["TitleSwirl"]);

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


            if (CopyItemType == MpCopyItemType.Image) {
                SetData(MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
                ImageItemObjectList = MpDetectedImageObject.GetAllObjectsForItem(CopyItemId);
            } else {
                SetData(dr["ItemText"].ToString());
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
                        "update MpCopyItem set TitleSwirl=@ts, fk_MpCopyItemTypeId=@citd, fk_MpClientId=@cid, fk_MpAppId=@aid, fk_MpColorId=@clrId, Title=@t, CopyCount=@cc, ItemText=@it, ItemImage=@ii where pk_MpCopyItemId=@ciid",
                        new Dictionary<string, object> {
                            { "@ts", MpHelpers.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
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
                foreach (var imgObj in ImageItemObjectList) {
                    imgObj.WriteToDatabase();
                }
                isNew = false;
            } else {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpCopyItem(TitleSwirl,fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount,ItemText,ItemImage) " + 
                    "values (@ts,@citd,@cid,@aid,@clrId,@t,@cdt,@cc,@it,@ii)",
                    new Dictionary<string, object> {
                            { "@ts", MpHelpers.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
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
                foreach(var imgObj in ImageItemObjectList) {
                    imgObj.CopyItemId = CopyItemId;
                    imgObj.WriteToDatabase();
                }
                isNew = true;
            }

            MapDataToColumns();
            Console.WriteLine(isNew ? "Created " : "Updated " + " MpCopyItem");
            Console.WriteLine(ToString());
        }

        public object Clone() {
            return MpCopyItem.CreateCopyItem(
                CopyItemType,
                _itemData,
                App.AppPath,
                ItemColor.Color,
                IntPtr.Zero,
                App.AppName);
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
