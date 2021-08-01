using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDbLogTracker {
        public static MonkeyPaste.MpDbLogActionType TrackDbWrite(string query, Dictionary<string,object> args, string objGuid, string clientGuid, object obj) {
            if (string.IsNullOrEmpty(objGuid)) {
                MpConsole.WriteLine(@"Cannot track item without a guid");
                return MpDbLogActionType.None;
            }

            Guid objectGuid = Guid.Parse(objGuid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(Properties.Settings.Default.ThisClientGuid) : Guid.Parse(clientGuid);
            string tableName = "UnknownTableName";
            var actionType = MonkeyPaste.MpDbLogActionType.None;
            var actionDateTime = DateTime.UtcNow;
           
            if(query.ToLower().StartsWith("insert")) {
                actionType = MonkeyPaste.MpDbLogActionType.Create;
                string preStr = "insert into ";
                int tableNameLength = query.IndexOf(@"(") - preStr.Length;
                tableName = query.Substring(preStr.Length, tableNameLength);                
            } else if (query.ToLower().StartsWith("update")) {
                actionType = MonkeyPaste.MpDbLogActionType.Modify;
                string preStr = "update ";
                int tableNameLength = query.Substring(preStr.Length).IndexOf(@" ");
                tableName = query.Substring(preStr.Length, tableNameLength);
            } else if (query.ToLower().StartsWith("delete")) {
                actionType = MonkeyPaste.MpDbLogActionType.Delete;
                string preStr = "delete from ";
                int tableNameLength = query.Substring(preStr.Length).IndexOf(@" ");
                tableName = query.Substring(preStr.Length, tableNameLength);
            } else {
                throw new Exception(@"Unknown query format: " + query);
            }

            if (actionType != MonkeyPaste.MpDbLogActionType.Delete) {
                if (obj == null) {
                    throw new Exception(@"DbLog object cannot be null for non-delete transactions");
                }
                var oldRow = MpDb.Instance.GetDbDataRowByTableGuid(tableName, objGuid);
                var alteredColumnNameValueLookUp = (obj as MonkeyPaste.MpISyncableDbObject).DbDiff(oldRow);
                if (alteredColumnNameValueLookUp.Count == 0) {
                    //since no data is altered return false to not write to db or change log
                    return MonkeyPaste.MpDbLogActionType.None;
                }
                foreach (var kvp in alteredColumnNameValueLookUp) {
                    var newLog = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
                    newLog.WriteToDatabase(clientGuid,true,true);
                }
            } else {
                var newLog = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
                newLog.WriteToDatabase(clientGuid, true, true);
            }
            return actionType;
        }

        
    }
}
