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
using System.IO;
using MpWpfApp.Properties;
using System.Windows.Markup;
using MonkeyPaste;

namespace MpWpfApp {

    public enum MpOpenAiParamType {
        None = 0,
        Engine,
        EndPoint,
        Temperature,
        MaxTokens,
        TopP,
        FreqPen,
        PresPen,
        Prompt
    }

    public class MpOpenAiViewModel : MpAnalyticItemViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region State

        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpOpenAiViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpOpenAiViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        protected override async Task<object> ExecuteAnalysis(object obj) {
            IsBusy = true;

            var paramLookup = SelectedPresetViewModel.ParamLookup;
            string engine = paramLookup[(int)MpOpenAiParamType.Engine].CurrentValue.ToLower();
            string ep = paramLookup[(int)MpOpenAiParamType.EndPoint].CurrentValue.ToLower();

            string endpoint = string.Format(
                @"https://api.openai.com/v1/engines/{0}/{1}",
                engine,
                ep);


            MpOpenAiRequest jsonReq = new MpOpenAiRequest() {
                Prompt = Regex.Escape(obj.ToString()),
                Temperature = paramLookup[(int)MpOpenAiParamType.Temperature].DoubleValue,
                MaxTokens = paramLookup[(int)MpOpenAiParamType.MaxTokens].IntValue,
                TopP = paramLookup[(int)MpOpenAiParamType.TopP].DoubleValue,
                FrequencyPenalty = paramLookup[(int)MpOpenAiParamType.FreqPen].DoubleValue,
                PresencePenalty = paramLookup[(int)MpOpenAiParamType.PresPen].DoubleValue
            };

            string jsonReqStr = JsonConvert.SerializeObject(jsonReq);
            string jsonRespStr = await MpOpenAi.Instance.Request(
                endpoint,
                jsonReqStr);

            MpOpenAiResponse jsonResp = JsonConvert.DeserializeObject<MpOpenAiResponse>(jsonRespStr);
            
            string resultData = string.Empty;
            if (jsonResp != null && jsonResp.choices != null && jsonResp.choices.Count > 0) {
               resultData = Regex.Unescape(jsonResp.choices[0].text);
            } 

            IsBusy = false;

            return new Tuple<object, object>(resultData,jsonReq);
        }
        #endregion

        #region Private Methods
        private void MpOpenAiViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }
        #endregion
    }
}
