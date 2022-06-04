using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common.Plugin {
    public interface MpIContactModel {
        string FirstName { get; }
        string LastName { get; }
        string FullName { get; }
        string PhoneNumber { get; }
        string Address { get; }
        string Email { get; }
    }
}
