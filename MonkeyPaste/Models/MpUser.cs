using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpUserAccountType {
        None = 0,
        Free,
        Basic,
        Premium,
        Trial,
        Admin
    }

    public class MpUser :
        MpDbModelBase {
        #region Constants
        public const MpUserAccountType TEST_ACCOUNT_TYPE = MpUserAccountType.Free;
        #endregion

        #region Statics

        public static async Task<MpUser> CreateAsync(
            string guid = "",
            string email = "",
            bool suppressWrite = false) {
            var ud = new MpUser() {
                Guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid,
                Email = email,
            };
            if (!suppressWrite) {
                await ud.WriteToDatabaseAsync();
            }
            return ud;
        }
        #endregion

        #region Interfaces

        #endregion

        #region Properties

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUserId")]
        public override int Id { get; set; }

        [Column("MpUserGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string Email { get; set; }
        public string AuthHash { get; set; }


        #endregion

        [Ignore]
        public Guid UserGuid {
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

        public MpUserAccountType AccountType =>
            // TODO this property is a placeholder, need platform store/this app server workflow
            // so this may not be stored in the client db
            TEST_ACCOUNT_TYPE;
        //MpUserAccountType.Free;

        #endregion

        #region Constructors

        public MpUser() { }

        #endregion

        #region Public Methods

        public override string ToString() {
            return Email;
        }
        #endregion
    }
}
