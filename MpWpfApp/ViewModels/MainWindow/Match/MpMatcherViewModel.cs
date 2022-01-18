using Azure;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MpWpfApp {

    public interface MpIMatchTrigger {
        void RegisterMatcher(MpMatcherViewModel mvm);
        void UnregisterMatcher(MpMatcherViewModel mvm);
        //ObservableCollection<MpMatcherViewModel> Matchers { get; }
    }

    public class MpMatcherViewModel : MpViewModelBase<MpMatcherCollectionViewModel>, MpIFileSystemEventHandler, MpITreeItemViewModel, MpIMatchTrigger {
        #region Private Variables

        private Regex _regEx = null;

        #endregion

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

        public event EventHandler<MpCopyItem> OnMatch;

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

        public void LinkTriggers() {
            switch (Matcher.TriggerType) {
                case MpMatchTriggerType.ContentItemAdded:
                    MpClipTrayViewModel.Instance.RegisterMatcher(this);
                    break;
                case MpMatchTriggerType.ContentItemAddedToTag:
                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == TriggerActionObjId);
                    if(ttvm != null) {
                        ttvm.RegisterMatcher(this);
                    }
                    break;
                case MpMatchTriggerType.Shortcut:
                    var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == TriggerActionObjId);
                    if(scvm != null) {
                        scvm.RegisterMatcher(this);
                    }
                    break;
                case MpMatchTriggerType.WatchFileChanged:
                case MpMatchTriggerType.WatchFolderChange:
                    Task.Run(async () => {
                        var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(TriggerActionObjId);
                        if(ci != null) {
                            if(ci.Source.App.UserDeviceId == MpPreferences.Instance.ThisUserDevice.Id) {
                                //only add filesystem watchers for this device
                                MatchData = ci.ItemData.ToString();
                                MpFileSystemWatcher.Instance.RegisterMatcher(this);
                            }                            
                        }
                    });
                    break;
                case MpMatchTriggerType.ParentMatchOutput:
                    var pmvm = Parent.Matchers.FirstOrDefault(x => x.MatcherId == ParentMatcherId);
                    if (pmvm != null) {
                        ParentMatcherViewModel = pmvm;
                        ParentMatcherViewModel.RegisterMatcher(this);
                    }
                    break;
            }

            if(MatcherType == MpMatcherType.Regex) {
                _regEx = new Regex(
                    MatchData, 
                    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            }
        }

        public void UnlinkTriggers() {

        }

        #endregion

        #region MpIMatchTrigger Implementation

        public void RegisterMatcher(MpMatcherViewModel mvm) {
            OnMatch += mvm.OnMatcherTrigggered;
            MpConsole.WriteLine($"Parent Matcher {Title} Registered {mvm.Title} matcher");
        }

        public void UnregisterMatcher(MpMatcherViewModel mvm) {
            OnMatch -= mvm.OnMatcherTrigggered;
            MpConsole.WriteLine($"Parent Matcher {Title} Unregistered {mvm.Title} from OnCopyItemAdded");
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

        public void OnMatcherTrigggered(object sender, MpCopyItem e) {
            PerformTriggerAction(e);
        }

        private void PerformTriggerAction(MpCopyItem arg) {
            if(arg == null) {
                return;
            }
            Task.Run(async () => {
                switch (TriggerActionType) {
                    case MpMatchActionType.Analyze:
                        var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Matcher.TriggerActionObjId);
                        object[] args = new object[] { aipvm, arg as MpCopyItem };
                        aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                        while (aipvm.Parent.IsBusy) {
                            await Task.Delay(100);
                        }

                        OnMatch?.Invoke(this, aipvm.Parent.LastResultContentItem);
                        break;
                    case MpMatchActionType.Classify:
                        var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x=>x.TagId == Matcher.TriggerActionObjId);
                        await ttvm.AddContentItem((arg as MpCopyItem).Id);
                        OnMatch?.Invoke(this, arg);
                        break;
                    case MpMatchActionType.Compare:
                        // NOTE always case insensitive

                        object matchVal = arg.GetPropertyValue(IsMatchPropertyPath);
                        string compareStr = string.Empty;
                        if (matchVal != null) {
                            compareStr = matchVal.ToString();                            
                        }                       

                        if (IsMatch(compareStr)) {
                            OnMatch?.Invoke(this, arg);
                        }
                        break;
                }
            });            
        }

        private bool IsMatch(string compareStr) {
            switch (MatcherType) {
                case MpMatcherType.Contains:
                    if (compareStr.ToLower().Contains(MatchData.ToLower())) {
                        return true;
                    }
                    break;
                case MpMatcherType.Exact:
                    if (compareStr.ToLower().Equals(MatchData.ToLower())) {
                        return true;
                    }
                    break;
                case MpMatcherType.BeginsWith:
                    if (compareStr.ToLower().StartsWith(MatchData.ToLower())) {
                        return true;
                    }
                    break;
                case MpMatcherType.EndsWith:
                    if (compareStr.ToLower().EndsWith(MatchData.ToLower())) {
                        return true;
                    }
                    break;
                case MpMatcherType.Regex:
                    if (_regEx != null && _regEx.IsMatch(compareStr)) {
                        return true;
                    }
                    break;
            }
            return false;
        }
        #endregion

    }
}
