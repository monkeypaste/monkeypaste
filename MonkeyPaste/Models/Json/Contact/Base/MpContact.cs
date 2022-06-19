using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste {
    public enum MpContactFieldType {
        None = 0,
        FirstName,
        LastName,
        Email
    }

    public enum MpContactSourceType {
        None = 0,
        Google,
        Outlook
    }

    public class MpContact {
        protected object _contactSource;

        public static MpContact EmptyContact {
            get {
                return new MpContact() {
                    FullName = string.Empty
                };
            }
        }
        public virtual string SourceName { get; set; } = MpContactSourceType.None.ToString();
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string FullName { get; set; }

        public virtual string Email { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual string Address { get; set; }        

        public string DisplayValue {
            get {
                if(string.IsNullOrWhiteSpace(FullName)) {
                    if(string.IsNullOrWhiteSpace(Email)) {
                        return "No Display Data";
                    }
                    return Email;
                }
                return FullName;
            }
        }

        public MpContact() { }

        public MpContact(MpIContact contact) {
            _contactSource = contact.Source;
            SourceName = contact.SourceName;
            FirstName = contact.FirstName;
            LastName = contact.LastName;
            Email = contact.Email;
            PhoneNumber = contact.PhoneNumber;
            Address = contact.Address;
        }

        public MpContact(object contactSource) {
            _contactSource = contactSource;
        }

        public object GetField(string propertyPath, object[] index = null) {
            if(_contactSource == null) {
                return string.Empty;
            }

            return _contactSource.GetPropertyValue(propertyPath, index);
        }
    }
}
