using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpHttpRequester {

        public static async Task<string> PostDataToUrlAsync(string url, Dictionary<string, string> keyValuePairs, int timeout_ms = 10_000) {
            // from https://stackoverflow.com/a/62640006/105028
            using (HttpClient httpClient = new HttpClient())
            using (MultipartFormDataContent formDataContent = new MultipartFormDataContent()) {
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeout_ms);
                try {
                    foreach (var keyValuePair in keyValuePairs) {
                        formDataContent.Add(new StringContent(keyValuePair.Value.ToStringOrEmpty()), keyValuePair.Key);
                    }
                    // Post Request And Wait For The Response.
                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(url, formDataContent);

                    // Check If Successful Or Not.
                    if (httpResponseMessage.IsSuccessStatusCode) {
                        // Return Byte Array To The Caller.
                        return await httpResponseMessage.Content.ReadAsStringAsync();
                    } else {
                        // Throw Some Sort of Exception?
                        return string.Empty;
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Post to url '{url}' error.", ex);
                    return string.Empty;
                }
            }
        }
    }
}
