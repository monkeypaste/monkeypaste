using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpTextItem : MpCopyItem {
        public int TextItemId { get; set; }
        public string ItemText { get; set; }
        public List<MpSubTextToken> SubTextTokenList = new List<MpSubTextToken>();

        public MpTextItem(int textItemId,int copyItemId,string itemText) {
            TextItemId = textItemId;
            CopyItemId = copyItemId;
            ItemText = MpHelpers.IsStringRichText(itemText) ? itemText : MpHelpers.ConvertPlainTextToRichText(itemText);
        }
        public MpTextItem(int textItemId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpTextItem where pk_MpTextItemId=" + textItemId);
            if(dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpTextItem(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            TextItemId = Convert.ToInt32(dr["pk_MpTextItemId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            ItemText = dr["ItemText"].ToString();

            var dt = MpDb.Instance.Execute("select * from MpCopyItem where pk_MpCopyItemId=" + CopyItemId);
            if (dt != null && dt.Rows.Count > 0) {
                base.LoadDataRow(dt.Rows[0]);
            }

            //get subtokens
            dt = MpDb.Instance.Execute("select * from MpSubTextToken where fk_MpCopyItemId=" + CopyItemId);
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow row in dt.Rows) {
                    SubTextTokenList.Add(new MpSubTextToken(row));
                }
            }

            MapDataToColumns();
        }

        public override void WriteToDatabase() {
            if(string.IsNullOrEmpty(ItemText) || MpDb.Instance.NoDb) {
                Console.WriteLine("MpTextItem Error, cannot create empty text item");
                return;
            }
            //if new tag
            if(TextItemId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpTextItem(fk_MpCopyItemId,ItemText) values(" + CopyItemId + ",'" + ItemText + "')");
                TextItemId = MpDb.Instance.GetLastRowId("MpTextItem", "pk_MpTextItemId");                 
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpTextItem set ItemText='" + ItemText + "', fk_MpCopyItemId=" + CopyItemId+" where pk_MpTextItemId="+TextItemId);                
            }

            foreach (MpSubTextToken subToken in SubTextTokenList) {
                subToken.CopyItemId = CopyItemId;
                subToken.WriteToDatabase();
            }
        }
        public override void DeleteFromDatabase() {
            base.DeleteFromDatabase();
            MpDb.Instance.ExecuteNonQuery("delete from MpTextItem where fk_MpCopyItemId=" + CopyItemId);
            
        }
        private void MapDataToColumns() {
            TableName = "MpTextItem";
            columnData.Clear();
            columnData.Add("pk_MpTextItemId",TextItemId);
            columnData.Add("fk_MpCopyItemId", CopyItemId);
            columnData.Add("ItemText", ItemText);
        }

        public override MpCopyItem GetExistingCopyItem() {
            var dt = MpDb.Instance.Execute("select * from MpTextItem");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    int tiId = Convert.ToInt32(dr["pk_MpTextItemId"].ToString());
                    string tiItemText = dr["ItemText"].ToString();
                    if(ItemText == tiItemText) {
                        return new MpTextItem(dr);
                    }
                }
            }
            return null;
        }

        public override string GetPlainText() {
            RichTextBox rtb = new RichTextBox();
            rtb.SetRtf(ItemText);
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.Replace("''", "'");
        }
    }
}
