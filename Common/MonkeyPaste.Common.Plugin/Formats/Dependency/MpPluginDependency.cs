namespace MonkeyPaste.Common.Plugin {
    /// <summary>
    /// The current users operating system
    /// </summary>
    public enum MpUserDeviceType {
        None = 0,
        Ios,
        Android,
        Windows,
        Wsl, //  gray area for this one and not sure how to actually detect it yet
        Mac,
        Linux,
        Browser,
        Unknown
    }
    public class MpPluginDependency {
        /// <summary>
        /// Determines how <see cref="name"/> and <see cref="version"/> are used for validation.
        /// </summary>
        public MpPluginDependencyType type { get; set; }
        /// <summary>
        /// The string paramValue of <see cref="MpUserDeviceType"/>. There's only support currrently for 'Windows' here
        /// </summary>
        public string name { get; set; }
        public string version { get; set; }
        public override string ToString() {
            return $"Type: {type} Name: {name} Version: {version}";
        }
    }


}
