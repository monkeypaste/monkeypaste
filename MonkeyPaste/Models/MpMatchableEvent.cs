using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {


    public class MpMatchableEvent : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpMatchableEventId")]
        public override int Id { get; set; }

        [Column("MpMpMatchableEventGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpMatchableEventTypeId")]
        public int MatchableEventTypeId { get; set; }

        [Column("fk_MpMatcherId")]
        public int MatcherId { get; set; }

        #endregion

        #region Fk Objects

        [ManyToOne]
        public MpMatcher Matcher { get; set; }

        [ManyToMany(typeof(MpMatchCommand))]
        public List<MpMatchCommand> MatchCommands { get; set; } = new List<MpMatchCommand>();

        #endregion

        #region Properties

        [Ignore]
        public Guid MatchableEventGuid {
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
        public MpMatchTriggerType MatchableEventType {
            get => (MpMatchTriggerType)MatchableEventTypeId;
            set => MatchableEventTypeId = (int)value;
        }

        #endregion

        public static async Task<MpMatchableEvent> Create(MpMatchTriggerType matchType, int mrId) {
            var mr = new MpMatchableEvent() {
                MatchableEventGuid = System.Guid.NewGuid(),
                MatchableEventType = matchType,
                MatcherId = mrId
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpMatchableEvent() { }
    }
}
