using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.PeopleService.v1.Data;
using Google.Apis.Services;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoogleContactsFetcher {
    public class GoogleContactFetcher :
        MpIContactFetcherComponentAsync,
        MpISupportDeferredParameterCommand,
        MpIAnalyzeAsyncComponent,
        INotifyPropertyChanged {
        private string ClientSecretsPath =>
            @"C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia.Desktop\bin\Debug\net7.0-windows\Plugins\GoogleContactsFetcher\client_secrets_desktop.json";
        //Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"client_secrets_desktop.json");

        //     addresses,ageRanges,biographies,birthdays,calendarUrls,clientData
        //    ,coverPhotos,emailAddresses,events,externalIds,genders,imClients *
        //     interests,locales,locations,memberships,metadata,miscKeywords,names
        //    ,nicknames,occupations,organizations,phoneNumbers,photos,relations
        //    ,sipAddresses,skills,urls,userDefined
        private string _personFields = "addresses,names,emailAddresses";
        //"addresses,ageRanges,biographies,birthdays,calendarUrls,clientData,coverPhotos,emailAddresses,events,externalIds,genders,imClients,interests,locales,locations,memberships,metadata,miscKeywords,names,,nicknames,occupations,organizations,phoneNumbers,photos,relations,,sipAddresses,skills,urls,userDefined";

        public async Task<IEnumerable<MpIContact>> FetchContactsAsync(object args) {
            var result = await FetchContactsAsync_internal(_personFields);
            return result;
        }

        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {
            string fields_str = null;
            if (req.GetRequestParamStringListValue(1) is IEnumerable<string> fields) {
                fields_str = string.Join(",", fields);
            }
            if (fields_str == null) {
                return null;
            }
            var contacts = await FetchContactsAsync_internal(fields_str);

            return new MpAnalyzerPluginResponseFormat() {
                userNotifications = new[] {
                    new MpPluginUserNotificationFormat() {
                        Title = $"Request: {fields_str}",
                        Body = string.Join(Environment.NewLine,contacts.Select(x=>x.FullName)),
                        NotificationType = MpPluginNotificationType.PluginResponseMessage,
                    }
                }.ToList()
            };
        }

        private async Task<IEnumerable<MpIContact>> FetchContactsAsync_internal(object personFields) {
            List<MpIContact> fallback = new List<MpIContact>();
            if (!File.Exists(ClientSecretsPath)) {
                Debugger.Break();
                MpConsole.WriteTraceLine($"Google Cred file does not exists at path: '{ClientSecretsPath}'");
                return fallback;
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

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
