using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDbLogTracker {
        public static void TrackDbWrite(string query, Dictionary<string,string> alteredColumnNameValuePairs, string objGuid, string clientGuid) {
            if(string.IsNullOrEmpty(query.Trim())) {
                return;
            }
            if(string.IsNullOrEmpty(objGuid)) {
                throw new Exception(@"DbLog requires the altered objects guid");
            }

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

            if(alteredColumnNameValuePairs == null || alteredColumnNameValuePairs.Count == 0) {
                new MpDbLog(objectGuid, tableName,"*","AllValues", actionType, actionDateTime, sourceClientGuid).WriteToDatabase();
            } else {
                foreach(var kvp in alteredColumnNameValuePairs) {
                    new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid).WriteToDatabase();
                }
            }
            
        }
    }
}
