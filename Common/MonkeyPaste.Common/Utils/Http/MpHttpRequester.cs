using MonkeyPaste.Avalonia;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpHttpRequester {
        const string SUCCESS_PREFIX = "[SUCCESS]";
        const string ERROR_PREFIX = "[ERROR]";

        public static async Task<string> SubmitPostDataToUrlAsync(
            string url,
            Dictionary<string, string> keyValuePairs,
            int timeout_ms = 10_000,
            bool add_debug = MpServerConstants.IS_SERVER_LOCAL) {
            // from https://stackoverflow.com/a/62640006/105028
            using (HttpClient httpClient = new HttpClient())
            using (MultipartFormDataContent formDataContent = new MultipartFormDataContent()) {
                try {
                    if (keyValuePairs != null) {
                        foreach (var keyValuePair in keyValuePairs) {
                            formDataContent.Add(new StringContent(keyValuePair.Value.ToStringOrEmpty()), keyValuePair.Key);
                        }
                    }
                    if (add_debug) {
                        formDataContent.Add(new StringContent("1"), "XDEBUG_SESSION");
                        httpClient.Timeout = TimeSpan.FromMinutes(30);
                    } else {
                        httpClient.Timeout = TimeSpan.FromMilliseconds(timeout_ms);
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

        public static async Task<string> SubmitGetDataToUrlAsync(string url, Dictionary<string, string> keyValuePairs, int timeout_ms = 10_000) {
            var sb = new StringBuilder();

            if (keyValuePairs != null) {
                sb.Append("?");
                foreach (var (kvp, idx) in keyValuePairs.WithIndex()) {
                    if (!string.IsNullOrEmpty(kvp.Key)) {
                        sb.Append(kvp.Key);
                        sb.Append("=");
                    }
                    sb.Append(kvp.Value);
                    if (idx < keyValuePairs.Count - 1) {
                        sb.Append("&");
                    }
                }
            }
            string result = await MpFileIo.ReadTextFromUriAsync(url + sb.ToString(), timeoutMs: timeout_ms);
            return result;
        }

        public static bool ProcessServerResponse(string response, out Dictionary<string, string> args) {
            response = response.ToStringOrEmpty();
            //MpConsole.WriteLine($"Server response: '{response}'");
            string msg_suffix;
            bool success = false;

            if (response.StartsWith(SUCCESS_PREFIX) &&
                response.SplitNoEmpty(SUCCESS_PREFIX) is string[] success_parts) {
                msg_suffix = string.Join(string.Empty, success_parts);
                success = true;
            } else if (response.StartsWith(ERROR_PREFIX) &&
                response.SplitNoEmpty(ERROR_PREFIX) is string[] error_parts) {
                msg_suffix = string.Join(string.Empty, error_parts);
            } else {
                msg_suffix = response;
            }

            args = msg_suffix.DeserializeObject<Dictionary<string, string>>();
            if (!string.IsNullOrWhiteSpace(msg_suffix) && args.Count == 0) {
                // shouldnon-input error, add it to empty key
                MpDebug.Assert(!success, $"Should only have non-lookup result for error");
                args = new() { { string.Empty, msg_suffix } };
            }
            return success;
        }
    }
}
