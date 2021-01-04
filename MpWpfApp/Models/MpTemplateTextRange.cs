using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTemplateTextRange : MpDbObject {
        public int TemplateTextRangeId { get; set; } = 0;
        public int CopyItemTemplateId { get; set; } = 0;
        public int StartIdx { get; set; } = 0;
        public int EndIdx { get; set; } = 0;

        public static List<MpTemplateTextRange> GetAllTextRangesForTemplate(int copyItemTemplateId) {
            var templateTextRangeList = new List<MpTemplateTextRange>();
            DataTable dt = MpDb.Instance.Execute("select * from MpTemplateTextRange where fk_MpCopyItemTemplateId=@citid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@citid", copyItemTemplateId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                foreach (DataRow dr in dt.Rows) {
                    templateTextRangeList.Add(new MpTemplateTextRange(dr));
                }
            }
            return templateTextRangeList;
        }
        public MpTemplateTextRange() : this(0,0,0) { }

        public MpTemplateTextRange(int copyItemTemplateId, int sIdx, int eIdx) {
            TemplateTextRangeId = 0;
            CopyItemTemplateId = copyItemTemplateId;
            StartIdx = sIdx;
            EndIdx = eIdx;
        }

        public MpTemplateTextRange(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            TemplateTextRangeId = Convert.ToInt32(dr["pk_MpTemplateTextRangeId"].ToString());
            CopyItemTemplateId = Convert.ToInt32(dr["fk_MpCopyItemTemplateId"].ToString());
            StartIdx = Convert.ToInt32(dr["StartIdx"].ToString());
            EndIdx = Convert.ToInt32(dr["EndIdx"].ToString());
        }

        public override void WriteToDatabase() {
            if (TemplateTextRangeId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpTemplateTextRange(fk_MpCopyItemTemplateId,StartIdx,EndIdx) values(@citid,@sidx,@eidx)",
                        new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", CopyItemTemplateId },
                        { "@sidx", StartIdx },
                        { "@eidx", EndIdx }
                    });
                TemplateTextRangeId = MpDb.Instance.GetLastRowId("MpTemplateTextRange", "pk_MpTemplateTextRangeId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpTemplateTextRange set fk_MpCopyItemTemplateId=@citid, StartIdx=@sidx, EndIdx=@eidx where pk_MpTemplateTextRangeId=@ttrid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@citid", CopyItemTemplateId },
                        { "@sidx", StartIdx },
                        { "@eidx", EndIdx },
                        { "@ttrid", TemplateTextRangeId }
                    });
            }
        }
        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpTemplateTextRange where pk_MpTemplateTextRangeId=@ttrid",
                new Dictionary<string, object> {
                    { "@ttrid", TemplateTextRangeId }
                });
        }
    }
}
