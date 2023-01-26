using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Atk;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Gtk;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutRecorderParameterViewModel : 
        MpAvParameterViewModelBase, MpAvIShortcutCommandViewModel {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        #endregion

        #region Appearance
        #endregion

        #region MpAvIShortcutCommandViewModel Implementation

        public ICommand AssignCommand => new MpAsyncCommand(
            async () => {

                ICommand cmd = null;
                object cmdParam = null;
                if(Parent is MpAvShortcutTriggerViewModel sctvm) {
                    cmd = MpAvTriggerCollectionViewModel.Instance.InvokeActionCommand;
                    cmdParam = sctvm.ActionId;
                }
                await MpAvShortcutCollectionViewModel.Instance.RegisterViewModelShortcutAsync(
                    shortcutType: MpShortcutType.InvokeAction,
                    title: $"Trigger {Label} Action",
                    command: cmd,
                    commandParameter: cmdParam.ToString(),
                    keys: ShortcutKeyString);

                OnPropertyChanged(nameof(ShortcutViewModel));

                OnPropertyChanged(nameof(ShortcutKeyString));


                if (ShortcutViewModel != null) {
                    ShortcutViewModel.OnPropertyChanged(nameof(ShortcutViewModel.KeyItems));
                }
            });

        public MpShortcutType ShortcutType {
            get {
                if (Parent is MpAvShortcutTriggerViewModel sctvm) {
                    return MpShortcutType.InvokeAction;
                }
                return MpShortcutType.None;
            }
        }
        public MpAvShortcutViewModel ShortcutViewModel {
            get {
                if (Parent is MpAvShortcutTriggerViewModel sctvm) {
                    return
                        MpAvShortcutCollectionViewModel.Instance.Items
                            .FirstOrDefault(x => x.CommandParameter == sctvm.ActionId.ToString() && x.ShortcutType == MpShortcutType.InvokeAction);
                }
                return null;
            }
        }
        public string ShortcutKeyString => ShortcutViewModel == null ? string.Empty : ShortcutViewModel.KeyString;

        #endregion

        #region Model

        #endregion        

        #endregion

        #region Constructors

        public MpAvShortcutRecorderParameterViewModel() : base(null) { }

        public MpAvShortcutRecorderParameterViewModel(MpIParameterHostViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            OnPropertyChanged(nameof(CurrentValue));     
            if(this is MpISliderViewModel svm) {
                svm.OnPropertyChanged(nameof(svm.MinValue));
                svm.OnPropertyChanged(nameof(svm.MaxValue));
                svm.OnPropertyChanged(nameof(svm.SliderValue));

            }
            
            await Task.Delay(1);

            IsBusy = false;
        }





        #endregion
    }
}
