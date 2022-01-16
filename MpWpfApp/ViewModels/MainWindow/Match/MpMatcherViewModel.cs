using Azure;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpMatcherViewModel : MpViewModelBase<MpMatcherCollectionViewModel>, MpIFileSystemEventHandler, MpITreeItemViewModel {

        #region Properties

        #region View Models

        public ObservableCollection<string> TriggerTypes { get; set; } = new ObservableCollection<string>(typeof(MpMatchTriggerType).EnumToLabels("Select Trigger"));

        public int SelectedTriggerTypeIdx {
            get {
                return (int)TriggerType;
            }
            set {
                if((int)TriggerType != value) {
                    TriggerType = (MpMatchTriggerType)value;
                    OnPropertyChanged(nameof(SelectedTriggerTypeIdx));
                }
            }
        }

        public ObservableCollection<string> TriggerActionTypes { get; set; } = new ObservableCollection<string>(typeof(MpMatchActionType).EnumToLabels("Select Trigger Action"));
        
        public int SelectedTriggerActionTypeIdx {
            get {
                return (int)TriggerActionType;
            }
            set {
                if ((int)TriggerActionType != value) {
                    TriggerActionType = (MpMatchActionType)value;
                    OnPropertyChanged(nameof(SelectedTriggerActionTypeIdx));
                }
            }
        }

        public MpMatcherViewModel ParentMatcherViewModel { get; set; } = null;

        public MpContextMenuItemViewModel MatcherContextMenuItemViewModel {
            get {
                var cmvml = FindChildren();

                return new MpContextMenuItemViewModel(
                        header: Title,
                        command: null,
                        commandParameter: null,
                        isChecked: null,
                        bmpSrc: MpIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == IconId).IconBitmapSource,
                        subItems: cmvml.Count == 0 ? null : new ObservableCollection<MpContextMenuItemViewModel>(cmvml.Select(x=>x.MatcherContextMenuItemViewModel)),
                        bgBrush: null);
            }
        }
        #endregion

        #region MpITreeItemViewModel Implementation


        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }
        public bool IsExpanded { get; set; }

        public MpITreeItemViewModel ParentTreeItem => ParentMatcherViewModel;

        public ObservableCollection<MpITreeItemViewModel> Children => new ObservableCollection<MpITreeItemViewModel>();

        #endregion

        #region Model

        public int TriggerActionObjId {
            get {
                if (Matcher == null) {
                    return 0;
                }
                return Matcher.TriggerActionObjId;
            }
            set {
                if (TriggerActionObjId != value) {
                    Matcher.TriggerActionObjId = value;
                    OnPropertyChanged(nameof(TriggerActionObjId));
                }
            }
        }

        public MpMatcherType MatcherType {
            get {
                if (Matcher == null) {
                    return MpMatcherType.None;
                }
                return Matcher.MatcherType;
            }
            set {
                if (MatcherType != value) {
                    Matcher.MatcherType = value;
                    OnPropertyChanged(nameof(MatcherType));
                }
            }
        }

        public MpMatchTriggerType TriggerType {
            get {
                if(Matcher == null) {
                    return MpMatchTriggerType.None;
                }
                return Matcher.TriggerType;
            }
            set {
                if(TriggerType != value) {
                    Matcher.TriggerType = value;
                    OnPropertyChanged(nameof(TriggerType));
                }
            }
        }

        public MpMatchActionType TriggerActionType {
            get {
                if (Matcher == null) {
                    return MpMatchActionType.None;
                }
                return Matcher.TriggerActionType;
            }
            set {
                if (TriggerActionType != value) {
                    Matcher.TriggerActionType = value;
                    OnPropertyChanged(nameof(TriggerActionType));
                }
            }
        }

        public string Title {
            get {
                if (Matcher == null) {
                    return null;
                }
                return Matcher.Title;
            }
            set {
                if (MatchData != value) {
                    Matcher.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string MatchData {
            get {
                if(Matcher == null) {
                    return null;
                }
                return Matcher.MatchData;
            }
            set {
                if(MatchData != value) {
                    Matcher.MatchData = value;
                    OnPropertyChanged(nameof(MatchData));
                }
            }
        }

        public string IsMatchPropertyPath {
            get {
                if (Matcher == null) {
                    return null;
                }
                return Matcher.IsMatchPropertyPath;
            }
            set {
                if (MatchData != value) {
                    Matcher.IsMatchPropertyPath = value;
                    OnPropertyChanged(nameof(IsMatchPropertyPath));
                }
            }
        }

        public int SortOrderIdx {
            get {
                if (Matcher == null) {
                    return 0;
                }
                return Matcher.SortOrderIdx;
            }
            set {
                if (SortOrderIdx != value) {
                    Matcher.SortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public int IconId {
            get {
                if (Matcher == null) {
                    return 0;
                }
                return Matcher.IconId;
            }
        }

        public int ParentMatcherId {
            get {
                if (Matcher == null) {
                    return 0;
                }
                return Matcher.ParentMatcherId;
            }
            set {
                if(ParentMatcherId != value) {
                    Matcher.ParentMatcherId = value;
                    OnPropertyChanged(nameof(ParentMatcherId));
                }
            }
        }

        public int MatcherId {
            get {
                if(Matcher == null) {
                    return 0;
                }
                return Matcher.Id;
            }
        }

        public MpMatcher Matcher { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler<object> OnMatch;

        #endregion

        #region Constructors

        public MpMatcherViewModel() : base(null) { }

        public MpMatcherViewModel(MpMatcherCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpMatcher m) {
            IsBusy = true;

            Matcher = m;
            await Task.Delay(1);

            IsBusy = false;
        }

        public void Register() {
            switch (Matcher.TriggerType) {
                case MpMatchTriggerType.ContentItemAdded:
                    MpClipTrayViewModel.Instance.OnCopyItemItemAdd += OnMatcherTrigggered;
                    break;
                case MpMatchTriggerType.ContentItemAddedToTag:
                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == TriggerActionObjId);
                    if(ttvm != null) {
                        ttvm.OnCopyItemLinked += OnMatcherTrigggered;
                    }
                    break;
                case MpMatchTriggerType.Shortcut:
                    var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == TriggerActionObjId);
                    if(scvm != null) {
                        scvm.OnShortcutExecuted += OnMatcherTrigggered;
                    }
                    break;
                case MpMatchTriggerType.WatchFileChanged:
                case MpMatchTriggerType.WatchFolderChange:
                    Task.Run(async () => {
                        var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(TriggerActionObjId);
                        if(ci != null) {
                            if(ci.Source.App.UserDeviceId == MpPreferences.Instance.ThisUserDevice.Id) {
                                //only add filesystem watchers for this device
                                MpFileSystemWatcher.Instance.AddWatcher(ci.ItemData.ToString(), this);
                            }                            
                        }
                    });
                    break;
                case MpMatchTriggerType.ParentMatchOutput:
                    var pmvm = Parent.Matchers.FirstOrDefault(x => x.MatcherId == ParentMatcherId);
                    if (pmvm != null) {
                        ParentMatcherViewModel = pmvm;
                        ParentMatcherViewModel.OnMatch += OnMatcherTrigggered;
                    }
                    break;
            }
        }

        public void Unegister() {

        }

        #endregion

        

        #region MpIFileSystemWatcher Implementation

        void MpIFileSystemEventHandler.OnFileSystemItemChanged(object sender, FileSystemEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(async () => {
                MpCopyItem ci = null;
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        var app = MpPreferences.Instance.ThisAppSource.App;
                        var source = await MpSource.Create(app, null);
                        ci = await MpCopyItem.Create(source, e.FullPath, MpCopyItemType.FileList, true);
                        break;
                    case WatcherChangeTypes.Renamed:
                        RenamedEventArgs re = e as RenamedEventArgs;
                        ci = await MpDataModelProvider.Instance.GetCopyItemByData(re.OldFullPath);
                        ci.ItemData = re.FullPath;
                        await ci.WriteToDatabaseAsync();
                        break;
                }

                if(ci != null) {
                    OnMatch?.Invoke(this, ci);
                }
            });
        }


        public List<MpMatcherViewModel> FindChildren() {
            var cl = new List<MpMatcherViewModel>();
            foreach (var cttvm in Children.Cast<MpMatcherViewModel>()) {
                cl.Add(cttvm);
                cl.AddRange(cttvm.Children.Cast<MpMatcherViewModel>());
            }
            return cl;
        }
        #endregion

        #region Private Methods

        private void OnMatcherTrigggered(object sender, object e) {
            CheckForMatch(e);
        }


        private void CheckForMatch(object arg) {
            Task.Run(async () => {
                object resultOutput = null;

                switch (Matcher.TriggerActionType) {
                    case MpMatchActionType.Analyze:

                        var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Matcher.TriggerActionObjId);
                        object[] args = new object[] { aipvm, arg as MpCopyItem };
                        aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                        while (aipvm.Parent.IsBusy) {
                            await Task.Delay(100);
                        }

                        resultOutput = aipvm.Parent.LastResultContentItem;
                        if (resultOutput == null) {
                            return;
                        }
                        break;
                }

                switch(Matcher.MatcherType) {
                    case MpMatcherType.Contains:
                        object matchVal = resultOutput.GetPropertyValue(IsMatchPropertyPath);
                        if(matchVal != null) {
                            string compareStr = matchVal.ToString();
                            if (compareStr != null &&
                                compareStr.ToLower().Contains(Matcher.MatchData.ToLower())) {
                                //await PerformIsMatchAction(arg);
                                OnMatch?.Invoke(this, resultOutput);
                            }
                        }
                        
                        break;
                }
            });            
        }



        //private async Task PerformIsMatchAction(object arg) {
        //    switch(Matcher.IsMatchActionType) {
        //        case MpMatchActionType.Classify:
        //            var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == Matcher.IsMatchTargetObjectId);
        //            await ttvm.AddContentItem((arg as MpCopyItem).Id);
        //            break;
        //    }
        //}


        #endregion

    }
}
