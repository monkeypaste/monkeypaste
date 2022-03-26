using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.PeopleService.v1;
using Google.Apis.Books.v1;
using Google.Apis.Books.v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.PeopleService.v1.Data;
using System.Collections.Generic;

namespace MonkeyPaste {
    public static class MpGoogleApiHelpers {
        public static string ClientSecretsPath => Path.Combine(Directory.GetCurrentDirectory(), @"Resources\Data\Google\client_secrets_desktop.json");

        public static string ContactsScope => @"https://www.googleapis.com/auth/contacts";
        public static string ContactsReadOnlyScope => @"https://www.googleapis.com/auth/contacts.readonly";

        public static async Task Test(string access_token) {
            if(!File.Exists(ClientSecretsPath)) {
                Debugger.Break();
            }
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
            peopleRequest.PersonFields = "names,emailAddresses";
            ListConnectionsResponse response = peopleRequest.Execute();
            IList<Person> people = response.Connections;

            //credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            //                new ClientSecrets {
            //                    ClientId = "160673782966-ib60mppl9ll9kijv0qks9ue6ndtjs9gj.apps.googleusercontent.com",
            //                    ClientSecret = "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"
            //                },
            //                new[] { ContactsReadOnlyScope },
            //                "user",
            //                CancellationToken.None);

            // Create the service.
            

            //var result = await service.ContactGroups.List().ExecuteAsync();
            return;
            //var bookshelves = await service.Mylibrary.Bookshelves.List().ExecuteAsync();
        }
    }
}
