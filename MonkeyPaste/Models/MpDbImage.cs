using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbImage : MpDbModelBase, MpISyncableDbObject {
        [PrimaryKey,AutoIncrement]
        [Column("pk_MpDbImageId")]
        public override int Id { get; set; }

        [Column("MpDbImageGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid DbImageGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        public string ImageBase64 { get; set; }

        public static async Task<MpDbImage> Create(string base64Str) {
            if(!base64Str.IsBase64String()) {
                MpConsole.WriteLine("Warning malformed base64 str, cannot create dbimage so returing default");
                return MpPreferences.Instance.ThisAppSource.PrimarySource.SourceIcon.IconImage;
            }

            var dupCheck = await MpDataModelProvider.Instance.GetDbImageByBase64Str(base64Str);
            if(dupCheck != null) {
                dupCheck = await MpDb.Instance.GetItemAsync<MpDbImage>(dupCheck.Id);
                return dupCheck;
            }
            var i = new MpDbImage() {
                DbImageGuid = System.Guid.NewGuid(),
                ImageBase64 = base64Str
            };

            await i.WriteToDatabaseAsync();
            return i;
        }
        public MpDbImage() { }

        public async Task<object> CreateFromLogs(string imgGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {            
            var imgDr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpDbImage", imgGuid);
            MpDbImage img = null;
            if (imgDr == null) {
                img = new MpDbImage();
            } else {
                img = imgDr as MpDbImage;
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpDbImageGuid":
                        img.DbImageGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "ImageBase64":
                        img.ImageBase64 = li.AffectedColumnValue;
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
                ImageBase64 = objParts[1]
            };
            return img;
        }

        public async Task<string> SerializeDbObject() {
            await Task.Delay(1);
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                ParseToken,
                DbImageGuid.ToString(),
                ImageBase64);
        }

        public Type GetDbObjectType() {
            return typeof(MpDbImage);
        }

        public async Task<Dictionary<string, string>> DbDiff(object drOrModel) {
            await Task.Delay(1);

            MpDbImage other = null;
            
            if (drOrModel is MpDbImage) {
                other = drOrModel as MpDbImage;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpDbImage();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(DbImageGuid, other.DbImageGuid,
                "MpDbImageGuid",
                diffLookup);
            diffLookup = CheckValue(ImageBase64, other.ImageBase64,
                "ImageBase64",
                diffLookup);

            return diffLookup;
        }
    }
}
