using MonkeyPaste;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpMatcherViewModel : MpViewModelBase<MpMatcherCollectionViewModel> {

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

        public ObservableCollection<string> MatchActionTypes { get; set; } = new ObservableCollection<string>(typeof(MpMatchActionType).EnumToLabels("Select Match Action"));

        public int SelectedMatchActionTypeIdx {
            get {
                return (int)IsMatchActionType;
            }
            set {
                if ((int)IsMatchActionType != value) {
                    IsMatchActionType = (MpMatchActionType)value;
                    OnPropertyChanged(nameof(SelectedMatchActionTypeIdx));
                }
            }
        }

        #endregion

        #region Model

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

        public MpMatchActionType IsMatchActionType {
            get {
                if (Matcher == null) {
                    return MpMatchActionType.None;
                }
                return Matcher.IsMatchActionType;
            }
            set {
                if (IsMatchActionType != value) {
                    Matcher.IsMatchActionType = value;
                    OnPropertyChanged(nameof(IsMatchActionType));
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

        public MpMatcher Matcher { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpMatcherViewModel() : base(null) { }

        public MpMatcherViewModel(MpMatcherCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpMatcher m) {
            IsBusy = true;

            Matcher = m;

            switch(Matcher.TriggerType) {
                case MpMatchTriggerType.ContentItemAdded:
                    MpClipTrayViewModel.Instance.OnCopyItemItemAdd += Instance_OnCopyItemItemAdd;
                    break;
            }

            await Task.Delay(1);

            IsBusy = false;
        }


        private void CheckForMatch(object arg) {
            Task.Run(async () => {
                MpCopyItem nci = arg as MpCopyItem;

                string compareStr = string.Empty;

                switch (Matcher.TriggerActionType) {
                    case MpMatchActionType.Analyzer:

                        var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(Matcher.TriggerActionObjId);
                        object[] args = new object[] { aipvm, nci };
                        aipvm.Parent.ExecuteAnalysisCommand.Execute(args);

                        while (aipvm.Parent.IsBusy) {
                            await Task.Delay(100);
                        }

                        if (aipvm.Parent.LastTransaction == null) {
                            return;
                        }

                        compareStr = aipvm.Parent.LastTransaction.Response.ToString();
                        break;
                }

                switch(Matcher.MatcherType) {
                    case MpMatcherType.Contains:
                        if(compareStr.ToLower().Contains(Matcher.MatchData.ToLower())) {
                            await PerformIsMatchAction(arg);
                        }
                        break;
                }
            });            
        }

        private async Task PerformIsMatchAction(object arg) {
            switch(Matcher.IsMatchActionType) {
                case MpMatchActionType.Classifier:
                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == Matcher.IsMatchTargetObjectId);
                    await ttvm.AddContentItem((arg as MpCopyItem).Id);
                    break;
            }
        }

        private void Instance_OnCopyItemItemAdd(object sender, object e) {
            CheckForMatch(e);
        }

        #endregion

    }
}
