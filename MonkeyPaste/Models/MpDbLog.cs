using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;
using SQLite;

namespace MonkeyPaste {
    public enum MpDbLogActionType {
        None = 0,
        Create,
        Modify,
        Delete,
        Sync
    }

    public class MpDbLog : MpDbModelBase {
        private static List<MpDbLog> _AllDbLogList = null;
        public static int TotalDbLogCount = 0;

        [Column("pk_MpDbLogId")]
        public override int Id { get; set; }

        [Column("DbObjectGuid")]
        public string ObjectGuid { get; set; }

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

        public MpDbLog() : base(typeof(MpDbLog)) { }

        public MpDbLog(
            Guid dbObjectGuid, 
            string tableName, 
            MpDbLogActionType actionType, 
            DateTime actionDateTime, 
            Guid sourceClientGuid) : this() {

            DbObjectGuid = dbObjectGuid;
            DbTableName = tableName;
            LogActionType = actionType;
            LogActionDateTime = actionDateTime;
            SourceClientGuid = sourceClientGuid;
        }        
    }
}
