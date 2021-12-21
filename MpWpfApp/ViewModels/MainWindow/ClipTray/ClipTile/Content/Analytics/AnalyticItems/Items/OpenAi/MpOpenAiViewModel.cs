using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.Globalization;
using Xamarin.Forms.Internals;

namespace MpWpfApp {
    public enum MpOpenAiParamType {
        None = 0,
        Execute,
        Result,
        Engine,
        EndPoint,
        Temperature,
        MaxTokens,
        TopP,
        FreqPen,
        PresPen
    }

    public class MpOpenAiViewModel : MpAnalyticItemViewModel {
        #region Private Variables

        private MpOpenAiResponse _responseObj;

        #endregion

        #region Properties

        #region State

        #endregion

        #region Model

        #endregion

        #region Http

        public override MpHttpResponseBase ResponseObj => _responseObj;

        #endregion

        #endregion

        #region Constructors

        public MpOpenAiViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpOpenAiViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override async Task Initialize() {
            MpAnalyticItem oaai = await MpDataModelProvider.Instance.GetAnalyticItemByTitle("Open Ai");
            if (oaai == null) {
                //var h1 = await MpHttpHeaderItem.Create(
                //    "Authorization", 
                //    $"Bearer {MpPreferences.Instance.RestfulOpenAiApiKey}");

                //var req1 = await MpHttpRequest.Create(
                //    new List<MpHttpHeaderItem>() { h1 },
                //    "https://api.openai.com/v1/",
                //     MpPreferences.Instance.RestfulOpenAiApiKey);

                //oaai = await MpAnalyticItem.Create(
                //        new List<MpHttpRequest> { req1},
                //        MpInputFormatType.Text,
                //        "Open Ai",
                //        "OpenAI is an artificial intelligence research laboratory consisting of the for-profit corporation OpenAI LP and its parent company, the non-profit OpenAI Inc.");

                oaai = await MpAnalyticItem.Create(
                        "https://api.openai.com/v1/",
                        MpPreferences.Instance.RestfulOpenAiApiKey,
                        MpInputFormatType.Text,
                        "Open Ai",
                        "OpenAI is an artificial intelligence research laboratory consisting of the for-profit corporation OpenAI LP and its parent company, the non-profit OpenAI Inc.");
            } else {
                //oaai = await MpDb.Instance.GetItemAsync<MpAnalyticItem>(oaai.Id);
            }

            await InitializeDefaultsAsync(oaai);
        }

        public override async Task LoadChildren() {
            IsBusy = true;

            var aipl = new List<MpAnalyticItemParameter>() {
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.ComboBox,
                    ValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                    Label = "Engine",
                    IsParameterRequired = true,
                    SortOrderIdx = 0,
                    EnumId = (int)MpOpenAiParamType.Engine,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            IsDefault = true,
                            Label = "DaVinci",
                            Value = "davinci"
                        },
                        new MpAnalyticItemParameterValue() {
                            Label = "Curie",
                            Value = "curie"
                        },
                        new MpAnalyticItemParameterValue() {
                            Label = "Babbage",
                            Value = "babbage"
                        },
                        new MpAnalyticItemParameterValue() {
                            Label = "Ada",
                            Value = "ada"
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.ComboBox,
                    Label = "End Point",
                    IsParameterRequired = true,
                    SortOrderIdx = 1,
                    EnumId = (int)MpOpenAiParamType.EndPoint,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            Label = "Completions",
                            Value =  "completions"
                        },
                        new MpAnalyticItemParameterValue() {
                            Label = "Searches",
                            Value =  "searches"
                        },
                        new MpAnalyticItemParameterValue() {
                            IsDefault = true,
                            Label = "Classifications",
                            Value =  "classifications"
                        },
                        new MpAnalyticItemParameterValue() {
                            Label = "Answers",
                            Value =  "answers"
                        },
                        new MpAnalyticItemParameterValue() {
                            Label = "Files",
                            Value =  "files"
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.Slider,
                    ValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                    Label = "Temperature",
                    IsParameterRequired = true,
                    SortOrderIdx = 2,
                    Description = "Controls randomness: Lowering results in less random completions. As the temperature approaches zero, the model will become deterministic and repetitive.",
                    EnumId = (int)MpOpenAiParamType.Temperature,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {                            
                            Value = "0",
                            IsMinimum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "1",
                            IsMaximum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "0.75",
                            IsDefault = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.Slider,
                    ValueType = MpAnalyticItemParameterValueUnitType.Integer,
                    Label = "Max Tokens",
                    IsParameterRequired = true,
                    SortOrderIdx = 3,
                    EnumId = (int)MpOpenAiParamType.MaxTokens,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            Value = "1",
                            IsMinimum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "2048",
                            IsMaximum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "64",
                            IsDefault = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.Slider,
                    ValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                    Label = "Top P",
                    IsParameterRequired = true,
                    SortOrderIdx = 4,
                    EnumId = (int)MpOpenAiParamType.TopP,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            Value = "0",
                            IsMinimum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "1",
                            IsMaximum = true,
                            IsDefault = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.Slider,
                            ValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                    Label = "Frequency Penalty",
                    IsParameterRequired = true,
                    SortOrderIdx = 5,
                    EnumId = (int)MpOpenAiParamType.FreqPen,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            Value = "0",
                            IsMinimum = true,
                            IsDefault = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "2",
                            IsMaximum = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.Slider,
                            ValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                    Label = "Presence Penalty",
                    IsParameterRequired = true,
                    SortOrderIdx = 6,
                    EnumId = (int)MpOpenAiParamType.PresPen,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            Value = "0",
                            IsMinimum = true,
                            IsDefault = true
                        },
                        new MpAnalyticItemParameterValue() {
                            Value = "2",
                            IsMaximum = true
                        }
                    }
                }
            };
            AnalyticItem.Parameters = aipl;

            await base.LoadChildren();
            IsBusy = false;
        }
        #endregion

        #region Protected Methods

        protected override async Task ExecuteAnalysis(object obj) {
            IsBusy = true;

            string engine = GetParam((int)MpOpenAiParamType.Engine).CurrentValue.ToLower();
            string ep = GetParam((int)MpOpenAiParamType.EndPoint).CurrentValue.ToLower();

            string endpoint = string.Format(
                @"https://api.openai.com/v1/engines/{0}/{1}",
                engine,
                ep);


            MpOpenAiRequest jsonReq = new MpOpenAiRequest() {
                Prompt = Regex.Escape(obj.ToString()),
                Temperature = GetParam((int)MpOpenAiParamType.Temperature).DoubleValue,
                MaxTokens = GetParam((int)MpOpenAiParamType.MaxTokens).IntValue,
                TopP = GetParam((int)MpOpenAiParamType.TopP).DoubleValue,
                FrequencyPenalty = GetParam((int)MpOpenAiParamType.FreqPen).DoubleValue,
                PresencePenalty = GetParam((int)MpOpenAiParamType.PresPen).DoubleValue
            };

            string jsonReqStr = JsonConvert.SerializeObject(jsonReq);
            string jsonRespStr = await MpOpenAi.Instance.Request(
                endpoint,
                jsonReqStr);

            MpOpenAiResponse jsonResp = JsonConvert.DeserializeObject<MpOpenAiResponse>(jsonRespStr);

            if (jsonResp != null && jsonResp.choices != null && jsonResp.choices.Count > 0) {
               string resultData = Regex.Unescape(jsonResp.choices[0].text);
                await ResultViewModel.ConvertToCopyItem(jsonReqStr, resultData);
            } else {
                Debugger.Break();
            }

            IsBusy = false;
        }
        #endregion

        #region Private Methods
        private void MpOpenAiViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }
        #endregion
    }
}
