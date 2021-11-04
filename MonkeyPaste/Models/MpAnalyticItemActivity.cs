using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpAnalyticItemActivity : MpDbModelBase {
        #region Columns
        [Column("pk_MpAnalyticItemActivityId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemActivityGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("ContentType")]
        public string ContentType { get; set; } = string.Empty;

        [Column("Method")]
        public string Method { get; set; } = string.Empty;

        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Column("Description")]
        public string Description { get; set; } = string.Empty;
        #endregion

        #region Fk Models

        [ManyToMany(typeof(MpAnalyticItemActivityParameter))]
        public List<MpAnalyticItemParameter> Parameters { get; set; } = new List<MpAnalyticItemParameter>();

        #endregion

        #region Properties

        [Ignore]
        public Guid AnalyticItemActivityGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        #endregion

        //public static async Task<MpAnalyticItemActivity> Create(MpAnalyticItem parentItem,string key, string value, bool isRequired, bool isHeader, bool isRequest, int sortOrderIdx) {
        //    if(parentItem == null) {
        //        throw new Exception("Parameter must be associated with an item");
        //    }
        //    var dupItem = await MpDataModelProvider.Instance.GetAnalyticItemActivityByKey(parentItem.Id,key);
        //    if (dupItem != null) {
        //        dupItem = await MpDb.Instance.GetItemAsync<MpAnalyticItemActivity>(dupItem.Id);
        //        if(dupItem.ValueCsv != value || dupItem.SortOrderIdx != sortOrderIdx || dupItem.IsParameterRequired != isRequired) {
        //            MpConsole.WriteLine($"Updating parameter {key} for {parentItem.Title}");
        //            dupItem.ValueCsv = value;
        //            dupItem.SortOrderIdx = sortOrderIdx;
        //            dupItem.IsParameterRequired = isRequired;
        //            await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemActivity>(dupItem);
        //        }
        //        return dupItem;
        //    }

        //    var newAnalyticItemActivity = new MpAnalyticItemActivity() {
        //        AnalyticItemActivityGuid = System.Guid.NewGuid(),
        //        Key = key,
        //        ValueCsv = value,
        //        AnalyticItemId = parentItem.Id,
        //        SortOrderIdx = sortOrderIdx,
        //        IsParameterRequired = isRequired,
        //        IsHeaderParameter = isHeader,
        //        IsRequestParameter = isRequest
        //    };

        //    await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemActivity>(newAnalyticItemActivity);

        //    return newAnalyticItemActivity;
        //}

        public MpAnalyticItemActivity() { }
    }
}
