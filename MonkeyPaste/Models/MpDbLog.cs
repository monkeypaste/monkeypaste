using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpDbLogActionType {
        None = 0,
        Create,
        Modify,
        Delete,
        Sync
    }

    public class MpDbLog : MpDbModelBase, MpISyncableDbObject {
        [Column("pk_MpDbLogId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("DbObjectGuid")]
        public string ObjectGuid { get; set; }

        public string AffectedColumnName { get; set; } = @"UnknownColumnName";

        public string AffectedColumnValue { get; set; } = @"UnknownColumnValue";

        [Ignore]
        public Guid DbObjectGuid {
            get {
                if (string.IsNullOrEmpty(ObjectGuid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(ObjectGuid);
            }
            set {
                ObjectGuid = value.ToString();
            }
        }

        public string DbTableName { get; set; }

        [Column("LogActionType")]
        public int ActionType { get; set; }

        [Ignore]
        public MpDbLogActionType LogActionType {
            get {
                return (MpDbLogActionType)ActionType;
            }
            set {
                ActionType = (int)value;
            }
        }

        public DateTime LogActionDateTime { get; set; }

        [Column("SourceClientGuid")]
        public string ClientGuid { get; set; }

        [Ignore]
        public Guid SourceClientGuid {
            get {
                if (string.IsNullOrEmpty(ClientGuid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(ClientGuid);
            }
            set {
                ClientGuid = value.ToString();
            }
        }

        public static async Task<List<MpDbLog>> FilterOutdatedRemoteLogs(string dboGuid, List<MpDbLog> rlogs, DateTime lastSyncDt) {
            //this is an update so cross check local log and only apply updates more recent
            //than what is local            
            //sort logs by transaction date time so most recent changes applied last
            rlogs = rlogs.OrderBy(x => x.LogActionDateTime).ToList();
            var remoteLogsMinDt = rlogs.FirstOrDefault().LogActionDateTime;
            var rlogsToRemove = new List<MpDbLog>();
            //query local db and get logs for item since oldest remote transaction datetime
            var llogs = await MpDataModelProvider.GetDbLogsByGuidAsync(dboGuid, remoteLogsMinDt);
            foreach (var rlog in rlogs) {
                if (rlog.LogActionDateTime < lastSyncDt) {
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

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
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
            await Task.Delay(1);
            return dbLog;
        }

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);

            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}",
                ParseToken,
                ObjectGuid,
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

        public Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            throw new NotImplementedException();
        }

        public MpDbLog() { }

        public MpDbLog(
            Guid dbObjectGuid,
            string tableName,
            string affectedColumnName,
            string affectedColumnValue,
            MpDbLogActionType actionType,
            DateTime actionDateTime,
            Guid sourceClientGuid) : this() {
            DbObjectGuid = dbObjectGuid;
            DbTableName = tableName;
            AffectedColumnName = affectedColumnName;
            AffectedColumnValue = affectedColumnValue;
            LogActionType = actionType;
            LogActionDateTime = actionDateTime;
            SourceClientGuid = sourceClientGuid;
        }

        public override string ToString() {
            string outStr = string.Empty;
            foreach (var prop in GetType().GetProperties()) {
                outStr += prop.Name + ": " + prop.GetValue(this) + ", ";
            }
            return outStr;
        }

        public void PrintLog() {
            foreach (var prop in GetType().GetProperties()) {
                MpConsole.WriteLine(prop.Name + ": " + prop.GetValue(this));
            }
        }

        public Task<object> CreateFromLogsAsync(string dboGuid, List<MpDbLog> logs, string fromClientGuid) {
            throw new NotImplementedException();
        }
    }
}
