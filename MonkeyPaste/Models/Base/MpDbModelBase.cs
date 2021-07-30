using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;

namespace MonkeyPaste {
    public abstract class MpDbModelBase {
        public const string ParseToken = @"^(@!@";
        public abstract int Id { set; get; }

        protected string SyncingWithDeviceGuid { get; set; } = string.Empty;

        public bool IsSyncing => !string.IsNullOrEmpty(SyncingWithDeviceGuid);

        public void StartSync(string sourceGuid) {
            SyncingWithDeviceGuid = sourceGuid;
        }

        public void EndSync() {
            SyncingWithDeviceGuid = string.Empty;
        }

        [Ignore]
        public string Guid { get; set; }

        protected Dictionary<string, string> CheckValue(object a, object b, string colName, Dictionary<string, string> diffLookup, object forceAVal = null) {
            a = a == null ? string.Empty : a;
            b = b == null ? string.Empty : b;
            if (a.ToString() == b.ToString()) {
                return diffLookup;
            }
            diffLookup.Add(colName, forceAVal == null ? a.ToString() : forceAVal.ToString());
            return diffLookup;
        }

        public static object CreateOrUpdateFromLogs(
            List<MonkeyPaste.MpDbLog> logs, 
            string fromClientGuid) {
            string tableName = logs[0].DbTableName;
            var tm = MpDb.Instance.GetTableMapping(tableName);
            if (tm == null) {
                throw new Exception(@"Cannot find table mapping for table: " + tableName);
            }
            string dboGuid = logs[0].DbObjectGuid.ToString();
            var actionType = logs[0].LogActionType;
            var dbo = MpDb.Instance.GetDbObjectByTableGuid(tableName, dboGuid);
            if (dbo == null) {
                //for add transactions
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tableName);
                dbo = Activator.CreateInstance(dbot);
            }
            if(actionType == MpDbLogActionType.Delete) {
                return dbo;
            }
            foreach (var log in logs.OrderBy(x => x.LogActionDateTime)) {
                //get column mapping for log item
                var colProp = tm.Columns.Where(x => x.Name == log.AffectedColumnName).FirstOrDefault();
                if (colProp == null) {
                    throw new Exception(@"Cannot find column: " + log.AffectedColumnName);
                }
                if (colProp.IsPK) {
                    //ignore primary keys (shouldn't be here) only used locally
                    continue;
                }
                //get model prop from column mapping
                var dboProp = dbo.GetType()
                            .GetProperties()
                            .Where(x => x.Name == colProp.PropertyName)
                            .FirstOrDefault();
                if (dboProp == null) {
                    throw new Exception("Cannot find model property: " + colProp.PropertyName);
                }

                //handle fk substitution
                string fkPrefix = @"fk_";
                if (colProp.Name.StartsWith(fkPrefix)) {
                    string fkTableName = colProp.Name
                                            .Replace(fkPrefix, string.Empty)
                                            .Replace(@"Id", string.Empty);
                    var fkDbo = MpDb.Instance.GetDbObjectByTableGuid(fkTableName, log.AffectedColumnValue);
                    dboProp.SetValue(dbo, (fkDbo as MpDbModelBase).Id);

                    var fkGuidProp = dbo.GetType()
                                        .GetProperties()
                                        .Where(x => x.Name == fkTableName.Replace("Mp", string.Empty))
                                        .FirstOrDefault();
                    fkGuidProp.SetValue(dbo, fkDbo);
                } else if(colProp.Name.EndsWith(@"Guid")) {
                    (dbo as MpDbModelBase).Guid = log.AffectedColumnValue;
                } else {
                    if(dboProp.PropertyType == typeof(int)) {
                        dboProp.SetValue(dbo, Convert.ToInt32(log.AffectedColumnValue));
                    } else if (dboProp.PropertyType == typeof(bool)) {
                        dboProp.SetValue(dbo, Convert.ToInt32(log.AffectedColumnValue) == 1);
                    } else {
                        dboProp.SetValue(dbo, log.AffectedColumnValue);
                    }                    
                }
            }
            return dbo;
        }
    }
}
