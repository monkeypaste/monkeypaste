using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpIcon : MpDBObject {
        public static int TotalIconCount = 0;
        public int iconId { get; set; }
        public Image IconImage { get; set; }
        public string Path { get; set; }

        private MpIcon() {}
        public MpIcon(int iconId,IntPtr sourceHandle) : base() {
            this.iconId = iconId;
            this.IconImage = MpHelperSingleton.Instance.GetIconImage(sourceHandle);
            this.Path = MpHelperSingleton.Instance.GetProcessPath(sourceHandle);
            WriteToDatabase();
        }
        public MpIcon(IntPtr sourceHandle) {
            IconImage = MpHelperSingleton.Instance.GetIconImage(sourceHandle);
            Path = MpHelperSingleton.Instance.GetProcessPath(sourceHandle);
            DataTable dt_app = MpLogFormController.Db.Execute("select * from MpApp where SourcePath='" + Path + "'");
            if(dt_app != null && dt_app.Rows.Count > 0) {
                iconId = Convert.ToInt32(dt_app.Rows[0]["fk_MpIconId"].ToString());
                DataTable dt_icon = MpLogFormController.Db.Execute("select * from MpIcon where pk_MpIconId=" + iconId);
                if(dt_icon != null && dt_icon.Rows.Count > 0) {
                    LoadDataRow(dt_icon.Rows[0]);
                    return;
                }
            }
            WriteToDatabase();
        }
        public MpIcon(int iconId) {
            DataTable dt = MpLogFormController.Db.Execute("select * from MpIcon where pk_MpIconId=" + iconId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            } else {
                throw new Exception("MpIcon error trying access unknown icon w/ pk: " + iconId);
            }
        }
        public MpIcon(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            this.iconId = Convert.ToInt32(dr["pk_MpIconId"].ToString());
            this.IconImage = MpHelperSingleton.Instance.ConvertByteArrayToImage((byte[])dr["IconBlob"]);
            MapDataToColumns();
            Console.WriteLine("Loaded MpIcon");
            Console.WriteLine(ToString());
        }
        public override void WriteToDatabase() {
            
            bool isNew = false;
            if(IconImage == null) {
                Console.WriteLine("Error creating MpIcon Image cannot be null");
                return;
            }
            if(iconId == 0) {
                if(MpLogFormController.Db.NoDb) {
                    this.iconId = ++TotalIconCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpLogFormController.Db.Execute("select * from MpIcon where IconBlob=@0",new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ConvertImageToByteArray(this.IconImage) });
                if(dt.Rows.Count > 0) {
                    this.iconId = Convert.ToInt32(dt.Rows[0]["pk_MpIconId"]);
                    MpLogFormController.Db.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId=" + this.iconId,new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ConvertImageToByteArray(this.IconImage) });
                    isNew = false;
                }
                else {
                    MpLogFormController.Db.ExecuteNonQuery("insert into MpIcon(IconBlob) values(@0)",new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ConvertImageToByteArray(this.IconImage) });
                    this.iconId = MpLogFormController.Db.GetLastRowId("MpIcon","pk_MpIconId");
                    isNew = true;
                }
            }
            else {
                MpLogFormController.Db.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId="+this.iconId,new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ConvertImageToByteArray(this.IconImage) });                
            }
            MapDataToColumns();
            //MpSingletonController.Instance.GetMpData().AddMpIcon(this);
            Console.WriteLine(isNew ? "Created ":"Updated "+ " MpIcon");
            Console.WriteLine(ToString());
        }
        
        private void MapDataToColumns() {
            tableName = "MpIcon";
            columnData.Add("pk_MpIconId",this.iconId);
            columnData.Add("IconBlob",this.IconImage);
        }
    }    
}
