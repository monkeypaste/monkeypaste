using System;
using System.Collections.Generic;
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

        public MpTranslatorViewModel(MpAnalyticItemCollectionViewModel parent, int aiid) : base(parent) {
            PropertyChanged += MpTranslatorViewModel_PropertyChanged;
            RuntimeId = aiid;
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
                tai = MpDb.Instance.GetItem<MpAnalyticItem>(tai.Id);
            }            

            await InitializeDefaultsAsync(tai);
        }

        public override async Task LoadChildren() {
            IsBusy = true;
            int defFromLangIdx = -1;
            if (!MpLanguageTranslator.Instance.IsLoaded) {
                await MpLanguageTranslator.Instance.Init();

                string defaultFromLangCode = await MpLanguageTranslator.Instance.DetectLanguage(Parent.Parent.CopyItemData.ToPlainText());
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
                    Value = lcat.Value.ToString(),
                    ParameterValueType = MpAnalyticItemParameterValueUnitType.PlainText
                };
                laipvl.Add(laipv);
            }

            var aipl = new List<MpAnalyticItemParameter>() {
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Label = "From Language",
                    ValueSeeds = laipvl,
                    IsParameterRequired = true,
                    EnumId = (int)MpTranslatorParamType.FromLang,
                    SortOrderIdx = 0
                },
                new MpAnalyticItemParameter() {
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Label = "To Language",
                    ValueSeeds = laipvl,
                    IsParameterRequired = true,
                    EnumId = (int)MpTranslatorParamType.ToLang,
                    SortOrderIdx = 1
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

            int fromNameIdxEnd = GetParam((int)MpTranslatorParamType.FromLang).CurrentValueViewModel.Value.IndexOf(" ");
            string fromName = GetParam((int)MpTranslatorParamType.FromLang).CurrentValueViewModel.Value.Substring(0,fromNameIdxEnd);
            string fromCode = MpLanguageTranslator.Instance.GetCodeByLanguageName(fromName);

            int toNameIdxEnd = GetParam((int)MpTranslatorParamType.ToLang).CurrentValueViewModel.Value.IndexOf(" ");
            string toName = GetParam((int)MpTranslatorParamType.ToLang).CurrentValueViewModel.Value.Substring(0,toNameIdxEnd);
            string toCode = MpLanguageTranslator.Instance.GetCodeByLanguageName(toName);

            //MpCheckBoxParameterViewModel useSpellCheckParam = (MpCheckBoxParameterViewModel)Parameters.Where(x => x.Key == "Use Spell Check").FirstOrDefault();
            
            string translatedText = await MpLanguageTranslator.Instance.TranslateAsync(
                Parent.Parent.CopyItemData.ToPlainText(),
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
