using MonkeyPaste;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSourceViewModel : MpViewModelBase<MpSourceCollectionViewModel> {
        #region Properties

        #region View Models

        public MpAppViewModel AppViewModel {
            get {
                if (Source == null) {
                    return null;
                }
                int appId = Source.IsUrlPrimarySource ? SecondarySource.RootId : PrimarySource.RootId;
                return MpAppCollectionViewModel.Instance.GetAppViewModelByAppId(appId);
            }
        }

        public MpUrlViewModel UrlViewModel {
            get {
                if (Source == null) {
                    return null;
                }
                int urlId = Source.IsUrlPrimarySource ? PrimarySource.RootId : SecondarySource.RootId;
                return MpUrlCollectionViewModel.Instance.GetUrlViewModelByUrlId(urlId);
            }
        }

        public MpIconViewModel PrimarySourceIconViewModel {
            get {
                if(Source == null) {
                    return null;
                }
                return Source.IsUrlPrimarySource ? UrlViewModel.IconViewModel : AppViewModel.IconViewModel;
            }
        }

        public MpIconViewModel SecondarySourceIconViewModel {
            get {
                if (Source == null) {
                    return null;
                }
                return !Source.IsUrlPrimarySource ? UrlViewModel.IconViewModel : AppViewModel.IconViewModel;
            }
        }

        #endregion

        #region State

        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }

        #endregion

        #region Model

        public MpISourceItem PrimarySource {
            get {
                if(Source == null) {
                    return null;
                }
                return Source.PrimarySource;
            }
        }

        public MpISourceItem SecondarySource {
            get {
                if (Source == null) {
                    return null;
                }
                return Source.SecondarySource;
            }
        }

        public MpSource Source { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpSourceViewModel(MpSourceCollectionViewModel parent) : base(parent) { }

        public async Task InitializeAsync(MpSource s) {
            IsBusy = true;

            Source = s;

            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion
    }
}
