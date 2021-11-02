using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpOutlookContactCollectionViewModel : MpSingletonViewModel<MpOutlookContactCollectionViewModel,object> {
        #region Properties

        public ObservableCollection<ContactItem> OutlookContacts = new ObservableCollection<ContactItem>();

        #endregion

        #region Constructor

        public override async Task Init() {
            await Task.Run(() => {
                IsBusy = true;

                Microsoft.Office.Interop.Outlook.Application outlookObj = new Microsoft.Office.Interop.Outlook.Application();
                MAPIFolder folderContacts = (MAPIFolder)outlookObj.Session.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
                Microsoft.Office.Interop.Outlook.Items outlookItems = folderContacts.Items;

                OutlookContacts.Clear();

                for (int i = 0; i < outlookItems.Count; i++) {
                    ContactItem contact = (Microsoft.Office.Interop.Outlook.ContactItem)outlookItems[i + 1];
                    OutlookContacts.Add(contact);
                }

                IsBusy = false;
            });
        }

        #endregion
    }
}
