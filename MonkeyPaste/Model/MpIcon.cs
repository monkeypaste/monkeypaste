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
        private string _path = null;

        private MpIcon() {}
        public MpIcon(int iconId,IntPtr sourceHandle) : base() {
            this.iconId = iconId;
            this.IconImage = GetIconImage(sourceHandle);
            this._path = GetProcessPath(sourceHandle);
            WriteToDatabase();
        }
        public MpIcon(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            this.iconId = Convert.ToInt32(dr["pk_MpIconId"].ToString());
            this.IconImage = MpHelperFunctions.Instance.ConvertByteArrayToImage((byte[])dr["IconBlob"]);
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
                if(MpSingletonController.Instance.GetMpData().Db.NoDb) {
                    this.iconId = ++TotalIconCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpSingletonController.Instance.GetMpData().Db.Execute("select * from MpIcon where IconBlob=@0",new List<string>() { "@0" },new List<object>() { MpHelperFunctions.Instance.ConvertImageToByteArray(this.IconImage) });
                if(dt.Rows.Count > 0) {
                    this.iconId = Convert.ToInt32(dt.Rows[0]["pk_MpIconId"]);
                    MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId=" + this.iconId,new List<string>() { "@0" },new List<object>() { MpHelperFunctions.Instance.ConvertImageToByteArray(this.IconImage) });
                    isNew = false;
                }
                else {
                    MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("insert into MpIcon(IconBlob) values(@0)",new List<string>() { "@0" },new List<object>() { MpHelperFunctions.Instance.ConvertImageToByteArray(this.IconImage) });
                    this.iconId = MpSingletonController.Instance.GetMpData().Db.GetLastRowId("MpIcon","pk_MpIconId");
                    isNew = true;
                }
            }
            else {
                MpSingletonController.Instance.GetMpData().Db.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId="+this.iconId,new List<string>() { "@0" },new List<object>() { MpHelperFunctions.Instance.ConvertImageToByteArray(this.IconImage) });                
            }
            MapDataToColumns();
            MpSingletonController.Instance.GetMpData().AddMpIcon(this);
            Console.WriteLine(isNew ? "Created ":"Updated "+ " MpIcon");
            Console.WriteLine(ToString());
        }
        private Image GetIconImage(IntPtr sourceHandle) {
            return Icon.ExtractAssociatedIcon(GetProcessPath(sourceHandle)).ToBitmap();
        }
        private void MapDataToColumns() {
            tableName = "MpIcon";
            columnData.Add("pk_MpIconId",this.iconId);
            columnData.Add("IconBlob",this.IconImage);
        }
    }    
}
