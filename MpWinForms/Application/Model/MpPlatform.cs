using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWinFormsApp {
    public enum MpPlatformType {
        Ios = 1,
        Android = 2,
        Windows = 3,
        Mac = 4,
        Linux = 5
    }
    public enum MpDeviceType {
        Windows = 1,
        Mac = 2,
        Android = 3,
        Iphone = 4,
        Ipad = 5,
        Tablet
    }
    public class MpPlatform : MpDbObject {
        public int PlatformId { get; set; }
        public MpPlatformType PlatformType { get; set; }
        public MpDeviceType DeviceType { get; set; }
        public string Version { get; set; }

        public MpPlatform(int platformId,MpPlatformType platformType,MpDeviceType deviceType,string version) {
            PlatformId = platformId;
            PlatformType = platformType;
            DeviceType = deviceType;
            Version = version;
        }
        public MpPlatform(int platformId) {
            DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpPlatform where pk_MpPlatformId=" + platformId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpPlatform(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            PlatformId = Convert.ToInt32(dr["pk_MpPlatformId"].ToString());
            PlatformType = (MpPlatformType)Convert.ToInt32(dr["fk_MpPlatformTypeId"].ToString());
            DeviceType = (MpDeviceType)Convert.ToInt32(dr["fk_MpDeviceTypeId"].ToString());
            Version = dr["Version"].ToString();
        }

        public override void WriteToDatabase() {
            if(Version == null || Version == string.Empty || MpApplication.Instance.DataModel.Db.NoDb) {
                Console.WriteLine("MpPlatform Error, cannot create nameless tag");
                return;
            }
            if(PlatformId == 0) {
                DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpPlatform where pk_MpPlatformId=" + PlatformId);
                //if tag already exists just populate this w/ its data
                if(dt != null && dt.Rows.Count > 0) {
                    PlatformId = Convert.ToInt32(dt.Rows[0]["pk_MpPlatformId"].ToString());
                }
                else {
                    MpApplication.Instance.DataModel.Db.ExecuteNonQuery("insert into MpPlatform(fk_MpPlatformTypeId,fk_MpDeviceTypeId,Version) values(" + (int)PlatformType + "," + (int)DeviceType + ",'" + Version + "')");
                    PlatformId = MpApplication.Instance.DataModel.Db.GetLastRowId("MpPlatform","pk_MpPlatformId");
                }
            }
            else {
                Console.WriteLine("MpPlatform warning, attempting to update a platform but not implemented");
            }
        }
    }
}
