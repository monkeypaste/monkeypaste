using MonkeyPaste.Common.Plugin;

namespace GoogleContactsFetcher {
    public class GoogleContact : MpIContact {
        #region Private Variables

        private Google.Apis.PeopleService.v1.Data.Person _person;

        #endregion

        #region Properties

        public object Source => _person;

        public string guid =>
            _person == null ?
            string.Empty : _person.ResourceName;
        public string FirstName {
            get {
                if (_person.Names == null || _person.Names.Count == 0) {
                    return string.Empty;
                }
                return _person.Names[0].GivenName;
            }
        }
        public string LastName {
            get {
                if (_person.Names == null || _person.Names.Count == 0) {
                    return string.Empty;
                }
                return _person.Names[0].FamilyName;
            }
        }
        public string FullName {
            get {
                if (_person.Names == null || _person.Names.Count == 0) {
                    return string.Empty;
                }
                return _person.Names[0].DisplayName;
            }
        }

        public string Email {
            get {
                if (_person.EmailAddresses == null || _person.EmailAddresses.Count == 0) {
                    return string.Empty;
                }
                return _person.EmailAddresses[0].Value;
            }
        }

        public string PhoneNumber {
            get {
                if (_person.PhoneNumbers == null || _person.PhoneNumbers.Count == 0) {
                    return string.Empty;
                }
                return _person.PhoneNumbers[0].Value;
            }
        }

        public string Address {
            get {
                if (_person.Addresses == null || _person.Addresses.Count == 0) {
                    return string.Empty;
                }
                return _person.Addresses[0].FormattedValue;
            }
        }

        public string SourceName => "Google";

        #endregion

        public GoogleContact(Google.Apis.PeopleService.v1.Data.Person person) {
            _person = person;
        }
    }
}
