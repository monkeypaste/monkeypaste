using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public enum MpFontStyleType {
        None = 0,
        Normal,
        Bold,
        Italic,
        Strikethrough,
        BoldItalic,
        BoldStrikethrough,
        BoldItalicStrikethrough,
        ItalicStrikethrough
    }
    public class MpRichTextStyle : MpDbObject {
        public int RichTextStyleId { get; set; }
        public int FontColorId { get; set; }
        public int BackgroundColorId { get; set; }
        public string FontFamilyName { get; set; }
        public MpFontStyleType FontStyleType { get; set; }
        public double FontSize { get; set; }

        public MpColor FontColor { get; set; }
        public MpColor BackgroundColor { get; set; }

        public MpRichTextStyle() : this(
            "Consolas",
            Colors.Black,
            Colors.White,
            12,
            MpFontStyleType.Normal) { }

        public MpRichTextStyle(
            string fontFamilyName, 
            Color fontColor,
            Color backgroundColor,
            double fontSize,
            MpFontStyleType fontStyle) {
            FontFamilyName = fontFamilyName;
            FontColor = new MpColor((int)fontColor.R, (int)fontColor.G, (int)fontColor.B, 255);
            BackgroundColor = new MpColor((int)backgroundColor.R, (int)backgroundColor.G, (int)backgroundColor.B, 255);
            FontSize = fontSize;
            FontStyleType = fontStyle;
        }
        public MpRichTextStyle(int richTextStyleId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpRichTextStyle where pk_MpRichTextStyleId=" + richTextStyleId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpRichTextStyle(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpRichTextStyle> GetAllRichTextStyles() {
            List<MpRichTextStyle> richTextStyles = new List<MpRichTextStyle>();
            DataTable dt = MpDb.Instance.Execute("select * from MpRichTextStyle");
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow r in dt.Rows) {
                    richTextStyles.Add(new MpRichTextStyle(r));
                }
            }
            return richTextStyles;
        }
        public override void LoadDataRow(DataRow dr) {
            RichTextStyleId = Convert.ToInt32(dr["pk_MpRichTextStyleId"].ToString());
            FontFamilyName = dr["FontFamilyName"].ToString();
            FontColorId = Convert.ToInt32(dr["fk_MpFontColorId"].ToString());
            FontColor = new MpColor(FontColorId);
        }

        public override void WriteToDatabase() {
            if (string.IsNullOrEmpty(FontFamilyName) || MpDb.Instance.NoDb) {
                Console.WriteLine("MpRichTextStyle Error, cannot create nameless RichTextStyle");
                return;
            }
            //if new RichTextStyle
            if (RichTextStyleId == 0) {
                FontColor.WriteToDatabase();
                FontColorId = FontColor.ColorId;
                MpDb.Instance.ExecuteNonQuery("insert into MpRichTextStyle(FontFamilyName,fk_MpFontColorId) values('" + FontFamilyName + "'," + FontColorId + ")");
                RichTextStyleId = MpDb.Instance.GetLastRowId("MpRichTextStyle", "pk_MpRichTextStyleId");
            } else {
                FontColor.WriteToDatabase();
                FontColorId = FontColor.ColorId;
                MpDb.Instance.ExecuteNonQuery("update MpRichTextStyle set FontFamilyName='" + FontFamilyName + "', fk_MpFontColorId=" + FontColorId + " where pk_MpRichTextStyleId=" + RichTextStyleId);
            }
        }
        public bool IsLinkedWithCopyItem(MpCopyItem ci) {
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItemRichTextStyle where fk_MpRichTextStyleId=" + RichTextStyleId + " and fk_MpCopyItemId=" + ci.CopyItemId);
            if (dt != null && dt.Rows.Count > 0) {
                return true;
            }
            return false;
        }
        public void LinkWithCopyItem(MpCopyItem ci) {
            if (IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpRichTextStyle Warning attempting to relink RichTextStyle " + RichTextStyleId + " with copyitem " + ci.copyItemId+" ignoring...");
                return;
            }
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItemRichTextStyle where fk_MpRichTextStyleId=" + this.RichTextStyleId);
            int SortOrderIdx = dt.Rows.Count + 1;
            MpDb.Instance.ExecuteNonQuery("insert into MpCopyItemRichTextStyle(fk_MpCopyItemId,fk_MpRichTextStyleId) values(" + ci.CopyItemId + "," + RichTextStyleId + ")");
            MpDb.Instance.ExecuteNonQuery("insert into MpCopyItemSortTypeOrder(fk_MpCopyItemId,fk_MpSortTypeId,SortOrder) values(" + ci.CopyItemId + "," + this.RichTextStyleId + "," + SortOrderIdx + ")");
            WriteToDatabase();
            Console.WriteLine("RichTextStyle link created between RichTextStyle " + RichTextStyleId + " with copyitem " + ci.CopyItemId);
        }
        public void UnlinkWithCopyItem(MpCopyItem ci) {
            if (!IsLinkedWithCopyItem(ci)) {
                //Console.WriteLine("MpRichTextStyle Warning attempting to unlink non-linked RichTextStyle " + RichTextStyleId + " with copyitem " + ci.copyItemId + " ignoring...");
                return;
            }
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemRichTextStyle where fk_MpCopyItemId=" + ci.CopyItemId + " and fk_MpRichTextStyleId=" + RichTextStyleId);
            //MpDb.Instance.ExecuteNonQuery("delete from MpRichTextStyleCopyItemSortOrder where fk_MpRichTextStyleId=" + this.RichTextStyleId);
            Console.WriteLine("RichTextStyle link removed between RichTextStyle " + RichTextStyleId + " with copyitem " + ci.CopyItemId + " ignoring...");
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteNonQuery("delete from MpRichTextStyle where pk_MpRichTextStyleId=" + this.RichTextStyleId);
            MpDb.Instance.ExecuteNonQuery("delete from MpCopyItemRichTextStyle where fk_MpRichTextStyleId=" + this.RichTextStyleId);
            //MpDb.Instance.ExecuteNonQuery("delete from MpRichTextStyleCopyItemSortOrder where fk_MpRichTextStyleId=" + this.RichTextStyleId);
        }
        private void MapDataToColumns() {
            TableName = "MpRichTextStyle";
            columnData.Clear();
            columnData.Add("pk_MpRichTextStyleId", this.RichTextStyleId);
            columnData.Add("fk_MpFontColorId", this.FontColorId);
            columnData.Add("FontFamilyName", this.FontFamilyName);
        }
    }
}
