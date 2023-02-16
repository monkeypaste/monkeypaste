using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIHasDataContext {
        object DataContext { get; }

    }
    public interface MpIContentView : MpIHasDataContext {
        void ShowDevTools();
        bool IsSubSelectable { get; }
        Task LoadContentAsync();
        Task UpdateContentAsync(MpJsonObject contentJsonObj);

        void SendMessage(string msgJsonBase64Str);
    }
}
