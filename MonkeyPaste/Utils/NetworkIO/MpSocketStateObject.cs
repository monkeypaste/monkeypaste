using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MonkeyPaste {
    // State object for reading client data asynchronously 
    public class MpSocketStateObject {
        public Socket WorkSocket = null;
        public const int BufferSize = 8192;
        public byte[] Buffer = new byte[BufferSize];
    }
}
