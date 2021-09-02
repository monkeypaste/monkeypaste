using MonkeyPaste;
using SQLite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public abstract class MpDbModelBase : MpObject {
        public const string ParseToken = @"^(@!@";
        public string TableName = "Unknown";
        public Dictionary<string, object> columnData = new Dictionary<string, object>();

        public SQLite.TableMapping GetTableMapping() {
            return MpDb.Instance.GetTableMapping(GetType().ToString().Replace(@"MpWpfApp.",string.Empty));
        }

        //public static event EventHandler<MpDbSyncEventArgs> SyncAdd;
        //public static event EventHandler<MpDbSyncEventArgs> SyncUpdate;
        //public static event EventHandler<MpDbSyncEventArgs> SyncDelete;

        //public void NotifyRemoteUpdate(MpDbLogActionType actionType, object dbo, string sourceClientGuid) {
        //    var eventArgs = new MpDbSyncEventArgs() {
        //        DbObject = dbo,
        //        EventType = actionType,
        //        SourceGuid = sourceClientGuid
        //    };
        //    switch(actionType) {
        //        case MpDbLogActionType.Create:
        //            SyncAdd?.Invoke(dbo, eventArgs);
        //            break;
        //        case MpDbLogActionType.Modify:
        //            SyncUpdate?.Invoke(dbo, eventArgs);
        //            break;
        //        case MpDbLogActionType.Delete:
        //            SyncDelete?.Invoke(dbo, eventArgs);
        //            break;
        //    }
        //}

        [Ignore]
        public string SyncingWithDeviceGuid { get; set; } = string.Empty;

        [Ignore]
        public bool IsSyncing => !string.IsNullOrEmpty(SyncingWithDeviceGuid);

        public void StartSync(string sourceGuid) {
            SyncingWithDeviceGuid = sourceGuid;
        }

        public void EndSync() {
            SyncingWithDeviceGuid = string.Empty;
        }

        public virtual void LoadDataRow(DataRow dr) { }
                

        public int GetByteSize() {
            return 0;
        }

        public DateTime GetCreationDate() {
            return DateTime.Now;
        }

        public int GetOwnerId() {
            return 0;
        }

        public abstract void WriteToDatabase();

        public virtual void WriteToDatabase(string sourceClientGuid,bool ignoreTracking = false,bool ignoreSyncing = false) { throw new Exception(@"WriteToDb w/ args must be overriden"); }

        public virtual void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) { throw new Exception(@"DeleteFromoDb w/ args must be overriden"); }
        
        protected Dictionary<string, string> CheckValue(
            object a, object b, string colName, Dictionary<string, string> diffLookup, object forceAVal = null) {
            // a = current model property
            // b = model in db
            // when a != b add a to diffLookup OR substitue with forceVal (so guids are used instead of int keys)
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
            var actionType = logs[0].LogActionType;
            var tm = MpDb.Instance.GetTableMapping(tableName);
            if (tm == null) {
                throw new Exception(@"Cannot find table mapping for table: " + tableName);
            }
            string dboGuid = logs[0].DbObjectGuid.ToString();
            var dbo = MpDb.Instance.GetDbObjectByTableGuid(tableName, dboGuid);
            if (dbo == null) {
                //for add transactions
                var dbot = new MpWpfStringToDbObjectTypeConverter().Convert(tableName);
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

                string fkPrefix = @"fk_";
                if (colProp.Name.StartsWith(fkPrefix)) {
                    //handle fk substitution
                    //table column fk_*Id are passed into log but values are the fk_Guid
                    //so to get the local fk_*Id the db is queried for the its its guid to get the id
                    string fkTableName = colProp.Name
                                            .Replace(fkPrefix, string.Empty)
                                            .Replace(@"Id", string.Empty);
                    var fkDbo = MpDb.Instance.GetDbObjectByTableGuid(fkTableName, log.AffectedColumnValue);
                    var fkDboProp = fkDbo.GetType()
                                         .GetProperties()
                                         .Where(x => x.Name == dboProp.Name)
                                         .FirstOrDefault();

                    dboProp.SetValue(dbo, (int)fkDboProp.GetValue(fkDbo));

                    var fkGuidProp = dbo.GetType()
                                        .GetProperties()
                                        .Where(x => x.Name == fkTableName.Replace("Mp",string.Empty))
                                        .FirstOrDefault();
                    fkGuidProp.SetValue(dbo, fkDbo);
                } else if (colProp.Name.EndsWith(@"Guid")) {
                    //all dbo Guid properties follow conventions of:
                    // column name = <TableName>Guid and dbo property name = <TableName w/o Mp prefix>Guid
                    var pkGuidProp = dbo.GetType()
                                        .GetProperties()
                                        .Where(x => x.Name == colProp.Name.Replace("Mp", string.Empty))
                                        .FirstOrDefault();
                    pkGuidProp.SetValue(dbo, System.Guid.Parse(log.AffectedColumnValue));                    
                } else {
                    if (dboProp.PropertyType == typeof(int)) {
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

        private void MpDbObject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            HasChanged = true;
        }

        public override string ToString() {            
            string outstr = "";
            foreach (KeyValuePair<string, object> cd in columnData) {
                if (cd.Value == null) {
                    continue;
                }
                outstr += "| " + cd.Key.ToString() + ": \n";
                if (cd.Value.GetType() == typeof(Image)) {
                    outstr += "| " + ((Image)cd.Value).Width + " x " + ((Image)cd.Value).Height + " \n";
                } else if (cd.Value.GetType() == typeof(string[])) {
                    foreach (string str in (string[])cd.Value) {
                        outstr += "| " + str + "\n";
                    }
                } else {
                    outstr += "| " + cd.Value.ToString() + "\n";
                }
                outstr += "|-----------------------------------------------------------------------------------------------|\n";
            }
            return outstr;
        }
    }
}
