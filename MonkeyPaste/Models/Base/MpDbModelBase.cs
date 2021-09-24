using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFImageLoading.Helpers.Exif;
using SQLite;
using SQLiteNetExtensions.Extensions;
using static SQLite.SQLite3;

namespace MonkeyPaste {
    public abstract class MpDbModelBase : INotifyPropertyChanged {
        public const string ParseToken = @"^(@!@";
        public abstract int Id { set; get; }
        
        [Ignore]
        public bool HasModelChanged { get; set; }

        #region Wpf Compatibility

        [Ignore]
        protected string SyncingWithDeviceGuid { get; set; } = string.Empty;

        [Ignore]
        public bool IsSyncing => !string.IsNullOrEmpty(SyncingWithDeviceGuid);

        public void StartSync(string sourceGuid) {
            SyncingWithDeviceGuid = sourceGuid;
        }

        public void EndSync() {
            SyncingWithDeviceGuid = string.Empty;
        }

        public MpDbModelBase() { }

        public MpDbModelBase(DataRow dr) {
            LoadDataRow(dr);
        }

        private void LoadDataRow(DataRow dr) {
            var tn = GetType().ToString().Replace("MonkeyPaste.", string.Empty);
            var tm = MpDb.Instance.GetTableMapping(tn);

            foreach (var rowProp in dr.GetType().GetProperties()) {
                if (rowProp.GetAttribute<SQLite.IgnoreAttribute>() != null) {
                    continue;
                }
                string cn = tm.FindColumnWithPropertyName(rowProp.Name).Name;

                rowProp.SetValue(this, dr[cn]);
            }
        }

        public void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = MpPreferences.Instance.ThisDeviceGuid;
            }
            if (string.IsNullOrEmpty(Guid)) {
                Guid = System.Guid.NewGuid().ToString();
            }

            var dbot = GetType();
            var addOrUpdateMethod = typeof(MpDb).GetMethod(nameof(MpDb.Instance.AddOrUpdate));
            var addOrUpdateByDboTypeMethod = addOrUpdateMethod.MakeGenericMethod(new[] { dbot });
            addOrUpdateByDboTypeMethod.Invoke(MpDb.Instance, new object[] { this, sourceClientGuid, ignoreTracking, ignoreSyncing });
        }

        public void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(MpPreferences.Instance.ThisDeviceGuid);
            }
        }

        public void WriteToDatabase(bool isFirstLoad) {
            WriteToDatabase(MpPreferences.Instance.ThisDeviceGuid, isFirstLoad);
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(MpPreferences.Instance.ThisDeviceGuid);
            }
        }

        public void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (Id <= 0) {
                return;
            }

            var dbot = GetType();
            var deleteItemMethod = typeof(MpDb).GetMethod(nameof(MpDb.Instance.DeleteItem));
            var deleteItemByDboTypeMethod = deleteItemMethod.MakeGenericMethod(new[] { dbot });
            deleteItemByDboTypeMethod.Invoke(MpDb.Instance, new object[] { this, sourceClientGuid, ignoreTracking, ignoreSyncing });
        }
        #endregion


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

        #region PropertyChanged 
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsChanged { get; set; }
        #endregion
    }
}
