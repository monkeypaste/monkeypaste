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
        #region Constants

        public const MpLogicalQueryType DEFAULT_QUERY_JOIN_TYPE = MpLogicalQueryType.And;

        #endregion

        #region Interfaces
        #endregion

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
        public string NextJoinTypeName { get; set; } = DEFAULT_QUERY_JOIN_TYPE.ToString();

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

        public MpLogicalQueryType NextJoinType {
            get => NextJoinTypeName.ToEnum<MpLogicalQueryType>();
            set => NextJoinTypeName = value.ToString();
        }

        #endregion

        #region Statics

        public static async Task<MpSearchCriteriaItem> CreateAsync(
            string guid="",
            int tagId=0,
            int sortOrderIdx = -1,
            MpLogicalQueryType nextJoinType = DEFAULT_QUERY_JOIN_TYPE,
            string options = "",
            bool suppressWrite = false) {
            if(tagId < 0 && !suppressWrite) {
                throw new Exception("Must provide tag id");
            }
            var sci = new MpSearchCriteriaItem() {
                SearchCriteriaItemGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                QueryTagId = tagId,
                SortOrderIdx = sortOrderIdx,
                NextJoinType = nextJoinType,
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

            deleteTasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(deleteTasks);
        }

    }
}
