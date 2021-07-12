using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDbLogTracker {
        public static bool TrackDbWrite(string query, Dictionary<string,object> args, string objGuid, string clientGuid, object obj) {
            if(string.IsNullOrEmpty(objGuid)) {
                throw new Exception(@"DbLog object guid cannot be null");
            }
            

            var alteredColumnNameValueLookUp = new Dictionary<string, string>();

            Guid objectGuid = Guid.Parse(objGuid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(Properties.Settings.Default.ThisClientGuid) : Guid.Parse(clientGuid);
            string tableName = "UnknownTableName";
            var actionType = MonkeyPaste.MpDbLogActionType.None;
            var actionDateTime = DateTime.Now;
           
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

            if(actionType != MonkeyPaste.MpDbLogActionType.Delete) {
                if (obj == null) {
                    throw new Exception(@"DbLog object cannot be null for non-delete transactions");
                }
                var oldRow = MpDb.Instance.GetObjDbRow(tableName, objGuid);
                alteredColumnNameValueLookUp = (obj as MonkeyPaste.MpISyncableDbObject).DbDiff(oldRow);
                if(alteredColumnNameValueLookUp.Count == 0) {
                    //since no data is altered return false to not write to db or change log
                    return false;
                }
                foreach (var kvp in alteredColumnNameValueLookUp) {
                    new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid).WriteToDatabase();
                }
            } else {
                new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid).WriteToDatabase();
            }

            return true;
        }

        
    }
}
