using MonkeyPaste;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSourceViewModel : 
        MpViewModelBase<MpSourceCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel{
        #region Properties

        #region View Models

        //public MpAppViewModel AppViewModel {
        //    get {
        //        if (Source == null) {
        //            return null;
        //        }
        //        int appId = Source.IsUrlPrimarySource ? SecondarySource.RootId : PrimarySource.RootId;
        //        return MpAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == appId);
        //    }
        //}

        //public MpUrlViewModel UrlViewModel {
        //    get {
        //        if (Source == null) {
        //            return null;
        //        }
        //        int urlId = Source.IsUrlPrimarySource ? PrimarySource.RootId : SecondarySource.RootId;
        //        return MpUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == urlId);
        //    }
        //}

        //public MpIconViewModel PrimarySourceIconViewModel {
        //    get {
        //        if(Source == null) {
        //            return null;
        //        }
        //        if(Source.IsUrlPrimarySource) {
        //            if(UrlViewModel == null) {
        //                return null;
        //            }
        //            return UrlViewModel.IconViewModel;
        //        } else {
        //            if (AppViewModel == null) {
        //                return null;
        //            }
        //            return AppViewModel.IconViewModel;
        //        }
        //    }
        //}

        //public MpIconViewModel SecondarySourceIconViewModel {
        //    get {
        //        if (Source == null) {
        //            return null;
        //        }
        //        if (!Source.IsUrlPrimarySource) {
        //            if (UrlViewModel == null) {
        //                return null;
        //            }
        //            return UrlViewModel.IconViewModel;
        //        } else {
        //            if (AppViewModel == null) {
        //                return null;
        //            }
        //            return AppViewModel.IconViewModel;
        //        }
        //    }
        //}

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
                if(Source.PrimarySource == null) {
                    // BUG I think there's issues w/ SQLite extensions and foreign properties aren't being created. But 
                    // primary source isn't being set for CopyItemTransactions
                    return MpPreferences.ThisAppSource.PrimarySource;
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
