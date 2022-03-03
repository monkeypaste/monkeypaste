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
        private bool _showDebug = true;

        private string _testResponse = @"{
	""categories"": [],
	""adult"": {
		""isAdultContent"": false,
		""isRacyContent"": false,
		""isGoryContent"": false,
		""adultScore"": 0.004809280391782522,
		""racyScore"": 0.007183143403381109,
		""goreScore"": 0.1027374342083931
	},
	""color"": {
		""dominantColorForeground"": ""Brown"",
		""dominantColorBackground"": ""Brown"",
		""dominantColors"": [
			""Brown""
		],
		""accentColor"": ""684528"",
		""isBwImg"": false,
		""isBWImg"": false
	},
	""tags"": [
		{
			""name"": ""animal"",
			""confidence"": 0.9789923429489136
		},
		{
			""name"": ""grass"",
			""confidence"": 0.9699687957763672
		},
		{
			""name"": ""cattle"",
			""confidence"": 0.9672636389732361
		},
		{
			""name"": ""cow"",
			""confidence"": 0.8340009450912476
		},
		{
			""name"": ""hay"",
			""confidence"": 0.8253458738327026
		},
		{
			""name"": ""livestock"",
			""confidence"": 0.7987529039382935
		},
		{
			""name"": ""goat"",
			""confidence"": 0.7143263816833496
		},
		{
			""name"": ""bull"",
			""confidence"": 0.6668627262115479
		},
		{
			""name"": ""group"",
			""confidence"": 0.6275402903556824
		},
		{
			""name"": ""mammal"",
			""confidence"": 0.6023078560829163,
			""hint"": ""animal""
		},
		{
			""name"": ""sheep"",
			""confidence"": 0.54606693983078
		},
		{
			""name"": ""bovine"",
			""confidence"": 0.5429664850234985
		},
		{
			""name"": ""horse"",
			""confidence"": 0.5125572085380554
		},
		{
			""name"": ""herd"",
			""confidence"": 0.5079358816146851
		},
		{
			""name"": ""dry"",
			""confidence"": 0.3997076749801636
		},
		{
			""name"": ""several"",
			""confidence"": 0.1021641194820404
		}
	],
	""description"": {
		""tags"": [
			""grass"",
			""hay"",
			""group"",
			""mammal"",
			""dry"",
			""several""
		],
		""captions"": [
			{
				""text"": ""a group of animals in a barn"",
				""confidence"": 0.5060526728630066
			}
		]
	},
	""faces"": [],
	""objects"": [
		{
			""rectangle"": {
				""x"": 209,
				""y"": 179,
				""w"": 306,
				""h"": 210
			},
			""object"": ""mammal"",
			""confidence"": 0.66,
			""parent"": {
				""object"": ""animal"",
				""confidence"": 0.66
			}
		},
		{
			""rectangle"": {
				""x"": 95,
				""y"": 346,
				""w"": 183,
				""h"": 169
			},
			""object"": ""merino sheep"",
			""confidence"": 0.557,
			""parent"": {
				""object"": ""sheep"",
				""confidence"": 0.6,
				""parent"": {
					""object"": ""mammal"",
					""confidence"": 0.764,
					""parent"": {
						""object"": ""animal"",
						""confidence"": 0.765
					}
				}
			}
		},
		{
			""rectangle"": {
				""x"": 212,
				""y"": 358,
				""w"": 142,
				""h"": 217
			},
			""object"": ""merino sheep"",
			""confidence"": 0.618,
			""parent"": {
				""object"": ""sheep"",
				""confidence"": 0.644,
				""parent"": {
					""object"": ""mammal"",
					""confidence"": 0.795,
					""parent"": {
						""object"": ""animal"",
						""confidence"": 0.795
					}
				}
			}
		},
		{
			""rectangle"": {
				""x"": 347,
				""y"": 375,
				""w"": 89,
				""h"": 230
			},
			""object"": ""merino sheep"",
			""confidence"": 0.709,
			""parent"": {
				""object"": ""sheep"",
				""confidence"": 0.728,
				""parent"": {
					""object"": ""mammal"",
					""confidence"": 0.902,
					""parent"": {
						""object"": ""animal"",
						""confidence"": 0.902
					}
				}
			}
		},
		{
			""rectangle"": {
				""x"": 448,
				""y"": 303,
				""w"": 158,
				""h"": 294
			},
			""object"": ""mammal"",
			""confidence"": 0.861,
			""parent"": {
				""object"": ""animal"",
				""confidence"": 0.861
			}
		},
		{
			""rectangle"": {
				""x"": 192,
				""y"": 470,
				""w"": 134,
				""h"": 135
			},
			""object"": ""merino sheep"",
			""confidence"": 0.543,
			""parent"": {
				""object"": ""sheep"",
				""confidence"": 0.595,
				""parent"": {
					""object"": ""mammal"",
					""confidence"": 0.767,
					""parent"": {
						""object"": ""animal"",
						""confidence"": 0.772
					}
				}
			}
		}
	],
	""brands"": [],
	""requestId"": ""c4f7d12a-89f1-46ec-81f7-8f6bebd664e3"",
	""metadata"": {
		""height"": 721,
		""width"": 721,
		""format"": ""Bmp""
	}
}";
        private string _testResponse2 = @"{
	""categories"": [
		{
			""name"": ""people_group"",
			""score"": 0.80859375,
			""detail"": {
				""celebrities"": [
					{
						""name"": ""Tim Robbins"",
						""confidence"": 0.9999910593032837,
						""faceRectangle"": {
							""left"": 460,
							""top"": 47,
							""width"": 50,
							""height"": 50
						}
					},
					{
						""name"": ""Charlize Theron"",
						""confidence"": 0.9808109402656555,
						""faceRectangle"": {
							""left"": 97,
							""top"": 92,
							""width"": 49,
							""height"": 49
						}
					},
					{
						""name"": ""Sean Penn"",
						""confidence"": 0.9961344003677368,
						""faceRectangle"": {
							""left"": 206,
							""top"": 118,
							""width"": 48,
							""height"": 48
						}
					}
				]
			}
		}
	],
	""color"": {
		""dominantColorForeground"": ""Black"",
		""dominantColorBackground"": ""White"",
		""dominantColors"": [
			""White"",
			""Black""
		],
		""accentColor"": ""181D23"",
		""isBwImg"": false,
		""isBWImg"": false
	},
	""tags"": [
		{
			""name"": ""person"",
			""confidence"": 0.9981362819671631
		},
		{
			""name"": ""wine"",
			""confidence"": 0.9887799620628357
		},
		{
			""name"": ""indoor"",
			""confidence"": 0.9738012552261353
		},
		{
			""name"": ""wedding dress"",
			""confidence"": 0.9619781970977783
		},
		{
			""name"": ""bride"",
			""confidence"": 0.9412243366241455
		},
		{
			""name"": ""dress"",
			""confidence"": 0.862946093082428
		},
		{
			""name"": ""curtain"",
			""confidence"": 0.855199933052063
		},
		{
			""name"": ""standing"",
			""confidence"": 0.8317896723747253
		},
		{
			""name"": ""suit"",
			""confidence"": 0.7997269034385681
		},
		{
			""name"": ""posing"",
			""confidence"": 0.7365117073059082
		},
		{
			""name"": ""wedding"",
			""confidence"": 0.6982574462890625
		},
		{
			""name"": ""people"",
			""confidence"": 0.6689982414245606
		},
		{
			""name"": ""couple"",
			""confidence"": 0.666380763053894
		},
		{
			""name"": ""smile"",
			""confidence"": 0.6111418008804321
		},
		{
			""name"": ""fashion accessory"",
			""confidence"": 0.50831139087677
		},
		{
			""name"": ""dressed"",
			""confidence"": 0.4675471782684326
		}
	],
	""description"": {
		""tags"": [
			""person"",
			""wine"",
			""indoor"",
			""curtain"",
			""standing"",
			""posing"",
			""people"",
			""couple"",
			""dressed""
		],
		""captions"": [
			{
				""text"": ""Tim Robbins, Charlize Theron, Sean Penn et al. holding trophies"",
				""confidence"": 0.6147781014442444
			}
		]
	},
	""faces"": [
		{
			""age"": 44,
			""gender"": ""Male"",
			""faceRectangle"": {
				""left"": 460,
				""top"": 47,
				""width"": 50,
				""height"": 50
			}
		},
		{
			""age"": 44,
			""gender"": ""Female"",
			""faceRectangle"": {
				""left"": 97,
				""top"": 92,
				""width"": 49,
				""height"": 49
			}
		},
		{
			""age"": 47,
			""gender"": ""Male"",
			""faceRectangle"": {
				""left"": 206,
				""top"": 118,
				""width"": 48,
				""height"": 48
			}
		},
		{
			""age"": 32,
			""gender"": ""Female"",
			""faceRectangle"": {
				""left"": 375,
				""top"": 134,
				""width"": 43,
				""height"": 43
			}
		}
	],
	""objects"": [
		{
			""rectangle"": {
				""x"": 312,
				""y"": 100,
				""w"": 137,
				""h"": 256
			},
			""object"": ""person"",
			""confidence"": 0.903
		},
		{
			""rectangle"": {
				""x"": 414,
				""y"": 13,
				""w"": 190,
				""h"": 347
			},
			""object"": ""person"",
			""confidence"": 0.891
		},
		{
			""rectangle"": {
				""x"": 8,
				""y"": 63,
				""w"": 176,
				""h"": 292
			},
			""object"": ""person"",
			""confidence"": 0.898
		},
		{
			""rectangle"": {
				""x"": 162,
				""y"": 77,
				""w"": 161,
				""h"": 282
			},
			""object"": ""person"",
			""confidence"": 0.892
		}
	],
	""requestId"": ""6de2d2c8-9456-4d22-a04d-218969014d90"",
	""metadata"": {
		""height"": 360,
		""width"": 640,
		""format"": ""Bmp""
	}
}";
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
            string unalteredHttpFormat = JsonConvert.SerializeObject(_httpTransactionFormat);

            reqParams = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());
            if(reqParams == null) {
                Console.WriteLine($"Warning! Empty or malformed request arguments for plugin: '{_httpTransactionFormat.name}'");
                Console.WriteLine($"With args: {args}");
                reqParams = new List<MpAnalyzerPluginRequestItemFormat>();
            }

            using(var client = new HttpClient()) {
                using (var request = CreateRequestMessage(args)) {
                    try {
                        var response = await client.SendAsync(request);

                        if (!response.IsSuccessStatusCode) {
                            var userAction = await MpNotificationBalloonViewModel.Instance.ShowUserAction(
                                                    notificationType: MpNotificationDialogType.BadHttpRequest,
                                                    exceptionType: MpNotificationExceptionSeverityType.WarningWithOption,
                                                    msg: $"{response.ReasonPhrase}");
                            if(userAction == MpDialogResultType.Retry) {
                                return new MpPluginResponseFormat() {
                                    message = MpPluginResponseFormat.RETRY_MESSAGE
                                };
                            }
                        }

                        string responseStr = await response.Content.ReadAsStringAsync();

                        // string responseStr = _testResponse2;

                        Console.WriteLine($"Response from '{request.RequestUri.AbsoluteUri}':");
                        Console.WriteLine(responseStr.ToPrettyPrintJson());

                        request.Content.Dispose();
                        var responseObj = CreateResponse(responseStr);

                        //reset format or subsequent requests compound data 
                        _httpTransactionFormat = JsonConvert.DeserializeObject<MpHttpTransactionFormat>(unalteredHttpFormat);
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

        public string GetRequestUri(object args) {
            var reqMsg = CreateRequestMessage(args);
            if(reqMsg == null) {
                return null;
            }
            return reqMsg.RequestUri.AbsoluteUri;
        }

        #endregion

        #region Private Methods

        private HttpRequestMessage CreateRequestMessage(object args) {
            reqParams = JsonConvert.DeserializeObject<List<MpAnalyzerPluginRequestItemFormat>>(args.ToString());
            if (reqParams == null) {
                Console.WriteLine($"Warning! Empty or malformed request arguments for plugin: '{_httpTransactionFormat.name}'");
                Console.WriteLine($"With args: {args}");
                reqParams = new List<MpAnalyzerPluginRequestItemFormat>();
            }

            using (var client = new HttpClient()) {
                using (var request = new HttpRequestMessage()) {
                    request.Method = RequestMethod;
                    CreateHeaders(request);
                    request.RequestUri = CreateRequestUri();
                    request.Content = CreateRequestContent();
                    return request;
                }
            }
        }

        private void CreateHeaders(HttpRequestMessage request) {
            if (_httpTransactionFormat != null &&
                _httpTransactionFormat.request != null &&
                _httpTransactionFormat.request.header != null) {
                foreach (var kvp in _httpTransactionFormat.request.header) {
                    if (kvp.type == "guid") {
                        request.Headers.Add(kvp.key, System.Guid.NewGuid().ToString());
                    } else if (kvp.valuePath != null) {
                        kvp.valuePath.SetValue(null, reqParams, 0);
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
            if(urlFormat.dynamicPath != null) {
                urlFormat.dynamicPath.ForEach(x => x.SetValue(null, reqParams));
                uriStr += string.Join("/", urlFormat.dynamicPath.Select(x => x.value)) + "?";
            } else {
                uriStr += string.Join("/", urlFormat.path) + "?";
            }

            if(urlFormat.query != null) {
                foreach (var qkvp in urlFormat.query) {
                    string queryVal = qkvp.value;
                    if (qkvp.isEnumId) {
                        queryVal = GetParamValue(qkvp.value);
                        if (string.IsNullOrEmpty(queryVal) && qkvp.omitIfNullOrEmpty) {
                            continue;
                        }
                    }
                    uriStr += string.Format(@"{0}={1}&", qkvp.key, queryVal);
                }
            }
            uriStr = uriStr.Substring(0, uriStr.Length - 1);
            if(!Uri.IsWellFormedUriString(uriStr,UriKind.Absolute)) {
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
            if(_httpTransactionFormat.request.body.encoding.ToUpper() == "UTF8") {
                reqEncoding = Encoding.UTF8;
            }
            Console.WriteLine("Content-Encoding: " + reqEncoding.ToString());
            Console.WriteLine("Media-Type", mediaType);
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
            Console.WriteLine("Request Body: " + raw);
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

            response.newContentItem = CreateNewContent(_httpTransactionFormat.response.newContentItem, jo);
            response.annotations = CreateAnnotations(_httpTransactionFormat.response.annotations,jo);
            return response;
        }

        private MpPluginResponseNewContentFormat CreateNewContent(MpPluginResponseNewContentFormat prncf, JObject jo) {
            if(prncf == null) {
                return null;
            }

            prncf = CreateElement(prncf, jo, 0) as MpPluginResponseNewContentFormat;
            if (prncf.content != null) {
                prncf.content.SetValue(jo, reqParams);
            }
            prncf.annotations = CreateAnnotations(prncf.annotations, jo, 0);
            return prncf;
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
            a = CreateElement(a, jo, idx) as MpPluginResponseAnnotationFormat;
            try {
                if (a.box != null) {
                    a.box.x.SetValue(jo, reqParams, idx);
                    a.box.y.SetValue(jo, reqParams, idx);
                    a.box.width.SetValue(jo, reqParams, idx);
                    a.box.height.SetValue(jo, reqParams, idx);
                }
                
            }
            catch (MpJsonPathPropertyException jppex) {
                Console.WriteLine(jppex);
                return null;
            }
            return a;
        }

        private MpPluginResponseItemBaseFormat CreateElement(MpPluginResponseItemBaseFormat a,JObject jo, int idx = 0) {
            try {
                if (a.label != null) {
                    a.label.SetValue(jo, reqParams, idx);
                    a.label = a.label.omitIfPathNotFound && a.label.value == null ? null : a.label;
                }
                if (a.score != null) {
                    a.score.SetValue(jo, reqParams, idx);
                    a.score = a.score.omitIfPathNotFound && a.score.value == null ? null : a.score;
                }
                a.children = CreateAnnotations(a.children, jo, 0);

                if (a.dynamicChildren != null && a.dynamicChildren.Count > 0) {
                    for (int i = 0; i < a.dynamicChildren.Count; i++) {
                        int curDynamicChildIdx = 0;
                        while (true) {
                            var newChild = JsonConvert.DeserializeObject<MpPluginResponseAnnotationFormat>(
                                        JsonConvert.SerializeObject(a.dynamicChildren[i]));
                            newChild = CreateAnnotation(newChild, jo, curDynamicChildIdx);
                            if (newChild == null) {
                                break;
                            }
                            if (a.children == null) {
                                a.children = new List<MpPluginResponseAnnotationFormat>();
                            }
                            a.children.Add(newChild);
                            curDynamicChildIdx++;
                        }
                    }
                }
            }
            catch (MpJsonPathPropertyException jppex) {
                Console.WriteLine(jppex);
                return null;
            }
            return a;
        }

        #endregion
    }
}
