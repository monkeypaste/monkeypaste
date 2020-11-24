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
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpRichTextStyle where pk_MpRichTextStyleId=@rtsid",
                new Dictionary<string, object> {
                    { "@rtsid", richTextStyleId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpRichTextStyle(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpRichTextStyle> GetAllRichTextStyles() {
            List<MpRichTextStyle> richTextStyles = new List<MpRichTextStyle>();
            DataTable dt = MpDb.Instance.Execute("select * from MpRichTextStyle", null);
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
            if (string.IsNullOrEmpty(FontFamilyName)) {
                Console.WriteLine("MpRichTextStyle Error, cannot create RichTextStyle without font family name");
                return;
            }
            //if new RichTextStyle
            if (RichTextStyleId == 0) {
                FontColor.WriteToDatabase();
                FontColorId = FontColor.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "insert into MpRichTextStyle(FontFamilyName,fk_MpFontColorId) values(@ffn,@fcid)",
                    new Dictionary<string, object> {
                        { "@ffn", FontFamilyName },
                        { "@fcid", FontColorId }
                    });
                    //"'" + FontFamilyName + "'," + FontColorId + ")");
                RichTextStyleId = MpDb.Instance.GetLastRowId("MpRichTextStyle", "pk_MpRichTextStyleId");
            } else {
                FontColor.WriteToDatabase();
                FontColorId = FontColor.ColorId;
                MpDb.Instance.ExecuteWrite(
                    "update MpRichTextStyle set FontFamilyName=@ffn, fk_MpFontColorId=@fcid where pk_MpRichTextStyleId=@rtsid",
                    new Dictionary<string, object> {
                        { "@ffn", FontFamilyName },
                        { "@fcid", FontColorId },
                        { "@rtsid", RichTextStyleId }
                    });
            }
        }
       
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpRichTextStyle where pk_MpRichTextStyleId=@rtsid",
                new Dictionary<string, object> {
                    { "@rtsid", RichTextStyleId }
                });
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
