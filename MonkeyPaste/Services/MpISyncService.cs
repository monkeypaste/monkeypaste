using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Messages;

namespace MonkeyPaste {
    public interface MpISyncService {
        bool IsConnected { get; }
        event EventHandler<MpNewMessageEventArgs> NewMessage;
        Task CreateConnection();
        Task SendMessage(MpMessage message);
        Task Dispose();
    }
}
