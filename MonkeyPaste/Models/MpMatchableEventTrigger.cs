using SQLite;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpMpMatchableEventTriggerType {
        None = 0,
        Clipboard,
        File,
        Folder,
        Tag,
        Shortcut
    }

    public class MpMpMatchableEventTrigger : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpMpMatchableEventTriggerId")]
        public override int Id { get; set; }

        [Column("MpMpMpMatchableEventTriggerGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpMpMatchableEventTriggerTypeId")]
        public int MpMatchableEventTriggerTypeId { get; set; }

        [Column("fk_MpMatchCommandId")]
        public int MatchableCommandId { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid MpMatchableEventTriggerGuid {
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
        public MpMpMatchableEventTriggerType MpMatchableEventTriggerType {
            get => (MpMpMatchableEventTriggerType)MpMatchableEventTriggerTypeId;
            set => MpMatchableEventTriggerTypeId = (int)value;
        }

        #endregion

        public static async Task<MpMpMatchableEventTrigger> Create(MpMpMatchableEventTriggerType matchType, int mcId) {
            var mr = new MpMpMatchableEventTrigger() {
                MpMatchableEventTriggerGuid = System.Guid.NewGuid(),
                MpMatchableEventTriggerType = matchType,
                MatchableCommandId = mcId
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpMpMatchableEventTrigger() { }
    }
}
