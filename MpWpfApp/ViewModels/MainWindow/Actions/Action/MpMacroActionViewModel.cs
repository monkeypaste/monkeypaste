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


        #endregion

        #region Model

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

        }

        #endregion

        #region Protected Overrides

        public override async Task PerformAction(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            MpCopyItem ci = null;
            if (arg is MpCopyItem) {
                ci = arg as MpCopyItem;
            } else if (arg is MpCompareOutput co) {
                ci = co.CopyItem;
            } else if (arg is MpAnalyzeOutput ao) {
                ci = ao.CopyItem;
            } else if (arg is MpClassifyOutput clo) {
                ci = clo.CopyItem;
            }

            if(MacroActionType == MpMacroActionType.Tokenize) {
                if(ci.ItemType == MpCopyItemType.Image) {

                } else {
                    if(arg is MpCompareOutput co) {
                        var fd = co.CopyItem.ItemData.ToFlowDocument();
                        var matchRanges = await MpHelpers.FindStringRangesFromPositionAsync(
                             position: fd.ContentStart,
                             matchStr: co.MatchValue,
                             isCaseSensitive: co.IsCaseSensitive,
                             ct: MpActionCollectionViewModel.CTS.Token);

                        foreach(var matchRange in matchRanges) {
                            var hl = new Hyperlink(matchRange.Start, matchRange.End);
                            hl.IsEnabled = true;
                            
                            hl.Unloaded += Hl_Unloaded;
                            hl.Click += Hl_Click;
                        }
                    }
                }
            }

            await Task.Delay(1);
        }

        private void Hl_Click(object sender, System.Windows.RoutedEventArgs e) {
            if(MacroCommand == null) {
                MacroCommand = new RelayCommand(
                    () => {
                        MessageBox.Show("SUP YOOO");
                    });
            }
            MacroCommand.Execute(MacroCommandParameter);
        }

        private void Hl_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            var hl = sender as Hyperlink;
            hl.Click -= Hl_Click;
            hl.Unloaded -= Hl_Unloaded;
        }

        #endregion
    }
}
