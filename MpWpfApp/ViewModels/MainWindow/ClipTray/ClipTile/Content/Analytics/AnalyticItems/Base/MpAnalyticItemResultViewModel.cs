using MonkeyPaste;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpAnalyticItemResultViewModel : MpAnalyticItemComponentViewModel {
        #region Properties

        #region State

        public bool HasResult => !string.IsNullOrEmpty(Result);

        #endregion

        #region Model

        public MpCopyItemType ResultType { get; set; } = MpCopyItemType.RichText;

        public string Result { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemResultViewModel() : base(null) { }

        public MpAnalyticItemResultViewModel(MpAnalyticItemViewModel parent) : base(parent) {
        }


        #endregion

        #region Public Methods

        public async Task ConvertToCopyItem(string reqStr, string resultData) {
            var app = MpPreferences.Instance.ThisAppSource.App;
            var url = await MpUrlBuilder.Create(Parent.AnalyticItem.EndPoint,null,reqStr);
            var source = await MpSource.Create(app, url);
            var ci = await MpCopyItem.Create(source, resultData, MpCopyItemType.RichText);

            var scivm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(Parent.SourceCopyItemId);

            var sml = scivm.Parent.ItemViewModels.Select(x => x.CopyItem).OrderBy(x=>x.CompositeSortOrderIdx).ToList();
            for (int i = 0; i < sml.Count; i++) {
                if(i == scivm.CompositeSortOrderIdx) {
                    ci.CompositeParentCopyItemId = sml[0].Id;
                    ci.CompositeSortOrderIdx = i + 1;
                } else if(i > scivm.CompositeSortOrderIdx) {
                    sml[i].CompositeSortOrderIdx += 1;
                }
            }

            await ci.WriteToDatabaseAsync();

            await scivm.Parent.InitializeAsync(sml[0]);
        }

        public override void Reset() {
            base.Reset();

            Result = string.Empty;
            OnPropertyChanged(nameof(HasResult));
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
