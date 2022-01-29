using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpCompareActionViewModel : MpActionViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel CompareTypesMenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpCompareType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    MpCompareType ct = (MpCompareType)i;
                    switch (ct) {
                        case MpCompareType.BeginsWith:
                        case MpCompareType.EndsWith:
                        case MpCompareType.Contains:
                            resourceKey = "CaretIcon";
                            break;
                        case MpCompareType.Exact:
                            resourceKey = "BullsEyeIcon";
                            break;
                        case MpCompareType.Regex:
                            resourceKey = "BeakerIcon";
                            break;
                        case MpCompareType.Automatic:
                            resourceKey = "AppendLineIcon";
                            break;
                        case MpCompareType.Wildcard:
                            resourceKey = "AsteriskIcon";
                            break;
                    }
                    amivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = ChangeCompareTypeCommand,
                        CommandParameter = ct,
                        IsVisible = !(ct == MpCompareType.None || ct == MpCompareType.Lexical)
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = amivml
                };
            }
        }
        #endregion

        #region Appearance

        public string CompareTypeLabel {
            get {
                if(Action == null) {
                    return string.Empty;
                }
                return CompareType.EnumToLabel("None");
            } 
        }

        #endregion

        #region Model

        public string SourcePropertyPath {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Arg1;
            }
            set {
                if (SourcePropertyPath != value) {
                    Action.Arg1 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(SourcePropertyPath));
                }
            }
        }

        public string CompareData {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Arg2;
            }
            set {
                if (CompareData != value) {
                    Action.Arg2 = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareData));
                }
            }
        }

        public MpCompareType CompareType {
            get {
                if (Action == null) {
                    return MpCompareType.None;
                }
                if((MpCompareType)ActionObjId == MpCompareType.None) {
                    ActionObjId = (int)MpCompareType.Contains;
                }
                return (MpCompareType)Action.ActionObjId;
            }
            set {
                if (CompareType != value) {
                    Action.ActionObjId = (int)value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CompareType));
                }
            }
        }

        #endregion

        #endregion


        #region Constructors

        public MpCompareActionViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Overrides

        protected override async Task PerformAction(MpCopyItem arg) {
            object matchVal = arg.GetPropertyValue(SourcePropertyPath);
            string compareStr = string.Empty;
            if (matchVal != null) {
                compareStr = matchVal.ToString();
            }

            if (IsMatch(compareStr)) {
                await base.PerformAction(arg);
            }
        }

        #endregion

        #region Private Methods

        private bool IsMatch(string compareStr) {
            switch (CompareType) {
                case MpCompareType.Contains:
                    if (compareStr.ToLower().Contains(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.Exact:
                    if (compareStr.ToLower().Equals(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.BeginsWith:
                    if (compareStr.ToLower().StartsWith(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.EndsWith:
                    if (compareStr.ToLower().EndsWith(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.Regex:
                    var regEx = new Regex(CompareData,
                                            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
                    if (regEx.IsMatch(compareStr)) {
                        return true;
                    }
                    break;
            }
            return false;
        }

        #endregion

        #region Commands

        public ICommand ShowCompareTypeChooserMenuCommand => new RelayCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;
                 var cm = new MpContextMenuView();
                 cm.DataContext = CompareTypesMenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
             });

        public ICommand ChangeCompareTypeCommand => new RelayCommand<object>(
             async(args) => {
                 CompareType = (MpCompareType)args;
                 await Action.WriteToDatabaseAsync();
             });

        #endregion
    }
}
