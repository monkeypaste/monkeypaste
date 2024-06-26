﻿using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;

namespace MonkeyPaste {
    public enum MpBillingCycleType {
        Never = 0,
        Monthly,
        Yearly
    }
    public class MpSubscriptionFormat {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics

        private static MpSubscriptionFormat _default;
        public static MpSubscriptionFormat Default {
            get {
                if (_default == null) {
                    _default = new MpSubscriptionFormat() {
#if DEBUG
                        //AccountType = MpUserAccountType.Unlimited,
                        AccountType = MpUserAccountType.Free,
#else
                        AccountType = MpUserAccountType.Free,
#endif
                        IsActive = true,
                        ExpireOffsetUtc = DateTimeOffset.MaxValue
                    };
                }
                return _default;
            }
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        [JsonConverter(typeof(StringEnumConverter))]
        public MpUserAccountType AccountType { get; set; }
        public bool IsActive { get; set; }
        public bool IsMonthly { get; set; } = false;
        public bool IsTrial { get; set; }
        public bool CanTrial { get; set; }
        public DateTimeOffset ExpireOffsetUtc { get; set; }

        #region Auto Props
        [JsonIgnore]
        public bool IsYearly =>
            !IsMonthly;
        [JsonIgnore]
        public MpBillingCycleType BillingCycleType =>
            AccountType == MpUserAccountType.Free ?
                MpBillingCycleType.Never :
                IsMonthly ?
                    MpBillingCycleType.Monthly :
                    MpBillingCycleType.Yearly;

        #endregion
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public string GetSubscriptionTypeString() {
            string suffix = string.Empty;
            if (AccountType != MpUserAccountType.Free) {
                suffix = IsMonthly ? "Monthly" : "Yearly";
            }
            return $"{AccountType}{suffix}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion


    }
}
