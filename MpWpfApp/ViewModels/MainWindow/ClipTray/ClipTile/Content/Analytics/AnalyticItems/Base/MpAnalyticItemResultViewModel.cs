using MonkeyPaste;
using System;
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

        public MpAnalyticItemResultViewModel(MpAnalyticItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task<MpCopyItem> ConvertToCopyItem() {
            if(!HasResult) {
                return null;
            }
            var app = MpPreferences.Instance.ThisAppSource.App;
            var url = await MpUrlBuilder.Create(Parent.AnalyticItem.EndPoint,null);
            var source = await MpSource.Create(app, url);
            var ci = await MpCopyItem.Create(source, Result, MpCopyItemType.RichText);

            return ci;
        }

        public override void Reset() {
            base.Reset();

            Result = string.Empty;
            OnPropertyChanged(nameof(HasResult));
        }
        #endregion
    }
}
