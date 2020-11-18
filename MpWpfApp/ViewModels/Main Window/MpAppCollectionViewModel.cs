using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAppCollectionViewModel : MpObservableCollectionViewModel<MpAppViewModel> {
        private static readonly Lazy<MpAppCollectionViewModel> _Lazy = new Lazy<MpAppCollectionViewModel>(() => new MpAppCollectionViewModel());
        public static MpAppCollectionViewModel Instance { get { return _Lazy.Value; } }

        #region View Models
        #endregion

        #region Properties

        #endregion

        #region Public Methods
        public void Init() {
            //empty to initialize singleton
        }
        public MpAppCollectionViewModel() {
            foreach (var app in MpApp.GetAllApps()) {
                this.Add(new MpAppViewModel(app));
            }
        }

        public MpAppViewModel GetAppViewModelByAppId(int appId) {
            foreach(var avm in this.Where(x => x.AppId == appId)) {
                return avm;
            }
            return null;
        }

        public MpAppViewModel GetAppViewModelByProcessPath(string processPath) {
            foreach (var avm in this.Where(x => x.AppPath == processPath)) {
                return avm;
            }
            return null;
        }

        public void UpdateRejection(MpAppViewModel app, bool rejectApp) {
            if (this.Contains(app)) {
                bool wasCanceled = false;
                if (rejectApp) {
                    var ctrvm = MainWindowViewModel.ClipTrayViewModel;
                    var clipsFromApp = ctrvm.Where(x => x.CopyItemAppId == app.AppId).ToList();
                    if(clipsFromApp != null && clipsFromApp.Count > 0) {
                        MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + app.AppName + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (confirmExclusionResult == MessageBoxResult.Cancel) {
                            wasCanceled = true;
                        } else {
                            MpApp appToReject = app.App;
                            if (confirmExclusionResult == MessageBoxResult.Yes) {
                                var clipTilesToRemove = new List<MpClipTileViewModel>();
                                foreach (MpClipTileViewModel ctvm in clipsFromApp) {
                                    if (ctvm.CopyItemAppId == appToReject.AppId) {
                                        clipTilesToRemove.Add(ctvm);
                                    }
                                }
                                foreach (MpClipTileViewModel ctToRemove in clipTilesToRemove) {
                                    ctrvm.Remove(ctToRemove);
                                    ctToRemove.CopyItem.DeleteFromDatabase();
                                }
                            }
                        }
                    }                    
                }
                if (!wasCanceled) {
                    this[this.IndexOf(app)].IsAppRejected = rejectApp;
                    this[this.IndexOf(app)].App.WriteToDatabase();
                }
            }
        }

        public new void Add(MpAppViewModel avm) {
            if(avm.IsAppRejected) {
                var dupList = this.Where(x => x.AppPath == avm.AppPath).ToList();
                if (dupList != null && dupList.Count > 0) {
                    var ctrvm = MainWindowViewModel.ClipTrayViewModel;
                    var ctvms = ctrvm.Where(x => x.CopyItemAppId == dupList[0].AppId).ToList();
                    if(ctvms != null && ctvms.Count > 0) {
                        MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + avm.AppName + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                        if (confirmExclusionResult == MessageBoxResult.Cancel) {
                            //do nothing
                        } else {
                            MpApp appToReject = dupList[0].App;
                            if (confirmExclusionResult == MessageBoxResult.Yes) {
                                var clipTilesToRemove = new List<MpClipTileViewModel>();
                                foreach (MpClipTileViewModel ctvm in ctrvm) {
                                    if (ctvm.CopyItemAppId == appToReject.AppId) {
                                        clipTilesToRemove.Add(ctvm);
                                    }
                                }
                                foreach (MpClipTileViewModel ctToRemove in clipTilesToRemove) {
                                    ctrvm.Remove(ctToRemove);
                                    ctToRemove.CopyItem.DeleteFromDatabase();
                                }
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
            base.Add(avm);
        }

        public new void Remove(MpAppViewModel avm) {
            base.Remove(avm);
        }
        #endregion

        #region Commands
        #endregion
    }
}
