using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;
using Newtonsoft.Json;
using Windows.Globalization;
using Xamarin.Forms.Internals;

namespace MpWpfApp {
    public enum MpTranslatorParamType {
        None = 0,
        FromLang,
        ToLang
    }
    public class MpTranslatorViewModel : MpAnalyticItemViewModel {
        #region Properties
        #region State
        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpTranslatorViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
         }

        #endregion

        #region Public Methods

        public override async Task<MpAnalyticItemParameter> DeferredCreateParameterModel(MpAnalyticItemParameter aip) {
            IsBusy = true;

            if (!MpLanguageTranslator.Instance.IsLoaded) {
                await MpLanguageTranslator.Instance.Init();
            }

            foreach(var lcat in MpLanguageTranslator.Instance.LanguageCodesAndTitles) {
                var laipv = new MpAnalyticItemParameterValue() {
                    Label = string.Format(@"{0} - {1}",lcat.Value.LanguageName, lcat.Value.NativeName),
                    Value = lcat.Key.ToString()
                };
                aip.Values.Add(laipv);
            }

            IsBusy = false;
            return aip;
        }


        public override bool Validate() {
            if (IsBusy || Parent.IsBusy) {
                return true;
            }

            var paramLookup = SelectedPresetViewModel.ParamLookup;

            var fromParam = paramLookup[(int)MpTranslatorParamType.FromLang] as MpComboBoxParameterViewModel;
            var toParam = paramLookup[(int)MpTranslatorParamType.ToLang] as MpComboBoxParameterViewModel;

            if (fromParam.CurrentValue == toParam.CurrentValue) {
                toParam.ValidationMessage = "Cannot translate to same language";
            } else {
                toParam.ValidationMessage = string.Empty;
            }
            return string.IsNullOrEmpty(toParam.ValidationMessage);
        }

        #endregion

        #region Protected Methods


        protected override async Task<object> ExecuteAnalysis(object obj) {
            IsBusy = true;
            var paramLookup = SelectedPresetViewModel.ParamLookup;

            string fromCode = paramLookup[(int)MpTranslatorParamType.FromLang].CurrentValue;
            string toCode = paramLookup[(int)MpTranslatorParamType.ToLang].CurrentValue;

            string translatedText = await MpLanguageTranslator.Instance.TranslateAsync(
                obj.ToString(),
                toCode,
                fromCode);

            IsBusy = false;
            return new Tuple<object, object>(
                translatedText,
                new MpLangTranslateRequestFormat() {
                    FromCode = fromCode,
                    ToCode = toCode
                });
        }
        #endregion

        #region Private Methods
        
        #endregion
    }
}
