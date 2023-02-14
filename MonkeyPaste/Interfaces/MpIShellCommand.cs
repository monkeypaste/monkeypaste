namespace MonkeyPaste {
    public interface MpIShellCommand {
        object Run(string cmd, params object[] args);
    }
}
