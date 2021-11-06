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
            HasChildren = true;
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

            await InitializeAsync(openAiModel);
        }

        public override async Task LoadChildren() {
            IsBusy = true;
            /*
             * "temperature": 0,
  "max_tokens": 60,
  "top_p": 1.0,
  "frequency_penalty": 0.0,
  "presence_penalty": 0.0,
             */
            var aipl = new List<MpAnalyticItemParameter>() {
                new MpAnalyticItemParameter() {
                    Id = RuntimeId,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Key = "Engine",
                    ValueCsv = "Ada,Babbage,Curie,DaVinci",
                    DefaultValue = "DaVinci", 
                    IsParameterRequired = true,
                    SortOrderIdx = 0
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 1,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Key = "End Point",
                    ValueCsv = "Completions,Searches,Classifications,Answers,Files",
                    DefaultValue = "Completions",
                    IsParameterRequired = true,
                    SortOrderIdx = 1
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 2,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.Slider,
                    Key = "Temperature",
                    ValueCsv = "0,1",
                    DefaultValue = "0.5",
                    IsParameterRequired = true,
                    SortOrderIdx = 2,
                    Description = "Controls randomness: Lowering results in less random completions. As the temperature approaches zero, the model will become deterministic and repetitive."
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 3,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.Slider,
                    Key = "Max Tokens",
                    ValueCsv = "1,2048",
                    DefaultValue = "64",
                    IsParameterRequired = true,
                    SortOrderIdx = 3
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 4,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.Slider,
                    Key = "Top P",
                    ValueCsv = "0,1",
                    DefaultValue = "1",
                    IsParameterRequired = true,
                    SortOrderIdx = 4
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 5,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.Slider,
                    Key = "Frequency Penalty",
                    ValueCsv = "0,2",
                    DefaultValue = "0",
                    IsParameterRequired = true,
                    SortOrderIdx = 5
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 6,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.Slider,
                    Key = "Presence Penalty",
                    ValueCsv = "0,2",
                    DefaultValue = "0",
                    IsParameterRequired = true,
                    SortOrderIdx = 6
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

            
            foreach (MpAnalyticItemParameterViewModel pvm in Parameters) {

            }
            string endpoint = "https://api.openai.com/v1/engines/";

            MpComboBoxParameterViewModel engineParam = (MpComboBoxParameterViewModel)Parameters.FirstOrDefault(x => x.Key == "Engine");
            endpoint += engineParam?.SelectedValue.Value.ToLower() + @"/";

            MpComboBoxParameterViewModel endpointParam = (MpComboBoxParameterViewModel)Parameters.FirstOrDefault(x => x.Key == "End Point");
            endpoint += endpointParam?.SelectedValue.Value.ToLower();


            MpOpenAiRequest jsonReq = new MpOpenAiRequest() {
                Prompt = Regex.Escape(Parent.Parent.CopyItemData.ToPlainText()),
                //Engine = engineParam?.SelectedValue.Value.ToLower(),
                Temperature = ((MpSliderParameterViewModel)Parameters.FirstOrDefault(x=>x.Key=="Temperature")).Value,
                MaxTokens = (int)((MpSliderParameterViewModel)Parameters.FirstOrDefault(x => x.Key == "Max Tokens")).Value,
                TopP = ((MpSliderParameterViewModel)Parameters.FirstOrDefault(x => x.Key == "Top P")).Value,
                FrequencyPenalty = ((MpSliderParameterViewModel)Parameters.FirstOrDefault(x => x.Key == "Frequency Penalty")).Value,
                PresencePenalty = ((MpSliderParameterViewModel)Parameters.FirstOrDefault(x => x.Key == "Presence Penalty")).Value
            };
            var jsonRespStr = await MpOpenAi.Instance.Request(
                endpoint,
                JsonConvert.SerializeObject(jsonReq));

            MpOpenAiResponse jsonResp = JsonConvert.DeserializeObject<MpOpenAiResponse>(jsonRespStr);

            MpResultParameterViewModel resultParam = (MpResultParameterViewModel)Parameters.FirstOrDefault(x => x.Parameter.ParameterType == MpAnalyticParameterType.Result);
            if (jsonResp != null && jsonResp.choices != null && jsonResp.choices.Count > 0) {
                resultParam.ResultValue = Regex.Unescape(jsonResp.choices[0].text);
            } else {
                Debugger.Break();
            }

            IsBusy = false;
        }
        #endregion

        #region Private Methods
        private void MpOpenAiViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                
            }
        }
        #endregion
    }
}
