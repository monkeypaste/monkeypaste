using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public interface MpIWebSocketClient {
        void Configure();
        void Connect();
        void Send();

        void Connection_OnOpened();
        void Connection_OnMessage(string obj);
    }
}
