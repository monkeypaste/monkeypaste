using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.PeopleService.v1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.PeopleService.v1.Data;
using System.Collections.Generic;
using MonkeyPaste.Common.Plugin;
using System.Linq;
using MonkeyPaste.Common;
using System.Reflection;

namespace GoogleContactsFetcher {
    public class GoogleContactFetcher : MpIContactFetcherComponentAsync {
        private string ClientSecretsPath => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"client_secrets_desktop.json");

        //     addresses,ageRanges,biographies,birthdays,calendarUrls,clientData
        //    ,coverPhotos,emailAddresses,events,externalIds,genders,imClients *
        //     interests,locales,locations,memberships,metadata,miscKeywords,names
        //    ,nicknames,occupations,organizations,phoneNumbers,photos,relations
        //    ,sipAddresses,skills,urls,userDefined
        private string _personFields = "addresses,names,emailAddresses";
            //"addresses,ageRanges,biographies,birthdays,calendarUrls,clientData,coverPhotos,emailAddresses,events,externalIds,genders,imClients,interests,locales,locations,memberships,metadata,miscKeywords,names,,nicknames,occupations,organizations,phoneNumbers,photos,relations,,sipAddresses,skills,urls,userDefined";

        public async Task<IEnumerable<MpIContact>> FetchContactsAsync(object args) {
            List<MpIContact> emptyContacts = new List<MpIContact>();
            if(!File.Exists(ClientSecretsPath)) {
                Debugger.Break();
                MpConsole.WriteTraceLine($"Google Cred file does not exists at path: '{ClientSecretsPath}'");
                return emptyContacts;
            }

            try {
                UserCredential credential = null;
                using (var stream = new FileStream(ClientSecretsPath, FileMode.Open, FileAccess.Read)) {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { PeopleServiceService.ScopeConstants.ContactsReadonly },
                        "user", CancellationToken.None);
                }
                var peopleService = new PeopleServiceService(new BaseClientService.Initializer() {
                    HttpClientInitializer = credential,
                    ApplicationName = "Google API Test",
                });

                PeopleResource.ConnectionsResource.ListRequest peopleRequest =
                peopleService.People.Connections.List("people/me");
                peopleRequest.PersonFields = _personFields;
                ListConnectionsResponse response = peopleRequest.Execute();
                IList<Person> people = response.Connections;

                return people.Select(x => new GoogleContact(x));

            } catch(Exception ex) {
                MpConsole.WriteTraceLine("Exception accessing google contacts, returning NOTHING ", ex);
            }

            return emptyContacts;
        }
    }
}
