using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpCopyItemTemplate : MpDbObject {
        public int CopyItemTemplateId { get; set; } = 0;
        public int CopyItemId { get; set; } = 0;
        public int ColorId { get; set; } = 0;

        public MpColor Color { get; set; }

        public Brush TemplateColor { 
            get {
                if(Color == null) {
                    Color = new MpColor(MpHelpers.Instance.GetRandomColor());
                }
                return Color.ColorBrush;
            }
            set {
                Color = new MpColor((value as SolidColorBrush).Color);
            }
        }

        public string TemplateName { get; set; } = String.Empty;
        //only set at runtime
        public string TemplateText { get; set; } = string.Empty;
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

        public MpCopyItemTemplate() : this(0,MpHelpers.Instance.GetRandomBrushColor(),"Default Template Name") { }

        public MpCopyItemTemplate(int ciid, Brush templateColor, string templateName) {
            CopyItemTemplateId = 0;
            CopyItemId = ciid;
            TemplateColor = templateColor;
            TemplateName = templateName;
        }

        public MpCopyItemTemplate(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            CopyItemTemplateId = Convert.ToInt32(dr["pk_MpCopyItemTemplateId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());

            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            Color = new MpColor(ColorId);
            TemplateName = dr["TemplateName"].ToString(); 
            //TemplateColor = (Brush)new BrushConverter().ConvertFromString(dr["HexColor"].ToString());
        }

        public override void WriteToDatabase() {
            Color.WriteToDatabase();
            ColorId = Color.ColorId;

            if (CopyItemTemplateId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpCopyItemTemplate(fk_MpCopyItemId,fk_MpColorId,TemplateName) values(@ciid,@cid,@tn)",
                        new System.Collections.Generic.Dictionary<string, object> {
                        { "@ciid", CopyItemId },
                        { "@cid", ColorId},
                        { "@tn", TemplateName }
                    });
                CopyItemTemplateId = MpDb.Instance.GetLastRowId("MpCopyItemTemplate", "pk_MpCopyItemTemplateId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpCopyItemTemplate set fk_MpCopyItemId=@ciid, fk_MpColorId=@cid, TemplateName=@tn where pk_MpCopyItemTemplateId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", CopyItemTemplateId },
                        { "@ciid", CopyItemId },
                        { "@cid", ColorId },
                        { "@tn", TemplateName }
                    });
            }
        }
        public void DeleteFromDatabase() {
            Color.DeleteFromDatabase();

            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTemplate where pk_MpCopyItemTemplateId=@citid",
                new Dictionary<string, object> {
                    { "@citid", CopyItemTemplateId }
                });
        }
    }
}
