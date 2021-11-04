using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemCollectionViewModel : MpSingletonViewModel<MpAnalyticItemCollectionViewModel,object> {

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        #endregion

        #endregion

        #region Constructors

        public override async Task Init() {
            IsBusy = true;

            var ail = await MpDb.Instance.GetItemsAsync<MpAnalyticItem>();
            if(ail.Count == 0) {
                await InitTestData();
                ail = await MpDb.Instance.GetItemsAsync<MpAnalyticItem>();
            }

            Items.Clear();
            foreach(var ai in ail) {
                var naivm = await CreateAnalyticItemViewModel(ai);
                Items.Add(naivm);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpAnalyticItemViewModel> CreateAnalyticItemViewModel(MpAnalyticItem ai) {
            var naivm = new MpAnalyticItemViewModel(this);
            await naivm.InitializeAsync(ai);
            return naivm;
        }
        #endregion

        #region Private Methods

        private async Task InitTestData() {
            var ai_1 = await MpAnalyticItem.Create(
                endPoint: "https://api.cognitive.microsofttranslator.com/{0}",
                apiKey: MpPreferences.Instance.AzureCognitiveServicesKey,
                title: "Language Translator",
                description: "Azure Cognitive-Services Language Translator",
                format: MpInputFormatType.Text
                );

            var ai_1_p_0 = await MpAnalyticItemParameter.Create(
                key: "api-version",
                value: "3.0",
                isRequired: true,
                sortOrderIdx: 0,
                isHeader: false,
                isRequest: true,
                parentItem: ai_1);

            var ai_1_p_1 = await MpAnalyticItemParameter.Create(
                key: "languages",
                value: null,
                isRequired: true,
                sortOrderIdx: 0,
                isHeader: false,
                isRequest: true,
                parentItem: ai_1);

            var ai_1_p_2 = await MpAnalyticItemParameter.Create(
                key: "scope",
                value: "translation",
                isRequired: true,
                sortOrderIdx: 1,
                isHeader: false, 
                isRequest: true,
                parentItem: ai_1);

            var ai_1_p_3 = await MpAnalyticItemParameter.Create(
                key: "detect",
                value: null,
                isRequired: true,
                sortOrderIdx: 1,
                isHeader: false,
                isRequest: true,
                parentItem: ai_1);

            var ai_1_h_1 = await MpAnalyticItemParameter.Create(
                key: "Ocp-Apim-Subscription-Key",
                value: MpPreferences.Instance.AzureCognitiveServicesKey,
                isRequired: true,
                sortOrderIdx: 0,
                isHeader: true,
                isRequest: true,
                parentItem: ai_1);

            var ai_1_h_2 = await MpAnalyticItemParameter.Create(
                key: "Ocp-Apim-Subscription-Region",
                value: "westus",
                isRequired: true,
                sortOrderIdx: 1,
                isHeader: true,
                isRequest: true,
                parentItem: ai_1);

            var ai_1_h_3 = await MpAnalyticItemParameter.Create(
                key: "Accept-Language",
                value: "en",
                isRequired: true,
                sortOrderIdx: 2,
                isHeader: true,
                isRequest: true,
                parentItem: ai_1);

            var ai_1_r_1 = await MpAnalyticItemParameter.Create(
                key: "translation",
                value: "en",
                isRequired: true,
                sortOrderIdx: 1,
                isHeader: true,
                isRequest: false,
                parentItem: ai_1);

            var ai_1_activity_1 = new MpAnalyticItemActivity() {
                AnalyticItemActivityGuid = Guid.NewGuid(),
                ContentType = "application/json; charset=utf-8",
                Method = "POST",
                Name = "Detect Language",
                Description = "Detects language of user supplied text and replies with the languages name",
                Parameters = new List<MpAnalyticItemParameter>() {
                    ai_1_p_0,

                }
            };
            await MpDb.Instance.AddOrUpdateAsync<MpAnalyticItemActivity>(ai_1_activity_1);

            var ai_1_activity_1_p_1 = new MpAnalyticItemActivityParameter() {
                AnalyticItemActivityParameterGuid = Guid.NewGuid(),
                AnalyticItemActivityId = ai_1_activity_1.Id,
                Activity = ai_1_activity_1
            };
        }

        #endregion
    }
}
