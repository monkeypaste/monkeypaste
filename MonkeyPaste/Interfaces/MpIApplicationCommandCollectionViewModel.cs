using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIApplicationCommandCollectionViewModel : MpIViewModel {
        IEnumerable<MpApplicationCommand> Commands { get; }
    }
}
