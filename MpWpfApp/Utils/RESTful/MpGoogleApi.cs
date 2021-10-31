using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Discovery;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;

namespace MpWpfApp {
    public class MpGoogleApi : MpSingleton<MpGoogleApi> {
        #region Private constants
        private const string G_API_KEY = "AIzaSyAMhJvuoptUvconBSCJDdQDh3mHYDl9KxM";
        private const string G_OAUTH_CLIENT_ID = "160673782966-087nt2ga6udhjr476ekj8n9apqvct0d9.apps.googleusercontent.com";
        #endregion


        public override async void Init() {
            await Run();
        }

        public async Task Run() {
            // Create the service.
            var service = new DiscoveryService(new BaseClientService.Initializer {
                ApplicationName = "Discovery Sample",
                ApiKey = G_API_KEY,
            });

            // Run the request.
            Console.WriteLine("Executing a list request...");
            var result = await service.Apis.List().ExecuteAsync();

            // Display the results.
            if (result.Items != null) {
                foreach (DirectoryList.ItemsData api in result.Items) {
                    Console.WriteLine(api.Id + " - " + api.Title);
                }
            }
        }

    }
}
