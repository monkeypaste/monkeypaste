using MonkeyPaste;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSourceViewModel : 
        MpViewModelBase<MpAvSourceCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel{
        #region Properties

        #region View Models

        public MpAvAppViewModel AppViewModel => MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == AppId);

        public MpAvUrlViewModel UrlViewModel => MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == UrlId);

        public MpISourceItemViewModel PrimarySourceViewModel {
            get {
                if(Source == null) {
                    return null;
                }
                if(Source.IsUrlPrimarySource) {
                    return UrlViewModel;
                }
                return AppViewModel;
            }
        }

        #endregion

        #region State

        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Model

        
        public int UrlId {
            get {
                if(Source == null) {
                    return 0;
                }

                return Source.UrlId;
            }
        }

        public int AppId {
            get {
                if (Source == null) {
                    return 0;
                }

                return Source.AppId;
            }
        }

        public int SourceId {
            get {
                if(Source == null) {
                    return 0;
                }
                return Source.Id;
            }
        }
        //public MpISourceItem PrimarySource {
        //    get {
        //        if(Source == null) {
        //            return null;
        //        }
        //        if(Source.PrimarySource == null) {
        //            // BUG I think there's issues w/ SQLite extensions and foreign properties aren't being created. But 
        //            // primary source isn't being set for CopyItemTransactions
        //            return MpJsonPreferenceIO.Instance.ThisAppSource.PrimarySource;
        //        }
        //        return Source.PrimarySource;
        //    }
        //}

        //public MpISourceItem SecondarySource {
        //    get {
        //        if (Source == null) {
        //            return null;
        //        }
        //        return Source.SecondarySource;
        //    }
        //}

        public MpSource Source { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvSourceViewModel(MpAvSourceCollectionViewModel parent) : base(parent) { }

        public async Task InitializeAsync(MpSource s) {
            IsBusy = true;

            Source = s;


            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion
    }
}
