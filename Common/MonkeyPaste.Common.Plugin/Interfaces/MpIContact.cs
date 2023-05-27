namespace MonkeyPaste.Common.Plugin {
    public interface MpIContact {
        object Source { get; }
        string SourceName { get; }
        string guid { get; }


        string FirstName { get; }
        string LastName { get; }
        string FullName { get; }
        string PhoneNumber { get; }
        string Address { get; }
        string Email { get; }
    }
}
