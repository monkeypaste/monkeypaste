using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDbLogTracker {
        public static async Task LogDbWriteAsync(MpDbLogActionType actionType, MpDbModelBase dbModel, string clientGuid = "") {
            if(string.IsNullOrEmpty(dbModel.Guid)) {
                MpConsole.WriteLine(@"Cannot track item without a guid");
                return;
            }
            Guid objectGuid = Guid.Parse(dbModel.Guid);
            Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisDeviceGuid) : Guid.Parse(clientGuid);
            string tableName = dbModel.GetType().ToString();
            tableName = tableName.Substring(tableName.IndexOf(".") + 1);
            var actionDateTime = DateTime.UtcNow;

            if (actionType == MpDbLogActionType.Delete) {
                var dbi = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
                await dbi.WriteToDatabaseAsync(string.Empty, true, true);
                return;
            }

            var oldItem = await MpDb.Instance.GetDbObjectByTableGuidAsync(tableName, objectGuid.ToString());
            var alteredColumnNameValuePairs = await (dbModel as MpISyncableDbObject).DbDiff(oldItem);
            if (alteredColumnNameValuePairs.Count == 0) {
                return;
            }

            foreach (var kvp in alteredColumnNameValuePairs) {
                var dbi = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
                await dbi.WriteToDatabaseAsync(string.Empty, true, true);
            }
        }

        //public static void TrackDbWrite(MpDbLogActionType actionType, MpDbModelBase dbModel, string clientGuid = "") {
        //    Guid objectGuid = Guid.Parse(dbModel.Guid);
        //    Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisDeviceGuid) : Guid.Parse(clientGuid);
        //    string tableName = dbModel.GetType().ToString();
        //    tableName = tableName.Substring(tableName.IndexOf(".") + 1);
        //    var actionDateTime = DateTime.UtcNow;

        //    if (actionType == MpDbLogActionType.Delete) {
        //        var dbi = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
        //        MpDb.Instance.AddItem(dbi, string.Empty, true, true);
        //        return;
        //    }

        //    var oldItem = MpDb.Instance.GetDbObjectByTableGuid(tableName, objectGuid.ToString());
        //    var alteredColumnNameValuePairs = (dbModel as MpISyncableDbObject).DbDiff(oldItem);
        //    if (alteredColumnNameValuePairs.Count == 0) {
        //        return;
        //    }

        //    foreach (var kvp in alteredColumnNameValuePairs) {
        //        var dbi = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
        //        MpDb.Instance.AddItem(dbi, string.Empty, true, true);
        //    }
        //}

        //public static MonkeyPaste.MpDbLogActionType TrackDbWrite(string query, Dictionary<string, object> args, string objGuid, string clientGuid, object obj) {
        //    if (string.IsNullOrEmpty(objGuid)) {
        //        MpConsole.WriteLine(@"Cannot track item without a guid");
        //        return MpDbLogActionType.None;
        //    }

        //    Guid objectGuid = Guid.Parse(objGuid);
        //    Guid sourceClientGuid = string.IsNullOrEmpty(clientGuid) ? Guid.Parse(MpPreferences.Instance.ThisDeviceGuid) : Guid.Parse(clientGuid);
        //    string tableName = "UnknownTableName";
        //    var actionType = MonkeyPaste.MpDbLogActionType.None;
        //    var actionDateTime = DateTime.UtcNow;

        //    if (query.ToLower().StartsWith("insert")) {
        //        actionType = MonkeyPaste.MpDbLogActionType.Create;
        //        string preStr = "insert into ";
        //        int tableNameLength = query.IndexOf(@"(") - preStr.Length;
        //        tableName = query.Substring(preStr.Length, tableNameLength);
        //    } else if (query.ToLower().StartsWith("update")) {
        //        actionType = MonkeyPaste.MpDbLogActionType.Modify;
        //        string preStr = "update ";
        //        int tableNameLength = query.Substring(preStr.Length).IndexOf(@" ");
        //        tableName = query.Substring(preStr.Length, tableNameLength);
        //    } else if (query.ToLower().StartsWith("delete")) {
        //        actionType = MonkeyPaste.MpDbLogActionType.Delete;
        //        string preStr = "delete from ";
        //        int tableNameLength = query.Substring(preStr.Length).IndexOf(@" ");
        //        tableName = query.Substring(preStr.Length, tableNameLength);
        //    } else {
        //        throw new Exception(@"Unknown query format: " + query);
        //    }

        //    if (actionType != MonkeyPaste.MpDbLogActionType.Delete) {
        //        if (obj == null) {
        //            throw new Exception(@"DbLog object cannot be null for non-delete transactions");
        //        }
        //        var oldRow = MpDb.Instance.GetDbObjectByTableGuid(tableName, objGuid);
        //        var alteredColumnNameValueLookUp = (obj as MonkeyPaste.MpISyncableDbObject).DbDiff(oldRow);
        //        if (alteredColumnNameValueLookUp.Count == 0) {
        //            //since no data is altered return false to not write to db or change log
        //            return MonkeyPaste.MpDbLogActionType.None;
        //        }
        //        foreach (var kvp in alteredColumnNameValueLookUp) {
        //            var newLog = new MpDbLog(objectGuid, tableName, kvp.Key, kvp.Value.ToString(), actionType, actionDateTime, sourceClientGuid);
        //            MpDb.Instance.AddItem(newLog, string.Empty, true, true);
        //        }
        //    } else {
        //        var newLog = new MpDbLog(objectGuid, tableName, "*", "AllValues", actionType, actionDateTime, sourceClientGuid);
        //        MpDb.Instance.AddItem(newLog, string.Empty, true, true);
        //    }
        //    return actionType;
        //}

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
