using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MonkeyPaste {
    // State object for reading client data asynchronously 
    public class MpSocketStateObject {
        public const int Port = 11000;
        public const string EofToken = "<EOF>";

        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket workSocket = null;
    }
}
