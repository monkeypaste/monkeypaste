using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Plugin {
    public class MpJsonPathPropertyException : Exception {
        public MpJsonPathPropertyException(string msg) : base(msg) { }
        public MpJsonPathPropertyException(string msg, Exception innerException) : base(msg,innerException) { }
    }
    public class MpJsonPathProperty<T> : MpJsonPathProperty where T : struct {
        public new T value { get; private set; }

        public MpJsonPathProperty() { }

        public MpJsonPathProperty(T value)  {
            this.value = value;
        }

        public new void SetValue(JObject jo, List<MpAnalyzerPluginRequestItemFormat> reqParams, int idx = 0) {
            string result = base.FindValue(jo, reqParams, idx);
            if(string.IsNullOrEmpty(result)) {
                value = default;
                return;
            }
            try {
                if (typeof(T) == typeof(double)) {
                    value = (T)(object)Convert.ToDouble(result);
                } else if (typeof(T) == typeof(bool)) {
                    value = (T)(object)(result == "1" || result.ToLower() == "true");
                } else if (typeof(T) == typeof(int)) {
                    value = (T)(object)Convert.ToInt32(result);
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error converting '{result}' to type '{typeof(T)}'");
                Console.WriteLine(ex);

                value = default;
            }
        }
    }
    public class MpJsonPathProperty {
        public int thisIndex { get; set; } = -1;

        public string value { get; private set; }
        public string valuePath { get; set; }

        public bool omitIfPathNotFound { get; set; } = true;

        public MpJsonPathProperty() { }

        public MpJsonPathProperty(string value) {
            this.value = value;
        }

        public void SetValue(string text) {
            value = text;
        }
        public void SetValue(JObject jo, List<MpAnalyzerPluginRequestItemFormat> reqParams, int idx = 0) {
            value = FindValue(jo, reqParams, idx);
        }

        protected string FindValue(JObject jo, List<MpAnalyzerPluginRequestItemFormat> reqParams, int idx = 0) {
            string result = string.Empty;
            if (valuePath.StartsWith("@")) {
                result = GetParamValue(valuePath, reqParams);
            } else {
                try {
                    if(valuePath.Contains("[#]")) {
                        string jsonPathValue = valuePath.Replace("[#]", "[*]");
                        var dataTokens = jo.SelectTokens(jsonPathValue, false).ToList();
                        if (dataTokens == null || dataTokens.Count == 0) {
                            if (!omitIfPathNotFound) {
                                result = valuePath;
                            }
                        } else {
                            if (idx >= dataTokens.Count) {
                                throw new MpJsonPathPropertyException($"Exceeded query count of {dataTokens.Count()} for idx {idx}");
                            }
                            result = dataTokens[idx].ToString();
                        }
                    } else {
                        JToken dataToken = jo.SelectToken(valuePath, false);
                        if (dataToken == null) {
                            if (!omitIfPathNotFound) {
                                result = valuePath;
                            }
                        } else {
                            result = dataToken.ToString();
                        }
                    }
                    
                }
                catch (Exception ex) {
                    if(ex is MpJsonPathPropertyException jppe) {
                        throw jppe;
                    }
                    Console.WriteLine("Error parsing resposne: " + ex);
                    if (!omitIfPathNotFound) {
                        result = valuePath;
                    }
                }
            }

            return result;
        }

        private string GetParamValue(string queryParamValueStr, List<MpAnalyzerPluginRequestItemFormat> reqParams) {
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
            if (string.IsNullOrEmpty(queryParamValueStr)) {
                throw new Exception("Error creating http uri, dynamic query item has undefined value");
            }
            if (!queryParamValueStr.StartsWith("@")) {
                throw new Exception("Parameterized values must start with '@'");
            }
            try {
                return Convert.ToInt32(queryParamValueStr.Substring(1, queryParamValueStr.Length - 1));
            }
            catch (Exception ex) {
                throw new Exception("Error converting param reference: " + queryParamValueStr + " " + ex);
            }
        }
    }
}
