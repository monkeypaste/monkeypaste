using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MonkeyPaste.Functions {
    public static class ClearStorage {
        [FunctionName("ClearStorage")]
        public static async Task Run([TimerTrigger("0 */60 * * * *")] TimerInfo myTimer, ILogger log) {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            await StorageHelper.Clear();
        }
    }
}
