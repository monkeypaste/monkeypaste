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
        protected override string FormatSourcePath => "MonkeyPaste.Resources.Data.Analytics.Formats.LanguageTranslator.azuretranslator.json";
        #endregion

        #endregion

        #region Constructors

        public MpTranslatorViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpTranslatorViewModel_PropertyChanged;
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

        #endregion

        #region Protected Methods

        protected override void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            if(IsBusy || Parent.IsBusy) {
                return;
            }

            var fromParam = ParamLookup[(int)MpTranslatorParamType.FromLang] as MpComboBoxParameterViewModel;
            var toParam = ParamLookup[(int)MpTranslatorParamType.ToLang] as MpComboBoxParameterViewModel;

            if (fromParam.CurrentValue == toParam.CurrentValue) {
                if(sender == fromParam) {
                    fromParam.ValidationMessage = "Cannot translate to same language";
                } else {
                    toParam.ValidationMessage = "Cannot translate to same language";
                }
            } else {
                if (sender == fromParam) {
                    fromParam.ValidationMessage = string.Empty;
                } else {
                    toParam.ValidationMessage = string.Empty;
                }
            }
        }

        protected override async Task<object> ExecuteAnalysis(object obj) {
            IsBusy = true;

            string fromCode = ParamLookup[(int)MpTranslatorParamType.FromLang].CurrentValue;
            string toCode = ParamLookup[(int)MpTranslatorParamType.ToLang].CurrentValue;


            //MpCheckBoxParameterViewModel useSpellCheckParam = (MpCheckBoxParameterViewModel)Parameters.Where(x => x.Key == "Use Spell Check").FirstOrDefault();

            string translatedText = await MpLanguageTranslator.Instance.TranslateAsync(
                obj.ToString(),
                toCode,
                fromCode);

            IsBusy = false;
            return new Tuple<string, MpJsonMessage>(
                translatedText,
                new MpLangTranslateRequestFormat() {
                    FromCode = fromCode,
                    ToCode = toCode
                });
        }
        #endregion

        #region Private Methods
        private void MpTranslatorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                
            }
        }



        #endregion
    }
}
