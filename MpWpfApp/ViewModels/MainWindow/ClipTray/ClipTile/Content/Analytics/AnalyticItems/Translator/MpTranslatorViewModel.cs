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
            HasChildren = true;
            RuntimeId = aiid;
        }

        #endregion

        #region Public Methods

        public override async Task Initialize() {
            ItemIconSourcePath = Application.Current.Resources["TranslateIcon"] as string;

            var translateModel = new MpAnalyticItem() {
                Id = RuntimeId,
                AnalyticItemGuid = Guid.NewGuid(),
                EndPoint = "https://api.cognitive.microsofttranslator.com/{0}",
                ApiKey = MpPreferences.Instance.AzureCognitiveServicesKey,
                Title = "Language Translator",
                Description = "Azure Cognitive-Services Language Translator",
                InputFormatType = MpInputFormatType.Text                
            };

            await InitializeAsync(translateModel);
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

            var aipl = new List<MpAnalyticItemParameter>() {
                new MpAnalyticItemParameter() {
                    Id = RuntimeId,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Key = "From Language",
                    ValueCsv = string.Join(",", MpLanguageTranslator.Instance.LanguageList),
                    DefaultValue = defFromLangIdx >= 0 ? MpLanguageTranslator.Instance.LanguageList[defFromLangIdx]:null,
                    IsParameterRequired = true,
                    SortOrderIdx = 0
                },
                new MpAnalyticItemParameter() {
                    Id = RuntimeId + 1,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = RuntimeId,
                    ParameterType = MpAnalyticParameterType.ComboBox,
                    Key = "To Language",
                    ValueCsv = string.Join(",", MpLanguageTranslator.Instance.LanguageList),
                    IsParameterRequired = true,
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

            MpComboBoxParameterViewModel fromLangParam = (MpComboBoxParameterViewModel)Parameters.Where(x => x.Key == "From Language").FirstOrDefault();
            string fromCode = MpLanguageTranslator.Instance.GetCodeByLanguageName(fromLangParam.SelectedValue.Value);

            MpComboBoxParameterViewModel toLangParam = (MpComboBoxParameterViewModel)Parameters.Where(x => x.Key == "To Language").FirstOrDefault();
            string toCode = MpLanguageTranslator.Instance.GetCodeByLanguageName(toLangParam.SelectedValue.Value);

            //MpCheckBoxParameterViewModel useSpellCheckParam = (MpCheckBoxParameterViewModel)Parameters.Where(x => x.Key == "Use Spell Check").FirstOrDefault();
            
            var translatedText = await MpLanguageTranslator.Instance.TranslateAsync(
                Parent.Parent.CopyItemData.ToPlainText(),
                toCode,
                fromCode, 
                false);

            MpResultParameterViewModel resultParam = (MpResultParameterViewModel)Parameters.Where(x => x.Parameter.ParameterType == MpAnalyticParameterType.Result).FirstOrDefault();
            resultParam.ResultValue = translatedText;

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
