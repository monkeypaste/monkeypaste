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
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpMacroOutput : MpActionOutput {
        public override object OutputData => CommandPresetGuid;

        public string CommandPresetGuid { get; set; }
        public override string ActionDescription {
            get {
                if(string.IsNullOrEmpty(CommandPresetGuid)) {
                    return $"CopyItem({CopyItem.Id},{CopyItem.Title}) did not have criteria for a macro";
                }
                return $"CopyItem({CopyItem.Id},{CopyItem.Title}) was embedded with Analyzer {CommandPresetGuid} ";
            }
        }
    }
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
        }

        #endregion

        #region State 
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

            MpCopyItem outputItem = null;

            if (actionInput.CopyItem.ItemType == MpCopyItemType.Text) {
                outputItem = actionInput.CopyItem;
                var fd = actionInput.CopyItem.ItemData.ToFlowDocument();
                List<MpComparisionMatch> matches = null;

                if (actionInput is MpCompareOutput co) {
                    matches = co.Matches;
                } else {
                    matches = new List<MpComparisionMatch>() { new MpComparisionMatch(fd.ContentStart.ToOffset(),fd.ContentEnd.ToOffset()) };
                }
                int offset = 0;
                foreach (var m in matches.OrderBy(x=>x.Offset)) {
                    // store total doc length before converting range to hyperlink

                    int pre_hl_doc_length = fd.ContentStart.GetOffsetToPosition(fd.ContentEnd);
                    var tr = new TextRange(
                                fd.ContentStart.GetPositionAtOffset(m.Offset + offset),
                                fd.ContentStart.GetPositionAtOffset(m.Offset + m.Length + offset));
                    var hl = new Hyperlink(tr.Start, tr.End) {
                        IsEnabled = true,
                        NavigateUri = new Uri($"http://{SelectedPreset.PresetGuid}")
                    };

                    // get new doc length after adding link
                    int post_hl_doc_length = fd.ContentStart.GetOffsetToPosition(fd.ContentEnd);

                    // since link will skew match ranges adjust ranges by the difference the link created
                    offset += post_hl_doc_length - pre_hl_doc_length;
                }
                var ctvm = MpClipTrayViewModel.Instance.GetClipTileViewModelById(actionInput.CopyItem.Id);
                
                if (ctvm == null) {
                    actionInput.CopyItem.ItemData = fd.ToRichText();
                    await actionInput.CopyItem.WriteToDatabaseAsync();
                    outputItem = actionInput.CopyItem;
                } else {
                    ctvm.CopyItemData = fd.ToRichText();
                    outputItem = ctvm.CopyItem;
                }
            }
            var macroOutput = new MpMacroOutput() {
                Previous = actionInput,
                CopyItem = outputItem == null ? actionInput.CopyItem : outputItem,
                CommandPresetGuid = outputItem == null ? null : SelectedPreset.PresetGuid
            };

            base.PerformAction(macroOutput).FireAndForgetSafeAsync(this);
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
