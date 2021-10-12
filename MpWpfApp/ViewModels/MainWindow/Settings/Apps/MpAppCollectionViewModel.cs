using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAppCollectionViewModel : MpViewModelBase<object> {
        private static readonly Lazy<MpAppCollectionViewModel> _Lazy = new Lazy<MpAppCollectionViewModel>(() => new MpAppCollectionViewModel());
        public static MpAppCollectionViewModel Instance { get { return _Lazy.Value; } }

        #region View Models
        private ObservableCollection<MpAppViewModel> _appViewModels = new ObservableCollection<MpAppViewModel>();
        public ObservableCollection<MpAppViewModel> AppViewModels {
            get {
                return _appViewModels;
            }
            set {
                if(_appViewModels != value) {
                    _appViewModels = value;
                    OnPropertyChanged(nameof(AppViewModels));
                }
            }
        }
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public void Init() {
            //empty to initialize singleton
        }
        private MpAppCollectionViewModel() : base(null) {
            Refresh();
        }

        public MpAppViewModel GetAppViewModelByAppId(int appId) {
            foreach(var avm in AppViewModels.Where(x => x.AppId == appId)) {
                return avm;
            }
            return null;
        }

        public MpAppViewModel GetAppViewModelByProcessPath(string processPath) {
            foreach (var avm in AppViewModels.Where(x => x.AppPath == processPath)) {
                return avm;
            }
            return null;
        }

        public bool UpdateRejection(MpAppViewModel app, bool rejectApp) {
            if(app.App.IsAppRejected == rejectApp) {
                return rejectApp;
            }
            if (AppViewModels.Contains(app)) {
                bool wasCanceled = false;
                if (rejectApp) {
                    var ctrvm = MpClipTrayViewModel.Instance;
                    var clipsFromApp = ctrvm.GetClipTilesByAppId(app.AppId);
                    if (clipsFromApp != null && clipsFromApp.Count > 0) {
                        MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + app.AppName + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (confirmExclusionResult == MessageBoxResult.Cancel) {
                            wasCanceled = true;
                        } else {
                            MpApp appToReject = app.App;
                            if (confirmExclusionResult == MessageBoxResult.Yes) {
                                //var clipTilesToRemove = clipsFromApp.Select(x => x.ItemViewModels.Where(y => y.CopyItem.Source.AppId == appToReject.Id));

                                //foreach (var ctToRemove in clipTilesToRemove) {
                                //    ctToRemove.Parent.ItemViewModels.Remove(ctToRemove);
                                //    if (ctr)
                                //        ctToRemove.CopyItem.DeleteFromDatabase();
                                //}


                                // TODO Remove content items or empty clips above
                            }
                        }
                    }
                }
                if (wasCanceled) {
                    return app.IsAppRejected;
                }
                int appIdx = AppViewModels.IndexOf(app);
                AppViewModels[appIdx].App.IsAppRejected = rejectApp;
                AppViewModels[appIdx].App.WriteToDatabase();

                // TODO Ensure appcollection is loaded BEFORE clip tiles and its App object references part of this collection and not another instance w/ same appId
                foreach (var ctvm in MpClipTrayViewModel.Instance.Items) {
                    foreach (var rtbvm in ctvm.ItemViewModels) {
                        if (rtbvm.CopyItem.Source.App.Id == AppViewModels[appIdx].AppId) {
                            rtbvm.CopyItem.Source.App = AppViewModels[appIdx].App;
                        }
                    }
                }
            } else {
                MonkeyPaste.MpConsole.WriteLine("AppCollection.UpdateRejection error, app: " + app.AppName + " is not in collection");
            }
            return rejectApp;
        }

        public void Add(MpAppViewModel avm) {
            if(avm.IsAppRejected && avm.App != null) {
                var dupList = AppViewModels.Where(x => x.AppPath == avm.AppPath).ToList();
                if (dupList != null && dupList.Count > 0) {
                    var ctrvm = MpClipTrayViewModel.Instance;
                    var ctvms = ctrvm.GetClipTilesByAppId(dupList[0].AppId);
                    if(ctvms != null && ctvms.Count > 0) {
                        MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + avm.AppName + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (confirmExclusionResult == MessageBoxResult.Cancel) {
                            //do nothing
                        } else {
                            MpApp appToReject = dupList[0].App;
                            if (confirmExclusionResult == MessageBoxResult.Yes) {
                                //var clipTilesToRemove = new List<MpClipTileViewModel>();
                                //foreach (MpClipTileViewModel ctvm in ctrvm.ClipTileViewModels) {
                                //    if (ctvm.CopyItemAppId == appToReject.Id) {
                                //        clipTilesToRemove.Add(ctvm);
                                //    }
                                //}
                                //foreach (MpClipTileViewModel ctToRemove in clipTilesToRemove) {
                                //    ctrvm.Remove(ctToRemove);
                                //    ctToRemove.CopyItem.DeleteFromDatabase();
                                //}


                                // TODO Remove content items or empty clips above
                            }
                            appToReject.IsAppRejected = true;
                            appToReject.WriteToDatabase();
                            return;
                        }
                    }
                } else {
                    avm.App.WriteToDatabase();
                }
            }
            //base.Add(avm);
            Refresh();
        }

        public void Remove(MpAppViewModel avm) {
            AppViewModels.Remove(avm);
        }

        public void Refresh() {
            AppViewModels.Clear();
            foreach (var app in MpDb.Instance.GetItems<MpApp>()) {
                AppViewModels.Add(new MpAppViewModel(this,app));
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
