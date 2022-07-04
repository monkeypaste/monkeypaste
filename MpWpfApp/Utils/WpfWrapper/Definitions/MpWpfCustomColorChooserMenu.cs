using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpWpfCustomColorChooserMenu : MpICustomColorChooserMenu {

        public ICommand SelectCustomColorCommand => new RelayCommand<object>(
            (args) => {
                if(args is MpIUserColorViewModel ucvm) {
                    string selectedColor = ucvm.UserHexColor;
                    ShowCustomColorMenu(selectedColor, ucvm);
                } 
            });

        public string ShowCustomColorMenu(string selectedColorHexStr, MpIUserColorViewModel ucvm = null) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MpMainWindowViewModel.Instance.IsShowingDialog = true;
            MpContextMenuView.Instance.IsShowingChildDialog = true;

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = selectedColorHexStr.ToWinFormsColor();
            cd.CustomColors = MpJsonPreferenceIO.Instance.UserCustomColorIdxArray.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToArray();
            cd.FullOpen = cd.CustomColors.Length > 0;

            string selectedCustomColor = string.Empty;
            var result = cd.ShowDialog();            

            if (result == System.Windows.Forms.DialogResult.OK) {
                MpJsonPreferenceIO.Instance.UserCustomColorIdxArray = string.Join(",", cd.CustomColors);

                if(ucvm != null) {
                    ucvm.UserHexColor = cd.Color.ToHex();
                }

                selectedCustomColor = cd.Color.ToSolidColorBrush().ToHex();
            }

            MpContextMenuView.Instance.IsShowingChildDialog = false;
            MpMainWindowViewModel.Instance.IsShowingDialog = false;

            return selectedCustomColor;
        }
    }
}
