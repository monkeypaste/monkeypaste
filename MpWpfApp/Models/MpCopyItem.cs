using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpCopyItem : MpDbObject, ICloneable {
        #region Private Variables
        private static int _CopyItemCount = 0;
        private object _itemData = null;
        private string _imageItemObjectSeperator = @"?";

        private static List<MpApp> _AppList = new List<MpApp>();
        private static List<MpColor> _ColorList = new List<MpColor>();
        private static List<MpCopyItem> _CopyItemList = null;

        #endregion

        #region Properties

        public static List<MpCopyItem> CopyItemList {
            get {
                if(_CopyItemList == null) {
                    _CopyItemList = MpCopyItem.GetAllCopyItems();
                }
                return _CopyItemList;
            }
        }

        public int CopyItemId { get; set; } = 0;
        public int PreCopyItemId { get; set; } = 0;
        public int PostCopyItemId { get; set; } = 0;
        public MpApp App { get; set; }
        public MpClient Client { get; set; }
        public MpColor ItemColor { get; set; }
        //public int ColorId { get; private set; } = 1;
        public string Title { get; set; } = "Untitled";
        public MpCopyItemType CopyItemType { get; private set; } = MpCopyItemType.None;
        //public int ClientId { get; private set; } = 0;
        //public int AppId { get; set; } = 0;
        //public int IconId { get; set; } = 0;
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
                    outStr += obj.ObjectTypeName + ",";
                }
                return string.IsNullOrEmpty(outStr) ? outStr : outStr.Remove(0, 1);
            }
        }

        //public string SourcePath { get; set; } = string.Empty;

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

        public BitmapSource ItemBitmapSource {
            get {
                switch (CopyItemType) {
                    //case MpCopyItemType.FileList:
                    //    return MpHelpers.ConvertRichTextToBitmapSource(MpHelpers.ConvertPlainTextToRichText((string)_itemData));
                    case MpCopyItemType.Image:
                        return (BitmapSource)_itemData;
                    //case MpCopyItemType.RichText:
                    //    return MpHelpers.ConvertRichTextToBitmapSource((string)_itemData);
                }
                return new BitmapImage();
            }
        }

        public BitmapSource ItemTitleSwirl { get; set; }

        public List<MpDetectedImageObject> ImageItemObjectList = new List<MpDetectedImageObject>();

        public List<MpCopyItemTemplate> TemplateList {
            get {
                return MpCopyItemTemplate.GetAllTemplatesForCopyItem(CopyItemId);
            }
        }

        public string TemplateRegExMatchString {
            get {
                var outStr = string.Empty;
                foreach(var t in TemplateList) {
                    outStr += t.TemplateName + "|";
                }
                if(!string.IsNullOrEmpty(outStr)) {
                    return outStr.Remove(outStr.Length - 1, 1);
                }
                return outStr;
            }
        }


        #endregion

        #region Static Methods
        public static MpCopyItem CreateFromClipboard(IntPtr processHandle) {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null) {
                return null;
            }
            Color itemColor = MpHelpers.GetRandomColor();
            object itemData = null;
            MpCopyItemType itemType = MpCopyItemType.None;
            try {
                if (iData.GetDataPresent(DataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = (string[])iData.GetData(DataFormats.FileDrop, true);                    
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    itemType = MpCopyItemType.RichText;
                    itemData = (string)iData.GetData(DataFormats.Rtf);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = Clipboard.GetImage();
                } else if ((iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) && !string.IsNullOrEmpty((string)iData.GetData(DataFormats.Text))) {
                    itemType = MpCopyItemType.RichText;
                    itemData = MpHelpers.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.UnicodeText));
                } else {
                    Console.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }
                if(Properties.Settings.Default.IgnoreNewDuplicates) {
                    var dupItem = MpCopyItem.GetCopyItemByData(itemData);
                    if(dupItem != null) {
                        return dupItem;
                    }
                }
                return new MpCopyItem(itemType, itemData, itemColor, processHandle);
            }
            catch (Exception e) {
                //this catches intermittent COMExceptions (happened copy/pasting in Excel)
                Console.WriteLine("Caught exception creating copyitem (will reattempt to open clipboard): " + e.ToString());
                return CreateFromClipboard(processHandle);
            }
        }        

        public static MpCopyItem CreateRandomItem(MpCopyItemType itemType) {
            switch(itemType) {
                case MpCopyItemType.RichText:
                    var ci =  new MpCopyItem(
                        MpCopyItemType.RichText,
                        MpHelpers.ConvertPlainTextToRichText(
                            MpHelpers.GetRandomString(80, MpHelpers.Rand.Next(1, 100))),
                        MpHelpers.GetRandomColor(),
                        new WindowInteropHelper(Application.Current.MainWindow).Handle);//((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.ThisAppHandle);
                    ci.WriteToDatabase();
                    return ci;
                    

            }
            return null;
        }

        public static List<MpCopyItem> GetAllCopyItems() {
            _AppList = MpApp.GetAllApps();
            _ColorList = MpColor.GetAllColors();

            List<MpCopyItem> clips = new List<MpCopyItem>();
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItem", null);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    clips.Add(new MpCopyItem(dr));
                }
            }
            return clips;
        }

        public static MpCopyItem GetCopyItemByData(object data) {
            if (data.GetType() == typeof(string)) {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpCopyItem where ItemText=@it",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@it", (string)data }
                        });
                if (dt != null && dt.Rows.Count > 0) {
                    return new MpCopyItem(dt.Rows[0]);
                }
                return null;
            } else {
                DataTable dt = MpDb.Instance.Execute(
                    "select * from MpCopyItem where ItemImage=@ii",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@ii", MpHelpers.ConvertBitmapSourceToByteArray((BitmapSource)data) }
                        });
                if (dt != null && dt.Rows.Count > 0) {
                    return new MpCopyItem(dt.Rows[0]);
                }
            }
            return null;
        }
        #endregion

        #region Public Methods

        public MpCopyItem() {
            ItemColor = new MpColor(MpHelpers.GetRandomColor());
            Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            Title = "Loading";
            ItemTitleSwirl = new BitmapImage();
            CopyDateTime = DateTime.Now;
            App = new MpApp();
            _itemData = "Default";
            CopyItemType = MpCopyItemType.RichText;
            ImageItemObjectList = new List<MpDetectedImageObject>();
        }
        private MpCopyItem(
            MpCopyItemType itemType,
            object data,
            Color tileColor,
            IntPtr hwnd) {
            CopyItemType = itemType;
            CopyDateTime = DateTime.Now;
            Title = "Untitled";
            CopyCount = 1;
            Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            
            var appPath = MpHelpers.GetProcessPath(hwnd);
            var app = _AppList.Where(x => x.AppPath == appPath).ToList();
            if(app == null || app.Count == 0) {
                App = new MpApp(false, hwnd);
                _AppList.Add(App);
            } else {
                App = app[0];
            }

            var color = _ColorList.Where(x => x.Color == tileColor).ToList();
            if(color == null || color.Count == 0) {
                ItemColor = new MpColor(tileColor);
                _ColorList.Add(ItemColor);
            } else {
                ItemColor = color[0];
            }

            ItemTitleSwirl = InitSwirl();
            ImageItemObjectList = new List<MpDetectedImageObject>();

            switch (itemType) {
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                    }
                    SetData(paths);
                    break;
                case MpCopyItemType.Image:
                    SetData((BitmapSource)data);
                    ImageItemObjectList = MpHelpers.DetectObjects(MpHelpers.ConvertBitmapSourceToByteArray(ItemBitmapSource));

                    //Console.WriteLine("Image metadata: ");
                    //foreach (var iio in ImageItemObjectList) {
                    //    Console.WriteLine(iio);
                    //}
                    break;
                case MpCopyItemType.RichText:
                    SetData((string)data);
                    break;
            }
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

        public MpCopyItemTemplate GetTemplateByName(string templateName) {
            foreach(var t in TemplateList) {
                if(t.TemplateName == templateName) {
                    return t;
                }
            }
            return null;
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
                        MpHelpers.CombineRichText(
                                otherItem.ItemRichText,
                                ItemRichText));
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
                "delete from MpCopyItemTag where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemSortTypeOrder where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            foreach(var cit in TemplateList) {
                cit.DeleteFromDatabase();
            }
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

        #endregion

        #region Overrides

        public override void LoadDataRow(DataRow dr) {
            CopyItemId = Convert.ToInt32(dr["pk_MpCopyItemId"].ToString());
            CopyItemType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            int clientId = Convert.ToInt32(dr["fk_MpClientId"].ToString());
            int appId = Convert.ToInt32(dr["fk_MpAppId"].ToString());
            int colorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            CopyDateTime = DateTime.Parse(dr["CopyDateTime"].ToString());
            Title = dr["Title"].ToString();
            CopyCount = Convert.ToInt32(dr["CopyCount"].ToString());
            ItemTitleSwirl = MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["TitleSwirl"]);

            Client = new MpClient(0, 0, MpHelpers.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            App = _AppList.Where(x => x.AppId == appId).ToList()[0];
            ItemColor = _ColorList.Where(x => x.ColorId == colorId).ToList()[0];

            if (CopyItemType == MpCopyItemType.Image) {
                SetData(MpHelpers.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
                ImageItemObjectList = MpDetectedImageObject.GetAllObjectsForItem(CopyItemId);
            } else {
                SetData(dr["ItemText"].ToString());
            }
        }
        
        // still req'd if NoDb=true
        public override void WriteToDatabase() {
            App.WriteToDatabase();
            ItemColor.WriteToDatabase();

            string itemText = CopyItemType == MpCopyItemType.RichText ? ItemRichText : ItemPlainText;
            byte[] itemImage = CopyItemType == MpCopyItemType.Image ? MpHelpers.ConvertBitmapSourceToByteArray(ItemBitmapSource) : null;
            //if copyitem already exists
            if (CopyItemId > 0) {
                MpDb.Instance.ExecuteWrite(
                        "update MpCopyItem set TitleSwirl=@ts, fk_MpCopyItemTypeId=@citd, fk_MpClientId=@cid, fk_MpAppId=@aid, fk_MpColorId=@clrId, Title=@t, CopyCount=@cc, ItemText=@it, ItemImage=@ii where pk_MpCopyItemId=@ciid",
                        new Dictionary<string, object> {
                            { "@ts", MpHelpers.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
                            { "@citd", (int)CopyItemType },
                            { "@cid", Client.ClientId },
                            { "@aid", App.AppId },
                            { "@clrId", ItemColor.ColorId },
                            { "@t", Title },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", itemImage},
                            { "@ciid", CopyItemId},
                        });
            } else {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpCopyItem(TitleSwirl,fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount,ItemText,ItemImage) " + 
                    "values (@ts,@citd,@cid,@aid,@clrId,@t,@cdt,@cc,@it,@ii)",
                    new Dictionary<string, object> {
                            { "@ts", MpHelpers.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
                            { "@citd", (int)CopyItemType },
                            { "@cid", Client.ClientId },
                            { "@aid", App.AppId },
                            { "@clrId", ItemColor.ColorId },
                            { "@t", Title },
                            { "@cdt", CopyDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@cc", CopyCount },
                            { "@it", itemText },
                            { "@ii", itemImage},
                            { "@ciid", CopyItemId},
                        });
                CopyItemId = MpDb.Instance.GetLastRowId("MpCopyItem", "pk_MpCopyItemId");  
            }
            foreach (var imgObj in ImageItemObjectList) {
                imgObj.CopyItemId = CopyItemId;
                imgObj.WriteToDatabase();
            }
            foreach(var cit in TemplateList) {
                cit.CopyItemId = CopyItemId;
                cit.WriteToDatabase();
            }
        }

        public object Clone() {
            return new MpCopyItem(
                CopyItemType,
                _itemData,
                ItemColor.Color,
                IntPtr.Zero);
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
