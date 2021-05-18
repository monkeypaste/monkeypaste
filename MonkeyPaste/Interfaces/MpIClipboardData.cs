using System.Threading.Tasks;

namespace MonkeyPaste
{
    public interface MpIClipboardData
    {
        Task<object> GetData();
        Task SetData(object data);
    }
}