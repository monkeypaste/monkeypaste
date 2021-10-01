using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MpWpfApp {
    public enum CurrencyType {
        ALL, XCD, EUR, BBD, BTN, BND, XAF, CUP, USD, FKP, GIP, HUF, IRR, JMD, AUD, LAK, LYD, MKD, XOF, NZD, OMR, PGK, RWF, WST, RSD, SEK, TZS, AMD, BSD, BAM, CVE, CNY, CRC, CZK, ERN, GEL, HTG, INR, JOD, KRW, LBP, MWK, MRO, MZN, ANG, PEN, QAR, STD, SLL, SOS, SDG, SYP, AOA, AWG, BHD, BZD, BWP, BIF, KYD, COP, DKK, GTQ, HNL, IDR, ILS, KZT, KWD, LSL, MYR, MUR, MNT, MMK, NGN, PAB, PHP, RON, SAR, SGD, ZAR, SRD, TWD, TOP, VEF, DZD, ARS, AZN, BYR, BOB, BGN, CAD, CLP, CDF, DOP, FJD, GMD, GYD, ISK, IQD, JPY, KPW, LVL, CHF, MGA, MDL, MAD, NPR, NIO, PKR, PYG, SHP, SCR, SBD, LKR, THB, TRY, AED, VUV, YER, AFN, BDT, BRL, KHR, KMF, HRK, DJF, EGP, ETB, XPF, GHS, GNF, HKD, XDR, KES, KGS, LRD, MOP, MVR, MXN, NAD, NOK, PLN, RUB, SZL, TJS, TTD, UGX, UYU, VND, TND, UAH, UZS, TMT, GBP, ZMW, BTC, BYN
    }

    public class MpCountry {
        [JsonProperty("alpha3")]
        public string Alpha3 { get; set; }

        [JsonProperty("currencyId")]
        public string CurrencyId { get; set; }

        [JsonProperty("currencyName")]
        public string CurrencyName { get; set; }

        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class MpCurrencyHistory {
        public string Date { get; set; }
        public double ExchangeRate { get; set; }
    }

    public class MpCurrency {
        [JsonProperty("currencyName")]
        public string CurrencyName { get; set; }

        [JsonProperty("currencySymbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public static class MpCurrencyConverterRequestHelper {
        public const string FreeBaseUrl = "https://free.currconv.com/api/v7/";
        public const string PremiumBaseUrl = "https://api.currconv.com/api/v7/";

        public static List<MpCurrency> GetAllCurrencies(string apiKey = null) { 
            //string url;
            //if (string.IsNullOrEmpty(apiKey))
            //    url = FreeBaseUrl + "currencies";
            //else
            //    url = PremiumBaseUrl + "currencies" + "?apiKey=" + apiKey;
            string url = FreeBaseUrl + "currencies" + "?apiKey=" + apiKey;

            var jsonString = GetResponse(url);

            var data = JObject.Parse(jsonString)["results"].ToArray();
            return data.Select(item => item.First.ToObject<MpCurrency>()).ToList();
        }

        public static List<MpCountry> GetAllCountries(string apiKey = null) {
            string url = FreeBaseUrl + "countries" + "?apiKey=" + apiKey;

            var jsonString = GetResponse(url);

            var data = JObject.Parse(jsonString)["results"].ToArray();

            return data.Select(item => item.First.ToObject<MpCountry>()).ToList();
        }

        public static List<MpCurrencyHistory> GetHistoryRange(CurrencyType from, CurrencyType to, string startDate, string endDate, string apiKey = null) {
            string url = FreeBaseUrl + "convert?q=" + from + "_" + to + "&compact=ultra&date=" + startDate + "&endDate=" + endDate + "&apiKey=" + apiKey;

            var jsonString = GetResponse(url);
            var data = JObject.Parse(jsonString).First.ToArray();
            return (from item in data
                    let obj = (JObject)item
                    from prop in obj.Properties()
                    select new MpCurrencyHistory {
                        Date = prop.Name,
                        ExchangeRate = item[prop.Name].ToObject<double>()
                    }).ToList();
        }

        public static MpCurrencyHistory GetHistory(CurrencyType from, CurrencyType to, string date, string apiKey = null) {
            string url = FreeBaseUrl + "convert?q=" + from + "_" + to + "&compact=ultra&date=" + date + "&apiKey=" + apiKey;

            var jsonString = GetResponse(url);
            var data = JObject.Parse(jsonString);
            return data.Properties().Select(prop => new MpCurrencyHistory {
                Date = prop.Name,
                ExchangeRate = data[prop.Name][date].ToObject<double>()
            }).FirstOrDefault();
        }

        public static double ExchangeRate(CurrencyType from, CurrencyType to, string apiKey = null) {
            string url = FreeBaseUrl + "convert?q=" + from + "_" + to + "&compact=y&apiKey=" + apiKey;

            var jsonString = GetResponse(url);
            return JObject.Parse(jsonString).First.First["val"].ToObject<double>();
        }

        private static string GetResponse(string url) {
            string jsonString = string.Empty;

            try {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream)) {
                    jsonString = reader.ReadToEnd();
                }
            }
            catch(Exception) {

            }

            return jsonString;
        }
    }

    public class MpCurrencyConverter : MpRestfulApi {
        private static readonly Lazy<MpCurrencyConverter> _Lazy = new Lazy<MpCurrencyConverter>(() => new MpCurrencyConverter());
        public static MpCurrencyConverter Instance { get { return _Lazy.Value; } }

        public List<MpCurrency> CurrencyList { get; private set; } = new List<MpCurrency>();
        public string CurrencySymbols { get; private set; } = string.Empty;

        private string ApiKey { get; }

        private MpCurrencyConverter() : base("Currency Conversion") {

            ApiKey = Properties.Settings.Default.CurrencyConverterFreeApiKey;
        }

        public void Init() {
            try {
                if (!MpHelpers.Instance.IsConnectedToNetwork()) {
                    return;
                }
                CurrencyList = GetAllCurrencies();

                CurrencyList = CurrencyList.OrderBy(x => x.CurrencyName).ToList();

                CurrencySymbols = string.Empty;
                foreach (var currency in CurrencyList) {
                    if (string.IsNullOrEmpty(currency.CurrencySymbol) || CurrencySymbols.Contains(currency.CurrencySymbol)) {
                        continue;
                    }
                    CurrencySymbols += currency.CurrencySymbol + "|";
                }
                CurrencySymbols = CurrencySymbols.Substring(0, CurrencySymbols.Length - 2);
            }
            catch (Exception ex) {
                MonkeyPaste.MpConsole.WriteLine("Currency Converter error: " + ex);
            }
        }

        public double Convert(double amount, CurrencyType from, CurrencyType to) {
            return MpCurrencyConverterRequestHelper.ExchangeRate(from, to, ApiKey) * amount;
        }

        public async Task<double> ConvertAsync(double amount, CurrencyType from, CurrencyType to) {
            return await Task.Run(() => Convert(amount, from, to));
        }


        public List<MpCurrency> GetAllCurrencies() {
            return MpCurrencyConverterRequestHelper.GetAllCurrencies(ApiKey);
        }

        public async Task<List<MpCurrency>> GetAllCurrenciesAsync() {
            return await Task.Run(() => GetAllCurrencies());
        }


        public List<MpCountry> GetAllCountries() {
            return MpCurrencyConverterRequestHelper.GetAllCountries(ApiKey);
        }

        public async Task<List<MpCountry>> GetAllCountriesAsync() {
            return await Task.Run(() => GetAllCountries());
        }


        public MpCurrencyHistory GetHistory(CurrencyType from, CurrencyType to, DateTime date) {
            return MpCurrencyConverterRequestHelper.GetHistory(from, to, date.ToString("yyyy-MM-dd"), ApiKey);
        }

        public async Task<MpCurrencyHistory> GetHistoryAsync(CurrencyType from, CurrencyType to, DateTime date) {
            return await Task.Run(() => GetHistory(from, to, date.ToString("yyyy-MM-dd")));
        }


        public MpCurrencyHistory GetHistory(CurrencyType from, CurrencyType to, string date) {
            return MpCurrencyConverterRequestHelper.GetHistory(from, to, date, ApiKey);
        }

        public async Task<MpCurrencyHistory> GetHistoryAsync(CurrencyType from, CurrencyType to, string date) {
            return await Task.Run(() => GetHistory(from, to, date));
        }


        public List<MpCurrencyHistory> GetHistoryRange(CurrencyType from, CurrencyType to, string startDate, string endDate) {
            return MpCurrencyConverterRequestHelper.GetHistoryRange(from, to, startDate, endDate, ApiKey);
        }

        public async Task<List<MpCurrencyHistory>> GetHistoryRangeAsync(CurrencyType from, CurrencyType to, string startDate, string endDate) {
            return await Task.Run(() => GetHistoryRange(from, to, startDate, endDate));
        }


        public List<MpCurrencyHistory> GetHistoryRange(CurrencyType from, CurrencyType to, DateTime startDate, DateTime endDate) {
            return GetHistoryRange(from, to, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        }

        public async Task<List<MpCurrencyHistory>> GetHistoryRangeAsync(CurrencyType from, CurrencyType to, DateTime startDate, DateTime endDate) {
            return await Task.Run(() => GetHistoryRange(from, to, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd")));
        }

        protected override int GetMaxCallCount() {
            return Properties.Settings.Default.RestfulCurrencyConversionMaxCount;
        }

        protected override int GetCurCallCount() {
            return Properties.Settings.Default.RestfulCurrencyConversionCount;
        }

        protected override void IncrementCallCount() {
            Properties.Settings.Default.RestfulCurrencyConversionCount++;
            Properties.Settings.Default.Save();
        }

        protected override void ClearCount() {
            Properties.Settings.Default.RestfulCurrencyConversionCount = 0;
            Properties.Settings.Default.Save();
        }
    }
}
