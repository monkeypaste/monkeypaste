using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpContact {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public string HashKey {
            get {
                return this.ToString().CheckSum();
            }
        }

        public override string ToString() {
            return string.Format(@"{0} {1} {2}", FirstName, LastName, Email);
        }
    }
}
