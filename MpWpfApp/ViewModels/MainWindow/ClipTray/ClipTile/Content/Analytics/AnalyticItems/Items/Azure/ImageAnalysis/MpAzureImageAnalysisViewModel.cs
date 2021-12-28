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

namespace MpWpfApp {

    public enum MpAzureImageAnalysisParamType {
        None = 0,
        Language,
        VisualFeatures,
        Details
    }

    public class MpAzureImageAnalysisViewModel : MpAnalyticItemViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region State

        #endregion

        #region Model


        #endregion

        #endregion

        #region Constructors

        public MpAzureImageAnalysisViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods

        protected override async Task<object> ExecuteAnalysis(object obj) {
            IsBusy = true;

            var paramLookup = SelectedPresetViewModel.ParamLookup;
            string lang = paramLookup[(int)MpAzureImageAnalysisParamType.Language].CurrentValue;
            string[] features = paramLookup[(int)MpAzureImageAnalysisParamType.VisualFeatures].CurrentValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string[] details = paramLookup[(int)MpAzureImageAnalysisParamType.Details].CurrentValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            MpAzureImageAnalysisRequest aiar = new MpAzureImageAnalysisRequest() {
                DefaultLanguageCode = lang,
                VisualFeatures = features.ToList(),
                Details = details.ToList()
            };

            var response = await MpImageAnalyzer.Instance.AnalyzeImage(obj.ToString().ToBitmapSource().ToByteArray(), aiar);

            IsBusy = false;

            return new Tuple<object, object>(response, aiar);
        }

        
        #endregion

        #region Private Methods
        #endregion
    }
}
