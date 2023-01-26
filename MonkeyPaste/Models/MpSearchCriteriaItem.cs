using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    [Table("MpSearchCriteriaItem")]
    public class MpSearchCriteriaItem : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpSearchCriteriaItemId")]
        public override int Id { get; set; }

        [Column("MpSearchCriteriaItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpTagId")]
        public int QueryTagId { get; set; } = 0;

        [Column("e_MpContentFilterType")]
        public string SearchCriteriaPropertyTypeName { get; set; }

        [Column("e_MpSearchCriteriaUnitFlags")]
        public string SearchCriteriaUnitFlagsCsv { get; set; }

        public int SortOrderIdx { get; set; } = 0;

        #endregion


        #region Properties

        [Ignore]
        public Guid SearchCriteriaItemGuid {
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
        [Ignore]
        public MpContentFilterType CriteriaType {
            get => SearchCriteriaPropertyTypeName.ToEnum<MpContentFilterType>();
            set => SearchCriteriaPropertyTypeName = value.ToString();
        }

        [Ignore]
        public MpSearchCriteriaUnitFlags UnitFlags {
            get => string.IsNullOrEmpty(SearchCriteriaUnitFlagsCsv) ?
                MpSearchCriteriaUnitFlags.None :
                (MpSearchCriteriaUnitFlags)SearchCriteriaUnitFlagsCsv
                .SplitNoEmpty(",")
                .Select(x => x.ToEnum<MpSearchCriteriaUnitFlags>())
                .Cast<int>()
                .Aggregate((a, b) => a | b);

            set => 
                SearchCriteriaPropertyTypeName = 
                string.Join(
                    ",", 
                    typeof(MpSearchCriteriaUnitFlags)
                    .GetEnumNames()
                    .Where(x => value.HasFlag(x.ToEnum<MpSearchCriteriaUnitFlags>())));
        }
        #endregion

        #region Statics

        public static async Task<MpSearchCriteriaItem> CreateAsync(
            string guid="",
            int tagId=0,
            int sortOrderIdx = -1,
            MpContentFilterType criteriaType = MpContentFilterType.None,
            MpSearchCriteriaUnitFlags unitFlags = MpSearchCriteriaUnitFlags.None,
            bool suppressWrite = false) {
            if(tagId < 0 && !suppressWrite) {
                throw new Exception("Must provide tag id");
            }
            var sci = new MpSearchCriteriaItem() {
                SearchCriteriaItemGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                QueryTagId = tagId,
                SortOrderIdx = sortOrderIdx,
                CriteriaType = criteriaType,
                UnitFlags = unitFlags
            };

            if(!suppressWrite) {
                await sci.WriteToDatabaseAsync();
            }
            return sci;
        }
        #endregion

        public MpSearchCriteriaItem() : base() { }

        public override async Task DeleteFromDatabaseAsync() {
            List<Task> deleteTasks = new List<Task>();
            var pvl = await MpDataModelProvider.GetAllParameterHostValuesAsync(MpParameterHostType.Query, Id);
            deleteTasks.AddRange(pvl.Select(x => x.DeleteFromDatabaseAsync()));
            deleteTasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(deleteTasks);
        }
    }
}
