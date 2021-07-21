using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using Newtonsoft.Json;

namespace MpWpfApp {   
    public class MpDbLog : MpDbObject, MpISyncableDbObject {
        private static List<MpDbLog> _AllDbLogList = null;
        public static int TotalDbLogCount = 0;

        #region Events
        public static event EventHandler<MpDbLog> OnItemAdded;
        public static event EventHandler<MpDbLog> OnItemUpdated;
        public static event EventHandler<MpDbLog> OnItemDeleted;
        #endregion


        public int DbLogId { get; set; }
        public Guid DbObjectGuid { get; set; }
        public string DbTableName { get; set; }
        public string AffectedColumnName { get; set; } = "UnknownColumnName";
        public string AffectedColumnValue { get; set; } = "UnknownColumnValue";
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

        public static List<MonkeyPaste.MpDbLog> GetDbLogsByGuid(string dboGuid, DateTime fromDateUtc) {
            if (_AllDbLogList == null) {
                GetAllDbLogs();
            }
            var wpfLogs = _AllDbLogList
                    .Where(x => x.DbObjectGuid.ToString() == dboGuid && x.LogActionDateTime > fromDateUtc)
                    .ToList();

            var logs = new List<MonkeyPaste.MpDbLog>();
            foreach(var wpfl in wpfLogs) {
                var l = new MonkeyPaste.MpDbLog() {
                    ObjectGuid = wpfl.DbObjectGuid.ToString(),
                    DbTableName = wpfl.DbTableName,
                    ActionType = (int)wpfl.LogActionType,
                    LogActionDateTime = wpfl.LogActionDateTime,
                    SourceClientGuid = wpfl.SourceClientGuid,
                    AffectedColumnName = wpfl.AffectedColumnName,
                    AffectedColumnValue = wpfl.AffectedColumnValue
                };
                logs.Add(l);
            }
            return logs;
        }

        public static List<MonkeyPaste.MpDbLog> FilterOutdatedRemoteLogs(string dboGuid, List<MonkeyPaste.MpDbLog> rlogs, DateTime lastSyncDt) {
            //this is an update so cross check local log and only apply updates more recent
            //than what is local

            //sort logs by transaction date time so most recent changes applied last
            rlogs = rlogs.OrderBy(x => x.LogActionDateTime).ToList();
            var remoteLogsMinDt = rlogs.FirstOrDefault().LogActionDateTime;
            var rlogsToRemove = new List<MonkeyPaste.MpDbLog>();
            //query local db and get logs for item since oldest remote transaction datetime
            var llogs = MpDbLog.GetDbLogsByGuid(dboGuid, remoteLogsMinDt);
            foreach (var rlog in rlogs) {
                if(rlog.LogActionDateTime < lastSyncDt) {
                    rlogsToRemove.Add(rlog);
                } else {
                    foreach (var llog in llogs) {
                        if (rlog.AffectedColumnName == llog.AffectedColumnName &&
                           rlog.LogActionDateTime < llog.LogActionDateTime) {
                            rlogsToRemove.Add(rlog);
                            //break so rlog entries are not duplicated
                            break;
                        }
                    }
                }
            }
            //remove outdated remote changes
            foreach (var rlogToRemove in rlogsToRemove) {
                rlogs.Remove(rlogToRemove);
            }
            return rlogs;
        }

        public MpDbLog() { }

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
            string affectedColumnName,
            string affectedColumnValue,
            MpDbLogActionType actionType, 
            DateTime actionDateTime, 
            Guid sourceClientGuid) {
            DbObjectGuid = dbObjectGuid;
            DbTableName = tableName;
            AffectedColumnName = affectedColumnName;
            AffectedColumnValue = affectedColumnValue;
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
            AffectedColumnName = dr["AffectedColumnName"].ToString();
            AffectedColumnValue = dr["AffectedColumnValue"].ToString();
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

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (DbLogId == 0) {
                MpDb.Instance.ExecuteWrite(
                        "insert into MpDbLog(DbObjectGuid,DbTableName,AffectedColumnName,AffectedColumnValue,LogActionType,LogActionDateTime,SourceClientGuid) " +
                        "values(@dbog,@dbtn,@acn,@acv,@lat,@ladt,@scg)",
                        new System.Collections.Generic.Dictionary<string, object> {
                            { "@dbog",DbObjectGuid.ToString() },
                            { "@dbtn", DbTableName },
                            { "@acn",AffectedColumnName },
                            { "@acv",AffectedColumnValue },
                            { "@lat", (int)LogActionType },
                            { "@ladt",LogActionDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                            { "@scg", SourceClientGuid.ToString() }
                    },DbObjectGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);
                DbLogId = MpDb.Instance.GetLastRowId("MpDbLog", "pk_MpDbLogId");
                OnItemAdded?.Invoke(this, this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpDbLog set DbObjectGuid=@dbog,DbTableName=@dbtn,AffectedColumnName=@acn,AffectedColumnValue=@acv,LogActionType=@lat,LogActionDateTime=@ladt,SourceClientGuid=@scg where pk_MpDbLogId=@dblid",
                    new System.Collections.Generic.Dictionary<string, object> {
                        { "@dblid", DbLogId },
                        { "@dbog",DbObjectGuid.ToString() },
                        { "@dbtn", DbTableName },
                        { "@acn",AffectedColumnName },
                        { "@acv",AffectedColumnValue },
                        { "@lat", (int)LogActionType },
                        { "@ladt",LogActionDateTime.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "@scg", SourceClientGuid.ToString() }
                    }, DbObjectGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                OnItemUpdated?.Invoke(this, this);
            }
            var al = GetAllDbLogs().Where(x => x.DbLogId == DbLogId).ToList();
            if (al.Count > 0) {
                _AllDbLogList[_AllDbLogList.IndexOf(al[0])] = this;
            } else {
                _AllDbLogList.Add(this);
            }
        }

        public override void WriteToDatabase() {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid, false, false);
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

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);

            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);

            var dbLog = new MpDbLog() {
                DbObjectGuid = System.Guid.Parse(objParts[0]),
                DbTableName = objParts[1],
                AffectedColumnName = objParts[2],
                AffectedColumnValue = objParts[3],
                LogActionType = (MpDbLogActionType)Convert.ToInt32(objParts[4]),
                LogActionDateTime = DateTime.Parse(objParts[5]),
                SourceClientGuid = System.Guid.Parse(objParts[6])
            };
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}",
                ParseToken,
                DbObjectGuid.ToString(),
                DbTableName,
                AffectedColumnName,
                AffectedColumnValue,
                (int)LogActionType,
                LogActionDateTime.ToString(),
                SourceClientGuid.ToString());
        }

        public Type GetDbObjectType() {
            return typeof(MpDbLog);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            throw new NotImplementedException();
        }

        public Task<object> CreateFromLogs(string dboGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            throw new NotImplementedException();
        }
    }
}
