using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpImageItem : MpCopyItem {
        public int ImageItemId { get; set; }
        public List<byte> ItemImage { get; set; } = new List<byte>();

        public MpImageItem(int imageItemId, int copyItemId, List<byte> itemImage) {
            ImageItemId = imageItemId;
            CopyItemId = copyItemId;
            ItemImage = itemImage;
        }
        public MpImageItem(int imageItemId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpImageItem where pk_MpImageItemId=" + ImageItemId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpImageItem(DataRow dr) : base(dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            ImageItemId = Convert.ToInt32(dr["pk_MpImageItemId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            foreach (byte b in (byte[])dr["ItemImage"]) {
                ItemImage.Add(b);
            }

            var dt = MpDb.Instance.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
            if (dt != null && dt.Rows.Count > 0) {
                base.LoadDataRow(dt.Rows[0]);
            }
        }

        public override void WriteToDatabase() {
            if (ItemImage.Count == 0 || MpDb.Instance.NoDb) {
                Console.WriteLine("MpImageItem Error, cannot create no byte imagde");
                return;
            }
            //if new image item
            if (ImageItemId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpImageItem(fk_MpCopyItemId,ItemImage) values(" + CopyItemId + ",@0)",new List<string>() { "@0" },new List<object>() { ItemImage.ToArray() });
                ImageItemId = MpDb.Instance.GetLastRowId("MpImageItem", "pk_MpImageItemId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpImageItem set fk_MpCopyItemId=" + CopyItemId + ", ItemImage=@0 where pk_MpImageItemId=" + ImageItemId, new List<string>() { "@0" }, new List<object>() { ItemImage.ToArray() });
            }
        }
        public override void DeleteFromDatabase() {
            base.DeleteFromDatabase();
            MpDb.Instance.ExecuteNonQuery("delete from MpImageItem where fk_MpCopyItemId=" + CopyItemId);            
        }
        private void MapDataToColumns() {
            TableName = "MpImageItem";
            columnData.Clear();
            columnData.Add("pk_MpImageItemId", ImageItemId);
            columnData.Add("fk_MpCopyItemId", CopyItemId);
        }

        public override MpCopyItem GetExistingCopyItem() {
            var dt = MpDb.Instance.Execute("select * from MpImageItem");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    int iId = Convert.ToInt32(dr["pk_MpImageItemId"].ToString());
                    byte[] iImage = (byte[])dr["ItemImage"];
                    if (ItemImage.ToArray() == iImage) {
                        return new MpImageItem(dr);
                    }
                }
            }
            return null;
        }

        public override string GetPlainText() {
            return "[IMAGE]";
        }
    }
}
