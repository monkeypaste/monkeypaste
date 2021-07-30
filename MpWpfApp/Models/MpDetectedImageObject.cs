using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDetectedImageObject : MpDbModelBase {
        public int DetectedImageObjectId { get; set; } = 0;
        public int CopyItemId { get; set; }

        public double Confidence { get; set; } = 0;

        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
        
        public double Width { get; set; } = 0;
        public double Height { get; set; } = 0;

        public string ObjectTypeName { get; set; } = String.Empty;

        public static List<MpDetectedImageObject> GetAllObjectsForItem(int copyItemId) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpDetectedImageObject where fk_MpCopyItemId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@cid", copyItemId }
                    });
            var objectList = new List<MpDetectedImageObject>();
            if(dt != null && dt.Rows.Count > 0) {
                foreach(DataRow dr in dt.Rows) {
                    objectList.Add(new MpDetectedImageObject(dr));
                }
            }
            return objectList;
        }

        public MpDetectedImageObject() : this(0,0,0,0,0,0,0,string.Empty) { }

        public MpDetectedImageObject(int dioid, int cid, double c, double x, double y, double w, double h, string tcsv) {
            DetectedImageObjectId = dioid;
            CopyItemId = cid;
            Confidence = c;
            X = x;
            Y = y;
            Width = w;
            Height = h;
            ObjectTypeName = tcsv;
        }

        public MpDetectedImageObject(DataRow dr) {
            LoadDataRow(dr);
        }

        public override string ToString() {
            return string.Format(
                "Type: {0} Bounding Box: ({1},{2},{3},{4}) Confidence: {5} CopyItemId: {6}",
                ObjectTypeName, X, Y, Width, Height, Confidence, CopyItemId);
        }

        public override void LoadDataRow(DataRow dr) {
            DetectedImageObjectId = Convert.ToInt32(dr["pk_MpDetectedImageObjectId"].ToString());
            CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            Confidence = Convert.ToDouble(dr["Confidence"].ToString());
            X = Convert.ToDouble(dr["X"].ToString());
            Y = Convert.ToDouble(dr["Y"].ToString());
            Width = Convert.ToDouble(dr["Width"].ToString());
            Height = Convert.ToDouble(dr["Height"].ToString());
            ObjectTypeName = dr["ObjectTypeName"].ToString();
        }

        public override void WriteToDatabase() {
            if(DetectedImageObjectId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpDetectedImageObject(fk_MpCopyItemId,Confidence,X,Y,Width,Height,ObjectTypeName) " +
                    "values (@cid,@c,@x,@y,@w,@h,@tcsv)",
                    new Dictionary<string, object> {
                            { "@cid", CopyItemId },
                            { "@c", Confidence },
                            { "@x", X },
                            { "@y", Y },
                            { "@w", Width },
                            { "@h", Height },
                            { "@tcsv", ObjectTypeName }
                        });

                DetectedImageObjectId = MpDb.Instance.GetLastRowId("MpDetectedImageObject", "pk_MpDetectedImageObjectId");
            } else {
                MpDb.Instance.ExecuteWrite(
                        "update MpDetectedImageObject set Confidence=@c,X=@x,Y=@y,Width=@w,Height=@h,ObjectTypeName=@tcsv where pk_MpDetectedImageObjectId=@dioid",
                        new Dictionary<string, object> {
                            { "@c", Confidence },
                            { "@x", X },
                            { "@y", Y },
                            { "@w", Width },
                            { "@h", Height },
                            { "@tcsv", ObjectTypeName },
                            { "@dioid", DetectedImageObjectId }
                        });
            }
        }

        public void DeleteFromDatabase() {
            MpDb.Instance.ExecuteWrite(
                "delete from MpDetectedImageObject where pk_MpDetectedImageObjectId=@dioid",
                new System.Collections.Generic.Dictionary<string, object> {
                        { "@dioid", DetectedImageObjectId }
                    });
        }
    }
}
