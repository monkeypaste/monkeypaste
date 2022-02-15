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

namespace MonkeyPaste.Plugin {
    public class MpHttpPlugin : MpIAnalyzerPluginComponent {
        #region Private Variables

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
            var requestParams = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());
            if(requestParams == null) {
                Console.WriteLine($"Warning! Empty or malformed request arguments for plugin: '{_httpTransactionFormat.name}'");
                Console.WriteLine($"With args: {args}");
                requestParams = new List<MpAnalyzerPluginRequestItemFormat>();
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
                    request.RequestUri = CreateRequestUri(requestParams);
                    request.Content = CreateRequestContent(requestParams);

                    try {
                        var response = await client.SendAsync(request);

                        string responseStr = await response.Content.ReadAsStringAsync();

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

        private Uri CreateRequestUri(List<MpAnalyzerPluginRequestItemFormat> reqParams) {
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
                    int enumId = GetParamId(qkvp.value);
                    var enumParam = reqParams.FirstOrDefault(x => x.enumId == enumId);
                    if(enumParam == null) {
                        Console.WriteLine($"Error parsing dynamic query item, enumId: '{enumId}' does not exist");
                        Console.WriteLine($"In request with params: ");
                        Console.WriteLine(JsonConvert.SerializeObject(reqParams));
                        return null;
                    }
                    queryVal = enumParam.value;
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

        private HttpContent CreateRequestContent(List<MpAnalyzerPluginRequestItemFormat> reqParams) {
            // TODO may need to add property to discern between different types of HttpContent here
            string mediaType = _httpTransactionFormat.request.body.mediaType;

            Encoding reqEncoding = null;
            if(_httpTransactionFormat.request.body.encoding.ToUpper() == "UTF8") {
                reqEncoding = Encoding.UTF8;
            }

            string body = CreatRequestBody(reqParams);
            return new StringContent(body, reqEncoding, mediaType);
        }

        private string CreatRequestBody(List<MpAnalyzerPluginRequestItemFormat> reqParams) {
            string raw = _httpTransactionFormat.request.body.raw;
            if(!string.IsNullOrEmpty(raw)  && 
               _httpTransactionFormat.request.body.mode.ToLower() == "parameterized") {
                Regex paramReg = new Regex(_paramRefRegEx, RegexOptions.Compiled | RegexOptions.Multiline);
                MatchCollection mc = paramReg.Matches(raw);

                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            int enumId = GetParamId(c.Value);
                            var paramEnum = reqParams.FirstOrDefault(x => x.enumId == enumId);
                            if(paramEnum == null) {
                                Console.WriteLine($"Parameter '{c.Value}' not provided in request: ");
                                Console.WriteLine(JsonConvert.SerializeObject(reqParams));
                            }
                            raw = raw.Replace(c.Value, paramEnum.value);// JsonConvert.SerializeObject(raw.Replace(c.Value, paramEnum.value));


                            System.Object[] body = new System.Object[] { new { Text = paramEnum.value } };
                            var test = JsonConvert.SerializeObject(body);

                            Console.WriteLine("Raw:");
                            Console.WriteLine(raw);
                            Console.WriteLine("Test:");
                            Console.WriteLine(test);
                        }
                    }
                }

            }
            return raw;
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

        private string FindPropertyValue(JContainer c, string propertyName) {
            if(c is JArray ja) {
                foreach (JObject o in ja.Children<JObject>()) {
                    string result = FindPropertyValue(o, propertyName);
                    if (result != null) {
                        return result;
                    }
                }
            } 
            if(c is JObject jo) {
                foreach (JProperty p in jo.Properties()) {
                    if (p.Name.ToLower() == propertyName.ToLower()) {
                        return p.Value.ToString();
                    }
                }
                foreach (JProperty p in jo.Properties()) {
                    string result = FindPropertyValue(p, propertyName);
                    if (result != null) {
                        return result;
                    }                    
                }
            } 
            if(c is JProperty jp) {
                foreach(JObject o in jp.Children<JObject>()) {
                    string result = FindPropertyValue(o, propertyName);
                    if (result != null) {
                        return result;
                    }
                }
            }
            
            
            return null;
        }

        private object CreateResponse(string responseStr) {
            if(_httpTransactionFormat.response.text != null) {
                var tf = _httpTransactionFormat.response.text;
                JObject o = null;
                if(responseStr.StartsWith("[")) {
                    JArray a = JArray.Parse(responseStr);
                    o = a.Children<JObject>().First();
                } else {
                    o = JObject.Parse(responseStr);
                }

                try {
                    JToken dataToken = o.SelectToken(_httpTransactionFormat.response.text.contentPath, false);
                    if (dataToken == null) {
                        tf.content = _httpTransactionFormat.response.text.contentPath;
                    } else {
                        tf.content = dataToken.ToString();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error parsing resposne: " + ex);
                    if (!string.IsNullOrEmpty(_httpTransactionFormat.response.text.contentPath)) {
                        tf.content = _httpTransactionFormat.response.text.contentPath;
                    }
                }

                try {
                    JToken titleToken = o.SelectToken(_httpTransactionFormat.response.text.titlePath, false);
                    if (titleToken == null) {
                        tf.label = _httpTransactionFormat.response.text.titlePath;
                    } else {
                        tf.label = titleToken.ToString();
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Error parsing resposne: " + ex);
                    if (!string.IsNullOrEmpty(_httpTransactionFormat.response.text.titlePath)) {
                        tf.label = _httpTransactionFormat.response.text.titlePath;
                    }
                }


                try {
                    JToken descriptionToken = o.SelectToken(_httpTransactionFormat.response.text.descriptionPath, false);
                    if (descriptionToken == null) {
                        tf.description = _httpTransactionFormat.response.text.descriptionPath;
                    } else {
                        tf.description = descriptionToken.ToString();
                    }
                    return tf;
                }
                catch (Exception ex) {
                    Console.WriteLine("Error parsing resposne: " + ex);
                    if (!string.IsNullOrEmpty(_httpTransactionFormat.response.text.description)) {
                        tf.description = _httpTransactionFormat.response.text.descriptionPath;
                    }
                }
                return tf;
               
            }
            return null;
        }
        #endregion
    }
}
