using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbLogTracker {
        public static async Task TrackDbWriteAsync(MpDbLogActionType actionType, MpDbModelBase dbModel, string clientGuid = "") {
            Guid objectGuid = Guid.Parse(dbModel.Guid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisClientGuidStr) : Guid.Parse(clientGuid);
            string tableName = dbModel.GetType().ToString();
            tableName = tableName.Substring(tableName.IndexOf(".") + 1);
            var actionDateTime = DateTime.UtcNow;

            if (actionType == MpDbLogActionType.Delete) {
                var dbi = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
                await MpDb.Instance.AddItemAsync(dbi, string.Empty, true, true);
                return;
            }

            var oldItem = await MpDb.Instance.GetDbObjectByTableGuidAsync(tableName, objectGuid.ToString());
            var alteredColumnNameValuePairs = (dbModel as MpISyncableDbObject).DbDiff(oldItem);
            if (alteredColumnNameValuePairs.Count == 0) {
                return;
            }

            foreach (var kvp in alteredColumnNameValuePairs) {
                var dbi = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
                await MpDb.Instance.AddItemAsync(dbi, string.Empty, true, true);
            }
        }

        public static void TrackDbWrite(MpDbLogActionType actionType, MpDbModelBase dbModel, string clientGuid = "") {
            Guid objectGuid = Guid.Parse(dbModel.Guid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisClientGuidStr) : Guid.Parse(clientGuid);
            string tableName = dbModel.GetType().ToString();
            tableName = tableName.Substring(tableName.IndexOf(".") + 1);
            var actionDateTime = DateTime.UtcNow;

            if (actionType == MpDbLogActionType.Delete) {
                var dbi = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
                MpDb.Instance.AddItem(dbi, string.Empty, true, true);
                return;
            }

            var oldItem = MpDb.Instance.GetDbObjectByTableGuidAsync(tableName, objectGuid.ToString());
            var alteredColumnNameValuePairs = (dbModel as MpISyncableDbObject).DbDiff(oldItem);
            if (alteredColumnNameValuePairs.Count == 0) {
                return;
            }

            foreach (var kvp in alteredColumnNameValuePairs) {
                var dbi = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
                MpDb.Instance.AddItem(dbi, string.Empty, true, true);
            }
        }

        public static void PrintDbLog() {
            Task.Run(async () => {
                var dblil = await MpDb.Instance.GetItemsAsync<MpDbLog>();

                foreach(var dbli in dblil) {
                    Console.WriteLine(dbli.ToString());
                }
            });

        }
    }
}
