using System;
using System.Linq;
using System.Collections.Generic;
using MonkeyPaste;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpUrlDomain : MpDbModelBase, MonkeyPaste.MpISyncableDbObject {
        private static List<MpUrlDomain> _AllUrlDomainList = null;

        public static int TotalUrlDomainCount = 0;

        public int UrlDomainId { get; set; } = 0;
        public Guid UrlDomainGuid { get; set; }
        public int FavIconId { get; set; }

        public string UrlDomainPath { get; set; } = string.Empty;
        public string UrlDomainTitle { get; set; } = string.Empty;
        public bool IsUrlDomainRejected { get; set; } = false;
        public MpIcon FavIcon { get; set; }

        public BitmapSource FavIconImage {
            get {
                return FavIcon.IconImage;
            }
            set {
                FavIcon.IconImage = value;
            }
        }
        public BitmapSource FavIconBorderImage {
            get {
                return FavIcon.IconBorderImage;
            }
            set {
                FavIcon.IconBorderImage = value;
            }
        }
        public BitmapSource FavIconHighlightBorderImage {
            get {
                return FavIcon.IconBorderHighlightImage;
            }
            set {
                FavIcon.IconBorderHighlightImage = value;
            }
        }
        public BitmapSource FavIconSelectedHighlightBorderImage {
            get {
                return FavIcon.IconBorderHighlightSelectedImage;
            }
            set {
                FavIcon.IconBorderHighlightSelectedImage = value;
            }
        }

        //public BitmapSource FavIconImage { get; set; } = new BitmapImage();
        //public BitmapSource FavIconBorderImage { get; set; } = new BitmapImage();
        //public BitmapSource FavIconHighlightBorderImage { get; set; } = new BitmapImage();
        //public BitmapSource FavIconSelectedHighlightBorderImage { get; set; } = new BitmapImage();

        //public int[] ColorId = new int[5];

        //public MpObservableCollection<MpColor> PrimaryIconColorList = new MpObservableCollection<MpColor>();

        #region Static Methods
        public static List<MpUrlDomain> GetAllUrlDomains() {
            if(_AllUrlDomainList == null) {
                _AllUrlDomainList = new List<MpUrlDomain>();
                var dt = MpDb.Instance.Execute("select * from MpUrlDomain", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllUrlDomainList.Add(new MpUrlDomain(dr));
                    }
                }
            }
            return _AllUrlDomainList;
        }

        public static MpUrlDomain GetUrlDomainById(int urlDomainId) {
            if (_AllUrlDomainList == null) {
                GetAllUrlDomains();
            }
            var udbpl = _AllUrlDomainList.Where(x => x.UrlDomainId == urlDomainId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpUrlDomain GetUrlDomainByPath(string urlDomain) {
            if(_AllUrlDomainList == null) {
                GetAllUrlDomains();
            }
            var udbpl = _AllUrlDomainList.Where(x => x.UrlDomainPath.ToLower().Contains(urlDomain)).ToList();
            if(udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static bool IsUrlDomainRejectedByHandle(string fullUrl) {
            string urlDomainPath = MpHelpers.Instance.GetUrlDomain(fullUrl);
            foreach (MpUrlDomain urlDomain in GetAllUrlDomains()) {
                if (urlDomain.UrlDomainPath == urlDomainPath && urlDomain.IsUrlDomainRejected) {
                    return true;
                }
            }
            return false;
        }
        public static List<MpUrlDomain> GetAllRejectedUrlDomains() {
            return GetAllUrlDomains().Where(x => x.IsUrlDomainRejected == true).ToList();
        }
        #endregion

        public MpUrlDomain(string domainUrl, BitmapSource favIconBmpSrc, string domainTitle = "", bool isUrlDomainRejected = false) {
            UrlDomainGuid = Guid.NewGuid();
            UrlDomainPath = domainUrl;
            UrlDomainTitle = string.IsNullOrEmpty(domainTitle) ? UrlDomainPath : domainTitle;
            IsUrlDomainRejected = isUrlDomainRejected;
            favIconBmpSrc = favIconBmpSrc == null ? (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/web.png")) : favIconBmpSrc;
            FavIcon = new MpIcon(favIconBmpSrc);

            //FavIconImage = favIconBmpSrc == null ? (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/web.png")) : favIconBmpSrc;
            //FavIconBorderImage = MpHelpers.Instance.CreateBorder(FavIconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
            //FavIconHighlightBorderImage = MpHelpers.Instance.CreateBorder(FavIconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            //FavIconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(FavIconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
            //PrimaryIconColorList = MpColor.CreatePrimaryColorList(FavIconImage);
        }

        public MpUrlDomain() { }

        public MpUrlDomain(DataRow dr) {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            UrlDomainId = Convert.ToInt32(dr["pk_MpUrlDomainId"].ToString());
            UrlDomainGuid = Guid.Parse(dr["MpUrlDomainGuid"].ToString());

            UrlDomainPath = dr["UrlDomainPath"].ToString();
            UrlDomainTitle = dr["UrlDomainTitle"].ToString();

            FavIconId = Convert.ToInt32(dr["fk_MpIconId"].ToString());
            FavIcon = new MpIcon(FavIconId);

            //FavIconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconBlob"]);
            //FavIconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconBorderBlob"]);
            //FavIconSelectedHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconSelectedHighlightBorderBlob"]);
            //FavIconHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconHighlightBorderBlob"]);
            IsUrlDomainRejected = Convert.ToInt32(dr["IsUrlDomainRejected"].ToString()) == 1;

            //PrimaryIconColorList.Clear();
            //for (int i = 0; i < 5; i++) {
            //    ColorId[i] = Convert.ToInt32(dr["fk_MpColorId" + (i + 1)].ToString());
            //    if (ColorId[i] > 0) {
            //        PrimaryIconColorList.Add(new MpColor(ColorId[i]));
            //    }
            //}
        }

        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            //for (int i = 1; i <= PrimaryIconColorList.Count; i++) {
            //    var c = PrimaryIconColorList[i - 1];
            //    c.WriteToDatabase();
            //    ColorId[i - 1] = c.ColorId;
            //}
            FavIcon.WriteToDatabase();
            FavIconId = FavIcon.IconId;

            if (UrlDomainId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpUrlDomain(MpUrlDomainGuid, UrlDomainPath,UrlDomainTitle,fk_MpIconId,IsUrlDomainRejected) " +
                        "values (@udg,@udp,@udt,@fib,@fibb,@fishbb,@fhbb,@iudr)",
                        new Dictionary<string, object> {
                            { "@udg",UrlDomainGuid.ToString() },
                            { "@udp", UrlDomainPath },
                            { "@udt", UrlDomainTitle },
                            { "@fib", FavIconId },
                            { "@iudr", Convert.ToInt32(IsUrlDomainRejected) }
                        },UrlDomainGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                UrlDomainId = MpDb.Instance.GetLastRowId("MpUrlDomain", "pk_MpUrlDomainId");
            }

            var urldl = GetAllUrlDomains().Where(x => x.UrlDomainId == UrlDomainId).ToList();
            if(urldl.Count > 0) {
                _AllUrlDomainList[_AllUrlDomainList.IndexOf(urldl[0])] = this;
            } else {
                _AllUrlDomainList.Add(this);
            }
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (UrlDomainId <= 0) {
                return;
            }
            FavIcon.DeleteFromDatabase();
            // NOTE: Colors not deleted since they may be referenced by another UrlDomain

            MpDb.Instance.ExecuteWrite(
                "delete from MpUrlDomain where pk_MpUrlDomainId=@aid",
                new Dictionary<string, object> {
                    { "@aid", UrlDomainId }
                },UrlDomainGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            var urldl = GetAllUrlDomains().Where(x => x.UrlDomainId == UrlDomainId).ToList();
            if (urldl.Count > 0) {
                _AllUrlDomainList.RemoveAt(_AllUrlDomainList.IndexOf(urldl[0]));
            } 
        }

        public string GetUrlDomainName() {
            return UrlDomainPath == null || UrlDomainPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(UrlDomainPath);
        }

        public async Task<object> CreateFromLogs(string urlDomainGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var urlDomain = MpDb.Instance.GetDbObjectByTableGuid("MpUrlDomain", urlDomainGuid) as MpUrlDomain;

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUrlDomainGuid":
                        urlDomain.UrlDomainGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpIconId":
                        urlDomain.FavIcon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon",li.AffectedColumnValue) as MpIcon;
                        urlDomain.FavIconId = urlDomain.FavIcon.IconId;
                        break;
                    case "UrlDomainPath":
                        urlDomain.UrlDomainPath = li.AffectedColumnValue;
                        break;
                    case "UrlDomainTitle":
                        urlDomain.UrlDomainTitle = li.AffectedColumnValue;
                        break;
                    case "IsUrlDomainRejected":
                        urlDomain.IsUrlDomainRejected = li.AffectedColumnValue == "1";
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return urlDomain;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var urld = new MpUrlDomain() {
                UrlDomainGuid = System.Guid.Parse(objParts[0])
            };

            urld.FavIcon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon", objParts[1]) as MpIcon;
            urld.FavIconId = urld.FavIcon.IconId;

            urld.UrlDomainPath = objParts[2];
            urld.UrlDomainTitle = objParts[3];
            urld.IsUrlDomainRejected = objParts[4] == "1";
            return urld;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}",
                ParseToken,
                UrlDomainGuid.ToString(),
                FavIcon.IconGuid.ToString(),
                UrlDomainPath,
                UrlDomainTitle,
                IsUrlDomainRejected ? "1":"0");
        }

        public Type GetDbObjectType() {
            return typeof(MpUrlDomain);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpUrlDomain other = null;
            if (drOrModel is DataRow) {
                other = new MpUrlDomain(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpUrlDomain();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(UrlDomainGuid, other.UrlDomainGuid,
                "MpUrlDomainGuid",
                diffLookup,
                UrlDomainGuid.ToString());
            diffLookup = CheckValue(FavIconId, other.FavIconId,
                "fk_MpIconId",
                diffLookup,
                FavIcon.IconGuid.ToString());
            diffLookup = CheckValue(UrlDomainPath, other.UrlDomainPath,
                "UrlDomainPath",
                diffLookup);
            diffLookup = CheckValue(UrlDomainTitle, other.UrlDomainTitle,
                "UrlDomainTitle",
                diffLookup);
            diffLookup = CheckValue(IsUrlDomainRejected, other.IsUrlDomainRejected,
                "IsUrlDomainRejected",
                diffLookup,
                IsUrlDomainRejected ? "1":"0");

            return diffLookup;
        }
    }
}
