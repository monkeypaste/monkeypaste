using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpToken : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTokenId")]
        public override int Id { get; set; }

        [Column("MpTokenGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; } = 0;

        [Column("fk_MpActionId")]
        [ForeignKey(typeof(MpAction))]
        public int ActionId { get; set; } = 0;

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string Label { get; set; } = string.Empty;

        public string MatchData { get; set; } = string.Empty;

        #endregion

        #region Fk Objects


        #endregion

        #region Properties

        [Ignore]
        public Guid TokenGuid {
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

        public static async Task<MpToken> Create(
            int copyItemId = 0,
            int actionId = 0,
            string label = "",
            string matchData = "",
            int x = 0,
            int y = 0,
            int w = 0,
            int h = 0) {

            var dupCheck = await MpDataModelProvider.GetToken(copyItemId,actionId,matchData);
            if (dupCheck != null) {
                MpConsole.WriteTraceLine("Token with same data already defined");
                return dupCheck;
            }

            var mr = new MpToken() {
                TokenGuid = System.Guid.NewGuid(),
                CopyItemId = copyItemId,
                ActionId = actionId,
                Label = label,
                MatchData = matchData,
                X = x,
                Y = y,
                Width = w,
                Height = h
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpToken() { }
    }
}
