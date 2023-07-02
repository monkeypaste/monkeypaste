using Avalonia.Controls;
using MonkeyPaste.Common;
using System;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvApplicationCommand : MpViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvApplicationCommand _instance;
        public static MpAvApplicationCommand Instance => _instance ?? (_instance = new MpAvApplicationCommand());
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        private MpAvApplicationCommand() { }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods



        #endregion

        #region Commands

        public ICommand RenameCommand => new MpCommand(
            () => {
                var fc = MpAvFocusManager.Instance.FocusElement as Control;
                if (fc.TryGetSelfOrAncestorDataContext<MpAvTagTileViewModel>(out var ttvm)) {
                    ttvm.RenameTagCommand.Execute(null);
                    return;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var ctvm)) {
                    MpAvClipTrayViewModel.Instance.EditSelectedTitleCommand.Execute(null);
                    return;
                }
            },
            () => {

                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return false;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvTagTileViewModel>(out var ttvm)) {
                    return ttvm.RenameTagCommand.CanExecute(null);
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var ctvm)) {
                    return MpAvClipTrayViewModel.Instance.EditSelectedTitleCommand.CanExecute(null);
                }
                return false;
            });

        public ICommand OpenPopoutCommand => new MpCommand(
             () => {
                 var fc = MpAvFocusManager.Instance.FocusElement as Control;
                 if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                     MpAvClipTrayViewModel.Instance.OpenSelectedTileInWindowCommand.Execute(null);
                     return;
                 }

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvSearchBoxViewModel>(out _) ||
                     fc.TryGetSelfOrAncestorDataContext<MpAvSearchCriteriaItemCollectionViewModel>(out _)) {
                     MpAvSearchCriteriaItemCollectionViewModel.Instance.OpenCriteriaWindowCommand.Execute(null);
                     return;
                 }

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvTriggerCollectionViewModel>(out _)) {
                     MpAvTriggerCollectionViewModel.Instance.ShowDesignerWindowCommand.Execute(null);
                     return;
                 }
             },
            () => {

                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return false;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                    return MpAvClipTrayViewModel.Instance.OpenSelectedTileInWindowCommand.CanExecute(null);
                }

                if (fc.TryGetSelfOrAncestorDataContext<MpAvSearchBoxViewModel>(out _) ||
                    fc.TryGetSelfOrAncestorDataContext<MpAvSearchCriteriaItemCollectionViewModel>(out _)) {
                    return MpAvSearchCriteriaItemCollectionViewModel.Instance.OpenCriteriaWindowCommand.CanExecute(null);
                }

                if (fc.TryGetSelfOrAncestorDataContext<MpAvTriggerCollectionViewModel>(out _)) {
                    return MpAvTriggerCollectionViewModel.Instance.ShowDesignerWindowCommand.CanExecute(null);
                }
                return false;
            });

        public ICommand DecreaseFocusCommand => new MpCommand(
            () => {
                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    MpAvMainWindowViewModel.Instance.HideMainWindowCommand.Execute(null);
                    return;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var ctvm)) {
                    if (fc is TextBox && !ctvm.IsTitleReadOnly) {
                        ctvm.FinishEditTitleCommand.Execute(null);
                        return;
                    }
                    if (ctvm.IsSubSelectionEnabled) {
                        if (ctvm.IsContentReadOnly) {
                            ctvm.DisableSubSelectionCommand.Execute(null);
                            return;
                        }
                        ctvm.EnableContentReadOnlyCommand.Execute(null);
                        return;
                    }
                }

                MpAvMainWindowViewModel.Instance.HideMainWindowCommand.Execute(null);
            }, () => {
                return MpAvWindowManager.ActiveWindow != null;
            });
        #endregion
    }
}
