using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    
    public class MpApp : MpDBObject {
        public static int TotalAppCount = 0;

        public int appId { get; set; }
        public int iconId { get; set; }
        public string SourcePath { get; set; }
        public bool IsAppRejected { get; set; }

        public MpIcon Icon { get; set; }

        private IntPtr _sourceHandle;
        //for new MpApp's set appId and iconId to 0
        public MpApp(int appId,int iconId,IntPtr sourceHandle,bool isAppRejected) {            
            this.appId = appId;
            this.iconId = iconId;
            this.SourcePath = MpHelperSingleton.Instance.GetProcessPath(sourceHandle);
            this.IsAppRejected = isAppRejected;
            this._sourceHandle = sourceHandle;

            WriteToDatabase();
        }
        public MpApp(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            this.appId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            this.iconId = Convert.ToInt32(dr["fk_MpIconId"].ToString());
            Icon = new MpIcon(iconId);
            this.SourcePath = dr["SourcePath"].ToString();
            if(Convert.ToInt32(dr["IsAppRejected"].ToString()) == 0) {
                this.IsAppRejected = false;
            }
            else {
                this.IsAppRejected = true;
            }
            MapDataToColumns();
            Console.WriteLine("Loaded MpApp");
            Console.WriteLine(ToString());
        }
        public override void WriteToDatabase() {           
            bool isNew = false;
            if(this.iconId == 0) {
                Icon = new MpIcon(0,_sourceHandle);
                this.iconId = Icon.iconId;
                //MpSingletonController.Instance.GetMpData().AddMpIcon(Icon);
            } else {
                Icon = new MpIcon(iconId);
            }
            if(this.appId == 0) {
                if(MpLogFormController.Db.NoDb) {
                    this.appId = ++TotalAppCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpLogFormController.Db.Execute("select * from MpApp where SourcePath='" + this.SourcePath + "'");
                if(dt.Rows.Count > 0) {
                    this.appId = Convert.ToInt32(dt.Rows[0]["pk_MpAppId"]);
                    this.iconId = Convert.ToInt32(dt.Rows[0]["fk_MpIconId"]);
                    isNew = false;
                }
                else {
                    MpLogFormController.Db.ExecuteNonQuery("insert into MpApp(fk_MpIconId,SourcePath,IsAppRejected) values (" + this.iconId + ",'" + SourcePath + "'," + Convert.ToInt32(this.IsAppRejected) + ")");//+ "',"+Convert.ToInt32(this.IsAppRejected)+",@0)",new List<string>() { "@0" },new List<object>() { MpHelperFunctions.Instance.ConvertImageToByteArray(MpSingletonController.Instance.GetMpLastWindowWatcher().LastIconImage) });
                    this.appId = MpLogFormController.Db.GetLastRowId("MpApp","pk_MpAppId");
                    isNew = false;
                }                
            }
            else {
                MpLogFormController.Db.ExecuteNonQuery("update MpApp set fk_MpIconId=" + this.iconId + ",IsAppRejected="+Convert.ToInt32(this.IsAppRejected)+",SourcePath='" + this.SourcePath + "' where pk_MpAppId=" + this.appId);
            }
            //MpSingletonController.Instance.GetMpData().AddMpApp(this);
            MapDataToColumns();
            Console.WriteLine(isNew ? "Created ":"Updated "+ " MpApp");
            Console.WriteLine(ToString());
        }
        private void MapDataToColumns() {
            tableName = "MpApp";
            columnData.Add("pk_MpAppId",this.appId);
            columnData.Add("fk_MpIconId",this.iconId);
            columnData.Add("SourcePath",this.SourcePath);
            columnData.Add("IsAppRejected",this.IsAppRejected);
        }
    }
}
