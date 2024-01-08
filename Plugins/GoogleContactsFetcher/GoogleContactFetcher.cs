using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleContactsFetcher {
    public class GoogleContactFetcher :
        MpIContactFetcherComponentAsync,
        MpISupportDeferredParameterCommand,
        MpIAnalyzeComponentAsync {
        const string FIELDS_PARAM_ID = "1";

        static string _credPath =>
            Path.Combine(
                Path.GetDirectoryName(typeof(GoogleContactFetcher).Assembly.Location),
                "credentials.json");
        private string _clientSecretsPath = null;

        private string _personFields = "metadata,addresses,names,emailAddresses";
        //"addresses,ageRanges,biographies,birthdays,calendarUrls,clientData,coverPhotos,emailAddresses,events,externalIds,genders,
        //imClients,interests,locales,locations,memberships,metadata,miscKeywords,names,,nicknames,occupations,organizations,phoneNumbers,
        //photos,relations,,sipAddresses,skills,urls,userDefined";

        public async Task<MpPluginContactFetchResponseFormat> FetchAsync(MpPluginContactFetchRequestFormat args) {
            var result = await FetchContactsAsync_internal(_personFields);
            return new MpPluginContactFetchResponseFormat() {
                Contacts = result
            };
        }
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            string fields_str = null;
            if (req.GetParamValue<List<string>>(FIELDS_PARAM_ID) is IEnumerable<string> fields) {
                fields_str = string.Join(",", fields);
            }
            if (fields_str == null) {
                return null;
            }
            var contacts = await FetchContactsAsync_internal(fields_str);

            return new MpAnalyzerPluginResponseFormat() {
                userNotifications = new[] {
                    new MpUserNotification() {
                        Title = $"Request: {fields_str}",
                        Body = string.Join(Environment.NewLine,contacts.Select(x=>x.FullName)),
                        NotificationType = MpPluginNotificationType.PluginResponseMessage,
                    }
                }.ToList()
            };
        }

        private async Task<IEnumerable<MpIContact>> FetchContactsAsync_internal(object personFields) {
            List<MpIContact> fallback = new List<MpIContact>();
            if (!File.Exists(_credPath)) {
                MpDebug.Break();
                MpConsole.WriteTraceLine($"Google Cred file does not exists at path: '{_credPath}'");
                return fallback;
            }

            try {
                UserCredential credential = null;
                using (var stream = new FileStream(_credPath, FileMode.Open, FileAccess.Read)) {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        new[] { PeopleServiceService.ScopeConstants.ContactsReadonly },
                        "user", CancellationToken.None);
                }
                var peopleService = new PeopleServiceService(new BaseClientService.Initializer() {
                    HttpClientInitializer = credential,
                    ApplicationName = "MonkeyPaste - Contact Fetcher",
                });

                PeopleResource.ConnectionsResource.ListRequest peopleRequest =
                peopleService.People.Connections.List("people/me");
                peopleRequest.PersonFields = personFields;
                ListConnectionsResponse response = peopleRequest.Execute();
                IList<Person> people = response.Connections;

                return people.Select(x => new GoogleContact(x));

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("Exception accessing google contacts, returning NOTHING ", ex);
            }
            return fallback;
        }

        public MpPluginDeferredParameterCommandResponseFormat RequestParameterCommand(MpPluginDeferredParameterCommandRequestFormat req) {
            return new MpPluginDeferredParameterCommandResponseFormat() {
                DeferredCommand = GoogleAuthCommand
            };
        }

        MpIAsyncCommand<object> GoogleAuthCommand => new MpAsyncCommand<object>(
            async (args) => {
                await Task.Delay(1);
                MpDebug.Break("cmd deferred");
            });

    }
}
