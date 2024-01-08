using Google.Apis.PeopleService.v1.Data;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GoogleContactsFetcher {
    public class GoogleContact : MpIContact {
        #region Private Variables
        private Person _person;

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

        public GoogleContact(Person person) {
            _person = person;
        }
    }

    // 

    public class GoogleConnectionsFormat {
        public List<GoogleConnectionFormat> connections { get; set; }
        public int totalPeople { get; set; }
        public int totalItems { get; set; }
    }

    public class GoogleConnectionFormat {
        public string resourceName { get; set; }
        public string etag { get; set; }
        public GoogleConnectionMetadataFormat metadata { get; set; }
        public List<GoogleConnectionNameFormat> names { get; set; }
        public List<GoogleConnectionPhotoFormat> photos { get; set; }
        public List<GoogleConnectionPhoneNumberFormat> phoneNumbers { get; set; }
        public List<GoogleConnectionMembershipFormat> memberships { get; set; }
        public List<GoogleConnectionMetaDataFormat> emailAddresses { get; set; }
    }

    public class GoogleConnectionMetaDataFormat {
        public GoogleConnectionMetadataFormat metadata { get; set; }
        public string value { get; set; }
    }

    public class GoogleConnectionMembershipFormat {
        public GoogleConnectionMetadataFormat metadata { get; set; }
        public GoogleConnectionContactGroupMembershipFormat contactGroupMembership { get; set; }
    }

    public class GoogleConnectionContactGroupMembershipFormat {
        public string contactGroupId { get; set; }
        public string contactGroupResourceName { get; set; }
    }

    public class GoogleConnectionPhoneNumberFormat {
        public GoogleConnectionMetadataFormat metadata { get; set; }
        public string value { get; set; }
        public string canonicalForm { get; set; }
        public string type { get; set; }
        public string formattedType { get; set; }
    }

    public class GoogleConnectionPhotoFormat {
        public GoogleConnectionMetadataFormat metadata { get; set; }
        public string url { get; set; }

        [JsonProperty("default")]
        public bool isDefault { get; set; }
    }

    public class GoogleConnectionNameFormat {
        public GoogleConnectionMetadataFormat metadata { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string displayNameLastFirst { get; set; }
        public string unstructuredName { get; set; }
        public string familyName { get; set; }
        public string middleName { get; set; }
        public string honorificPrefix { get; set; }
    }

    public class GoogleConnectionMetadataFormat {
        public List<GoogleConnectionSourceFormat> sources { get; set; }
        public string objectType { get; set; }
        public bool primary { get; set; }
        public GoogleConnectionSourceFormat source { get; set; }
        public bool sourcePrimary { get; set; }
    }

    public class GoogleConnectionSourceFormat {
        public string type { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public DateTime updateTime { get; set; }
    }
}
