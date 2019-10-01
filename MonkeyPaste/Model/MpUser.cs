using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Auth0.Windows;

namespace MonkeyPaste {
    public enum MpUserState {
        none,
        prepending,
        pending,
        active,
        reset,
        inactive,
        deactivated
    }
    public class MpUser : MpDBObject   {
        public int MpUserId { get; set; }
        public int MpClientId { get; set; }
        public DateTime LoginDateTime { get; set; }
        public MpUserState UserState { get; set; }
        public string IdentityToken { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        //public Auth0User LoggedInUser { get; set; }

        public override void LoadDataRow(DataRow dr) {
            throw new NotImplementedException();
        }

        public override void WriteToDatabase() {
            throw new NotImplementedException();
        }
    }
}
