
using System;

namespace MonkeyPaste.Console {
    using System;
    using System.IO;
    using System.Text;

    class Program {
        static void Main(string[] args) {
            while (true) {
                // Read a message from the extension
                int messageLength = 0;
                try {
                    messageLength = ReadInt32FromStdin();
                }
                catch (Exception) {
                    break;
                }
                string message = ReadStringFromStdin(messageLength);

                // Process the message
                Console.WriteLine("Message received from extension: " + message);

                // Send a message back to the extension
                string response = "Hello from the native application!";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                int responseLength = responseBytes.Length;
                byte[] lengthBytes = BitConverter.GetBytes(responseLength);
                Console.OpenStandardOutput().Write(lengthBytes, 0, 4);
                Console.OpenStandardOutput().Write(responseBytes, 0, responseLength);
                Console.OpenStandardOutput().Flush();
            }
        }

        static int ReadInt32FromStdin() {
            byte[] lengthBytes = new byte[4];
            Console.OpenStandardInput().Read(lengthBytes, 0, 4);
            return BitConverter.ToInt32(lengthBytes, 0);
        }

        static string ReadStringFromStdin(int length) {
            byte[] bytes = new byte[length];
            Console.OpenStandardInput().Read(bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }
    }

}