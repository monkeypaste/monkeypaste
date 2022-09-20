using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;

namespace MonkeyPaste {
    public abstract class MpDbModelBase : IComparer {
        public const string ParseToken = @"^(@!@";
        public abstract int Id { set; get; }

        #region Properties

        [Ignore]
        protected string SyncingWithDeviceGuid { get; set; } = string.Empty;

        [Ignore]
        public bool IsSyncing => !string.IsNullOrEmpty(SyncingWithDeviceGuid);

        [Ignore]
        public string Guid { get; set; }

        [Ignore]
        public virtual bool IsReadOnly { get; set; } = false;

        [Ignore]
        public virtual string PublicHandle => (Id.ToString() + Guid).CheckSum();
        #endregion

        #region Public Methods

        public MpDbModelBase() { }

        public static int GetUniqueId() {
            return MpHelpers.Rand.Next(int.MaxValue - 10000, int.MaxValue);
        }

        public void StartSync(string sourceGuid) {
            SyncingWithDeviceGuid = sourceGuid;
        }

        public void EndSync() {
            SyncingWithDeviceGuid = string.Empty;
        }

        public virtual async Task WriteToDatabaseAsync(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if(IsReadOnly) {
                MpConsole.WriteTraceLine($"Warning, trying to write read-only data object: '{GetType()}' ignoring");
                return;
            }
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = MpPrefViewModel.Instance.ThisDeviceGuid;
            }
            if (string.IsNullOrEmpty(Guid)) {
                Guid = System.Guid.NewGuid().ToString();
            }

            try {
                var dbot = GetType();
                var addOrUpdateAsyncMethod = typeof(MpDb).GetMethod(nameof(MpDb.AddOrUpdateAsync));
                var addOrUpdateByDboTypeAsyncMethod = addOrUpdateAsyncMethod.MakeGenericMethod(new[] { dbot });
                await addOrUpdateByDboTypeAsyncMethod.InvokeAsync(null, new object[] { this, sourceClientGuid, ignoreTracking, ignoreSyncing });
            } catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                
                Debugger.Break();
            }

            
        }

        public virtual async Task WriteToDatabaseAsync() {
            if (IsSyncing) {
                await WriteToDatabaseAsync(SyncingWithDeviceGuid, false, true);
            } else {
                await WriteToDatabaseAsync(MpPrefViewModel.Instance.ThisDeviceGuid);
            }
        }

        public virtual async Task DeleteFromDatabaseAsync(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (IsReadOnly) {
                MpConsole.WriteTraceLine($"Warning, trying to delete read-only data object: '{GetType()}' ignoring");
                return;
            }
            if (Id <= 0) {
                return;
            }

            var dbot = GetType();
            var deleteItemAsyncMethod = typeof(MpDb).GetMethod(nameof(MpDb.DeleteItemAsync));
            var deleteItemByDboTypeAsyncMethod = deleteItemAsyncMethod.MakeGenericMethod(new[] { dbot });
            await deleteItemByDboTypeAsyncMethod.InvokeAsync(null, new object[] { this, sourceClientGuid, ignoreTracking, ignoreSyncing });
        }

        public virtual async Task DeleteFromDatabaseAsync() {
            MpConsole.WriteLine($"Deleting '{GetType()}' [{Id}]:{Guid}");

            if (IsSyncing) {
                await DeleteFromDatabaseAsync(SyncingWithDeviceGuid, false, true);
            } else {
                await DeleteFromDatabaseAsync(MpPrefViewModel.Instance.ThisDeviceGuid);
            }
        }

        public static async Task<object> CreateOrUpdateFromLogs(
            List<MonkeyPaste.MpDbLog> logs,
            string fromClientGuid) {
            string tableName = logs[0].DbTableName;
            var tm = await MpDb.GetTableMappingAsync(tableName);
            if (tm == null) {
                throw new Exception(@"Cannot find table mapping for table: " + tableName);
            }
            string dboGuid = logs[0].DbObjectGuid.ToString();
            var actionType = logs[0].LogActionType;
            var dbo = await MpDb.GetDbObjectByTableGuidAsync(tableName, dboGuid);
            if (dbo == null) {
                //for add transactions
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tableName);
                dbo = Activator.CreateInstance(dbot);
            }
            if (actionType == MpDbLogActionType.Delete) {
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
                    var fkDbo = await MpDb.GetDbObjectByTableGuidAsync(fkTableName, log.AffectedColumnValue);
                    dboProp.SetValue(dbo, (fkDbo as MpDbModelBase).Id);

                    var fkGuidProp = dbo.GetType()
                                        .GetProperties()
                                        .Where(x => x.Name == fkTableName.Replace("Mp", string.Empty))
                                        .FirstOrDefault();
                    fkGuidProp.SetValue(dbo, fkDbo);
                } else if (colProp.Name.EndsWith(@"Guid")) {
                    (dbo as MpDbModelBase).Guid = log.AffectedColumnValue;
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

        #endregion

        #region Protected Methods

        protected Dictionary<string, string> CheckValue(object a, object b, string colName, Dictionary<string, string> diffLookup, object forceAVal = null) {
            a = a == null ? string.Empty : a;
            b = b == null ? string.Empty : b;
            if (a.ToString() == b.ToString()) {
                return diffLookup;
            }
            diffLookup.Add(colName, forceAVal == null ? a.ToString() : forceAVal.ToString());
            return diffLookup;
        }

        #endregion


        #region ICompare 

        public int Compare(object x, object y) {
            if (x == null || x is not MpDbModelBase) {
                if (y == null || y is not MpDbModelBase) {
                    return 0;
                }
                return -1;
            }
            if (y == null || y is not MpDbModelBase) {
                return 1;
            }
            if(x.GetType() != y.GetType()) {
                return -1;
            }
            return (x as MpDbModelBase).Id.CompareTo((y as MpDbModelBase).Id);
        }
        #endregion


        //public MpDbModelBase(DataRow dr) {
        //    LoadDataRow(dr);
        //}

        //private void LoadDataRow(DataRow dr) {
        //    var tn = GetType().ToString().Replace("MonkeyPaste.", string.Empty);
        //    var tm = MpDb.GetTableMapping(tn);

        //    foreach (var rowProp in dr.GetType().GetProperties()) {
        //        if (rowProp.GetAttribute<SQLite.IgnoreAttribute>() != null) {
        //            continue;
        //        }
        //        string cn = tm.FindColumnWithPropertyName(rowProp.Name).Name;

        //        rowProp.SetValue(this, dr[cn]);
        //    }
        //}

        //public virtual void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
        //    if (string.IsNullOrEmpty(sourceClientGuid)) {
        //        sourceClientGuid = MpJsonPreferenceIO.Instance.ThisDeviceGuid;
        //    }
        //    if (string.IsNullOrEmpty(Guid)) {
        //        Guid = System.Guid.NewGuid().ToString();
        //    }

        //    var dbot = GetType();
        //    var addOrUpdateMethod = typeof(MpDb).GetMethod(nameof(MpDb.AddOrUpdate));
        //    var addOrUpdateByDboTypeMethod = addOrUpdateMethod.MakeGenericMethod(new[] { dbot });
        //    addOrUpdateByDboTypeMethod.Invoke(null, new object[] { this, sourceClientGuid, ignoreTracking, ignoreSyncing });
        //}

        //public virtual void WriteToDatabase() {
        //    if (IsSyncing) {
        //        WriteToDatabase(SyncingWithDeviceGuid, false, true);
        //    } else {
        //        WriteToDatabase(MpJsonPreferenceIO.Instance.ThisDeviceGuid);
        //    }
        //}

        //public void WriteToDatabase(bool isFirstLoad) {
        //    WriteToDatabase(MpJsonPreferenceIO.Instance.ThisDeviceGuid, isFirstLoad);
        //}

        //public virtual void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
        //    if (Id <= 0) {
        //        return;
        //    }

        //    var dbot = GetType();
        //    var deleteItemMethod = typeof(MpDb).GetMethod(nameof(MpDb.DeleteItem));
        //    var deleteItemByDboTypeMethod = deleteItemMethod.MakeGenericMethod(new[] { dbot });
        //    deleteItemByDboTypeMethod.Invoke(null, new object[] { this, sourceClientGuid, ignoreTracking, ignoreSyncing });
        //}

        //public virtual void DeleteFromDatabase() {
        //    if (IsSyncing) {
        //        DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
        //    } else {
        //        DeleteFromDatabase(MpJsonPreferenceIO.Instance.ThisDeviceGuid);
        //    }
        //}
    }
}
