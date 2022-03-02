using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpNotifierType {
        None = 0,
        Startup,
        Dialog
    }

    public enum MpNotificationDialogType {
        None = 0,
        InvalidPlugin,
        InvalidAction,
        BadHttpRequest,
        DbError,
        LoadComplete
    }

    public enum MpNotificationExceptionSeverityType {
        None = 0,
        Warning, //confirm
        WarningWithOption, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
        
    public class MpNotification : MpDbModelBase {

        #region Columns

        [Column("pk_MpNotificationId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpNotificationGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpNotificationDialogType")]
        public int NotificationDialogTypeId { get; set; } = 0;

        public int DoNotShowAgainValue { get; set; } = 0;

        #endregion

        #region Properties
        [Ignore]
        public MpNotifierType NotifierType { get; set; } = MpNotifierType.None;

        [Ignore]
        public MpNotificationDialogType DialogType {
            get => (MpNotificationDialogType)NotificationDialogTypeId;
            set => NotificationDialogTypeId = (int)value;
        }

        [Ignore]
        public MpNotificationExceptionSeverityType SeverityType { get; set; } = MpNotificationExceptionSeverityType.None;

        [Ignore]
        public string Title { get; set; }
        
        [Ignore]
        public string Body { get; set; }

        [Ignore]
        public string Detail { get; set; }

        [Ignore]
        public string IconImageBase64 { get; set; }

        [Ignore]
        public double MaxShowMs { get; set; } = -1;

        public bool DoNotShowAgain {
            get => DoNotShowAgainValue == 1;
            set => DoNotShowAgainValue = value ? 1 : 0;
        }

        #endregion

        public static async Task<MpNotification> Create(
            MpNotificationDialogType dialogType = MpNotificationDialogType.None,
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetNotificationByDialogType(dialogType);
            if(dupCheck != null) {
                return dupCheck;
            }
            var n = new MpNotification() {
                DialogType = dialogType
            };

            if(!suppressWrite) {
                await n.WriteToDatabaseAsync();
            }
            return n;
        }

        public MpNotification() { }
    }
}
