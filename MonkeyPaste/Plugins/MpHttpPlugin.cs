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
using MonkeyPaste.Plugin;
namespace MonkeyPaste {
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

                        Console.WriteLine($"Response from '{request.RequestUri.AbsoluteUri}':");
                        Console.WriteLine(responseStr.ToPrettyPrintJson());

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
                            
                            //System.Object[] body = new System.Object[] { new { Text = paramVal } };
                            //var test = JsonConvert.SerializeObject(body);

                            //Console.WriteLine("Raw Param: ");
                            //Console.WriteLine(paramVal);
                            //Console.WriteLine("serialized param: ");
                            //Console.WriteLine(escapedParamVal);
                            //Console.WriteLine("Final Raw:");
                            //Console.WriteLine(raw);
                            //Console.WriteLine("Test:");
                            //Console.WriteLine(test);
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

        private object CreateResponse(string responseStr) {
            var response = _httpTransactionFormat.response;

            JObject jo;
            if (responseStr.StartsWith("[")) {
                JArray a = JArray.Parse(responseStr);
                jo = a.Children<JObject>().First();
            } else {
                jo = JObject.Parse(responseStr);
            }

            response.annotations = CreateAnnotations(_httpTransactionFormat.response.annotations,jo);
            //textResponse.label = string.Join(string.Empty, response.titlePath.Select(x => QueryJsonPath(o, x, response.omitTitleIfPathNotFound)));
            //textResponse.content = string.Join(string.Empty, response.contentPath.Select(x => QueryJsonPath(o, x, response.omitContentIfPathNotFound)));
            //textResponse.description = string.Join(string.Empty, response.descriptionPath.Select(x => QueryJsonPath(o, x, response.omitDescriptionIfPathNotFound)));

            return response;
        }

        private List<MpPluginResponseAnnotationFormat> CreateAnnotations(List<MpPluginResponseAnnotationFormat> al, JObject jo, int idx = 0) {
            if(al == null) {
                return null;
            }

            for (int i = 0; i < al.Count; i++) {
                al[i] = CreateAnnotation(al[i], jo, i);
            }
            return al;
        }

        private MpPluginResponseAnnotationFormat CreateAnnotation(MpPluginResponseAnnotationFormat a, JObject jo, int idx = 0) {
            try {
                if (a.label != null) {
                    a.label.SetValue(jo, reqParams, idx);
                }
                if (a.score != null) {
                    a.score.SetValue(jo, reqParams, idx);
                }
                if (a.box != null) {
                    a.box.x.SetValue(jo, reqParams, idx);
                    a.box.y.SetValue(jo, reqParams, idx);
                    a.box.width.SetValue(jo, reqParams, idx);
                    a.box.height.SetValue(jo, reqParams, idx);
                }
            }catch(MpJsonPathPropertyException jppex) {
                Console.WriteLine(jppex);
                return null;
            }
            if(a.dynamicChildren != null && a.dynamicChildren.Count > 0) {
                if(a.children == null) {
                    a.children = new List<MpPluginResponseAnnotationFormat>();
                }
                for (int i = 0; i < a.dynamicChildren.Count; i++) {
                    int curDynamicChildIdx = 0;
                    while(true) {
                        var childAnnotationFormat = CreateAnnotation(a.dynamicChildren[i], jo, curDynamicChildIdx);
                        if(childAnnotationFormat == null) {
                            break;
                        }
                        a.children.Add(childAnnotationFormat);
                        curDynamicChildIdx++;
                    }                 
                }
            }
            return a;
        }

        
        #endregion
    }
}
