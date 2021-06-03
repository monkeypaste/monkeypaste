using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace MonkeyPaste.Messages {
    public class MpLocalSimpleTextMessage : MpSimpleTextMessage {
        public MpLocalSimpleTextMessage(MpSimpleTextMessage message) {
            Id = message.Id;
            Text = message.Text;
            Timestamp = message.Timestamp;
            Username = message.Username;
            TypeInfo = message.TypeInfo;
        }
    }
}
