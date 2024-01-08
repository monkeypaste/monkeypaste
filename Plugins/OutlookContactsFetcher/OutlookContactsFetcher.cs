using Microsoft.Office.Interop.Outlook;
using MonkeyPaste.Common.Plugin;

namespace OutlookContactsFetcher {
    public class OutlookContactsFetcher : MpIContactFetcherComponent {

        public MpPluginContactFetchResponseFormat Fetch(MpPluginContactFetchRequestFormat req) {
            var contacts = new List<OutlookContact>();

            Application outlookObj = new Application();
            MAPIFolder folderContacts = (MAPIFolder)outlookObj.Session.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
            Items outlookItems = folderContacts.Items;


            for (int i = 0; i < outlookItems.Count; i++) {
                ContactItem contact = (ContactItem)outlookItems[i + 1];
                contacts.Add(new OutlookContact {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    FullName = contact.FullName,
                    Email = contact.Email1Address,
                    Address = contact.MailingAddress,
                    PhoneNumber = contact.PrimaryTelephoneNumber,
                    Source = contact,
                    guid = System.Guid.NewGuid().ToString()
                });
            }

            return new MpPluginContactFetchResponseFormat() {
                Contacts = contacts
            };
        }

    }
}
