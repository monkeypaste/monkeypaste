using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpCopyItemTemplate : MpDbModelBase {
        public int CopyItemTemplateId { get; set; } = 0;
        public Guid CopyItemTemplateGuid { get; set; }

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
            CopyItemTemplateGuid = Guid.NewGuid();
            CopyItemId = ciid;
            TemplateColor = templateColor;
            TemplateName = templateName;
        }

        public MpCopyItemTemplate(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            CopyItemTemplateId = Convert.ToInt32(dr["pk_MpCopyItemTemplateId"].ToString());
            CopyItemTemplateGuid = Guid.Parse(dr["MpCopyItemTemplateGuid"].ToString());

            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());

            ColorId = Convert.ToInt32(dr["fk_MpColorId"].ToString());
            Color = new MpColor(ColorId);
            TemplateName = dr["TemplateName"].ToString(); 
            //TemplateColor = (Brush)new BrushConverter().ConvertFromString(dr["HexColor"].ToString());
        }

        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            Color.WriteToDatabase();
            ColorId = Color.ColorId;

            if (CopyItemTemplateId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpCopyItemTemplate(MpCopyItemTemplateGuid,fk_MpCopyItemId,fk_MpColorId,TemplateName) values(@citg,@ciid,@cid,@tn)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@citg", CopyItemTemplateGuid.ToString() },
                            { "@ciid", CopyItemId },
                            { "@cid", ColorId},
                            { "@tn", TemplateName }
                    },CopyItemTemplateGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                CopyItemTemplateId = MpDb.Instance.GetLastRowId("MpCopyItemTemplate", "pk_MpCopyItemTemplateId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpCopyItemTemplate set MpCopyItemTemplateGuid=@citg, fk_MpCopyItemId=@ciid, fk_MpColorId=@cid, TemplateName=@tn where pk_MpCopyItemTemplateId=@citid",
                    new System.Collections.Generic.Dictionary<string, object> {
                            { "@citg", CopyItemTemplateGuid.ToString() },
                            { "@citid", CopyItemTemplateId },
                            { "@ciid", CopyItemId },
                            { "@cid", ColorId },
                            { "@tn", TemplateName }
                    },CopyItemTemplateGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
            }
        }
        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {

            Color.DeleteFromDatabase();

            MpDb.Instance.ExecuteWrite(
                "delete from MpCopyItemTemplate where pk_MpCopyItemTemplateId=@citid",
                new Dictionary<string, object> {
                    { "@citid", CopyItemTemplateId }
                },CopyItemTemplateGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
        }
    }
}
