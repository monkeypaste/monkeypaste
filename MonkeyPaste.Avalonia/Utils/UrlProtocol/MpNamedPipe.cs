using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace MonkeyPaste.Avalonia {
    public enum MpNamedPipeTypes {
        SourceRef
    }
    public class MpNamedPipe<T> : IDisposable {
        #region Attribute and Properties

        private string _pipeName;
        private NamedPipeServerStream _pipeServer;
        private bool _disposed;
        private Thread _thread;
        private bool _started;

        #endregion

        #region Constructors

        public MpNamedPipe(MpNamedPipeTypes pipeType) {
            _disposed = false;
            _started = false;
            _pipeName = pipeType.ToString();
            _thread = new Thread(Main);
#if WINDOWS
            _thread.SetApartmentState(ApartmentState.STA);
#endif
            _thread.Name = "NamePipe: " + pipeType.ToString() + " Thread";
            _thread.IsBackground = true;
        }

        ~MpNamedPipe() {
            Dispose();
        }

        #endregion

        #region Events

        public delegate void Request(T t);
        public event Request OnRequest;

        #endregion

        #region Public Methods

        public static void Send(MpNamedPipeTypes pipeType, T t) {
#pragma warning disable SYSLIB0011 
            using (var npc = new NamedPipeClientStream(".", pipeType.ToString(), PipeDirection.Out)) {
                var bf = new BinaryFormatter();
                npc.Connect();
                bf.Serialize(npc, t);
            }
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        }

        public static T Recieve(MpNamedPipeTypes pipeType) {
            using (var nps = new NamedPipeServerStream(pipeType.ToString(), PipeDirection.In)) {
                return Recieve(nps);
            }
        }

        public void Start() {
            if (!_disposed && !_started) {
                _started = true;
                _thread.Start();
            }
        }

        public void Stop() {
            _started = false;

            if (_pipeServer != null) {
                _pipeServer.Close();
                // disposing will occur on thread
            }
        }

        public void Dispose() {
            _disposed = true;
            Stop();

            if (OnRequest != null)
                OnRequest = null;
        }

        #endregion

        private void Main() {
            while (_started && !_disposed) {
                try {
                    using (_pipeServer = new NamedPipeServerStream(_pipeName)) {
                        T t = Recieve(_pipeServer);

                        if (OnRequest != null && _started)
                            OnRequest(t);
                    }
                }
                catch (ThreadAbortException) { }
                catch (System.IO.IOException iox) {
                    Console.WriteLine("ERROR: {0}", iox.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                }
                catch (Exception ex) {
                    Console.WriteLine("ERROR: {0}", ex.Message);
                    return;
                }
            }
        }

        private static T Recieve(NamedPipeServerStream nps) {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            var bf = new BinaryFormatter();


            try {
                nps.WaitForConnection();

                var obj = bf.Deserialize(nps);


                if (obj is T)
                    return (T)obj;
            }
            // Catch the IOException that is raised if the pipe is
            // broken or disconnected.
            catch (IOException e) {
                Console.WriteLine("ERROR: {0}", e.Message);
            }
            return default(T);
#pragma warning restore SYSLIB0011 // Type or member is obsolete
        }
    }
}
