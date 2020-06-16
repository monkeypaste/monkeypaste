using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MpWinFormsClassLibrary;

namespace MpWpfApp {
    public class MpIcon : MpDbObject {
        public static int TotalIconCount = 0;
        public int iconId { get; set; } 
       // public Image IconImage { get; set; }
        public ImageSource IconImage { get; set; }
        //    get {
        //        BitmapImage bi = new BitmapImage();
        //        bi.BeginInit();
        //        bi.StreamSource = new MemoryStream(MpHelperSingleton.Instance.ImageConverter.ConvertImageSourceToByteArray(IconImage));
        //        bi.EndInit();
        //        return bi;
        //    }
        //}
        //public string Path { get; set; }

        public MpIcon() {
            iconId = 0;
            IconImage = null;
            ++TotalIconCount;
        }
        public MpIcon(ImageSource iconImage) : base() {
            this.iconId = 0;
            this.IconImage = iconImage;
            ++TotalIconCount;
            //this.Path = MpHelperSingleton.Instance.GetProcessPath(sourceHandle);
            //WriteToDatabase();
        }
        /*public MpIcon(IntPtr sourceHandle) {
            IconImage = MpHelperSingleton.Instance.GetIconImage(sourceHandle);
            string appPath = MpHelperSingleton.Instance.GetProcessPath(sourceHandle);
            DataTable dt_app = MpAppController.DataModel.Db.Execute("select * from MpApp where SourcePath='" + appPath + "'");
            if(dt_app != null && dt_app.Rows.Count > 0) {
                iconId = Convert.ToInt32(dt_app.Rows[0]["fk_MpIconId"].ToString());
                DataTable dt_icon = MpAppController.DataModel.Db.Execute("select * from MpIcon where pk_MpIconId=" + iconId);
                if(dt_icon != null && dt_icon.Rows.Count > 0) {
                    LoadDataRow(dt_icon.Rows[0]);
                    return;
                }
            }
            WriteToDatabase();
        }*/
        public MpIcon(int iconId) {
            DataTable dt = MpDataStore.Instance.Db.Execute("select * from MpIcon where pk_MpIconId=" + iconId);
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
            this.IconImage = MpHelperSingleton.Instance.ImageConverter.ConvertByteArrayToImageSource((byte[])dr["IconBlob"]);
            MapDataToColumns();
            Console.WriteLine("Loaded MpIcon");
            Console.WriteLine(ToString());
        }
        public override void WriteToDatabase() {            
            bool isNew = false;
            if(IconImage == null) {
                throw new Exception("Error creating MpIcon Image cannot be null");
            }
            if(iconId == 0) {
                if(MpDataStore.Instance.Db.NoDb) {
                    this.iconId = ++TotalIconCount;
                    MapDataToColumns();
                    return;
                }
                DataTable dt = MpDataStore.Instance.Db.Execute("select * from MpIcon where IconBlob=@0",new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ImageConverter.ConvertImageSourceToByteArray(this.IconImage) });
                if(dt.Rows.Count > 0) {
                    this.iconId = Convert.ToInt32(dt.Rows[0]["pk_MpIconId"]);
                    MpDataStore.Instance.Db.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId=" + this.iconId,new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ImageConverter.ConvertImageSourceToByteArray(this.IconImage) });
                    isNew = false;
                }
                else {
                    MpDataStore.Instance.Db.ExecuteNonQuery("insert into MpIcon(IconBlob) values(@0)",new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ImageConverter.ConvertImageSourceToByteArray(this.IconImage) });
                    this.iconId = MpDataStore.Instance.Db.GetLastRowId("MpIcon","pk_MpIconId");
                    isNew = true;
                }
            }
            else {
                MpDataStore.Instance.Db.ExecuteNonQuery("update MpIcon set IconBlob=@0 where pk_MpIconId="+this.iconId,new List<string>() { "@0" },new List<object>() { MpHelperSingleton.Instance.ImageConverter.ConvertImageSourceToByteArray(this.IconImage) });                
            }
            if(isNew) {
                MapDataToColumns();
            }
            Console.WriteLine(isNew ? "Created ":"Updated "+ " MpIcon");
            Console.WriteLine(ToString());
        }
        
        private void MapDataToColumns() {
            TableName = "MpIcon";
            columnData.Add("pk_MpIconId",this.iconId);
            columnData.Add("IconBlob",this.IconImage);
        }
    }    
}
