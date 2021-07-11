using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbLogTracker {
        public static void TrackDbWrite(MpDbLogActionType actionType, Dictionary<string, string> alteredColumnNameValuePairs, MpDbModelBase dbModel, string clientGuid = "") {
            if(string.IsNullOrEmpty(dbModel.Guid)) {
                //only set for sync items so ignore
                return;
            }

            Guid objectGuid = Guid.Parse(dbModel.Guid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisClientGuidStr) : Guid.Parse(clientGuid);
            string tableName = dbModel.GetType().ToString();
            tableName = tableName.Substring(tableName.IndexOf(".") + 1);
            var actionDateTime = DateTime.Now;   
            
            Task.Run(async () => {
                if (alteredColumnNameValuePairs == null || alteredColumnNameValuePairs.Count == 0) {
                    var dbi = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
                    await MpDb.Instance.AddItem(dbi);
                } else {
                    foreach (var kvp in alteredColumnNameValuePairs) {
                        var dbi = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
                        await MpDb.Instance.AddItem(dbi);
                    }
                }
            });
        }
    }
}
