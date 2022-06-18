using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public enum MpContactFieldType {
        None = 0,
        FirstName,
        LastName,
        Email
    }

    public class MpContact {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public async Task<string> GetHashKey() {
            return await this.ToString().CheckSum();
        }

        public override string ToString() {
            return string.Format(@"{0} {1} {2}", FirstName, LastName, Email);
        }
    }
}
