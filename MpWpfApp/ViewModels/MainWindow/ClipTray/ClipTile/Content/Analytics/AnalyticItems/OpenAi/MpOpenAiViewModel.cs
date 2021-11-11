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
        #region Properties

        #region State

        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpOpenAiViewModel(MpAnalyticItemCollectionViewModel parent, int aiid) : base(parent) {
            PropertyChanged += MpOpenAiViewModel_PropertyChanged;
            RuntimeId = aiid;
        }

        #endregion

        #region Public Methods

        public override async Task Initialize() {
            ItemIconSourcePath = Application.Current.Resources["BrainIcon"] as string;

            var openAiModel = new MpAnalyticItem() {
                Id = RuntimeId,
                AnalyticItemGuid = Guid.NewGuid(),
                EndPoint = "https://api.openai.com/v1/",
                ApiKey = MpPreferences.Instance.RestfulOpenAiApiKey,
                Title = "Open Ai",
                Description = "OpenAI is an artificial intelligence research laboratory consisting of the for-profit corporation OpenAI LP and its parent company, the non-profit OpenAI Inc.",
                InputFormatType = MpInputFormatType.Text                
            };

            await InitializeDefaultsAsync(openAiModel);
        }

        public override async Task LoadChildren() {
            IsBusy = true;

            var aipl = new List<MpAnalyticItemParameter>() {
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Label = "Engine",
                    IsParameterRequired = true,
                    SortOrderIdx = 0,
                    ParameterEnumId = MpOpenAiParamType.Engine,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            IsDefault = true,
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "DaVinci",
                            Value = "davinci"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Curie",
                            Value = "curie"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Babbage",
                            Value = "babbage"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Ada",
                            Value = "ada"
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Label = "End Point",
                    IsParameterRequired = true,
                    SortOrderIdx = 1,
                    ParameterEnumId = MpOpenAiParamType.EndPoint,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            IsDefault = true,
                            Label = "Completions",
                            Value =  "completions"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Searches",
                            Value =  "searches"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Classifications",
                            Value =  "classifications"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Answers",
                            Value =  "answers"
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                            Label = "Files",
                            Value =  "files"
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.Slider,
                    Label = "Temperature",
                    IsParameterRequired = true,
                    SortOrderIdx = 2,
                    Description = "Controls randomness: Lowering results in less random completions. As the temperature approaches zero, the model will become deterministic and repetitive.",
                    ParameterEnumId = MpOpenAiParamType.Temperature,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "0",
                            IsMinimum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "1",
                            IsMaximum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "0.75",
                            IsDefault = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.Slider,
                    Label = "Max Tokens",
                    IsParameterRequired = true,
                    SortOrderIdx = 3,
                    ParameterEnumId = MpOpenAiParamType.MaxTokens,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Integer,
                            Value = "1",
                            IsMinimum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Integer,
                            Value = "2048",
                            IsMaximum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Integer,
                            Value = "64",
                            IsDefault = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.Slider,
                    Label = "Top P",
                    IsParameterRequired = true,
                    SortOrderIdx = 4,
                    ParameterEnumId = MpOpenAiParamType.TopP,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "0",
                            IsMinimum = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "1",
                            IsMaximum = true,
                            IsDefault = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.Slider,
                    Label = "Frequency Penalty",
                    IsParameterRequired = true,
                    SortOrderIdx = 5,
                    ParameterEnumId = MpOpenAiParamType.FreqPen,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "0",
                            IsMinimum = true,
                            IsDefault = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "2",
                            IsMaximum = true
                        }
                    }
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.Slider,
                    Label = "Presence Penalty",
                    IsParameterRequired = true,
                    SortOrderIdx = 6,
                    ParameterEnumId = MpOpenAiParamType.PresPen,
                    ValueSeeds = new List<MpAnalyticItemParameterValue>() {
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
                            Value = "0",
                            IsMinimum = true,
                            IsDefault = true
                        },
                        new MpAnalyticItemParameterValue() {
                            ParameterValueType = MpAnalyticItemParameterValueUnitType.Decimal,
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

        protected override async Task ExecuteAnalysis() {
            IsBusy = true;

            string endpoint = string.Format(
                @"https://api.openai.com/v1/engines/{0}/{1}",
                GetParam(MpOpenAiParamType.Engine).CurrentValueViewModel.Value.ToLower(),
                GetParam(MpOpenAiParamType.EndPoint).CurrentValueViewModel.Value.ToLower());


            MpOpenAiRequest jsonReq = new MpOpenAiRequest() {
                Prompt = Regex.Escape(Parent.Parent.CopyItemData.ToPlainText()),
                Temperature = GetParam(MpOpenAiParamType.Temperature).CurrentValueViewModel.DoubleValue,
                MaxTokens = GetParam(MpOpenAiParamType.MaxTokens).CurrentValueViewModel.IntValue,
                TopP = GetParam(MpOpenAiParamType.TopP).CurrentValueViewModel.DoubleValue,
                FrequencyPenalty = GetParam(MpOpenAiParamType.FreqPen).CurrentValueViewModel.DoubleValue,
                PresencePenalty = GetParam(MpOpenAiParamType.PresPen).CurrentValueViewModel.DoubleValue
            };

            string jsonRespStr = await MpOpenAi.Instance.Request(
                endpoint,
                JsonConvert.SerializeObject(jsonReq));

            MpOpenAiResponse jsonResp = JsonConvert.DeserializeObject<MpOpenAiResponse>(jsonRespStr);

            if (jsonResp != null && jsonResp.choices != null && jsonResp.choices.Count > 0) {
               ResultViewModel.Result = Regex.Unescape(jsonResp.choices[0].text);
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
