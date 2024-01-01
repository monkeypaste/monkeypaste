using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpAvApplicationCommandType {
        None = 0,
        ToggleAppendLineMode,
        CopySelection,
        CutSelection,
        PasteSelection,
    }

    public class MpAvApplicationCommandManager : MpIApplicationCommandManager {

        private Dictionary<MpAvApplicationCommandType, ICommand> _commands;
        public Dictionary<MpAvApplicationCommandType, ICommand> Commands {
            get {
                if (_commands == null) {
                    _commands = new Dictionary<MpAvApplicationCommandType, ICommand>() {
                        {
                            MpAvApplicationCommandType.ToggleAppendLineMode,
                            MpAvClipTrayViewModel.Instance.ToggleAppendLineModeCommand
                        }
                    };
                }
                return _commands;
            }
        }
        public ICommand PerformApplicationCommand => new MpCommand<object>(
            (args) => {
                string appUri = null;
                if (args is string argStr) {
                    appUri = argStr;
                } else if (args is MpApplicationCommand acArg) {
                    appUri = acArg.NavigateUri;
                }

                var ac = MpAvTriggerCollectionViewModel.Instance.Items
                .Where(x => x is MpIApplicationCommandCollectionViewModel)
                .Cast<MpIApplicationCommandCollectionViewModel>()
                .SelectMany(x => x.Commands)
                .FirstOrDefault(x => x.NavigateUri == appUri);

                if (ac == null) {
                    return;
                }

                ac.Command?.Execute(ac.CommandParameter);
            },
            (args) => {
                string appUri = null;
                if (args is string argStr) {
                    appUri = argStr;
                } else if (args is MpApplicationCommand ac) {
                    appUri = ac.NavigateUri;
                }

                return !string.IsNullOrEmpty(appUri);
            });
    }
}
