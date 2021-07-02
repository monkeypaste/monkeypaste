using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;

namespace MpWpfApp {   
    public class MpDbLog : MpDbObject {
        private static List<MpDbLog> _AllDbLogList = null;
        public static int TotalDbLogCount = 0;

        public int DbLogId { get; set; }
        public Guid DbObjectGuid { get; set; }
        public string DbTableName { get; set; }
        public MpDbLogActionType LogActionType { get; set; } = MpDbLogActionType.None;
        public DateTime LogActionDateTime { get; set; }
        public Guid SourceClientGuid { get; set; }

        public static List<MpDbLog> GetAllDbLogs() {
            if(_AllDbLogList == null) {
                _AllDbLogList = new List<MpDbLog>();
                DataTable dt = MpDb.Instance.Execute("select * from MpDbLog", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllDbLogList.Add(new MpDbLog(dr));
                    }
                }
            }
            return _AllDbLogList;
        }
        public static MpDbLog GetDbLogById(int DbLogId) {
            if (_AllDbLogList == null) {
                GetAllDbLogs();
            }
            var udbpl = _AllDbLogList.Where(x => x.DbLogId == DbLogId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public MpDbLog(int DbLogId) {
            DataTable dt = MpDb.Instance.Execute(
                "select * from MpDbLog where pk_MpDbLogId=@cid",
                new System.Collections.Generic.Dictionary<string, object> {
                    { "@cid", DbLogId }
                });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpDbLog(
            Guid dbObjectGuid, 
            string tableName, 
            MpDbLogActionType actionType, 
            DateTime actionDateTime, 
            Guid sourceClientGuid) {
            DbObjectGuid = dbObjectGuid;
            DbTableName = tableName;
            LogActionType = actionType;
            LogActionDateTime = actionDateTime;
            SourceClientGuid = sourceClientGuid;
        }
        public MpDbLog(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            DbLogId = Convert.ToInt32(dr["pk_MpDbLogId"].ToString());
            DbObjectGuid = Guid.Parse(dr["DbObjectGuid"].ToString());
            DbTableName = dr["DbTableName"].ToString();
            LogActionType = (MpDbLogActionType)Convert.ToInt32(dr["LogActionType"].ToString());
            LogActionDateTime = DateTime.Parse(dr["LogActionDateTime"].ToString());
            SourceClientGuid = Guid.Parse(dr["SourceClientGuid"].ToString());
        }
        //public void DeleteFromDatabase() {
        //    if (DbLogId <= 0) {
        //        return;
        //    }

        //    MpDb.Instance.ExecuteWrite(
        //        "delete from MpDbLog where pk_MpDbLogId=@cid",
        //        new Dictionary<string, object> {
        //            { "@cid", DbLogId }
        //        });
        //}

        public override void WriteToDatabase() {
            if (DbLogId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpDbLog(DbObjectGuid,DbTableName,LogActionType,LogActionDateTime,SourceClientGuid) " +
                        "values(@dbog,@dbtn,@lat,@ladt,@scg)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@dbog",DbObjectGuid.ToString() },
                            { "@dbtn", DbTableName },
                            { "@lat", (int)LogActionType },
                            { "@ladt",LogActionDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@scg", SourceClientGuid.ToString() }
                    });
                DbLogId = MpDb.Instance.GetLastRowId("MpDbLog", "pk_MpDbLogId");
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpDbLog set DbObjectGuid=@dbog,DbTableName=@dbtn,LogActionType=@lat,LogActionDateTime=@ladt,SourceClientGuid=@scg where pk_MpDbLogId=@dblid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@dblid", DbLogId },
                        { "@dbog",DbObjectGuid.ToString() },
                        { "@dbtn", DbTableName },
                        { "@lat", (int)LogActionType },
                        { "@ladt",LogActionDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "@scg", SourceClientGuid.ToString() }
                    });
            }
            var al = GetAllDbLogs().Where(x => x.DbLogId == DbLogId).ToList();
            if (al.Count > 0) {
                _AllDbLogList[_AllDbLogList.IndexOf(al[0])] = this;
            } else {
                _AllDbLogList.Add(this);
            }        
        }

        public void DeleteFromDatabase() {
            if (DbLogId <= 0) {
                return;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpDbLog where pk_MpDbLogId=@cid",
                new Dictionary<string, object> {
                    { "@cid", DbLogId }
                });
        }
    }
}
