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
        private static List<MpDbLog> _AllDbLogList = null;
        public static int TotalDbLogCount = 0;

        [Column("pk_MpDbLogId")]
        public override int Id { get; set; }

        [Column("DbObjectGuid")]
        public string ObjectGuid { get; set; }

        public string AffectedColumnName { get; set; } = @"UnknownColumnName";

        public string AffectedColumnValue { get; set; } = @"UnknownColumnValue";

        [Ignore]
        public Guid DbObjectGuid { 
            get {
                if(string.IsNullOrEmpty(ObjectGuid)) {
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
                if(string.IsNullOrEmpty(ClientGuid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(ClientGuid);
            }
            set {
                ClientGuid = value.ToString();
            }
        }
        public static async Task<MpDbLog> GetDbLogById(int DbLogId) {
            var allLogs = await MpDb.Instance.GetItems<MpDbLog>();
            return allLogs.Where(x => x.Id == DbLogId).FirstOrDefault();
        }

        public async Task<object> DeserializeDbObject(string objStr, string parseToken = @"^(@!@") {
            var objParts = objStr.Split(new string[] { parseToken }, StringSplitOptions.RemoveEmptyEntries);

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

        public string SerializeDbObject(string parseToken = @"^(@!@") {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}",
                parseToken,
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

        public MpDbLog() : base(typeof(MpDbLog)) { }

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
    }
}
