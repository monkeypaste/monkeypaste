using Azure;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {

    public interface MpIMatcherTriggerViewModel {
        void RegisterMatcher(MpMatcherViewModel mvm);
        void UnregisterMatcher(MpMatcherViewModel mvm);

        ObservableCollection<MpMatcherViewModel> MatcherViewModels { get; }
    }

    public class MpMatcherViewModel : 
        MpViewModelBase<MpMatcherCollectionViewModel>, 
        MpIFileSystemEventHandler, 
        MpITreeItemViewModel,
        MpIMenuItemViewModel,
        MpIMatcherTriggerViewModel {
        #region Private Variables

        private Regex _regEx = null;

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<string> TriggerTypes { get; set; } = new ObservableCollection<string>(typeof(MpMatcherTriggerType).EnumToLabels("Select Trigger"));

        public int SelectedTriggerTypeIdx {
            get {
                return (int)TriggerType;
            }
            set {
                if((int)TriggerType != value) {
                    TriggerType = (MpMatcherTriggerType)value;
                    OnPropertyChanged(nameof(SelectedTriggerTypeIdx));
                }
            }
        }

        public ObservableCollection<string> TriggerActionTypes { get; set; } = new ObservableCollection<string>(typeof(MpMatcherActionType).EnumToLabels("Select Trigger Action"));
        
        public int SelectedTriggerActionTypeIdx {
            get {
                return (int)TriggerActionType;
            }
            set {
                if ((int)TriggerActionType != value) {
                    TriggerActionType = (MpMatcherActionType)value;
                    OnPropertyChanged(nameof(SelectedTriggerActionTypeIdx));
                }
            }
        }

        public MpMatcherViewModel ParentMatcherViewModel { get; set; } = null;

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var cmvml = FindChildren();

                return new MpMenuItemViewModel() {
                    Header = Title,
                    IconId = IconId,
                    SubItems = FindChildren().Select(x => x.MenuItemViewModel).ToList()
                };
            }
        }
        
        private ObservableCollection<MpMatcherViewModel> _matcherViewModels;
        public ObservableCollection<MpMatcherViewModel> MatcherViewModels {
            get {
                if(_matcherViewModels == null) {
                    _matcherViewModels = new ObservableCollection<MpMatcherViewModel>();
                }
                if(Parent == null) {
                    return _matcherViewModels;
                }
                //to maintain any collection changed handlers only add/remove matchers if they do/don't exist
                var cmvml = Parent.Matchers.Where(x => x.ParentMatcherId == MatcherId).ToList();
                foreach(var cmvm in cmvml) {
                    if(!_matcherViewModels.Contains(cmvm)) {
                        _matcherViewModels.Add(cmvm);
                    }
                }
                var matchersToRemove = _matcherViewModels.Where(x => !cmvml.Any(y => y.MatcherId == x.MatcherId)).ToList();
                for (int i = 0; i < matchersToRemove.Count; i++) {
                    _matcherViewModels.Remove(matchersToRemove[i]);
                }
                return _matcherViewModels;
            }
        }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsSelected { get; set; }
        public bool IsHovering { get; set; }
        public bool IsExpanded { get; set; }

        public MpITreeItemViewModel ParentTreeItem => ParentMatcherViewModel;

        public ObservableCollection<MpITreeItemViewModel> Children => 
                        new ObservableCollection<MpITreeItemViewModel>(MatcherViewModels.Cast<MpITreeItemViewModel>());

        #endregion

        #region MpIUserIcon Implementation

        public async Task<MpIcon> Get() {
            var icon = await MpDb.GetItemAsync<MpIcon>(IconId);
            return icon;
        }

        public async Task Set(MpIcon icon) {
            Matcher.IconId = icon.Id;
            await Matcher.WriteToDatabaseAsync();
            OnPropertyChanged(nameof(IconId));
        }

        #endregion

        #region State

        public bool IsRootMatcher => ParentMatcherId == 0;

        public bool IsEnabled { get; set; } = false;

        #endregion

        #region Model

        public bool IsReadOnly {
            get {
                if (Matcher == null) {
                    return false;
                }
                return Matcher.IsReadOnly;
            }
            set {
                if (IsReadOnly != value) {
                    Matcher.IsReadOnly = value;
                    OnPropertyChanged(nameof(IsReadOnly));
                }
            }
        }

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

        public MpMatcherTriggerType TriggerType {
            get {
                if(Matcher == null) {
                    return MpMatcherTriggerType.None;
                }
                return Matcher.MatcherTriggerType;
            }
            set {
                if(TriggerType != value) {
                    Matcher.MatcherTriggerType = value;
                    OnPropertyChanged(nameof(TriggerType));
                }
            }
        }

        public MpMatcherActionType TriggerActionType {
            get {
                if (Matcher == null) {
                    return MpMatcherActionType.None;
                }
                return Matcher.MatcherActionType;
            }
            set {
                if (TriggerActionType != value) {
                    Matcher.MatcherActionType = value;
                    OnPropertyChanged(nameof(TriggerActionType));
                }
            }
        }

        public string Title {
            get {
                if (Matcher == null) {
                    return null;
                }
                return Matcher.Label;
            }
            set {
                if (MatchData != value) {
                    Matcher.Label = value;
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

            var cml = await MpDataModelProvider.GetChildMatchers(MatcherId);

            foreach(var cm in cml.OrderBy(x=>x.SortOrderIdx)) {
                var dupCheck = Parent.Matchers.FirstOrDefault(x => x.MatcherId == cm.Id);
                if (dupCheck != null) {
                    Parent.Matchers.Remove(dupCheck);
                }
                var cmvm = await Parent.CreateMatcherViewModel(cm);
                cmvm.ParentMatcherViewModel = this;
                Parent.Matchers.Add(cmvm);
            }

            OnPropertyChanged(nameof(MatcherViewModels));

            IsBusy = false;
        }

        public void Enable() {
            switch (Matcher.MatcherTriggerType) {
                case MpMatcherTriggerType.ContentItemAdded:
                    MpClipTrayViewModel.Instance.RegisterMatcher(this);
                    break;
                case MpMatcherTriggerType.ContentItemAddedToTag:
                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == TriggerActionObjId);
                    if(ttvm != null) {
                        ttvm.RegisterMatcher(this);
                    }
                    break;
                case MpMatcherTriggerType.Shortcut:
                    var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == TriggerActionObjId);
                    if(scvm != null) {
                        scvm.RegisterMatcher(this);
                    }
                    break;
                case MpMatcherTriggerType.WatchFileChanged:
                case MpMatcherTriggerType.WatchFolderChange:
                    Task.Run(async () => {
                        var ci = await MpDb.GetItemAsync<MpCopyItem>(TriggerActionObjId);
                        if(ci != null) {
                            if(ci.Source.App.UserDeviceId == MpPreferences.ThisUserDevice.Id) {
                                //only add filesystem watchers for this device
                                MatchData = ci.ItemData.ToString();
                                MpFileSystemWatcherViewModel.Instance.RegisterMatcher(this);
                            }                            
                        }
                    });
                    break;
                case MpMatcherTriggerType.ParentMatchOutput:
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

            MatcherViewModels.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Enable());

            IsEnabled = true;
        }

        public void Disable() {
            // TODO reverse enable
            MatcherViewModels.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Disable());
            IsEnabled = false;
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
            MpHelpers.RunOnMainThread(async () => {
                MpCopyItem ci = null;
                switch (e.ChangeType) {
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.Created:
                        var app = MpPreferences.ThisAppSource.App;
                        var source = await MpSource.Create(app, null);
                        ci = await MpCopyItem.Create(source, e.FullPath, MpCopyItemType.FileList, true);
                        break;
                    case WatcherChangeTypes.Renamed:
                        RenamedEventArgs re = e as RenamedEventArgs;
                        ci = await MpDataModelProvider.GetCopyItemByData(re.OldFullPath);
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

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpMatcher m && MatcherViewModels.Any(x=>x.MatcherId == m.Id)) {
               
            }
        }
        #endregion

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
                    case MpMatcherActionType.Analyze:
                        var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Matcher.TriggerActionObjId);
                        object[] args = new object[] { aipvm, arg as MpCopyItem };
                        aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                        while (aipvm.Parent.IsBusy) {
                            await Task.Delay(100);
                        }

                        OnMatch?.Invoke(this, aipvm.Parent.LastResultContentItem);
                        break;
                    case MpMatcherActionType.Classify:
                        var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x=>x.TagId == Matcher.TriggerActionObjId);
                        await ttvm.AddContentItem((arg as MpCopyItem).Id);
                        OnMatch?.Invoke(this, arg);
                        break;
                    case MpMatcherActionType.Compare:
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

        #region Commands

        public ICommand ToggleIsEnabledCommand => new RelayCommand(
             () => {
                if(IsEnabled) {
                     Disable();
                } else {
                     Enable();
                 }
            });

        public ICommand AddChildMatcherCommand => new RelayCommand(
              () => {
                  Parent.AddMatcherCommand.Execute(this);
             },Parent != null);
        #endregion
    }
}
