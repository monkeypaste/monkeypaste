using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
//using Xamarin.Forms;

namespace MonkeyPaste.Avalonia {
    public class MpHttpAnalyzerPlugin : MpDeclarativePluginBase {
        #region Private Variables
        private IEnumerable<MpParameterRequestItemFormat> _reqParams;
        private MpHttpAnalyzerTransactionFormat _httpTransactionFormat;
        private JToken _rootResponseToken;
        private readonly string _paramRefRegEx = @"@[0-9]*";

        private string _unalteredHttpFormat;
        #endregion

        #region Properties

        public HttpMethod RequestMethod {
            get {
                if (_httpTransactionFormat == null ||
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

        public MpHttpAnalyzerPlugin(MpHttpAnalyzerTransactionFormat hf) {
            _httpTransactionFormat = hf;
        }

        #endregion

        #region Public Methods

        public string GetRequestUri(IEnumerable<MpParameterRequestItemFormat> reqParams) {
            var reqMsg = CreateRequestMessage(reqParams);
            if (reqMsg == null) {
                return null;
            }
            return reqMsg.RequestUri.AbsoluteUri;
        }

        #endregion

        #region Protected Methods

        protected override string RunDeclarativeAnalyzer(MpAnalyzerPluginRequestFormat req) {
            throw new NotImplementedException("Http only asynchronous");
        }
        protected override async Task<string> RunDeclarativeAnalyzerAsync(MpAnalyzerPluginRequestFormat req) {
            _unalteredHttpFormat = JsonConvert.SerializeObject(_httpTransactionFormat);

            _reqParams = req.items;
            if (_reqParams == null) {
                Console.WriteLine($"Warning! Empty or malformed request arguments for plugin: '{_httpTransactionFormat.name}'");
                Console.WriteLine($"With args: {req.SerializeJsonObject()}");
                _reqParams = new List<MpParameterRequestItemFormat>();
            }

            using (var client = new HttpClient()) {
                using (var httpRequest = CreateRequestMessage(_reqParams)) {
                    try {
                        var response = await client.SendAsync(httpRequest);

                        if (!response.IsSuccessStatusCode) {
                            // NOTE fix command should probably open manfiest folder but only http plugin info is provided so just opening plugin folder
                            var userAction = await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                                                    notificationType: MpNotificationType.BadHttpRequest,
                                                    body: $"{response.ReasonPhrase}",
                                                    fixCommand: new MpCommand(() => MpFileIo.OpenFileBrowser(MpPluginLoader.PluginRootFolderPath)));

                            //if(userAction == MpNotificationDialogResultType.Retry) {
                            //    return new MpPluginResponseFormatBase() {
                            //        message = MpPluginResponseFormatBase.RETRY_MESSAGE
                            //    };
                            //}
                        }

                        string responseStr = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response from '{httpRequest.RequestUri.AbsoluteUri}':");
                        Console.WriteLine(responseStr.ToPrettyPrintJson());
                        httpRequest.Content.Dispose();

                        // return raw response to base which calls decode override
                        return responseStr;
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Error performing analysis w/ plugin: " + _httpTransactionFormat.name);
                        Console.WriteLine(ex);
                        return null;
                    }
                }
            }
        }

        protected override MpAnalyzerPluginResponseFormat DecodeResponseOutput(string responseStr) {
            var responseObj = CreateResponse(responseStr);
            //reset format or subsequent requests compound data 
            _httpTransactionFormat = JsonConvert.DeserializeObject<MpHttpAnalyzerTransactionFormat>(_unalteredHttpFormat);
            return responseObj;
        }
        #endregion

        #region Private Methods

        #region Request
        private HttpRequestMessage CreateRequestMessage(IEnumerable<MpParameterRequestItemFormat> requestParams) {
            _reqParams = requestParams;//JsonConvert.DeserializeObject<List<MpPluginRequestItemFormat>>(args.ToString());
            if (_reqParams == null) {
                Console.WriteLine($"Warning! Empty or malformed request arguments for plugin: '{_httpTransactionFormat.name}'");
                Console.WriteLine($"With args: {requestParams.ToString()}");
                _reqParams = new List<MpParameterRequestItemFormat>();
            }
            var request = new HttpRequestMessage();
            request.Method = RequestMethod;
            CreateHeaders(request);
            request.RequestUri = CreateRequestUri();
            request.Content = CreateRequestContent();
            return request;
        }

        private void CreateHeaders(HttpRequestMessage request) {
            if (_httpTransactionFormat != null &&
                _httpTransactionFormat.request != null &&
                _httpTransactionFormat.request.header != null) {
                foreach (var kvp in _httpTransactionFormat.request.header) {
                    if (kvp.type == "guid") {
                        request.Headers.Add(kvp.key, System.Guid.NewGuid().ToString());
                    } else if (kvp.valuePath != null) {
                        kvp.valuePath.SetValue(null, _reqParams, 0);
                        request.Headers.Add(kvp.key, kvp.valuePath.value);
                    } else {
                        request.Headers.Add(kvp.key, kvp.value);
                    }
                    Console.WriteLine($"Header Item: key: '{kvp.key}' value: '{(kvp.valuePath != null ? kvp.valuePath.value : kvp.value)}'");
                }
            }
        }

        private Uri CreateRequestUri() {
            if (_httpTransactionFormat == null ||
                _httpTransactionFormat.request == null ||
                _httpTransactionFormat.request.url == null) {
                throw new HttpRequestException("Url undefined for " + _httpTransactionFormat.name);
            }
            var urlFormat = _httpTransactionFormat.request.url;
            string uriStr = string.Format(@"{0}://", urlFormat.protocol);
            uriStr += string.Join(".", urlFormat.host) + "/";
            if (urlFormat.dynamicPath != null) {
                urlFormat.dynamicPath.ForEach(x => x.SetValue(null, _reqParams));
                uriStr += string.Join("/", urlFormat.dynamicPath.Select(x => x.value)) + "?";
            } else {
                uriStr += string.Join("/", urlFormat.path) + "?";
            }

            if (urlFormat.query != null) {
                foreach (var qkvp in urlFormat.query) {
                    string queryVal = qkvp.value;
                    if (qkvp.isEnumId) {
                        queryVal = GetParamValueStr(qkvp.value);
                        if (string.IsNullOrEmpty(queryVal) && qkvp.omitIfNullOrEmpty) {
                            continue;
                        }
                    }
                    uriStr += string.Format(@"{0}={1}&", qkvp.key, queryVal);
                }
            }
            uriStr = uriStr.Substring(0, uriStr.Length - 1);
            if (!Uri.IsWellFormedUriString(uriStr, UriKind.Absolute)) {
                Console.WriteLine("Uri string is not properly defined: " + uriStr);
                return null;
            }
            Console.WriteLine("UriStr: " + uriStr);
            return new Uri(uriStr);
        }

        private HttpContent CreateRequestContent() {
            // TODO may need to add property to discern between different types of HttpContent here
            string mediaType = _httpTransactionFormat.request.body.mediaType;

            Encoding reqEncoding = Encoding.UTF8;
            if (_httpTransactionFormat.request.body.encoding.ToUpper() == "UTF8") {
                reqEncoding = Encoding.UTF8;
            }
            Console.WriteLine("Content-Encoding: " + reqEncoding.ToString());
            Console.WriteLine("Media-Type", mediaType);
            string body = CreatRequestBody();
            if (mediaType.ToLower() == "application/json") {
                var sc = new StringContent(body, reqEncoding, mediaType);
                sc.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                return sc;
            } else if (mediaType.ToLower() == "application/octet-stream") {
                var bac = new ByteArrayContent(Convert.FromBase64String(body));
                bac.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                return bac;
            } else {
                throw new Exception("Currently unsupported mediaType");
            }

        }

        private string CreatRequestBody() {
            string raw = _httpTransactionFormat.request.body.raw;
            if (!string.IsNullOrEmpty(raw) &&
               _httpTransactionFormat.request.body.mode.ToLower() == "parameterized") {
                Regex paramReg = new Regex(_paramRefRegEx, RegexOptions.Compiled | RegexOptions.Multiline);
                MatchCollection mc = paramReg.Matches(raw);

                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            string paramVal = GetParamValueStr(c.Value);
                            string escapedParamVal = HttpUtility.JavaScriptStringEncode(paramVal);

                            raw = raw.Replace(c.Value, escapedParamVal);
                            // JsonConvert.SerializeObject(raw.Replace(c.Value, paramEnum.value));
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
            Console.WriteLine("Request Body: " + raw);
            return raw;
        }

        private string GetParamValueStr(string queryParamValueStr) {
            string paramIdStr = GetParamIdStr(queryParamValueStr);
            var enumParam = _reqParams.FirstOrDefault(x => x.paramId.Equals(paramIdStr));
            if (enumParam == null) {
                Console.WriteLine($"Error parsing dynamic query item, enumId: '{paramIdStr}' does not exist");
                Console.WriteLine($"In request with params: ");
                Console.WriteLine(JsonConvert.SerializeObject(_reqParams));
                return null;
            }
            return enumParam.value;
        }

        private string GetParamIdStr(string queryParamValueStr) {
            if (string.IsNullOrEmpty(queryParamValueStr)) {
                throw new Exception("Error creating http uri, dynamic query item has undefined value");
            }
            if (!queryParamValueStr.StartsWith("@")) {
                throw new Exception("Parameterized values must start with '@'");
            }
            try {
                return queryParamValueStr.Substring(1, queryParamValueStr.Length - 1);
            }
            catch (Exception ex) {
                throw new Exception("Error converting param reference: " + queryParamValueStr + " " + ex);
            }
        }

        #endregion

        #region Response
        private MpAnalyzerPluginResponseFormat CreateResponse(string responseStr) {
            var response_format = _httpTransactionFormat.response;

            if (responseStr.StartsWith("[")) {
                JArray a = JArray.Parse(responseStr);
                _rootResponseToken = a.Children<JObject>().First();
            } else {
                _rootResponseToken = JObject.Parse(responseStr);
            }

            response_format.dataObjectLookup = CreateDataObject(_httpTransactionFormat.response.dataObjectLookup, _rootResponseToken.DeepClone());

            //response_format.newContentItem = CreateNewContent(_httpTransactionFormat.response.newContentItem, _rootResponseToken.DeepClone());
            //response_format.annotations = CreateAnnotations(_httpTransactionFormat.response.annotations, _rootResponseToken.DeepClone());
            return response_format;
        }

        private Dictionary<string, object> CreateDataObject(Dictionary<string, object> data_object_format, JToken curToken) {
            if (data_object_format == null) {
                return null;
            }

            // dataObjectLookup format should have a json path as its value to be used as query for result
            // when format value empty return whole result
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var do_format in data_object_format) {
                JToken query_token = curToken.SelectToken(do_format.Value.ToStringOrDefault());
                string do_value = string.Empty;
                if (query_token == null) {
                    // no path return whole response, mainly for testing/creating path later
                    do_value = curToken.ToString();
                } else {
                    do_value = query_token.ToStringOrDefault();
                }
                result.Add(do_format.Key, do_value);
            }
            return result;
        }
        //private MpPluginResponseNewContentFormat CreateNewContent(MpPluginResponseNewContentFormat prncf, JToken curToken) {
        //    if (prncf == null) {
        //        return null;
        //    }

        //    prncf = CreateElement(prncf, curToken, 0) as MpPluginResponseNewContentFormat;
        //    if (prncf != null) {
        //        if (prncf.content != null) {
        //            prncf.content.SetValue(curToken, _reqParams);
        //        }
        //        prncf.annotations = CreateAnnotations(prncf.annotations, curToken, 0);
        //    }
        //    return prncf;
        //}

        //private List<MpPluginResponseAnnotationFormat> CreateAnnotations(
        //    List<MpPluginResponseAnnotationFormat> al, JToken curToken, int idx = 0) {
        //    if (al == null) {
        //        return null;
        //    }

        //    for (int i = 0; i < al.Count; i++) {
        //        var a = CreateAnnotation(al[i], curToken, i);
        //        if (a != null) {
        //            al[i] = a;
        //        }
        //    }
        //    return al;
        //}

        //private MpPluginResponseAnnotationFormat CreateAnnotation(MpPluginResponseAnnotationFormat a, JToken curToken, int idx = 0) {
        //    a = CreateElement(a, curToken, idx) as MpPluginResponseAnnotationFormat;
        //    if (a != null) {
        //        try {
        //            if (a.box != null) {
        //                a.box.x.SetValue(curToken, _reqParams, idx);
        //                a.box.y.SetValue(curToken, _reqParams, idx);
        //                a.box.width.SetValue(curToken, _reqParams, idx);
        //                a.box.height.SetValue(curToken, _reqParams, idx);
        //            }

        //        }
        //        catch (MpJsonPathPropertyException jppex) {
        //            Console.WriteLine(jppex);
        //            return null;
        //        }
        //    }
        //    return a;
        //}

        //private MpPluginResponseItemBaseFormat CreateElement(
        //    MpPluginResponseItemBaseFormat a, JToken curToken, int idx = 0) {
        //    if (a != null) {
        //        try {
        //            //if (a.queryPath != null) {
        //            //    if(a.queryPath.pathType == MpJsonPathType.Absolute) {
        //            //        curToken = _rootResponseToken.DeepClone();
        //            //    }
        //            //    curToken = curToken.SelectToken(a.queryPath.pathExpression);
        //            //}
        //            if (a.label != null) {
        //                a.label.SetValue(curToken, _reqParams, idx);
        //                a.label = a.label.omitIfPathNotFound && a.label.value == null ? null : a.label;
        //            }
        //            if (a.score != null) {
        //                a.score.SetValue(curToken, _reqParams, idx);
        //                a.score = a.score.omitIfPathNotFound && a.score.value == default ? null : a.score;
        //            }
        //            a.children = CreateAnnotations(a.children, curToken, 0);

        //            if (a.dynamicChildren != null && a.dynamicChildren.Count > 0) {
        //                for (int i = 0; i < a.dynamicChildren.Count; i++) {
        //                    int curDynamicChildIdx = 0;
        //                    while (true) {
        //                        var newChild = JsonConvert.DeserializeObject<MpPluginResponseAnnotationFormat>(
        //                                    JsonConvert.SerializeObject(a.dynamicChildren[i]));
        //                        newChild = CreateAnnotation(newChild, curToken, curDynamicChildIdx);
        //                        if (newChild == null) {
        //                            break;
        //                        }
        //                        if (a.children == null) {
        //                            a.children = new List<MpPluginResponseAnnotationFormat>();
        //                        }
        //                        a.children.Add(newChild);
        //                        curDynamicChildIdx++;
        //                    }
        //                }
        //            }
        //        }
        //        catch (MpJsonPathPropertyException jppex) {
        //            Console.WriteLine(jppex);
        //            return null;
        //        }
        //    }
        //    return a;
        //}

        #endregion

        #endregion
    }
}
