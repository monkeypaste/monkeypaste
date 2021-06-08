using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpCpuType {
        None=0,
        x86,
        x64,
        Arm,
        Arm64,
        Other,
        Unknown
    }
    public enum MpDeviceType {
        None=0,
        Desktop,
        Mobile,
        Tablet,
        Watch,
        Other,
        Unknown
    }
    public enum MpPlatformType {
        None = 0,
        Windows,
        Mac,
        Linux,
        Android,
        Ios,
        Web,
        Other,
        Unknown
    }
    public class MpClientPlatform : MpDbModelBase {
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; } = -1;

        public string Version { get; set; }
        public string CpuId { get; set; }

        public int CpuTypeId { get; set; }
        public int DeviceTypeId { get; set; }
        public int PlatformTypeId { get; set; }

        public MpClientPlatform() : base(typeof(MpClientPlatform)) { }
    }
}
