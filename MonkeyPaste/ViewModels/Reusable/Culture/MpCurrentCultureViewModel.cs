using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MonkeyPaste {
    public class MpCurrentCultureViewModel : MpViewModelBase {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpCurrentCultureViewModel _instance;
        public static MpCurrentCultureViewModel Instance => _instance ?? (_instance = new MpCurrentCultureViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        public ICommand SetLanguageCommand => new MpCommand<object>(
            (args) => {
                string newLanguage = args.ToString();

                //foreach (SettingsProperty dsp in Properties.DefaultUiStrings.Default.Properties) {
                //    foreach (SettingsProperty usp in Properties.UserUiStrings.Default.Properties) {
                //        if (dsp.Name == usp.Name) {
                //            //usp.DefaultValue = await MpLanguageTranslator.TranslateAsync((string)dsp.DefaultValue, newLanguage,"");
                //            MpConsole.WriteLine("Default: " + (string)dsp.DefaultValue + "New: " + (string)usp.DefaultValue);
                //        }
                //    }
                //}

                //Properties.UserUiStrings.Default.Save();
            }, (args) => {
                return args is string;
            });
        #endregion
    }
}
