using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage;

namespace MpWpfApp {
    public class MpCopyItem : MpDbObject, ICloneable {
        #region Private Variables
        private static int _CopyItemCount = 0;
        private object _itemData = null;

        private static List<MpApp> _AppList = null;
        private static List<MpColor> _ColorList = null;

        #endregion

        #region Properties

        public int CopyItemId { get; set; } = 0;

        public int PreCopyItemId { get; set; } = 0;

        public int PostCopyItemId { get; set; } = 0;

        public MpApp App { get; set; }

        public MpClient Client { get; set; }

        public MpColor ItemColor { get; set; }

        public string Title { get; set; } = string.Empty;

        public MpCopyItemType CopyItemType { get; private set; } = MpCopyItemType.None;

        //public int ClientId { get; private set; } = 0;
        //public int AppId { get; set; } = 0;
        //public int IconId { get; set; } = 0;

        public DateTime CopyDateTime { get; set; }

        public int LineCount { get; set; } = 0;

        public int CharCount { get; set; } = 0;

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

        public int FileCount { get; set; } = 0;

        public long DataSizeInMb { get; set; } = 0;

        public int RelevanceScore {
            get {
                return CopyCount + PasteCount;
            }
        }
                
        public string ItemPlainText { get; set; }

        public string ItemRichText {
            get {
                switch (CopyItemType) {
                    case MpCopyItemType.FileList:
                        return MpHelpers.Instance.ConvertPlainTextToRichText(ItemPlainText);
                    case MpCopyItemType.Image:
                        return MpHelpers.Instance.ConvertPlainTextToRichText(ItemPlainText);
                    case MpCopyItemType.RichText:
                        return (string)_itemData;
                }
                return (string)_itemData;
            }
        }

        private string _itemCsv = string.Empty;
        public string ItemCsv { 
            get {
                if(CopyItemType == MpCopyItemType.Image) {
                    var outStr = string.Empty;
                    foreach (var obj in ImageItemObjectList) {
                        outStr += obj.ObjectTypeName + ",";
                    }
                    return string.IsNullOrEmpty(outStr) ? outStr : outStr.Remove(outStr.Length-1, 1);
                }
                return _itemCsv;
            }
            set {
                _itemCsv = value;
            }
        }

        public BitmapSource ItemBitmapSource {
            get {
                switch (CopyItemType) {
                    //case MpCopyItemType.FileList:
                    //    return MpHelpers.Instance.ConvertRichTextToBitmapSource(MpHelpers.Instance.ConvertPlainTextToRichText((string)_itemData));
                    case MpCopyItemType.Image:
                        return (BitmapSource)_itemData;
                    //case MpCopyItemType.RichText:
                    //    return MpHelpers.Instance.ConvertRichTextToBitmapSource((string)_itemData);
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

        public bool HasOcrText {
            get {
                if(CopyItemType != MpCopyItemType.Image) {
                    return false;
                }
                if((ImageItemObjectList == null || ImageItemObjectList.Count == 0) && !string.IsNullOrEmpty(ItemPlainText)) {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Static Methods
        public static async Task<MpCopyItem> CreateFromClipboardAsync(IntPtr processHandle) {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null) {
                return null;
            }
            Color itemColor = MpHelpers.Instance.GetRandomColor();
            object itemData = null;
            MpCopyItemType itemType = MpCopyItemType.None;
            try {
                if (iData.GetDataPresent(DataFormats.FileDrop)) {
                    itemType = MpCopyItemType.FileList;
                    itemData = (string[])iData.GetData(DataFormats.FileDrop, true);                    
                } else if (iData.GetDataPresent(DataFormats.CommaSeparatedValue)) {
                    itemType = MpCopyItemType.Csv;
                    itemData = (string)iData.GetData(DataFormats.CommaSeparatedValue);
                } else if (iData.GetDataPresent(DataFormats.Rtf)) {
                    itemType = MpCopyItemType.RichText; 
                    itemData = (string)iData.GetData(DataFormats.Rtf);
                } else if (iData.GetDataPresent(DataFormats.Bitmap)) {
                    itemType = MpCopyItemType.Image;
                    itemData = (BitmapSource)Clipboard.GetImage();
                } else if ((iData.GetDataPresent(DataFormats.Html) || iData.GetDataPresent(DataFormats.Text)) && !string.IsNullOrEmpty((string)iData.GetData(DataFormats.Text))) {
                    itemType = MpCopyItemType.RichText;
                    itemData = MpHelpers.Instance.ConvertPlainTextToRichText((string)iData.GetData(DataFormats.UnicodeText));
                } else {
                    Console.WriteLine("MpData error clipboard data is not known format");
                    return null;
                }
                if (itemType == MpCopyItemType.RichText && ((string)itemData).Length > Properties.Settings.Default.MaxRtfCharCount) {
                    itemData = MpHelpers.Instance.ConvertRichTextToPlainText((string)itemData);
                    if (((string)itemData).Length > Properties.Settings.Default.MaxRtfCharCount) {
                        //item is TOO LARGE so ignore
                        return null;
                    }
                }
                if (Properties.Settings.Default.IgnoreNewDuplicates) {
                    var dupItem = MpCopyItem.GetCopyItemByData(itemData);
                    if(dupItem != null) {
                        return dupItem;
                    }
                }

                var newItem = new MpCopyItem();
                newItem.CopyItemType = itemType;
                newItem.CopyDateTime = DateTime.Now;
                newItem.Title = newItem.CopyItemType == MpCopyItemType.RichText || newItem.CopyItemType == MpCopyItemType.Csv ? "Text" : Enum.GetName(typeof(MpCopyItemType), newItem.CopyItemType);
                newItem.CopyCount = 1;
                newItem.Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);

                var appPath = MpHelpers.Instance.GetProcessPath(processHandle);
                var app = _AppList.Where(x => x.AppPath == appPath).ToList();
                if (app == null || app.Count == 0) {
                    newItem.App = new MpApp(false, processHandle);
                    _AppList.Add(newItem.App);
                } else {
                    newItem.App = app[0];
                }

                var color = _ColorList.Where(x => x.Color == itemColor).ToList();
                if (color == null || color.Count == 0) {
                    newItem.ItemColor = new MpColor(itemColor);
                    _ColorList.Add(newItem.ItemColor);
                } else {
                    newItem.ItemColor = color[0];
                }

                newItem.ItemTitleSwirl = newItem.InitSwirl();
                newItem.ImageItemObjectList = new List<MpDetectedImageObject>();
                switch (itemType) {
                    case MpCopyItemType.Csv:
                        newItem.ItemCsv = (string)itemData;
                        newItem.CopyItemType = MpCopyItemType.RichText;
                        newItem.SetData(MpHelpers.Instance.ConvertPlainTextToRichText(newItem.ItemCsv));
                        break;
                    case MpCopyItemType.FileList:
                        string paths = string.Empty;
                        foreach (string str in (string[])itemData) {
                            paths += str + Environment.NewLine;
                        }
                        newItem.SetData(paths);
                        break;
                    case MpCopyItemType.Image:
                        newItem.SetData((BitmapSource)itemData);
                        break;
                    case MpCopyItemType.RichText:
                        newItem.SetData((string)itemData);
                        break;
                }
                return newItem;
            }
            catch (Exception e) {
                //this catches intermittent COMExceptions (happened copy/pasting in Excel)
                Console.WriteLine("Caught exception creating copyitem (will reattempt to open clipboard): " + e.ToString());
                return await CreateFromClipboardAsync(processHandle);
            }
        }        

        public static MpCopyItem CreateRandomItem(MpCopyItemType itemType) {
            switch(itemType) {
                case MpCopyItemType.RichText:
                    var ci =  new MpCopyItem(
                        MpCopyItemType.RichText,
                        MpHelpers.Instance.ConvertPlainTextToRichText(
                            MpHelpers.Instance.GetRandomString(80, MpHelpers.Instance.Rand.Next(1, 100))),
                        MpHelpers.Instance.GetRandomColor(),
                        new WindowInteropHelper(Application.Current.MainWindow).Handle);//((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).MainWindowViewModel.ClipTrayViewModel.ClipboardManager.LastWindowWatcher.ThisAppHandle);
                    //ci.WriteToDatabase();
                    return ci;
                    

            }
            return null;
        }

        public static List<MpCopyItem> GetAllCopyItems() {
            _AppList = MpApp.GetAllApps();
            _ColorList = MpColor.GetAllColors();

            var clips = new List<MpCopyItem>();
            var dt = MpDb.Instance.Execute("select * from MpCopyItem order by CopyDateTime DESC", null);
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
                            { "@ii", MpHelpers.Instance.ConvertBitmapSourceToByteArray((BitmapSource)data) }
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
            ItemColor = new MpColor(MpHelpers.Instance.GetRandomColor());
            Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            Title = "Loading";
            ItemTitleSwirl = new BitmapImage();
            CopyDateTime = DateTime.Now;
            App = new MpApp();
            _itemData = "Default";
            CopyItemType = MpCopyItemType.RichText;
            ImageItemObjectList = new List<MpDetectedImageObject>();
        }

        //public MpCopyItem() {

        //}

        public MpCopyItem(MpCopyItemType type, string title, object data) : this(type, data, MpHelpers.Instance.GetRandomColor(), ((HwndSource)PresentationSource.FromVisual(Application.Current.MainWindow)).Handle) { }
        
        private MpCopyItem(
            MpCopyItemType itemType,
            object data,
            Color tileColor,
            IntPtr hwnd) {
            CopyItemType = itemType;
            CopyDateTime = DateTime.Now;
            Title = CopyItemType == MpCopyItemType.RichText || CopyItemType == MpCopyItemType.Csv ? "Text" : Enum.GetName(typeof(MpCopyItemType), CopyItemType);
            CopyCount = 1;
            Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            
            var appPath = MpHelpers.Instance.GetProcessPath(hwnd);
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
                case MpCopyItemType.Csv:
                    ItemCsv = (string)data;
                    CopyItemType = MpCopyItemType.RichText;
                    SetData(MpHelpers.Instance.ConvertPlainTextToRichText(ItemCsv));
                    break;
                case MpCopyItemType.FileList:
                    string paths = string.Empty;
                    foreach (string str in (string[])data) {
                        paths += str + Environment.NewLine;
                    }
                    SetData(paths);
                    break;
                case MpCopyItemType.Image:
                    SetData((BitmapSource)data);
                    break;
                case MpCopyItemType.RichText:                    
                    SetData((string)data);
                    break;
            }
        }

        public MpCopyItem(DataRow dr) {
            LoadDataRow(dr);
        }

        public async Task SetData(object data) {
            _itemData = data;
            await UpdateItemData();
            UpdateDetails();
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
                    fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(Path.GetTempFileName(), ItemBitmapSource));
                } else if (forceType == MpCopyItemType.RichText) {
                    fileList.Add(MpHelpers.Instance.WriteTextToFile(Path.GetTempFileName(), ItemPlainText));
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
                string fn = MpHelpers.Instance.RemoveSpecialCharacters(Title.Trim());
                if (string.IsNullOrEmpty(fn)) {
                    fn = Path.GetRandomFileName();
                }
                string fe = CopyItemType == MpCopyItemType.RichText ? ".txt" : ".png";
                fe = forceType == MpCopyItemType.RichText ? ".txt" : fe;
                fe = forceType == MpCopyItemType.Image ? ".png" : fe;
                string op = MpHelpers.Instance.GetUniqueFileName(fp + fn + fe);
                //file extension
                switch (CopyItemType) {
                    case MpCopyItemType.RichText:
                        if(forceType == MpCopyItemType.Image) {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, ItemBitmapSource));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, ItemPlainText));
                        }
                        break;
                    case MpCopyItemType.Image:
                        if (forceType == MpCopyItemType.RichText) {
                            fileList.Add(MpHelpers.Instance.WriteTextToFile(op, ItemPlainText));
                        } else {
                            fileList.Add(MpHelpers.Instance.WriteBitmapSourceToFile(op, ItemBitmapSource));
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
                        MpHelpers.Instance.CombineBitmap(
                            new List<BitmapSource> { 
                                ItemBitmapSource, 
                                otherItem.ItemBitmapSource}));
                    break;
                case MpCopyItemType.RichText:
                    SetData(
                        MpHelpers.Instance.CombineRichText(
                                ItemRichText,
                            otherItem.ItemRichText,
                                true));
                    break;
            }
        }

        public BitmapSource InitSwirl(BitmapSource sharedSwirl = null, bool forceUseItemColor = false) {
            if (sharedSwirl == null) {
                var path = @"pack://application:,,,/Resources/Images/";
                var swirlList = new List<BitmapSource>();
                if (forceUseItemColor) {
                    var itemBrush = new SolidColorBrush() { Color = ItemColor.Color };
                    SolidColorBrush lighterColor = MpHelpers.Instance.ChangeBrushAlpha(
                                    MpHelpers.Instance.ChangeBrushBrightness(itemBrush, -0.5f), 100);
                    SolidColorBrush darkerColor = MpHelpers.Instance.ChangeBrushAlpha(
                                    MpHelpers.Instance.ChangeBrushBrightness(itemBrush, -0.4f), 50);
                    SolidColorBrush accentColor = MpHelpers.Instance.ChangeBrushAlpha(
                                    MpHelpers.Instance.ChangeBrushBrightness(itemBrush, -0.0f), 100);

                    var swirl1 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0001.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl1, (itemBrush).Color));

                    var swirl2 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0002.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl2, lighterColor.Color));

                    var swirl3 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0003.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl3, darkerColor.Color));

                    var swirl4 = (BitmapSource)new BitmapImage(new Uri(path + "title_swirl0004.png"));
                    swirlList.Add(MpHelpers.Instance.TintBitmapSource(swirl4, accentColor.Color));
                } else {
                    var randomColorList = MpHelpers.Instance.GetRandomizedList<MpColor>(App.PrimaryIconColorList);
                    for (int i = 0; i < 4; i++) {
                        var c = randomColorList[i].Color;
                        c.A = (byte)MpHelpers.Instance.Rand.Next(40,120);
                        var swirl = (BitmapSource)new BitmapImage(new Uri(path + string.Format(@"title_swirl000{0}.png", i + 1)));
                        swirl = MpHelpers.Instance.TintBitmapSource(swirl, c);
                        swirlList.Add(swirl);
                    }
                }
                //randomize order of layers
                var rsl = MpHelpers.Instance.GetRandomizedList<BitmapSource>(swirlList);
                return MpHelpers.Instance.MergeImages(new List<BitmapSource>() { rsl[0], rsl[1], rsl[2], rsl[3] });
            } else {
                return sharedSwirl;
            }
        }
        #endregion

        #region Private Methods

        private async Task UpdateItemData() {
            switch (CopyItemType) {
                case MpCopyItemType.FileList:
                    ItemPlainText = (string)_itemData;
                    break;
                case MpCopyItemType.Image:
                    if(Application.Current.MainWindow.DataContext == null || 
                       ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).IsLoading) {
                        //when loading from database
                        ImageItemObjectList = MpDetectedImageObject.GetAllObjectsForItem(CopyItemId);
                    } else {
                        //resize image for scanning to 500x500 regardless of original resolution                        
                        //var maxSize = new Size(500, 500);
                        //double scale = Math.Min(maxSize.Width / ItemBitmapSource.Width, maxSize.Height / ItemBitmapSource.Height);
                        //var scanBmpSrc = MpHelpers.Instance.ResizeBitmapSource(ItemBitmapSource, new Size(scale, scale));

                        var ibsba = MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemBitmapSource);

                        //ImageItemObjectList = await MpHelpers.Instance.DetectObjectsAsync(
                        //    MpHelpers.Instance.ConvertBitmapSourceToByteArray(scanBmpSrc),
                        //    Properties.Settings.Default.DetectedImageMinConfidence);

                        //// don't perform ocr for now it takes too long on big images
                        ////string fp = Path.GetTempFileName();
                        ////fp = fp.Remove(fp.IndexOf('.'), fp.Length - fp.IndexOf('.')) + ".png";
                        ////var imgPath = MpHelpers.Instance.WriteBitmapSourceToFile(fp, ItemBitmapSource);
                        ////ItemPlainText = await MpHelpers.Instance.OcrBitmapSourceFileAsync(imgPath);
                        ////File.Delete(imgPath);
                        ///
                        var ia = await MpImageAnalyzer.Instance.AnalyzeImage(ibsba);

                        Console.WriteLine("-------------------------------------------------------------------");
                        Console.WriteLine("-------------------------------------------------------------------");
                        Console.WriteLine("-------------------------------------------------------------------");
                        await MpImageOcr.Instance.OcrImage(ibsba);

                        //if (string.IsNullOrEmpty(ItemPlainText)) {
                        //    if(ImageItemObjectList.Count > 0) {
                        //        ItemPlainText = ItemCsv;
                        //    } else {
                        //        ItemPlainText = "Image";
                        //    }                            
                        //}

                        ItemPlainText = "Image";
                    }
                    break;
                case MpCopyItemType.RichText:
                    ItemPlainText = MpHelpers.Instance.ConvertRichTextToPlainText((string)_itemData);
                    if(!string.IsNullOrEmpty(ItemCsv) && MpHelpers.Instance.IsStringCsv(ItemPlainText)) {
                        ItemCsv = ItemPlainText;
                    }
                    break;
            }            
        }

        private void UpdateDetails() {
            switch(CopyItemType) {
                case MpCopyItemType.Image:

                    break;
                case MpCopyItemType.FileList:
                    FileCount = GetFileList().Count;
                    DataSizeInMb = MpHelpers.Instance.FileListSize(GetFileList().ToArray());
                    break;
                case MpCopyItemType.RichText:
                    LineCount = MpHelpers.Instance.GetRowCount(ItemPlainText);
                    CharCount = ItemPlainText.Length;
                    break;
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
            ItemTitleSwirl = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["TitleSwirl"]);
            ItemCsv = dr["ItemCsv"].ToString();

            Client = new MpClient(0, 0, MpHelpers.Instance.GetCurrentIPAddress().MapToIPv4().ToString(), "unknown", DateTime.Now);
            if(_AppList == null) {
                _AppList = MpApp.GetAllApps();
            }
            App = _AppList.Where(x => x.AppId == appId).ToList()[0];
            if(_ColorList == null) {
                _ColorList = MpColor.GetAllColors();
            }
            ItemColor = _ColorList.Where(x => x.ColorId == colorId).ToList()[0];

            if (CopyItemType == MpCopyItemType.Image) {
                SetData(MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["ItemImage"]));
                ItemPlainText = dr["ItemText"].ToString();
                //ItemCsv = ItemPlainText;
            } else {
                SetData(dr["ItemText"].ToString());
            }

            PasteCount = GetPasteCount();
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
            MpDb.Instance.ExecuteWrite(
                "delete from MpShortcut where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTemplate where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId }
                    });

            var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
            //mwvm.ClipTrayViewModel.GetDataSource().CopyItemDataProvider.OnCopyItemChanged(this, new MpCopyItemChangeEventArgs(MpCopyItemChangeType.Remove));
        }

        // still req'd if NoDb=true
        public override void WriteToDatabase() {
            //var changeType = MpCopyItemChangeType.None;

            App.WriteToDatabase();
            ItemColor.WriteToDatabase();

            string itemText = CopyItemType == MpCopyItemType.RichText ? ItemRichText : ItemPlainText;
            if(string.IsNullOrEmpty(itemText)) {
                itemText = string.Empty;
            }
            byte[] itemImage = CopyItemType == MpCopyItemType.Image ? MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemBitmapSource) : null;
            //if copyitem already exists
            if (CopyItemId > 0) {
                //changeType = MpCopyItemChangeType.Update;
                MpDb.Instance.ExecuteWrite(
                        "update MpCopyItem set ItemCsv=@icsv, TitleSwirl=@ts, fk_MpCopyItemTypeId=@citd, fk_MpClientId=@cid, fk_MpAppId=@aid, fk_MpColorId=@clrId, Title=@t, CopyCount=@cc, ItemText=@it, ItemImage=@ii where pk_MpCopyItemId=@ciid",
                        new Dictionary<string, object> {
                            { @"icsv",ItemCsv },
                            { "@ts", MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
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
                //changeType = MpCopyItemChangeType.Add;
                MpDb.Instance.ExecuteWrite(
                    "insert into MpCopyItem(ItemCsv,TitleSwirl,fk_MpCopyItemTypeId,fk_MpClientId,fk_MpAppId,fk_MpColorId,Title,CopyDateTime,CopyCount,ItemText,ItemImage) " + 
                    "values (@icsv,@ts,@citd,@cid,@aid,@clrId,@t,@cdt,@cc,@it,@ii)",
                    new Dictionary<string, object> {
                            { @"icsv",ItemCsv },
                            { "@ts", MpHelpers.Instance.ConvertBitmapSourceToByteArray(ItemTitleSwirl) },
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

            var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;
            //mwvm.ClipTrayViewModel.GetDataSource().CopyItemDataProvider.OnCopyItemChanged(this, new MpCopyItemChangeEventArgs(changeType));
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
