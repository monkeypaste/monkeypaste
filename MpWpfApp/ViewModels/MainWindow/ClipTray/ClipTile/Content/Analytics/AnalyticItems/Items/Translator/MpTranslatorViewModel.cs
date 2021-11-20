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
        Execute,
        Result,
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
            PropertyChanged += MpTranslatorViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public override async Task Initialize() {
            MpAnalyticItem tai = await MpDataModelProvider.Instance.GetAnalyticItemByTitle("Language Translator");
            if(tai == null) {
                tai = await MpAnalyticItem.Create(
                        "https://api.cognitive.microsofttranslator.com/{0}",
                        MpPreferences.Instance.AzureCognitiveServicesKey,
                        MpInputFormatType.Text,
                        "Language Translator",
                        "Azure Cognitive-Services Language Translator");
            } else {
                tai = await MpDb.Instance.GetItemAsync<MpAnalyticItem>(tai.Id);
            }            

            await InitializeDefaultsAsync(tai);
        }

        public override async Task LoadChildren() {
            IsBusy = true;
            int defFromLangIdx = -1;
            if (!MpLanguageTranslator.Instance.IsLoaded) {
                await MpLanguageTranslator.Instance.Init();

                string defaultFromLangCode = "en"; //await MpLanguageTranslator.Instance.DetectLanguage(MpP);

                if (string.IsNullOrWhiteSpace(defaultFromLangCode)) {
                    defaultFromLangCode = "en";
                }

                if (MpLanguageTranslator.Instance.LanguageCodesAndTitles.ContainsKey(defaultFromLangCode)) {
                    var langKvp = MpLanguageTranslator.Instance.LanguageCodesAndTitles[defaultFromLangCode];
                    defFromLangIdx = MpLanguageTranslator.Instance.LanguageList.IndexOf(langKvp.LanguageName);
                }
            }

            List<MpAnalyticItemParameterValue> laipvl = new List<MpAnalyticItemParameterValue>();
            foreach(var lcat in MpLanguageTranslator.Instance.LanguageCodesAndTitles) {
                var laipv = new MpAnalyticItemParameterValue() {
                    IsDefault = MpLanguageTranslator.Instance.LanguageCodesAndTitles.IndexOf(lcat) == defFromLangIdx,
                    Label = lcat.Value.LanguageName,
                    Value = lcat.Value.ToString()
                };
                laipvl.Add(laipv);
            }

            var aipl = new List<MpAnalyticItemParameter>() {
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.ComboBox,
                    ValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                    Label = "From Language",
                    ValueSeeds = laipvl,
                    IsParameterRequired = true,
                    EnumId = (int)MpTranslatorParamType.FromLang,
                    SortOrderIdx = 0
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticItemParameterType.ComboBox,
                    ValueType = MpAnalyticItemParameterValueUnitType.PlainText,
                    Label = "To Language",
                    ValueSeeds = laipvl,
                    IsParameterRequired = true,
                    EnumId = (int)MpTranslatorParamType.ToLang,
                    SortOrderIdx = 1
                }
            };
            //aipl[1].ValueSeeds.ForEach(x => x.IsDefault = false);
            AnalyticItem.Parameters = aipl;

            await base.LoadChildren();

            IsBusy = false;
        }
        #endregion

        #region Protected Methods

        protected override void ParameterViewModel_OnValidate(object sender, EventArgs e) {
            if(IsBusy) {
                return;
            }

            var fromParam = GetParam((int)MpTranslatorParamType.FromLang) as MpComboBoxParameterViewModel;
            var toParam = GetParam((int)MpTranslatorParamType.ToLang) as MpComboBoxParameterViewModel;

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

        protected override async Task ExecuteAnalysis(object obj) {
            IsBusy = true;

            int fromNameIdxEnd = GetParam((int)MpTranslatorParamType.FromLang).CurrentValue.IndexOf(" ");
            string fromName = GetParam((int)MpTranslatorParamType.FromLang).CurrentValue.Substring(0,fromNameIdxEnd);
            string fromCode = MpLanguageTranslator.Instance.GetCodeByLanguageName(fromName);

            int toNameIdxEnd = GetParam((int)MpTranslatorParamType.ToLang).CurrentValue.IndexOf(" ");
            string toName = GetParam((int)MpTranslatorParamType.ToLang).CurrentValue.Substring(0,toNameIdxEnd);
            string toCode = MpLanguageTranslator.Instance.GetCodeByLanguageName(toName);

            //MpCheckBoxParameterViewModel useSpellCheckParam = (MpCheckBoxParameterViewModel)Parameters.Where(x => x.Key == "Use Spell Check").FirstOrDefault();
            
            string translatedText = await MpLanguageTranslator.Instance.TranslateAsync(
                obj.ToString(),
                toCode,
                fromCode, 
                false);

            ResultViewModel.Result = translatedText;
            IsBusy = false;
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
