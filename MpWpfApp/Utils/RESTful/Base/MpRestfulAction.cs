using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media.Protection.PlayReady;
using MonkeyPaste;

namespace MpWpfApp {
    public abstract class MpRestfulAction {
        protected virtual Dictionary<string, string> HeaderLookup { get; set; } = new Dictionary<string, string>();

        protected virtual HttpMethod HttpMethod { get; } = HttpMethod.Post;

        protected virtual Uri RequestUri { get; } = new Uri("https://www.google.com");

        protected virtual Encoding ContentEncoding => Encoding.UTF8;

        protected virtual string MediaType => "application/json";

        protected virtual string ProcessResponse(string responseBody) { return responseBody; }

        public string ErrorMessage { get; private set; } = string.Empty;

        public async Task<string> ExecuteRequest(MpBillableItem service, string requestBody, bool isSystemRequest) {            
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage()) {
                try {
                    if (!isSystemRequest) {
                        bool? status = await CheckRestfulApiStatus(service, requestBody);
                        if (status == false) {
                            return string.Empty;
                        }
                    }
                    request.Method = HttpMethod;
                    request.RequestUri = RequestUri;
                    request.Content = new StringContent(requestBody, ContentEncoding, MediaType);
                    foreach (var header in HeaderLookup) {
                        request.Headers.Add(header.Key, header.Value);
                    }

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode) {
                        service.CurrentCycleRequestByteCount += ContentEncoding.GetByteCount(requestBody);

                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseBody) && !isSystemRequest) {
                            service.CurrentCycleResponseByteCount += ContentEncoding.GetByteCount(responseBody);
                            service.CurrentCycleRequestCount++;
                            await service.WriteToDatabaseAsync();
                            return responseBody;
                        }
                    } else {
                        ErrorMessage = response.ReasonPhrase;
                    }
                    await service.WriteToDatabaseAsync();
                    return string.Empty;
                }
                catch (Exception ex) {
                    Console.WriteLine($"Endpoint {request.RequestUri} exception: " + ex.ToString());
                    return string.Empty;
                }
            }
           
        }

        private async Task Reset(MpBillableItem service) {
            // TODO need to use PaymentDate + CycleType for next paymentdatetime
            service.CurrentCycleRequestByteCount = service.CurrentCycleResponseByteCount = 0;
            service.CurrentCycleRequestCount = 0;

            // TODO need to have converter or change CycleType to a TimeSpan
            service.NextPaymentDateTime += TimeSpan.FromDays(30);

            await service.WriteToDatabaseAsync();
        }

        private async Task RefreshCount(MpBillableItem service) {
           if(DateTime.Now > service.NextPaymentDateTime) {
                // TODO add check using ApiName or this app's billing to ensure they paid
                await Reset(service);
           }
        }

        public async Task<bool?> CheckRestfulApiStatus(MpBillableItem service,string requestBody) {
            if(service == null) {
                // TODO need to fill in service models
                return true;
            }
            if(!MpHelpers.IsConnectedToInternet()) {
                return false;
            }

            await RefreshCount(service);

            if (service.CurrentCycleRequestCount >= service.MaxRequestCountPerCycle) {
                throw new Exception("Reached maximum calls this cycle");
            }
            if (service.MaxRequestByteCount != default &&
                ContentEncoding.GetByteCount(requestBody) > service.MaxRequestByteCount) {
                throw new Exception($"Request too large {ContentEncoding.GetByteCount(requestBody)} bytes, Max is {service.MaxRequestByteCount} bytes");
            }
            if (service.MaxRequestByteCountPerCycle != default &&
                service.CurrentCycleResponseByteCount > service.MaxRequestByteCountPerCycle) {
                throw new Exception($"Reached maximum response size of {service.CurrentCycleResponseByteCount} bytes, Max is {service.MaxRequestByteCountPerCycle} bytes");
            }


            return true;
        }

        protected void ShowError() {
            MpConsole.WriteLine(ErrorMessage);
        }
        protected virtual int GetCurCallCount() {
            return MpPreferences.RestfulLinkMinificationCount;
        }

        protected virtual int GetMaxCallCount() {
            return MpPreferences.RestfulLinkMinificationMaxCount;
        }

        protected virtual void IncrementCallCount() {
            MpPreferences.RestfulLinkMinificationCount++;
        }

        protected virtual void ClearCount() {
            MpPreferences.RestfulLinkMinificationCount = 0;
        }
    }
}
