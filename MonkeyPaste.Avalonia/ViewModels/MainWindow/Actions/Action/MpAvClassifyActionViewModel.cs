using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;

namespace MonkeyPaste.Avalonia {
    public class MpAvClassifyActionViewModel : 
        MpAvActionViewModelBase,  
        MpIPopupSelectorMenu {
        #region Private Variables

        #endregion

        #region MpIPopupSelectorMenu Implementation


        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        MpAvTagTrayViewModel.Instance.AllTagViewModel.GetTagMenu(SelectTagCommand,new int[] { TagId}, true)
                    }
                };
            }
        }
        public MpMenuItemViewModel SelectedMenuItem =>
            SelectedTag == null ? null : SelectedTag.GetTagMenu(null, new int[] { TagId }, false);
        public string EmptyText => "Select Tag...";
        public object EmptyIconResourceObj => MpAvActionViewModelBase.GetDefaultActionIconResourceKey(MpActionType.Classify, null);
        #endregion

        #region Properties

        #region View Models

        public MpAvTagTileViewModel SelectedTag {
            get {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            }
            set {
                if (SelectedTag != value) {
                    TagId = value == null ? 0 : value.TagId;
                    OnPropertyChanged(nameof(SelectedTag));
                }
            }
        }
        #endregion

        #region Model

        public int TagId {
            get {
                if (Action == null || string.IsNullOrEmpty(Arg1)) {
                    return 0;
                }
                return int.Parse(Arg1);
            }
            set {
                if (TagId != value) {
                    Arg1 = value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvClassifyActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
        }

        #endregion

        #region Protected Overrides
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(ValidateActionAsync);
            }
        }
        protected override async Task ValidateActionAsync() {
            await Task.Delay(1);
            if(TagId == 0) {
                ValidationText = $"No Collection selected for Classifier '{FullName}'";
            } else {
                //while (MpAvTagTrayViewModel.Instance.IsAnyBusy) {
                //    await Task.Delay(100);
                //}
                //var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);

                if (SelectedTag == null) {
                    ValidationText = $"Collection for Classifier '{FullName}' not found";
                } else {
                    ValidationText = string.Empty;
                }
            }
            if(!IsValid) {
                ShowValidationNotification();
            }
        }


        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if(ttvm != null && actionInput != null && actionInput.CopyItem != null) {
                ttvm.LinkCopyItemCommand.Execute(actionInput.CopyItem.Id);
            }

            await base.PerformActionAsync(new MpAvClassifyOutput() {
                Previous = arg as MpAvActionOutput,
                CopyItem = actionInput.CopyItem,
                TagId = TagId
            });
        }

        #endregion

        #region Commands

        public ICommand SelectTagCommand => new MpCommand<object>(
            (args) => {
                if(args is int tagId) {
                    if(TagId == tagId) {
                        TagId = 0;
                    } else {
                        TagId = tagId;
                    }
                    
                    OnPropertyChanged(nameof(SelectedTag));
                    OnPropertyChanged(nameof(SelectedMenuItem));
                }
            });


        #endregion
    }
}
