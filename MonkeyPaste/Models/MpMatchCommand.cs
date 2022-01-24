using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    

    public class MpMatchCommand : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpMatcherCommandId")]
        public override int Id { get; set; }

        [Column("MpMatcherCommandGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpMatcherCommandTypeId")]
        public int MatcherCommandTypeId { get; set; }

        [Column("fk_MpMatcherId")]
        [ForeignKey(typeof(MpMatcher))]
        public int MatcherId { get; set; }

        [Column("afk_CommandObjId")]
        public int CommandObjId { get; set; }

        #endregion

        #region Fk Objects

        #endregion

        #region Properties

        [Ignore]
        public Guid MatcherCommandGuid {
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
        public MpMatcherActionType MatcherCommandType {
            get => (MpMatcherActionType)MatcherCommandTypeId;
            set => MatcherCommandTypeId = (int)value;
        }

        #endregion

        public static async Task<MpMatchCommand> Create(MpMatcherActionType matchType, int mrId, int coid) {            
            var mr = new MpMatchCommand() {
                MatcherCommandGuid = System.Guid.NewGuid(),
                MatcherCommandType = matchType,
                MatcherId = mrId,
                CommandObjId = coid
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpMatchCommand() { }
    }
}
