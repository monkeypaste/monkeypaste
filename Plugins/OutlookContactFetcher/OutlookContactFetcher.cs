﻿using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

namespace OutlookContactFetcher {
    public class OutlookContactFetcher : MpIContactFetcherComponent {

        public IEnumerable<MpIContact> FetchContacts(object args) {
            //await Task.Delay(1);
            var contacts = new List<OutlookContact>();
            return contacts;

            Microsoft.Office.Interop.Outlook.Application outlookObj = new Microsoft.Office.Interop.Outlook.Application();
            MAPIFolder folderContacts = (MAPIFolder)outlookObj.Session.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
            Microsoft.Office.Interop.Outlook.Items outlookItems = folderContacts.Items;


            for (int i = 0; i < outlookItems.Count; i++) {
                ContactItem contact = (Microsoft.Office.Interop.Outlook.ContactItem)outlookItems[i + 1];
                contacts.Add(new OutlookContact {
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    FullName = contact.FullName,
                    Email = contact.Email1Address,
                    Address = contact.MailingAddress,
                    PhoneNumber = contact.PrimaryTelephoneNumber,
                    Source = contact
                });
            }

            return contacts;
        }

    }
}