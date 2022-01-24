using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpWpfCustomColorChooserMenu : MpICustomColorChooserMenu {
        public ICommand SelectCustomColorCommand => new RelayCommand<object>(
            (args) => {
                var ucvm = args as MpIUserColorViewModel;
                string selectedColor = ucvm.GetColor();
                ShowCustomColorMenu(selectedColor, ucvm);
            });

        public string ShowCustomColorMenu(string selectedColorHexStr, MpIUserColorViewModel ucvm = null) {
            var mw = (MpMainWindow)Application.Current.MainWindow;
            MpMainWindowViewModel.Instance.IsShowingDialog = true;

            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = selectedColorHexStr.ToWinFormsColor();
            cd.CustomColors = MpPreferences.UserCustomColorIdxArray.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToArray();
            cd.FullOpen = cd.CustomColors.Length > 0;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {

                MpMainWindowViewModel.Instance.IsShowingDialog = false;

                MpPreferences.UserCustomColorIdxArray = string.Join(",", cd.CustomColors);

                if (ucvm != null) {
                    ucvm.SetColorCommand.Execute(cd.Color.ToHex());
                }

                return cd.Color.ToSolidColorBrush().ToHex();
            }

            MpMainWindowViewModel.Instance.IsShowingDialog = false;
            return string.Empty;
        }
    }
}
