using MonkeyPaste.Common.Plugin;

namespace OutlookContactsFetcher {
    public class OutlookContact : MpIContact {
        protected object _contactSource;

        public object Source { get; set; }
        public virtual string SourceName { get; set; } = "Outlook";
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string FullName { get; set; }

        public virtual string Email { get; set; }

        public virtual string PhoneNumber { get; set; }

        public virtual string Address { get; set; }
        public string guid { get; set; }
    }
}
