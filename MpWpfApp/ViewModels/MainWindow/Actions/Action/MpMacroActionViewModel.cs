using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace MpWpfApp {

    public class MpMacroActionViewModel : MpActionViewModelBase {
        #region Properties

        #region View Models

        public MpAnalyticItemPresetViewModel SelectedPreset {
            get {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == AnalyticItemPresetId);
            }
            //set {
            //    if (SelectedPreset != value) {
            //        AnalyticItemPresetId = value.AnalyticItemPresetId;
            //        // NOTE dont understand why but HasModelChanged property change not firing from this
            //        // so forcing db write for now..
            //        Task.Run(async () => { await Action.WriteToDatabaseAsync(); });
            //        OnPropertyChanged(nameof(SelectedPreset));
            //    }
            //}
        }

        #endregion

        #region State 

        public bool IsRemoteCommand => CommandType == MpMacroCommandType.Remote;

        public bool IsLocalCommand => CommandType == MpMacroCommandType.Local;

        #endregion

        #region Model

        //Arg2
        public int AnalyticItemPresetId {
            get {
                if (Action == null) {
                    return 0;
                }
                return ActionObjId;
            }
            set {
                if (AnalyticItemPresetId != value) {
                    ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AnalyticItemPresetId));
                    OnPropertyChanged(nameof(SelectedPreset));
                }
            }
        }

        //Arg1
        public MpMacroCommandType CommandType {
            get {
                if (Action == null) {
                    return MpMacroCommandType.None;
                }
                if (string.IsNullOrWhiteSpace(Arg1)) {
                    return MpMacroCommandType.None;
                }

                return Arg1.ToEnum<MpMacroCommandType>();
            }
            set {
                if (CommandType != value) {
                    Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CommandType));
                }
            }
        }

        public ICommand MacroCommand { get; set; } 

        public object MacroCommandParameter { get; set; }

        public MpMacroActionType MacroActionType {
            get {
                if (Action == null) {
                    return MpMacroActionType.None;
                }
                if (string.IsNullOrWhiteSpace(Action.Arg1)) {
                    return MpMacroActionType.None;
                }
                return (MpMacroActionType)Convert.ToInt32(Action.Arg1);
            }
            set {
                if (MacroActionType != value) {
                    Action.Arg1 = ((int)value).ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(MacroActionType));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpMacroActionViewModel(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpMacroActionViewModel_PropertyChanged;
        }

        #endregion

        #region Protected Overrides

        public override async Task PerformAction(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            if (actionInput.CopyItem.ItemType == MpCopyItemType.Text) {
                if (arg is MpCompareOutput co) {
                    var fd = co.CopyItem.ItemData.ToFlowDocument();
                    var matchRanges = co.Matches.Select(x => new TextRange(
                        fd.ContentStart.GetPositionAtOffset(x.Offset),
                        fd.ContentStart.GetPositionAtOffset(x.Offset + x.Length)));

                    int offset = 0;
                    foreach(var m in co.Matches) {
                        int pre_hl_doc_length = fd.ContentStart.GetOffsetToPosition(fd.ContentEnd);
                        var tr = new TextRange(
                                    fd.ContentStart.GetPositionAtOffset(m.Offset + offset),
                                    fd.ContentStart.GetPositionAtOffset(m.Offset + m.Length + offset));
                        var hl = new Hyperlink(tr.Start, tr.End) {
                            IsEnabled = true,
                            NavigateUri = new Uri($"http://{SelectedPreset.PresetGuid}")
                        };
                        int post_hl_doc_length = fd.ContentStart.GetOffsetToPosition(fd.ContentEnd);

                        offset += post_hl_doc_length - pre_hl_doc_length;
                    }

                    var ctvm = MpClipTrayViewModel.Instance.GetClipTileViewModelById(co.CopyItem.Id);
                    if (ctvm == null) {
                        co.CopyItem.ItemData = fd.ToRichText();
                        await co.CopyItem.WriteToDatabaseAsync();

                        return;
                    }
                    ctvm.CopyItemData = fd.ToRichText();
                }
            }
            await Task.Delay(1);
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAnalyticItemPreset aip && aip.Id == AnalyticItemPresetId) {
                Task.Run(Validate);
            }
        }

        protected override async Task<bool> Validate() {
            await base.Validate();
            if (!IsValid) {
                return IsValid;
            }

            var aipvm = MpAnalyticItemCollectionViewModel.Instance.GetPresetViewModelById(AnalyticItemPresetId);
            if (aipvm == null) {
                ValidationText = $"Analyzer for Action '{FullName}' not found";
                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }
        #endregion

        #region Private Methods
        private void MpMacroActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    OnPropertyChanged(nameof(SelectedPreset));
                    break;
                case nameof(CommandType):
                    if (SelectedPreset != null) {
                        // if user changes the command type delete the preset vm that was associated
                        SelectedPreset.Parent.DeletePresetCommand.Execute(SelectedPreset);
                        ActionObjId = 0;
                        OnPropertyChanged(nameof(SelectedPreset));
                    }

                    MpAnalyticItemViewModel automationAnalyzer = null;

                    switch (CommandType) {
                        case MpMacroCommandType.None:
                            return;
                        case MpMacroCommandType.Local:
                            automationAnalyzer = MpAnalyticItemCollectionViewModel.Instance.ProcessAutomationViewModel;
                            break;
                        case MpMacroCommandType.Remote:
                            automationAnalyzer = MpAnalyticItemCollectionViewModel.Instance.HttpAutomationViewModel;
                            break;
                    }
                    if(automationAnalyzer != null) {
                        automationAnalyzer.CreateNewPresetCommand.Execute(true);

                        MpHelpers.RunOnMainThread(async () => {
                            while (automationAnalyzer.IsBusy) {
                                await Task.Delay(100);
                            }

                            ActionObjId = automationAnalyzer.SelectedItem.AnalyticItemPresetId;
                            OnPropertyChanged(nameof(SelectedPreset));
                        });
                    }
                    
                    break;
            }
        }
        #endregion
    }
}
