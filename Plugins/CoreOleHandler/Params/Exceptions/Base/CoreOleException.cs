using System;

namespace CoreOleHandler {
    public abstract class CoreOleException : Exception {
        public CoreOleException(string msg) : base(msg) { }

    }
}