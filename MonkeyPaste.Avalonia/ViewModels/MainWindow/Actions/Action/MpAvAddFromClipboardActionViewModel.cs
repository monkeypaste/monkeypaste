using Avalonia.Input;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvAddFromClipboardActionViewModel :
        MpAvActionViewModelBase {
        #region Constants

        #endregion

        #region Properties

        #region View Models

        #endregion

        #region Appearance
        public override string ActionHintText =>
            UiStrings.ActionAddFromClipboardHint;

        #endregion

        #region State
        public override bool AllowNullArg =>
            true;

        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpAvAddFromClipboardActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
        }


        #endregion

        #region Protected Overrides

        public override async Task PerformActionAsync(object arg) {
            if (!ValidateStartAction(arg)) {
                return;
            }

            //var actionInput = GetInput(arg);

            MpPortableDataObject mpdo = null;
            MpCopyItem new_ci = null;
            object ido_obj = await Dispatcher.UIThread.InvokeAsync<object>(async () => {
                return await Mp.Services.DataObjectTools.ReadClipboardAsync(false);
            });



            if (ido_obj is MpPortableDataObject) {
                mpdo = ido_obj as MpPortableDataObject;
                new_ci = await Dispatcher.UIThread.InvokeAsync(async () => {
                    //avoid collisions if clipboard just changed work w/ cliptrays result and avoid building seperately
                    if (MpAvClipTrayViewModel.Instance.IsAddingClipboardItem) {
                        MpConsole.WriteLine("Add content from clipboard action called while clip tray adding new item...");
                        object passive_result = null;
                        EventHandler<MpCopyItem> added_handler = null;
                        added_handler = (s, e) => {
                            MpAvClipTrayViewModel.Instance.OnCopyItemAdd -= added_handler;
                            if (e == null) {
                                // couldnt create, flag to fallback
                                passive_result = "FALLBACK";
                            } else {
                                passive_result = e;
                            }
                            MpConsole.WriteLine($"Cliptray added: '{passive_result}'");
                        };
                        MpAvClipTrayViewModel.Instance.OnCopyItemAdd += added_handler;
                        var sw = Stopwatch.StartNew();
                        while (true) {
                            if (passive_result != null) {
                                if (passive_result is string &&
                                    passive_result.ToString() == "FALLBACK") {
                                    // try fallback create
                                    MpConsole.WriteLine("Cliptray couldn't add data object Add content from clipboard action tring manually...");
                                    break;
                                }
                                return passive_result as MpCopyItem;
                            }
                            if (sw.ElapsedMilliseconds > 3_000 ||
                                !MpAvClipTrayViewModel.Instance.IsAddingClipboardItem) {

                                MpConsole.WriteLine("Add content from clipboard action timed out waitinf for cliptray to add...falling back to manual");
                                // fallback and create?
                                break;
                            }
                            await Task.Delay(100);
                        }
                    } else {

                        MpConsole.WriteLine("Add content from Clipboard called, clipboard not busy adding manually");
                    }
                    //var result = await Mp.Services.CopyItemBuilder.BuildAsync(mpdo, transType: MpTransactionType.Created);
                    var result = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(mpdo, false);
                    return result;
                });
            }

            await base.PerformActionAsync(
                    new MpAvAddFromClipboardOutput() {
                        Previous = arg as MpAvActionOutput,
                        CopyItem = new_ci,
                        ClipboardDataObject = mpdo
                    });
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpFileSystemTriggerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //case nameof(FileSystemPath):
            //    if (IsBusy) {
            //        return;
            //    }
            //    if (IsEnabled.IsTrue()) {
            //        ReEnable().FireAndForgetSafeAsync(this);
            //    }
            //    break;
            //}
        }


        #endregion

        #region Commands

        #endregion
    }
}
