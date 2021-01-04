using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpCopyItemTemplate : MpDbObject {
        public int CopyItemTemplateId { get; set; } = 0;
        public int CopyItemId { get; set; } = 0;
        public Brush TemplateColor { get; set; } = Brushes.Red;
        public string TemplateName { get; set; } = String.Empty;
        public List<MpTemplateTextRange> TemplateTextRangeList = new List<MpTemplateTextRange>();

        public static List<MpCopyItemTemplate> GetAllTemplatesForCopyItem(int copyItemId) {
            var copyItemTemplateList = new List<MpCopyItemTemplate>();
            DataTable dt = MpDb.Instance.Execute("select * from MpCopyItemTemplate where fk_MpCopyItemId=@ciid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@ciid", copyItemId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    copyItemTemplateList.Add(new MpCopyItemTemplate(dr));
                }
            }
            return copyItemTemplateList;
        }

        public MpCopyItemTemplate() : this(0,MpHelpers.GetRandomBrushColor(),"Default Template Name") { }

        public MpCopyItemTemplate(int ciid, Brush templateColor, string templateName) {
            CopyItemTemplateId = 0;
            CopyItemId = ciid;
            TemplateColor = templateColor;
            TemplateName = templateName;
            TemplateTextRangeList = new List<MpTemplateTextRange>();
        }

        public MpCopyItemTemplate(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            CopyItemTemplateId = Convert.ToInt32(dr["pk_MpCopyItemTemplateId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            TemplateName = dr["TemplateName"].ToString();
            TemplateColor = (Brush)new BrushConverter().ConvertFromString(dr["HexColor"].ToString());
            TemplateTextRangeList = MpTemplateTextRange.GetAllTextRangesForTemplate(CopyItemTemplateId);
        }

        public override void WriteToDatabase() {
            if (CopyItemTemplateId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpCopyItemTemplate(fk_MpCopyItemId,HexColor,TemplateName) values(@ciid,@hc,@tn)",
                        new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId },
                        { "@hc", ((SolidColorBrush)TemplateColor).Color.ToString() },
                        { "@tn", TemplateName }
                    });
                CopyItemTemplateId = MpDb.Instance.GetLastRowId("MpCopyItemTemplate", "pk_MpCopyItemTemplateId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpCopyItemTemplate set fk_MpCopyItemId=@ciid, HexColor=@hc, TemplateName=@tn where pk_MpCopyItemTemplateId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", CopyItemTemplateId },
                        { "@ciid", CopyItemId },
                        { "@hc", ((SolidColorBrush)TemplateColor).Color.ToString() },
                        { "@tn", TemplateName }
                    });
            }
            foreach(var ttr in TemplateTextRangeList) {
                ttr.CopyItemTemplateId = CopyItemTemplateId;
                ttr.WriteToDatabase();
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTemplate where pk_MpCopyItemTemplateId=@citid",
                new Dictionary<string, object> {
                    { "@citid", CopyItemTemplateId }
                });
            foreach (var ttr in TemplateTextRangeList) {
                ttr.DeleteFromDatabase();
            }
        }
    }
}
