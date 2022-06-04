using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpGoogleConnectionsFormat : MpJsonObject {
        public List<MpGoogleConnectionFormat> connections { get; set; }
        public int totalPeople { get; set; }
        public int totalItems { get; set; }
    }

    public class MpGoogleConnectionFormat : MpJsonObject {
        public string resourceName { get; set; }
        public string etag { get; set; }
        public MpGoogleConnectionMetadataFormat metadata { get; set; }
        public List<MpGoogleConnectionNameFormat> names { get; set; }
        public List<MpGoogleConnectionPhotoFormat> photos { get; set; }
        public List<MpGoogleConnectionPhoneNumberFormat> phoneNumbers { get; set; }
        public List<MpGoogleConnectionMembershipFormat> memberships { get; set; }
        public List<MpGoogleConnectionMetaDataFormat> emailAddresses { get; set; }
    }

    public class MpGoogleConnectionMetaDataFormat : MpJsonObject {
        public MpGoogleConnectionMetadataFormat metadata { get; set; }
        public string value { get; set; }
    }

    public class MpGoogleConnectionMembershipFormat : MpJsonObject {
        public MpGoogleConnectionMetadataFormat metadata { get; set; }
        public MpGoogleConnectionContactGroupMembershipFormat contactGroupMembership { get; set; }
    }

    public class MpGoogleConnectionContactGroupMembershipFormat : MpJsonObject {
        public string contactGroupId { get; set; }
        public string contactGroupResourceName { get; set; }
    }

    public class MpGoogleConnectionPhoneNumberFormat : MpJsonObject {
        public MpGoogleConnectionMetadataFormat metadata { get; set; }
        public string value { get; set; }
        public string canonicalForm { get; set; }
        public string type { get; set; }
        public string formattedType { get; set; }
    }

    public class MpGoogleConnectionPhotoFormat {
        public MpGoogleConnectionMetadataFormat metadata { get; set; }
        public string url { get; set; }

        [JsonProperty("default")]
        public bool isDefault { get; set; }
    }

    public class MpGoogleConnectionNameFormat : MpJsonObject {
        public MpGoogleConnectionMetadataFormat metadata { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public string displayNameLastFirst { get; set; }
        public string unstructuredName { get; set; }
        public string familyName { get; set; }
        public string middleName { get; set; }
        public string honorificPrefix { get; set; }
    }

    public class MpGoogleConnectionMetadataFormat : MpJsonObject {
        public List<MpGoogleConnectionSourceFormat> sources { get; set; }
        public string objectType { get; set; }
        public bool primary { get; set; }
        public MpGoogleConnectionSourceFormat source { get; set; }
        public bool sourcePrimary { get; set; }
    }

    public class MpGoogleConnectionSourceFormat : MpJsonObject {
        public string type { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public DateTime updateTime { get; set; }
    }
}
