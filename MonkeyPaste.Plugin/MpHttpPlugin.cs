using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Web;

namespace MonkeyPaste.Plugin {
    public class MpHttpPlugin : MpIAnalyzerPluginComponent {
        #region Private Variables

        private List<MpAnalyzerPluginRequestItemFormat> reqParams;
        private MpHttpTransactionFormat _httpTransactionFormat;        

        private readonly string _paramRefRegEx = @"@[0-9]*";
        #endregion

        #region Properties

        public HttpMethod RequestMethod {
            get {
                if(_httpTransactionFormat == null || 
                    _httpTransactionFormat.request == null ||
                   string.IsNullOrWhiteSpace(_httpTransactionFormat.request.method)) {
                    throw new HttpRequestException("Request method undefined for " + _httpTransactionFormat.name);
                }
                string methodStr = _httpTransactionFormat.request.method;

                return new HttpMethod(methodStr);
            }
        }

        #endregion

        #region Constructor

        public MpHttpPlugin(MpHttpTransactionFormat hf) {
            _httpTransactionFormat = hf;
        }

        #endregion

        #region Public Methods

        public async Task<object> AnalyzeAsync(object args) {
            reqParams = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());
            if(reqParams == null) {
                Console.WriteLine($"Warning! Empty or malformed request arguments for plugin: '{_httpTransactionFormat.name}'");
                Console.WriteLine($"With args: {args}");
                reqParams = new List<MpAnalyzerPluginRequestItemFormat>();
            }
            using (var client = new HttpClient()) {
                using (var request = new HttpRequestMessage()) {
                    request.Method = RequestMethod;
                    if (_httpTransactionFormat != null &&
                        _httpTransactionFormat.request != null &&
                       _httpTransactionFormat.request.header != null) {
                        foreach(var kvp in _httpTransactionFormat.request.header) {
                            if (kvp.type == "guid") {
                                request.Headers.Add(kvp.key, System.Guid.NewGuid().ToString());
                            } else {
                                request.Headers.Add(kvp.key, kvp.value);
                            }
                        }
                        
                    }
                    request.RequestUri = CreateRequestUri();
                    request.Content = CreateRequestContent();

                    try {
                        var response = await client.SendAsync(request);

                        if(!response.IsSuccessStatusCode) {
                            Debugger.Break();
                        }
                        
                        string responseStr = await response.Content.ReadAsStringAsync();
                        
                        request.Content.Dispose();
                        var responseObj = CreateResponse(responseStr);
                        return responseObj;
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Error performing analysis w/ plugin: " + _httpTransactionFormat.name);
                        Console.WriteLine(ex);
                        return null;
                    }                    
                }
            }
        }

        #endregion

        #region Private Methods

        private Uri CreateRequestUri() {
            if (_httpTransactionFormat == null ||
                _httpTransactionFormat.request == null ||
                _httpTransactionFormat.request.url == null) {
                throw new HttpRequestException("Url undefined for " + _httpTransactionFormat.name);
            }
            var urlFormat = _httpTransactionFormat.request.url;
            string uriStr = string.Format(@"{0}://", urlFormat.protocol);
            uriStr += string.Join(".", urlFormat.host) + "/";
            uriStr += string.Join("/", urlFormat.path) + "?";
            foreach (var qkvp in urlFormat.query) {
                string queryVal = qkvp.value;
                if (qkvp.isEnumId) {
                    queryVal = GetParamValue(qkvp.value);
                    if(string.IsNullOrEmpty(queryVal) && qkvp.omitIfNullOrEmpty) {
                        continue;
                    }
                }                
                uriStr += string.Format(@"{0}={1}&", qkvp.key, queryVal);
            }
            uriStr = uriStr.Substring(0, uriStr.Length - 1);
            if(!Uri.IsWellFormedUriString(uriStr,UriKind.Absolute)) {
                Console.WriteLine("Uri string is not properly defined: " + uriStr);
                return null;
            }
            return new Uri(uriStr);
        }

        private HttpContent CreateRequestContent() {
            // TODO may need to add property to discern between different types of HttpContent here
            string mediaType = _httpTransactionFormat.request.body.mediaType;

            Encoding reqEncoding = Encoding.UTF8;
            if(_httpTransactionFormat.request.body.encoding.ToUpper() == "UTF8") {
                reqEncoding = Encoding.UTF8;
            }

            string body = CreatRequestBody();
            if(mediaType.ToLower() == "application/json") {
                var sc = new StringContent(body, reqEncoding, mediaType);
                sc.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                return sc;
            } else if(mediaType.ToLower() == "application/octet-stream") {
                var bac = new ByteArrayContent(Convert.FromBase64String(body));
                bac.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                return bac;
            } else {
                throw new Exception("Currently unsupported mediaType");
            }
            
        }

        private string CreatRequestBody() {
            string raw = _httpTransactionFormat.request.body.raw;
            if(!string.IsNullOrEmpty(raw)  && 
               _httpTransactionFormat.request.body.mode.ToLower() == "parameterized") {
                Regex paramReg = new Regex(_paramRefRegEx, RegexOptions.Compiled | RegexOptions.Multiline);
                MatchCollection mc = paramReg.Matches(raw);

                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            string paramVal = GetParamValue(c.Value);
                            string escapedParamVal = HttpUtility.JavaScriptStringEncode(paramVal);

                            raw = raw.Replace(c.Value, escapedParamVal);// JsonConvert.SerializeObject(raw.Replace(c.Value, paramEnum.value));
                            //raw = JsonConvert.SerializeObject(raw);
                            
                            System.Object[] body = new System.Object[] { new { Text = paramVal } };
                            var test = JsonConvert.SerializeObject(body);

                            Console.WriteLine("Raw Param: ");
                            Console.WriteLine(paramVal);
                            Console.WriteLine("serialized param: ");
                            Console.WriteLine(escapedParamVal);
                            Console.WriteLine("Final Raw:");
                            Console.WriteLine(raw);
                            Console.WriteLine("Test:");
                            Console.WriteLine(test);
                        }
                    }
                }

            }
            return raw;
        }

        private string GetParamValue(string queryParamValueStr) {
            int enumId = GetParamId(queryParamValueStr);
            var enumParam = reqParams.FirstOrDefault(x => x.enumId == enumId);
            if (enumParam == null) {
                Console.WriteLine($"Error parsing dynamic query item, enumId: '{enumId}' does not exist");
                Console.WriteLine($"In request with params: ");
                Console.WriteLine(JsonConvert.SerializeObject(reqParams));
                return null;
            }
            return enumParam.value;
        }

        private int GetParamId(string queryParamValueStr) {
            if(string.IsNullOrEmpty(queryParamValueStr)) {
                throw new Exception("Error creating http uri, dynamic query item has undefined value");
            }
            if(!queryParamValueStr.StartsWith("@")) {
                throw new Exception("Parameterized values must start with '@'");
            }
            try {
                return Convert.ToInt32(queryParamValueStr.Substring(1, queryParamValueStr.Length - 1));
            } catch(Exception ex) {
                throw new Exception("Error converting param reference: " + queryParamValueStr + " "+ex);
            }
        }

        private string QueryJsonPath(JObject jo, string propertyPath, bool omitIfNull) {
            string result = string.Empty;
            if(propertyPath.StartsWith("@")) {
                return GetParamValue(propertyPath);
            }
            try {
                JToken dataToken = jo.SelectToken(propertyPath, false);
                if (dataToken == null) {
                    if(!omitIfNull) {
                        result = propertyPath;
                    }                    
                } else {
                    result = dataToken.ToString();
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error parsing resposne: " + ex);
                if (!omitIfNull) {
                    result = propertyPath;
                }
            }
            return result;
        }

        private object CreateResponse(string responseStr) {
            var textResponse = new MpAnalyzerPluginTextResponseFormat() {
                label = string.Empty,
                description = string.Empty,
                content = responseStr
            };
            var responseMap = _httpTransactionFormat.responseMap;

            JObject o = null;
            if (responseStr.StartsWith("[")) {
                JArray a = JArray.Parse(responseStr);
                o = a.Children<JObject>().First();
            } else {
                o = JObject.Parse(responseStr);
            }

            textResponse.label = string.Join(string.Empty, responseMap.titlePath.Select(x => QueryJsonPath(o, x, responseMap.omitTitleIfPathNotFound)));
            textResponse.content = string.Join(string.Empty, responseMap.contentPath.Select(x => QueryJsonPath(o, x, responseMap.omitContentIfPathNotFound)));
            textResponse.description = string.Join(string.Empty, responseMap.descriptionPath.Select(x => QueryJsonPath(o, x, responseMap.omitDescriptionIfPathNotFound)));

            return textResponse;
        }

        #endregion
    }
}
