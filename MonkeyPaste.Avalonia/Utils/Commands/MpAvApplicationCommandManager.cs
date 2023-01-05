using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    internal class MpAvApplicationCommandManager : MpIApplicationCommandManager{
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
                .SelectMany(x=>x.Commands)
                .FirstOrDefault(x => x.NavigateUri == appUri);

                if(ac == null) {
                    return;
                }

                ac.Command?.Execute(ac.CommandParameter);
            },
            (args)=> {
                string appUri = null;
                if(args is string argStr) {
                    appUri = argStr;
                } else if(args is MpApplicationCommand ac) {
                    appUri = ac.NavigateUri;
                }

                return !string.IsNullOrEmpty(appUri);
            });
    }
}
