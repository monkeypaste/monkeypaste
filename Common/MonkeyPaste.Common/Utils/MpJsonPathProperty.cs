﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonkeyPaste.Common {
    public enum MpJsonDataTokenType {
        None = 0,
        RequestParam, // @<enumId>
        Subsititution, // value: '...{0}...' where valuePath={0}
        PathIndex, // [#] where # is swapped for * and Property is list
        Eof, // <EOF> used to denoted contentEnd for text range annotation
    }

    public enum MpJsonPathType {
        Relative,
        Absolute
    }

    public class MpJsonPathProperty {
        private static readonly string _inputParamRegexStr = @"@[0-9]*";
        private static Regex _inputParamRegex;

        public string value { get; set; } = string.Empty;
        public string pathExpression { get; set; } = string.Empty;

        public MpJsonPathType pathType { get; set; } = MpJsonPathType.Absolute;

        public bool omitIfPathNotFound { get; set; } = true;

        public static string Query(object jsonObj, string jsonQuery) {
            string jsonStr;
            if(jsonObj == null) {
                return null;
            }
            if(jsonObj is string) {
                jsonStr = jsonObj.ToString();
            } else {
                jsonStr = JsonConvert.SerializeObject(jsonObj);
            }
            JObject jo;
            if (jsonStr.StartsWith("[")) {
                JArray a = JArray.Parse(jsonStr);
                jo = a.Children<JObject>().First();
            } else {
                jo = JObject.Parse(jsonStr);
            }

            var matchValPath = new MpJsonPathProperty() {
                pathExpression = jsonQuery
            };
            matchValPath.SetValue(jo, null);

            return matchValPath.value;
        }

        public MpJsonPathProperty() {
            if (_inputParamRegex == null) {
                _inputParamRegex = new Regex(_inputParamRegexStr, RegexOptions.Compiled | RegexOptions.Multiline);
            }
        }

        public MpJsonPathProperty(string value) : this() {
            this.value = value;
        }

        public MpJsonPathProperty(string value,string valuePath) : this(value) {
            this.pathExpression = valuePath;
        }

        public virtual void SetValue(string text) {
            value = text;
        }
        public virtual void SetValue(JToken curToken, List<MpAnalyzerPluginRequestItemFormat> reqParams, int idx = 0) {
            value = FindValuePathResult(curToken, reqParams, idx);
        }

        protected string FindValuePathResult(JToken curToken, List<MpAnalyzerPluginRequestItemFormat> reqParams, int idx = 0) {
            string result = string.Empty;
            if (pathExpression.StartsWith("@")) {
                result = GetParamValue(pathExpression, reqParams);
            } else if(!string.IsNullOrEmpty(pathExpression)) {
                try {
                    if(pathExpression.Contains("[#]")) {
                        string jsonPathValue = pathExpression.Replace("[#]", "[*]");
                        var dataTokens = curToken.SelectTokens(jsonPathValue, false).ToList();
                        if(dataTokens == null || dataTokens.Count == 0) {
                            throw new MpJsonPathPropertyException($"valuePath '{pathExpression}' not found");
                        }
                        if (idx >= dataTokens.Count) {
                            throw new MpJsonPathPropertyException($"Exceeded query count of {dataTokens.Count()} for idx {idx}");
                        }
                        result = dataTokens[idx].ToString();
                    } else {
                        JToken dataToken = curToken.SelectToken(pathExpression, false);
                        if(dataToken == null) {
                            throw new MpJsonPathPropertyException($"valuePath '{pathExpression}' not found");
                        }
                        result = dataToken.ToString();
                    }
                    
                }
                catch (Exception ex) {
                    if(ex is MpJsonPathPropertyException jppe) {
                        throw jppe;
                    }
                    MpConsole.WriteLine("Error parsing resposne: " + ex);
                }
            }
            if (string.IsNullOrWhiteSpace(result) && string.IsNullOrWhiteSpace(value)) {
                return omitIfPathNotFound ? null : string.Empty;
            }
            string outputValue = value;
            outputValue = string.IsNullOrWhiteSpace(outputValue) ? "{0}" : outputValue;

            if (!string.IsNullOrWhiteSpace(outputValue) && !string.IsNullOrWhiteSpace(result) && !outputValue.Contains("{0}")) {
                throw new Exception($"if path exists, value must be formatted to subsititue it (value: '{outputValue}' path: '{pathExpression}' pathResult: '{result}')");
            }
            outputValue = outputValue.Replace("{0}", result);
            MatchCollection mc = _inputParamRegex.Matches(outputValue);

            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        string paramVal = GetParamValue(c.Value, reqParams);

                        outputValue = outputValue.Replace(c.Value, paramVal);
                    }
                }
            }
            return outputValue;
        }

        private string GetParamValue(string queryParamValueStr, List<MpAnalyzerPluginRequestItemFormat> reqParams) {
            int enumId = GetParamId(queryParamValueStr);
            var enumParam = reqParams.FirstOrDefault(x => x.paramId == enumId);
            if (enumParam == null) {
                MpConsole.WriteLine($"Error parsing dynamic query item, enumId: '{enumId}' does not exist");
                MpConsole.WriteLine($"In request with params: ");
                MpConsole.WriteLine(JsonConvert.SerializeObject(reqParams));
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

        public override string ToString() {
            return value;
        }
    }

    public class MpJsonPathProperty<T> : MpJsonPathProperty where T : struct {

        public new T value { get; set; }

        public MpJsonPathProperty() : base() { }

        public MpJsonPathProperty(T value) : this() {
            this.value = value;
        }

        public void SetValue(T val) {
            value = val;
        }

        public override void SetValue(JToken curToken, List<MpAnalyzerPluginRequestItemFormat> reqParams, int idx = 0) {
            string result = base.FindValuePathResult(curToken, reqParams, idx);
            if (string.IsNullOrEmpty(result)) {
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
            }
            catch (Exception ex) {
                MpConsole.WriteLine($"Error converting '{result}' to type '{typeof(T)}'");
                MpConsole.WriteLine(ex);

                value = default;
            }
        }
    }
    public class MpJsonPathPropertyException : Exception {
        public MpJsonPathPropertyException(string msg) : base(msg) { }
        public MpJsonPathPropertyException(string msg, Exception innerException) : base(msg, innerException) { }
    }
}
