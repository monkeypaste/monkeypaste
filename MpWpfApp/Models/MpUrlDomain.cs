using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace MpWpfApp {

    public class MpUrlDomain : MpDbObject {
        private static List<MpUrlDomain> _AllUrlDomainList = null;

        public static int TotalUrlDomainCount = 0;

        public int UrlDomainId { get; set; } = 0;
        public string UrlDomainPath { get; set; } = string.Empty;
        public string UrlDomainTitle { get; set; } = string.Empty;
        public bool IsUrlDomainRejected { get; set; } = false;
        public BitmapSource FavIconImage { get; set; } = new BitmapImage();
        public BitmapSource FavIconBorderImage { get; set; } = new BitmapImage();
        public BitmapSource FavIconHighlightBorderImage { get; set; } = new BitmapImage();
        public BitmapSource FavIconSelectedHighlightBorderImage { get; set; } = new BitmapImage();

        public int[] ColorId = new int[5];

        public MpObservableCollection<MpColor> PrimaryIconColorList = new MpObservableCollection<MpColor>();

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
            UrlDomainPath = domainUrl;
            UrlDomainTitle = string.IsNullOrEmpty(domainTitle) ? UrlDomainPath : domainTitle;
            IsUrlDomainRejected = isUrlDomainRejected;

            FavIconImage = favIconBmpSrc == null ? (BitmapSource)new BitmapImage(new Uri(Properties.Settings.Default.AbsoluteResourcesPath + @"/Images/web.png")) : favIconBmpSrc;
            FavIconBorderImage = MpHelpers.Instance.CreateBorder(FavIconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.White);
            FavIconHighlightBorderImage = MpHelpers.Instance.CreateBorder(FavIconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Yellow);
            FavIconSelectedHighlightBorderImage = MpHelpers.Instance.CreateBorder(FavIconImage, MpMeasurements.Instance.ClipTileTitleIconBorderSizeRatio, Colors.Pink);
            PrimaryIconColorList = MpColor.CreatePrimaryColorList(FavIconImage);
        }

        public MpUrlDomain() { }

        public MpUrlDomain(DataRow dr) {
            LoadDataRow(dr);
        }

        public override void LoadDataRow(DataRow dr) {
            UrlDomainId = Convert.ToInt32(dr["pk_MpUrlDomainId"].ToString());
            UrlDomainPath = dr["UrlDomainPath"].ToString();
            UrlDomainTitle = dr["UrlDomainTitle"].ToString();
            FavIconImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconBlob"]);
            FavIconBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconBorderBlob"]);
            FavIconSelectedHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconSelectedHighlightBorderBlob"]);
            FavIconHighlightBorderImage = MpHelpers.Instance.ConvertByteArrayToBitmapSource((byte[])dr["FavIconHighlightBorderBlob"]);
            IsUrlDomainRejected = Convert.ToInt32(dr["IsUrlDomainRejected"].ToString()) == 1;

            PrimaryIconColorList.Clear();
            for (int i = 0; i < 5; i++) {
                ColorId[i] = Convert.ToInt32(dr["fk_MpColorId" + (i + 1)].ToString());
                if (ColorId[i] > 0) {
                    PrimaryIconColorList.Add(new MpColor(ColorId[i]));
                }
            }
        }

        public override void WriteToDatabase() {
            for (int i = 1; i <= PrimaryIconColorList.Count; i++) {
                var c = PrimaryIconColorList[i - 1];
                c.WriteToDatabase();
                ColorId[i - 1] = c.ColorId;
            }
            if (UrlDomainId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpUrlDomain(UrlDomainPath,UrlDomainTitle,FavIconBlob,FavIconBorderBlob,FavIconSelectedHighlightBorderBlob,FavIconHighlightBorderBlob,IsUrlDomainRejected,fk_MpColorId1,fk_MpColorId2,fk_MpColorId3,fk_MpColorId4,fk_MpColorId5) " +
                        "values (@udp,@udt,@fib,@fibb,@fishbb,@fhbb,@iudr,@c1,@c2,@c3,@c4,@c5)",
                        new Dictionary<string, object> {
                            { "@udp", UrlDomainPath },
                            { "@udt", UrlDomainTitle },
                            { "@fib", MpHelpers.Instance.ConvertBitmapSourceToByteArray(FavIconImage) },
                            { "@fibb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(FavIconBorderImage) },
                            { "@fishbb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(FavIconSelectedHighlightBorderImage) },
                            { "@fhbb", MpHelpers.Instance.ConvertBitmapSourceToByteArray(FavIconHighlightBorderImage) },
                            { "@iudr", Convert.ToInt32(IsUrlDomainRejected) },
                            { "@c1", ColorId[0] },
                            { "@c2", ColorId[1] },
                            { "@c3", ColorId[2] },
                            { "@c4", ColorId[3] },
                            { "@c5", ColorId[4] }
                        });
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
            if (UrlDomainId <= 0) {
                return;
            }

            // NOTE: Colors not deleted since they may be referenced by another UrlDomain

            MpDb.Instance.ExecuteWrite(
                "delete from MpUrlDomain where pk_MpUrlDomainId=@aid",
                new Dictionary<string, object> {
                    { "@aid", UrlDomainId }
                });

            var urldl = GetAllUrlDomains().Where(x => x.UrlDomainId == UrlDomainId).ToList();
            if (urldl.Count > 0) {
                _AllUrlDomainList.RemoveAt(_AllUrlDomainList.IndexOf(urldl[0]));
            } 
        }

        public string GetUrlDomainName() {
            return UrlDomainPath == null || UrlDomainPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(UrlDomainPath);
        }
    }
}
