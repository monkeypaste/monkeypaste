using System;
using System.Data;
//using Auth0.Windows;

namespace MpWpfApp {
    public enum MpUserState {
        none = 0,
        prepending,
        pending,
        active,
        reset,
        inactive,
        deactivated
    }
    public class MpUser : MpDbObject {
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public DateTime LoginDateTime { get; set; }
        public MpUserState UserState { get; set; }
        public string IdentityToken { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        //public Auth0User LoggedInUser { get; set; }

        public MpUser(int userId, int clientId, DateTime loginDateTime, MpUserState userState, string identityToken, string email, string hashedPassword) {
            UserId = userId;
            ClientId = clientId;
            LoginDateTime = loginDateTime;
            UserState = userState;
            IdentityToken = identityToken;
            Email = email;
            HashedPassword = hashedPassword;
        }
        public MpUser(int userId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpUser where pk_MpUserId=" + userId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpUser(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            throw new NotImplementedException();
        }

        public override void WriteToDatabase() {
            throw new NotImplementedException();
        }
    }
}
