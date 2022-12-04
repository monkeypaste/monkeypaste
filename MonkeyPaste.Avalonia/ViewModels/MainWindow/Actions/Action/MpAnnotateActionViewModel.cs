
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAnnotateActionViewModel : MpAvActionViewModelBase {
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

        public MpAnnotateActionViewModel(MpAvActionCollectionViewModel parent) : base(parent) {

        }

        #endregion

        #region Protected Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            if(MacroActionType == MpMacroActionType.Tokenize) {
                if(actionInput.CopyItem.ItemType == MpCopyItemType.Image) {

                } else {
                    if(arg is MpAvCompareOutput co) {
                        //var fd = co.CopyItem.ItemData.ToFlowDocument();
                        //var matchRanges = co.Matches.Select(x => new TextRange(
                        //    fd.ContentStart.GetPositionAtOffset(x.Offset),
                        //    fd.ContentStart.GetPositionAtOffset(x.Offset + x.Length)));

                        //foreach(var matchRange in matchRanges) {
                        //    var hl = new Hyperlink(matchRange.Start, matchRange.End);
                        //    hl.IsEnabled = true;
                            
                        //    hl.Unloaded += Hl_Unloaded;
                        //    hl.Click += Hl_Click;
                        //}
                    }
                }
            }

            await Task.Delay(1);
        }

        private void Hl_Click(object sender, EventArgs e) {
            if(MacroCommand == null) {
                MacroCommand = new MpCommand(
                    () => {
                        //MessageBox.Show("SUP YOOO");
                    });
            }
            MacroCommand.Execute(MacroCommandParameter);
        }

        private void Hl_Unloaded(object sender, EventArgs e) {
            //var hl = sender as Hyperlink;
            //hl.Click -= Hl_Click;
            //hl.Unloaded -= Hl_Unloaded;
        }

        #endregion
    }
}
