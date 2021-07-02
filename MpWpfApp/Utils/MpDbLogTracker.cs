using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpDbLogTracker {
        public static void TrackDbWrite(string query, Dictionary<string,object> args, string objGuid, string clientGuid) {
            if(string.IsNullOrEmpty(query.Trim())) {
                return;
            }
            if(string.IsNullOrEmpty(objGuid)) {
                throw new Exception(@"DbLog requires the altered objects guid");
            }

            Guid objectGuid = Guid.Parse(objGuid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(Properties.Settings.Default.ThisClientGuid) : Guid.Parse(clientGuid);
            string tableName = "Unknown";
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

            new MpDbLog(objectGuid, tableName, actionType, actionDateTime, sourceClientGuid).WriteToDatabase();
        }
    }
}
