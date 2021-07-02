using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbLogTracker {
        public static void TrackDbWrite(MpDbLogActionType actionType, MpDbModelBase dbModel, string clientGuid = "") {
            if(string.IsNullOrEmpty(dbModel.Guid)) {
                //only set for sync items so ignore
                return;
            }

            Guid objectGuid = Guid.Parse(dbModel.Guid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisClientGuidStr) : Guid.Parse(clientGuid);
            string tableName = dbModel.GetType().ToString();
            tableName = tableName.Substring(tableName.IndexOf(".") + 1);
            var actionDateTime = DateTime.Now;   

            var dbl = new MpDbLog(objectGuid, tableName, actionType, actionDateTime, sourceClientGuid);

            Task.Run(async () => {
                await MpDb.Instance.AddItem(dbl); 
            });
        }
    }
}
