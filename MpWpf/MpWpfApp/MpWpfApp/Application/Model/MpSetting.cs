using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSetting:MpDbObject {
        public static int TotalSettingCount { get; set; } = 0;

        public int SettingId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }

        public MpSetting(string settingName,string settingValue) {
            SettingName = settingName;
            SettingValue = settingValue;
        }
        public MpSetting(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            this.SettingId = Convert.ToInt32(dr["pk_MpSettingId"].ToString());
            this.SettingName = dr["SettingName"].ToString();
            this.SettingValue = dr["SettingValue"].ToString();
            MapDataToColumns();
            Console.WriteLine("Loaded MpSetting");
            Console.WriteLine(ToString());
        }
        public override void WriteToDatabase() {
            bool isNew = false;

            if(this.SettingId == 0) {
                if(MpApplication.Instance.DataModel.Db.NoDb) {
                    this.SettingId = ++TotalSettingCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpApplication.Instance.DataModel.Db.Execute("select * from MpSetting where SettingName='" + this.SettingName + "'");
                if(dt.Rows.Count > 0) {
                    this.SettingId = Convert.ToInt32(dt.Rows[0]["pk_MpSettingId"]);
                    this.SettingName = dt.Rows[0]["SettingName"].ToString();
                    this.SettingValue = dt.Rows[0]["SettingValue"].ToString();
                    isNew = false;
                }
                else {
                    MpApplication.Instance.DataModel.Db.ExecuteNonQuery("insert into MpSetting(SettingName,SettingValue) values ('" + this.SettingName + "','" + this.SettingValue + "')");
                    this.SettingId = MpApplication.Instance.DataModel.Db.GetLastRowId("MpSetting","pk_MpSettingId");
                    isNew = false;
                }
            }
            else {
                MpApplication.Instance.DataModel.Db.ExecuteNonQuery("update MpSetting set SettingName='" + this.SettingName + "',SettingValue='" + this.SettingValue + "' where pk_MpSettingId=" + this.SettingId);
            }
            if(isNew) {
                MapDataToColumns();
            }
            Console.WriteLine(isNew ? "Created " : "Updated " + " MpSetting");
            Console.WriteLine(ToString());
        }
        private void MapDataToColumns() {
            TableName = "MpSetting";
            columnData.Add("pk_MpSettingId",this.SettingId);
            columnData.Add("SettingName",this.SettingName);
            columnData.Add("SettingValue",this.SettingValue);
        }
    }
}
