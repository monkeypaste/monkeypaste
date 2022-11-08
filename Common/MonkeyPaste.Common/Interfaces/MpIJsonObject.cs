using System.Text;

namespace MonkeyPaste.Common {
    public interface MpIJsonObject {
        string SerializeJsonObject();
    }

    public interface MpIJsonBase64Object {
        string SerializeJsonObjectToBase64(Encoding enc = null);
    }
}
