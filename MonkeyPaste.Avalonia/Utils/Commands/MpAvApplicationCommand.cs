using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvApplicationCommand : MpAvViewModelBase {
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

        public ICommand BackNavCommand => new MpCommand(
            () => {
                if (App.PrimaryView is MpAvSettingsView) {
                    MpAvSettingsViewModel.Instance.CloseSettingsCommand.Execute(null);
                    App.SetPrimaryView(MpAvMainView.Instance);
                }
            }, () => {
                return App.PrimaryView is not MpAvMainView;
            });

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

        public ICommand AssignHotkeyFromShortcutCommand => new MpCommand(
            () => {
                MpIShortcutCommandViewModel focus_vm = null;

                var fc = MpAvFocusManager.Instance.FocusElement as Control;
                if (fc.TryGetSelfOrAncestorDataContext<MpAvTagTileViewModel>(out var ttvm)) {
                    focus_vm = ttvm;
                } else if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var ctvm)) {
                    focus_vm = ctvm;
                } else if (fc.TryGetSelfOrAncestorDataContext<MpAvAnalyticItemPresetViewModel>(out var aipvm)) {
                    focus_vm = aipvm;
                }
                if (focus_vm == null) {
                    return;
                }
                MpAvShortcutCollectionViewModel.Instance.ShowAssignShortcutDialogCommand.Execute(focus_vm);
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

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvTextBoxParameterViewModel>(out var tbpvm)) {
                     tbpvm.OpenPopOutWindowCommand.Execute(null);
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
                if (fc.TryGetSelfOrAncestorDataContext<MpAvTextBoxParameterViewModel>(out var tbpvm)) {
                    return tbpvm.OpenPopOutWindowCommand.CanExecute(null);
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

        public ICommand CopySelectionCommand => new MpCommand(
             () => {
                 var fc = MpAvFocusManager.Instance.FocusElement as Control;
                 if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                     MpAvClipTrayViewModel.Instance.CopySelectedClipFromShortcutCommand.Execute(null);
                     return;
                 }

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                     avm.CopyActionCommand.Execute(null);
                     return;
                 }
             },
            () => {

                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return false;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                    return MpAvClipTrayViewModel.Instance.CopySelectedClipFromShortcutCommand.CanExecute(null);
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                    return avm.CopyActionCommand.CanExecute(null);
                }
                return false;
            });

        public ICommand CutSelectionCommand => new MpCommand(
             () => {
                 var fc = MpAvFocusManager.Instance.FocusElement as Control;
                 if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                     MpAvClipTrayViewModel.Instance.CutSelectionFromContextMenuCommand.Execute(null);
                     return;
                 }

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                     avm.CutActionCommand.Execute(null);
                     return;
                 }
             },
            () => {

                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return false;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                    return MpAvClipTrayViewModel.Instance.CutSelectionFromContextMenuCommand.CanExecute(null);
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                    return avm.CutActionCommand.CanExecute(null);
                }
                return false;
            });
        public ICommand PasteSelectionCommand => new MpCommand(
             () => {
                 var fc = MpAvFocusManager.Instance.FocusElement as Control;
                 if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                     MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand.Execute(null);
                     return;
                 }

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                     avm.PasteActionCommand.Execute(null);
                     return;
                 }
             },
            () => {

                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return false;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                    return MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand.CanExecute(null);
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                    return avm.PasteActionCommand.CanExecute(null);
                }
                return false;
            });

        public ICommand DecraseFocusCommand => new MpCommand(
             () => {
                 var fc = MpAvFocusManager.Instance.FocusElement as Control;
                 if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                     MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand.Execute(null);
                     return;
                 }

                 if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                     avm.PasteActionCommand.Execute(null);
                     return;
                 }
             },
            () => {

                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return false;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out _)) {
                    return MpAvClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand.CanExecute(null);
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvActionViewModelBase>(out var avm)) {
                    return avm.PasteActionCommand.CanExecute(null);
                }
                return false;
            });

        public ICommand DecreaseFocusCommand => new MpCommand(
            () => {
                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    MpAvMainWindowViewModel.Instance.HideMainWindowCommand.Execute(MpMainWindowHideType.Force);
                    return;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var ctvm)) {
                    // clip tile control focus
                    if (fc is TextBox && !ctvm.IsTitleReadOnly) {
                        // finish title edit
                        ctvm.FinishEditTitleCommand.Execute(null);
                        return;
                    }
                    if (ctvm.IsSubSelectionEnabled) {
                        if (ctvm.IsContentReadOnly) {
                            if (ctvm.DisableSubSelectionCommand.CanExecute(null)) {
                                // disable sub selection
                                ctvm.DisableSubSelectionCommand.Execute(null);
                                if (fc is MpAvContentTextBox ctb) {
                                    ctb.TryKillFocusAsync().FireAndForgetSafeAsync();
                                }
                                return;
                            }
                            if (ctvm.IsAppendNotifier) {
                                // prompt to end appending
                                MpAvClipTrayViewModel.Instance.DeactivateAppendModeCommand.Execute(null);
                                return;
                            }
                        }
                        // enable read only
                        ctvm.EnableContentReadOnlyCommand.Execute(null);
                        return;
                    }
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvSearchBoxViewModel>(out var sbvm)) {
                    // search box control focus
                    if (fc is TextBox || fc is AutoCompleteBox) {
                        if (fc.GetVisualAncestor<MpAvSearchBoxView>() is MpAvSearchBoxView sbv &&
                            sbv.FindControl<MpAvClearTextButton>("ClearSearchTextButton") is MpAvClearTextButton ctb) {
                            // move focus to clear text button
                            // (user can then press to clear or escape again to focus mw)
                            ctb.TrySetFocusAsync().FireAndForgetSafeAsync();
                            return;
                        }
                    }
                    if (fc.GetVisualAncestor<MpAvClearTextButton>() is MpAvClearTextButton focus_ctb &&
                        focus_ctb.DataContext is MpAvSearchBoxViewModel &&
                        TopLevel.GetTopLevel(fc) is MpAvWindow w) {
                        // searchbox clear text button is focus, move focus to mw
                        w.TrySetFocusAsync().FireAndForgetSafeAsync();
                        return;
                    }
                }
                if (fc.GetVisualAncestor<MpAvWindow>() is { } fc_w &&
                    fc_w is not MpAvNotificationWindow &&
                    fc_w is not MpAvMainWindow) {
                    // minimize window
                    fc_w.WindowState = WindowState.Minimized;
                    return;
                }
                // attempt to hide main window
                MpAvMainWindowViewModel.Instance.HideMainWindowCommand.Execute(MpMainWindowHideType.Force);
            }, () => {
                return MpAvWindowManager.IsAnyActive || MpAvMainWindowViewModel.Instance.IsMainWindowOpen;
            });

        public ICommand IncreaseFocusCommand => new MpCommand(
            () => {
                if (MpAvFocusManager.Instance.FocusElement is not Control fc) {
                    return;
                }
                if (fc.TryGetSelfOrAncestorDataContext<MpAvClipTileViewModel>(out var ctvm)) {
                    // clip tile control focus
                    if (!ctvm.IsTitleReadOnly) {
                        // ignore when editing title
                        return;
                    }
                    if (ctvm.IsSubSelectionEnabled) {
                        if (ctvm.IsContentReadOnly) {
                            // disable read only
                            ctvm.DisableContentReadOnlyCommand.Execute(null);
                            return;
                        }
                    } else {
                        // enable sub-selection
                        ctvm.EnableSubSelectionCommand.Execute(null);
                        return;
                    }
                }
            }, () => {
                return MpAvWindowManager.IsAnyActive;
            });

        public ICommand ZoomInSelectionCommand => new MpCommand(
             () => {

                 if (MpAvFocusManager.Instance.FocusElement is Control fc &&
                    fc.TryGetSelfOrAncestorDataContext<MpIZoomFactorViewModel>(out var zfvm)) {
                     zfvm.ZoomInCommand.Execute(null);
                     return;
                 }
             });

        public ICommand ZoomOutSelectionCommand => new MpCommand(
             () => {
                 if (MpAvFocusManager.Instance.FocusElement is Control fc &&
                    fc.TryGetSelfOrAncestorDataContext<MpIZoomFactorViewModel>(out var zfvm)) {
                     zfvm.ZoomOutCommand.Execute(null);
                     return;
                 }
             });

        public ICommand ResetSelectionZoomCommand => new MpCommand(
             () => {
                 if (MpAvFocusManager.Instance.FocusElement is Control fc &&
                    fc.TryGetSelfOrAncestorDataContext<MpIZoomFactorViewModel>(out var zfvm)) {
                     zfvm.ResetZoomCommand.Execute(null);
                     return;
                 }
             });

        #endregion
    }
}
