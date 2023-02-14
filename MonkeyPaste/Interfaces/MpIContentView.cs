using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIContentView {
        void ShowDevTools();
        bool IsSubSelectable { get; }
        object DataContext { get; }
        Task LoadContentAsync();
        Task UpdateContentAsync(MpJsonObject contentJsonObj);

        void SendMessage(string msgJsonBase64Str);
    }
}
