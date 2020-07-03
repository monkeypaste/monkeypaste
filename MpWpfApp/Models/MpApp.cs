
using MpWinFormsClassLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    
    public class MpApp : MpDbObject {
        public static int TotalAppCount = 0;

        public int appId { get; set; }
        public int iconId { get; set; }
        public string AppPath { get; set; }
        
        public bool IsAppRejected { get; set; }

        public MpIcon Icon { get; set; }
        
        //for new MpApp's set appId and iconId to 0
        public MpApp(String sourcePath,bool isAppRejected) {
            this.iconId = this.appId = 0;
            this.AppPath = sourcePath;
            this.IsAppRejected = isAppRejected;
            this.Icon = new MpIcon(MpHelperSingleton.Instance.GetIconImage(this.AppPath));
        }
        public MpApp(int appId,int iconId,IntPtr sourceHandle,bool isAppRejected) {            
            this.appId = appId;
            this.iconId = iconId;
            this.AppPath = MpHelperSingleton.Instance.GetProcessPath(sourceHandle);
            this.IsAppRejected = isAppRejected;
        }
        public MpApp(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            this.appId = Convert.ToInt32(dr["pk_MpAppId"].ToString());
            this.iconId = Convert.ToInt32(dr["fk_MpIconId"].ToString());
            Icon = new MpIcon(iconId);
            this.AppPath = dr["SourcePath"].ToString();
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
                Icon = new MpIcon(MpHelperSingleton.Instance.GetIconImage(this.AppPath));
            } 
            Icon.WriteToDatabase();
            this.iconId = Icon.iconId;
            if(this.appId == 0) {
                if(MpDb.Instance.NoDb) {
                    this.appId = ++TotalAppCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpDb.Instance.Execute("select * from MpApp where SourcePath='" + this.AppPath + "'");
                if(dt.Rows.Count > 0) {
                    this.appId = Convert.ToInt32(dt.Rows[0]["pk_MpAppId"]);
                    this.iconId = Convert.ToInt32(dt.Rows[0]["fk_MpIconId"]);
                    isNew = false;
                }
                else {
                    MpDb.Instance.ExecuteNonQuery("insert into MpApp(fk_MpIconId,SourcePath,IsAppRejected) values (" + this.iconId + ",'" + AppPath + "'," + Convert.ToInt32(this.IsAppRejected) + ")");//+ "',"+Convert.ToInt32(this.IsAppRejected)+",@0)",new List<string>() { "@0" },new List<object>() { MpHelperFunctions.Instance.ConvertImageToByteArray(MpSingletonController.Instance.GetMpLastWindowWatcher().LastIconImage) });
                    this.appId = MpDb.Instance.GetLastRowId("MpApp","pk_MpAppId");
                    isNew = false;
                }                
            }
            else {
                MpDb.Instance.ExecuteNonQuery("update MpApp set fk_MpIconId=" + this.iconId + ",IsAppRejected="+Convert.ToInt32(this.IsAppRejected)+",SourcePath='" + this.AppPath + "' where pk_MpAppId=" + this.appId);
            }
            if(isNew) {
                MapDataToColumns();
            }
            Console.WriteLine(isNew ? "Created ":"Updated "+ " MpApp");
            Console.WriteLine(ToString());
        }
        public void DeleteFromDatabase() {
            if (appId <= 0) {
                return;
            }

            MpDb.Instance.ExecuteNonQuery("delete from MpApp where pk_MpAppId=" + appId);
        }

        public string GetAppName() {
            return AppPath == null || AppPath == string.Empty ? "None" : Path.GetFileNameWithoutExtension(AppPath);
        }
        private void MapDataToColumns() {
            TableName = "MpApp";
            columnData.Add("pk_MpAppId",this.appId);
            columnData.Add("fk_MpIconId",this.iconId);
            columnData.Add("SourcePath",this.AppPath);
            columnData.Add("IsAppRejected",this.IsAppRejected);
        }
    }
}
