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

    public interface MpITriggerActionViewModel {
        void RegisterTrigger(MpActionViewModelBase mvm);
        void UnregisterTrigger(MpActionViewModelBase mvm);
    }

    public abstract class MpActionViewModelBase : 
        MpViewModelBase<MpActionCollectionViewModel>,
        MpIHoverableViewModel,
        MpISelectableViewModel,
        MpITreeItemViewModel<MpActionViewModelBase>,
        MpIMenuItemViewModel,
        MpITriggerActionViewModel {
        

        #region Properties

        #region View Models

        //public ObservableCollection<string> TriggerTypes { get; set; } = new ObservableCollection<string>(typeof(MpTriggerType).EnumToLabels("Select Trigger"));

        //public int SelectedTriggerTypeIdx {
        //    get {
        //        return (int)TriggerType;
        //    }
        //    set {
        //        if((int)TriggerType != value) {
        //            TriggerType = (MpTriggerType)value;
        //            OnPropertyChanged(nameof(SelectedTriggerTypeIdx));
        //        }
        //    }
        //}

        //public ObservableCollection<string> TriggerActionTypes { get; set; } = new ObservableCollection<string>(typeof(MpActionType).EnumToLabels("Select Trigger Action"));
        
        //public int SelectedTriggerActionTypeIdx {
        //    get {
        //        return (int)ActionType;
        //    }
        //    set {
        //        if ((int)ActionType != value) {
        //            ActionType = (MpActionType)value;
        //            OnPropertyChanged(nameof(SelectedTriggerActionTypeIdx));
        //        }
        //    }
        //}

        public MpActionViewModelBase ParentActionViewModel { get; set; } = null;

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var cmvml = FindChildren();

                return new MpMenuItemViewModel() {
                    Header = Label,
                    IconId = IconId,
                    SubItems = FindChildren().Select(x => x.MenuItemViewModel).ToList()
                };
            }
        }
        
        private ObservableCollection<MpActionViewModelBase> _matcherViewModels;
        public ObservableCollection<MpActionViewModelBase> MatcherViewModels {
            get {
                if(_matcherViewModels == null) {
                    _matcherViewModels = new ObservableCollection<MpActionViewModelBase>();
                }
                if(Parent == null) {
                    return _matcherViewModels;
                }
                //to maintain any collection changed handlers only add/remove matchers if they do/don't exist
                var cmvml = Parent.Items.Where(x => x.ParentActionId == ActionId).ToList();
                foreach(var cmvm in cmvml) {
                    if(!_matcherViewModels.Contains(cmvm)) {
                        _matcherViewModels.Add(cmvm);
                    }
                }
                var matchersToRemove = _matcherViewModels.Where(x => !cmvml.Any(y => y.ActionId == x.ActionId)).ToList();
                for (int i = 0; i < matchersToRemove.Count; i++) {
                    _matcherViewModels.Remove(matchersToRemove[i]);
                }
                return _matcherViewModels;
            }
        }

        #endregion

        #region MpITreeItemViewModel Implementation

        public bool IsExpanded { get; set; }

        public MpActionViewModelBase ParentTreeItem => ParentActionViewModel;

        public IList<MpActionViewModelBase> Children => FindChildren();

        #endregion

        #region MpIUserIcon Implementation

        public async Task<MpIcon> Get() {
            var icon = await MpDb.GetItemAsync<MpIcon>(IconId);
            return icon;
        }

        public async Task Set(MpIcon icon) {
            Action.IconId = icon.Id;
            await Action.WriteToDatabaseAsync();
            OnPropertyChanged(nameof(IconId));
        }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        #endregion

        #region State

        public bool IsRootAction => ParentActionId == 0;

        public bool IsEnabled { get; set; } = false;

        public bool IsEditingDetails { get; set; }

        public bool IsCompareAction => ActionType == MpActionType.Compare;

        public bool IsAnalyzeAction => ActionType == MpActionType.Analyze;
        #endregion

        #region Model

        public bool IsReadOnly {
            get {
                if (Action == null) {
                    return false;
                }
                return Action.IsReadOnly;
            }
            set {
                if (IsReadOnly != value) {
                    Action.IsReadOnly = value;
                    OnPropertyChanged(nameof(IsReadOnly));
                }
            }
        }

        public int ActionObjId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.ActionObjId;
            }
            set {
                if (ActionObjId != value) {
                    Action.ActionObjId = value;
                    OnPropertyChanged(nameof(ActionObjId));
                }
            }
        }



        public MpActionType ActionType {
            get {
                if (Action == null) {
                    return MpActionType.None;
                }
                return Action.ActionType;
            }
            set {
                if (ActionType != value) {
                    Action.ActionType = value;
                    OnPropertyChanged(nameof(ActionType));
                }
            }
        }

        public string Label {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Label;
            }
            set {
                if (Label != value) {
                    Action.Label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }



        public int SortOrderIdx {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.SortOrderIdx;
            }
            set {
                if (SortOrderIdx != value) {
                    Action.SortOrderIdx = value;
                    OnPropertyChanged(nameof(SortOrderIdx));
                }
            }
        }

        public int IconId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.IconId;
            }
        }

        public int ParentActionId {
            get {
                if (Action == null) {
                    return 0;
                }
                return Action.ParentActionId;
            }
            set {
                if(ParentActionId != value) {
                    Action.ParentActionId = value;
                    OnPropertyChanged(nameof(ParentActionId));
                }
            }
        }

        public int ActionId {
            get {
                if(Action == null) {
                    return 0;
                }
                return Action.Id;
            }
        }

        public MpAction Action { get; set; }

        #endregion

        #endregion

        #region Events

        public event EventHandler<MpCopyItem> OnAction;

        #endregion

        #region Constructors

        public MpActionViewModelBase() : base(null) { }

        public MpActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) { 
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpAction m) {
            IsBusy = true;

            Action = m;

            var cml = await MpDataModelProvider.GetChildActions(ActionId);

            foreach(var cm in cml.OrderBy(x=>x.SortOrderIdx)) {
                var dupCheck = Parent.Items.FirstOrDefault(x => x.ActionId == cm.Id);
                if (dupCheck != null) {
                    Parent.Items.Remove(dupCheck);
                }
                var cmvm = await Parent.CreateActionViewModel(cm);
                cmvm.ParentActionViewModel = this;
                Parent.Items.Add(cmvm);
            }

            OnPropertyChanged(nameof(MatcherViewModels));

            IsBusy = false;
        }


        public IList<MpActionViewModelBase> FindChildren() {
            if(Parent == null) {
                return new List<MpActionViewModelBase>();
            }
            var cl = Parent.FindChildren(this);
            cl.Insert(0, this);
            return cl;
        }

        public virtual void Enable() {

            MatcherViewModels.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Enable());

            IsEnabled = true;
        }

        public virtual void Disable() {
            // TODO reverse enable
            MatcherViewModels.OrderBy(x => x.SortOrderIdx).ForEach(x => x.Disable());
            IsEnabled = false;
        }

        public void OnTrigger(object sender, MpCopyItem ci) {
            OnAction?.Invoke(this, ci);
        }
        
        #endregion

        #region MpIMatchTrigger Implementation

        public void RegisterTrigger(MpActionViewModelBase mvm) {
            OnAction += mvm.OnAction;
            MpConsole.WriteLine($"Parent Matcher {Label} Registered {mvm.Label} matcher");
        }

        public void UnregisterTrigger(MpActionViewModelBase mvm) {
            OnAction -= mvm.OnAction;
            MpConsole.WriteLine($"Parent Matcher {Label} Unregistered {mvm.Label} from OnCopyItemAdded");
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if(e is MpAction m && MatcherViewModels.Any(x=>x.ActionId == m.Id)) {
               
            }
        }
        #endregion

        #endregion

        #region Protected Methods


        protected virtual async Task PerformAction(MpCopyItem arg) {
            if (arg == null) {
                return;
            }
            await Task.Delay(1);
            OnAction?.Invoke(this, arg);
        }
        #endregion

        #region Private Methods

        //private void OnTrigger(object sender, MpCopyItem e) {
        //    MpHelpers.RunOnMainThreadAsync(()=>PerformAction(e));
        //}


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

        public ICommand AddChildActionCommand => new RelayCommand(
              () => {
                  Parent.AddActionCommand.Execute(this);
             },Parent != null);
        #endregion
    }
}
