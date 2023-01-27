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

        public string Options { get; set; }

        [Column("e_MpLogicalQueryType")]
        public string PrevJoinTypeName { get; set; } = MpLogicalQueryType.And.ToString();

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

        public MpLogicalQueryType PrevJoinType {
            get => PrevJoinTypeName.ToEnum<MpLogicalQueryType>();
            set => PrevJoinTypeName = value.ToString();
        }

        #endregion

        #region Statics

        public static async Task<MpSearchCriteriaItem> CreateAsync(
            string guid="",
            int tagId=0,
            int sortOrderIdx = -1,
            MpLogicalQueryType prevJoinType = MpLogicalQueryType.And,
            string options = "",
            bool suppressWrite = false) {
            if(tagId < 0 && !suppressWrite) {
                throw new Exception("Must provide tag id");
            }
            var sci = new MpSearchCriteriaItem() {
                SearchCriteriaItemGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                QueryTagId = tagId,
                SortOrderIdx = sortOrderIdx,
                PrevJoinType = prevJoinType,
                Options = options
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
