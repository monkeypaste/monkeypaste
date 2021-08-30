using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Linq;
using FFImageLoading.Helpers.Exif;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpDbImage : MpDbModelBase, MonkeyPaste.MpISyncableDbObject {
        private static List<MpDbImage> _AllImagesList = null;

        public int DbImageId { get; set; }
        public Guid DbImageGuid { get; set; }

        public BitmapSource DbImage { get; set; }

        public string DbImageBase64 { get; set; }


        public static List<MpDbImage> GetAllImages() {
            if (_AllImagesList == null) {
                _AllImagesList = new List<MpDbImage>();
                DataTable dt = MpDb.Instance.Execute("select * from MpDbImage", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllImagesList.Add(new MpDbImage(dr));
                    }
                }
            }
            return _AllImagesList;
        }

        public static MpDbImage GetImageById(int imgId) {
            return GetAllImages().Where(x => x.DbImageId == imgId).FirstOrDefault();
        }


        public MpDbImage() { }

        public MpDbImage(BitmapSource img) {
            DbImageGuid = Guid.NewGuid();
            DbImage = img;
            DbImageBase64 = MpHelpers.Instance.ConvertBitmapSourceToBase64String(DbImage);
        }

        public MpDbImage(DataRow dr) {
            LoadDataRow(dr);
        }

        public MpDbImage(int dbImageId) {
            if(dbImageId <= 0) {
                return;
            }
            var dt = MpDb.Instance.Execute(
                "select * from MpDbImage where pk_MpDbImageId=@iid",
                new Dictionary<string, object> {
                    { "@iid", dbImageId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            } else {
                throw new Exception("MpIcon error trying access unknown icon w/ pk: " + dbImageId);
            }
        }

        public override void LoadDataRow(DataRow dr) {
            DbImageId = Convert.ToInt32(dr["pk_MpDbImageId"].ToString());
            DbImageGuid = Guid.Parse(dr["MpDbImageGuid"].ToString());
            DbImageBase64 = dr["ImageBase64"].ToString();
            DbImage = MpHelpers.Instance.ConvertStringToBitmapSource(DbImageBase64);
        }

        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            string imgStr = DbImageBase64;//MpHelpers.Instance.ConvertBitmapSourceToBase64String(DbImage);
            if (DbImageId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpDbImage(MpDbImageGuid,ImageBase64) values(@dbig,@ib64)",
                    new Dictionary<string, object> {
                        { "@dbig",DbImageGuid.ToString() },
                        { "@ib64", imgStr }
                    },DbImageGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);
                DbImageId = MpDb.Instance.GetLastRowId("MpDbImage", "pk_MpDbImageId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpDbImage set MpDbImageGuid=@dbig, ImageBase64=@ib64 where pk_MpDbImageId=@dbiid",
                    new Dictionary<string, object> {
                        { "@dbig", DbImageGuid.ToString() },
                        { "@dbiid", DbImageId },
                        { "@ib64", imgStr  }
                    },DbImageGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
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
            MpDb.Instance.ExecuteWrite(
                "delete from MpDbImage where pk_MpDbImageId=@tid",
                new Dictionary<string, object> {
                    { "@tid", DbImageId }
                },DbImageGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
        }

        public async Task<object> CreateFromLogs(string imgGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var imgDr = MpDb.Instance.GetDbDataRowByTableGuid("MpDbImage", imgGuid);
            MpDbImage img = null;
            if (imgDr == null) {
                img = new MpDbImage();
            } else {
                img = new MpDbImage(imgDr);
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpDbImageGuid":
                        img.DbImageGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "ImageBase64":
                        img.DbImageBase64 = li.AffectedColumnValue;
                        img.DbImage = MpHelpers.Instance.ConvertStringToBitmapSource(img.DbImageBase64);
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return img;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var img = new MpDbImage() {
                DbImageGuid = System.Guid.Parse(objParts[0]),
                DbImageBase64 = objParts[1]
            };

            img.DbImage = MpHelpers.Instance.ConvertStringToBitmapSource(DbImageBase64);
            return img;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                ParseToken,
                DbImageGuid.ToString(),
                DbImageBase64);
        }

        public Type GetDbObjectType() {
            return typeof(MpDbImage);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpDbImage other = null;
            if (drOrModel is DataRow) {
                other = new MpDbImage(drOrModel as DataRow);
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpDbImage();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(DbImageGuid, other.DbImageGuid,
                "MpDbImageGuid",
                diffLookup);
            diffLookup = CheckValue(DbImageBase64, other.DbImageBase64,
                "ImageBase64",
                diffLookup);

            return diffLookup;
        }
    }
}
